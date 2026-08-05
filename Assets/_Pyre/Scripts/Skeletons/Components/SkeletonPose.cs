using Unity.Entities;

namespace Pyre.Skeletons.Components
{
    /// <summary>
    /// The pose currently asked of a skeleton, expressed as two playback heads over a
    /// <see cref="SkeletonClipLibrary"/> plus the blend between them. Two slots rather than one so idle to
    /// walk can cross-fade instead of popping — the manual equivalent of a 1D blend tree.
    ///
    /// Gameplay writes this; SkeletonPoseSystem reads it and applies the result to the bones.
    /// </summary>
    public struct SkeletonPose : IComponentData
    {
        public BlobAssetReference<SkeletonClipLibrary> Library;

        public int ClipA;
        public int ClipB;

        public float TimeA;
        public float TimeB;

        /// <summary>0 = pure A, 1 = pure B.</summary>
        public float Blend;

        public float Speed;
    }
}
