using Pyre.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Systems
{
    public partial struct KnockbackVelocityDumpingSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            const float linearDumping = 3f;
            const float angularDumping = 5f;

            foreach (var knockbackVelocity in SystemAPI
                         .Query<RefRW<KnockbackVelocity>>())
            {
                knockbackVelocity.ValueRW.Linear *= math.exp(-linearDumping * SystemAPI.Time.DeltaTime);
                knockbackVelocity.ValueRW.Angular *= math.exp(-angularDumping * SystemAPI.Time.DeltaTime);
            }
        }
    }
}