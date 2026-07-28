using Pyre.Animations.Components;
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

            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

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

            if (Keyboard.current?.eKey.wasPressedThisFrame == true)
            {
                foreach (var (pulseSource, entity) in
                         SystemAPI.Query<RefRO<PulseAnimationSource>>().WithEntityAccess())
                {
                    ecb.AddComponent(entity, new PulseAnimation
                    {
                        MinScale = pulseSource.ValueRO.MinScale,
                        MaxScale = pulseSource.ValueRO.MaxScale,
                        BaseFrequency = pulseSource.ValueRO.BaseFrequency,
                        MaxFrequency = pulseSource.ValueRO.MaxFrequency,

                        TotalDuration = 3f,
                        ElapsedTime = 0f,
                    });
                }

                foreach (var (blinkSource, entity) in
                         SystemAPI.Query<RefRO<BlinkAnimationSource>>().WithEntityAccess())
                {
                    ecb.AddComponent(entity, new BlinkAnimation
                    {
                        StartColor = blinkSource.ValueRO.StartColor,
                        EndColor = blinkSource.ValueRO.EndColor,
                        MinOpacity = blinkSource.ValueRO.MinOpacity,
                        MaxOpacity = blinkSource.ValueRO.MaxOpacity,
                        BaseFrequency = blinkSource.ValueRO.BaseFrequency,
                        MaxFrequency = blinkSource.ValueRO.MaxFrequency,

                        TotalDuration = 3f,
                        ElapsedTime = 0f,
                    });
                }
            }

            if (Keyboard.current?.aKey.wasPressedThisFrame == true)
            {
                var entity = SystemAPI.GetSingletonEntity<PlayerTag>();
                var archetype = state.EntityManager.GetChunk(entity).Archetype;

                var componentTypes = archetype.GetComponentTypes(Allocator.Temp);

                foreach (var type in componentTypes)
                {
                    Debug.Log($"Component: {type.GetManagedType().Name} IsManagedComponent={type.IsManagedComponent} IsComponent={type.IsComponent} ToString={type.ToString()}");
                }

                componentTypes.Dispose();

                var audioSource = SystemAPI.ManagedAPI.GetComponent<AudioSource>(entity);
                Debug.Log($"AudioSource: {audioSource.clip.name} IsPlaying={audioSource.isPlaying} Volume={audioSource.volume}");
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}