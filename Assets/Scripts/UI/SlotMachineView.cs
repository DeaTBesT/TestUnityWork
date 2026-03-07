using System.Linq;
using AxGrid.Base;
using UnityEngine;

namespace UI
{
    public class SlotMachineView : MonoBehaviourExt
    {
        [SerializeField] private SlotReelView[] _reels;

        private bool _wasSpinning;

        public bool IsSpinning => _reels != null && _reels.Any(t => t != null && t.IsSpinning);

        [OnStart]
        private void StartThis()
        {
            Model.EventManager.AddAction("OnSlotStart", OnSlotStart);
            Model.EventManager.AddAction("OnSlotStop", OnSlotStop);
        }

        [OnDestroy]
        private void DestroyThis()
        {
            Model.EventManager.RemoveAction("OnSlotStart", OnSlotStart);
            Model.EventManager.RemoveAction("OnSlotStop", OnSlotStop);
        }

        [OnUpdate]
        private void UpdateThis()
        {
            var spinningNow = IsSpinning;

            if ((_wasSpinning) && (!spinningNow))
            {
                Model.EventManager.Invoke("OnSlotStopped");
            }

            _wasSpinning = spinningNow;
        }

        private void OnSlotStart() => 
            StartSpin();

        private void OnSlotStop() => 
            RequestStop();

        [ContextMenu("Start spin")]
        public void StartSpin()
        {
            if (_reels == null)
                return;

            foreach (var reel in _reels)
            {
                if (reel != null)
                    reel.StartSpin();
            }
        }

        [ContextMenu("Stop spin")]
        public void RequestStop()
        {
            if (_reels == null)
                return;

            foreach (var reel in _reels)
            {
                if (reel != null)
                    reel.RequestStop();
            }
        }
    }
}