using AxGrid;
using AxGrid.Base;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Обёртка над несколькими столбцами-слотами.
    /// Принимает сигналы только от FSM через EventManager:
    /// - "OnSlotStart"  — запустить вращение;
    /// - "OnSlotStop"   — начать остановку;
    /// - "OnSlotStopped" отправляется обратно в FSM, когда все барабаны остановились.
    /// </summary>
    public class SlotMachineView : MonoBehaviourExt
    {
        [SerializeField] private SlotReelView[] reels;

        private bool _wasSpinning;

        /// <summary>
        /// Хоть один барабан сейчас крутится/останавливается.
        /// </summary>
        public bool IsSpinning
        {
            get
            {
                if (reels == null) return false;

                for (int i = 0; i < reels.Length; i++)
                {
                    if (reels[i] != null && reels[i].IsSpinning)
                        return true;
                }

                return false;
            }
        }

        [OnStart]
        private void StartThis()
        {
            // Сигналы ИЗ FSM.
            Model.EventManager.AddAction("OnSlotStart", OnSlotStart);
            Model.EventManager.AddAction("OnSlotStop", OnSlotStop);
        }

        [OnDestroy]
        private void DestroyThis()
        {
            Model.EventManager.RemoveAction("OnSlotStart", OnSlotStart);
            Model.EventManager.RemoveAction("OnSlotStop", OnSlotStop);
        }

        [OnUpdate]
        private void UpdateThis()
        {
            bool spinningNow = IsSpinning;

            // Переход из "крутилось" в "не крутится" → сообщаем FSM, что всё остановилось.
            if (_wasSpinning && !spinningNow)
            {
                Model.EventManager.Invoke("OnSlotStopped");
            }

            _wasSpinning = spinningNow;
        }

        private void OnSlotStart()
        {
            StartSpin();
        }

        private void OnSlotStop()
        {
            RequestStop();
        }

        /// <summary>
        /// Запустить вращение всех столбцов.
        /// Вызывается через событие "OnSlotStart" из FSM или из контекстного меню.
        /// </summary>
        [ContextMenu("Start spin")]
        public void StartSpin()
        {
            if (reels == null)
                return;

            for (int i = 0; i < reels.Length; i++)
            {
                if (reels[i] != null)
                    reels[i].StartSpin();
            }
        }

        /// <summary>
        /// Запросить остановку всех столбцов.
        /// Вызывается через событие "OnSlotStop" из FSM или из контекстного меню.
        /// </summary>
        [ContextMenu("Stop spin")]
        public void RequestStop()
        {
            if (reels == null)
                return;

            for (int i = 0; i < reels.Length; i++)
            {
                if (reels[i] != null)
                    reels[i].RequestStop();
            }
        }
    }
}