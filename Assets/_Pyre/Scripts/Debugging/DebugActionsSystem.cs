using Pyre.Animations;
using Pyre.Cameras.Components;
using Pyre.Gameplay.Components;
using Pyre.Player.Components;
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

namespace Pyre.Debugging
{
    public partial struct DebugActionsSystem : ISystem
    {
        private const float DebugAnimationDuration = 3f;

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
                foreach (var (warning, entity) in
                         SystemAPI.Query<RefRO<ExplosiveWarning>>().WithEntityAccess())
                {
                    if (warning.ValueRO.PlayPulse)
                    {
                        AnimationPlayer.Play(ecb, entity, DebugAnimationDuration, warning.ValueRO.Pulse);
                    }

                    if (warning.ValueRO.PlayBlink)
                    {
                        AnimationPlayer.Play(ecb, entity, DebugAnimationDuration, warning.ValueRO.Blink);
                    }
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

            if (Keyboard.current?.bKey.wasPressedThisFrame == true)
            {
                foreach (var (_, entity) in
                         SystemAPI.Query<RefRO<Explosive>>().WithNone<Burning>().WithEntityAccess())
                {
                    state.EntityManager.GetName(entity, out var name);
                    Debug.Log($"Adding Burning component to entity {entity} with name {name}");

                    ecb.AddComponent(entity, new Burning { HeatRadius = 0 });
                }
            }

            if (Keyboard.current?.digit1Key.wasPressedThisFrame == true)
            {
                SetEnemyAnimation(ref state, EnemyAnimation.Idle);
            }

            if (Keyboard.current?.digit2Key.wasPressedThisFrame == true)
            {
                SetEnemyAnimation(ref state, EnemyAnimation.Warning);
            }

            if (Keyboard.current?.digit3Key.wasPressedThisFrame == true)
            {
                SetEnemyAnimation(ref state, EnemyAnimation.Burning);
            }
        }

        private void SetEnemyAnimation(ref SystemState state, EnemyAnimation animation)
        {
            var count = 0;

            foreach (var enemyAnimation in SystemAPI.Query<RefRW<EnemyAnimationState>>())
            {
                enemyAnimation.ValueRW.State = animation;
                count++;
            }

            Debug.Log($"Enemy animation set to {animation} on {count} enemy(ies)");
        }
    }
}