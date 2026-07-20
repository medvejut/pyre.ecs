using Pyre.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Pyre.Systems
{
    public partial struct SyncPlayerPosition : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            
            foreach (var monoPlayerTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<MonoPlayerTag>())
            {
                foreach (var playerTransform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<PlayerTag>())
                {
                    playerTransform.ValueRW = monoPlayerTransform.ValueRO;
                    return;
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}