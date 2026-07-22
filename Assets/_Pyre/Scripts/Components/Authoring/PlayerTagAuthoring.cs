using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Components
{
    public class PlayerTagAuthoring : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private Vector3 isometricDirectionMultiplier = new(0f, 45f, 0f);

        public class PlayerTagBaker : Baker<PlayerTagAuthoring>
        {
            public override void Bake(PlayerTagAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerTag>(entity);
                AddComponent<PlayerMoveInput>(entity);
                AddComponent(entity, new PlayerMovement
                {
                    MoveSpeed = authoring.moveSpeed,
                    RotationSpeed = authoring.rotationSpeed,
                    IsometricRotation = quaternion.Euler(
                        math.radians(authoring.isometricDirectionMultiplier.x),
                        math.radians(authoring.isometricDirectionMultiplier.y),
                        math.radians(authoring.isometricDirectionMultiplier.z))
                });
            }
        }
    }
}