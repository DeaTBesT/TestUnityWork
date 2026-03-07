using AxGrid;
using AxGrid.Base;
using AxGrid.FSM;

namespace States
{
    [State(nameof(ResultState))]
    public class ResultState : FSMState
    {
        [Enter]
        public void Enter()
        {
            Model.Set("BtnButtonStartEnable", false);
            Model.Set("BtnButtonStopEnable", false);

            Log.Debug($"Entering {Parent.CurrentStateName}");
        }

        [OnDelay]
        private void BackToIdle() => 
            Parent.Change(nameof(IdleState));
    }
}