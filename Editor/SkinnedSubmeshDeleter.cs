using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SkinnedSubmeshDeleter : EditorWindow
{
    public SkinnedMeshRenderer skinnedMeshRenderer;

    private bool[] includeInAtlas;
    private bool[] deleteSubmesh;

    [MenuItem("Bunny/Skinned Submesh Atlaser")]
    public static void ShowWindow()
    {
        GetWindow<SkinnedSubmeshDeleter>("Skinned Submesh Atlaser");
    }

    private void OnGUI()
    {
        skinnedMeshRenderer = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Skinned Mesh Renderer", skinnedMeshRenderer, typeof(SkinnedMeshRenderer), true);

        if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
        {
            if (deleteSubmesh == null || deleteSubmesh.Length != skinnedMeshRenderer.sharedMesh.subMeshCount)
            {
                deleteSubmesh = new bool[skinnedMeshRenderer.sharedMesh.subMeshCount];
            }

            for (int i = 0; i < skinnedMeshRenderer.sharedMesh.subMeshCount; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Submesh {i}");
                deleteSubmesh[i] = EditorGUILayout.ToggleLeft("Delete", deleteSubmesh[i]);
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Delete Selected Submeshes"))
            {
                DeleteSelectedSubmeshes();
            }
        }
    }

    private void DeleteSelectedSubmeshes()
    {
        Mesh originalMesh = skinnedMeshRenderer.sharedMesh;
        Mesh newMesh = Instantiate(originalMesh);
        newMesh.name = originalMesh.name + "_modified";

        for (int i = deleteSubmesh.Length - 1; i >= 0; i--)
        {
            if (deleteSubmesh[i])
            {
                List<int> combinedIndices = new List<int>();
                for (int j = 0; j < newMesh.subMeshCount; j++)
                {
                    if (j != i)
                    {
                        combinedIndices.AddRange(newMesh.GetIndices(j));
                    }
                }

                newMesh.subMeshCount = 1;
                newMesh.SetIndices(combinedIndices.ToArray(), MeshTopology.Triangles, 0);
            }
        }

        // Save the modified mesh as a new asset
        string path = "Assets/" + newMesh.name + ".asset";
        AssetDatabase.CreateAsset(newMesh, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Selected submeshes deleted and new mesh saved at " + path);

        // Apply the new mesh to the SkinnedMeshRenderer
        skinnedMeshRenderer.sharedMesh = newMesh;
    }
}
