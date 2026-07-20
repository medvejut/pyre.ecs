using Pyre.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Pyre.Systems
{
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

            foreach (var (transform, entity) in SystemAPI.Query<RefRW<LocalTransform>>()
                         .WithAll<Billboard>()
                         .WithEntityAccess())
            {
                if (parentLookup.TryGetComponent(entity, out var parent))
                {
                    var parentLtw = ltwLookup[parent.Value];
                    var parentRotation = parentLtw.Rotation;

                    transform.ValueRW.Rotation = math.mul(math.inverse(parentRotation), cameraRotation);
                }
            }
        }
    }
}