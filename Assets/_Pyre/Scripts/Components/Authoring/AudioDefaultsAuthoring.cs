using Unity.Entities;
using UnityEngine;

namespace Pyre.Components
{
    public class AudioDefaultsAuthoring : MonoBehaviour
    {
        [SerializeField] private AudioClip extinguishClip;

        public class AudioDefaultsBaker : Baker<AudioDefaultsAuthoring>
        {
            public override void Bake(AudioDefaultsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new AudioDefaults { ExtinguishClip = authoring.extinguishClip });
            }
        }
    }
}
