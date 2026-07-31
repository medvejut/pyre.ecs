using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Transforms.Components
{
    public struct FreezeWorldRotation : IComponentData
    {
        public quaternion WorldRotation;
    }
}