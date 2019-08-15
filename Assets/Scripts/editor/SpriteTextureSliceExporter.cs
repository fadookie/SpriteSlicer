/**
 * script for export sprite sheet from Unity to Cocos2d
 * Based on https://gist.github.com/bmanGH/8785040
 *
 * @author bman (zx123xz321hm3@hotmail.com)
 * @version 1.0   1/3/2014
 * @version 1.1   6/9/2014   add image slice exporter
 */

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SpriteTextureSliceExporter : ScriptableObject
{
    internal static string Render(string assetPath, Texture2D texture, List<SheetFrame> frames) {
        var sb = new StringBuilder();

        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine(
            "<!DOCTYPE plist PUBLIC \"-//Apple Computer//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">");
        sb.AppendLine("<plist version=\"1.0\">");
        sb.AppendLine("    <dict>");

        sb.AppendLine("        <key>frames</key>");
        sb.AppendLine("        <dict>");
        foreach (var frame in frames) {
            sb.AppendFormat("            <key>{0}</key>",
                frame.name);
            sb.AppendLine();
            sb.AppendLine("            <dict>");
            sb.AppendLine("                <key>frame</key>");
            sb.AppendFormat("                <string>{{{{{0},{1}}},{{{2},{3}}}}}</string>",
                frame.rect.x, frame.rect.y, frame.rect.width, frame.rect.height);
            sb.AppendLine();
            sb.AppendLine("                <key>offset</key>");
            sb.AppendLine("                <string>{0,0}</string>");
            sb.AppendLine("                <key>rotated</key>");
            sb.AppendLine("                <false/>");
            sb.AppendLine("                <key>sourceColorRect</key>");
            sb.AppendFormat("                <string>{{{{0,0}},{{{0},{1}}}}}</string>",
                frame.rect.width, frame.rect.height);
            sb.AppendLine();
            sb.AppendLine("                <key>sourceSize</key>");
            sb.AppendFormat("                <string>{{{0},{1}}}</string>",
                frame.rect.width, frame.rect.height);
            sb.AppendLine();
            sb.AppendLine("            </dict>");
        }
        sb.AppendLine("        </dict>");

        sb.AppendLine("        <key>metadata</key>");
        sb.AppendLine("        <dict>");
        sb.AppendLine("            <key>format</key>");
        sb.AppendLine("            <integer>2</integer>");
        sb.AppendLine("            <key>realTextureFileName</key>");
        sb.AppendFormat("            <string>{0}</string>",
            Path.GetFileName(assetPath));
        sb.AppendLine();
        sb.AppendLine("            <key>size</key>");
        sb.AppendFormat("            <string>{{{0},{1}}}</string>",
            texture.width, texture.height);
        sb.AppendLine();
        sb.AppendLine("            <key>textureFileName</key>");
        sb.AppendFormat("            <string>{0}</string>",
            Path.GetFileName(assetPath));
        sb.AppendLine();
        sb.AppendLine("        </dict>");
        sb.AppendLine("    </dict>");
        sb.AppendLine("</plist>");

        return sb.ToString();
    }

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

    [MenuItem("SpriteTextureSliceExporter/SpriteSheetExporter")]
    public static void SpriteSheetExporter() {
        var selectedTextures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
        if (selectedTextures.Length == 0) {
            EditorUtility.DisplayDialog("SpriteSheetExporter", "Please select texture", "OK");
            return;
        }

        var outputDirectory = GetOutputDirectory();

        foreach (var activeObject in selectedTextures) {
            var selectedTexture = activeObject as Texture2D;
            if (selectedTexture != null) {
                var assetPath = AssetDatabase.GetAssetPath(selectedTexture);
                Debug.Log("export sprite sheet \"" + assetPath + "\"");
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer) {
                    var frames = new List<SheetFrame>();

                    foreach (var spriteMetaData in importer.spritesheet) {
                        var frame = new SheetFrame {name = spriteMetaData.name};


                        // Center = 0, TopLeft = 1, TopCenter = 2, 
                        // TopRight = 3, LeftCenter = 4, RightCenter = 5, 
                        // BottomLeft = 6, BottomCenter = 7, BottomRight = 8, Custom = 9.
                        switch (spriteMetaData.alignment) {
                            case 0:
                                frame.pivot.x = 0.5f;
                                frame.pivot.y = 0.5f;
                                break;
                            case 1:
                                frame.pivot.x = 0.0f;
                                frame.pivot.y = 1.0f;
                                break;
                            case 2:
                                frame.pivot.x = 0.5f;
                                frame.pivot.y = 1.0f;
                                break;
                            case 3:
                                frame.pivot.x = 1.0f;
                                frame.pivot.y = 1.0f;
                                break;
                            case 4:
                                frame.pivot.x = 0.0f;
                                frame.pivot.y = 0.5f;
                                break;
                            case 5:
                                frame.pivot.x = 1.0f;
                                frame.pivot.y = 0.5f;
                                break;
                            case 6:
                                frame.pivot.x = 0.0f;
                                frame.pivot.y = 0.0f;
                                break;
                            case 7:
                                frame.pivot.x = 0.5f;
                                frame.pivot.y = 0.0f;
                                break;
                            case 8:
                                frame.pivot.x = 1.0f;
                                frame.pivot.y = 0.0f;
                                break;
                            case 9:
                                frame.pivot = spriteMetaData.pivot;
                                break;
                        }

                        // flip frame Y
                        frame.rect = spriteMetaData.rect;
                        frame.rect.y = selectedTexture.height - (frame.rect.y + frame.rect.height);

                        frames.Add(frame);
                    }

                    // write sprite sheet data to target file
                    var outputPath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(assetPath))
                                     + ".plist";
                    var output = Render(assetPath, selectedTexture, frames);
                    using (var sw = File.CreateText(outputPath)) {
                        sw.Write(output);
                    }

                    // copy image to target path
                    File.Copy(assetPath, Path.Combine(outputDirectory, Path.GetFileName(assetPath)), true);
                } // if (importer)
            } // if (selectedTexture != null)
        } // foreach (Object activeObject in selectedTextures)
    }
    
    [MenuItem("SpriteTextureSliceExporter/ImageSliceExporter")]
    public static void ImageSliceExporter() {
        var selectedTextures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
        if (selectedTextures.Length == 0) {
            EditorUtility.DisplayDialog("ImageSliceExporter", "Please select texture", "OK");
            return;
        }

        var outputDirectory = GetOutputDirectory();

        foreach (var activeObject in selectedTextures) {
            var selectedTexture = activeObject as Texture2D;
            if (selectedTexture != null) {
                var assetPath = AssetDatabase.GetAssetPath(selectedTexture);
                Debug.Log("export sprite sheet \"" + assetPath + "\"");
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer) {
                    var frames = new List<SheetFrame>();

                    foreach (var spriteMetaData in importer.spritesheet) {
                        var frame = new SheetFrame {name = spriteMetaData.name};


                        // Center = 0, TopLeft = 1, TopCenter = 2, 
                        // TopRight = 3, LeftCenter = 4, RightCenter = 5, 
                        // BottomLeft = 6, BottomCenter = 7, BottomRight = 8, Custom = 9.
                        switch (spriteMetaData.alignment) {
                            case 0:
                                frame.pivot.x = 0.5f;
                                frame.pivot.y = 0.5f;
                                break;
                            case 1:
                                frame.pivot.x = 0.0f;
                                frame.pivot.y = 1.0f;
                                break;
                            case 2:
                                frame.pivot.x = 0.5f;
                                frame.pivot.y = 1.0f;
                                break;
                            case 3:
                                frame.pivot.x = 1.0f;
                                frame.pivot.y = 1.0f;
                                break;
                            case 4:
                                frame.pivot.x = 0.0f;
                                frame.pivot.y = 0.5f;
                                break;
                            case 5:
                                frame.pivot.x = 1.0f;
                                frame.pivot.y = 0.5f;
                                break;
                            case 6:
                                frame.pivot.x = 0.0f;
                                frame.pivot.y = 0.0f;
                                break;
                            case 7:
                                frame.pivot.x = 0.5f;
                                frame.pivot.y = 0.0f;
                                break;
                            case 8:
                                frame.pivot.x = 1.0f;
                                frame.pivot.y = 0.0f;
                                break;
                            case 9:
                                frame.pivot = spriteMetaData.pivot;
                                break;
                        }

                        frame.rect = spriteMetaData.rect;

                        frames.Add(frame);
                    }

                    // export sliced PNG file
                    foreach (var frame in frames) {
                        // copy frame image data
                        var x = Mathf.FloorToInt(frame.rect.x);
                        var y = Mathf.FloorToInt(frame.rect.y);
                        var width = Mathf.FloorToInt(frame.rect.width);
                        var height = Mathf.FloorToInt(frame.rect.height);
                        var pix = selectedTexture.GetPixels(x, y, width, height);
                        var frameTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
                        frameTex.SetPixels(pix);
                        frameTex.Apply();

                        // save PNG file
                        var bytes = frameTex.EncodeToPNG();
                        DestroyImmediate(frameTex);
                        var outputPath = Path.Combine(outputDirectory, frame.name) + ".png";
                        File.WriteAllBytes(outputPath, bytes);
                    }
                } // if (importer)
            } // if (selectedTexture != null)
        } // foreach (Object activeObject in selectedTextures)
    }

    internal struct SheetFrame
    {
        public string name;
        public Vector2 pivot;
        public Rect rect;
    }
    

    [MenuItem("SpriteTextureSliceExporter/Export Slices")]
    public static void ExportSlices() {
//        var selectedTextures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
//        if (selectedTextures.Length == 0) {
//            EditorUtility.DisplayDialog("ImageSliceExporter", "Please select texture", "OK");
//            return;
//        }
        var outputDirectory = GetOutputDirectory();
//        var outputDirectory = Application.persistentDataPath;
        
        var assetPaths = Selection.assetGUIDs
            .Select(AssetDatabase.GUIDToAssetPath)
//            .Select(path => Path.ChangeExtension(path, null))
            .Select(AssetDatabase.LoadAllAssetsAtPath)
            .ToArray();
        Debug.Log($"ExportSlicesContext, len={assetPaths.Length} {string.Join(System.Environment.NewLine, (object[])assetPaths)}");
        foreach (var subassets in assetPaths) {
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
                var data = subtex.EncodeToPNG();
                var outPath = $"{outputDirectory}/{sprite.name}.png";
                File.WriteAllBytes(outPath, data);
                Debug.Log($"Wrote to '{outPath}'");
            }
        }
//        foreach (var texture in selectedTextures) {
//            var texture = Resources.Load<Texture2D>(assetPath);
//            Debug.Log($"ExportSlicesContext tex= {texture}");
//            var sprites = Resources.LoadAll<Sprite>(texture.name);
//            Debug.Log($"ExportSlicesContext sprites= {string.Join(System.Environment.NewLine, (object[])sprites)} length={sprites.Length}");
//            foreach (Sprite sprite in sprites) {
//                Debug.Log($"ExportSlicesContext inner loop, {sprite}");
//                var tex = sprite.texture;
//                var r = sprite.textureRect;
//                var subtex = tex.CropTexture( (int)r.x, (int)r.y, (int)r.width, (int)r.height );
//                var data = subtex.EncodeToPNG();
//                var outPath = $"{outputDirectory}/{sprite.name}.png";
//                File.WriteAllBytes(outPath, data);
//                Debug.Log($"Wrote to '{outPath}'");
//            }
//        }
//        var selectedTexture = Selection.activeObject as Texture2D;
//        selectedTexture.
    }
    
    [MenuItem("SpriteTextureSliceExporter/Export Slices", true)]
    public static bool ExportSlicesValidation() {
        return Selection.activeObject as Texture2D != null;
    }

}