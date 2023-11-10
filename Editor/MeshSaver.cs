//Copyright 2023 JennaScvl on GitHub
//https://github.com/JennaScvl/BnuuyTools
//License: You may use and modify this script for your personal projects
//As these are tools for manipulating assets, feel free to do whatever you want
//with the meshes you manipulate with it, as long as you have the rights to the
//meshes in question. I only care about whether or not you distribute the code
//without linking to the BnuuyTools github repository
//However you may not distribute it, nor any piece of it.
//If you wish to share the script please link to this github
//If you wish to contribute improvements, feel free to do a pull request.

using UnityEngine;
using UnityEditor;

public class MeshSaver : EditorWindow
{
    public SkinnedMeshRenderer skinnedMeshRenderer;
    public string meshName = "SavedMesh";

    [MenuItem("Bnuuy/Mesh Saver")]
    public static void ShowWindow()
    {
        GetWindow<MeshSaver>("Mesh Saver");
    }

    private void OnGUI()
    {
        GUILayout.Label("Save Skinned Mesh as Asset", EditorStyles.boldLabel);

        skinnedMeshRenderer = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Skinned Mesh Renderer", skinnedMeshRenderer, typeof(SkinnedMeshRenderer), true);

        meshName = EditorGUILayout.TextField("Mesh Name:", meshName);

        if (GUILayout.Button("Save Mesh"))
        {
            SaveMesh();
        }
    }

    private void SaveMesh()
    {
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("Skinned Mesh Renderer must be assigned.");
            return;
        }

        if (string.IsNullOrEmpty(meshName))
        {
            Debug.LogError("Asset Name must be provided.");
            return;
        }

        Mesh skinnedMesh = skinnedMeshRenderer.sharedMesh;

        if (skinnedMesh == null)
        {
            Debug.LogError("No mesh found in Skinned Mesh Renderer.");
            return;
        }

        // Clone the original mesh
        Mesh clonedMesh = Object.Instantiate(skinnedMesh);

        // Give it a name to show up in the Assets folder
        clonedMesh.name = meshName;

        // Save the cloned mesh as a new asset
        string assetPath = "Assets/" + meshName + ".asset";
        AssetDatabase.CreateAsset(clonedMesh, assetPath);
        AssetDatabase.SaveAssets(); // Save and refresh the asset database

        // Reassign the saved mesh asset to the skinned mesh renderer
        skinnedMeshRenderer.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);

        Debug.Log("Skinned Mesh saved and applied successfully to " + assetPath);
    }

}
