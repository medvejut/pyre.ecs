using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Animations.Components
{
    public struct AnimationRestColor : IComponentData
    {
        public float4 Value;
    }
}