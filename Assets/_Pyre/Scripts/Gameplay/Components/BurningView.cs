using Unity.Entities;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    public struct BurningView : IComponentData
    {
        public Entity FireEntity;
    }
}