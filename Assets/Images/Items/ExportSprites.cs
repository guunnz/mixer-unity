using UnityEngine;
using UnityEditor;
using System.IO;

public class ExportAssetImages : MonoBehaviour
{
    [MenuItem("123/Export Asset Images to PNG")]
    private static void ExportAssetImagesToPNG()
    {
        // Get all selected asset files in the editor
        foreach (Object obj in Selection.objects)
        {
            ScriptableObject asset = obj as ScriptableObject;
            if (asset != null)
            {
                // Assume the asset has a public Texture2D field or property named 'texture'
                Texture2D texture = typeof(ScriptableObject).GetField("texture")?.GetValue(asset) as Texture2D
                                  ?? typeof(ScriptableObject).GetProperty("texture")?.GetValue(asset) as Texture2D;

                if (texture != null)
                {
                    // Continue with the export process as before
                    byte[] bytes = texture.EncodeToPNG();
                    File.WriteAllBytes(Path.Combine("Assets/ExportedImages2", asset.name + ".png"), bytes);
                }
            }
        }
        Debug.Log("Asset images exported successfully!");
    }
}
