using AxGrid.Base;
using AxGrid.Path;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Визуализация вертикального мини-слота (одна колонка).
    /// </summary>
    public class SlotReelView : MonoBehaviourExt
    {
        [Header("Элементы слота")] [Tooltip("RectTransform-ы элементов слота.")] [SerializeField]
        private RectTransform[] _items;

        [Tooltip("Высота одного элемента. Если 0 — возьмётся из первого элемента.")] [SerializeField]
        private float _itemHeight = 150f;

        [Header("Скорости и тайминги")] [Tooltip("Начальная скорость при старте, px/сек.")] [SerializeField]
        private float _minSpeed = 300f;

        [Tooltip("Максимальная скорость при раскрутке, px/сек.")] [SerializeField]
        private float _maxSpeed = 1500f;

        [Tooltip("Множитель скорости для этого барабана.")] [SerializeField]
        private float _speedMultiplier = 1f;

        [Tooltip("Ускорение при раскрутке, px/сек^2.")] [SerializeField]
        private float _acceleration = 2500f;

        [Tooltip("Замедление при остановке, px/сек^2.")] [SerializeField]
        private float _deceleration = 3000f;

        [Tooltip("Минимальное время до разрешения на остановку, сек.")] [SerializeField]
        private float _minSpinTime = 3f;

        [Tooltip("Время «доворота» до идеального выравнивания, сек.")] [SerializeField]
        private float _snapDuration = 0.25f;

        [Header("Спрайты для слота")]
        [Tooltip("Набор возможных картинок, из которых будут собираться элементы слота.")]
        [SerializeField]
        private Sprite[] _symbolSprites;

        private Image[] _itemImages;

        private bool _isSpinning;
        private bool _isStopping;
        private bool _stopRequested;
        private float _timeSinceStart;
        private float _currentSpeed;

        public bool IsSpinning => _isSpinning || _isStopping;

        public bool CanRequestStop => _timeSinceStart >= _minSpinTime;

        [OnAwake]
        private void AwakeThis()
        {
            if ((_items == null) || (_items.Length == 0))
            {
                return;
            }

            _itemImages = new Image[_items.Length];
            for (var i = 0; i < _items.Length; i++)
            {
                _itemImages[i] = _items[i].GetComponent<Image>();
            }

            if (_itemHeight <= 0f)
            {
                _itemHeight = _items[0].rect.height;
            }

            RandomizeAllSymbols();
        }

        [OnUpdate]
        private void UpdateThis()
        {
            if (_isSpinning)
            {
                UpdateSpinning(Time.deltaTime);
            }
        }

        public void StartSpin()
        {
            if (IsSpinning)
            {
                return;
            }

            _isSpinning = true;
            _isStopping = false;
            _stopRequested = false;
            _timeSinceStart = 0f;

            var effMinSpeed = _minSpeed * _speedMultiplier;
            var effMaxSpeed = _maxSpeed * _speedMultiplier;

            _currentSpeed = effMinSpeed;

            Path = new CPath();

            var accelTime = 0f;
            if (_acceleration > 0f && effMaxSpeed > effMinSpeed)
            {
                accelTime = (effMaxSpeed - effMinSpeed) / _acceleration;
            }

            if (accelTime > 0.01f)
            {
                Path.EasingLinear(accelTime,
                    effMinSpeed,
                    effMaxSpeed,
                    value => { _currentSpeed = value; });
            }
            else
            {
                _currentSpeed = effMaxSpeed;
            }
        }

        public void RequestStop()
        {
            if (!_isSpinning)
            {
                return;
            }

            if (!CanRequestStop)
            {
                return;
            }

            _stopRequested = true;

            var startSpeed = _currentSpeed;
            if (startSpeed < 0.01f)
            {
                StartSnapWithPath();
                return;
            }

            Path.EasingLinear(
                    Mathf.Max(0.05f, startSpeed / Mathf.Max(1f, _deceleration)),
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

            var distance = _currentSpeed * dt;
            ScrollItems(distance);
        }

        private void ScrollItems(float distance)
        {
            if ((_items == null) || (_items.Length == 0))
            {
                return;
            }

            foreach (var item in _items)
            {
                var pos = item.anchoredPosition;
                pos.y -= distance;
                item.anchoredPosition = pos;
            }

            var loopHeight = _itemHeight * _items.Length;
            var halfLoop = loopHeight * 0.5f;

            for (var i = 0; i < _items.Length; i++)
            {
                var pos = _items[i].anchoredPosition;
                var wrapped = false;

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

                if (!wrapped)
                {
                    continue;
                }
                
                _items[i].anchoredPosition = pos;
                RandomizeSymbol(i);
            }
        }


        private void StartSnapWithPath()
        {
            if ((_items == null) || (_items.Length == 0))
            {
                return;
            }

            _isStopping = true;

            var closest = _items[0];
            var minDist = Mathf.Abs(_items[0].anchoredPosition.y);

            for (var i = 1; i < _items.Length; i++)
            {
                var d = Mathf.Abs(_items[i].anchoredPosition.y);

                if (!(d < minDist))
                {
                    continue;
                }

                minDist = d;
                closest = _items[i];
            }

            var targetDistance = closest.anchoredPosition.y;
            var applied = 0f;

            Path.EasingCircEaseIn(
                    _snapDuration,
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
            if ((_symbolSprites == null) || (_symbolSprites.Length == 0) || (_itemImages == null))
            {
                return;
            }

            for (var i = 0; i < _itemImages.Length; i++)
            {
                RandomizeSymbol(i);
            }
        }

        private void RandomizeSymbol(int index)
        {
            if ((_symbolSprites == null) || (_symbolSprites.Length == 0))
            {
                return;
            }

            if ((_itemImages == null) || (index < 0) || (index >= _itemImages.Length))
            {
                return;
            }

            var spriteIndex = Random.Range(0, _symbolSprites.Length);
            _itemImages[index].sprite = _symbolSprites[spriteIndex];
        }
    }
}