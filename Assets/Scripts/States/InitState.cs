using AxGrid.FSM;

namespace States
{
    [State(nameof(InitState))]
    public class InitState : FSMState
    {
        [Enter]
        public void Enter() => 
            Parent.Change(nameof(IdleState));
    }
}