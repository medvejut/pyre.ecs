using Pyre.Animations.Settings;
using Pyre.Audio;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public class ExplosiveAuthoring : MonoBehaviour
    {
        [Header("Fuse")]
        public bool ExplodeOnStartBurn = true;
        public float Delay = 3f;

        [Header("Warning")]
        public PulseAnimationConfig WarningPulse;
        public BlinkAnimationConfig WarningBlink;
        public AudioSource TickAudioSource;

        [Header("Charge")]
        public float ExplosionRadius = 3f;
        public float ExplosionImpulse = 10f;
        public float3 ExplosionOffset;
        [Space]
        public float CustomExplosionAngularImpulseMultiplier = 5f;
        public uint CustomExplosionAngularImpulseRandomSeed = 2;
        [Space]
        public SoundClipSet ExplosionSound;
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
                DependsOn(authoring.WarningPulse);
                DependsOn(authoring.WarningBlink);

                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Explosive
                {
                    ExplodeOnStartBurn = authoring.ExplodeOnStartBurn,
                    Delay = authoring.Delay,
                });

                AddComponent(entity, new ExplosiveCharge
                {
                    Offset = authoring.ExplosionOffset,
                    Radius = authoring.ExplosionRadius,
                    Impulse = authoring.ExplosionImpulse,

                    AngularImpulseMultiplier = authoring.CustomExplosionAngularImpulseMultiplier,
                    AngularImpulseSeed = authoring.CustomExplosionAngularImpulseRandomSeed,

                    Sound = authoring.ExplosionSound,
                    Vfx = authoring.ExplosionVfx,
                });

                BakeWarning(authoring, entity);
            }

            private void BakeWarning(ExplosiveAuthoring authoring, Entity entity)
            {
                var hasWarning = authoring.WarningPulse != null
                                 || authoring.WarningBlink != null
                                 || authoring.TickAudioSource != null;

                if (!hasWarning)
                    return;

                AddComponent(entity, new ExplosiveWarning
                {
                    TickAudioSourceEntity = authoring.TickAudioSource
                        ? GetEntity(authoring.TickAudioSource, TransformUsageFlags.Dynamic)
                        : Entity.Null,

                    PlayPulse = authoring.WarningPulse != null,
                    Pulse = authoring.WarningPulse != null ? authoring.WarningPulse.ToAnimation() : default,

                    PlayBlink = authoring.WarningBlink != null,
                    Blink = authoring.WarningBlink != null ? authoring.WarningBlink.ToAnimation() : default,
                });
            }
        }
    }
}