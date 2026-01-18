using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Unity Editor script to build the FirstPickupInfo asset bundle.
/// Place this script in Assets/EndstarPopupBundle/Editor/ folder.
/// </summary>
public class BuildPopupBundle
{
    private const string BUNDLE_NAME = "firstpickupinfo.bundle";
    private const string OUTPUT_PATH = "Assets/Bundles";
    private const string PREFAB_PATH = "Assets/EndstarPopupBundle/Prefabs/FirstPickupInfoPopup.prefab";

    [MenuItem("EndstarMod/Build Popup Bundle")]
    public static void BuildBundle()
    {
        // Ensure output directory exists
        if (!Directory.Exists(OUTPUT_PATH))
        {
            Directory.CreateDirectory(OUTPUT_PATH);
        }

        // Mark prefab for bundle
        AssetImporter prefabImporter = AssetImporter.GetAtPath(PREFAB_PATH);
        if (prefabImporter == null)
        {
            Debug.LogError($"Prefab not found at: {PREFAB_PATH}");
            return;
        }
        prefabImporter.assetBundleName = BUNDLE_NAME;

        // Also include sprites
        string[] spritePaths = new string[]
        {
            "Assets/EndstarPopupBundle/Sprites/CloseButtonIcon.png",
            "Assets/EndstarPopupBundle/Sprites/PanelBackground.png",
            "Assets/EndstarPopupBundle/Sprites/DefaultItemImage.png"
        };

        foreach (string path in spritePaths)
        {
            if (File.Exists(path))
            {
                AssetImporter importer = AssetImporter.GetAtPath(path);
                if (importer != null)
                {
                    importer.assetBundleName = BUNDLE_NAME;
                }
            }
            else
            {
                Debug.LogWarning($"Sprite not found: {path}");
            }
        }

        // Build for Windows
        BuildPipeline.BuildAssetBundles(
            OUTPUT_PATH,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows64
        );

        Debug.Log($"Bundle built successfully to: {OUTPUT_PATH}/{BUNDLE_NAME}");

        // Open folder
        EditorUtility.RevealInFinder($"{OUTPUT_PATH}/{BUNDLE_NAME}");
    }

    [MenuItem("EndstarMod/Build Popup Bundle", true)]
    public static bool ValidateBuildBundle()
    {
        return File.Exists(PREFAB_PATH);
    }

    [MenuItem("EndstarMod/Create Popup Prefab Structure")]
    public static void CreatePrefabStructure()
    {
        // Create necessary folders
        string[] folders = new string[]
        {
            "Assets/EndstarPopupBundle",
            "Assets/EndstarPopupBundle/Editor",
            "Assets/EndstarPopupBundle/Prefabs",
            "Assets/EndstarPopupBundle/Sprites",
            "Assets/Bundles"
        };

        foreach (string folder in folders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string[] parts = folder.Split('/');
                string currentPath = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string parentPath = currentPath;
                    currentPath = $"{currentPath}/{parts[i]}";
                    if (!AssetDatabase.IsValidFolder(currentPath))
                    {
                        AssetDatabase.CreateFolder(parentPath, parts[i]);
                    }
                }
            }
        }

        Debug.Log("Created folder structure for EndstarPopupBundle");
        Debug.Log("Next steps:");
        Debug.Log("1. Create sprites: CloseButtonIcon.png, PanelBackground.png, DefaultItemImage.png");
        Debug.Log("2. Place them in Assets/EndstarPopupBundle/Sprites/");
        Debug.Log("3. Create the FirstPickupInfoPopup prefab in Assets/EndstarPopupBundle/Prefabs/");
        Debug.Log("4. Run 'EndstarMod > Build Popup Bundle' to generate the asset bundle");

        AssetDatabase.Refresh();
    }
}
