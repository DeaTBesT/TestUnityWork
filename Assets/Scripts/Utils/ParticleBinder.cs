using AxGrid.Base;
using AxGrid.Model;
using Coffee.UIExtensions;
using UnityEngine;

namespace Utils
{
    [RequireComponent(typeof(UIParticle))]
    public class ParticleBinder : MonoBehaviourExtBind
    {
        [SerializeField] private string _particleName = "";

        private UIParticle _particleSystem;
        
        [OnAwake]
        private void AwakeThis() => 
            _particleSystem = GetComponent<UIParticle>();

        [Bind("On{_particleName}Changed")]
        public void OnParticleSetActive(string value)
        {
            if (value == "1")
            {
                _particleSystem.enabled = true;
                _particleSystem.Play();
            }
            else
            {
                _particleSystem.Stop();
                _particleSystem.enabled = false;
            }
        }
    }
}