using Pyre.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Pyre.Systems
{
    public partial struct ExplodeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            ProcessExplodeOnStartBurn(ref state, ecb);
            ExplodeAfterDelay(ref state, ecb);
        }

        private void ProcessExplodeOnStartBurn(ref SystemState state, EntityCommandBuffer ecb)
        {
            foreach (var (explosive, entity) in
                     SystemAPI.Query<RefRO<Explosive>>()
                         .WithNone<ExplodeTimer>()
                         .WithAll<Burning>()
                         .WithEntityAccess())
            {
                if (explosive.ValueRO.ExplodeOnStartBurn)
                {
                    ecb.AddComponent(entity, new ExplodeTimer { TimeRemaining = explosive.ValueRO.Delay });
                }
            }
        }

        private void ExplodeAfterDelay(ref SystemState state, EntityCommandBuffer ecb)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (explodeTimer, explosive, ltw, entity) in
                     SystemAPI.Query<RefRW<ExplodeTimer>, RefRO<Explosive>, RefRO<LocalToWorld>>()
                         .WithEntityAccess())
            {
                explodeTimer.ValueRW.TimeRemaining -= deltaTime;

                if (explodeTimer.ValueRO.TimeRemaining <= 0f)
                {
                    var explosionEntity = ecb.CreateEntity();

                    ecb.AddComponent(explosionEntity, new Explosion
                    {
                        Position = ltw.ValueRO.Position + explosive.ValueRO.ExplosionOffset,
                        Radius = explosive.ValueRO.ExplosionRadius,
                        Impulse = explosive.ValueRO.ExplosionImpulse,
                        AngularImpulse = CalculateExplosionAngularImpulse(explosive),
                    });

                    ecb.RemoveComponent<ExplodeTimer>(entity);
                }
            }
        }

        private static float3 CalculateExplosionAngularImpulse(RefRO<Explosive> explosive)
        {
            var random = Random.CreateFromIndex(explosive.ValueRO.CustomExplosionAngularImpulseRandomSeed);
            return random.NextFloat3Direction() * explosive.ValueRO.CustomExplosionAngularImpulseMultiplier;
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}