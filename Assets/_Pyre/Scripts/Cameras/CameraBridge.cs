using Pyre.Cameras.Components;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Cameras
{
    public class CameraBridge : MonoBehaviour
    {
        [SerializeField] private CameraShake cameraShake;

        private EntityQuery _entityQuery;

        private void Start()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            entityManager.CreateSingletonBuffer<CameraShakeEvent>();
            _entityQuery = entityManager.CreateEntityQuery(typeof(CameraShakeEvent));
        }

        private void LateUpdate()
        {
            var buffer = _entityQuery.GetSingletonBuffer<CameraShakeEvent>();
            foreach (var _ in buffer)
            {
                cameraShake.Shake();
            }

            buffer.Clear();
        }
    }
}