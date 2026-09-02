using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LandingEffect
{
    public sealed class LandingEffectService
    {
        private readonly LandingEffectPool _pool;
        private readonly float _delayBefore = 0;
        private readonly int _lifeTimeMs = 1000;

        public LandingEffectService(LandingEffectView prefab, Transform parent)
        {
            _pool = new LandingEffectPool();
            _pool.Initialize(prefab, parent);
     
        }

        public async UniTask PlayAsync(PaintPiece piece)
        {
            if (_delayBefore > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_delayBefore));
            }

            if (piece == null) return;

            Transform pieceTransform = piece.transform;
            LandingEffectView effect = _pool.Get(
                pieceTransform.position + new Vector3(0f,1.298f, 0f),//todo move to So
                Quaternion.Euler(-90f, pieceTransform.rotation.eulerAngles.y, pieceTransform.rotation.eulerAngles.z),
                pieceTransform
            );

            effect.PlayParticleEffect();

            try
            {
                await UniTask.Delay(_lifeTimeMs);
            }
            finally
            {
                _pool.Release(effect);
            }
        }

        public void Clear()
        {
            _pool.Clear();
        }
    }
}