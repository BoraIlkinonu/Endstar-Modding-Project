using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace GameCloneDiagnostics;

/// <summary>
/// Diagnostic plugin to research game cloning possibilities in Endstar.
///
/// Key findings from decompiled code:
/// - Game.Clone() exists: JsonConvert.DeserializeObject<Game>(JsonConvert.SerializeObject(this))
/// - CloudService.CreateAssetAsync() can create new games
/// - CloudService.GetAssetAsync() can fetch games by ID
/// - Games have AssetID, levels, GameLibrary (props, terrain, audio)
/// - No existing UI for clone/delete games
/// </summary>
[BepInPlugin("com.endstar.gameclone.diagnostics", "Game Clone Diagnostics", "1.0.0")]
public class GameCloneDiagnosticsPlugin : BaseUnityPlugin
{
    private static ManualLogSource Log;
    private Harmony _harmony;

    // Cached types
    private static Type _gameEditorType;
    private static Type _cloudServiceType;
    private static Type _gameType;
    private static Type _runtimeDatabaseType;
    private static Type _endlessServicesType;

    // Cached instances
    private static object _gameEditorInstance;
    private static object _cloudServiceInstance;

    // UI State
    private bool _showDiagWindow = false;
    private Vector2 _scrollPos;
    private string _diagnosticOutput = "";
    private string _gameListOutput = "";
    private bool _isGatheringInfo = false;

    private void Awake()
    {
        Log = Logger;
        Log.LogInfo("Game Clone Diagnostics Plugin v1.0.0 loading...");

        _harmony = new Harmony("com.endstar.gameclone.diagnostics");

        // Cache types at startup
        CacheTypes();

        Log.LogInfo("Game Clone Diagnostics Plugin loaded. Press F9 to open diagnostics window.");
    }

    private void CacheTypes()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (_gameEditorType == null)
                _gameEditorType = assembly.GetType("Endless.Creator.GameEditor");

            if (_cloudServiceType == null)
                _cloudServiceType = assembly.GetType("Endless.Matchmaking.EndlessCloudService");

            if (_gameType == null)
                _gameType = assembly.GetType("Endless.Gameplay.LevelEditing.Level.Game");

            if (_runtimeDatabaseType == null)
                _runtimeDatabaseType = assembly.GetType("Endless.Gameplay.RuntimeDatabase");

            if (_endlessServicesType == null)
                _endlessServicesType = assembly.GetType("Endless.Matchmaking.EndlessServices");
        }

        Log.LogInfo($"[CACHE] GameEditor: {_gameEditorType != null}");
        Log.LogInfo($"[CACHE] CloudService: {_cloudServiceType != null}");
        Log.LogInfo($"[CACHE] Game: {_gameType != null}");
        Log.LogInfo($"[CACHE] RuntimeDatabase: {_runtimeDatabaseType != null}");
        Log.LogInfo($"[CACHE] EndlessServices: {_endlessServicesType != null}");
    }

    private void Update()
    {
        // Toggle diagnostics window with F9
        if (Input.GetKeyDown(KeyCode.F9))
        {
            _showDiagWindow = !_showDiagWindow;
            if (_showDiagWindow)
            {
                GatherDiagnosticInfo();
            }
        }
    }

    private void OnGUI()
    {
        if (!_showDiagWindow) return;

        // Main diagnostic window
        GUI.Window(9999, new Rect(50, 50, 600, 500), DrawDiagWindow, "Game Clone Diagnostics (F9 to close)");
    }

    private void DrawDiagWindow(int windowId)
    {
        GUILayout.BeginVertical();

        // Button row
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Refresh Info", GUILayout.Width(120)))
        {
            GatherDiagnosticInfo();
        }

        if (GUILayout.Button("List User Games", GUILayout.Width(120)))
        {
            StartCoroutine(ListUserGamesCoroutine());
        }

        if (GUILayout.Button("Dump Active Game", GUILayout.Width(120)))
        {
            DumpActiveGame();
        }

        if (GUILayout.Button("Test Clone API", GUILayout.Width(120)))
        {
            TestCloneAPI();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Status
        if (_isGatheringInfo)
        {
            GUILayout.Label("Gathering info...", GUILayout.Height(20));
        }

        // Scroll view for output
        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(400));

        GUILayout.Label(_diagnosticOutput);

        if (!string.IsNullOrEmpty(_gameListOutput))
        {
            GUILayout.Space(10);
            GUILayout.Label("=== User Games ===");
            GUILayout.Label(_gameListOutput);
        }

        GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUI.DragWindow();
    }

    private void GatherDiagnosticInfo()
    {
        _diagnosticOutput = "=== Game Clone Diagnostics ===\n\n";

        // Check GameEditor
        _diagnosticOutput += "--- GameEditor Status ---\n";
        try
        {
            if (_gameEditorType != null)
            {
                var instanceProp = _gameEditorType.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                if (instanceProp != null)
                {
                    _gameEditorInstance = instanceProp.GetValue(null);
                    _diagnosticOutput += $"GameEditor.Instance: {(_gameEditorInstance != null ? "FOUND" : "NULL")}\n";

                    if (_gameEditorInstance != null)
                    {
                        // Get ActiveGame
                        var activeGameProp = _gameEditorType.GetProperty("ActiveGame");
                        if (activeGameProp != null)
                        {
                            var activeGame = activeGameProp.GetValue(_gameEditorInstance);
                            if (activeGame != null)
                            {
                                _diagnosticOutput += $"ActiveGame: FOUND\n";
                                DumpGameProperties(activeGame, "  ");
                            }
                            else
                            {
                                _diagnosticOutput += $"ActiveGame: NULL (not in editor?)\n";
                            }
                        }
                    }
                }
            }
            else
            {
                _diagnosticOutput += "GameEditor type not found\n";
            }
        }
        catch (Exception ex)
        {
            _diagnosticOutput += $"Error accessing GameEditor: {ex.Message}\n";
        }

        // Check RuntimeDatabase
        _diagnosticOutput += "\n--- RuntimeDatabase Status ---\n";
        try
        {
            if (_runtimeDatabaseType != null)
            {
                var activeGameProp = _runtimeDatabaseType.GetProperty("ActiveGame",
                    BindingFlags.Public | BindingFlags.Static);

                if (activeGameProp != null)
                {
                    var activeGame = activeGameProp.GetValue(null);
                    if (activeGame != null)
                    {
                        _diagnosticOutput += "RuntimeDatabase.ActiveGame: FOUND\n";
                        DumpGameProperties(activeGame, "  ");
                    }
                    else
                    {
                        _diagnosticOutput += "RuntimeDatabase.ActiveGame: NULL\n";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _diagnosticOutput += $"Error accessing RuntimeDatabase: {ex.Message}\n";
        }

        // Check CloudService
        _diagnosticOutput += "\n--- CloudService Status ---\n";
        try
        {
            if (_endlessServicesType != null)
            {
                var instanceProp = _endlessServicesType.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static);

                if (instanceProp != null)
                {
                    var servicesInstance = instanceProp.GetValue(null);
                    if (servicesInstance != null)
                    {
                        var cloudProp = _endlessServicesType.GetProperty("CloudService");
                        if (cloudProp != null)
                        {
                            _cloudServiceInstance = cloudProp.GetValue(servicesInstance);
                            _diagnosticOutput += $"CloudService: {(_cloudServiceInstance != null ? "FOUND" : "NULL")}\n";

                            if (_cloudServiceInstance != null)
                            {
                                // List available methods
                                var methods = _cloudServiceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                                _diagnosticOutput += "Key Methods:\n";
                                foreach (var method in methods)
                                {
                                    if (method.Name.Contains("Asset") || method.Name.Contains("Game"))
                                    {
                                        _diagnosticOutput += $"  - {method.Name}\n";
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _diagnosticOutput += $"Error accessing CloudService: {ex.Message}\n";
        }

        // Clone capability
        _diagnosticOutput += "\n--- Clone Capability ---\n";
        if (_gameType != null)
        {
            var cloneMethod = _gameType.GetMethod("Clone", BindingFlags.Public | BindingFlags.Instance);
            _diagnosticOutput += $"Game.Clone() method: {(cloneMethod != null ? "FOUND" : "NOT FOUND")}\n";
        }

        Log.LogInfo("[DIAG] Diagnostic info gathered");
    }

    private void DumpGameProperties(object game, string prefix)
    {
        try
        {
            // Try to get Name via property first
            var nameProp = game.GetType().GetProperty("Name");
            if (nameProp != null)
            {
                _diagnosticOutput += $"{prefix}Name: {nameProp.GetValue(game)}\n";
            }
            else
            {
                // Fallback to field
                var nameField = game.GetType().GetField("Name");
                if (nameField != null)
                {
                    _diagnosticOutput += $"{prefix}Name: {nameField.GetValue(game)}\n";
                }
            }

            var assetIdProp = game.GetType().GetProperty("AssetID");
            if (assetIdProp != null)
            {
                _diagnosticOutput += $"{prefix}AssetID: {assetIdProp.GetValue(game)}\n";
            }

            var versionProp = game.GetType().GetProperty("AssetVersion");
            if (versionProp != null)
            {
                _diagnosticOutput += $"{prefix}AssetVersion: {versionProp.GetValue(game)}\n";
            }

            var levelsField = game.GetType().GetField("levels");
            if (levelsField != null)
            {
                var levels = levelsField.GetValue(game) as System.Collections.IList;
                _diagnosticOutput += $"{prefix}Levels: {(levels != null ? levels.Count.ToString() : "?")}\n";
            }
        }
        catch (Exception ex)
        {
            _diagnosticOutput += $"{prefix}Error dumping game: {ex.Message}\n";
        }
    }

    private void DumpActiveGame()
    {
        _diagnosticOutput = "=== Active Game Dump ===\n\n";

        try
        {
            object activeGame = null;

            // Try RuntimeDatabase first
            if (_runtimeDatabaseType != null)
            {
                var prop = _runtimeDatabaseType.GetProperty("ActiveGame", BindingFlags.Public | BindingFlags.Static);
                if (prop != null)
                {
                    activeGame = prop.GetValue(null);
                }
            }

            // Try GameEditor
            if (activeGame == null && _gameEditorType != null)
            {
                var instanceProp = _gameEditorType.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (instanceProp != null)
                {
                    var instance = instanceProp.GetValue(null);
                    if (instance != null)
                    {
                        var gameProp = _gameEditorType.GetProperty("ActiveGame");
                        if (gameProp != null)
                        {
                            activeGame = gameProp.GetValue(instance);
                        }
                    }
                }
            }

            if (activeGame != null)
            {
                _diagnosticOutput += "Game found! Dumping all fields...\n\n";

                // Dump all fields
                foreach (var field in activeGame.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    try
                    {
                        var value = field.GetValue(activeGame);
                        _diagnosticOutput += $"{field.Name}: {value}\n";
                    }
                    catch
                    {
                        _diagnosticOutput += $"{field.Name}: <error>\n";
                    }
                }

                // Dump all properties
                _diagnosticOutput += "\nProperties:\n";
                foreach (var prop in activeGame.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    try
                    {
                        if (prop.CanRead)
                        {
                            var value = prop.GetValue(activeGame);
                            _diagnosticOutput += $"{prop.Name}: {value}\n";
                        }
                    }
                    catch
                    {
                        _diagnosticOutput += $"{prop.Name}: <error>\n";
                    }
                }
            }
            else
            {
                _diagnosticOutput += "No active game found.\n";
                _diagnosticOutput += "Make sure you're in the Creator mode with a game loaded.\n";
            }
        }
        catch (Exception ex)
        {
            _diagnosticOutput += $"Error: {ex.Message}\n{ex.StackTrace}\n";
        }
    }

    private IEnumerator ListUserGamesCoroutine()
    {
        _isGatheringInfo = true;
        _gameListOutput = "Fetching user games...\n";

        yield return new WaitForSeconds(0.1f);

        try
        {
            // This would need the CloudService to actually query
            // For now, just log what we'd need to do
            _gameListOutput = "To list user games, we would call:\n";
            _gameListOutput += "CloudService.GetAssetsByTypeAsync(\"game\", ...)\n\n";
            _gameListOutput += "This requires proper authentication context.\n";
            _gameListOutput += "The game uses GraphQL queries to the Endless cloud.\n\n";

            if (_cloudServiceInstance != null)
            {
                _gameListOutput += "CloudService is available - could potentially call API.\n";

                // List the methods we could call
                var methods = _cloudServiceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                _gameListOutput += "\nAvailable methods:\n";
                foreach (var m in methods)
                {
                    if (m.Name.StartsWith("Get") && m.Name.Contains("Asset"))
                    {
                        _gameListOutput += $"  {m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name))})\n";
                    }
                }
            }
            else
            {
                _gameListOutput += "CloudService not available (may need to be in lobby).\n";
            }
        }
        catch (Exception ex)
        {
            _gameListOutput = $"Error: {ex.Message}\n";
        }

        _isGatheringInfo = false;
    }

    private void TestCloneAPI()
    {
        _diagnosticOutput = "=== Clone API Test ===\n\n";

        try
        {
            // Get active game
            object activeGame = null;

            if (_runtimeDatabaseType != null)
            {
                var prop = _runtimeDatabaseType.GetProperty("ActiveGame", BindingFlags.Public | BindingFlags.Static);
                if (prop != null)
                {
                    activeGame = prop.GetValue(null);
                }
            }

            if (activeGame != null)
            {
                _diagnosticOutput += "Found active game. Testing Clone()...\n\n";

                // Get Clone method
                var cloneMethod = activeGame.GetType().GetMethod("Clone", BindingFlags.Public | BindingFlags.Instance);

                if (cloneMethod != null)
                {
                    _diagnosticOutput += "Clone() method found!\n";

                    // Actually call Clone
                    var clonedGame = cloneMethod.Invoke(activeGame, null);

                    if (clonedGame != null)
                    {
                        _diagnosticOutput += "Clone successful!\n\n";

                        // Compare original and clone
                        var nameProp = activeGame.GetType().GetProperty("Name");
                        var assetIdProp = activeGame.GetType().GetProperty("AssetID");

                        _diagnosticOutput += "Original:\n";
                        _diagnosticOutput += $"  Name: {nameProp?.GetValue(activeGame)}\n";
                        _diagnosticOutput += $"  AssetID: {assetIdProp?.GetValue(activeGame)}\n";

                        _diagnosticOutput += "\nClone:\n";
                        _diagnosticOutput += $"  Name: {nameProp?.GetValue(clonedGame)}\n";
                        _diagnosticOutput += $"  AssetID: {assetIdProp?.GetValue(clonedGame)}\n";

                        _diagnosticOutput += "\nNOTE: Clone has SAME AssetID!\n";
                        _diagnosticOutput += "To create a true copy, you would need to:\n";
                        _diagnosticOutput += "1. Call Clone()\n";
                        _diagnosticOutput += "2. Generate new AssetID (GUID)\n";
                        _diagnosticOutput += "3. Call CloudService.CreateAssetAsync()\n";
                    }
                    else
                    {
                        _diagnosticOutput += "Clone returned null!\n";
                    }
                }
                else
                {
                    _diagnosticOutput += "Clone() method not found!\n";
                }
            }
            else
            {
                _diagnosticOutput += "No active game to test with.\n";
                _diagnosticOutput += "Load a game in Creator mode first.\n";
            }
        }
        catch (Exception ex)
        {
            _diagnosticOutput += $"Error: {ex.Message}\n{ex.StackTrace}\n";
        }
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }
}
