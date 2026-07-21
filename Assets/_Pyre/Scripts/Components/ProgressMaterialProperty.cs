using Unity.Entities;
using Unity.Rendering;

namespace Pyre.Components
{
    [MaterialProperty("_Progress")]
    public struct ProgressMaterialProperty : IComponentData
    {
        public float Value;
    }
}