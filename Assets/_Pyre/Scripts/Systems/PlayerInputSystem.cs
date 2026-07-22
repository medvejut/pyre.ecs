using Pyre.Components;
using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Systems
{
    [UpdateBefore(typeof(PlayerMovementSystem))]
    public partial class PlayerInputSystem : SystemBase
    {
        private PlayerInputActions _inputActions;

        protected override void OnCreate()
        {
            RequireForUpdate<PlayerTag>();

            _inputActions = new PlayerInputActions();
            _inputActions.Enable();
        }

        protected override void OnDestroy()
        {
            _inputActions.Disable();
            _inputActions.Dispose();
        }

        protected override void OnUpdate()
        {
            var raw = _inputActions.Gameplay.Move.ReadValue<UnityEngine.Vector2>();
            var moveInput = new float2(raw.x, raw.y);

            foreach (var input in SystemAPI.Query<RefRW<PlayerMoveInput>>().WithAll<PlayerTag>())
            {
                input.ValueRW.Value = moveInput;
            }
        }
    }
}