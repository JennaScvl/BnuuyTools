//Copyright 2023 JennaScvl on GitHub
//https://github.com/JennaScvl/BnuuyTools
//License: You may use and modify this script for your personal projects
//However you may not distribute it, nor any piece of it.
//If you wish to share the script please link to this github
//If you wish to contribute improvements, feel free to do a pull request.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BoneRetargeting : EditorWindow
{
    public SkinnedMeshRenderer meshB;
    public Transform armatureRootA;

    [MenuItem("Bnuuy/Bone Retargeting")]
    static void Init()
    {
        BoneRetargeting window = (BoneRetargeting)EditorWindow.GetWindow(typeof(BoneRetargeting));
        window.Show();
    }

    void OnGUI()
    {
        armatureRootA = EditorGUILayout.ObjectField("Target Armature", armatureRootA, typeof(Transform), true) as Transform;
        meshB = EditorGUILayout.ObjectField("Mesh", meshB, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;

        if (GUILayout.Button("Retarget Bones"))
        {
            Retarget();
        }
    }

    void Retarget()
    {
        if (armatureRootA == null || meshB == null)
        {
            Debug.LogError("Please assign both Target Armature and Mesh.");
            return;
        }

        Transform[] allTransformsA = armatureRootA.GetComponentsInChildren<Transform>();
        Transform[] bonesB = meshB.bones;

        Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();

        foreach (Transform t in allTransformsA)
        {
            boneMap.Add(t.name,t);
        }
        //if (bonesB == null) Debug.Log("bonesB null");
        for (int i = 0; i < bonesB.Length; i++)
        {
            if (bonesB[i] != null)
            {
                if (boneMap.ContainsKey(bonesB[i].name))
                {
                    bonesB[i] = boneMap[bonesB[i].name];
                }
                else
                {
                    Debug.LogWarning("Bone not found in Armature Root A: " + bonesB[i].name);
                }
            }
        }

        meshB.bones = bonesB;
        Debug.Log("Bone retargeting completed!");
    }
}
