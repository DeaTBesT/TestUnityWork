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
            Model?.EventManager.AddAction("OnButtonStopClick", OnButtonStop);

            // Сигнал во view: запустить анимацию рулетки.
            Model?.EventManager.Invoke("OnSlotStart");

            Model.Set("SpinParticle", "1");
            
            Log.Debug($"Entering {Parent.CurrentStateName}");
        }

        [OnDelay(0.1f)]
        private void DiactivateButtons()
        {
            // В режиме ожидания можно нажимать только "Старт".
            Model.Set("BtnButtonStartEnable", false); // включим с небольшой задержкой
            Model.Set("BtnButtonStopEnable", false);
        }
        
        [Exit]
        public void Exit()
        {
            //Model?.EventManager.RemoveAction("OnButtonStopClick", OnButtonStop);
        }

        // Через 3 секунды после входа в состояние разрешаем нажимать "Стоп".
        [One(3.0f)]
        private void EnableStopButton()
        {
            Log.Debug("Enable stopbutton");
            Model.Set("BtnButtonStopEnable", true);
        }

        private void OnButtonStop()
        {
            Log.Debug($"{Parent.CurrentStateName} ButtonStop");

            if (!Model.Get<bool>("BtnButtonStopEnable"))
                return;

            Model.Set("SpinParticle", "0");
            
            // Переход в состояние остановки.
            Parent.Change(nameof(StoppingState));
        }
    }
}