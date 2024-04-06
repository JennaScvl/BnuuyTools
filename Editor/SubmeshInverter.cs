using UnityEditor;
using UnityEngine;

public class SubmeshInverter : EditorWindow
{
    private SkinnedMeshRenderer skinnedMeshRenderer;

    [MenuItem("Bunny/Submesh Inverter")]
    public static void ShowWindow()
    {
        GetWindow<SubmeshInverter>("Submesh Inverter");
    }

    private void OnGUI()
    {
        skinnedMeshRenderer = EditorGUILayout.ObjectField("Skinned Mesh Renderer", skinnedMeshRenderer, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;

        if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
        {
            Mesh mesh = skinnedMeshRenderer.sharedMesh;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Submesh {i}: {skinnedMeshRenderer.sharedMaterials[i].name}");

                if (GUILayout.Button("Reverse Winding"))
                {
                    Undo.RecordObject(mesh, "Reverse Submesh Winding");
                    ReverseWinding(mesh, i);
                    EditorUtility.SetDirty(mesh);
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private void ReverseWinding(Mesh mesh, int submeshIndex)
    {
        int[] indices = mesh.GetTriangles(submeshIndex);
        for (int i = 0; i < indices.Length; i += 3)
        {
            // Swap the first and last index to reverse the triangle's winding
            int temp = indices[i];
            indices[i] = indices[i + 2];
            indices[i + 2] = temp;
        }
        mesh.SetTriangles(indices, submeshIndex);

        // Flip normals
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -normals[i];
        }
        mesh.normals = normals;
    }
}
