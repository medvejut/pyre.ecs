using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Player.Components
{
    public struct PlayerMoveInput : IComponentData
    {
        public float2 Value;
    }
}