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
            Model?.EventManager.Invoke("OnSlotStart");
            Model?.Set("SpinParticle", "1");
        }

        [Exit]
        public void Exit()
        {
            //Model?.EventManager.RemoveAction("OnButtonStopClick", OnButtonStop);
        }
        
        [OnDelay]
        private void DiactivateButtons()
        {
            Model.Set("BtnButtonStartEnable", false);
            Model.Set("BtnButtonStopEnable", false);
        }
      
        [One(3.0f)]
        private void EnableStopButton() => 
            Model.Set("BtnButtonStopEnable", true);

        private void OnButtonStop()
        {
            if (!Model.Get<bool>("BtnButtonStopEnable"))
            {
                return;
            }

            Model.Set("SpinParticle", "0");
            
            Parent.Change(nameof(StoppingState));
        }
    }
}