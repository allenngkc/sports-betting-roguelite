using System;
using UnityEditor;
using UnityEngine;

namespace SBR.Editor
{
    /// <summary>Keeps the generated SureThing annotation sprites on their documented UI import contract.</summary>
    internal sealed class SureThingInkImporter : AssetPostprocessor
    {
        private const string InkRoot = "Assets/SBR/Resources/SureThing/Ink/";

        private void OnPreprocessTexture()
        {
            if (!IsDirectInkPng(assetPath)) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);

            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = false;

            SetUncompressed(importer, "DefaultTexturePlatform");
            SetUncompressed(importer, "Standalone");
            SetUncompressed(importer, "WebGL");
            // Deliberately no ImportAsset/SaveAndReimport call: these settings apply to the
            // import already in progress, so the postprocessor cannot create a reimport loop.
        }

        private static bool IsDirectInkPng(string path)
        {
            if (!path.StartsWith(InkRoot, StringComparison.Ordinal)
                || !path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                return false;
            return path.IndexOf('/', InkRoot.Length) < 0;
        }

        private static void SetUncompressed(TextureImporter importer, string platform)
        {
            TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platform);
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            settings.crunchedCompression = false;
            importer.SetPlatformTextureSettings(settings);
        }
    }
}
