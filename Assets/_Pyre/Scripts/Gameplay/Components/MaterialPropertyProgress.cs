using Unity.Entities;
using Unity.Rendering;

namespace Pyre.Gameplay.Components
{
    [MaterialProperty("_Progress")]
    public struct MaterialPropertyProgress : IComponentData
    {
        public float Value;
    }
}