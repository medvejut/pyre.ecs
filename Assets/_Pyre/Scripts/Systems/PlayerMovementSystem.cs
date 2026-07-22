using Pyre.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Pyre.Systems
{
    public partial struct PlayerMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (input, movement, velocity, localTransform) in SystemAPI
                         .Query<RefRO<PlayerMoveInput>, RefRO<PlayerMovement>, RefRW<PhysicsVelocity>, RefRW<LocalTransform>>()
                         .WithAll<PlayerTag>())
            {
                var moveInput = input.ValueRO.Value;
                var worldDirection = math.rotate(movement.ValueRO.IsometricRotation, new float3(moveInput.x, 0f, moveInput.y));

                velocity.ValueRW.Linear.xz = worldDirection.xz * movement.ValueRO.MoveSpeed;

                velocity.ValueRW.Angular = float3.zero;
                if (math.lengthsq(worldDirection) > 0.001f)
                {
                    var targetRotation = quaternion.LookRotationSafe(worldDirection, math.up());
                    localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, targetRotation, movement.ValueRO.RotationSpeed * deltaTime);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}