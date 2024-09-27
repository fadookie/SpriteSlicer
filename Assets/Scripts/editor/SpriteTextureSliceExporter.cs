/**
 * Exports sprite sheet as individual images
 * Based loosely on https://gist.github.com/bmanGH/8785040
 */

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SpriteTextureSliceExporter : ScriptableObject
{
    static string GetOutputDirectory() {
        const string LAST_FOLDER_PREF = "SpriteTextureSliceExporter.LAST_FOLDER_PREF";
        var lastFolder = EditorPrefs.GetString(LAST_FOLDER_PREF, "");
        if (!Directory.Exists(lastFolder)) {
            lastFolder = "";
        }
        var outputDirectory = EditorUtility.OpenFolderPanel("Output of Directory", lastFolder, "");
        if (string.IsNullOrEmpty(outputDirectory)) throw new IOException("Invalid directory chosen.");
        EditorPrefs.SetString(LAST_FOLDER_PREF, outputDirectory);
        return outputDirectory;
    }

    [MenuItem("SpriteTextureSliceExporter/Export Slices")]
    public static void ExportSlices() {
        var outputDirectory = GetOutputDirectory();
        
        var assets = Selection.assetGUIDs
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAllAssetsAtPath)
            .ToArray();
        
        if (assets.Length == 0) {
            EditorUtility.DisplayDialog("SpriteTextureSliceExporter", "Please select texture", "OK");
            return;
        }
        Debug.Log($"ExportSlicesContext, len={assets.Length} {string.Join(System.Environment.NewLine, (object[])assets)}");
        foreach (var subassets in assets) {
            Debug.Log($"ExportSlicesContext, subasset len={subassets.Length} {string.Join(System.Environment.NewLine, (object[])subassets)}");
            var sprites = subassets
                .Where(x => x is Sprite)
                .Cast<Sprite>()
                .ToArray();
            foreach (var sprite in sprites) {
                Debug.Log($"ExportSlicesContext inner loop, {sprite.GetType()} = {sprite}");
                var tex = sprite.texture;
                var r = sprite.textureRect;
                var subtex = tex.CropTexture( (int)r.x, (int)r.y, (int)r.width, (int)r.height );
                if (subtex != null) {
                    var data = subtex.EncodeToPNG();
                    var outPath = $"{outputDirectory}/{sprite.name}.png";
                    File.WriteAllBytes(outPath, data);
                    Debug.Log($"Wrote to '{outPath}'");
                } else {
                    Debug.LogError("Failed to export sprites. Please ensure sprite import settings are correct - see README.md");
                    return;
                }
            }
        }
    }
    
    [MenuItem("SpriteTextureSliceExporter/Export Slices", true)]
    public static bool ExportSlicesValidation() {
        if(Selection.activeObject as Texture2D != null){
            TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Selection.activeObject)) as TextureImporter;
            return textureImporter.textureType == TextureImporterType.Sprite;
        }
        return false;
    }

}