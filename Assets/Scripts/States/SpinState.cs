using AxGrid;
using AxGrid.Base;
using AxGrid.FSM;

namespace States
{
    [State(nameof(SpinState))]
    public class SpinState : FSMState
    {
        [Enter]
        public void Enter()
        {
            // При вращении старт недоступен, стоп появится позже.
            Model.Set("BtnButtonStartEnable", false);
            Model.Set("BtnButtonStopEnable", false);

            Model.EventManager.AddAction("OnButtonStopClick", OnButtonStop);

            // Сигнал во view: запустить анимацию рулетки.
            Model.EventManager.Invoke("OnSlotStart");

            Log.Debug($"Entering {Parent.CurrentStateName}");
        }

        [Exit]
        public void Exit()
        {
            Model.EventManager.RemoveAction("OnButtonStopClick", OnButtonStop);
        }

        // Через 3 секунды после входа в состояние разрешаем нажимать "Стоп".
        [OnDelay(3.0f)]
        private void EnableStopButton()
        {
            Model.Set("BtnButtonStopEnable", true);
        }

        private void OnButtonStop()
        {
            Log.Debug($"{Parent.CurrentStateName} ButtonStop");

            if (!Model.Get<bool>("BtnButtonStopEnable"))
                return;

            // Переход в состояние остановки.
            Parent.Change(nameof(StoppingState));
        }
    }
}