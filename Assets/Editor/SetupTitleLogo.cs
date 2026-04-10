using UnityEngine;
using UnityEditor;

public class SetupTitleLogo
{
    public static void Execute()
    {
        string path = "Assets/Sprites/UI/TitleLogo.png";
        
        // Set texture import settings to Sprite
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            Debug.Log("TitleLogo texture import settings configured as Sprite.");
        }
        else
        {
            Debug.LogError("Could not find texture at: " + path);
        }
    }
}
