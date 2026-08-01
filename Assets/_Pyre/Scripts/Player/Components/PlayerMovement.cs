using Unity.Entities;

namespace Pyre.Player.Components
{
    public struct PlayerMovement : IComponentData
    {
        public float MoveSpeed;
        public float RotationSpeed;
    }
}
