using System;
using UnityEngine;

namespace Pyre.Skeletons.Settings
{
    /// <summary>
    /// One clip sampled onto a fixed bone list. Every array is flat and indexed [frame * boneCount + bone],
    /// where the bone order is the one recorded in <see cref="SkeletonClipSet.bonePaths"/>.
    /// </summary>
    [Serializable]
    public class BakedSkeletonClip
    {
        public string name;
        public float length;
        public int frameCount;
        public bool looping;

        public Vector3[] translations;
        public Quaternion[] rotations;
        public float[] scales;
    }

    [CreateAssetMenu(fileName = "SkeletonClipSet", menuName = "Pyre/Skeletons/Skeleton Clip Set")]
    public class SkeletonClipSet : ScriptableObject
    {
        [Header("Source")]
        public GameObject modelPrefab;
        public AnimationClip[] clips = Array.Empty<AnimationClip>();

        [Tooltip("Frames sampled per second. Sampling includes the clip end, so a clip bakes to length * sampleRate + 1 frames.")]
        public int sampleRate = 30;

        [Tooltip("Hold the root motion bone at its bind pose translation, so clips do not fight PlayerMovementSystem.")]
        public bool stripRootMotion = true;

        [Tooltip("Transform that carries root motion. Matches rootMotionBoneName in the model importer.")]
        public string rootMotionBone = "root";

        [Header("Baked — regenerate with Bake Clips")]
        public int boneCount;

        [Tooltip("Path of every sampled bone relative to the model root, in sample order. The baker verifies its own hierarchy against this.")]
        public string[] bonePaths = Array.Empty<string>();

        public BakedSkeletonClip[] bakedClips = Array.Empty<BakedSkeletonClip>();

        public bool HasBakedData => boneCount > 0 && bakedClips is { Length: > 0 };

        /// <summary>
        /// Index of a baked clip by name, or -1. Clip names are resolved to indices at bake time so that
        /// systems never carry hardcoded integers.
        /// </summary>
        public int IndexOf(string clipName)
        {
            if (string.IsNullOrEmpty(clipName) || bakedClips == null)
                return -1;

            for (var i = 0; i < bakedClips.Length; i++)
            {
                if (bakedClips[i] != null && bakedClips[i].name == clipName)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Path of <paramref name="bone"/> relative to <paramref name="root"/>, empty for the root itself.
        /// Shared by the bake tool and the baker so both describe the hierarchy the same way.
        /// </summary>
        public static string GetBonePath(Transform root, Transform bone)
        {
            if (bone == root)
                return string.Empty;

            var path = bone.name;

            for (var parent = bone.parent; parent != null && parent != root; parent = parent.parent)
                path = parent.name + "/" + path;

            return path;
        }
    }
}
