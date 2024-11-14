import bpy
import csv
import mathutils
from collections import OrderedDict
from bpy_extras.io_utils import axis_conversion
from bpy.props import BoolProperty, StringProperty, EnumProperty

bl_info = {
    "name": "PIX CSV",
    "author": "Stanislav Bobovych",
    "version": (2, 0, 0),
    "blender": (4, 0, 0),
    "location": "File > Import-Export",
    "description": "Import PIX CSV dump of mesh. Imports mesh, normals and UVs.",
    "category": "Import"
}


class PIX_CSV_Operator(bpy.types.Operator):
    # Plugin definitions, such as ID, name, and file extension filters
    bl_idname = "object.pix_csv_importer"
    bl_label = "Import PIX CSV"
    filepath: StringProperty(subtype="FILE_PATH")
    filter_glob: StringProperty(default="*.csv", options={'HIDDEN'})

    # Options for generation of vertices
    mirror_x: BoolProperty(
        name="Mirror X",
        description="Mirror all the vertices across X axis",
        default=True,
    )

    vertex_order: BoolProperty(
        name="Change vertex order",
        description="Reorder vertices in counter-clockwise order",
        default=True,
    )

    # Options for axis alignment
    axis_forward: EnumProperty(
        name="Forward",
        items=(
            ('X', "X Forward", ""),
            ('Y', "Y Forward", ""),
            ('Z', "Z Forward", ""),
            ('-X', "-X Forward", ""),
            ('-Y', "-Y Forward", ""),
            ('-Z', "-Z Forward", ""),
        ),
        default='Z',
    )

    axis_up: EnumProperty(
        name="Up",
        items=(
            ('X', "X Up", ""),
            ('Y', "Y Up", ""),
            ('Z', "Z Up", ""),
            ('-X', "-X Up", ""),
            ('-Y', "-Y Up", ""),
            ('-Z', "-Z Up", ""),
        ),
        default='Y',
    )

    def execute(self, context):
        # Ignore `axis_forward` and `axis_up` from `as_keywords`
        keywords = self.as_keywords(ignore=("axis_forward", "axis_up", "filter_glob"))
        global_matrix = axis_conversion(from_forward=self.axis_forward, from_up=self.axis_up).to_4x4()

        keywords["global_matrix"] = global_matrix
        importCSV(**keywords)

        return {'FINISHED'}

    def invoke(self, context, event):
        # This method opens the file selector dialog in Blender 4.0
        context.window_manager.fileselect_add(self)
        return {'RUNNING_MODAL'}

    def draw(self, context):
        layout = self.layout
        col = layout.column()
        col.label(text="Import Options")

        row = col.row()
        row.prop(self, "mirror_x")
        row = col.row()
        row.prop(self, "vertex_order")
        layout.prop(self, "axis_forward")
        layout.prop(self, "axis_up")


def make_mesh(vertices, faces, normals, uvs, global_matrix):
    # Create a new mesh from the vertices and faces
    mesh = bpy.data.meshes.new('name')
    mesh.from_pydata(vertices, [], faces)

    # Update normals
    # Create custom normals for the mesh
    if normals:
        mesh.normals_split_custom_set(normals)

    # Generate UV data
    uv_layer = mesh.uv_layers.new(name="UV")
    for face, uv in enumerate(uv_layer.data):
        uv.uv = uvs[face]

    mesh.update(calc_edges=False)

    # Create an object from the mesh and apply the global matrix
    obj = bpy.data.objects.new('name', mesh)
    obj.matrix_world = global_matrix  # Apply transformation matrix
    bpy.context.collection.objects.link(obj)  # Link object to scene


def importCSV(filepath=None, mirror_x=False, vertex_order=True, global_matrix=None):
    if global_matrix is None:
        global_matrix = mathutils.Matrix()

    # Check if a valid filepath was given
    if filepath is None:
        return

    # Dictionaries
    vertex_dict = {}
    normal_dict = {}

    # Arrays/Lists
    vertices = []
    faces = []
    normals = []
    uvs = []

    with open(filepath) as f:
        # Create the CSV reader
        reader = csv.reader(f)
        next(reader)  # Skip the CSV header

        # Determine the mirroring factor
        x_mod = -1 if mirror_x else 1

        # Variables for processing
        i = 0
        current_face = []

        for row in reader:
            vertex_index = int(row[0])

            # X, Y, Z coordinates of vertices
            vertex_dict[vertex_index] = (
                x_mod * float(row[2]),  # X
                float(row[3]),  # Y
                float(row[4]),  # Z
            )

            # Normal vectors
            normal_dict[vertex_index] = (
                float(row[5]),  # X
                float(row[6]),  # Y
                float(row[7]),  # Z
            )

            # UV coordinates (flipping the V coordinate)
            uv = (float(row[16]), 1.0 - float(row[17]))  # UV (V flipped)

            if i < 2:
                # Append "current" data to list until a 3-vertex face is formed
                current_face.append(vertex_index)
                uvs.append(uv)
                i += 1
            else:
                # Append face and UV data to appropriate dictionary/array/list
                current_face.append(vertex_index)
                if vertex_order:
                    faces.append((current_face[2], current_face[1], current_face[0]))
                else:
                    faces.append(current_face)
                uvs.append(uv)

                # Reset for next iteration
                current_face = []
                i = 0

        # Ensure all vertices and normals exist
        for i in range(len(vertex_dict)):
            if i not in vertex_dict:
                vertex_dict[i] = (0, 0, 0)
                normal_dict[i] = (0, 0, 0)

        # Sort dictionaries by key
        vertex_dict = OrderedDict(sorted(vertex_dict.items(), key=lambda t: t[0]))
        normal_dict = OrderedDict(sorted(normal_dict.items(), key=lambda t: t[0]))

        # Populate vertices and normals arrays
        for key in vertex_dict:
            vertices.append(list(vertex_dict[key]))
        for key in normal_dict:
            normals.append(list(normal_dict[key]))

        make_mesh(vertices, faces, normals, uvs, global_matrix)


# Registration functions
classes = (PIX_CSV_Operator,)

def menu_func_import(self, context):
    self.layout.operator(PIX_CSV_Operator.bl_idname, text="PIX CSV (.csv)")

def register():
    for cls in classes:
        bpy.utils.register_class(cls)
    bpy.types.TOPBAR_MT_file_import.append(menu_func_import)

def unregister():
    for cls in reversed(classes):
        bpy.utils.unregister_class(cls)
    bpy.types.TOPBAR_MT_file_import.remove(menu_func_import)


# Main block
if __name__ == "__main__":
    register()
    # These lines will only run if you execute the script directly
    bpy.ops.object.pix_csv_importer('INVOKE_DEFAULT')
