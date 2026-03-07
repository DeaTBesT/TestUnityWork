using AxGrid;
using AxGrid.Base;
using AxGrid.FSM;

namespace States
{
    [State(nameof(StoppingState))]
    public class StoppingState : FSMState
    {
        [Enter]
        public void Enter()
        {
            // Во время остановки кнопки заблокированы.
            Model.Set("BtnButtonStartEnable", false);
            Model.Set("BtnButtonStopEnable", false);

            // Слушаем событие окончания анимации рулетки.
            Model.EventManager.AddAction("OnSlotStopped", OnSlotStopped);

            // Сигнал во view: начать плавную остановку барабанов.
            Model.EventManager.Invoke("OnSlotStop");

            Log.Debug($"Entering {Parent.CurrentStateName}");
        }

        [Exit]
        public void Exit()
        {
            Model.EventManager.RemoveAction("OnSlotStopped", OnSlotStopped);
        }

        private void OnSlotStopped()
        {
            // Когда визуальная часть сообщила, что всё остановилось — переходим к результату.
            Parent.Change(nameof(ResultState));
        }
    }
}