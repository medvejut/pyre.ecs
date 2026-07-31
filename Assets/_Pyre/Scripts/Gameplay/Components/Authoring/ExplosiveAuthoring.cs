using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public class ExplosiveAuthoring : MonoBehaviour
    {
        public bool ExplodeOnStartBurn = true;
        public float Delay = 3f;
        [Space]
        public float ExplosionRadius = 3f;
        public float ExplosionImpulse = 10f;
        public float3 ExplosionOffset;
        [Space]
        public float CustomExplosionAngularImpulseMultiplier = 5f;
        public uint CustomExplosionAngularImpulseRandomSeed = 2;
        [Space]
        public AudioClip ExplosionClip;
        public AudioSource TickAudioSource;
        [Space]
        public ParticleSystem ExplosionVfx;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + (Vector3)ExplosionOffset, 0.1f);
            Gizmos.color = new Color(1f, 0f, 0f, 0.05f);
            Gizmos.DrawSphere(transform.position + (Vector3)ExplosionOffset, ExplosionRadius);
        }

        public class ExplosiveBaker : Baker<ExplosiveAuthoring>
        {
            public override void Bake(ExplosiveAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Explosive
                {
                    ExplodeOnStartBurn = authoring.ExplodeOnStartBurn,
                    Delay = authoring.Delay,
                    ExplosionRadius = authoring.ExplosionRadius,

                    CustomExplosionAngularImpulseMultiplier = authoring.CustomExplosionAngularImpulseMultiplier,
                    CustomExplosionAngularImpulseRandomSeed = authoring.CustomExplosionAngularImpulseRandomSeed,

                    ExplosionImpulse = authoring.ExplosionImpulse,
                    ExplosionOffset = authoring.ExplosionOffset,

                    ExplosionClip = authoring.ExplosionClip,
                    TickAudioSourceEntity = authoring.TickAudioSource ? GetEntity(authoring.TickAudioSource, TransformUsageFlags.Dynamic) : Entity.Null,

                    ExplosionVfx = authoring.ExplosionVfx
                });
            }
        }
    }
}