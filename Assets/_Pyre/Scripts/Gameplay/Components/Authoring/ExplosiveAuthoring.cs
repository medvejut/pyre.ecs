using Pyre.Animations.Settings;
using Pyre.Audio;
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
        public PulseAnimationConfig WarningPulse;
        public BlinkAnimationConfig WarningBlink;
        [Space]
        public float ExplosionRadius = 3f;
        public float ExplosionImpulse = 10f;
        public float3 ExplosionOffset;
        [Space]
        public float CustomExplosionAngularImpulseMultiplier = 5f;
        public uint CustomExplosionAngularImpulseRandomSeed = 2;
        [Space]
        public SoundClipSet ExplosionSound;
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
                DependsOn(authoring.WarningPulse);
                DependsOn(authoring.WarningBlink);
                DependsOn(authoring.ExplosionSound);

                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Explosive
                {
                    ExplodeOnStartBurn = authoring.ExplodeOnStartBurn,
                    Delay = authoring.Delay,

                    PlayWarningPulse = authoring.WarningPulse != null,
                    WarningPulse = authoring.WarningPulse != null ? authoring.WarningPulse.ToAnimation() : default,
                    PlayWarningBlink = authoring.WarningBlink != null,
                    WarningBlink = authoring.WarningBlink != null ? authoring.WarningBlink.ToAnimation() : default,

                    ExplosionRadius = authoring.ExplosionRadius,

                    CustomExplosionAngularImpulseMultiplier = authoring.CustomExplosionAngularImpulseMultiplier,
                    CustomExplosionAngularImpulseRandomSeed = authoring.CustomExplosionAngularImpulseRandomSeed,

                    ExplosionImpulse = authoring.ExplosionImpulse,
                    ExplosionOffset = authoring.ExplosionOffset,

                    ExplosionSound = authoring.ExplosionSound,
                    TickAudioSourceEntity = authoring.TickAudioSource ? GetEntity(authoring.TickAudioSource, TransformUsageFlags.Dynamic) : Entity.Null,

                    ExplosionVfx = authoring.ExplosionVfx
                });
            }
        }
    }
}