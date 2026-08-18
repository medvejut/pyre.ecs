# Pyre

**A Unity Entities (ECS) gameplay sandbox: fire that spreads on its own.**

https://github.com/user-attachments/assets/5b19ca34-6420-41fa-ad0c-9d36d91c147d

A learning project, built to explore ECS as gameplay architecture rather than as a performance tool. Hence
no 50,000-cube benchmark — just a couple of dozen dynamic objects and a fire propagation system.

## 🎮 What's implemented

**In ECS**

* **Fire** — heat radius queries against the physics world, per-object ignition time and cooling rate, newly
  lit objects becoming heat sources themselves
* **Explosions** — fuses with audio and visual warnings, chain detonation, falloff impulses, destruction
* **Water** — extinguishing on contact, by collider cast
* **Characters** — physics-based player movement, a knockback channel damped separately from input, and an
  animation state machine for the NPC
* **Skinned mesh animation** — clip baking, pose blending and skin matrix generation, written from scratch
  (see the note below)
* **Tweens** — animations as entities: scale pulse, emissive blink, noise-driven light flicker

**Hybrid bridges.** Entities has no native support for any of this, so systems queue events into singleton
buffers and a thin managed bridge drains them once per frame: positional audio one-shots, looping
`AudioSource`s living on entities, particle spawning, VFX Graph play/stop, camera shake, and a canvas UI
that reads entity queries to place progress icons. Input System actions arrive the same way, in reverse.

## 📁 Structure

```
Assets/_Pyre/Scripts/
├── Gameplay/     fire, ignition, explosions, knockback, destruction   ← the demo itself
├── Player/       input, movement, clip selection
├── UI/           debug ignition/fuse icons
├── Debugging/    my own test harness — nothing to see here
│
├── Skeletons/    clip baking, pose sampling, skin matrices        ┐
├── Animations/   entity-based tweens                              │
├── Audio/        event buffers + managed bridge                   ├─ standalone,
├── Cameras/      camera shake events                              │  no dependency on
├── Effects/      particle playback events                         │  anything else here
└── Transforms/   billboard, frozen world rotation                 ┘
```

The six modules on the right depend on nothing else in the project — they only reference Unity packages, so
they can be lifted into another ECS project as-is. `Gameplay` is the hub that ties them together, and
`Player`, `UI` and `Debugging` build on it.

> ⚠️ **A note on `Skeletons`.** That module was generated end-to-end by Claude and never reviewed line by
> line. It works in the demo and the design is documented in the source comments, but read it before you
> trust it.

Open `Assets/_Pyre/Scenes/Prototype.unity`; gameplay entities live in its `Entity Sub Scene`.

**Stack:** Unity 6000.5.4f1 (HDRP) · DOTS (Entities 6.5) · Input System · ProBuilder · VFX Graph

## 💡 Advice, if you're starting out

**Editor and scenes**
* **Close the sub-scene to preview particles and VFX in Play Mode.** While it's open for editing they
  silently don't play. This one cost me an evening.
* **Keep ProBuilder out of sub-scenes.** Export the blockout to a mesh; otherwise the scene regularly fails
  to load correctly.
* **Turn Domain Reload off.** It's the expected setup for an Entities project.

**Baking and debugging**
* **Declare `DependsOn(asset)` in every baker that reads a ScriptableObject.** Otherwise your edits do not
  re-bake and you debug stale data that looks like a logic bug.
* **Don't try to serialize `IComponentData`.** `UnityObjectRef<T>` isn't serializable, and once one field
  can't round-trip the whole approach falls apart. Author data in ScriptableObjects and bake it.
* **Name your entities and read the names back** with `EntityManager.GetName(entity)` when debugging. A bare
  entity index in a log line tells you nothing.

**Learning the package**
* **Be careful with gen-AI here.** The package changed a lot across versions and there is little material to
  learn from, so ChatGPT and Gemini keep producing code against APIs that no longer exist. Claude coped
  noticeably better, presumably by reading the package sources.
* **Read the [official documentation](https://docs.unity3d.com/Packages/com.unity.entities@6.6/index.html).**
  It is genuinely good and was more useful than any secondary source.

## 🙏 Credits

3D models, textures and icons by [Kenney](https://kenney.nl/) (CC0).  
Sound effects generated with [ElevenLabs](https://elevenlabs.io/).
