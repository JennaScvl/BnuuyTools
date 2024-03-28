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
using System.Collections.Generic;
using System.Linq;

public class SubmeshCombiner : EditorWindow
{
    public SkinnedMeshRenderer skinnedMeshRenderer;

    [MenuItem("Bnuuy/Submesh Combiner")]
    public static void ShowWindow()
    {
        GetWindow<SubmeshCombiner>("Submesh Combiner");
    }

   private void OnGUI()
    {
        skinnedMeshRenderer = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Skinned Mesh Renderer", skinnedMeshRenderer, typeof(SkinnedMeshRenderer), true);

        if (GUILayout.Button("Combine Submeshes"))
        {
            CombineSubmeshes();
        }
    }

    private void CombineSubmeshes()
    {
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("Skinned Mesh Renderer must be assigned.");
            return;
        }

        Mesh originalMesh = skinnedMeshRenderer.sharedMesh;

        if (originalMesh == null)
        {
            Debug.LogError("No mesh found in Skinned Mesh Renderer.");
            return;
        }

        Material[] materials = skinnedMeshRenderer.sharedMaterials;

        // Group submeshes by material
        Dictionary<Material, List<int[]>> groupedSubmeshes = new Dictionary<Material, List<int[]>>();
        Dictionary<Material, List<int>> groupedBlendShapes = new Dictionary<Material, List<int>>();
        HashSet<string> addedBlendShapeNames = new HashSet<string>(); // Keep track of added blend shape names

        for (int i = 0; i < originalMesh.subMeshCount; i++)
        {
            int[] triangles = originalMesh.GetTriangles(i);

            // Skip submeshes with 0 triangles
            if (triangles.Length == 0) continue;

            Material material = materials[i];
            if (!groupedSubmeshes.ContainsKey(material))
            {
                groupedSubmeshes[material] = new List<int[]>();
                groupedBlendShapes[material] = new List<int>();
            }

            groupedSubmeshes[material].Add(triangles);

            // Collect blend shapes
            for (int j = 0; j < originalMesh.blendShapeCount; j++)
            {
                groupedBlendShapes[material].Add(j);
            }
        }

        // Create new mesh with combined submeshes
        Mesh combinedMesh = new Mesh();

        // Copy vertices, normals, etc.
        combinedMesh.vertices = originalMesh.vertices;
        combinedMesh.normals = originalMesh.normals;
        combinedMesh.uv = originalMesh.uv;
        combinedMesh.boneWeights = originalMesh.boneWeights;
        combinedMesh.bindposes = originalMesh.bindposes;

        combinedMesh.subMeshCount = groupedSubmeshes.Count;
        Material[] newMaterials = new Material[groupedSubmeshes.Count];

        int submeshIndex = 0;
        foreach (Material material in groupedSubmeshes.Keys)
        {
            List<int[]> submeshTriangles = groupedSubmeshes[material];
            combinedMesh.SetTriangles(submeshTriangles.SelectMany(x => x).ToArray(), submeshIndex);
            newMaterials[submeshIndex] = material;

            // Merge blend shapes
            Dictionary<string, List<int>> blendShapeMapping = new Dictionary<string, List<int>>();
            foreach (int blendShapeIndex in groupedBlendShapes[material])
            {
                string blendShapeName = originalMesh.GetBlendShapeName(blendShapeIndex);
                if (!blendShapeMapping.ContainsKey(blendShapeName))
                {
                    blendShapeMapping[blendShapeName] = new List<int>();
                }
                blendShapeMapping[blendShapeName].Add(blendShapeIndex);
            }

            foreach (var blendShapeEntry in blendShapeMapping)
            {
                string blendShapeName = blendShapeEntry.Key;
                if (addedBlendShapeNames.Contains(blendShapeName))
                {
                    continue; // Skip if this blend shape name has already been added
                }
                addedBlendShapeNames.Add(blendShapeName);

                List<int> blendShapeIndices = blendShapeEntry.Value;

                for (int frame = 0; frame < originalMesh.GetBlendShapeFrameCount(blendShapeIndices[0]); frame++)
                {
                    Vector3[] deltaVertices = new Vector3[originalMesh.vertexCount];
                    Vector3[] deltaNormals = new Vector3[originalMesh.vertexCount];
                    Vector3[] deltaTangents = new Vector3[originalMesh.vertexCount];

                    float frameWeight = 0;
                    foreach (int blendShapeIndex in blendShapeIndices)
                    {
                        Vector3[] tempDeltaVertices = new Vector3[originalMesh.vertexCount];
                        Vector3[] tempDeltaNormals = new Vector3[originalMesh.vertexCount];
                        Vector3[] tempDeltaTangents = new Vector3[originalMesh.vertexCount];

                        frameWeight = originalMesh.GetBlendShapeFrameWeight(blendShapeIndex, frame);
                        originalMesh.GetBlendShapeFrameVertices(blendShapeIndex, frame, tempDeltaVertices, tempDeltaNormals, tempDeltaTangents);

                        for (int i = 0; i < deltaVertices.Length; i++)
                        {
                            deltaVertices[i] += tempDeltaVertices[i];
                            deltaNormals[i] += tempDeltaNormals[i];
                            deltaTangents[i] += tempDeltaTangents[i];
                        }
                    }

                    combinedMesh.AddBlendShapeFrame(blendShapeName, frameWeight, deltaVertices, deltaNormals, deltaTangents);
                }
            }

            submeshIndex++;
        }

        // Assign the combined mesh to the renderer
        skinnedMeshRenderer.sharedMesh = combinedMesh;
        skinnedMeshRenderer.sharedMaterials = newMaterials;

        Debug.Log("Combined Submeshes successfully.");
    }
}
