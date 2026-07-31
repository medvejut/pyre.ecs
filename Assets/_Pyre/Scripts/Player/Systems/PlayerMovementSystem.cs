using Pyre.Gameplay.Components;
using Pyre.Player.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Pyre.Player.Systems
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

            foreach (var (input, movement, velocity, localTransform, entity) in SystemAPI
                         .Query<RefRO<PlayerMoveInput>, RefRO<PlayerMovement>, RefRW<PhysicsVelocity>, RefRW<LocalTransform>>()
                         .WithAll<PlayerTag>()
                         .WithEntityAccess())
            {
                var moveInput = input.ValueRO.Value;
                var worldDirection = math.rotate(movement.ValueRO.IsometricRotation, new float3(moveInput.x, 0f, moveInput.y));

                var movementVelocity = float3.zero;

                if (math.lengthsq(moveInput) > 0.001f)
                {
                    movementVelocity.xz = worldDirection.xz * movement.ValueRO.MoveSpeed;
                }

                var knockbackVelocity = float3.zero;
                var knockbackAngular = float3.zero;

                if (SystemAPI.TryGetComponent<KnockbackVelocity>(entity, out var knockback))
                {
                    knockbackVelocity.xz = knockback.Linear.xz;
                    knockbackAngular = knockback.Angular;
                }

                velocity.ValueRW.Linear = movementVelocity + knockbackVelocity;
                velocity.ValueRW.Angular = float3.zero;

                var baseRotation = localTransform.ValueRO.Rotation;
                if (math.lengthsq(worldDirection) > 0.001f)
                {
                    var targetRotation = quaternion.LookRotationSafe(worldDirection, math.up());
                    baseRotation = math.slerp(localTransform.ValueRO.Rotation, targetRotation, movement.ValueRO.RotationSpeed * deltaTime);
                }

                if (math.lengthsq(knockbackAngular) > 0.0001f)
                {
                    baseRotation = math.mul(quaternion.AxisAngle(math.up(), knockbackAngular.y * deltaTime), baseRotation);
                }

                baseRotation.value.xz = 0;
                localTransform.ValueRW.Rotation = baseRotation;
            }
        }
    }
}