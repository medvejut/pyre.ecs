using Pyre.Transforms.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Pyre.Transforms.Systems
{
    [UpdateInGroup(typeof(TransformSystemGroup), OrderFirst = true)]
    public partial struct FreezeRotationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var parentLookup = SystemAPI.GetComponentLookup<Parent>(true);
            var ltwLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

            foreach (var (freezeRotation, transform, entity) in SystemAPI
                         .Query<RefRO<FreezeWorldRotation>, RefRW<LocalTransform>>()
                         .WithEntityAccess())
            {
                var parentWorldRotation = quaternion.identity;
                if (parentLookup.TryGetComponent(entity, out var parent) &&
                    ltwLookup.TryGetComponent(parent.Value, out var parentLtw))
                {
                    parentWorldRotation = parentLtw.Rotation;
                }

                transform.ValueRW.Rotation = math.mul(math.inverse(parentWorldRotation), freezeRotation.ValueRO.WorldRotation);
            }
        }
    }
}