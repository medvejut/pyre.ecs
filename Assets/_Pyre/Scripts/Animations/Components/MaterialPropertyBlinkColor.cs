using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace Pyre.Animations.Components
{
    [MaterialProperty("_BlinkColor")]
    public struct MaterialPropertyBlinkColor : IComponentData
    {
        public float4 Value;
    }
}
