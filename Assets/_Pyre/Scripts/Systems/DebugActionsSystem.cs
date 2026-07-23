using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
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
            _toggleVfx = true;
        }

        public void OnUpdate(ref SystemState state)
        {
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
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}