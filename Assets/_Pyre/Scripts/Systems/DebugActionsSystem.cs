using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using UnityEngine.InputSystem;

namespace Pyre.Systems
{
    public partial struct DebugActionsSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            if (Keyboard.current?.spaceKey.wasPressedThisFrame == true)
            {
                foreach (var (physicsVelocity, physicsMass) in SystemAPI
                             .Query<RefRW<PhysicsVelocity>, RefRO<PhysicsMass>>())
                {
                    physicsVelocity.ValueRW.ApplyLinearImpulse(physicsMass.ValueRO, new float3(10, 20, -10));
                    var random = Random.CreateFromIndex(2);
                    physicsVelocity.ValueRW.ApplyAngularImpulse(physicsMass.ValueRO, random.NextFloat3(-5f, 5f));
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}