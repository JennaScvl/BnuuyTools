using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class UVUnwrapper : EditorWindow
{
    private Mesh mesh;
    private Texture2D texture;

    [MenuItem("Bunny/UV Unwrapper")]
    public static void ShowWindow()
    {
        GetWindow<UVUnwrapper>("UV Texture Drawer");
    }

    void OnGUI()
    {
        mesh = (Mesh)EditorGUILayout.ObjectField("Mesh", mesh, typeof(Mesh), true);

        if (GUILayout.Button("Draw UVs"))
        {
            if (mesh != null)
            {
                DrawUVsForSubmeshes();
            }
            else
            {
                Debug.LogError("No mesh selected.");
            }
        }
    }

    void DrawUVsForSubmeshes()
    {
        for (int submeshIndex = 0; submeshIndex < mesh.subMeshCount; submeshIndex++)
        {
            Texture2D texture = new Texture2D(2048, 2048);
            var colors = new Color[2048 * 2048];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
            }
            texture.SetPixels(colors);

            int[] triangles = mesh.GetTriangles(submeshIndex);
            Vector2[] uvs = mesh.uv;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                DrawTriangle(texture, uvs[triangles[i]], uvs[triangles[i + 1]], uvs[triangles[i + 2]]);
            }

            texture.Apply();
            SaveTexture(texture, submeshIndex);
        }
    }

    void DrawTriangle(Texture2D texture, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        DrawLine(texture, p1, p2);
        DrawLine(texture, p2, p3);
        DrawLine(texture, p3, p1);
    }

    void DrawLine(Texture2D texture, Vector2 start, Vector2 end)
    {
        start.x *= 2048;
        start.y *= 2048;
        end.x *= 2048;
        end.y *= 2048;

        Vector2 difference = end - start;
        float steps = Mathf.Max(Mathf.Abs(difference.x), Mathf.Abs(difference.y));

        for (float i = 0; i <= steps; i++)
        {
            float lerp = i / steps;
            var point = Vector2.Lerp(start, end, lerp);
            texture.SetPixel((int)point.x, (int)point.y, Color.black);
        }
    }

    void SaveTexture(Texture2D texture, int submeshIndex)
    {
        byte[] bytes = texture.EncodeToPNG();
        string path = $"{Application.dataPath}/UVTexture_Submesh{submeshIndex}.png";
        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();
    }
}
