/**
 * script for export sprite sheet from Unity to Cocos2d
 *
 * @author bman (zx123xz321hm3@hotmail.com)
 * @version 1.0   1/3/2014
 * @version 1.1   6/9/2014   add image slice exporter
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class SpriteTextureSliceExporter : ScriptableObject {

	internal struct SheetFrame {
		public string name;
		public Vector2 pivot;
		public Rect rect;
	};

	static internal string Render (string assetPath, Texture2D texture, List<SheetFrame> frames) {
		StringBuilder sb = new StringBuilder();

		sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
		sb.AppendLine("<!DOCTYPE plist PUBLIC \"-//Apple Computer//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">");
		sb.AppendLine("<plist version=\"1.0\">");
		sb.AppendLine("    <dict>");

		sb.AppendLine("        <key>frames</key>");
		sb.AppendLine("        <dict>");
		foreach (SheetFrame frame in frames) {
			sb.AppendFormat("            <key>{0}</key>", 
				frame.name); sb.AppendLine();
			sb.AppendLine("            <dict>");
			sb.AppendLine("                <key>frame</key>");
			sb.AppendFormat("                <string>{{{{{0},{1}}},{{{2},{3}}}}}</string>", 
				frame.rect.x, frame.rect.y, frame.rect.width, frame.rect.height); sb.AppendLine();
			sb.AppendLine("                <key>offset</key>");
			sb.AppendLine("                <string>{0,0}</string>");
			sb.AppendLine("                <key>rotated</key>");
			sb.AppendLine("                <false/>");
			sb.AppendLine("                <key>sourceColorRect</key>");
			sb.AppendFormat("                <string>{{{{0,0}},{{{0},{1}}}}}</string>", 
				frame.rect.width, frame.rect.height); sb.AppendLine();
			sb.AppendLine("                <key>sourceSize</key>");
			sb.AppendFormat("                <string>{{{0},{1}}}</string>", 
				frame.rect.width, frame.rect.height); sb.AppendLine();
			sb.AppendLine("            </dict>");
		}
		sb.AppendLine("        </dict>");

		sb.AppendLine("        <key>metadata</key>");
		sb.AppendLine("        <dict>");
		sb.AppendLine("            <key>format</key>");
		sb.AppendLine("            <integer>2</integer>");
		sb.AppendLine("            <key>realTextureFileName</key>");
		sb.AppendFormat("            <string>{0}</string>", 
			Path.GetFileName(assetPath)); sb.AppendLine();
		sb.AppendLine("            <key>size</key>");
		sb.AppendFormat("            <string>{{{0},{1}}}</string>", 
			texture.width, texture.height); sb.AppendLine();
		sb.AppendLine("            <key>textureFileName</key>");
		sb.AppendFormat("            <string>{0}</string>", 
			Path.GetFileName(assetPath)); sb.AppendLine();
		sb.AppendLine("        </dict>");
		sb.AppendLine("    </dict>");
		sb.AppendLine("</plist>");

		return sb.ToString();
	}

	[MenuItem ("SpriteTextureSliceExporter/SpriteSheetExporter")]
	static public void SpriteSheetExporter () {
		Object[] selectedTextures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
		if (selectedTextures.Length == 0) {
			EditorUtility.DisplayDialog("SpriteSheetExporter", "Please select texture", "OK");
			return;
		}

		string outputDirectory = EditorUtility.OpenFolderPanel("Output of Directory", "", "");
		if (string.IsNullOrEmpty(outputDirectory)) {
			return;
		}

		foreach (Object activeObject in selectedTextures) {
			Texture2D selectedTexture = activeObject as Texture2D;
			if (selectedTexture != null) {
				string assetPath = AssetDatabase.GetAssetPath(selectedTexture);
				Debug.Log("export sprite sheet \"" + assetPath + "\"");
				TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
				if (importer) {
					List<SheetFrame> frames = new List<SheetFrame>();

					foreach (SpriteMetaData spriteMetaData in importer.spritesheet) {
						SheetFrame frame = new SheetFrame();

						frame.name = spriteMetaData.name;

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
					string outputPath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(assetPath)) + ".plist";
					string output = Render(assetPath, selectedTexture, frames);
					using (StreamWriter sw = File.CreateText(outputPath)) 
		            {
		                sw.Write(output);
		            }

		            // copy image to target path
					File.Copy(assetPath, Path.Combine(outputDirectory, Path.GetFileName(assetPath)), true);
				} // if (importer)
			} // if (selectedTexture != null)
		} // foreach (Object activeObject in selectedTextures)
	}

	[MenuItem ("SpriteTextureSliceExporter/ImageSliceExporter")]
	static public void ImageSliceExporter () {
		Object[] selectedTextures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
		if (selectedTextures.Length == 0) {
			EditorUtility.DisplayDialog("ImageSliceExporter", "Please select texture", "OK");
			return;
		}

		string outputDirectory = EditorUtility.OpenFolderPanel("Output of Directory", "", "");
		if (string.IsNullOrEmpty(outputDirectory)) {
			return;
		}

		foreach (Object activeObject in selectedTextures) {
			Texture2D selectedTexture = activeObject as Texture2D;
			if (selectedTexture != null) {
				string assetPath = AssetDatabase.GetAssetPath(selectedTexture);
				Debug.Log("export sprite sheet \"" + assetPath + "\"");
				TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
				if (importer) {
					List<SheetFrame> frames = new List<SheetFrame>();

					foreach (SpriteMetaData spriteMetaData in importer.spritesheet) {
						SheetFrame frame = new SheetFrame();

						frame.name = spriteMetaData.name;

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
					foreach (SheetFrame frame in frames) {
						// copy frame image data
						int x = Mathf.FloorToInt(frame.rect.x);
				        int y = Mathf.FloorToInt(frame.rect.y);
				        int width = Mathf.FloorToInt(frame.rect.width);
				        int height = Mathf.FloorToInt(frame.rect.height);
				        Color[] pix = selectedTexture.GetPixels(x, y, width, height);
				        Texture2D frameTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
				        frameTex.SetPixels(pix);
						frameTex.Apply();

						// save PNG file
						byte[] bytes = frameTex.EncodeToPNG();
						DestroyImmediate(frameTex);
						string outputPath = Path.Combine(outputDirectory, frame.name) + ".png";
						File.WriteAllBytes(outputPath, bytes);
					}
				} // if (importer)
			} // if (selectedTexture != null)
		} // foreach (Object activeObject in selectedTextures)
	}

}