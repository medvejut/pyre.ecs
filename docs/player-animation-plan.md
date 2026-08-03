# Player skeletal animation — pure ECS (no third-party packages)

## Context

The Hero currently has no animation at all. `Assets/_Pyre/Prefabs/Hero.prefab` renders its body via a
child `body-mesh` with a plain `MeshFilter` + `MeshRenderer` pointing at the *static* mesh inside
`Assets/_Pyre/Models/character-oopi.fbx` (localScale 3). There is not a single `Animator` or
`SkinnedMeshRenderer` anywhere in the project.

Two things were tried and both failed for the same underlying reason:

1. **`Animator` stops working once the model is in the subscene.** Baking converts GameObjects to
   entities and drops every `MonoBehaviour` that has no baker. Entities/Entities.Graphics ship
   companion-GameObject bakers for `Light`, `VisualEffect`, `AudioSource`, `ParticleSystem`,
   `ReflectionProbe` — which is exactly why `LightBlinkAnimationSystem` and `BurningViewSystem` can
   reach those through `SystemAPI.ManagedAPI`. **`Animator` is not on that list**, so it is silently
   discarded during conversion.

2. **The "default HDRP material + skinned mesh" warning.** Entities Graphics *does* bake a
   `SkinnedMeshRenderer` — it produces a `DeformedEntity` reference plus a `DynamicBuffer<SkinMatrix>`
   and expects the material's shader to contain Shader Graph's **Linear Blend Skinning** (or **Compute
   Deformation**) node. `HDRP/Lit` (shader guid `6e4ae4064600d784cac1e41a9e6f2e59`, what every material
   in `Assets/_Pyre/Materials/` uses) has neither node, so Entities Graphics warns that the mesh will
   render undeformed.

So Entities Graphics 6.5.0 can *render* a deformed mesh, but nothing *drives* the deformation: Unity
discontinued `com.unity.animation`, and there is no replacement in the box. **That missing half is
what we are writing.** We fill `DynamicBuffer<SkinMatrix>` ourselves from animation data we bake into
blob assets, so the character stays a normal entity in the subscene with no GameObject view and no new
packages.

**Scope:** player (Hero / `character-oopi.fbx`) only. Enemy stays a static mesh. Interaction animation
is driven by the existing ignite/burn gameplay state (`IgnitionProgress`, `Burning`) — no new input
action.

### Honest complexity assessment

This is the largest of the options and amounts to writing a miniature animation runtime: roughly
**450–600 lines of new C#** across ~10 files, plus a Shader Graph and an editor baking tool. Budget a
few focused sessions. The parts that reliably eat time are not the code volume but:

* getting bind-pose / root-space maths right (a wrong space gives an exploded or inside-out mesh),
* discovering exactly which entity Entities Graphics puts the `SkinMatrix` buffer on,
* the fact that Unity documents the render side and the compute side but not the join between them.

The payoff is that it stays 100% ECS and it teaches blob assets, `ComponentLookup` writes across
entities, transform-system ordering, and system groups — which is what you're after.

The staging below is deliberate: **each stage ends at something you can see on screen**, so a failure
is localized instead of being a black screen with five suspects.

---

## How the pieces fit

```
AnimationClip (FBX)
   │  editor bake (sample TRS per frame per bone)
   ▼
CharacterAnimationSet (ScriptableObject, serialized float arrays)
   │  Baker → BlobAssetReference<AnimationLibraryBlob>
   ▼
AnimationPlayer (IComponentData)  ──┐
SkeletonBone   (IBufferElementData) │  SkeletalAnimationSamplingSystem  (before TransformSystemGroup)
                                    ▼  writes LocalTransform of every bone entity
                          TransformSystemGroup → LocalToWorld per bone
                                    │
SkinBone (IBufferElementData) ──────┤  ComputeSkinMatricesSystem  (PresentationSystemGroup)
                                    ▼  writes DynamicBuffer<SkinMatrix>
                    Entities Graphics deformation systems → GPU
                                    ▼
                Shader Graph material w/ Linear Blend Skinning node
```

Two separate bone lists, and this distinction matters:

* **`SkeletonBone`** — *every* transform under the model root that the clips animate, in hierarchy
  order. This is what the sampling system writes `LocalTransform` to.
* **`SkinBone`** — only `SkinnedMeshRenderer.bones[i]` plus `sharedMesh.bindposes[i]`, in the
  renderer's own order. This is what the skin-matrix system reads.

They are usually *not* the same set. `smr.bones` omits intermediate/parent transforms that carry
animation but influence no vertices; if you drive only `smr.bones`, those parents stay frozen at bind
pose and the character animates wrong in a way that is very confusing to debug.

---

## Stage 0 — Prerequisites

* **`git lfs pull`.** Every `.fbx` in this checkout is a 131-byte LFS pointer. Nothing below works
  until the real files are present.
* **Importer settings on `Assets/_Pyre/Models/character-oopi.fbx`** (currently `animationType: 2`
  Generic, `avatarSetup: 0` No Avatar, `clipAnimations: []`):
  * Keep **Animation Type = Generic**.
  * Set **Avatar Definition = Create From This Model** (`avatarSetup: 1`). Not strictly required by
    this approach — generic clips bind by transform path, so `SampleAnimation` works without an
    Avatar — but it makes the rig inspectable and costs nothing.
  * Keep **`optimizeGameObjects: 0`**. If this is ever turned on the bone hierarchy is stripped and
    there is nothing left to animate.
  * Under **Model**, enable **Read/Write** on the mesh. Entities Graphics' deformation path needs CPU
    access to the mesh data.
  * Confirm the clips actually import as sub-assets and **write down their exact names** — the FBX is
    LFS-only in this checkout so the names could not be read here, and Stage 2 needs them.
* Confirm in the Project window that the imported model prefab has a real `SkinnedMeshRenderer` and a
  bone hierarchy. If the FBX has animation but no skinning, this whole approach does not apply and the
  model needs to be re-exported.

---

## Stage 1 — Get a skinned mesh rendering, undeformed

Goal: the warning disappears and the character still draws. No animation yet.

1. **New Shader Graph** `Assets/_Pyre/Shaders/Character Skinned.shadergraph`, HDRP **Lit** target, to
   match the look of the existing HDRP/Lit materials.
   * Add a **Linear Blend Skinning** node; wire its `Position` / `Normal` / `Tangent` outputs into the
     Vertex block. This node only appears when Entities Graphics is installed — if it is missing,
     stop and resolve that first.
   * Base Color: `Sample Texture 2D` of the character colormap (the existing materials bind
     `_BaseColorMap`; the character's atlas lives in `Assets/_Pyre/Textures/`) by UV0. Mirror
     `Assets/_Pyre/Shaders/Bomb Shader Graph.shadergraph` for the target/settings conventions already
     used here.
   * The **Compute Deformation** node and the `ENABLE_COMPUTE_DEFORMATIONS` scripting define are only
     needed for blend shapes. Skip both — linear blend skinning is enough and is one less moving part.
2. **New material** `Assets/_Pyre/Materials/Character.mat` using that graph.
3. **Edit `Assets/_Pyre/Prefabs/Hero.prefab`:** delete the `body-mesh` child and replace it with an
   instance of the `character-oopi` model prefab (which brings the `SkinnedMeshRenderer` *and* the bone
   hierarchy). Set its localScale to **3** to preserve the current size, and assign `Character.mat` in
   the `SkinnedMeshRenderer`'s material slot — this overrides the FBX-embedded material without having
   to extract it.
   * Leave the other children (`indicator-square-c`, `Icon_Fire`, `Icon_Ignition`, `Fire`) untouched;
     they are parented to the Hero root, which still moves normally.
   * `LocalTransform` only supports uniform scale, so 3 is fine.
4. Re-open/re-bake the subscene `Assets/_Pyre/Scenes/Prototype/Entity Sub Scene.unity`.

**Checkpoint:** Play. The character renders in bind pose, no HDRP-material warning. Open
*Window → Entities → Hierarchy*, select the baked Hero, and **record which entity carries
`DynamicBuffer<SkinMatrix>` and which carries `DeformedEntity`.** Stage 4 needs this, and it is the one
detail worth confirming empirically rather than assuming — if Entities Graphics 6.5.0 does not create
the buffer itself, our own baker adds it, sized to `smr.bones.Length`.

---

## Stage 2 — Bake clips and skeleton into blob assets

Follow the existing conventions: `Feature/Components/*.cs`, `Feature/Components/Authoring/*Authoring.cs`,
`Feature/Systems/*System.cs`, `Feature/Settings/*Config.cs`, namespace `Pyre.Animations.*`. There are no
asmdefs, so everything lands in `Assembly-CSharp` and editor-only code must live in an `Editor/` folder.

**`Assets/_Pyre/Scripts/Animations/Settings/CharacterAnimationSet.cs`** — a `ScriptableObject`
(`[CreateAssetMenu(menuName = "Pyre/Animations/Character Animation Set")]`, matching the existing
`BlinkAnimationConfig` / `PulseAnimationConfig` pattern). Holds the model prefab, the list of
`AnimationClip`s, a sample rate (30 is plenty), and the *baked output*: for each clip, a flat
`Vector3[] translations`, `Quaternion[] rotations`, `float[] scales` laid out `[frame * boneCount + bone]`,
plus `boneCount`, `frameCount`, `length`, `looping`.

**`Assets/_Pyre/Scripts/Animations/Editor/CharacterAnimationSetEditor.cs`** — a `[CustomEditor]` with a
**Bake Clips** button. Doing the sampling here rather than inside a Baker matters: `SampleAnimation`
*mutates the hierarchy it samples*, and bakers must not have side effects. The button:

```csharp
var instance = (GameObject)PrefabUtility.InstantiatePrefab(set.ModelPrefab);
var bones = instance.GetComponentsInChildren<Transform>(true); // hierarchy order, index 0 = root
foreach (var clip in set.Clips)
    for (var f = 0; f < frameCount; f++)
    {
        clip.SampleAnimation(instance, f / (float)set.SampleRate);
        for (var b = 0; b < bones.Length; b++)
        {
            translations[f * bones.Length + b] = bones[b].localPosition;
            rotations   [f * bones.Length + b] = bones[b].localRotation;
            scales      [f * bones.Length + b] = bones[b].localScale.x;
        }
    }
Object.DestroyImmediate(instance);
EditorUtility.SetDirty(set);
```

Sampling into the live hierarchy and reading transforms back is what makes clip-curve-path ↔ bone-index
mapping a non-issue: Unity resolves the paths, we just read the result in a fixed order.

**Root motion:** if the clips translate the root bone, the character will slide and fight
`PlayerMovementSystem` (which writes `PhysicsVelocity.Linear` directly). Zero out `translations` for
bone index 0 at bake time, behind a `stripRootMotion` toggle on the asset.

**`Assets/_Pyre/Scripts/Animations/Components/AnimationLibraryBlob.cs`**

```csharp
public struct BoneKey { public float3 Translation; public quaternion Rotation; public float Scale; }

public struct AnimationClipBlob
{
    public float Length, FrameRate;
    public int FrameCount, BoneCount;
    public bool Looping;
    public BlobArray<BoneKey> Keys;   // [frame * BoneCount + bone]
}

public struct AnimationLibraryBlob { public BlobArray<AnimationClipBlob> Clips; }
```

**`Assets/_Pyre/Scripts/Animations/Components/SkeletonBone.cs`** — `IBufferElementData { Entity Bone; }`,
hierarchy order, matching the bake order exactly.

**`Assets/_Pyre/Scripts/Animations/Components/SkinBone.cs`** — `IBufferElementData { Entity Bone; float4x4 BindPose; }`,
in `smr.bones` order.

**`Assets/_Pyre/Scripts/Animations/Components/Authoring/CharacterAnimationAuthoring.cs`** — sits on the
model root next to the `SkinnedMeshRenderer`. Its baker:

* `DependsOn(authoring.AnimationSet)` — same rebake-dependency idiom as
  `BlinkAnimationSourceAuthoring`.
* Builds the `AnimationLibraryBlob` with a `BlobBuilder` and `AddBlobAsset` (so it is deduplicated and
  serialized into the subscene).
* Fills `SkeletonBone` from `GetComponentsInChildren<Transform>(true)` via
  `GetEntity(t, TransformUsageFlags.Dynamic)` — **`Dynamic` is required**, the bones must have a
  writable `LocalTransform`.
* Fills `SkinBone` from `smr.bones[i]` + `smr.sharedMesh.bindposes[i]`.
* Adds `AnimationPlayer` (Stage 3) and, if Stage 1 showed it absent, the `SkinMatrix` buffer.

---

## Stage 3 — Sample the pose onto the bones

**`Assets/_Pyre/Scripts/Animations/Components/AnimationPlayer.cs`** — designed for a manual two-clip
blend from the start, which is what makes idle↔walk not pop:

```csharp
public struct AnimationPlayer : IComponentData
{
    public BlobAssetReference<AnimationLibraryBlob> Library;
    public int   ClipA, ClipB;
    public float TimeA, TimeB;
    public float Blend;   // 0 = pure A, 1 = pure B
    public float Speed;
}
```

**`Assets/_Pyre/Scripts/Animations/Systems/SkeletalAnimationSamplingSystem.cs`** — Burst `ISystem`,
`[UpdateInGroup(typeof(SimulationSystemGroup))] [UpdateBefore(typeof(TransformSystemGroup))]`, so the
bone `LocalToWorld`s are rebuilt from the new pose in the same frame.

An `IJobEntity` over `(ref AnimationPlayer, in DynamicBuffer<SkeletonBone>)` advances both clip times
(wrapping on `Length` when `Looping`), samples each clip at its two bracketing frames with
`math.lerp` / `math.slerp`, blends A→B by `Blend`, and writes the result through a
`ComponentLookup<LocalTransform>`.

Write via `.Schedule()` (single-threaded) rather than `.ScheduleParallel()`. Parallel writes through a
`ComponentLookup` need `[NativeDisableParallelForRestriction]`, which is only safe because bone sets
are disjoint — an assumption not worth taking on for one character.

**Checkpoint:** force `ClipA` to a walk clip and press Play. The bone entities should visibly animate
in the Entities Hierarchy inspector *even though the mesh is still in bind pose* — nothing is feeding
`SkinMatrix` yet. Seeing the transforms move here proves Stages 2–3 independently of the GPU side.

---

## Stage 4 — Compute skin matrices

**`Assets/_Pyre/Scripts/Animations/Systems/ComputeSkinMatricesSystem.cs`** — Burst `ISystem`,
`[UpdateInGroup(typeof(PresentationSystemGroup))]` and `[UpdateBefore(typeof(Unity.Rendering.DeformationsInPresentation))]`
(verify that group name against the installed package; if it is not public, order relative to whatever
Entities Graphics 6.5.0 exposes, or fall back to `[UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]`).

For each entity holding the `SkinMatrix` buffer, with `rootLtw` = the `LocalToWorld` of *that same
entity* (the deformed entity — this is the space Linear Blend Skinning expects, not the world):

```csharp
var worldToRoot = math.inverse(rootLtw.Value);
for (var i = 0; i < skinBones.Length; i++)
{
    var m = math.mul(worldToRoot, math.mul(ltwLookup[skinBones[i].Bone].Value, skinBones[i].BindPose));
    skinMatrices[i] = new SkinMatrix { Value = new float3x4(m.c0.xyz, m.c1.xyz, m.c2.xyz, m.c3.xyz) };
}
```

**Checkpoint:** the mesh deforms. If it explodes or turns inside out, the cause is almost always this
formula's spaces — try the deformed entity's `LocalToWorld` vs the renderer entity's, and confirm
`bindposes` orientation, before touching anything else.

---

## Stage 5 — Drive clip selection from gameplay

**`Assets/_Pyre/Scripts/Animations/Components/CharacterAnimationState.cs`**

```csharp
public struct CharacterAnimationState : IComponentData
{
    public float NormalizedSpeed;  // 0..1
    public bool  IsBurning;
    public float IgnitionRatio;    // 0..1, "being heated" reaction
}
```

**`Assets/_Pyre/Scripts/Animations/Systems/CharacterAnimationStateSystem.cs`** — Burst `ISystem` in
`SimulationSystemGroup`, `[UpdateBefore(typeof(SkeletalAnimationSamplingSystem))]`. Nothing currently
stores a speed anywhere (`PlayerMovementSystem` writes `PhysicsVelocity.Linear` and moves on), so
derive it:

* `NormalizedSpeed = saturate(length(velocity.Linear.xz) / movement.MoveSpeed)` from `PhysicsVelocity`
  + `PlayerMovement` (`Assets/_Pyre/Scripts/Player/Components/PlayerMovement.cs`).
* `IsBurning = HasComponent<Burning>(entity)` — added by `FirePropagationSystem`
  (`Assets/_Pyre/Scripts/Gameplay/Systems/FirePropagationSystem.cs:100`), removed by
  `FireExtinguishingSystem`.
* `IgnitionRatio = IgnitionProgress.Elapsed / Ignitable.IgnitionTime` — the Hero is `Ignitable`, so
  this rises while it stands in heat and decays via `CoolingRate`. This is the "interaction" signal:
  a flinch/react pose that blends in as the player starts catching fire, and a burning loop once
  `Burning` lands.

**`Assets/_Pyre/Scripts/Animations/Systems/CharacterAnimationSelectionSystem.cs`** — maps that state to
`AnimationPlayer`:

* `ClipA = Idle`, `ClipB = Walk`, `Blend = NormalizedSpeed` — a hand-rolled 1D blend tree, and the
  reason `AnimationPlayer` carries two slots.
* Scale `Speed` with `NormalizedSpeed` so footfalls roughly track ground speed.
* When `IsBurning`, swap `ClipB` to the burn/panic clip and drive `Blend` toward 1 over a short fade
  rather than snapping.
* Keep `TimeA`/`TimeB` phase-synced while blending locomotion clips, otherwise idle and walk fight
  each other.

Clip indices come from named fields on the `CharacterAnimationSet` asset, resolved to indices at bake
time — do not hardcode integers in systems.

---

## Files

**New**
```
Assets/_Pyre/Shaders/Character Skinned.shadergraph
Assets/_Pyre/Materials/Character.mat
Assets/_Pyre/Settings/Hero Animation Set.asset
Assets/_Pyre/Scripts/Animations/Settings/CharacterAnimationSet.cs
Assets/_Pyre/Scripts/Animations/Editor/CharacterAnimationSetEditor.cs
Assets/_Pyre/Scripts/Animations/Components/AnimationLibraryBlob.cs
Assets/_Pyre/Scripts/Animations/Components/AnimationPlayer.cs
Assets/_Pyre/Scripts/Animations/Components/CharacterAnimationState.cs
Assets/_Pyre/Scripts/Animations/Components/SkeletonBone.cs
Assets/_Pyre/Scripts/Animations/Components/SkinBone.cs
Assets/_Pyre/Scripts/Animations/Components/Authoring/CharacterAnimationAuthoring.cs
Assets/_Pyre/Scripts/Animations/Systems/SkeletalAnimationSamplingSystem.cs
Assets/_Pyre/Scripts/Animations/Systems/ComputeSkinMatricesSystem.cs
Assets/_Pyre/Scripts/Animations/Systems/CharacterAnimationStateSystem.cs
Assets/_Pyre/Scripts/Animations/Systems/CharacterAnimationSelectionSystem.cs
```

**Modified**
```
Assets/_Pyre/Models/character-oopi.fbx.meta          (avatarSetup, mesh Read/Write)
Assets/_Pyre/Prefabs/Hero.prefab                     (body-mesh → skinned model instance)
Assets/_Pyre/Scenes/Prototype/Entity Sub Scene.unity (re-bake)
```

Nothing in `Assets/_Pyre/Scripts/Player/` changes — movement stays exactly as it is and animation only
reads from it.

---

## Verification

Each stage has its own checkpoint above; run them in order rather than wiring everything and playing
once. End to end:

1. Open `Assets/_Pyre/Scenes/Prototype.unity`, enter Play mode.
2. Console is clean — in particular no Entities Graphics material/deformation warning.
3. WASD: the character blends idle → walk, speed of the cycle tracks movement speed, and it turns with
   the existing `RotationSpeed` slerp without foot sliding beyond what the clips imply.
4. Walk into the campfire: as `IgnitionProgress.Elapsed` climbs the reaction pose blends in; when
   `Burning` is added the burn state takes over; `FireExtinguishingSystem` returns it to locomotion.
5. *Window → Entities → Hierarchy* on the Hero: bone `LocalTransform`s change per frame and the
   `SkinMatrix` buffer is non-identity.
6. Trigger an explosion (`DebugActionsSystem` has debug keys) and confirm knockback still reads
   correctly with the animated mesh.
7. Profile briefly — `SkeletalAnimationSamplingSystem` is `.Schedule()`d single-threaded, which is
   fine for one character but is the first thing to revisit if the Enemy is animated later.

---

## Risks

* **`git lfs pull` is a hard prerequisite** — the FBX contents could not be inspected from this
  checkout, so the clip names, bone count, and whether the mesh is skinned at all are unverified.
* **Which entity owns `SkinMatrix`** is confirmed empirically at the Stage 1 checkpoint, not assumed.
* **Bind-pose/root-space maths** is the most likely source of a broken-looking mesh; Stage 3's
  checkpoint isolates it from the sampling code.
* **Root motion** in the clips will fight `PlayerMovementSystem` — handled by the `stripRootMotion`
  bake toggle.
* **`optimizeGameObjects` and `optimizeBones`** in the importer both reshape the bone set. Bake
  `SkinBone` from `smr.bones` at bake time (which we do) rather than assuming a hierarchy order.
* **Blend shapes are out of scope.** Adding them later means the Compute Deformation node plus the
  `ENABLE_COMPUTE_DEFORMATIONS` scripting define.
* **Fallback:** if the deformation path proves unworkable in HDRP 17.5 / Entities Graphics 6.5.0, the
  escape hatch is a hybrid presentation layer — entity stays sim-only, a plain GameObject with an
  `Animator` is spawned at runtime and synced from `LocalToWorld`, following the existing
  `AudioBridge` / `CameraBridge` idiom. Stages 0, 1 and 5 carry over; Stages 2–4 are discarded.
