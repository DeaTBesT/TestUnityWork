using AxGrid.FSM;

namespace States
{
    [State(nameof(StoppingState))]
    public class StoppingState : FSMState
    {
        [Enter]
        public void Enter()
        {
            Model.Set("BtnButtonStartEnable", false);
            Model.Set("BtnButtonStopEnable", false);

            Model?.EventManager.AddAction("OnSlotStopped", OnSlotStopped);

            Model?.EventManager.Invoke("OnSlotStop");
        }

        [Exit]
        public void Exit()
        {
            //Model?.EventManager.RemoveAction("OnSlotStopped", OnSlotStopped);
        }

        private void OnSlotStopped() => 
            Parent.Change(nameof(ResultState));
    }
}