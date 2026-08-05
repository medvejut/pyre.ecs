using Pyre.Player.Components;
using Pyre.Settings;
using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public class GameConfigAuthoring : MonoBehaviour
    {
        public GameConfig Config;

        public class GameConfigBaker : Baker<GameConfigAuthoring>
        {
            public override void Bake(GameConfigAuthoring authoring)
            {
                DependsOn(authoring.Config);

                if (authoring.Config == null)
                {
                    Debug.LogError($"No GameConfig assigned on '{authoring.name}'. " +
                                   "Systems depending on game settings will not run.", authoring);
                    return;
                }

                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new MoveInputSettings { Yaw = authoring.Config.inputYaw });
                AddComponent(entity, new KnockbackSettings
                {
                    LinearDamping = authoring.Config.knockbackLinearDamping,
                    AngularDamping = authoring.Config.knockbackAngularDamping
                });
                AddComponent(entity, new AudioDefaults { ExtinguishSound = authoring.Config.extinguishSound });
            }
        }
    }
}