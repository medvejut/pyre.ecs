using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Pyre.Gameplay.Utils
{
    public static class BodyCenter
    {
        public static float3 GetWorldPosition(in PhysicsWorldSingleton physicsWorld, Entity entity, in LocalToWorld ltw)
        {
            var rigidBodyIndex = physicsWorld.GetRigidBodyIndex(entity);
            if (rigidBodyIndex == -1)
            {
                return ltw.Position;
            }

            var body = physicsWorld.Bodies[rigidBodyIndex];
            if (!body.Collider.IsCreated)
            {
                return ltw.Position;
            }

            return body.CalculateAabb().Center;
        }
    }
}