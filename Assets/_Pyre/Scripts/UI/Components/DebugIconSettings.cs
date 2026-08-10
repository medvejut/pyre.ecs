using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.UI.Components
{
    public struct DebugIconSettings : IComponentData
    {
        public bool Enabled;
        public float3 Offset;
    }
}