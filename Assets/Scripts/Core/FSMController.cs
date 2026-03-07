using AxGrid;
using AxGrid.Base;
using AxGrid.FSM;
using States;
using UnityEngine;

namespace Core
{
    public class FSMController : MonoBehaviourExt
    {
        [OnAwake]
        private void AwakeThis()
        {
            Settings.Fsm = new FSM();
            Settings.Fsm.Add(new InitState());
            Settings.Fsm.Add(new IdleState());
            Settings.Fsm.Add(new SpinState());
            Settings.Fsm.Add(new StoppingState());
            Settings.Fsm.Add(new ResultState());
        }
        
        [OnStart]
        private void StartThis() => 
            Settings.Fsm.Start(nameof(InitState));

        [OnUpdate]
        private void UpdateThis() => 
            Settings.Fsm.Update(Time.deltaTime);
    }
}