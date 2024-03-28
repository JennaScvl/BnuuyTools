using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Collections.Generic;

public class MeshCombiner : EditorWindow
{
    private SkinnedMeshRenderer meshRenderer1;
    private SkinnedMeshRenderer meshRenderer2;

    [MenuItem("Tools/MeshCombiner")]
    public static void ShowWindow()
    {
        GetWindow<MeshCombiner>("MeshCombiner");
    }

    void OnGUI()
    {
        meshRenderer1 = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Mesh 1", meshRenderer1, typeof(SkinnedMeshRenderer), true);
        meshRenderer2 = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Mesh 2", meshRenderer2, typeof(SkinnedMeshRenderer), true);

        if (GUILayout.Button("Combine"))
        {
            if (meshRenderer1 == null || meshRenderer2 == null)
            {
                Debug.LogError("Assign both mesh renderers.");
                return;
            }

            CombineMeshes(meshRenderer1, meshRenderer2);
        }
    }

    private void CombineMeshes(SkinnedMeshRenderer meshRenderer1, SkinnedMeshRenderer meshRenderer2)
    {
        // Initialize the combined mesh arrays
        Vector3[] combinedVertices = new Vector3[meshRenderer1.sharedMesh.vertexCount + meshRenderer2.sharedMesh.vertexCount];
        Vector3[] combinedNormals = new Vector3[combinedVertices.Length];
        Vector2[] combinedUVs = new Vector2[combinedVertices.Length];
        BoneWeight[] combinedBoneWeights = new BoneWeight[combinedVertices.Length];
        Matrix4x4[] combinedBindPoses = meshRenderer1.sharedMesh.bindposes.Concat(meshRenderer2.sharedMesh.bindposes).ToArray();

        // Copy the data from the first mesh
        System.Array.Copy(meshRenderer1.sharedMesh.vertices, combinedVertices, meshRenderer1.sharedMesh.vertexCount);
        System.Array.Copy(meshRenderer1.sharedMesh.normals, combinedNormals, meshRenderer1.sharedMesh.vertexCount);
        System.Array.Copy(meshRenderer1.sharedMesh.uv, combinedUVs, meshRenderer1.sharedMesh.vertexCount);
        System.Array.Copy(meshRenderer1.sharedMesh.boneWeights, combinedBoneWeights, meshRenderer1.sharedMesh.vertexCount);

        // Copy the data from the second mesh, with an offset
        int offset = meshRenderer1.sharedMesh.vertexCount;
        System.Array.Copy(meshRenderer2.sharedMesh.vertices, 0, combinedVertices, offset, meshRenderer2.sharedMesh.vertexCount);
        System.Array.Copy(meshRenderer2.sharedMesh.normals, 0, combinedNormals, offset, meshRenderer2.sharedMesh.vertexCount);
        System.Array.Copy(meshRenderer2.sharedMesh.uv, 0, combinedUVs, offset, meshRenderer2.sharedMesh.vertexCount);
        System.Array.Copy(meshRenderer2.sharedMesh.boneWeights, 0, combinedBoneWeights, offset, meshRenderer2.sharedMesh.boneWeights.Length);

        // Adjust bone weights for the second mesh
        for (int i = offset; i < combinedBoneWeights.Length; i++)
        {
            BoneWeight boneWeight = combinedBoneWeights[i];
            boneWeight.boneIndex0 += meshRenderer1.sharedMesh.bindposes.Length;
            boneWeight.boneIndex1 += meshRenderer1.sharedMesh.bindposes.Length;
            boneWeight.boneIndex2 += meshRenderer1.sharedMesh.bindposes.Length;
            boneWeight.boneIndex3 += meshRenderer1.sharedMesh.bindposes.Length;
            combinedBoneWeights[i] = boneWeight;
        }

        // Create the new mesh and assign the combined data
        Mesh combinedMesh = new Mesh();
        combinedMesh.vertices = combinedVertices;
        combinedMesh.normals = combinedNormals;
        combinedMesh.uv = combinedUVs;
        combinedMesh.boneWeights = combinedBoneWeights;
        combinedMesh.bindposes = combinedBindPoses;

        // Combine submeshes and materials
        List<Material> materialsList = new List<Material>();
        materialsList.AddRange(meshRenderer1.materials);
        materialsList.AddRange(meshRenderer2.materials);

        // Handling submeshes and triangles
        combinedMesh.subMeshCount = meshRenderer1.sharedMesh.subMeshCount + meshRenderer2.sharedMesh.subMeshCount;
        int submeshOffset = 0;
        for (int submesh = 0; submesh < meshRenderer1.sharedMesh.subMeshCount; submesh++)
        {
            combinedMesh.SetTriangles(meshRenderer1.sharedMesh.GetTriangles(submesh), submesh);
        }
        submeshOffset += meshRenderer1.sharedMesh.subMeshCount;

        for (int submesh = 0; submesh < meshRenderer2.sharedMesh.subMeshCount; submesh++)
        {
            int[] triangles = meshRenderer2.sharedMesh.GetTriangles(submesh).Select(index => index + offset).ToArray();
            combinedMesh.SetTriangles(triangles, submesh + submeshOffset);
        }

        // Combine blend shapes
        CombineBlendShapes(meshRenderer1, combinedMesh, 0);
        CombineBlendShapes(meshRenderer2, combinedMesh, offset);

        // Assign the new mesh to a new SkinnedMeshRenderer or an existing one
        SkinnedMeshRenderer newMeshRenderer = new GameObject("CombinedMesh").AddComponent<SkinnedMeshRenderer>();
        newMeshRenderer.sharedMesh = combinedMesh;
        newMeshRenderer.rootBone = meshRenderer1.rootBone; // You might want to handle this differently
        newMeshRenderer.bones = meshRenderer1.bones.Concat(meshRenderer2.bones).ToArray();
        newMeshRenderer.materials = materialsList.ToArray();
    }

    private void CombineBlendShapes(SkinnedMeshRenderer meshRenderer, Mesh combinedMesh, int vertexOffset)
    {
        for (int i = 0; i < meshRenderer.sharedMesh.blendShapeCount; i++)
        {
            string blendShapeName = meshRenderer.sharedMesh.GetBlendShapeName(i);
            for (int frameIndex = 0; frameIndex < meshRenderer.sharedMesh.GetBlendShapeFrameCount(i); frameIndex++)
            {
                float frameWeight = meshRenderer.sharedMesh.GetBlendShapeFrameWeight(i, frameIndex);
                Vector3[] deltaVertices = new Vector3[combinedMesh.vertexCount];
                Vector3[] deltaNormals = new Vector3[combinedMesh.vertexCount];
                Vector3[] deltaTangents = new Vector3[combinedMesh.vertexCount];

                // Initialize arrays to zero
                Array.Clear(deltaVertices, 0, deltaVertices.Length);
                Array.Clear(deltaNormals, 0, deltaNormals.Length);
                Array.Clear(deltaTangents, 0, deltaTangents.Length);

                // Get blend shape frame vertices, normals, and tangents
                Vector3[] frameVertices = new Vector3[meshRenderer.sharedMesh.vertexCount];
                Vector3[] frameNormals = new Vector3[meshRenderer.sharedMesh.vertexCount];
                Vector3[] frameTangents = new Vector3[meshRenderer.sharedMesh.vertexCount];
                meshRenderer.sharedMesh.GetBlendShapeFrameVertices(i, frameIndex, frameVertices, frameNormals, frameTangents);

                // Apply the offset if needed (for the second mesh)
                for (int j = 0; j < frameVertices.Length; j++)
                {
                    deltaVertices[j + vertexOffset] = frameVertices[j];
                    deltaNormals[j + vertexOffset] = frameNormals[j];
                    // Tangents are typically not used for blend shapes, but they're included here for completeness
                    deltaTangents[j + vertexOffset] = frameTangents[j];
                }

                // Add the blend shape frame to the combined mesh
                combinedMesh.AddBlendShapeFrame(blendShapeName, frameWeight, deltaVertices, deltaNormals, deltaTangents);
            }
        }
    }
}
