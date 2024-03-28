using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SkinnedMeshRenderer))]
public class UVBaker : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)target;

        if (GUILayout.Button("Bake UVs"))
        {
            BakeUVs(skinnedMeshRenderer);
        }
    }

    private void BakeUVs(SkinnedMeshRenderer skinnedMeshRenderer)
    {
        Mesh newMesh = Instantiate(skinnedMeshRenderer.sharedMesh); // Create a copy to avoid changing the asset directly
        //Vector2[] originaluv = mesh.uv;

        Mesh originalMesh = skinnedMeshRenderer.sharedMesh;

        // Create a copy of the original mesh
        //Mesh newMesh = new Mesh();
        newMesh.name = originalMesh.name + "_uvbaked";
        //newMesh.vertices = originalMesh.vertices;
        //newMesh.triangles = originalMesh.triangles;
        //newMesh.normals = originalMesh.normals;
        //newMesh.tangents = originalMesh.tangents;

        Vector2[] originalUVs = originalMesh.uv;
        Vector2[] newUVs = new Vector2[originalUVs.Length];

        // Copy the original UVs to the new UV array
        System.Array.Copy(originalUVs, newUVs, originalUVs.Length);

        // Iterate through the submeshes and apply tiling/offset per material
        for (int submesh = 0; submesh < newMesh.subMeshCount; submesh++)
        {
            Material material = skinnedMeshRenderer.materials[submesh];
            Vector2 tiling = material.mainTextureScale;
            Vector2 offset = material.mainTextureOffset;

            int[] indices = newMesh.GetIndices(submesh);

            // Iterate through the indices of the submesh and modify the UVs
            for (int i = 0; i < indices.Length; i++)
            {
                int vertexIndex = indices[i];
                Vector2 uv = originalUVs[vertexIndex];
                uv.Scale(tiling);
                uv += offset;
                newUVs[vertexIndex] = uv;
            }
        }

        newMesh.uv = newUVs; // Set the UVs for the new mesh


        // Replace the mesh with the updated version
        skinnedMeshRenderer.sharedMesh = newMesh;
    }
}