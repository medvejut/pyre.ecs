using Pyre.Cameras;
using Pyre.Cameras.Components;
using Pyre.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using Random = Unity.Mathematics.Random;

namespace Pyre.Systems
{
    public partial struct DebugActionsSystem : ISystem
    {
        private bool _toggleVfx;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            _toggleVfx = true;
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!Keyboard.current.rightAltKey.isPressed)
                return;

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


            if (Keyboard.current?.vKey.wasPressedThisFrame == true)
            {
                foreach (var vfx in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<VisualEffect>>())
                {
                    Debug.Log($"VFX: {vfx.Value.name}");
                    if (_toggleVfx)
                    {
                        vfx.Value.Stop();
                    }
                    else
                    {
                        vfx.Value.Play();
                    }
                }

                _toggleVfx = !_toggleVfx;
            }

            if (Keyboard.current?.tKey.wasPressedThisFrame == true)
            {
                var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

                foreach (var (burningLtw, burning, entity) in SystemAPI
                             .Query<RefRO<LocalToWorld>, RefRO<Burning>>()
                             .WithEntityAccess())
                {
                    Debug.Log($"Checking for water hit for entity {entity}");
                    var rigidBodyIndex = physicsWorld.GetRigidBodyIndex(entity);
                    if (rigidBodyIndex != -1)
                    {
                        var hits = new NativeList<ColliderCastHit>(Allocator.Temp);

                        var body = physicsWorld.Bodies[rigidBodyIndex];

                        var input = new ColliderCastInput(body.Collider, burningLtw.ValueRO.Position, burningLtw.ValueRO.Position);
                        if (physicsWorld.CastCollider(input, ref hits))
                        {
                            foreach (var hit in hits)
                            {
                                var collideWater = SystemAPI.HasComponent<Water>(hit.Entity);
                                Debug.Log($"Hit detected for entity {entity} with {hit.Entity} {collideWater}");
                                if (collideWater)
                                {
                                    Debug.Log($"Water hit detected for entity {entity} with {hit.Entity}");
                                }
                            }
                        }

                        hits.Dispose();
                    }
                }
            }

            if (Keyboard.current?.cKey.wasPressedThisFrame == true)
            {
                var cameraShakeBuffer = SystemAPI.GetSingletonBuffer<CameraShakeEvent>();
                cameraShakeBuffer.Add(new CameraShakeEvent());
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}