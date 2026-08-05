using Unity.Entities;
using Unity.Mathematics;

namespace Pyre.Skeletons.Components
{
    public struct BoneKey
    {
        public float3 Translation;
        public quaternion Rotation;
        public float Scale;
    }

    public struct SkeletonClipBlob
    {
        public float Length;
        public float FrameRate;
        public int FrameCount;
        public int BoneCount;
        public bool Looping;

        /// <summary>Flat pose keys, indexed [frame * BoneCount + bone].</summary>
        public BlobArray<BoneKey> Keys;
    }

    public struct SkeletonClipLibrary
    {
        public BlobArray<SkeletonClipBlob> Clips;
    }
}
