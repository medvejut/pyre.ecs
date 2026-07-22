using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Components
{
    public struct FreezeWorldRotation : IComponentData
    {
        public quaternion WorldRotation;
    }
}