# Unity Import Scripts

These scripts enable you to import TSG meshes, colliders, and music into Unity to visualize.

These scripts have only been tested on the PS3 versions of the files.

## Installation

Copy all the files except TSGMesh into an `Assets/Editor/` folder. Make sure to copy the vgmstream-win64 folder into `Editor`. The path to `vgmstream-cli.exe` should look like `Assets/Editor/vgmstream-win64/vgmstream-cli.exe`

Copy TSGMesh to anywhere in `Assets/`, except the Editor folder.

## Usage

### Meshes

Open the window by going to Window -> Simpsons -> Mesh

Press the "Find Mesh" button, and locate a mesh you want to extract. These are found in `build/PS3/pal_en/assets/`.

Once you have selected a mesh, press the "Load Mesh" button. Depending on the size of the mesh, this may take from a few seconds to over a minute.

This will create multiple GameObjects as needed for all the meshes and submeshes. The objects are named `EAMeshXSubmeshY`. X is the index of which EARS_MESH it belongs to, and Y is the submesh index of the mesh inside that EARS_MESH.

Meshes are exported with vertex positions (obviously), vertex normals, vertex colors, and 2 sets of UVs. Depending on the mesh, the vertex colors and UVs might not be accurate or even present in the original file. Clicking the checkbox next to "Swap UV" in the TSG Mesh attached will swap UV1 with UV2.

### Collision

The process for collision is exactly the same as for meshes. Colliders are exported as eithe SphereColliders or MeshColliders.

### Music

For music, drag the `.mus` and `.msb` files into the project. Then, click on the `.mus` file and press the button in the inspector pane labeled "Extract Music". Depending on the length of the music, this could take several minutes.

When it has finished, it will have generated an `.mpf` file alongside four `.wav` files. The file with no number at the end is the full music track - 3 sets of stereo channels. The file ending with _0 is track 0, the file ending with _1 is track 1, and the file ending in _2 is track 2.