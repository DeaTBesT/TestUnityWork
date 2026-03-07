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

        [OnDelay]
        private void DiactivateButtons()
        {
            Model.Set("BtnButtonStartEnable", true);
            Model.Set("BtnButtonStopEnable", false);
            Model.Set("OnSpinParticleChanged", "0");
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