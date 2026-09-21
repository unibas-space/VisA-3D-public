"""Optional Blender helper: assign the workshop material to the active planet mesh.

Save the source file as Name-Lastname-blender/planet.blend, open this script in Blender's
Text Editor, select the planet, and Run Script. This does not save the blend.
"""

import json
from pathlib import Path

import bpy

PALETTE = "Ocean"  # Ocean, Rust, Ice, or Candy.


def linear_rgba(hex_color):
    """Convert a six-digit sRGB color to Blender's linear shader color."""
    values = [int(hex_color[i:i + 2], 16) / 255.0 for i in (1, 3, 5)]
    linear = [v / 12.92 if v <= 0.04045 else ((v + 0.055) / 1.055) ** 2.4 for v in values]
    return (*linear, 1.0)


if not bpy.data.filepath:
    raise RuntimeError("Save the file as Name-Lastname-blender/planet.blend before running this helper.")

obj = bpy.context.active_object
if obj is None or obj.type != "MESH":
    raise RuntimeError("Select the planet mesh before running this helper.")

root = Path(bpy.data.filepath).parent
settings = json.loads((root / "assets" / "material-presets.json").read_text())
preset = settings["presets"][PALETTE]
material = bpy.data.materials.new("PlanetSurface_" + PALETTE)
material.use_nodes = True
nodes = material.node_tree.nodes
nodes.clear()
links = material.node_tree.links

coordinates = nodes.new("ShaderNodeTexCoord")
coordinates.location = (-650, 0)
noise = nodes.new("ShaderNodeTexNoise")
noise.location = (-420, 0)
noise.noise_dimensions = settings["noise_dimensions"]
noise.inputs["Scale"].default_value = preset["noise_scale"]
noise.inputs["Detail"].default_value = settings["noise_detail"]
noise.inputs["Roughness"].default_value = settings["noise_roughness"]
noise.inputs["Distortion"].default_value = settings["noise_distortion"]
ramp = nodes.new("ShaderNodeValToRGB")
ramp.location = (-170, 0)
ramp.color_ramp.interpolation = settings["ramp_interpolation"]
ramp.color_ramp.elements[0].position = settings["ramp_positions"][0]
ramp.color_ramp.elements[1].position = settings["ramp_positions"][1]
ramp.color_ramp.elements[0].color = linear_rgba(preset["low_srgb"])
ramp.color_ramp.elements[1].color = linear_rgba(preset["high_srgb"])
shader = nodes.new("ShaderNodeBsdfPrincipled")
shader.location = (120, 0)
shader.inputs["Metallic"].default_value = settings["metallic"]
shader.inputs["Roughness"].default_value = settings["roughness"]
output = nodes.new("ShaderNodeOutputMaterial")
output.location = (450, 0)

links.new(coordinates.outputs[settings["coordinate_source"]], noise.inputs["Vector"])
links.new(noise.outputs["Fac"], ramp.inputs["Fac"])
links.new(ramp.outputs["Color"], shader.inputs["Base Color"])
links.new(shader.outputs["BSDF"], output.inputs["Surface"])

if obj.data.materials:
    obj.data.materials[0] = material
else:
    obj.data.materials.append(material)
for polygon in obj.data.polygons:
    polygon.material_index = 0

print(f"Assigned {material.name} to {obj.name}. Save the Blender file when ready.")
