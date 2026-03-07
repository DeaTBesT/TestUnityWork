using AxGrid;
using AxGrid.Base;
using AxGrid.FSM;

namespace States
{
    [State(nameof(IdleState))]
    public class IdleState : FSMState
    {
        [Enter]
        public void Enter()
        {
            Model?.EventManager.AddAction($"OnButtonStartClick", ButtonStart);
            Log.Debug($"Entering {Parent.CurrentStateName}");
        }

        [OnDelay(0.1f)]
        private void DiactivateButtons()
        {
            // В режиме ожидания можно нажимать только "Старт".
            Model.Set("BtnButtonStartEnable", false); // включим с небольшой задержкой
            Model.Set("BtnButtonStopEnable", false);
        }
        
        [OnDelay(0.2f)]
        private void ActivateButton()
        {
            Model.Set("BtnButtonStartEnable", true);
        }
        
        [Exit]
        public void Exit()
        {
            //Model.EventManager.RemoveAction($"OnButtonStartClick", ButtonStart);
        }
        
        private void ButtonStart()
        {
            Log.Debug($"{Parent.CurrentStateName} ButtonStart");
            if (!Model.Get<bool>("BtnButtonStartEnable"))
                return;

            // Только FSM знает, куда переходить дальше.
            Parent.Change(nameof(SpinState));
        }
    }
}