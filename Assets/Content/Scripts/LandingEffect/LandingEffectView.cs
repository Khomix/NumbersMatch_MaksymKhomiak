using UnityEngine;

namespace LandingEffect
{

    public class LandingEffectView : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem _particleSystem;
        public bool InPool { get; set; }

        public void PlayParticleEffect()
            => _particleSystem.Play();
    }

}