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
        }

        [Exit]
        public void Exit()
        {
            //Model.EventManager.RemoveAction($"OnButtonStartClick", ButtonStart);
        }
        
        [OnDelay]
        private void Init()
        {
            Model.Set("BtnButtonStartEnable", true);
            Model.Set("BtnButtonStopEnable", false);
            Model.Set("OnSpinParticleChanged", "0");
        }
        
        private void ButtonStart()
        {
            if (!Model.Get<bool>("BtnButtonStartEnable"))
            {
                return;
            }

            Parent.Change(nameof(SpinState));
        }
    }
}