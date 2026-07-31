using Pyre.Transforms.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Pyre.Transforms.Systems
{
    [UpdateInGroup(typeof(TransformSystemGroup), OrderFirst = true)]
    public partial struct BillboardSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var camera = Camera.main;
            if (camera == null)
                return;

            quaternion cameraRotation = camera.transform.rotation;

            var parentLookup = SystemAPI.GetComponentLookup<Parent>(true);
            var ltwLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);

            foreach (var (transform, entity) in SystemAPI
                         .Query<RefRW<LocalTransform>>()
                         .WithAll<Billboard>()
                         .WithEntityAccess())
            {
                var parentWorldRotation = quaternion.identity;
                if (parentLookup.TryGetComponent(entity, out var parent) &&
                    ltwLookup.TryGetComponent(parent.Value, out var parentLtw))
                {
                    parentWorldRotation = parentLtw.Rotation;
                }

                transform.ValueRW.Rotation = math.mul(math.inverse(parentWorldRotation), cameraRotation);
            }
        }
    }
}