using Pyre.Components;
using Pyre.Input;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Pyre
{
    [RequireComponent(typeof(CharacterController))]
    public class HeroController : MonoBehaviour
    {
        [SerializeField] private HeroInput input;
        [Space]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 15f;
        [Space]
        [SerializeField] private Vector3 isometricDirectionMultiplier = new(0, 45f, 0);

        private Entity Entity { get; set; }

        private CharacterController _characterController;
        private EntityManager _entityManager;

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();

            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity = _entityManager.CreateEntity();

            _entityManager.AddComponentData(Entity, new MonoPlayerTag());
            _entityManager.AddComponentData(Entity, LocalTransform.FromPosition(transform.position));
        }

        private void Update()
        {
            var direction = GetInputDirection();

            _characterController.Move(direction * (moveSpeed * Time.deltaTime));

            if (direction != Vector3.zero)
            {
                var lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }

            _entityManager.SetComponentData(Entity, LocalTransform.FromPositionRotation(transform.position, transform.rotation));
        }

        private Vector3 GetInputDirection()
        {
            var move = input.GetMove();
            return Quaternion.Euler(isometricDirectionMultiplier) * new Vector3(move.x, 0f, move.y);
        }
    }
}