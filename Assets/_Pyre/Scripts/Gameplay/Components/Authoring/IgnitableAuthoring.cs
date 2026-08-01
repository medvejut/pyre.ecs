using Pyre.Audio.Components;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public class IgnitableAuthoring : MonoBehaviour
    {
        [SerializeField] private float burningRadius = 1f;
        [SerializeField] private float ignitionTime = 1f;
        [SerializeField] private float coolingRate = 0.5f;
        [Space]
        [SerializeField] private AudioClip igniteClip;
        [SerializeField] private AudioClip loopClip;
        [SerializeField] private AudioClip extinguishClip;

        public class IgnitableBaker : Baker<IgnitableAuthoring>
        {
            public override void Bake(IgnitableAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Ignitable
                {
                    BurningRadius = authoring.burningRadius,
                    IgnitionTime = authoring.ignitionTime,
                    CoolingRate = authoring.coolingRate
                });
                AddComponent(entity, new IgnitionProgress { Elapsed = 0f });

                if (authoring.igniteClip)
                {
                    AppendToBuffer(entity, new SoundClipOverride { Kind = SoundKind.Burn, Clip = authoring.igniteClip });
                }

                if (authoring.loopClip)
                {
                    AppendToBuffer(entity, new SoundClipOverride { Kind = SoundKind.BurningLoop, Clip = authoring.loopClip });
                }

                if (authoring.extinguishClip)
                {
                    AppendToBuffer(entity, new SoundClipOverride { Kind = SoundKind.Extinguish, Clip = authoring.extinguishClip });
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            var bodyCollider = GetComponentInChildren<Collider>();
            var center = bodyCollider ? bodyCollider.bounds.center : transform.position;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, burningRadius);
        }
    }
}