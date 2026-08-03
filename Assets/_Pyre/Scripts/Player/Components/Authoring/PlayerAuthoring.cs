using Pyre.Gameplay.Components;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Player.Components
{
    public class PlayerAuthoring : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 15f;

        public class PlayerTagBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent<PlayerTag>(entity);

                AddComponent<PlayerMoveInput>(entity);
                AddComponent(entity, new PlayerMovement
                {
                    MoveSpeed = authoring.moveSpeed,
                    RotationSpeed = authoring.rotationSpeed
                });
                AddComponent<KnockbackVelocity>(entity);
            }
        }
    }
}