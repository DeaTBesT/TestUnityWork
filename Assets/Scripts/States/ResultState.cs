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
            // Здесь можно посчитать и отобразить результат, если нужно.
            // Для простоты сразу готовим переход к следующему запуску.
            Model.Set("BtnButtonStartEnable", false);
            Model.Set("BtnButtonStopEnable", false);

            Log.Debug($"Entering {Parent.CurrentStateName}");
        }

        // Небольшая пауза на показ результата и возврат в Idle.
        [OnDelay(1.0f)]
        private void BackToIdle()
        {
            Parent.Change(nameof(IdleState));
        }
    }
}