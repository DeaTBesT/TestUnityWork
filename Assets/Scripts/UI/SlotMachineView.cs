using AxGrid;
using AxGrid.Base;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Обёртка над несколькими столбцами-слотами.
    /// Позволяет одним вызовом запускать/останавливать сразу несколько SlotReelView.
    /// Верстка (количество и расположение столбцов) задаётся в инспекторе.
    /// </summary>
    public class SlotMachineView : MonoBehaviourExt
    {
        [SerializeField] private SlotReelView[] reels;

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

        /// <summary>
        /// Можно ли нажать "Стоп" — когда все барабаны уже отспинили minSpinTime.
        /// </summary>
        public bool CanRequestStop
        {
            get
            {
                if (reels == null || reels.Length == 0) return false;

                for (int i = 0; i < reels.Length; i++)
                {
                    if (reels[i] == null)
                        continue;

                    if (!reels[i].CanRequestStop)
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Запустить вращение всех столбцов.
        /// Вызывайте из FSM / кнопки "Старт".
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
        /// Вызывайте из FSM / кнопки "Стоп".
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

