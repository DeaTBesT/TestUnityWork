using AxGrid;
using AxGrid.Base;
using AxGrid.Path;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Визуализация вертикального мини-слота (одна колонка).
    /// Логика:
    /// - бесконечный скролл элементов вниз;
    /// - плавный разгон/торможение;
    /// - фиксация так, чтобы центральный элемент оказался по центру маски.
    /// Верстку (маски, расположение, кнопки) вы делаете сами.
    /// </summary>
    public class SlotReelView : MonoBehaviourExt
    {
        [Header("Элементы слота")]
        [Tooltip("RectTransform-ы элементов слота (3 видимых + 1-2 запасных сверху/снизу). Порядок не критичен.")]
        [SerializeField] private RectTransform[] items;

        [Tooltip("Высота одного элемента. Если 0 — возьмётся из первого элемента.")]
        [SerializeField] private float itemHeight = 150f;

        [Header("Скорости и тайминги")]
        [Tooltip("Начальная скорость при старте, px/сек.")]
        [SerializeField] private float minSpeed = 300f;

        [Tooltip("Максимальная скорость при раскрутке, px/сек.")]
        [SerializeField] private float maxSpeed = 1500f;
        
        [Tooltip("Множитель скорости для этого барабана (позволяет сделать столбцы с разной скоростью).")]
        [SerializeField] private float speedMultiplier = 1f;

        [Tooltip("Ускорение при раскрутке, px/сек^2.")]
        [SerializeField] private float acceleration = 2500f;

        [Tooltip("Замедление при остановке, px/сек^2.")]
        [SerializeField] private float deceleration = 3000f;

        [Tooltip("Минимальное время до разрешения на остановку, сек.")]
        [SerializeField] private float minSpinTime = 3f;

        [Tooltip("Время «доворота» до идеального выравнивания, сек.")]
        [SerializeField] private float snapDuration = 0.25f;

        [Header("Спрайты для слота")]
        [Tooltip("Набор возможных картинок, из которых будут собираться элементы слота.")]
        [SerializeField] private Sprite[] symbolSprites;

        private Image[] _itemImages;

        // Текущее состояние вращения
        private bool _isSpinning;
        private bool _isStopping;
        private bool _stopRequested;
        private float _timeSinceStart;
        private float _currentSpeed;

        /// <summary>
        /// Крутится ли сейчас слот (в том числе в фазе остановки/доворота).
        /// </summary>
        public bool IsSpinning => _isSpinning || _isStopping;

        /// <summary>
        /// Можно ли сейчас нажимать кнопку "Стоп".
        /// Условие из задания — через 3 секунды после старта (minSpinTime).
        /// </summary>
        public bool CanRequestStop => _timeSinceStart >= minSpinTime;

        [OnAwake]
        private void AwakeThis()
        {
            if (items == null || items.Length == 0)
                return;

            _itemImages = new Image[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                _itemImages[i] = items[i].GetComponent<Image>();
            }

            if (itemHeight <= 0f)
                itemHeight = items[0].rect.height;

            RandomizeAllSymbols();
        }

        [OnUpdate]
        private void UpdateThis()
        {
            if (_isSpinning)
                UpdateSpinning(Time.deltaTime);
        }

        /// <summary>
        /// Старт вращения слота. Вызывайте из FSM/кнопки "Старт".
        /// </summary>
        public void StartSpin()
        {
            if (IsSpinning)
                return;

            _isSpinning = true;
            _isStopping = false;
            _stopRequested = false;
            _timeSinceStart = 0f;

            // Эффективные скорости для этого барабана с учётом множителя.
            float effMinSpeed = minSpeed * speedMultiplier;
            float effMaxSpeed = maxSpeed * speedMultiplier;

            _currentSpeed = effMinSpeed;

            // Сбрасываем и настраиваем Path:
            // 1) плавный разгон скорости от minSpeed до maxSpeed
            // 2) дальнейшее вращение идёт с постоянной скоростью _currentSpeed (через UpdateThis)
            Path = new CPath();

            float accelTime = 0f;
            if (acceleration > 0f && effMaxSpeed > effMinSpeed)
                accelTime = (effMaxSpeed - effMinSpeed) / acceleration;

            if (accelTime > 0.01f)
            {
                Path.EasingLinear(accelTime, effMinSpeed, effMaxSpeed, value =>
                {
                    _currentSpeed = value;
                });
            }
            else
            {
                _currentSpeed = effMaxSpeed;
            }
        }

        /// <summary>
        /// Запрос на остановку. Вызывайте из FSM/кнопки "Стоп".
        /// Фактически остановка произойдёт плавно, плюс будет доворот к центру.
        /// </summary>
        public void RequestStop()
        {
            if (!_isSpinning)
                return;

            // Жёстко соблюдаем ограничение 3 секунды по ТЗ.
            if (!CanRequestStop)
                return;

            _stopRequested = true;

            // Плавное торможение скорости через Path,
            // затем — анимация доворота к центру тоже через Path.
            float startSpeed = _currentSpeed;
            if (startSpeed < 0.01f)
            {
                StartSnapWithPath();
                return;
            }

            Path.EasingLinear(
                    Mathf.Max(0.05f, startSpeed / Mathf.Max(1f, deceleration)),
                    startSpeed,
                    0f,
                    value => { _currentSpeed = value; }
                )
                .Action(() =>
                {
                    _isSpinning = false;
                    StartSnapWithPath();
                });
        }

        private void UpdateSpinning(float dt)
        {
            _timeSinceStart += dt;

            float distance = _currentSpeed * dt;
            ScrollItems(distance);
        }

        /// <summary>
        /// Прокручиваем все элементы вниз на distance пикселей
        /// и реализуем «бесконечный» скролл.
        /// </summary>
        private void ScrollItems(float distance)
        {
            if (items == null || items.Length == 0)
                return;

            // 1. Двигаем все элементы вниз (уменьшаем Y).
            for (int i = 0; i < items.Length; i++)
            {
                Vector2 pos = items[i].anchoredPosition;
                pos.y -= distance;
                items[i].anchoredPosition = pos;
            }

            // 2. «Бесконечный» скролл за счёт зацикливания по высоте всего стека.
            // Предполагаем, что центр маски находится в Y = 0.
            float loopHeight = itemHeight * items.Length;
            float halfLoop = loopHeight * 0.5f;

            for (int i = 0; i < items.Length; i++)
            {
                Vector2 pos = items[i].anchoredPosition;
                bool wrapped = false;

                if (pos.y < -halfLoop)
                {
                    pos.y += loopHeight;
                    wrapped = true;
                }
                else if (pos.y > halfLoop)
                {
                    pos.y -= loopHeight;
                    wrapped = true;
                }

                if (wrapped)
                {
                    items[i].anchoredPosition = pos;
                    RandomizeSymbol(i);
                }
            }
        }

        /// <summary>
        /// Анимация доворота через Path так, чтобы ближайший к центру элемент
        /// оказался ровно по центру (Y ~= 0 в локальных координатах).
        /// </summary>
        private void StartSnapWithPath()
        {
            if (items == null || items.Length == 0)
                return;

            _isStopping = true;

            RectTransform closest = items[0];
            float minDist = Mathf.Abs(items[0].anchoredPosition.y);

            for (int i = 1; i < items.Length; i++)
            {
                float d = Mathf.Abs(items[i].anchoredPosition.y);
                if (d < minDist)
                {
                    minDist = d;
                    closest = items[i];
                }
            }

            // Нужно довести closest до y = 0.
            // ScrollItems(distance) уменьшает y на distance,
            // поэтому общее distance = currentY, чтобы получить 0.
            float targetDistance = closest.anchoredPosition.y;
            float applied = 0f;

            Path.EasingCircEaseIn(
                    snapDuration,
                    0f,
                    targetDistance,
                    value =>
                    {
                        float delta = value - applied;
                        applied = value;
                        ScrollItems(delta);
                    })
                .Action(() => { _isStopping = false; });
        }

        private void RandomizeAllSymbols()
        {
            if (symbolSprites == null || symbolSprites.Length == 0 || _itemImages == null)
                return;

            for (int i = 0; i < _itemImages.Length; i++)
                RandomizeSymbol(i);
        }

        private void RandomizeSymbol(int index)
        {
            if (symbolSprites == null || symbolSprites.Length == 0)
                return;

            if (_itemImages == null || index < 0 || index >= _itemImages.Length)
                return;

            int spriteIndex = Random.Range(0, symbolSprites.Length);
            _itemImages[index].sprite = symbolSprites[spriteIndex];
        }
    }
}

