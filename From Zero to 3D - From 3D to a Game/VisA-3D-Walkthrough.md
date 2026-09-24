# From Zero to 3D — From 3D to a Game

**Visual Analytics seminar · Student walkthrough · Revised 21 September 2026**

Create a planet and a rocket in Blender, render them together, and build an interactive orbit experiment in Godot. You will choose the rocket's starting position and velocity, observe its trajectory, and change it with thrust.

| Activity | Result |
| --- | --- |
|  Introduction and target demonstration | Understand the asset-to-game workflow |
|  Task 1.A — Blender | Models, materials, rendered image, and GLB exports |
|  Task 1.B — Godot | Game scene, lighting, sky, and collision shapes |
|  Task 1.C — Simulation | Launch, gravity, steering, thrust, and collision |
|  Save and discuss | Explain one observed trajectory |

> [!NOTE]
> Complete installation and project setup before class. The two-hour schedule assumes a guided walkthrough and copy-paste code. Detailed styling and the optional VOXON demonstration can continue afterwards.

This is the complete student walkthrough. Each figure accompanies the action or result it illustrates. Screenshots document a rehearsal; captions identify relevant differences from the current recipe. The [image notes and index](doc-assets/README.md) distinguish design references, diagrams, and earlier captures.

## Contents

- [Before the workshop](#before-the-workshop)
- [Task 1.A — Create, render, and export the models in Blender](#task-1a--create-render-and-export-the-models-in-blender)
- [Task 1.B — Prepare the physical scenes in Godot](#task-1b--prepare-the-physical-scenes-in-godot)
- [Task 1.C — Add control logic and simulate the orbit](#task-1c--add-control-logic-and-simulate-the-orbit)
- [Save your work and optionally submit](#save-your-work-and-optionally-submit)
- [Assets and material recipes](#assets-and-material-recipes)
- [Bonus — Rocket exhaust](VisA-3D-Bonus-Rocket-Exhaust.md)
- [Optional extension — VOXON](#optional-extension--voxon)

---

## Before the workshop

### Required software

| Tool | Workshop target | Check before class |
| --- | --- | --- |
| Blender | 5.2.x | Starts and can save a `.blend` file |
| Godot | 4.7 **.NET edition** | C# is available when attaching a script |
| .NET SDK | 10, matching your CPU architecture | `dotnet --list-sdks` includes a 10.x SDK |
| JetBrains Rider | A current version with Godot support | Opens the Godot C# project |
| Git and a GitHub account | Needed for the optional fork-and-pull-request submission | Can clone your fork and push a branch |
| Three-button mouse | Recommended | Middle-button navigation works |

> [!NOTE]
> The supplied screenshots were captured with **Blender 5.2.1** and **Godot 4.6.3 .NET**. Godot 4.7 .NET remains the target specified for the workshop; use the release announced for the class and complete the setup check below with that release.

Use the [.NET edition of Godot](https://godotengine.org/download/) for the release announced in class and install the [.NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) separately. A runtime-only installation cannot compile the scripts. BlenderKit's free tier is optional: the required atlas and sky are already supplied.

### Get the repository and create your working folders

The [VisA-3D-public repository](https://github.com/unibas-space/VisA-3D-public) contains both the workshop materials and the **Students-works/** directory. Use the same repository layout throughout the workshop.

| Path from the repository root | Purpose |
| --- | --- |
| `From Zero to 3D - From 3D to a Game/` | Walkthrough, supplied assets, reference code, and tools |
| `Students-works/firstname-lastname/` | Your Blender files and Godot project |
| `README.md` | Repository overview and voluntary submission rules |

Replace **firstname-lastname** with your own name or a nickname, using hyphens and no spaces. Use the same folder name in every path below. Create a new folder for your work; leave other students' folders unchanged.

If you want to submit your work, click **Fork** on GitHub, then clone **your fork**. Replace **YOUR-GITHUB-USERNAME** in the URL:

```bash
git clone https://github.com/YOUR-GITHUB-USERNAME/VisA-3D-public.git
cd VisA-3D-public
git switch -c workshop-submission
```

> [!NOTE]
> Submission is voluntary, but fun.

> [!WARNING]
> A public fork and any submitted work are publicly visible. One can also use a nickname for your folder.

Inside **Students-works/firstname-lastname/**, create **blender/** and **godot/**. Copy the entire **assets/** directory from **From Zero to 3D - From 3D to a Game/** into your **blender/** folder. Create **exports/** and **renders/** there as well.

| Path inside `Students-works/firstname-lastname/` | Contents |
| --- | --- |
| `blender/assets/` | Supplied textures, sky, and material presets; later your baked planet image |
| `blender/planet.blend` | Planet geometry and material |
| `blender/rocket.blend` | Editable rocket parts and a separate export mesh |
| `blender/showcase.blend` | Composition, camera, lighting, and sky |
| `blender/exports/` | Exported GLB files |
| `blender/renders/` | Rendered PNG |
| `godot/` | Complete Godot project, including its own copy of imported assets |

> [!IMPORTANT]
> **Your Blender folder** means `Students-works/firstname-lastname/blender/`; **your Godot folder** means `Students-works/firstname-lastname/godot/`. In Blender steps, `assets/...` is relative to your Blender folder. In Godot, `res://` is relative to your Godot folder.
> Paths such as `code/RocketOrbit.cs` and `tools/create_planet_material.py` refer to supplied files inside the workshop directory.
> Do not place your blender folder inside your Godot folder.

### Create the Godot project and check C#

Create the project you will use throughout the workshop. You will return to the same project after modelling.

1. Open the **.NET edition** of Godot and choose **Create**. Name the project **VisA-firstname-lastname**.
2. Set Project Path to your existing **`Students-works/firstname-lastname/godot`** folder inside your local repository copy. Disable automatic folder creation if it would add another nested folder.
3. Choose **Forward+**. Set **Version Control Metadata to None**; Git is managed at the repository root, and we add the project exclusions in the final saving step.
4. Click **Create & Edit**.

![Godot Create New Project dialog with Forward Plus selected](doc-assets/setup/godot-create-project.png)

*Project creation dialog: choose Forward+. Set Project Path to your own Students-works folder and Version Control Metadata to None; the captured dialog still uses an earlier path and Git metadata.*

> [!TIP]
> If Forward+ does not start on your laptop, use Compatibility for the same project and tell the instructor. Keep Forward+ when the setup check works.

5. Create a scene using **Other Node → Node** and name the root **SetupCheck**. Save it as `res://setup_check.tscn`.
6. Select the root and choose **Attach Script**. Set Language to **C#**, keep Inherits as **Node**, and set the file path to **`res://SetupCheck.cs`**. Godot creates the C# project and solution.

![Attach Node Script dialog with C sharp selected](doc-assets/setup/godot-attach-csharp-script.png)

*The Attach Script dialog offers C# and Node inheritance. Replace the displayed Node.cs filename with SetupCheck.cs before creating the script.*

7. Replace the generated script with the following:

```csharp
using Godot;

public partial class SetupCheck : Node
{
    public override void _Ready()
    {
        GD.Print("C# workshop setup works.");
    }
}
```

8. Build the project and run the current scene with **F6**. Confirm the message in the Output panel.
9. In **Editor → Editor Settings → Dotnet → Editor**, set **External Editor** to **JetBrains Rider** and open the script.
10. Keep this project for the workshop. Resolve any package restore or missing-runtime error now; use the runtime named in the error if the generated project targets an older version.

![Godot Output panel confirming the C sharp setup check](doc-assets/setup/godot-csharp-setup-output.png)

*The Output panel contains “C# workshop setup works.” The captured editor is Godot 4.6.3 .NET.*

> [!IMPORTANT]
> A successful build and run is the setup check. Rider alone does not provide the .NET SDK. Godot's exported C# fields become visible after a successful build. See [Godot's C# setup documentation](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html).

### Editor conventions

The walkthrough uses English UI labels and default shortcuts. Blender shortcuts apply to the editor under the pointer. Decimal values use a dot. Without a numpad, use the viewport axis gizmo or **View → Viewpoint**. Save regularly with **Ctrl+S**.

---

## Task 1.A — Create, render, and export the models in Blender

**Time target: approximately 55 minutes.** You need Blender and the copied assets in your named Blender folder. Your outputs are two models, a separate composition scene, a rendered PNG, and two GLB files.

### A1. Learn the controls we need

Start with **File → New → General**.

| Action | Command / shortcut |
| --- | --- |
| Select / select all | Left-click / A |
| Orbit / pan / zoom | Middle mouse / Shift + middle mouse / mouse wheel |
| Move / rotate / scale | G / R / S |
| Constrain a transform | X, Y, or Z after G, R, or S |
| Confirm / cancel | Enter / Esc |
| Toggle Object Mode / Edit Mode | Tab |
| Add an object or a shader node | Shift+A in the relevant editor |
| Search a command | F3, then type its name |
| Apply object transforms | Ctrl+A in Object Mode, then choose the transform |
| Duplicate selected objects | Shift+D |
| Open the sidebar | N |
| Frame the selected object | Numpad decimal, or View → Frame Selected |

Object Mode changes complete objects. Edit Mode changes their vertices, edges, and faces. A **mesh** defines the shape; a **material** defines how its surface interacts with light; an image **texture** stores values such as color; **UV coordinates** tell the material where to read that image.

> [!TIP]
> Global axes belong to the scene; local axes follow an object's orientation. With the default Global transform orientation, **G, Z** moves along global Z, while **G, Z, Z** moves along the object's local Z. The same distinction applies to rotation and scale.

### A2. Create the planet

Create a sphere, then give it a procedural colour pattern. You can vary the palette and patch sizes; keep the radius and origin consistent so the later physics setup matches.

#### Model the sphere

1. In the 3D viewport, press **A** to select the default objects, then **X → Delete**.
2. Use **Shift+A → Mesh → UV Sphere**. Immediately open **Adjust Last Operation** with **F9**, or expand the panel at the bottom-left.
3. Set **Segments 32**, **Ring Count 16**, and **Radius 2**. Keep Location `(0, 0, 0)`.
4. Rename the object **Planet** in the Outliner. Right-click → **Shade Smooth**.
5. In Object Mode, use **Ctrl+A → Scale**: this is the **Apply Scale** command. Press **N** and open **Item** to check Scale `(1, 1, 1)` and Dimensions approximately `(4, 4, 4)`.
6. Save as **`Students-works/firstname-lastname/blender/planet.blend`**.

![Blender UV sphere with dimensions four and unit scale](doc-assets/blender/planet-sphere-dimensions.png)

*The Item panel shows dimensions of 4 on each axis and scale 1. Name your sphere Planet; the capture still uses Sphere.*

> [!NOTE]
> Applying scale keeps the visible size but records it in the mesh, leaving object scale at one. Shade Smooth changes the interpolated surface shading; it does not add polygons. The UV Sphere also comes with a UV map for the later bake.

#### Create and connect the material

1. Select Planet and open the **Shading** workspace.
2. In the Shader Editor header, keep the context at **Object** and click **New**. If a material already exists, use it. Rename it **PlanetSurface** in the material name field.
3. Keep the **Principled BSDF** and **Material Output** nodes and their existing connection.

![New Blender material with Principled BSDF and Material Output](doc-assets/blender/planet-new-material.png)

*The starting material has Principled BSDF connected to Material Output. This is the graph before adding the procedural nodes or changing roughness.*

4. With the pointer over the Shader Editor, press **Shift+A**, choose **Search**, and type **Texture Coordinate**. Click to place the node. Repeat for **Noise Texture** and **Color Ramp**. F3 also opens command search.
5. Drag from each output socket to the corresponding input:

| Output | Input | Meaning |
| --- | --- | --- |
| Texture Coordinate: **Generated** | Noise Texture: **Vector** | Read a position within the object's bounds |
| Noise Texture: **Factor** / **Fac** | Color Ramp: **Factor** / **Fac** | Convert the noise value to a chosen color |
| Color Ramp: **Color** | Principled BSDF: **Base Color** | Use that color on the surface |
| Principled BSDF: **BSDF** | Material Output: **Surface** | Send the material to the renderer |

![Schematic of the Factor-based procedural planet material](doc-assets/diagrams/planet-factor-material-flow.svg)

*Wiring reference for this recipe: Generated coordinates → Noise Factor → Color Ramp → Principled Base Color → Material Output. This is a diagram, not a screenshot.*

6. Set Noise to **3D**, Scale **3.0**, Detail **2.0**, Roughness **0.6**, Distortion **0**. Keep the other defaults.
7. In the Color Ramp, keep **Linear** interpolation. Select the left stop, set Position **0.47**, click its color field, and enter **`#1F5E9C`**. Select the right stop and set Position **0.53**, color **`#66B36D`**. If Blender expects eight hex digits, append `FF` for full opacity.
8. On Principled BSDF, set **Metallic 0**, **Roughness 0.85**, and **Alpha 1**.
9. Choose **Material Preview** using the viewport shading buttons at the top-right. Save.

> [!IMPORTANT]
> Use Noise Texture’s **gray Factor output** for this recipe. The blue-and-green preview in the optional variation below uses the Color output; its close-up is retained in the [image notes](doc-assets/README.md#procedural-material-variation). Follow the diagram above when wiring your graph.

Think of the graph as a sequence of functions: coordinates become a numerical pattern, the ramp maps numbers to colors, and the shader determines the visible surface. Roughness controls how broad or sharp reflections appear.

#### Make it your own

Choose different ramp colors or Noise Scale. These four starting points use the same graph and stop positions:

| Variant | Noise Scale | Left stop | Right stop |
| --- | --- | --- | --- |
| Ocean | 3.0 | `#1F5E9C` | `#66B36D` |
| Rust | 4.5 | `#8F392F` | `#E9B979` |
| Ice | 2.8 | `#466A92` | `#D9F2EE` |
| Candy | 3.8 | `#68418F` | `#EE9CAF` |

![Illustrated solar system used as colour and style inspiration](doc-assets/reference/colored-pencil-planet-inspiration.png)

*Design inspiration for colours and surface patterns. These detailed illustrated planets are not the output of the two-colour noise recipe.*

> [!TIP]
> The reference illustration is inspiration, not a requirement to reproduce rings, craters, or detailed continents. Your two-color procedural planet is sufficient. Keep radius **2** and the origin at `(0, 0, 0)`.

#### Recorded material variation (optional)

The rehearsal used Noise Texture’s yellow Color output instead of its gray Factor output. The image below belongs to that variation; it illustrates the surface style, while the diagram above defines the main recipe.

![Blue and green procedural planet in Blender Material Preview](doc-assets/blender/planet-procedural-material-preview.png)

*A completed blue-and-green material from the rehearsal. This capture uses the Color-output variation described in the image notes; your Factor-based recipe can produce a different patch pattern. See the [material-variation note](doc-assets/README.md#procedural-material-variation).*

**Checkpoint:** A colored planet is saved in `planet.blend`.

### A3. Build the rocket in a separate file

Use the illustration as a design direction. Our version uses a cylinder, cone, nozzle, and **three fins distributed around the body**.

![Illustrated rocket used as a design reference](doc-assets/reference/colored-pencil-rocket-inspiration.png)

*Design inspiration: a pointed nose, cylindrical body, nozzle, and fins. The workshop model uses simpler geometry and your own colour choices.*

#### Create the main parts and one fin

1. Save the planet, then choose **File → New → General** and delete the default objects.
2. Use **Shift+S → Cursor to World Origin**. Rename the default collection **RocketSource**.
3. Add the following parts. Enter cylinder/cone parameters with **F9 immediately after adding**, and locations/dimensions in the **N → Item** panel. The rocket points along Blender's **+Z** axis.

| Part | Primitive | Settings | Location |
| --- | --- | --- | --- |
| `Body` | Cylinder | 16 vertices; radius **0.30**; depth **1.20** | `(0, 0, 0)` |
| `Nose` | Cone | 16 vertices; Radius 1 **0.30**; Radius 2 **0**; depth **0.50** | `(0, 0, 0.85)` |
| `Nozzle` | Cylinder | 12 vertices; radius **0.18**; depth **0.20** | `(0, 0, -0.70)` |
| `Fin` | Cube | Dimensions **`(0.40, 0.10, 0.55)`** | `(0.38, 0, -0.38)` |

![Rocket cylinder cone nozzle and one rectangular fin](doc-assets/blender/rocket-primitives-and-unshaped-fin.png)

*Initial blockout: body, nose, nozzle, and a rectangular fin. The fin has not yet been shaped or repeated.*

4. Select all mesh parts and use **Ctrl+A → Scale**. Each should now have Scale `(1, 1, 1)`.

![Blender Apply menu with Scale highlighted](doc-assets/blender/apply-object-scale-menu.png)

*Use Apply Scale in Object Mode before the bevel and array steps. This menu capture shows the command, not the resulting transforms.*

> [!TIP]
> **Precise placement:** use the numeric locations above first. For free modelling, the magnet in the viewport header enables snapping; its dropdown selects targets such as Vertex, Edge, or Face. With snapping off, hold **Ctrl while moving with G** to enable it temporarily. Object Mode snaps a reference point of the object, so use **Edit Mode vertex snapping** when you need a specific vertex to meet another vertex. Turn snapping off again for the numerical recipe.

#### Shape and bevel the fin in Edit Mode

1. Select only Fin. Press **Tab** for Edit Mode, **1** for Vertex Select, and **Numpad 1** for Front View. Enable **X-Ray with Alt+Z** so a box selection reaches both sides of the thin fin.
2. Deselect with **Alt+A**. Use **B** to box-select the two vertices at the **outer upper corner**: the rightmost top corner in this view, including front and back vertices.
3. Press **G, X, -0.20, Enter** to pull that corner towards the body. The fin now has a sloping outer edge.
4. Press **2** for Edge Select and **A** to select all edges. Press **Ctrl+B** for **Bevel**, enter **0.01**, and confirm. Use one or two segments; the mouse wheel adjusts the segment count during the operation.
5. Turn X-Ray off and return to Object Mode with Tab.

> [!NOTE]
> **Ctrl+B in Edit Mode changes the mesh.** A Bevel modifier keeps the bevel adjustable. We use the first on the fin to practise mesh editing and the second on the body below.

#### Make three fins with an Array modifier

The array will rotate copies around the rocket's center, not around the fin's own center.

1. With Fin selected in Object Mode, confirm the 3D cursor is at the world origin. Use **Object → Set Origin → Origin to 3D Cursor**. The visible fin stays in place, but its origin moves to the rocket center.
2. Add **Shift+A → Empty → Plain Axes** at the world origin. Name it **FinRotation** and set its **Z rotation to 120°**. Keep Location zero and Scale one.
3. Select Fin. Open the **Modifiers** tab, choose **Add Modifier**, and search for **Array**.
4. Set Count to **3**. Disable **Relative Offset** and **Constant Offset**. Enable **Object Offset** and select **FinRotation** as the offset object.
5. Inspect from above: the fins should be separated by **120°**. Keep the modifier unapplied in the source model.

![Top-view diagram of three fins rotated about one shared origin](doc-assets/diagrams/rocket-three-fin-array.svg)

*Array diagram: the original fin and two copies are spaced by 120°. The fin origin and FinRotation Empty share the rocket centre.*

> [!TIP]
> If the copies form a spiral or change size, check the fin and Empty origins, scale, and offset settings. Only the Empty's Z rotation should provide the repeated transform.

#### Add a small amount of surface detail

1. Select Body and add a **Bevel modifier**. Use Amount **0.02**, Segments **2**, and Limit Method **Angle** with approximately **30°**. This softens the end rims while retaining the simple cylinder.
2. Optionally add a porthole: a UV Sphere with radius **0.16**, located at `(0, -0.29, 0.20)`, and scaled to `(1, 0.25, 1)`. Apply its scale and name it **Window**.
3. Keep the other details simple and save as **`Students-works/firstname-lastname/blender/rocket.blend`**.

![Untextured rocket with shaped fins](doc-assets/blender/rocket-untextured-fins.png)

*The rocket silhouette after shaping and repeating the fins, before texturing. The image shows the result; the array settings are explained separately.*

**Checkpoint:** The source collection contains separate, editable parts and the fin array. The reference screenshot shows the general silhouette; your bevels and fin shape can differ.

### A4. Apply the pencil texture and prepare an export mesh

#### Create one shared material

1. Select Body and open **Shading**. Click **New** to create **RocketPencil**.
2. Add an **Image Texture** node and open your local **`assets/textures/rocket-pencil-atlas.png`**.
3. Keep Color Space at **sRGB**. Connect **Image Texture Color → Principled Base Color**. Set Metallic **0**, Roughness **0.85**, Alpha **1**.
4. For Nose, Nozzle, Fin, and the optional Window, choose **RocketPencil** from the existing-material dropdown beside **New**. Reuse the material rather than creating a copy for each part.

![Rocket atlas image connected to Principled Base Color](doc-assets/blender/rocket-atlas-material-nodes.png)

*The atlas supplies Base Color and the material roughness is 0.85. The mixed colours on the small rocket preview show why the UV placement step is still needed.*

#### Move each part's UVs into a color

UVs are positions in the image. We use the atlas as a collection of colored pencil surfaces; choose any color for each part.

![Supplied rocket pencil texture atlas](assets/textures/rocket-pencil-atlas.png)

*Use UV placement to choose a patch. There are no prescribed color assignments or exact UV-center coordinates.*

1. Select a part in Object Mode and switch to **UV Editing**.
2. With the pointer over the **3D viewport**, press Tab, then A. Use **U → Smart UV Project** and confirm.
3. In the **UV Editor**, choose `rocket-pencil-atlas.png` from the image dropdown. Press A to select all of the part's UVs. Keep the pivot at **Median Point**.
4. Press **S**, type **0.2**, and press Enter. This shrinks all selected UVs together.
5. Press **G** and move the UVs visually into your chosen color patch. Left-click or Enter confirms. Use G again to adjust, or S to make the group smaller.
6. Check that **all islands are inside one patch with a clear margin**. Do not place them across a boundary between colors.
7. Return to Object Mode in the 3D viewport and repeat for the next part. The Array copies inherit the original fin's UVs.

> [!TIP]
> No exact UV coordinates are needed. If every color appears on one part, its UVs still cover the whole atlas. If another part's UVs move too, return to Object Mode and select only the intended part before entering Edit Mode.

![Rocket with red body blue nose green fins and yellow window](doc-assets/blender/rocket-textured-uv-result.png)

*Example after placing each part’s UV islands within a chosen atlas patch. The visible window is optional; your colours can differ.*

#### Keep the source editable and create a separate export object

Keep both representations in **the same `rocket.blend`**: `RocketSource` contains the editable parts; `RocketExport` contains the joined snapshot used for rendering and export.

1. Save the file. In RocketSource, select **only the mesh parts**, including Fin and the optional Window. Do not select FinRotation.
2. Press **Shift+D** to duplicate, then **right-click** to cancel movement while keeping the duplicates.
3. With the duplicates still selected, press **M → New Collection** and name it **RocketExport**.
4. In Object Mode, use **Object → Convert → Mesh**, or F3 and search **Convert Mesh**. This evaluates modifiers on the duplicates, including the fin array and bevel, and turns the result into ordinary mesh geometry.
5. Select all duplicate meshes, with the duplicate Body selected last as the active object. Press **Ctrl+J — Join**. Rename the result **Rocket**.
6. Use **Shift+S → Cursor to World Origin**, then **Object → Set Origin → Origin to 3D Cursor**. Confirm Rocket Location zero, Rotation zero, and Scale one.
![Joined Rocket object with zero location rotation and unit scale](doc-assets/blender/rocket-joined-export-transforms.png)

*The joined export mesh is named Rocket, with zero location/rotation and unit scale. This earlier capture shows only the export object; keep the editable RocketSource collection in your own file.*

7. In the Outliner, hide **RocketSource** in both the viewport and render. Use the eye/monitor and camera restriction toggles; expose them through the Outliner's filter menu if needed. Keep RocketExport visible.
8. Use **File → External Data → Make All Paths Relative**, then save `rocket.blend`.

> [!IMPORTANT]
> Convert and join **the duplicates**. Keep the original parts, the Array modifier, and FinRotation in RocketSource. When you change the source later, replace the old export snapshot by repeating these steps; it does not update automatically. Hide only after converting, while the Array's Empty is still available.

**Checkpoint:** The file retains editable parts and one final mesh named Rocket. Only the final mesh will be exported.

### A5. Compose and render a third Blender scene

#### Append the two assets

1. Save the rocket, start **File → New → General**, and delete the default objects.
2. Choose **File → Append**, open your `planet.blend`, enter **Object**, and select **Planet**.
3. Append **Rocket** from `rocket.blend`. Select the joined export object, not the source collection or individual parts.
4. Arrange them freely. Starting positions are Planet `(-1.8, 0, 0)` and Rocket `(2.2, 0, 0)`. You can rotate or scale the copies for the composition.
5. Save as **`Students-works/firstname-lastname/blender/showcase.blend`**.

> [!NOTE]
> Append copies data into this scene. The source files retain their original scale, orientation, and origins for the game. Later source edits do not automatically update an appended copy.

#### Add the sky

1. Open **Shading** and switch the Shader Editor context from **Object** to **World**. Enable **Use Nodes** if necessary.

![Blender Shader Editor switched to World context](doc-assets/blender/showcase-world-shader-context.png)

*World context exposes Background and World Output. The environment image has not yet been connected.*

2. Add an **Environment Texture** and open **`assets/sky/space-panorama.png`**. Use Equirectangular projection and sRGB.
3. Connect **Environment Texture Color → Background Color**, then **Background → World Output Surface**. Set Background Strength to **0.5**.

![Environment Texture connected to Background and World Output](doc-assets/blender/showcase-environment-node-connections.png)

*Connection reference: Environment Texture → Background → World Output, with strength 0.5. The captured node uses another sky image; load the supplied space-panorama.png.*

![Supplied star background panorama](assets/sky/space-panorama.png)

*The supplied panorama is a background rather than an HDR lighting source. Use the scene lights for the models.*

#### Add the camera and light

1. Add a **Camera** at `(0, -14, 7)`, Rotation `(63.435°, 0°, 0°)`. In Camera Data, choose **Perspective** and a focal length of **50 mm**.
2. Add an **Area Light** at `(0, -5, 7)`, Rotation `(35°, 0°, 0°)`, Power **1500 W**, Size **5**. The dark sky image is mainly a background; this light makes the models readable.
3. Choose **EEVEE** as the render engine. Set output resolution to **1280 × 720**, **100%**.
4. Enter Camera View using **Numpad 0** or **View → Cameras → Active Camera**. Adjust the framing until both assets fit.

> [!TIP]
> **Frame through the viewport:** navigate to a view you like, then press **Ctrl+Alt+Numpad 0** to align the active camera to it. Alternatively, enter Camera View, open **N → View**, and enable **Lock Camera to View**; navigate normally, then disable the lock when finished. With the camera selected, **G, Z, Z** moves it along its own viewing axis.

> [!NOTE]
> **Why use Perspective here?** An orthographic camera uses parallel viewing rays. An environment map can therefore appear uniform because those rays sample the same direction. The perspective camera shows different directions across the frame and makes the star background visible. This is separate from the orthographic gameplay camera used later in Godot.

#### Render and save

1. Disable **Render Properties → Film → Transparent** so the world background appears.
2. If checking the sky in Material Preview, enable **Scene World** in the shading dropdown; otherwise Blender can show its preview environment instead. **Rendered** viewport shading uses the scene settings.
3. Press **F12**. In Render Result, choose **Image → Save As** and save **`renders/showcase.png`**.
4. Make external paths relative and save `showcase.blend`.

![Rendered planet and rocket against a star background](doc-assets/blender/showcase-planet-rocket-render.png)

*Example of the completed composition. This rehearsal render uses a different star image and arrangement; your render should show both assets against your chosen sky.*

> [!TIP]
> **Optional Cycles render:** in **Edit → Preferences → System → Cycles Render Devices**, select a supported GPU backend, such as **OptiX** for an NVIDIA RTX card, and enable the GPU. Then choose **Cycles → Device: GPU Compute** in Render Properties. Start with **64 samples and denoising**. EEVEE already renders on the GPU and does not use this Cycles device selector. CPU is sufficient for the small color bake below.

### A6. See why the planet material needs baking

Open your original **`planet.blend`**, leaving its procedural graph connected for the first comparison.

#### First import: geometry without the procedural pattern

1. Select only Planet in Object Mode. Choose **File → Export → glTF 2.0**, format **glTF Binary (`.glb`)**, with **Selected Objects**, materials, UVs, and normals enabled. Keep **+Y Up**.
2. Save **`exports/planet-unbaked.glb`**.
3. Open the **existing VisA Godot project** from setup. Create `res://assets/tests/` and copy this file into it using the file explorer.
4. Wait for import, then double-click the GLB to open the advanced import preview. The sphere geometry is present, but the Blender noise/color-ramp pattern is not reproduced; it may appear as a uniform fallback color.
5. Close the preview and return to Blender. Keep this comparison asset separate from the final `planet.glb`.

> [!IMPORTANT]
> glTF does not transfer an arbitrary Blender shader graph for Godot to execute. A base-color image connected to Principled BSDF is portable. Baking records the procedural color into that image.

#### Bake only the base color

1. Select Planet, switch the render engine to **Cycles**, and use CPU or your configured GPU.
2. Confirm a UV map exists under **Object Data Properties → UV Maps**.

![UVMap entry in Blender mesh data properties](doc-assets/blender/planet-existing-uv-map.png)

*The UV Sphere already has a UVMap entry. This confirms that a UV map exists; it does not show the unwrap or the bake result.*

3. In the Shader Editor, add an **Image Texture** node. Click **New**, name the image **PlanetBaseColor**, and choose **1024 × 1024**, normal 8-bit color, sRGB.
4. Leave this destination node **unconnected**. Keep Noise → Color Ramp → Principled Base Color connected while baking.
5. Click the new image node to make it **active**, select only Planet, and remain in Object Mode.
6. In **Render Properties → Bake**, choose **Diffuse**. Under Influence / Contributions, enable **Color** and disable **Direct** and **Indirect**. Leave **Selected to Active** off and use a **16 px margin**.

![Cycles bake settings with Diffuse and Color contribution selected](doc-assets/blender/planet-diffuse-color-only-bake.png)

*Cycles bake settings: Diffuse with Color enabled and Direct/Indirect disabled. The capture uses CPU and a 16 px margin.*

7. Click **Bake**. The result should contain the colored pattern without baked scene lighting or shadows.
8. In the Image Editor, select PlanetBaseColor and choose **Image → Save As**. Save **`assets/textures/planet-basecolor.png`** in your Blender folder.

![Blender Image menu over a baked planet texture](doc-assets/blender/planet-save-baked-image-menu.png)

*The baked colour image is visible in the Image Editor. Use Image → Save As to write your planet-basecolor.png file.*

9. Now connect **PlanetBaseColor Color → Principled Base Color**, replacing the ramp's final connection. Keep the procedural nodes in the graph for later edits. Roughness remains **0.85**, Metallic **0**.
10. Save `planet.blend`.

![Baked PlanetBaseColor image connected to Principled Base Color](doc-assets/blender/planet-baked-image-base-color.png)

*After baking, PlanetBaseColor supplies Base Color. The original procedural branch remains in the graph but is disconnected from the shader.*

> [!IMPORTANT]
> Saving the `.blend` does not replace saving the generated image externally. To rebake a changed palette, reconnect the procedural ramp to Base Color first, leave the destination image unconnected but active, bake, save the PNG, then reconnect the image.

The rocket already uses an ordinary image texture. It does not need another color bake.

### A7. Clean up, export, and check the result in Godot

#### Keep the files portable

1. Save all modified Blender files and generated images.
2. In each `.blend`, choose **File → External Data → Make All Paths Relative**. A path such as **`//assets/textures/planet-basecolor.png`** is relative to that `.blend` file's folder; it will still work when the entire named Blender folder is moved to another computer.
3. Use **File → External Data → Report Missing Files** to check for unresolved image dependencies. Keep the copied `assets/` directory beside the `.blend` files.
4. In `rocket.blend`, keep RocketSource hidden in the viewport and render, and select only the joined Rocket object. Leave the editable source collection intact.

> [!TIP]
> **Pack Resources** can embed supported external resources into a `.blend` for a self-contained checkpoint. The workshop uses explicit relative image files so you can see and commit the dependencies. Moving only the `.blend` without its assets can break those paths.

#### Export the final models

For Planet in `planet.blend` and Rocket in `rocket.blend`:

1. Select only the final asset mesh in Object Mode. Check origin zero, rotation zero, unit scale, UVs, and the image-based material.
2. Choose **File → Export → glTF 2.0**.

![Blender File Export menu with glTF 2.0 highlighted](doc-assets/blender/export-gltf-menu.png)

*Open the glTF 2.0 exporter here. Choose GLB and Selected Objects in the export dialog that opens next; those settings are not shown in this capture.*

3. Choose **glTF Binary (`.glb`)** and enable **Selected Objects**. Export materials, UVs, and normals. Keep the default **+Y Up** conversion. Use the normal embedded-image GLB export, not a separate-texture option.
4. Save **`exports/planet.glb`** and **`exports/rocket.glb`**.
5. Copy the final files into **`Students-works/firstname-lastname/godot/assets/models/`**, creating the folder if necessary. Copy the sky into **`Students-works/firstname-lastname/godot/assets/sky/space-panorama.png`**.
6. Return to Godot and open each GLB's import preview. Check both geometry and texture. Compare the final planet with `planet-unbaked.glb` from A6.

![Godot advanced import preview showing the baked planet texture](doc-assets/godot/import-preview-baked-planet.png)

*Successful final planet import: the coloured surface pattern is visible. This is the baked asset, not the earlier unbaked comparison.*

![Godot advanced import preview showing the textured rocket standing upright](doc-assets/godot/import-preview-textured-rocket.png)

*Successful rocket import: geometry and atlas colours are present. The imported rocket points along +Y; the wrapper scene will rotate its Visual child.*

> [!NOTE]
> Blender uses Z-up; glTF and Godot use Y-up. The exported rocket therefore arrives pointing along +Y. We will rotate its **visual child** in Godot to face -Z. Do not add another manual rotation during export.

**Task 1.A complete:** You have three Blender files, a rendered PNG, and two final GLBs. The final GLBs are already imported into your Godot project.

#### Troubleshooting Task 1.A

| Symptom | Check |
| --- | --- |
| Pink material | Missing image; reopen it from the local assets folder and make paths relative |
| Every atlas color appears on one part | Shrink its UVs and move all islands inside one patch |
| Fin copies move away or change size | Fin/Empty share the origin; scale is one; Relative/Constant Offset are off |
| Duplicate rocket surfaces or unexpected shadows | Hide RocketSource in both viewport and render; keep only RocketExport visible |
| Plain sky in the Blender render | Use Perspective; check Film Transparent is off and the World nodes are connected |
| Preview sky differs from render | Enable Scene World in Material Preview, or use Rendered shading |
| Black bake or no destination | Cycles; Planet selected; active destination image; procedural source still connected |
| Bake contains shadows | Diffuse contributions must include Color only |
| Final planet loses its pattern in Godot | Saved image must feed Principled Base Color before final export |
| GLB contains extra parts | Export from the individual source file with only the final mesh selected |

---

## Task 1.B — Prepare the physical scenes in Godot

**Time target: approximately 25 minutes.** Continue in the **VisA-firstname-lastname** project created during setup. The final GLBs and sky were copied into it at the end of Task 1.A.

### B1. Set up the game area

#### Create Main and check the imported files

1. Create a **3D Scene**, rename its `Node3D` root to **Main**, and save it as **`res://scenes/main/Main.tscn`**. Leave the root transform at identity.
2. Confirm the files in the FileSystem panel:

| Godot path | Content |
| --- | --- |
| `res://assets/models/planet.glb` | Final planet with baked base color |
| `res://assets/models/rocket.glb` | Joined rocket export |
| `res://assets/sky/space-panorama.png` | Background panorama |
| `res://assets/tests/planet-unbaked.glb` | Optional comparison from A6 |

3. In **Project → Project Settings → Display → Window**, set **Viewport Width 1280** and **Viewport Height 720**. Under Stretch, use **Mode: canvas_items** so the HUD scales with the viewport. The settings search can locate each property.

![Godot window settings with viewport width 1280 and height 720](doc-assets/godot/window-viewport-size.png)

*Set Viewport Width to 1280 and Viewport Height to 720. Stretch Mode is configured separately; it is not visible in this crop.*

4. Keep **Physics → Common → Physics Ticks per Second = 60**.
5. Add these direct children to Main:

| Node | Type | Settings |
| --- | --- | --- |
| `Camera3D` | Camera3D | Position `(0, 20, 0)`; Rotation `(-90°, 0°, 0°)`; Projection **Orthogonal**; Keep Aspect **Keep Height**; Size **24**; Current on |
| `Sun` | DirectionalLight3D | Rotation `(-45°, -30°, 0°)`; Energy **1.5** |
| `WorldEnvironment` | WorldEnvironment | Configure below |
| `HUD` | CanvasLayer | Default settings |

The orbit lies in the **XZ plane**, at Y = 0. The camera looks down on that plane; world -Z points upward on screen. No ground plane is needed.

#### Add a readable status label

1. Select HUD and add a **Label** child named **Status**. It belongs to the screen overlay, not the 3D scene geometry.
2. Switch to the **2D** workspace and select Status. Under **Layout**, choose an anchor layout and the **Top Left** preset, so all four anchors are zero. Keep this Label directly under CanvasLayer; do not add a Container parent.
3. Expand **Layout → Transform**. Set Position **`(16, 16)`** and Size **`(1224, 114)`**.
4. If your Inspector exposes offsets instead, enter **Left 16, Top 16, Right 1240, Bottom 130**. These are edge positions relative to the same top-left anchor: width = 1240 − 16 and height = 130 − 16. Use either representation; they describe the same rectangle.
5. Set Text to **`Orbit workshop`**, leave the text aligned left/top, and set **Theme Overrides → Font Sizes → Font Size = 18**.

> [!NOTE]
> Anchors define the reference point; position/offsets place the rectangle relative to it. The status is a 2D overlay and remains at the upper-left while the 3D camera changes. See [Godot's explanation of anchors and offsets](https://docs.godotengine.org/en/stable/tutorials/ui/size_and_anchors.html).

#### Add the sky and ambient light

1. Select WorldEnvironment and create a new **Environment** resource.
2. Set **Background → Mode = Sky**. Under Sky, create a **Sky** resource, then a **PanoramaSkyMaterial** as its Sky Material.
3. Assign **`res://assets/sky/space-panorama.png`** to **Panorama**.

![Godot Environment Sky and PanoramaSkyMaterial resources](doc-assets/godot/environment-panorama-sky.png)

*Background Mode is Sky. The Sky resource contains a PanoramaSkyMaterial with the panorama assigned.*

4. Set **Ambient Light → Source = Color**, Color **`#B8CCE6`** (pale blue-gray), and Energy **0.5**. Use full opacity; append `FF` if the color field requires RGBA.

![Godot Ambient Light using Color and energy 0.5](doc-assets/godot/environment-ambient-light.png)

*Ambient Light uses Color as its source and energy 0.5. Enter the specified pale blue-gray in the colour field.*

![Main scene tree with camera light environment and HUD label](doc-assets/godot/main-camera-environment-hud-tree.png)

*Scene-tree checkpoint after adding the camera, light, environment, and CanvasLayer/Status. Name the CanvasLayer HUD in your scene; the capture uses Hud.*

5. Save and run the current Main scene with **F6**. At this stage, expect the star background and label, without the planet or rocket.

![Running scene with star background and Orbit workshop label](doc-assets/godot/main-sky-and-status-preview.png)

*First Main-scene run: the sky and upper-left status label are visible. No planet or rocket has been instanced yet.*

Godot's [PanoramaSkyMaterial](https://docs.godotengine.org/en/stable/classes/class_panoramaskymaterial.html) displays the 2:1 environment image. The background is separate from the ambient and directional lighting that make the models visible.

### B2. Create the planet scene

1. Create a new scene using **Other Node → StaticBody3D** and name the root **Planet**.
2. Drag `planet.glb` from the FileSystem panel onto Planet. Rename this imported instance **PlanetVisual**. Keep its position/rotation zero and scale one.
3. Add **CollisionShape3D** directly under Planet. In its Shape field, create **SphereShape3D** and set Radius **2.0**. Keep the node transform at identity.

![SphereShape3D resource with radius 2.0](doc-assets/godot/planet-sphere-collider-radius.png)

*Set Radius on the SphereShape3D resource to 2.0. Keep the collision node’s scale at one.*

![Planet root with PlanetVisual and CollisionShape3D children](doc-assets/godot/planet-wrapper-scene-tree.png)

*The imported visual and collision shape are separate children of the Planet physical root.*

4. Select Planet. Under Collision, enable **Layer 1 only** and **Mask 2 only**. Disable any other checked boxes.

![Planet collision layer one and mask two selected](doc-assets/godot/planet-collision-layer-mask.png)

*Planet belongs to layer 1 and checks mask 2.*

5. Save as **`res://scenes/planet/Planet.tscn`**.

| Node path | Responsibility |
| --- | --- |
| `Planet` | Fixed physical body |
| `Planet/PlanetVisual` | Imported visual model |
| `Planet/CollisionShape3D` | Sphere of radius 2 |

> [!IMPORTANT]
> The visible mesh and collision geometry are separate. Change the sphere's **Radius**, not the physical body's Scale. Keep Planet and CollisionShape3D at unit scale. A collision layer identifies the body; its mask selects layers to interact with.

### B3. Create the rocket scene

1. Create a new scene with a **CharacterBody3D** root named **Rocket**.
2. Add a **Node3D** child named **Visual**. Drag `rocket.glb` below Visual, keeping the imported instance's own transform unchanged.
3. Set **Visual Scale `(0.5, 0.5, 0.5)`** and **Rotation `(-90°, 0°, 0°)`**. The rocket's nose should now point along its parent's **-Z**, the forward direction used by our code.
4. Add **CollisionShape3D directly under Rocket**, beside Visual. Give it a **SphereShape3D**, Radius **0.65**, centered at zero. Keep the body and collider scales at one.

![Rocket scene hierarchy and rotated visual in the editor](doc-assets/godot/rocket-wrapper-orientation-preview.png)

*The imported mesh is under Visual; CollisionShape3D is a separate child of Rocket. The coloured arcs are the transform gizmo, not a running-game collision overlay.*

5. Select Rocket and set **Collision Layer 2 only**, **Collision Mask 1 only**.

![Rocket collision layer two and mask one selected](doc-assets/godot/rocket-collision-layer-mask.png)

*Rocket belongs to layer 2 and checks mask 1, matching the planet configuration.*

6. Set Rocket's **Motion Mode to Floating**. We will move it with `MoveAndCollide`, without floor movement logic.

![CharacterBody3D motion mode set to Floating](doc-assets/godot/rocket-floating-motion-mode.png)

*Set the Rocket CharacterBody3D to Floating. The workshop script supplies movement and central gravity.*

7. Save as **`res://scenes/rocket/Rocket.tscn`**. Check that the collider encloses the model.

| Node path | Responsibility |
| --- | --- |
| `Rocket` | Script-controlled physical body |
| `Rocket/Visual` | Scale and axis correction for the artwork |
| `Rocket/Visual/rocket` | Imported instance; its name may differ |
| `Rocket/CollisionShape3D` | Conservative bounding sphere |

> [!NOTE]
> The sphere intentionally approximates the whole rocket, including its fins. Contact can occur before the visible hull touches the planet. Keep styling inside this sphere for the reference experiment. A closer capsule or convex collider is a later refinement.

We use **CharacterBody3D** because the script computes motion. It provides collision-aware movement without requiring the rigid-body force solver. Creating it does not automatically supply our central gravitational attraction.

### B4. Place both scenes in Main

1. Reopen Main.tscn. Drag **Planet.tscn** into Main and set Position `(0, 0, 0)`, Rotation zero, Scale one.
2. Drag **Rocket.tscn** into Main and set Position `(6, 0, 0)`, Rotation zero, Scale one.

![Planet and rocket together in the Godot 3D editor](doc-assets/godot/main-planet-rocket-editor.png)

*Editor overview after instancing both models. Enter their numerical positions in the Inspector as specified above.*

3. Save and press **F5**. Choose **Main.tscn** as the main scene when prompted. If SetupCheck was previously assigned, set **Project Settings → Application → Run → Main Scene** to Main.tscn.
4. Confirm both assets are visible. The rocket is stationary until the script is added.

![Running Main scene with planet rocket sky and the initial label](doc-assets/godot/main-stationary-game-preview.png)

*Game-view checkpoint before adding orbit logic: both assets are visible, and the label still reads Orbit workshop.*

> [!TIP]
> Use **Debug → Visible Collision Shapes** while running to inspect the physical approximations. The normal game screenshot above shows the visual meshes; it is not a collision-overlay screenshot. If models seem too small, verify their scale and the camera's Orthogonal Size before changing any physical dimensions.

### B5. Configure the input actions

Open **Project → Project Settings → Input Map**. Add each action with exactly the spelling below, then assign its keyboard event using the **Physical Key** option when available.

| Action | Key | Before launch | During flight |
| --- | --- | --- | --- |
| `place_left` | Left arrow | Move along -X | — |
| `place_right` | Right arrow | Move along +X | — |
| `place_forward` | Up arrow | Move along -Z | — |
| `place_back` | Down arrow | Move along +Z | — |
| `turn_left` | A | Aim left | Turn left |
| `turn_right` | D | Aim right | Turn right |
| `speed_down` | Z | Decrease initial speed | — |
| `speed_up` | X | Increase initial speed | — |
| `launch` | Space | Launch | — |
| `thrust` | W | — | Accelerate in the nose direction |
| `pause` | P | — | Pause/resume |
| `reset` | R | Restore defaults | Restore defaults |

![Godot Input Map containing the workshop control actions](doc-assets/godot/orbit-input-map.png)

*The input actions map keyboard events to the names used by the script. Match their spelling exactly.*

The reference initial position is `(6, 0, 0)`, and velocity is `(0, 0, -4)`. The script restricts placement to radii 3–9 to keep the rocket outside the planet; the demonstration area ends at radius 10.

**Task 1.B complete:** The two scene instances have visual models, collision shapes, and input actions ready for the simulation.

#### Troubleshooting Task 1.B

| Symptom | Check |
| --- | --- |
| SetupCheck runs instead of Main | Change Application → Run → Main Scene, or use F6 on Main |
| Nothing visible | Active camera, X rotation -90°, both model positions, and lighting |
| Status is missing or misplaced | Label directly under CanvasLayer; top-left anchors; position `(16, 16)`; size `(1224, 114)` |
| Rocket points upward | Rotate Visual by -90° around X; do not rotate only the camera |
| Collider is too large or too small | Planet radius 2; rocket visual scale 0.5; rocket collider radius 0.65; physical roots unscaled |
| Imported content seems read-only | Edit the wrapper scene; keep the imported GLB as its visual child |
| Model edits do not appear | Re-export the final mesh, replace the GLB in `res://assets/models/`, and allow reimport |

---

## Task 1.C — Add control logic and simulate the orbit

**Time target: 30 minutes.** Prerequisites: the Main, Planet, and Rocket scenes from Task 1.B, working input actions, and a tested C# setup.

### C1. Add the complete script

**Project:** `Students-works/firstname-lastname/godot/` · **File:** `Students-works/firstname-lastname/godot/scenes/rocket/RocketOrbit.cs` · **Namespace:** `VisA.OrbitWorkshop` · **Class:** `RocketOrbit`.

1. Open Rocket.tscn and attach a **C#** script named `RocketOrbit.cs` to its CharacterBody3D root. Continue using the C# project created during setup.
2. Replace the generated file with the complete code below, or copy `code/RocketOrbit.cs` into that location. Keep only one compiled copy of this class inside the Godot project.
3. Build the project in Godot. Confirm there are no errors.

![RocketOrbit C sharp class displayed in the script editor](doc-assets/simulation/rocket-orbit-csharp-editor.png)

*RocketOrbit source with its exported references and experiment settings. Use the complete copy-paste listing below; this screenshot is a source-code overview, not a build-success check.*

4. Open **Main.tscn**, select the **Rocket instance**, and assign its exported **Planet** field by dragging Main's Planet node into it. Assign **Status** by dragging `Main/HUD/Status` into that field. These references belong on the instance in Main because the standalone Rocket scene cannot reference Main's nodes.

![RocketOrbit Inspector with Planet and Status assigned](doc-assets/simulation/rocket-planet-status-references.png)

*The Rocket instance in Main has its Planet and Status references assigned.*

5. Keep Initial Offset `(6, 0, 0)`, Initial Speed **4**, Initial Heading Degrees **0**, Mu **96**, and Substeps **4**.

6. Save Main.tscn after assigning the references. Continue with the straight-line experiment in C2 once the script below builds successfully.

```csharp
using Godot;

namespace VisA.OrbitWorkshop;

/// <summary>Moves a rocket in the XZ plane using a fixed central gravitational field.</summary>
public partial class RocketOrbit : CharacterBody3D
{
    [ExportGroup("Scene references")]
    [Export] private StaticBody3D _planet = null!;
    [Export] private Label _status = null!;

    [ExportGroup("Experiment")]
    [Export] private bool _useGravity = true;
    [Export] private bool _allowThrust = true;
    [Export(PropertyHint.Range, "1,500,1")] private float _mu = 96.0f;
    [Export(PropertyHint.Range, "1,8,1")] private int _substeps = 4;
    [Export] private Vector3 _initialOffset = new Vector3(6.0f, 0.0f, 0.0f);
    [Export(PropertyHint.Range, "0,8,0.1")] private float _initialSpeed = 4.0f;
    [Export] private float _initialHeadingDegrees;

    [ExportGroup("Controls")]
    [Export] private float _placementSpeed = 3.0f;
    [Export] private float _turnSpeedDegrees = 90.0f;
    [Export] private float _thrustAcceleration = 2.0f;

    private enum FlightState { Placement, Flying, Paused, Crashed, OutsideArea }
    private FlightState _state;
    private Vector3 _launchOffset;
    private float _launchSpeed;
    private float _heading;

    // Fixed workshop geometry: planet radius 2, rocket bounding radius 0.65.
    private const float MinimumLaunchRadius = 3.0f;
    private const float MaximumLaunchRadius = 9.0f;
    private const float DemoRadius = 10.0f;

    public bool IsFlying => _state == FlightState.Flying;
    public int ResetVersion { get; private set; }

    public override void _Ready()
    {
        if (_planet == null || _status == null)
        {
            GD.PushError("Assign Planet and Status on the Rocket instance in Main.tscn.");
            SetPhysicsProcess(false);
            return;
        }

        ResetExperiment();
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        if (Input.IsActionJustPressed("reset"))
        {
            ResetExperiment();
            return;
        }

        if (Input.IsActionJustPressed("pause"))
        {
            if (_state == FlightState.Flying)
            {
                _state = FlightState.Paused;
            }
            else if (_state == FlightState.Paused)
            {
                _state = FlightState.Flying;
            }
        }

        if (_state == FlightState.Placement)
        {
            UpdatePlacement(dt);
        }
        else if (_state == FlightState.Flying)
        {
            Simulate(dt);
        }

        UpdateStatus();
    }

    private void ResetExperiment()
    {
        _state = FlightState.Placement;
        _launchOffset = ClampLaunchOffset(_initialOffset);
        _launchSpeed = Mathf.Clamp(_initialSpeed, 0.0f, 8.0f);
        _heading = Mathf.DegToRad(_initialHeadingDegrees);
        GlobalPosition = _planet.GlobalPosition + _launchOffset;
        Rotation = new Vector3(0.0f, _heading, 0.0f);
        Velocity = Vector3.Zero;
        ResetVersion++;
        UpdateStatus();
    }

    private static Vector3 ClampLaunchOffset(Vector3 offset)
    {
        offset.Y = 0.0f;
        float radius = offset.Length();
        if (radius < 0.001f) return Vector3.Right * MinimumLaunchRadius;
        return offset / radius * Mathf.Clamp(radius, MinimumLaunchRadius, MaximumLaunchRadius);
    }

    private void UpdatePlacement(float dt)
    {
        Vector2 input = Input.GetVector("place_left", "place_right", "place_forward", "place_back");
        Vector3 movement = new Vector3(input.X, 0.0f, input.Y) * _placementSpeed * dt;
        _launchOffset = ClampLaunchOffset(_launchOffset + movement);
        GlobalPosition = _planet.GlobalPosition + _launchOffset;

        _heading += Input.GetAxis("turn_right", "turn_left") * Mathf.DegToRad(_turnSpeedDegrees) * dt;
        _launchSpeed = Mathf.Clamp(_launchSpeed + Input.GetAxis("speed_down", "speed_up") * 2.0f * dt, 0.0f, 8.0f);
        Rotation = new Vector3(0.0f, _heading, 0.0f);

        if (Input.IsActionJustPressed("launch"))
        {
            Velocity = -GlobalBasis.Z * _launchSpeed;
            _state = FlightState.Flying;
        }
    }

    private void Simulate(float dt)
    {
        if (_allowThrust)
        {
            _heading += Input.GetAxis("turn_right", "turn_left") * Mathf.DegToRad(_turnSpeedDegrees) * dt;
            Rotation = new Vector3(0.0f, _heading, 0.0f);
        }

        int steps = Mathf.Max(_substeps, 1);
        float h = dt / steps;
        for (int i = 0; i < steps; i++)
        {
            Vector3 acceleration = Vector3.Zero;
            if (_useGravity)
            {
                Vector3 toPlanet = _planet.GlobalPosition - GlobalPosition;
                toPlanet.Y = 0.0f;
                float distanceSquared = Mathf.Max(toPlanet.LengthSquared(), 0.01f);
                acceleration = toPlanet.Normalized() * (_mu / distanceSquared);
            }

            if (_allowThrust && Input.IsActionPressed("thrust"))
            {
                acceleration += -GlobalBasis.Z * _thrustAcceleration;
            }

            // Semi-implicit Euler: update velocity, then sweep the proposed displacement.
            Velocity += acceleration * h;
            KinematicCollision3D? collision = MoveAndCollide(Velocity * h);
            if (collision != null)
            {
                Velocity = Vector3.Zero;
                _state = FlightState.Crashed;
                GD.Print("Collision: experiment finished. Press R to reset.");
                break;
            }

            if (GlobalPosition.DistanceTo(_planet.GlobalPosition) > DemoRadius)
            {
                _state = FlightState.OutsideArea;
                break;
            }
        }
    }

    private void UpdateStatus()
    {
        Vector3 displayedVelocity = _state == FlightState.Placement ? -GlobalBasis.Z * _launchSpeed : Velocity;
        float radius = GlobalPosition.DistanceTo(_planet.GlobalPosition);
        float circularSpeed = Mathf.Sqrt(_mu / Mathf.Max(radius, 0.001f));
        string stateText = _state switch
        {
            FlightState.Placement => "PLACE: arrows move | A/D aim | Z/X speed | Space launch",
            FlightState.Flying => "FLYING: A/D turn | W thrust | P pause | R reset",
            FlightState.Paused => "PAUSED: P resume | R reset",
            FlightState.Crashed => "COLLISION: R reset",
            _ => "OUTSIDE DEMO AREA: R reset (this alone does not prove escape)"
        };

        _status.Text = stateText + $"\nRadius: {radius:F2}   Speed: {displayedVelocity.Length():F2}   Circular speed: {circularSpeed:F2}";
        _status.Text += $"\nVelocity: ({displayedVelocity.X:F2}, {displayedVelocity.Y:F2}, {displayedVelocity.Z:F2})";
        _status.Text += $"\nGravity: {_useGravity}   Thrust enabled: {_allowThrust}";
    }
}
```

Main and the physical roots start at unit scale and zero rotation as described in Task 1.B; the script then owns the rocket heading. The visual child's fixed orientation correction is separate from the rocket's heading.

### C2. Observe motion without gravity

Stop the game, select Rocket in Main, turn **Use Gravity off** and **Allow Thrust off**, and save. Run Main with **F5**.

![Experiment settings with gravity and thrust disabled](doc-assets/simulation/straight-line-gravity-thrust-disabled.png)

*Straight-line experiment settings: Use Gravity off, Allow Thrust off, Mu 96, and Substeps 4.*

1. Use the arrow keys to place the rocket, A/D to aim, and Z/X to change the initial speed. Its nose defines the launch direction; the HUD shows the velocity that will be used.
2. Press R to restore the reference case. Before launching, check the placement HUD:

![Placement HUD showing radius six speed four and the initial velocity](doc-assets/simulation/placement-initial-velocity-hud.png)

*Before launch: PLACE state, radius 6, speed 4, and velocity (0, 0, -4), with gravity and thrust disabled. This is the placement screen, not a flight result.*

3. Press Space to launch. With gravity and thrust disabled, the rocket moves in a straight line at constant velocity.
4. Press P to pause/resume or R to reset. The simulation also stops at the edge of the finite demonstration area, radius 10.

The relevant operation in `Simulate` is:

```csharp
KinematicCollision3D? collision = MoveAndCollide(Velocity * h);
```

Velocity is distance per unit time; `Velocity * h` is displacement. `MoveAndCollide` takes displacement, not velocity. Godot checks the swept motion of the collision shape and reports contact. See [PhysicsBody3D's movement API](https://docs.godotengine.org/en/stable/classes/class_physicsbody3d.html).

### C3. Add central gravity

Stop the game, select Rocket in Main, enable **Use Gravity**, leave **Allow Thrust off**, save, and run again. Press Space without changing the defaults.

![Experiment settings with gravity enabled and thrust disabled](doc-assets/simulation/gravity-only-settings.png)

*Gravity-only experiment: Use Gravity on and Allow Thrust off.*

Let **r** point from the rocket to the planet. The acceleration is

$$
\mathbf{a} = \mu\frac{\mathbf{r}}{\lVert\mathbf{r}\rVert^3}, \qquad \mu = GM.
$$

In the code, this is a unit direction multiplied by `mu / distanceSquared`. The rocket is treated as a test body: the planet stays fixed, and rocket mass cancels from its gravitational acceleration. We use consistent demonstration units rather than real astronomical scales.

The integrator is **semi-implicit Euler**:

$$
\mathbf{v}_{n+1} = \mathbf{v}_n + \mathbf{a}(\mathbf{x}_n)h, \qquad
\mathbf{x}_{n+1} = \mathbf{x}_n + \mathbf{v}_{n+1}h.
$$

The position step is performed by the collision sweep. The two key statements are:

```csharp
Velocity += acceleration * h;
KinematicCollision3D? collision = MoveAndCollide(Velocity * h);
```

The order matters: position uses the updated velocity. Godot calls `_PhysicsProcess` at a fixed physics rate. Four substeps within each 1/60-second tick give approximately `h = 1/240 s`. Rendering frame rate is not the integration rate.

For a circular orbit, tangential speed is

$$
v_c = \sqrt{\frac{\mu}{r}}.
$$

At radius 6 with μ = 96, this gives **4**. The initial velocity `(0, 0, -4)` is perpendicular to the initial radius `(6, 0, 0)`. The ideal period is approximately **9.42 seconds**.

**Expected result:** A near-circular numerical trajectory around the planet. Small radial oscillations are expected; this is an approximation, not an exact analytic orbit. Compare one and four substeps only after the reference case works.

### C4. Add rocket control

Stop the game, enable **Allow Thrust** on Rocket in Main, save, and run again.

![Experiment settings with gravity and thrust enabled](doc-assets/simulation/gravity-and-thrust-enabled.png)

*Enable Allow Thrust for the interactive controls experiment while keeping gravity enabled.*

- A/D turns the nose. Changing orientation alone does not change the existing velocity.
- W adds acceleration along the nose direction.
- P pauses without resetting the state.
- R restores the original Inspector-defined position, heading, and speed.

This simple model treats steering as directly controlled orientation and thrust as constant acceleration. It has no fuel, drag, spin dynamics, or varying mass. The rocket can continue moving sideways relative to its nose, as expected when orientation and velocity differ.

![Flight HUD with gravity and thrust controls enabled](doc-assets/simulation/flight-with-thrust-controls-enabled.png)

*Flight with gravity and thrust controls enabled. “Thrust enabled: True” means W is permitted to apply acceleration; it does not show whether W is currently held.*

### C5. Finish on collision

1. Reset with R.
2. Hold Z until initial speed is zero, then press Space.
3. Gravity pulls the rocket inward. The `MoveAndCollide` result becomes non-null when its bounding sphere touches the planet collider.
4. The script stops integration, sets velocity to zero, and displays **COLLISION**. Press R to try again.

The physical contact distance is approximately **2.65** from the planet center: planet radius 2 plus rocket radius 0.65, with a small engine collision margin. We do not implement a bounce or a separate geometric hit test.

![Collision HUD showing zero speed and radius 2.65](doc-assets/simulation/collision-stopped-hud.png)

*COLLISION state: speed and velocity are zero, and the centre distance is approximately 2.65. The screenshot shows the stopping condition, without collision overlays.*

### C6. Compare three or four experiments

Use the Inspector for exact speeds: stop the game, select Rocket in Main, change Initial Speed, save, and run. Keep Initial Offset `(6, 0, 0)`, Heading 0, gravity on, and thrust off.

| Initial speed | Expected behavior | Explanation |
| --- | --- | --- |
| 0 | Falls into the planet and stops | No initial tangential motion |
| 3.5 | Elliptical orbit, initially moving inward | Below circular speed but enough clearance for these colliders |
| 4 | Near-circular orbit | Tangential circular speed for μ = 96 and r = 6 |
| 6 | Outward trajectory, eventually outside the demo area | Above the ideal escape speed, approximately 5.66 |

The HUD's **OUTSIDE DEMO AREA** is a display/simulation boundary condition. Leaving radius 10 does not by itself prove escape: a large bound ellipse could also cross it. The interpretation above uses the ideal model and the specific initial conditions.

Discuss: What changes when you rotate the rocket without thrust? Why is a tangential initial velocity necessary for the circular case? What changes when you decrease the step size?

### Optional: show the trajectory

Skip this if time is short. The model already moves visibly without a trail.

**File:** `Students-works/firstname-lastname/godot/scenes/main/OrbitTrail.cs` · **Namespace:** `VisA.OrbitWorkshop` · **Class:** `OrbitTrail`.

1. Add a **MeshInstance3D** named **Trail** directly under Main. Leave its transform at identity and its Mesh empty.
2. Attach `OrbitTrail.cs`, paste the code below, and build.
3. Select Trail in Main and assign the Rocket instance to its **Rocket** field. Save.
4. The script draws recent positions, retains the trail while paused or after a crash, and clears it on R. It does not predict future motion.

![OrbitTrail Inspector with Rocket assigned and an empty Mesh resource](doc-assets/simulation/trail-rocket-reference.png)

*OrbitTrail has its Rocket reference assigned, while Mesh is still empty because the script creates it at runtime. Rename the MeshInstance3D node Trail in your scene.*

```csharp
using Godot;
using System.Collections.Generic;

namespace VisA.OrbitWorkshop;

/// <summary>Displays recent sampled rocket positions; optional visual aid.</summary>
public partial class OrbitTrail : MeshInstance3D
{
    [Export] private RocketOrbit _rocket = null!;
    private readonly List<Vector3> _points = new List<Vector3>();
    private readonly ImmediateMesh _mesh = new ImmediateMesh();
    private int _resetVersion = -1;

    public override void _Ready()
    {
        Mesh = _mesh;
        MaterialOverride = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = new Color(0.4f, 0.85f, 1.0f)
        };
        CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_rocket == null) return;
        if (_resetVersion != _rocket.ResetVersion)
        {
            _points.Clear();
            _mesh.ClearSurfaces();
            _resetVersion = _rocket.ResetVersion;
        }

        if (!_rocket.IsFlying) return;
        Vector3 point = ToLocal(_rocket.GlobalPosition);
        if (_points.Count > 0 && _points[_points.Count - 1].DistanceSquaredTo(point) < 0.0025f) return;
        _points.Add(point);
        if (_points.Count > 720)
        {
            _points.RemoveAt(0);
        }

        _mesh.ClearSurfaces();
        if (_points.Count < 2) return;
        _mesh.SurfaceBegin(Godot.Mesh.PrimitiveType.Lines);
        for (int i = 1; i < _points.Count; i++)
        {
            _mesh.SurfaceAddVertex(_points[i - 1]);
            _mesh.SurfaceAddVertex(_points[i]);
        }
        _mesh.SurfaceEnd();
    }
}
```

![Running game showing a curved trajectory history behind the rocket](doc-assets/simulation/trajectory-history-game-preview.png)

*The line records positions already visited. This example includes a changing trajectory with thrust controls enabled; it is not an analytic orbit prediction or a fixed-speed reference result.*

### Troubleshooting Task 1.C

| Symptom | Check |
| --- | --- |
| Exported fields missing | Build C# successfully, then select the Rocket instance again |
| Script reports missing references | Assign Planet and Status on Rocket **in Main.tscn**; run Main with F5 |
| No input | Action names must match exactly; focus the game window |
| Immediate collision | Root transforms must be correct; launch outside radius 2.65; colliders must not be under scaled Visual |
| No collision | Planet layer 1; Rocket mask 1; shapes present, direct children of their bodies, and enabled |
| Rocket follows a strange path | Reset to μ = 96, offset `(6, 0, 0)`, speed 4, heading 0, thrust off |
| Rocket nose does not follow the path | Orientation and velocity are separate; turning alone does not change velocity |
| Project uses a rigid-body solver as well | Root must be CharacterBody3D; do not add another movement script or use MoveAndSlide |

**Task 1.C complete:** You can place and launch the rocket, change its trajectory with thrust, observe a near-circular orbit, pause, reset, and terminate the run on collision.

---

## Save your work and optionally submit

Save the Blender files, baked images, exported models, and Godot scenes. Keep every dependency inside **Students-works/firstname-lastname/**. Publishing your work is optional.

### Save the project files

1. In each Blender file, save generated images and use **File → External Data → Make All Paths Relative**. Check for missing files before committing.
2. Keep `.blend`, `.png`, `.glb`, `project.godot`, `.tscn`, `.cs`, C# project/solution files, and relevant `.uid` files. Commit any separately saved Godot resources too.
3. The repository's `.gitignore` excludes Godot caches, C# build output, IDE settings, and Blender backup versions. Check the staged file list so generated data does not enter your submission.

### Optional: commit, push, and open a pull request

Use the fork and **workshop-submission** branch created during setup. Run the following at the **repository root**, replacing `firstname-lastname` with your folder name:

```bash
git status
git branch --show-current
git add -- Students-works/firstname-lastname
git diff --cached --stat
git commit -m "Add my Blender models and Godot orbit workshop"
git push -u origin workshop-submission
```

Check that `origin` points to your fork and that the staged files are all under your own folder. Then, on GitHub:

1. Open your fork and select **workshop-submission**.
2. Choose **Contribute → Open pull request** or **Compare & pull request**.
3. Set the base repository to **unibas-space/VisA-3D-public** and the base branch to **main**. The head repository is your fork and the compare branch is **workshop-submission**.
4. Include your name or chosen nickname and a short description. Review **Files changed**, then create the pull request.
5. If the teaching team requests corrections, commit and push them to the same branch. The pull request updates automatically.

> [!WARNING]
> Your fork, submission, and commit history are publicly visible. Submit only material you intend to publish. Follow the [repository submission rules](../README.md#submission-rules-voluntary).

### Final checkpoint

- [ ] `blender/planet.blend`, editable `blender/rocket.blend`, and `blender/showcase.blend` are saved inside my student folder.
- [ ] The final render, baked image, and both textured GLBs are present.
- [ ] Main opens with a camera, light, sky, planet, rocket, and readable status label.
- [ ] Placement, launch, gravity, pause, thrust, collision, and reset work.
- [ ] I can explain the difference between orientation, velocity, and acceleration.
- [ ] If I chose to submit, I pushed my branch to my fork and opened a pull request.

---

## Assets and material recipes

The workshop directory supplies the following assets. You copy the entire `assets/` folder into your named Blender folder during setup; the sky and exported models are copied into Godot in A7.

| File in `assets/` | Use |
| --- | --- |
| [textures/rocket-pencil-atlas.png](assets/textures/rocket-pencil-atlas.png) | Six colored pencil regions for the rocket's Base Color; sRGB |
| [sky/space-panorama.png](assets/sky/space-panorama.png) | A 2:1 illustrated background for Blender and Godot; sRGB, LDR |
| `material-presets.json` | The four procedural planet palettes used in A2 |
| `textures/planet-basecolor.png` | Your own baked output from A6; created during the workshop |

### Optional material helper

The manual node exercise is the main route. If you need the prepared graph, save the planet in **`Students-works/firstname-lastname/blender/planet.blend`**, select its mesh, and open **`tools/create_planet_material.py`** from the teaching materials in Blender's Text Editor. Set `PALETTE` to `Ocean`, `Rust`, `Ice`, or `Candy` and run it.

The helper reads **`assets/material-presets.json` beside the saved `.blend` file**, assigns a new material to the selected mesh, and leaves saving and baking to you. It does not create a planet mesh or run the bake automatically.

### Optional BlenderKit sky

The supplied sky is sufficient. For another environment, search BlenderKit for **space HDRI** or **starry sky HDRI** and choose a free equirectangular environment. Download it before class and check its own reuse terms before including a third-party file in your public submission.

The supplied atlas and panorama were generated for this workshop. The included screenshots and reference illustrations are the material supplied for this revision; they are teaching images, not additional UV textures.

---

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

