using UnityEngine;
using UnityEngine.Pool;

namespace LandingEffect
{
    public class LandingEffectPool
    {
        private ObjectPool<LandingEffectView> _pool;
        private LandingEffectView _prefab;
        private Transform _defaultParent;

        public void Initialize(LandingEffectView prefab, Transform defaultParent)
        {
            _prefab = prefab;
            _defaultParent = defaultParent;
            _pool = new ObjectPool<LandingEffectView>(Create, OnGet, OnRelease, OnDestroyInstance);
        }

        public LandingEffectView Get(Vector3 position, Quaternion rotation, Transform parent)
        {
            LandingEffectView effect = _pool.Get();
            effect.transform.SetParent(parent);
            effect.transform.SetPositionAndRotation(position, rotation);
            return effect;
        }

        public void Release(LandingEffectView effect)
        {
            if (effect != null)
            {
                _pool.Release(effect);
                effect.transform.SetParent(_defaultParent);
            }
        }

        public void Clear()
        {
            _pool.Clear();
        }

        private LandingEffectView Create()
        {
            LandingEffectView effect = Object.Instantiate(_prefab, _defaultParent);
            return effect;
        }

        private void OnGet(LandingEffectView effect)
        {
            effect.gameObject.SetActive(true);
        }

        private void OnRelease(LandingEffectView effect)
        {
            effect.gameObject.SetActive(false);
        }

        private void OnDestroyInstance(LandingEffectView effect)
        {
            if (effect != null)
                Object.Destroy(effect.gameObject);
        }
    }
}