using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Components
{
    public struct PlayerMovement : IComponentData
    {
        public float MoveSpeed;
        public float RotationSpeed;
        public quaternion IsometricRotation;
    }
}
