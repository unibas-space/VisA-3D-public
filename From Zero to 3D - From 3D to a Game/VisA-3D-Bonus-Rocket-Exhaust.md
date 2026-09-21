# Bonus — Rocket Exhaust

**Optional extension after Task 1.C · Approximately 15–20 minutes**

Add a short, animated exhaust plume to the rocket in Godot. The plume appears while the engine produces thrust and fades when thrust stops. The rocket can continue orbiting with the engine off.

This extension uses built-in meshes and materials; no additional textures are required. An optional final step adds a separate flame to the Blender showcase render.

## Prerequisites

- Complete Task 1.C of the [main walkthrough](VisA-3D-Walkthrough.md).
- Confirm that **W** activates the existing `thrust` input action and that **Allow Thrust** is enabled on Rocket in the main scene.
- Keep the workshop orientation: the rocket's nose points along its root's local **−Z** axis. The exhaust therefore points along local **+Z**.

The values below assume the original workshop geometry and a Visual scale of `(0.5, 0.5, 0.5)`. Adjust the nozzle position if you changed the model.

## 1. Add the exhaust nodes

1. Open **`res://scenes/rocket/Rocket.tscn`**.
2. Add a **Marker3D** directly under the **Rocket** root and name it **ExhaustPoint**.
3. Set its Position to **`(0, 0, 0.4)`**, Rotation to zero, and Scale to one. Inspect the model and move the marker to the nozzle opening if necessary.
4. Add a **GPUParticles3D** child under ExhaustPoint and name it **Exhaust**. Leave its local Position and Rotation at zero and Scale at one.

| Node path | Type | Purpose |
| --- | --- | --- |
| `Rocket` | CharacterBody3D | Rocket movement and collision |
| `Rocket/Visual` | Node3D | Existing model orientation and scale |
| `Rocket/ExhaustPoint` | Marker3D | Position of the nozzle opening |
| `Rocket/ExhaustPoint/Exhaust` | GPUParticles3D | Animated exhaust particles |

> [!IMPORTANT]
> Place ExhaustPoint directly under Rocket. The existing Visual node has a rotation and scale correction for the imported model; the positions and emission direction in this bonus use the Rocket root's coordinate system.

<!-- Screenshot placeholder: Rocket scene hierarchy and ExhaustPoint positioned at the nozzle opening. -->

## 2. Assign the particle mesh

A newly created GPUParticles3D node can warn that nothing is visible because no mesh is assigned. This is expected: the node needs geometry to draw for each particle.

1. Select **Exhaust**.
2. In the Inspector, expand **Draw Passes**. Keep the number of passes at **1**.
3. Open the resource dropdown beside **Pass 1** and choose **New SphereMesh**.
4. Click the new SphereMesh and set:

| SphereMesh property | Value |
| --- | --- |
| Radius | `0.06` |
| Height | `0.12` |
| Radial Segments | `8` |
| Rings | `4` |

The missing-mesh warning should disappear. The particle system also needs a Process Material, which you create next. **Draw Passes** define the visible geometry; **Process Material** defines particle behaviour. See [Godot's particle setup documentation](https://docs.godotengine.org/en/stable/tutorials/3d/particles/creating_a_3d_particle_system.html).

## 3. Configure the exhaust movement

On **Exhaust**, set the following starting values:

| GPUParticles3D property | Value |
| --- | --- |
| Amount | `48` |
| Lifetime | `0.25` seconds |
| One Shot | Off |
| Explosiveness | `0` |
| Local Coords | On |
| Emitting | On while previewing |

1. In **Process Material**, choose **New ParticleProcessMaterial**.
2. Click the resource to expand its settings. Use the Inspector search if a property is difficult to find.
3. Configure:

| ParticleProcessMaterial property | Value |
| --- | --- |
| Emission Shape | Point |
| Direction | `(0, 0, 1)` |
| Spread | `6°` |
| Initial Velocity Min | `2` |
| Initial Velocity Max | `4` |
| Gravity | `(0, 0, 0)` |
| Scale Min / Max | `1` / `1` |
| Color | White, with full opacity |

Leave other velocity, acceleration, and turbulence settings at their defaults. The particles should move backwards from the nozzle rather than falling down. Their lifetime and speed produce a plume approximately `0.5–1.0` scene units long before accounting for the small spread.

> [!NOTE]
> **Local Coords** makes emitted particles move and rotate with the rocket. This produces an attached plume suitable for the workshop. It is a visual approximation; the effect does not add forces to the orbit simulation. See [GPUParticles3D](https://docs.godotengine.org/en/stable/classes/class_gpuparticles3d.html#class-gpuparticles3d-property-local-coords).

**Checkpoint:** You can see a short stream of particles leaving the nozzle along the rocket's local +Z direction.

## 4. Add colour and fading

### Set the mesh material

1. Expand **Exhaust → Draw Passes → Pass 1 → SphereMesh**.
2. In the mesh's **Material** field, create a **StandardMaterial3D** and open it.
3. Set:

| StandardMaterial3D property | Value |
| --- | --- |
| Albedo Color | White, with full opacity |
| Shading Mode | Unshaded |
| Vertex Color → Use As Albedo | On |
| Transparency | Alpha |

Unshaded rendering keeps the particle colours visible independently of the scene lights. Vertex colours carry the particle colour, and alpha transparency allows particles to fade.

### Set the colour over each particle's lifetime

1. Return to the **ParticleProcessMaterial** resource.
2. Find **Color Ramp** and create a **GradientTexture1D**. Use Color Ramp, which changes colour over time, rather than Color Initial Ramp, which selects a starting colour.
3. Open the texture and its **Gradient** resource, creating a Gradient if the field is empty.
4. Edit the gradient stops:

| Position | Colour | Alpha |
| --- | --- | --- |
| `0.00` | Pale yellow — `#FFF7B0` | `1` |
| `0.55` | Orange — `#FF6A00` | `1` |
| `1.00` | Red-orange — `#FF3300` | `0` |

Use linear interpolation. The first stop describes a newly emitted particle; the final stop describes the end of its lifetime. Set the final stop's alpha to zero so the particle disappears smoothly. See [Godot's colour-ramp explanation](https://docs.godotengine.org/en/stable/tutorials/3d/particles/process_material_properties.html#color).

<!-- Screenshot placeholder: Exhaust preview, SphereMesh material, and colour ramp with a transparent final stop. -->

**Checkpoint:** The plume begins pale yellow, becomes orange, and fades at its end. This mesh-based effect has a deliberately stylized appearance.

## 5. Connect emission to the existing thrust control

**File:** `res://scenes/rocket/RocketOrbit.cs` · **Namespace:** `VisA.OrbitWorkshop` · **Class:** `RocketOrbit`.

1. Open the existing RocketOrbit script.
2. Add this field beside the other exported scene references, inside the class:

```csharp
[Export] private GPUParticles3D _exhaust = null!;
```

3. Add this method inside the same class, beside `_PhysicsProcess`:

```csharp
public override void _Process(double delta)
{
    if (_exhaust == null) return;

    _exhaust.Emitting = IsFlying && _allowThrust && Input.IsActionPressed("thrust");
}
```

The condition uses the existing flight state, thrust setting, and input action. The original `_PhysicsProcess` continues to calculate motion; `_Process` only updates the visual effect.

> [!IMPORTANT]
> Add these members to the existing class. If you already added the exhaust code during the demonstration, keep only one `_exhaust` field and one `_Process` method. If `_Process` contains other work, insert the emission update there instead of replacing that work; use an `if (_exhaust != null)` block so a missing reference does not skip the other updates.

4. Build the C# project in Godot.
5. In **Rocket.tscn**, select the **Rocket root**. Drag its **Exhaust** child from the scene tree into the exported **Exhaust** field in the Inspector.
6. Select the Exhaust particle node and turn **Emitting off** before saving. The script enables it when thrust is active.
7. Save the scene and script. Run **Main.tscn** with **F5**; the rocket's planet and status references are configured in the main scene.

> [!NOTE]
> Stopping emission prevents new particles from appearing. Existing particles finish their lifetime, so the plume can remain visible for up to `0.25` seconds after thrust stops. In this implementation, pause stops new emission and lets the plume fade; it does not freeze the particles. A reset can similarly leave a brief residual plume at the relocated nozzle because Local Coords is enabled.

## 6. Check the result

| Action | Expected result |
| --- | --- |
| Place the rocket before launch | No new exhaust particles |
| Launch with Space, without holding W | Rocket moves with its initial velocity; no new exhaust particles |
| Hold W while flying | Exhaust appears and the existing thrust acceleration changes the trajectory |
| Release W | Exhaust fades; the rocket continues moving |
| Turn with A/D while thrusting | Exhaust follows the rocket's heading |
| Pause with P | New emission stops; existing particles fade |
| Resume with P | Emission follows the current W input again |
| Collide, leave the demo area, or reset with R | New emission stops |
| Disable Allow Thrust on Rocket and run again | W produces neither thrust nor new exhaust particles |

Save `Rocket.tscn` and `RocketOrbit.cs`. The meshes, materials, and gradient can remain embedded resources in the scene; if you save any resource as a separate `.tres` file, keep that file with the project too.

<!-- Screenshot placeholder: Running main scene with W held and the coloured plume behind the rocket. -->

## 7. Optional: add a flame to the Blender showcase render

The runtime effect is created in Godot. For a flame in the Blender render, add a separate mesh to **`showcase.blend`**:

1. Add a **Cone** using **Shift+A → Mesh → Cone**. Start with **8–12 vertices**, Radius 1 about **`0.14`**, Radius 2 **`0`**, and Depth about **`0.8`** for the original, unscaled rocket.
2. Place its wide end at the nozzle and point its tip away from the rocket. In the original Blender model, the nose points along local **+Z**, so the flame points along local **−Z**. Rotate the cone by **180° around X** when working with that original orientation. Match the flame to any rotation and scale used in your showcase composition.
3. Name the object **ExhaustPreview** and give it a material named **FlamePreview**.
4. In **Principled BSDF**, choose an orange Base Color and an orange or yellow **Emission Color**. Start with **Emission Strength `3`** and adjust for the render exposure.
5. To make the flame follow later composition changes, select ExhaustPreview, then Shift-select Rocket last, and use **Ctrl+P → Object (Keep Transform)**.
6. Save `showcase.blend` and render with **F12**.

For a Blender animation, insert Scale keyframes with small changes to the flame's length. Keep the flame as a separate object so it remains easy to edit.

> [!NOTE]
> An emissive surface appears bright, but a surrounding glow is a separate rendering effect. This preview mesh belongs to the showcase scene. Godot's particle system supplies the interactive game effect; a GLB does not contain a ready-to-run Blender fire simulation. See the [glTF specification](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html).

## Troubleshooting

| Problem | Check |
| --- | --- |
| Warning: no mesh assigned; nothing is visible | Assign a SphereMesh to **Exhaust → Draw Passes → Pass 1** |
| Warning about a missing process material | Create a **ParticleProcessMaterial** in Exhaust's Process Material field |
| No particles in the editor | Enable Emitting temporarily, assign both resources, and check that the node is visible |
| Particles fall down | Set the particle material's Gravity to `(0, 0, 0)` |
| Particles remain at the nozzle | Set Initial Velocity Min/Max to `2`/`4` |
| Exhaust points forwards or sideways | Check Direction `(0, 0, 1)`, zero emitter rotation, and the parent hierarchy |
| Particles stay white | Enable **Vertex Color → Use As Albedo** on the SphereMesh material |
| Particles disappear abruptly | Set Transparency to Alpha and the last gradient stop's alpha to zero |
| No exhaust field on Rocket | Build C# successfully, then select the Rocket root again |
| No exhaust while playing | Run Main, launch first, enable Allow Thrust, hold W, and check the exported Exhaust reference |
| Exhaust appears briefly after releasing W or pausing | Existing particles are finishing their `0.25`-second lifetime |

**Bonus complete:** The rocket has a visible engine state, controlled by the same input that produces thrust.
