using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

class Program
{
    static void Main(string[] args)
    {
        string bundlePath = @"C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\StreamingAssets\aa\StandaloneWindows64\felix_assets_all_8ce5cfff23e50dfcaa729aa03940bfd7.bundle";
        string backupPath = bundlePath + ".backup_before_fix";
        string tpkPath = @"D:\Endstar Plot Flag\PLOT FLAG SYSTEM\Tools\UABEA\classdata.tpk";

        Console.WriteLine("=== Felix Bundle Material Fix ===");
        Console.WriteLine($"Bundle: {bundlePath}");

        // Create backup
        if (!File.Exists(backupPath))
        {
            Console.WriteLine("Creating backup...");
            File.Copy(bundlePath, backupPath);
        }

        var am = new AssetsManager();
        am.LoadClassPackage(tpkPath);

        Console.WriteLine("Loading bundle...");
        var bun = am.LoadBundleFile(bundlePath);
        var afileInst = am.LoadAssetsFileFromBundle(bun, 0);
        var afile = afileInst.file;

        am.LoadClassDatabaseFromPackage(afile.Metadata.UnityVersion);

        Console.WriteLine("Searching for material...");

        List<AssetsReplacer> replacers = new List<AssetsReplacer>();

        foreach (var info in afile.AssetInfos)
        {
            if (info.TypeId != (int)AssetClassID.Material) continue;

            var baseField = am.GetBaseField(afileInst, info);
            var nameField = baseField["m_Name"];
            string matName = nameField.AsString;

            if (!matName.Contains("Felix") && !matName.Contains("ThePack")) continue;

            Console.WriteLine($"\nFound material: {matName}");

            // Get current values
            var renderQueue = baseField["m_CustomRenderQueue"];
            var shaderKeywords = baseField["m_ShaderKeywords"];
            var lightmapFlags = baseField["m_LightmapFlags"];

            Console.WriteLine($"  Before:");
            Console.WriteLine($"    m_CustomRenderQueue: {renderQueue.AsInt}");
            Console.WriteLine($"    m_ShaderKeywords: {shaderKeywords.AsString}");
            Console.WriteLine($"    m_LightmapFlags: {lightmapFlags.AsInt}");

            // Fix values
            renderQueue.AsInt = 2450;
            shaderKeywords.AsString = "_ALPHATEST_ON";
            lightmapFlags.AsInt = 1; // EmissiveIsBlack

            // Fix float properties in m_SavedProperties
            var savedProps = baseField["m_SavedProperties"];
            if (savedProps != null)
            {
                var floats = savedProps["m_Floats"]["Array"];
                if (floats != null)
                {
                    foreach (var floatPair in floats.Children)
                    {
                        var propName = floatPair["first"].AsString;
                        var propValue = floatPair["second"];

                        if (propName == "_AlphaClip")
                        {
                            Console.WriteLine($"    _AlphaClip: {propValue.AsFloat} -> 1");
                            propValue.AsFloat = 1.0f;
                        }
                        else if (propName == "_AlphaToMask")
                        {
                            Console.WriteLine($"    _AlphaToMask: {propValue.AsFloat} -> 1");
                            propValue.AsFloat = 1.0f;
                        }
                    }
                }
            }

            Console.WriteLine($"  After:");
            Console.WriteLine($"    m_CustomRenderQueue: {renderQueue.AsInt}");
            Console.WriteLine($"    m_ShaderKeywords: {shaderKeywords.AsString}");
            Console.WriteLine($"    m_LightmapFlags: {lightmapFlags.AsInt}");

            // Create replacer
            var newBytes = baseField.WriteToByteArray();
            var replacer = new AssetsReplacerFromMemory(afile, info, newBytes);
            replacers.Add(replacer);
            Console.WriteLine("  Changes queued!");
        }

        if (replacers.Count == 0)
        {
            Console.WriteLine("No materials found to fix!");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"\nSaving bundle with {replacers.Count} changes...");

        // Write modified assets
        using (var ms = new MemoryStream())
        using (var writer = new AssetsFileWriter(ms))
        {
            afile.Write(writer, 0, replacers);
            var newAssetData = ms.ToArray();

            // Write to new bundle file
            var bunRepl = new BundleReplacerFromMemory(afileInst.name, afileInst.name, true, newAssetData, newAssetData.Length);

            using (var fs = File.Create(bundlePath + ".new"))
            using (var bunWriter = new AssetsFileWriter(fs))
            {
                bun.file.Write(bunWriter, new List<BundleReplacer> { bunRepl });
            }
        }

        am.UnloadAll();

        // Replace original with new
        File.Delete(bundlePath);
        File.Move(bundlePath + ".new", bundlePath);

        Console.WriteLine("Bundle saved successfully!");
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
