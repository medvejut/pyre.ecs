using Unity.Entities;
using UnityEngine;

namespace Pyre.Components
{
    public struct BurningView : IComponentData
    {
        public Entity FireEntity;
    }
}