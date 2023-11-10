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

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class BoneMerger : EditorWindow
{
    [MenuItem("Bnuuy/Bone Merger")]
    private static void Init()
    {
        GetWindow<BoneMerger>("Bone Merger");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Merge Selected Bone"))
        {
            Transform selectedBone = Selection.activeTransform;
            string name = selectedBone.name;
            if (selectedBone != null && selectedBone.parent != null &&
                selectedBone.GetComponent<SkinnedMeshRenderer>() == null)
            {
                MergeBone(selectedBone);
                Debug.Log("Bone " + name + " has been merged with its parent.");
            }
            else
            {
                Debug.LogError("Please select a valid bone that is not the root and has a parent, and is not a SkinnedMeshRenderer itself.");
            }
        }
    }

    private static void MergeBone(Transform boneToMerge)
    {
        Transform parentBone = boneToMerge.parent;
        SkinnedMeshRenderer[] skinnedMeshRenderers = Object.FindObjectsOfType<SkinnedMeshRenderer>();

        // Prepare a list of bones to be merged for all meshes that will need updating.
        foreach (var skinnedMesh in skinnedMeshRenderers)
        {
            int boneIndexToMerge = System.Array.IndexOf(skinnedMesh.bones, boneToMerge);
            if (boneIndexToMerge == -1) continue; // Bone is not used in this SkinnedMeshRenderer, so skip it.

            int parentBoneIndex = System.Array.IndexOf(skinnedMesh.bones, parentBone);
            if (parentBoneIndex == -1) continue; // Parent bone is not part of the skinned mesh renderer, skip it.

            // Clone the mesh to keep the original intact.
            Mesh clonedMesh = Instantiate(skinnedMesh.sharedMesh);
            clonedMesh.name = skinnedMesh.sharedMesh.name + "_merged";

            // Transfer bone weights from the bone to its parent.
            BoneWeight[] boneWeights = clonedMesh.boneWeights;
            for (int i = 0; i < boneWeights.Length; ++i)
            {
                // Transfer weight logic
                boneWeights[i] = TransferWeight(boneWeights[i], boneIndexToMerge, parentBoneIndex);
            }

            // Normalize after transfer to ensure the sum of weights is valid.
            NormalizeBoneWeights(boneWeights);

            // Remove the bone to be merged from the bones list.
            List<Transform> bonesList = new List<Transform>(skinnedMesh.bones);
            bonesList.RemoveAt(boneIndexToMerge);

            // Adjust the bone index references in all the weights.
            UpdateBoneWeightIndicesAfterRemoval(boneWeights, boneIndexToMerge);

            // Assign the updated bone weights and bone list to the mesh.
            clonedMesh.boneWeights = boneWeights;
            skinnedMesh.sharedMesh = clonedMesh;
            skinnedMesh.bones = bonesList.ToArray();

            // Recalculate the bind poses for the new bone setup.
            UpdateBindPoses(skinnedMesh);
        }

        // Destroy the bone GameObject after all skinned mesh renderers have been updated.
        Selection.activeTransform = boneToMerge.parent;
        DestroyImmediate(boneToMerge.gameObject);
    }

    // The rest of the necessary methods below...
    private static BoneWeight TransferWeight(BoneWeight weight, int boneIndexToMerge, int parentBoneIndex)
    {
        // First, let's find the total weight that needs to be transferred to the parent bone.
        float weightToTransfer = 0f;
        if (weight.boneIndex0 == boneIndexToMerge) weightToTransfer += weight.weight0;
        if (weight.boneIndex1 == boneIndexToMerge) weightToTransfer += weight.weight1;
        if (weight.boneIndex2 == boneIndexToMerge) weightToTransfer += weight.weight2;
        if (weight.boneIndex3 == boneIndexToMerge) weightToTransfer += weight.weight3;

        // Now, we transfer the accumulated weight to the parent bone. 
        // We need to check each bone index slot to see if the parent bone is already influencing the vertex.
        // If it is, we add to that weight; if not, we find an empty slot or the least influential slot for the parent bone.
        BoneWeight updatedWeight = weight;
        bool parentAlreadyInfluencing = false;

        int[] boneIndices = new[] { updatedWeight.boneIndex0, updatedWeight.boneIndex1, updatedWeight.boneIndex2, updatedWeight.boneIndex3 };
        float[] weights = new[] { updatedWeight.weight0, updatedWeight.weight1, updatedWeight.weight2, updatedWeight.weight3 };

        // Zero out the weights for the bone index to merge.
        for (int i = 0; i < 4; i++)
        {
            if (boneIndices[i] == boneIndexToMerge)
            {
                boneIndices[i] = parentBoneIndex; // Redirect to parent bone index.
                weights[i] = 0; // Zero out the weight as it will be accumulated in the parent's slot.
            }
            if (boneIndices[i] == parentBoneIndex)
            {
                parentAlreadyInfluencing = true; // Mark that parent is influencing to avoid overwriting its slot.
            }
        }

        // If the parent bone was already influencing, add the weight to be transferred.
        if (parentAlreadyInfluencing)
        {
            for (int i = 0; i < 4; i++)
            {
                if (boneIndices[i] == parentBoneIndex)
                {
                    weights[i] += weightToTransfer; // Add to the existing weight of the parent bone.
                    break; // We only add the weight to the first instance of the parent bone index found.
                }
            }
        }
        else
        {
            // If the parent bone was not influencing, find a slot to add its weight.
            // Typically, you would find the slot with the least influence to replace, but since the bone is being removed,
            // you can simply add the parent bone to the slot where the bone index was zeroed out.
            for (int i = 0; i < 4; i++)
            {
                if (boneIndices[i] == parentBoneIndex)
                {
                    weights[i] += weightToTransfer; // Assign the transferred weight.
                    break; // Exit after assigning the weight once.
                }
            }
        }

        // Normalize the weights across all influencing bones.
        float totalWeights = weights[0] + weights[1] + weights[2] + weights[3];
        if (totalWeights > 0)
        {
            weights[0] /= totalWeights;
            weights[1] /= totalWeights;
            weights[2] /= totalWeights;
            weights[3] /= totalWeights;
        }

        // Reassign back to the updated weight structure.
        updatedWeight.boneIndex0 = boneIndices[0];
        updatedWeight.weight0 = weights[0];
        updatedWeight.boneIndex1 = boneIndices[1];
        updatedWeight.weight1 = weights[1];
        updatedWeight.boneIndex2 = boneIndices[2];
        updatedWeight.weight2 = weights[2];
        updatedWeight.boneIndex3 = boneIndices[3];
        updatedWeight.weight3 = weights[3];

        return updatedWeight;
    }


    private static void UpdateBindPoses(SkinnedMeshRenderer skinnedMesh)
    {
        Matrix4x4[] bindPoses = new Matrix4x4[skinnedMesh.bones.Length];
        for (int i = 0; i < skinnedMesh.bones.Length; i++)
        {
            // Calculate the bind pose for each bone as the inverse of the bone's transformation matrix relative to the root bone's transform at the time of binding.
            // The root bone's localToWorldMatrix will transform the point from local root bone space to world space,
            // and the bone's worldToLocalMatrix will transform the point from world space to the bone's local space.
            bindPoses[i] = skinnedMesh.bones[i].worldToLocalMatrix * skinnedMesh.transform.localToWorldMatrix;
        }
        skinnedMesh.sharedMesh.bindposes = bindPoses;
    }

    // Normalizes the bone weights to ensure the sum of weights is equal to 1.
    private static void NormalizeBoneWeights(BoneWeight[] boneWeights)
    {
        for (int i = 0; i < boneWeights.Length; ++i)
        {
            float totalWeight = boneWeights[i].weight0 + boneWeights[i].weight1 + boneWeights[i].weight2 + boneWeights[i].weight3;

            // Normalize if the total weight is greater than zero to avoid division by zero
            if (totalWeight > 0)
            {
                boneWeights[i].weight0 /= totalWeight;
                boneWeights[i].weight1 /= totalWeight;
                boneWeights[i].weight2 /= totalWeight;
                boneWeights[i].weight3 /= totalWeight;
            }
        }
    }

    // Updates the bone weight indices after a bone has been removed from the skeleton.
    private static void UpdateBoneWeightIndicesAfterRemoval(BoneWeight[] boneWeights, int removedBoneIndex)
    {
        for (int i = 0; i < boneWeights.Length; ++i)
        {
            boneWeights[i].boneIndex0 = (boneWeights[i].boneIndex0 > removedBoneIndex) ? boneWeights[i].boneIndex0 - 1 : boneWeights[i].boneIndex0;
            boneWeights[i].boneIndex1 = (boneWeights[i].boneIndex1 > removedBoneIndex) ? boneWeights[i].boneIndex1 - 1 : boneWeights[i].boneIndex1;
            boneWeights[i].boneIndex2 = (boneWeights[i].boneIndex2 > removedBoneIndex) ? boneWeights[i].boneIndex2 - 1 : boneWeights[i].boneIndex2;
            boneWeights[i].boneIndex3 = (boneWeights[i].boneIndex3 > removedBoneIndex) ? boneWeights[i].boneIndex3 - 1 : boneWeights[i].boneIndex3;
        }
    }


}
