using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SBR.Tests.EditMode
{
    public class SureThingInkAssetTests
    {
        private const string InkRoot = "Assets/SBR/Resources/SureThing/Ink/";

        private static readonly string[] InkAssets =
        {
            InkRoot + "ring-price-a@2x.png",
            InkRoot + "ring-price-b@2x.png",
            InkRoot + "ring-price-c@2x.png",
            InkRoot + "ring-wide-a@2x.png",
            InkRoot + "ring-wide-b@2x.png",
            InkRoot + "strike-a@2x.png",
        };

        private static readonly IReadOnlyDictionary<string, string> ApprovedHashes =
            new Dictionary<string, string>
            {
                [InkRoot + "ring-wide-a@2x.png"] =
                    "F71F949D879DA756630591A7E85A5912D0E8A9BF54046694F30363BCF70B3B31",
                [InkRoot + "ring-wide-b@2x.png"] =
                    "0B0267A0E7CFFBEF2D08B6179C8145743161E3F4EF7405AA146BC84395555905",
                [InkRoot + "strike-a@2x.png"] =
                    "580C04B2494C71CD16AC350B1D436EC531E3FFF22CD2FC497702671A1F1F4A0D",
            };

        [Test]
        public void SureThing_ink_assets_match_the_approved_files_and_import_contract()
        {
            var guids = new HashSet<string>(StringComparer.Ordinal);

            foreach (string assetPath in InkAssets)
            {
                string absolutePath = ProjectAbsolutePath(assetPath);
                Assert.IsTrue(File.Exists(absolutePath), $"{assetPath}: source PNG is missing");

                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                Assert.IsNotEmpty(guid, $"{assetPath}: AssetDatabase GUID is missing");
                Assert.IsTrue(guids.Add(guid), $"{assetPath}: duplicate GUID {guid}");

                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                Assert.IsNotNull(importer, $"{assetPath}: TextureImporter is missing");
                Assert.AreEqual(TextureImporterType.Sprite, importer.textureType,
                    $"{assetPath}: texture type");
                Assert.AreEqual(SpriteImportMode.Single, importer.spriteImportMode,
                    $"{assetPath}: sprite import mode");

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                Assert.AreEqual(SpriteMeshType.FullRect, settings.spriteMeshType,
                    $"{assetPath}: sprite mesh type");

                Assert.IsTrue(importer.alphaIsTransparency, $"{assetPath}: alpha transparency");
                Assert.IsFalse(importer.mipmapEnabled, $"{assetPath}: mipmaps");
                Assert.AreEqual(TextureWrapMode.Clamp, importer.wrapModeU, $"{assetPath}: wrap U");
                Assert.AreEqual(TextureWrapMode.Clamp, importer.wrapModeV, $"{assetPath}: wrap V");
                Assert.AreEqual(TextureWrapMode.Clamp, importer.wrapModeW, $"{assetPath}: wrap W");
                Assert.AreEqual(FilterMode.Bilinear, importer.filterMode, $"{assetPath}: filtering");
                Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression,
                    $"{assetPath}: compression");
                Assert.IsFalse(importer.isReadable, $"{assetPath}: read/write must be disabled");

                if (ApprovedHashes.TryGetValue(assetPath, out string approved))
                    Assert.AreEqual(approved, Sha256(absolutePath), $"{assetPath}: SHA-256");
            }

            Assert.AreEqual(InkAssets.Length, guids.Count, "Every SureThing ink asset needs a unique GUID");
        }

        private static string ProjectAbsolutePath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static string Sha256(string path)
        {
            using (SHA256 hash = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }
}
