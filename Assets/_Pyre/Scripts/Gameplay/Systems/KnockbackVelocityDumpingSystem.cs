using Pyre.Gameplay.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Gameplay.Systems
{
    public partial struct KnockbackVelocityDumpingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<KnockbackSettings>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var settings = SystemAPI.GetSingleton<KnockbackSettings>();
            var deltaTime = SystemAPI.Time.DeltaTime;

            var linearDecay = math.exp(-settings.LinearDamping * deltaTime);
            var angularDecay = math.exp(-settings.AngularDamping * deltaTime);

            foreach (var knockbackVelocity in SystemAPI
                         .Query<RefRW<KnockbackVelocity>>())
            {
                knockbackVelocity.ValueRW.Linear *= linearDecay;
                knockbackVelocity.ValueRW.Angular *= angularDecay;
            }
        }
    }
}