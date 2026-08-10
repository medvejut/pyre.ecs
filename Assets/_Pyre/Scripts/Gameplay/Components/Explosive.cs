using Unity.Entities;

namespace Pyre.Gameplay.Components
{
    // The fuse: whether and how long after catching fire this thing goes off.
    // What it shows meanwhile is ExplosiveWarning, what it does is ExplosiveCharge.
    public struct Explosive : IComponentData
    {
        public bool ExplodeOnStartBurn;
        public float Delay;
    }
}
