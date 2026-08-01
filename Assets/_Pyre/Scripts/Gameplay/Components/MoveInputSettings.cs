using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pyre.Gameplay.Components
{
    [Serializable]
    public struct MoveInputSettings : IComponentData
    {
        [Tooltip("Yaw that maps raw movement input onto world axes. " +
                 "Match the camera rig's Y rotation for screen-relative movement.")]
        [Range(-180f, 180f)] public float Yaw;

        public quaternion InputToWorld => quaternion.RotateY(math.radians(Yaw));
    }
}
