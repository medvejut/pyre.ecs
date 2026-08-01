using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Pyre.Gameplay.Utils
{
    public static class BodyCenter
    {
        /// <summary>
        /// World-space center of an entity's collider. Prefab origins sit at the model's feet, so the
        /// transform position is half a body below the thing radius queries are supposed to measure from.
        /// The baked collider already carries the right offset, so read the center back from it.
        /// Falls back to the transform origin when the entity has no rigid body or no collider.
        /// </summary>
        public static float3 Get(in PhysicsWorldSingleton physicsWorld, Entity entity, in LocalToWorld ltw)
        {
            var rigidBodyIndex = physicsWorld.GetRigidBodyIndex(entity);
            if (rigidBodyIndex == -1)
                return ltw.Position;

            var body = physicsWorld.Bodies[rigidBodyIndex];
            if (!body.Collider.IsCreated)
                return ltw.Position;

            return body.CalculateAabb().Center;
        }
    }
}
