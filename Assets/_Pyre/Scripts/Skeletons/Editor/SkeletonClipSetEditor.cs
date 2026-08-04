using Pyre.Skeletons.Settings;
using UnityEditor;
using UnityEngine;

namespace Pyre.Skeletons.Editor
{
    /// <summary>
    /// Bakes the clips of a <see cref="SkeletonClipSet"/> into flat arrays on the asset.
    ///
    /// This deliberately runs from the inspector rather than from a Baker: AnimationClip.SampleAnimation
    /// mutates the hierarchy it samples, and bakers must not have side effects. Sampling into a live
    /// hierarchy and reading the transforms back also makes the clip-curve-path to bone-index mapping a
    /// non-issue — Unity resolves the paths, we just read the result in a fixed order.
    /// </summary>
    [CustomEditor(typeof(SkeletonClipSet))]
    public class SkeletonClipSetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var set = (SkeletonClipSet)target;

            EditorGUILayout.Space();

            var missingSource = set.modelPrefab == null || set.clips == null || set.clips.Length == 0;

            using (new EditorGUI.DisabledScope(missingSource))
            {
                if (GUILayout.Button("Bake Clips"))
                    Bake(set);
            }

            if (missingSource)
            {
                EditorGUILayout.HelpBox("Assign a model prefab and at least one clip to bake.", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox(
                set.HasBakedData
                    ? $"{set.bakedClips.Length} clip(s) baked over {set.boneCount} bones."
                    : "Not baked yet.",
                set.HasBakedData ? MessageType.None : MessageType.Warning);
        }

        private static void Bake(SkeletonClipSet set)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(set.modelPrefab);

            if (instance == null)
            {
                Debug.LogError($"[{set.name}] Could not instantiate {set.modelPrefab}.", set);
                return;
            }

            try
            {
                var root = instance.transform;
                var bones = instance.GetComponentsInChildren<Transform>(true);

                // Bind pose, read before any sampling mutates the hierarchy.
                var bindTranslations = new Vector3[bones.Length];
                var bonePaths = new string[bones.Length];

                for (var b = 0; b < bones.Length; b++)
                {
                    bindTranslations[b] = bones[b].localPosition;
                    bonePaths[b] = SkeletonClipSet.GetBonePath(root, bones[b]);

                    var scale = bones[b].localScale;
                    if (!Mathf.Approximately(scale.x, scale.y) || !Mathf.Approximately(scale.x, scale.z))
                    {
                        Debug.LogWarning(
                            $"[{set.name}] Bone '{bonePaths[b]}' has non-uniform scale {scale}. " +
                            "LocalTransform only carries uniform scale, so only the x component is baked.", set);
                    }
                }

                var rootMotionIndex = -1;

                if (set.stripRootMotion)
                {
                    rootMotionIndex = System.Array.FindIndex(bones, b => b.name == set.rootMotionBone);

                    if (rootMotionIndex < 0)
                    {
                        Debug.LogWarning(
                            $"[{set.name}] stripRootMotion is on but no bone named '{set.rootMotionBone}' was found. " +
                            "Root motion will be baked into the clips and will fight PlayerMovementSystem.", set);
                    }
                }

                var baked = new BakedSkeletonClip[set.clips.Length];
                var sampleRate = Mathf.Max(1, set.sampleRate);

                for (var c = 0; c < set.clips.Length; c++)
                {
                    var clip = set.clips[c];

                    if (clip == null)
                    {
                        Debug.LogError($"[{set.name}] Clip slot {c} is empty.", set);
                        return;
                    }

                    // +1 so the sampled range is inclusive of the clip end: for a looping clip the last
                    // frame equals the first, which is what makes the wrap in the pose system seamless.
                    var frameCount = Mathf.Max(2, Mathf.RoundToInt(clip.length * sampleRate) + 1);
                    var keyCount = frameCount * bones.Length;

                    var translations = new Vector3[keyCount];
                    var rotations = new Quaternion[keyCount];
                    var scales = new float[keyCount];

                    for (var f = 0; f < frameCount; f++)
                    {
                        clip.SampleAnimation(instance, Mathf.Min(f / (float)sampleRate, clip.length));

                        for (var b = 0; b < bones.Length; b++)
                        {
                            var key = f * bones.Length + b;

                            translations[key] = b == rootMotionIndex ? bindTranslations[b] : bones[b].localPosition;
                            rotations[key] = bones[b].localRotation;
                            scales[key] = bones[b].localScale.x;
                        }
                    }

                    baked[c] = new BakedSkeletonClip
                    {
                        name = clip.name,
                        length = clip.length,
                        frameCount = frameCount,
                        looping = clip.isLooping,
                        translations = translations,
                        rotations = rotations,
                        scales = scales,
                    };
                }

                Undo.RecordObject(set, "Bake Clips");

                set.boneCount = bones.Length;
                set.bonePaths = bonePaths;
                set.bakedClips = baked;

                EditorUtility.SetDirty(set);
                AssetDatabase.SaveAssetIfDirty(set);

                Debug.Log($"[{set.name}] Baked {baked.Length} clip(s) over {bones.Length} bones at {sampleRate} fps.", set);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }
    }
}
