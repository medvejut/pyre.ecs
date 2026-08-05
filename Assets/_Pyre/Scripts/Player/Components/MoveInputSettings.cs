using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Player.Components
{
    public struct MoveInputSettings : IComponentData
    {
        public float Yaw;

        public quaternion InputToWorld => quaternion.RotateY(math.radians(Yaw));
    }
}