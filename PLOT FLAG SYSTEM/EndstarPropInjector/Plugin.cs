using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

namespace EndstarPropInjector;

[BepInPlugin("com.endstar.propinjector", "Endstar Prop Injector", "10.66.0")]
public class Plugin : BaseUnityPlugin
{
	private enum SwimState
	{
		SURFACE,
		ENTERING,
		SWIMMING,
		EXITING
	}

	private class LoadedPropDefinition
	{
		public string PropId;

		public string DisplayName;

		public GameObject VisualPrefab;

		public Sprite Icon;

		public string BaseType;

		public override string ToString()
		{
			return DisplayName + " (" + PropId + ")";
		}
	}

	private static SwimState _currentSwimState = SwimState.SURFACE;

	private static int _exitGraceFrames = 0;

	private const int EXIT_GRACE_DURATION = 30;

	private const float SURFACE_MARGIN = 0.1f;

	private const float SUBMERSION_DEPTH = 0.5f;

	private const float JUMP_FORCE = 2.5f;

	private const float TELEPORT_THRESHOLD = 5f;

	private const float AUTO_SINK_FORCE = -2f;

	private static Vector3 _lastKnownPosition = Vector3.zero;

	private static bool _positionInitialized = false;

	private static int _landingResetFrames = 0;

	private const int LANDING_RESET_DURATION = 5;

	private static int _animLogCounter = 0;

	private static bool _lastGroundedState = true;

	private static SwimState _lastLoggedSwimState = SwimState.SURFACE;

	private static bool _fallAnimLogged = false;

	private static float _lastFallTime = 0f;

	private static Animator _cachedAnimator = null;

	private static bool _animatorCached = false;

	private static bool _lastDiveState = false;

	private static int _diveLogThrottle = 0;

	private static bool _animatorStatesLogged = false;

	private static readonly int HASH_ENTER_WATER = -1233582989;

	private static readonly int HASH_ENTER_WATER_HIGH = -1354818617;

	private static readonly int HASH_ABOVE_WATER_IDLE = 1260217858;

	private static readonly int HASH_ABOVE_WATER_SWIM_START = 1966917648;

	private static readonly int HASH_ABOVE_WATER_SWIM_STOP = 697086002;

	private static readonly int HASH_ABOVE_WATER_SWIM_BLEND = -547799254;

	private static readonly int HASH_DIVE = 914991998;

	private static readonly int HASH_BELOW_WATER_IDLE = 1443112800;

	private static readonly int HASH_BELOW_WATER_SWIM_START = 1820082062;

	private static readonly int HASH_BELOW_WATER_SWIM_STOP = 88956836;

	private static readonly int HASH_BELOW_WATER_SWIM_BLEND = -1503712467;

	private static readonly int HASH_BREACH = 940887619;

	private static readonly int PARAM_WATER_IN = 1691914667;

	private static readonly int PARAM_WATER_IN_BELOW = 86030555;

	private static readonly int PARAM_WATER_BELOW = 119971305;

	private static readonly int PARAM_WATER_ENTER = -88659915;

	private static readonly int PARAM_WATER_EXIT = 973649254;

	private static readonly int PARAM_WATER_DIVE = 951727400;

	private static readonly int PARAM_WATER_BREACH = -1019050783;

	private static bool _wasInWater = false;

	private static bool _wasUnderwater = false;

	private static bool _isMovingInWater = false;

	private static float _smoothedVelY = 0f;

	private const float VELY_LERP_SPEED = 3f;

	private static int _fallStateHash = 0;

	private static int _idleStateHash = 0;

	private static bool _idleHashCaptured = false;

	private static bool _fallHashCaptured = false;

	private static int _lastCapturedGroundedHash = 0;

	private static int _framesSinceGrounded = 0;

	private static float _lastDiagnosticTime = 0f;

	private static int _lastStateHash = 0;

	private static bool _wasDivingForDiag = false;

	private static int _jumpImmunityFrames = 0;

	private const int JUMP_IMMUNITY_DURATION = 15;

	public static ManualLogSource Log;

	public static Plugin Instance;

	private Harmony _harmony;

	public static Dictionary<string, object> CustomBaseTypes = new Dictionary<string, object>();

	public static Dictionary<string, object> CustomProps = new Dictionary<string, object>();

	public static readonly string CustomPropGuid = "11111111-1111-1111-1111-111111111111";

	public static readonly string CustomBaseTypeGuid = "22222222-2222-2222-2222-222222222222";

	private static AssetBundle _customPropsBundle;

	private static GameObject _loadedPrefab;

	private static Sprite _loadedIcon;

	private static Dictionary<string, LoadedPropDefinition> _loadedPropDefinitions = new Dictionary<string, LoadedPropDefinition>();

	private static Type _baseTypeDefinitionType;

	private static Type _componentDefinitionType;

	private static Type _propType;

	private static Type _serializableGuidType;

	private static Type _stageManagerType;

	private static Type _abstractComponentListBaseType;

	private static Type _endlessVisualsType;

	private static Type _endlessPropType;

	private static Type _propBasedToolType;

	private static Type _propLibraryType;

	private static Type _playerControllerType;

	private static Type _depthPlaneType;

	private static Type _appearanceAnimatorType;

	private static Type _filterType;

	private static Type _playerReferenceManagerType;

	private static Type _netStateType;

	private static Type _itemType;

	private static Type _inventoryType;

	private static Type _itemInteractableType;

	private static Type _inventorySlotType;

	private static Type _inventoryUsableDefinitionType;

	private static Type _treasureItemType;

	private static Type _genericUsableDefinitionType;

	private static Type _interactableBaseType;

	private static object _cachedTreasureDefinition;

	private const int PLAYER_INTERACTABLE_LAYER = 11;

	private static Type _simpleYawSpinType;

	private static Type _usableDefinitionType;

	private static object _customUsableDefinition;

	private const int CUSTOM_BUNDLE_FILE_INSTANCE_ID = -9999;

	private const string CUSTOM_PREFAB_ASSET_ID = "33333333-3333-3333-3333-333333333333";

	private const string CUSTOM_PREFAB_VERSION = "1.0.0";

	private static Type _endlessPrefabAssetType;

	private static Type _fileAssetInstanceType;

	private static Type _assetReferenceType;

	private static Type _loadedFileManagerType;

	private static FieldInfo _prefabBundleField;

	private static FieldInfo _loadedFilesField;

	private static object _customPrefabAssetReference;

	private static GameObject _customPrefab;

	private static GameObject _visualPrefab;

	private static object _customBaseTypeDefinition;

	private static GameObject _emptyVisualPlaceholder;

	public static bool IsPlayerUnderwater = false;

	public static bool IsSwimmingActive = false;

	public static float UnderwaterDepth = 0f;

	public static float SwimmingVerticalSpeed = 5f;

	public static float DepthPlaneY = float.MinValue;

	public static float WaterSurfaceY = float.MinValue;

	public static float WaterEntryY = float.MaxValue;

	public static float SwimActivationDepth = 1f;

	public static float MaxSwimDepth = 10f;

	public static float SurfaceStopOffset = 0.5f;

	public static float WaterEntryBuffer = 0.5f;

	public static float WaterExitBuffer = 1f;

	private static int _swimVerticalMotor = 0;

	private static int _swimMotorFrames = 8;

	private static int _swimMotorDegradeRate = 2;

	private static object _cachedPlayerController = null;

	private static bool _swimLoggedOnce = false;

	private static FieldInfo _currentStateField = null;

	private static FieldInfo _totalForceField = null;

	private static FieldInfo _calculatedMotionField = null;

	private static FieldInfo _blockGravityField = null;

	private static FieldInfo _framesSinceStableGroundField = null;

	private static bool _fieldsInitialized = false;

	private static bool _blurReductionApplied = false;

	private static Dictionary<object, float> _originalVolumeWeights = new Dictionary<object, float>();

	private static bool _volumeDiagnosticsLogged = false;

	private static bool _itemPickupDiagnosticsLogged = false;

	private static bool _shaderAlreadyModified = false;

	private static bool _swimPhysicsLogged = false;

	private static bool _wasSwimmingLastFrame = false;

	private static float _waterExitCooldown = 0f;

	private static float _lastExitTime = 0f;

	private static CharacterController _cachedCharController = null;

	private static float _characterHeight = 2f;

	private static float _characterRadius = 0.5f;

	private static bool _charControllerCached = false;

	private static object _cachedStageManager = null;

	private static PropertyInfo _cachedActiveStageProperty = null;

	private static PropertyInfo _cachedFallOffHeightProperty = null;

	private static bool _cacheInitialized = false;

	private static bool _waterDebugLogged = false;

	private static object _cachedDepthPlane = null;

	private static bool _filterLoggedOnce = false;

	private const float UNDERWATER_BLUR_INTENSITY = 0.35f;

	private static readonly Color UNDERWATER_TINT = new Color(0.4f, 0.7f, 0.9f, 1f);

	private static object _originalColorFilter = null;

	private static bool _colorFilterModified = false;

	private static object _cachedColorAdjustments = null;

	private static FieldInfo _colorFilterField = null;

	private static Type _networkManagerType;

	private static Type _networkObjectType;

	private static bool _networkTypesInitialized = false;

	private static Type _stageType;

	private static HashSet<string> _loggedPropTypes = new HashSet<string>();

	private static HashSet<string> _registeredDefinitionGuids = new HashSet<string>();

	private static GameObject _pearlBasketVisualPrefab;

	private static bool _collectibleResearchDone = false;

	// ============================================================================
	// FIRST PICKUP INFO POPUP SYSTEM - Fields
	// ============================================================================

	// Configuration
	private static ItemInfoConfig _itemInfoConfig;
	private static string _popupConfigPath;
	private static string _itemImagesPath;

	// Runtime State
	private static HashSet<string> _sessionCollectedItems = new HashSet<string>();
	private static GameObject _activePopupInstance;
	private static Canvas _popupCanvas;

	// UI References (populated when popup is shown)
	private static RawImage _popupItemImage;
	private static TextMeshProUGUI _popupTitleText;
	private static TextMeshProUGUI _popupDescriptionText;
	private static Button _popupCloseButton;
	private static CanvasGroup _popupCanvasGroup;

	private void Awake()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		Instance = this;
		Log = Logger;
		Log.LogInfo((object)"===========================================");
		Log.LogInfo((object)"[INJECTOR] Plugin v10.53.0 - MULTI-PROP FIX (per-GUID registration)");
		Log.LogInfo((object)"[INJECTOR] Search log for these tags to debug issues:");
		Log.LogInfo((object)"[INJECTOR]   [DIAG-ICON] - Icon/sprite loading details");
		Log.LogInfo((object)"[INJECTOR]   [DIAG-ROTATION] - Transform hierarchy for rotation");
		Log.LogInfo((object)"[INJECTOR]   [DIAG-REGISTER] - RuntimeDatabase registration");
		Log.LogInfo((object)"[INJECTOR]   [DIAG-EQUIP] - Inventory equip attempts");
		Log.LogInfo((object)"[INJECTOR]   [DIAG-DB] - RuntimeDatabase GUID lookup");
		Log.LogInfo((object)"===========================================");
		try
		{
			_harmony = new Harmony("com.endstar.propinjector");
			CacheTypes();
			InitializeFieldReferences();
			PatchStageManager();
			PatchEndlessPropBuildPrefab();
			PatchSpawnPreview();
			PatchFindAndManageChildRenderers();
			PatchProcessFallOffStage();
			PatchProcessPhysicsNetFrame();
			PatchProcessJumpPrefix();
			PatchAppearanceAnimator();
			PatchDepthPlaneStart();
			PatchFilterStartTransition();
			PatchItemPickup();
			PatchInventoryAttemptPickup();
			PatchItemInteractable();
			PatchItemComponentInitialize();
			PatchToggleLocalVisibility();
			PatchGetAssetBundleAsync();
			PatchInventoryEquipSlotDiagnostic();  // DIAGNOSTIC: Log equip attempts
			Log.LogInfo((object)"[INJECTOR] All patches applied successfully");

			// Initialize popup system
			InitializePopupSystem();
		}
		catch (Exception arg)
		{
			Log.LogError((object)$"[INJECTOR] Failed to initialize: {arg}");
		}
	}

	// ============================================================================
	// FIRST PICKUP INFO POPUP SYSTEM - Initialization
	// ============================================================================

	private void InitializePopupSystem()
	{
		try
		{
			// Set up paths relative to plugin location
			string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			_popupConfigPath = Path.Combine(pluginDir, "Config", "ItemInfoConfig.json");
			_itemImagesPath = Path.Combine(pluginDir, "ItemImages");

			Log.LogInfo((object)"[POPUP] Initializing popup system...");
			Log.LogInfo((object)$"[POPUP] Config path: {_popupConfigPath}");
			Log.LogInfo((object)$"[POPUP] Images path: {_itemImagesPath}");

			// Load configuration
			LoadItemInfoConfig();

			Log.LogInfo((object)"[POPUP] Popup system initialized (UI built in code, no asset bundle needed)");
		}
		catch (Exception ex)
		{
			Log.LogError((object)$"[POPUP] Failed to initialize popup system: {ex.Message}");
		}
	}

	private static void LoadItemInfoConfig()
	{
		try
		{
			if (File.Exists(_popupConfigPath))
			{
				string json = File.ReadAllText(_popupConfigPath);
				Log.LogInfo((object)$"[POPUP] Raw JSON ({json.Length} chars): {json.Substring(0, Math.Min(200, json.Length))}...");

				_itemInfoConfig = JsonUtility.FromJson<ItemInfoConfig>(json);

				// Unity's JsonUtility can fail silently on Lists - check and log
				if (_itemInfoConfig == null)
				{
					Log.LogWarning((object)"[POPUP] JsonUtility returned null, trying manual parse");
					_itemInfoConfig = new ItemInfoConfig { items = new List<ItemInfoEntry>() };
					ManualParseConfig(json);
				}
				else if (_itemInfoConfig.items == null || _itemInfoConfig.items.Count == 0)
				{
					Log.LogWarning((object)"[POPUP] JsonUtility parsed but items empty, trying manual parse");
					_itemInfoConfig.items = new List<ItemInfoEntry>();
					ManualParseConfig(json);
				}

				Log.LogInfo((object)$"[POPUP] Final config has {_itemInfoConfig.items.Count} items");
				foreach (var item in _itemInfoConfig.items)
				{
					Log.LogInfo((object)$"[POPUP]   - {item.itemId}: {item.title} (showOnFirstPickup={item.showOnFirstPickup})");
				}
			}
			else
			{
				Log.LogWarning((object)$"[POPUP] Config not found at: {_popupConfigPath}");
				_itemInfoConfig = new ItemInfoConfig { items = new List<ItemInfoEntry>() };
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)$"[POPUP] Failed to load config: {ex.Message}\n{ex.StackTrace}");
			_itemInfoConfig = new ItemInfoConfig { items = new List<ItemInfoEntry>() };
		}
	}

	/// <summary>
	/// Manual JSON parsing fallback for when Unity's JsonUtility fails with Lists.
	/// </summary>
	private static void ManualParseConfig(string json)
	{
		try
		{
			// Simple regex-based extraction for our known structure
			var itemMatches = System.Text.RegularExpressions.Regex.Matches(json,
				@"\{\s*""itemId""\s*:\s*""([^""]*)""\s*,\s*""title""\s*:\s*""([^""]*)""\s*,\s*""description""\s*:\s*""([^""]*)""\s*,\s*""imagePath""\s*:\s*""([^""]*)""\s*,\s*""showOnFirstPickup""\s*:\s*(true|false)");

			foreach (System.Text.RegularExpressions.Match match in itemMatches)
			{
				if (match.Groups.Count >= 6)
				{
					var entry = new ItemInfoEntry
					{
						itemId = match.Groups[1].Value,
						title = match.Groups[2].Value,
						description = match.Groups[3].Value,
						imagePath = match.Groups[4].Value,
						showOnFirstPickup = match.Groups[5].Value.ToLower() == "true"
					};
					_itemInfoConfig.items.Add(entry);
					Log.LogInfo((object)$"[POPUP] Manual parse found: {entry.itemId}");
				}
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)$"[POPUP] Manual parse failed: {ex.Message}");
		}
	}


	// ============================================================================
	// FIRST PICKUP INFO POPUP SYSTEM - Display Methods
	// ============================================================================

	/// <summary>
	/// Checks if a popup should be shown for the picked up item.
	/// Only triggers for custom props on first pickup of each type per session.
	/// </summary>
	private static void CheckFirstPickupPopup(object itemInstance)
	{
		try
		{
			if (itemInstance == null || _itemInfoConfig == null || _itemInfoConfig.items == null)
			{
				return;
			}

			// Get the prop ID from the item
			string propId = GetItemPropId(itemInstance);
			if (string.IsNullOrEmpty(propId))
			{
				Log.LogInfo((object)"[POPUP] Could not determine prop ID for item");
				return;
			}

			Log.LogInfo((object)$"[POPUP] Checking first pickup for prop: {propId}");

			// Check if already collected this session
			if (_sessionCollectedItems.Contains(propId))
			{
				Log.LogInfo((object)$"[POPUP] Already collected {propId} this session, skipping popup");
				return;
			}

			// Find item info in config
			ItemInfoEntry entry = null;
			foreach (var item in _itemInfoConfig.items)
			{
				if (item.itemId == propId)
				{
					entry = item;
					break;
				}
			}

			if (entry == null)
			{
				Log.LogInfo((object)$"[POPUP] No config entry found for {propId}");
				return;
			}

			if (!entry.showOnFirstPickup)
			{
				Log.LogInfo((object)$"[POPUP] showOnFirstPickup is false for {propId}");
				return;
			}

			// Mark as collected and show popup
			_sessionCollectedItems.Add(propId);
			Log.LogInfo((object)$"[POPUP] First pickup of {propId}, showing popup");
			ShowInfoPopup(entry);
		}
		catch (Exception ex)
		{
			Log.LogError((object)$"[POPUP] Error in CheckFirstPickupPopup: {ex.Message}");
		}
	}

	/// <summary>
	/// Extracts the prop ID from an item instance by looking at its definition GUID.
	/// </summary>
	private static string GetItemPropId(object itemInstance)
	{
		try
		{
			if (itemInstance == null) return null;

			Type itemType = itemInstance.GetType();

			// Try to get DefinitionGuid property
			PropertyInfo guidProp = itemType.GetProperty("DefinitionGuid");
			if (guidProp != null)
			{
				object guidValue = guidProp.GetValue(itemInstance);
				if (guidValue != null)
				{
					string guidStr = guidValue.ToString();
					Log.LogInfo((object)$"[POPUP] Found DefinitionGuid: {guidStr}");
					return guidStr;
				}
			}

			// Try to find definition through the component hierarchy
			Component itemComponent = itemInstance as Component;
			if (itemComponent != null)
			{
				// Look for EndlessProp parent which might have the prop info
				Transform current = itemComponent.transform;
				while (current != null)
				{
					// Check if this has a name that matches a loaded prop
					string objName = current.gameObject.name;
					// Remove "(Clone)" suffix if present
					if (objName.EndsWith("(Clone)"))
					{
						objName = objName.Substring(0, objName.Length - 7).Trim();
					}

					// Check against loaded prop definitions
					foreach (var kvp in _loadedPropDefinitions)
					{
						if (kvp.Value.DisplayName == objName || kvp.Key == objName)
						{
							Log.LogInfo((object)$"[POPUP] Matched prop by name: {kvp.Key}");
							return kvp.Key;
						}
					}

					current = current.parent;
				}
			}

			// Fallback: try to get AssetID field
			FieldInfo assetIdField = itemType.GetField("AssetID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (assetIdField != null)
			{
				object assetId = assetIdField.GetValue(itemInstance);
				if (assetId != null)
				{
					Log.LogInfo((object)$"[POPUP] Found AssetID: {assetId}");
					return assetId.ToString();
				}
			}

			return null;
		}
		catch (Exception ex)
		{
			Log.LogError((object)$"[POPUP] Error getting prop ID: {ex.Message}");
			return null;
		}
	}

	/// <summary>
	/// Shows the info popup for the given item entry.
	/// Builds the entire UI in code - no asset bundle required.
	/// </summary>
	private static void ShowInfoPopup(ItemInfoEntry entry)
	{
		try
		{
			// Close existing popup if any
			if (_activePopupInstance != null)
			{
				Object.Destroy(_activePopupInstance);
				_activePopupInstance = null;
			}

			// Find or create canvas
			Canvas targetCanvas = FindOrCreatePopupCanvas();
			if (targetCanvas == null)
			{
				Log.LogError((object)"[POPUP] Could not find or create canvas");
				return;
			}

			// Build the popup UI entirely in code
			_activePopupInstance = BuildPopupUI(targetCanvas, entry);
			if (_activePopupInstance == null)
			{
				Log.LogError((object)"[POPUP] Failed to build popup UI");
				return;
			}

			// Load and set image
			LoadAndSetItemImage(entry.imagePath);

			// Start fade-in animation
			if (_popupCanvasGroup != null)
			{
				CoroutineRunner.Instance.StartCoroutine(FadeInPopup());
			}

			Log.LogInfo((object)$"[POPUP] Showing popup for: {entry.title}");
		}
		catch (Exception ex)
		{
			Log.LogError((object)$"[POPUP] Failed to show popup: {ex.Message}\n{ex.StackTrace}");
		}
	}

	/// <summary>
	/// Builds the popup UI entirely in code.
	/// REQUIREMENTS (see INFO_POPUP_REQUIREMENTS.txt):
	/// - LEFT side anchoring, upper left corner, beneath player name/avatar
	/// - Image spans FULL WIDTH (with margins)
	/// - Text is BELOW the image
	/// - Manual close only, NO auto-close timer
	///
	/// PATTERN: Based on Runtime Prop Info Detail panel from PropToolDiagnostic.txt lines 1215-1464
	/// </summary>
	private static GameObject BuildPopupUI(Canvas canvas, ItemInfoEntry entry)
	{
		// ========================================================================
		// EXACT COLORS from Runtime Prop Info Detail panel
		// ========================================================================
		Color panelBgColor = new Color(0.220f, 0.220f, 0.220f, 1f);        // Dark gray background
		Color buttonOutlineColor = new Color(0.000f, 0.639f, 1.000f, 1f);  // Bright blue
		Color buttonFillColor = new Color(0.047f, 0.051f, 0.063f, 1f);     // Near black
		Color lineSeparatorColor = Color.white;
		Color textColor = Color.white;

		// Try to find game's Inter-Regular SDF font at runtime
		TMP_FontAsset gameFont = null;
		try
		{
			var allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
			foreach (var font in allFonts)
			{
				if (font.name.Contains("Inter"))
				{
					gameFont = font;
					Log.LogInfo((object)$"[POPUP] Found game font: {font.name}");
					break;
				}
			}
		}
		catch (Exception ex)
		{
			Log.LogWarning((object)$"[POPUP] Could not search for game fonts: {ex.Message}");
		}

		// Try to find game sprites for rounded buttons
		Sprite outlineSprite = null;
		Sprite fillSprite = null;
		Sprite xIconSprite = null;
		try
		{
			var allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
			foreach (var sprite in allSprites)
			{
				if (sprite.name == "ShapeSquare_64_Rounded4_Outline2") outlineSprite = sprite;
				else if (sprite.name == "ShapeSquare_64_Rounded1") fillSprite = sprite;
				else if (sprite.name == "IconX") xIconSprite = sprite;
			}
			if (outlineSprite != null) Log.LogInfo((object)"[POPUP] Found outline sprite");
			if (fillSprite != null) Log.LogInfo((object)"[POPUP] Found fill sprite");
			if (xIconSprite != null) Log.LogInfo((object)"[POPUP] Found X icon sprite");
		}
		catch (Exception ex)
		{
			Log.LogWarning((object)$"[POPUP] Could not search for game sprites: {ex.Message}");
		}

		// ========================================================================
		// Find Safe Area inside Creator UI (game's HUD element container)
		// Menu Button Icon is inside Safe Area at (10, -47), so popup goes there too
		// ========================================================================
		Transform safeArea = null;
		foreach (Transform child in canvas.transform)
		{
			if (child.name == "Safe Area")
			{
				safeArea = child;
				break;
			}
			// Check one level deeper
			foreach (Transform grandchild in child)
			{
				if (grandchild.name == "Safe Area")
				{
					safeArea = grandchild;
					break;
				}
			}
			if (safeArea != null) break;
		}

		// ========================================================================
		// ROOT PANEL - LEFT side, upper left, beneath menu button
		// Menu button is at (10, -47), 53x53, so popup starts at Y = -110
		// ========================================================================
		GameObject root = new GameObject("FirstPickupInfoPopup");

		// Parent to Safe Area (sibling of Menu Button Icon)
		if (safeArea != null)
		{
			root.transform.SetParent(safeArea, false);
			Log.LogInfo((object)"[POPUP] Parented to Safe Area");
		}
		else
		{
			root.transform.SetParent(canvas.transform, false);
			Log.LogWarning((object)"[POPUP] Safe Area not found, using canvas directly");
		}

		RectTransform rootRect = root.AddComponent<RectTransform>();
		rootRect.anchorMin = new Vector2(0, 1);       // TOP-LEFT corner
		rootRect.anchorMax = new Vector2(0, 1);
		rootRect.pivot = new Vector2(0, 1);           // Pivot at top-left
		rootRect.sizeDelta = new Vector2(400, 450);   // Fixed size: 400x450
		rootRect.anchoredPosition = new Vector2(10, -110);  // 10px from left, 110px from top

		_popupCanvasGroup = root.AddComponent<CanvasGroup>();
		_popupCanvasGroup.alpha = 0;
		_popupCanvasGroup.interactable = true;
		_popupCanvasGroup.blocksRaycasts = true;

		// ========================================================================
		// BACKGROUND IMAGE - fill entire panel (dark gray solid)
		// ========================================================================
		GameObject bgPanel = new GameObject("Background Image");
		bgPanel.transform.SetParent(root.transform, false);
		RectTransform bgRect = bgPanel.AddComponent<RectTransform>();
		bgRect.anchorMin = Vector2.zero;
		bgRect.anchorMax = Vector2.one;
		bgRect.sizeDelta = Vector2.zero;
		bgRect.anchoredPosition = Vector2.zero;
		Image bgImage = bgPanel.AddComponent<Image>();
		bgImage.color = panelBgColor;
		bgImage.raycastTarget = true;

		// ========================================================================
		// CLOSE BUTTON - 60x60, fully OUTSIDE at top-right corner
		// Button sits above and to the right of the popup window
		// ========================================================================
		GameObject closeBtn = new GameObject("Hide Button Icon");
		closeBtn.transform.SetParent(root.transform, false);
		RectTransform closeBtnRect = closeBtn.AddComponent<RectTransform>();
		closeBtnRect.anchorMin = new Vector2(1, 1);   // TOP-RIGHT of popup
		closeBtnRect.anchorMax = new Vector2(1, 1);
		closeBtnRect.pivot = new Vector2(0, 0);       // Pivot at bottom-left of button
		closeBtnRect.sizeDelta = new Vector2(60, 60); // 60x60 pixels
		closeBtnRect.anchoredPosition = new Vector2(0, 0);  // Button's bottom-left at popup's top-right

		// Button Background Image - BLUE OUTLINE
		GameObject btnBgImage = new GameObject("Button Background Image");
		btnBgImage.transform.SetParent(closeBtn.transform, false);
		RectTransform btnBgRect = btnBgImage.AddComponent<RectTransform>();
		btnBgRect.anchorMin = Vector2.zero;
		btnBgRect.anchorMax = Vector2.one;
		btnBgRect.sizeDelta = Vector2.zero;
		btnBgRect.anchoredPosition = Vector2.zero;
		Image btnBgImg = btnBgImage.AddComponent<Image>();
		btnBgImg.color = buttonOutlineColor;
		btnBgImg.raycastTarget = true;
		if (outlineSprite != null)
		{
			btnBgImg.sprite = outlineSprite;
			btnBgImg.type = Image.Type.Sliced;
		}

		// Fill Image - DARK INNER FILL (inset by 8px)
		GameObject fillImage = new GameObject("Fill Image");
		fillImage.transform.SetParent(closeBtn.transform, false);
		RectTransform fillRect = fillImage.AddComponent<RectTransform>();
		fillRect.anchorMin = Vector2.zero;
		fillRect.anchorMax = Vector2.one;
		fillRect.sizeDelta = new Vector2(-8, -8);  // Inset by 8px on all sides
		fillRect.anchoredPosition = Vector2.zero;
		Image fillImg = fillImage.AddComponent<Image>();
		fillImg.color = buttonFillColor;
		fillImg.raycastTarget = false;
		if (fillSprite != null)
		{
			fillImg.sprite = fillSprite;
			fillImg.type = Image.Type.Sliced;
		}

		// Icon Image - X icon (blue, centered at 60% of button size)
		GameObject iconImage = new GameObject("Icon Image");
		iconImage.transform.SetParent(closeBtn.transform, false);
		RectTransform iconRect = iconImage.AddComponent<RectTransform>();
		iconRect.anchorMin = new Vector2(0.2f, 0.2f);  // 20% inset
		iconRect.anchorMax = new Vector2(0.8f, 0.8f);  // 80% = 60% of button
		iconRect.sizeDelta = Vector2.zero;
		iconRect.anchoredPosition = Vector2.zero;
		Image iconImg = iconImage.AddComponent<Image>();
		iconImg.color = buttonOutlineColor;  // Blue to match outline
		iconImg.raycastTarget = false;
		if (xIconSprite != null)
		{
			iconImg.sprite = xIconSprite;
		}
		else
		{
			// Fallback: create X using TextMeshPro
			GameObject xTextFallback = new GameObject("XTextFallback");
			xTextFallback.transform.SetParent(closeBtn.transform, false);
			RectTransform xTextRect = xTextFallback.AddComponent<RectTransform>();
			xTextRect.anchorMin = Vector2.zero;
			xTextRect.anchorMax = Vector2.one;
			xTextRect.sizeDelta = Vector2.zero;
			xTextRect.anchoredPosition = Vector2.zero;
			var xTmp = xTextFallback.AddComponent<TextMeshProUGUI>();
			xTmp.text = "✕";
			xTmp.fontSize = 28;
			xTmp.alignment = TextAlignmentOptions.Center;
			xTmp.color = buttonOutlineColor;
			if (gameFont != null) xTmp.font = gameFont;
			iconImage.SetActive(false);  // Hide the empty image
		}

		// Button component on the root
		_popupCloseButton = closeBtn.AddComponent<Button>();
		_popupCloseButton.targetGraphic = btnBgImg;
		_popupCloseButton.transition = Selectable.Transition.ColorTint;
		ColorBlock colors = _popupCloseButton.colors;
		colors.normalColor = Color.white;
		colors.highlightedColor = new Color(0.961f, 0.961f, 0.961f, 1f);
		colors.pressedColor = new Color(0.784f, 0.784f, 0.784f, 1f);
		_popupCloseButton.colors = colors;
		_popupCloseButton.onClick.AddListener(HideInfoPopup);

		// ========================================================================
		// IMAGE CONTAINER - Full width, below close button area
		// Position: (10, -10) from top-left, width -20 (10px margins each side)
		// ========================================================================
		GameObject imageContainer = new GameObject("Image Container");
		imageContainer.transform.SetParent(root.transform, false);
		RectTransform imageContainerRect = imageContainer.AddComponent<RectTransform>();
		imageContainerRect.anchorMin = new Vector2(0, 1);
		imageContainerRect.anchorMax = new Vector2(1, 1);
		imageContainerRect.pivot = new Vector2(0.5f, 1);
		imageContainerRect.sizeDelta = new Vector2(-20, 200);  // Full width minus 20px margins, 200px height
		imageContainerRect.anchoredPosition = new Vector2(0, -10);  // 10px from top

		// Image background (for container appearance)
		Image imageContainerBg = imageContainer.AddComponent<Image>();
		imageContainerBg.color = new Color(0.106f, 0.110f, 0.137f, 1f);  // Dark blue-gray like icon container
		imageContainerBg.raycastTarget = false;

		// Item Image (fills container)
		GameObject itemImage = new GameObject("Item Image");
		itemImage.transform.SetParent(imageContainer.transform, false);
		RectTransform itemImageRect = itemImage.AddComponent<RectTransform>();
		itemImageRect.anchorMin = Vector2.zero;
		itemImageRect.anchorMax = Vector2.one;
		itemImageRect.sizeDelta = Vector2.zero;
		itemImageRect.anchoredPosition = Vector2.zero;
		_popupItemImage = itemImage.AddComponent<RawImage>();
		_popupItemImage.color = Color.white;  // White so image shows true colors

		// ========================================================================
		// LINE SEPARATOR - 5px white line below image
		// Position: Y = -220 (10 margin + 200 image + 10 spacing)
		// ========================================================================
		GameObject lineImage = new GameObject("Line Image");
		lineImage.transform.SetParent(root.transform, false);
		RectTransform lineRect = lineImage.AddComponent<RectTransform>();
		lineRect.anchorMin = new Vector2(0, 1);
		lineRect.anchorMax = new Vector2(1, 1);
		lineRect.pivot = new Vector2(0.5f, 1);
		lineRect.sizeDelta = new Vector2(-20, 5);  // Full width minus 20px, 5px height
		lineRect.anchoredPosition = new Vector2(0, -220);
		Image lineImg = lineImage.AddComponent<Image>();
		lineImg.color = lineSeparatorColor;
		lineImg.raycastTarget = false;

		// ========================================================================
		// TITLE TEXT - Below line separator
		// Font: 26pt, Bold + Underline, TopLeft alignment
		// Position: Y = -235 (220 + 5 line + 10 spacing)
		// ========================================================================
		GameObject titleObj = new GameObject("Name Text");
		titleObj.transform.SetParent(root.transform, false);
		RectTransform titleRect = titleObj.AddComponent<RectTransform>();
		titleRect.anchorMin = new Vector2(0, 1);
		titleRect.anchorMax = new Vector2(1, 1);
		titleRect.pivot = new Vector2(0.5f, 1);
		titleRect.sizeDelta = new Vector2(-20, 55);  // Full width minus margins, 55px height
		titleRect.anchoredPosition = new Vector2(0, -235);

		_popupTitleText = titleObj.AddComponent<TextMeshProUGUI>();
		_popupTitleText.text = entry.title;
		_popupTitleText.fontSize = 26;
		_popupTitleText.fontStyle = FontStyles.Bold | FontStyles.Underline;
		_popupTitleText.alignment = TextAlignmentOptions.TopLeft;
		_popupTitleText.color = textColor;
		if (gameFont != null) _popupTitleText.font = gameFont;

		// ========================================================================
		// DESCRIPTION TEXT - Below title
		// Font: 16pt, Normal, TopLeft alignment
		// Position: Y = -300 (235 + 55 title + 10 spacing)
		// ========================================================================
		GameObject descObj = new GameObject("Description Text");
		descObj.transform.SetParent(root.transform, false);
		RectTransform descRect = descObj.AddComponent<RectTransform>();
		descRect.anchorMin = new Vector2(0, 1);
		descRect.anchorMax = new Vector2(1, 1);
		descRect.pivot = new Vector2(0.5f, 1);
		descRect.sizeDelta = new Vector2(-20, 140);  // Full width minus margins, remaining height
		descRect.anchoredPosition = new Vector2(0, -300);

		_popupDescriptionText = descObj.AddComponent<TextMeshProUGUI>();
		_popupDescriptionText.text = entry.description;
		_popupDescriptionText.fontSize = 16;
		_popupDescriptionText.fontStyle = FontStyles.Normal;
		_popupDescriptionText.alignment = TextAlignmentOptions.TopLeft;
		_popupDescriptionText.color = textColor;
		_popupDescriptionText.enableWordWrapping = true;
		_popupDescriptionText.overflowMode = TextOverflowModes.Overflow;
		if (gameFont != null) _popupDescriptionText.font = gameFont;

		Log.LogInfo((object)"[POPUP] UI built - Runtime Prop Info Detail pattern, LEFT side, blue close button, full-width image");
		return root;
	}

	/// <summary>
	/// Finds an existing overlay canvas or creates a new one.
	/// </summary>
	private static Canvas FindOrCreatePopupCanvas()
	{
		try
		{
			Canvas[] canvases = Object.FindObjectsOfType<Canvas>();

			// Priority 1: Find "Creator UI" canvas (main HUD - contains Safe Area)
			foreach (var canvas in canvases)
			{
				if (canvas != null &&
					canvas.name == "Creator UI" &&
					canvas.renderMode == RenderMode.ScreenSpaceOverlay)
				{
					Log.LogInfo((object)"[POPUP] Found Creator UI canvas");
					return canvas;
				}
			}

			// Priority 2: Known safe fallbacks
			string[] safeNames = { "Ghost Mode", "Fade Canvas" };
			foreach (var canvas in canvases)
			{
				if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
				{
					foreach (var safeName in safeNames)
					{
						if (canvas.name == safeName)
						{
							Log.LogInfo((object)$"[POPUP] Found fallback canvas: {canvas.name}");
							return canvas;
						}
					}
				}
			}

			// Priority 3: Create our own canvas (last resort)
			Log.LogInfo((object)"[POPUP] Creating new overlay canvas");
			GameObject canvasObj = new GameObject("FirstPickupInfoCanvas");
			Canvas newCanvas = canvasObj.AddComponent<Canvas>();
			newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
			newCanvas.sortingOrder = 100;

			// Add required components
			CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);

			canvasObj.AddComponent<GraphicRaycaster>();
			Object.DontDestroyOnLoad(canvasObj);

			return newCanvas;
		}
		catch (Exception ex)
		{
			Log.LogError((object)$"[POPUP] Error creating canvas: {ex.Message}");
			return null;
		}
	}

	/// <summary>
	/// Loads an image from the ItemImages folder and sets it on the popup.
	/// </summary>
	private static void LoadAndSetItemImage(string imagePath)
	{
		if (_popupItemImage == null)
		{
			Log.LogWarning((object)"[POPUP] RawImage component not found, cannot set image");
			return;
		}

		try
		{
			string fullPath = Path.Combine(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				imagePath
			);

			Log.LogInfo((object)$"[POPUP] Loading image from: {fullPath}");

			if (File.Exists(fullPath))
			{
				byte[] imageData = File.ReadAllBytes(fullPath);
				Texture2D tex = new Texture2D(2, 2);
				if (tex.LoadImage(imageData))
				{
					_popupItemImage.texture = tex;
					Log.LogInfo((object)$"[POPUP] Image loaded successfully ({tex.width}x{tex.height})");
				}
				else
				{
					Log.LogWarning((object)"[POPUP] Failed to decode image data");
					SetDefaultImage();
				}
			}
			else
			{
				Log.LogWarning((object)$"[POPUP] Image not found: {fullPath}");
				SetDefaultImage();
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)$"[POPUP] Failed to load image: {ex.Message}");
			SetDefaultImage();
		}
	}

	/// <summary>
	/// Sets a default placeholder image if the specified image cannot be loaded.
	/// </summary>
	private static void SetDefaultImage()
	{
		if (_popupItemImage != null)
		{
			// Create a simple grey placeholder texture
			Texture2D placeholder = new Texture2D(128, 128);
			Color greyColor = new Color(0.3f, 0.3f, 0.35f, 1f);
			Color[] pixels = new Color[128 * 128];
			for (int i = 0; i < pixels.Length; i++)
				pixels[i] = greyColor;
			placeholder.SetPixels(pixels);
			placeholder.Apply();
			_popupItemImage.texture = placeholder;
			Log.LogInfo((object)"[POPUP] Using placeholder image");
		}
	}

	/// <summary>
	/// Hides and destroys the active popup.
	/// </summary>
	private static void HideInfoPopup()
	{
		try
		{
			Log.LogInfo((object)"[POPUP] HideInfoPopup called");

			if (_activePopupInstance != null)
			{
				if (_popupCanvasGroup != null)
				{
					CoroutineRunner.Instance.StartCoroutine(FadeOutAndDestroy());
				}
				else
				{
					Object.Destroy(_activePopupInstance);
					_activePopupInstance = null;
				}
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)$"[POPUP] Error hiding popup: {ex.Message}");
		}
	}

	/// <summary>
	/// Coroutine that fades in the popup.
	/// </summary>
	private static IEnumerator FadeInPopup()
	{
		if (_popupCanvasGroup == null) yield break;

		_popupCanvasGroup.alpha = 0;
		float elapsed = 0;
		float duration = 0.25f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			_popupCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
			yield return null;
		}

		_popupCanvasGroup.alpha = 1;
		Log.LogInfo((object)"[POPUP] Fade in complete");
	}

	/// <summary>
	/// Coroutine that fades out and destroys the popup.
	/// </summary>
	private static IEnumerator FadeOutAndDestroy()
	{
		if (_popupCanvasGroup == null) yield break;

		float elapsed = 0;
		float duration = 0.2f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			_popupCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
			yield return null;
		}

		Object.Destroy(_activePopupInstance);
		_activePopupInstance = null;
		Log.LogInfo((object)"[POPUP] Fade out complete, popup destroyed");
	}

	private void Update()
	{
		if (IsSwimmingActive)
		{
			UpdateSwimMotor();
			if (!_blurReductionApplied)
			{
				TryReduceBlur();
			}
		}
		else
		{
			_swimVerticalMotor = 0;
			if (_blurReductionApplied)
			{
				TryRestoreBlur();
			}
		}
		if (!IsPlayerUnderwater && _swimLoggedOnce)
		{
			ResetSwimmingCaches();
		}
	}

	private void UpdateSwimMotor()
	{
		float input = 0f;
		if (Input.GetKey((KeyCode)113))
		{
			input = 1f;
		}
		else if (Input.GetKey((KeyCode)101))
		{
			input = -1f;
		}
		_swimVerticalMotor = MotorFromInput(input, _swimVerticalMotor, _swimMotorFrames, _swimMotorDegradeRate);
	}

	private static int MotorFromInput(float input, int currentMotor, int maxFrames, int degradeRate)
	{
		if (input > 0f)
		{
			return Mathf.Min(currentMotor + 1, maxFrames);
		}
		if (input < 0f)
		{
			return Mathf.Max(currentMotor - 1, -maxFrames);
		}
		if (currentMotor > 0)
		{
			return Mathf.Max(0, currentMotor - degradeRate);
		}
		if (currentMotor < 0)
		{
			return Mathf.Min(0, currentMotor + degradeRate);
		}
		return 0;
	}

	private void ResetSwimmingCaches()
	{
		_currentSwimState = SwimState.SURFACE;
		_exitGraceFrames = 0;
		_cachedPlayerController = null;
		_swimLoggedOnce = false;
		_swimVerticalMotor = 0;
		_swimPhysicsLogged = false;
		_wasSwimmingLastFrame = false;
		_waterExitCooldown = 0f;
		_waterDebugLogged = false;
		_cachedDepthPlane = null;
		if (_blurReductionApplied)
		{
			TryRestoreBlur();
		}
		_blurReductionApplied = false;
		Log.LogInfo((object)"[SWIM] Caches reset (exited water), _currentSwimState=SURFACE");
	}

	private static void FullSwimmingStateReset()
	{
		_currentSwimState = SwimState.SURFACE;
		_exitGraceFrames = 0;
		IsPlayerUnderwater = false;
		IsSwimmingActive = false;
		UnderwaterDepth = 0f;
		WaterEntryY = float.MaxValue;
		WaterSurfaceY = float.MinValue;
		_swimVerticalMotor = 0;
		_swimLoggedOnce = false;
		_wasSwimmingLastFrame = false;
		_lastExitTime = Time.time;
		_waterExitCooldown = 0f;
		_waterDebugLogged = false;
		_cachedDepthPlane = null;
		_positionInitialized = false;
		_jumpImmunityFrames = 0;
		Log.LogInfo((object)"[SWIM-RESET] Full state reset: _currentSwimState=SURFACE");
	}

	private void CacheTypes()
	{
		_baseTypeDefinitionType = AccessTools.TypeByName("Endless.Gameplay.BaseTypeDefinition");
		_componentDefinitionType = AccessTools.TypeByName("Endless.Gameplay.ComponentDefinition");
		_propType = AccessTools.TypeByName("Endless.Props.Assets.Prop");
		_serializableGuidType = AccessTools.TypeByName("Endless.Shared.DataTypes.SerializableGuid");
		_stageManagerType = AccessTools.TypeByName("Endless.Gameplay.LevelEditing.Level.StageManager");
		_endlessVisualsType = AccessTools.TypeByName("Endless.Gameplay.Scripting.EndlessVisuals");
		_endlessPropType = AccessTools.TypeByName("Endless.Gameplay.Scripting.EndlessProp");
		_propBasedToolType = AccessTools.TypeByName("Endless.Creator.LevelEditing.Runtime.PropBasedTool");
		_propLibraryType = AccessTools.TypeByName("Endless.Gameplay.LevelEditing.PropLibrary");
		_playerControllerType = AccessTools.TypeByName("Endless.Gameplay.PlayerController");
		_depthPlaneType = AccessTools.TypeByName("Endless.Gameplay.DepthPlane");
		_appearanceAnimatorType = AccessTools.TypeByName("Endless.Gameplay.AppearanceAnimator");
		_filterType = AccessTools.TypeByName("Endless.Gameplay.Filter");
		_playerReferenceManagerType = AccessTools.TypeByName("Endless.Gameplay.PlayerReferenceManager");
		_netStateType = AccessTools.TypeByName("Endless.Gameplay.NetState");
		_itemType = AccessTools.TypeByName("Endless.Gameplay.Item");
		_inventoryType = AccessTools.TypeByName("Endless.Gameplay.Inventory");
		_itemInteractableType = AccessTools.TypeByName("Endless.Gameplay.ItemInteractable");
		_inventorySlotType = AccessTools.TypeByName("Endless.Gameplay.PlayerInventory.InventorySlot");
		_inventoryUsableDefinitionType = AccessTools.TypeByName("Endless.Gameplay.PlayerInventory.InventoryUsableDefinition");
		_treasureItemType = AccessTools.TypeByName("Endless.Gameplay.TreasureItem");
		_genericUsableDefinitionType = AccessTools.TypeByName("Endless.Gameplay.GenericUsableDefinition");
		_interactableBaseType = AccessTools.TypeByName("Endless.Gameplay.InteractableBase");
		_simpleYawSpinType = AccessTools.TypeByName("Endless.Gameplay.SimpleYawSpin");
		_usableDefinitionType = AccessTools.TypeByName("Endless.Gameplay.UsableDefinition");
		_endlessPrefabAssetType = AccessTools.TypeByName("Endless.Props.Assets.EndlessPrefabAsset");
		_fileAssetInstanceType = AccessTools.TypeByName("Endless.Assets.FileAssetInstance");
		_assetReferenceType = AccessTools.TypeByName("Endless.Assets.AssetReference");
		_loadedFileManagerType = AccessTools.TypeByName("Endless.FileManagement.LoadedFileManager");
		_prefabBundleField = AccessTools.Field(_propType, "prefabBundle");
		_loadedFilesField = AccessTools.Field(_loadedFileManagerType, "loadedFiles");
		Log.LogInfo((object)"[INJECTOR] v10.45.0 Asset bundle types cached:");
		Log.LogInfo((object)$"[INJECTOR]   EndlessPrefabAsset: {_endlessPrefabAssetType != null}");
		Log.LogInfo((object)$"[INJECTOR]   FileAssetInstance: {_fileAssetInstanceType != null}");
		Log.LogInfo((object)$"[INJECTOR]   AssetReference: {_assetReferenceType != null}");
		Log.LogInfo((object)$"[INJECTOR]   LoadedFileManager: {_loadedFileManagerType != null}");
		Log.LogInfo((object)$"[INJECTOR]   prefabBundle field: {_prefabBundleField != null}");
		Log.LogInfo((object)$"[INJECTOR]   loadedFiles field: {_loadedFilesField != null}");
		Type type = AccessTools.TypeByName("Endless.Gameplay.BaseTypeList");
		if (type != null)
		{
			_abstractComponentListBaseType = type.BaseType;
		}
		Log.LogInfo((object)"[INJECTOR] Types cached:");
		Log.LogInfo((object)$"[INJECTOR]   PlayerController: {_playerControllerType != null}");
		Log.LogInfo((object)$"[INJECTOR]   NetState: {_netStateType != null}");
		Log.LogInfo((object)$"[INJECTOR]   DepthPlane: {_depthPlaneType != null}");
		Log.LogInfo((object)$"[INJECTOR]   Item: {_itemType != null}");
		Log.LogInfo((object)$"[INJECTOR]   Inventory: {_inventoryType != null}");
		Log.LogInfo((object)$"[INJECTOR]   ItemInteractable: {_itemInteractableType != null}");
		Log.LogInfo((object)$"[INJECTOR]   TreasureItem: {_treasureItemType != null}");
		Log.LogInfo((object)$"[INJECTOR]   GenericUsableDefinition: {_genericUsableDefinitionType != null}");
	}

	private void InitializeFieldReferences()
	{
		if (!(_playerControllerType == null) && !(_netStateType == null))
		{
			_currentStateField = AccessTools.Field(_playerControllerType, "currentState");
			if (_currentStateField != null)
			{
				Type fieldType = _currentStateField.FieldType;
				_totalForceField = AccessTools.Field(fieldType, "TotalForce");
				_calculatedMotionField = AccessTools.Field(fieldType, "CalculatedMotion");
				_blockGravityField = AccessTools.Field(fieldType, "BlockGravity");
				_framesSinceStableGroundField = AccessTools.Field(fieldType, "FramesSinceStableGround");
			}
			_fieldsInitialized = _currentStateField != null && _totalForceField != null;
			Log.LogInfo((object)"[SWIM] Field references initialized:");
			Log.LogInfo((object)string.Format("[SWIM]   currentState: {0} (type: {1})", _currentStateField != null, _currentStateField?.FieldType?.Name ?? "null"));
			Log.LogInfo((object)$"[SWIM]   TotalForce: {_totalForceField != null}");
			Log.LogInfo((object)$"[SWIM]   CalculatedMotion: {_calculatedMotionField != null}");
			Log.LogInfo((object)$"[SWIM]   BlockGravity: {_blockGravityField != null}");
			Log.LogInfo((object)$"[SWIM]   FramesSinceStableGround: {_framesSinceStableGroundField != null}");
		}
	}

	private static void CacheCharacterController(object playerController)
	{
		if (_charControllerCached || playerController == null)
		{
			return;
		}
		try
		{
			FieldInfo field = playerController.GetType().GetField("playerReferences", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				Log.LogWarning((object)"[SWIM] playerReferences field not found");
				return;
			}
			object value = field.GetValue(playerController);
			if (value == null)
			{
				Log.LogWarning((object)"[SWIM] playerReferences value is null");
				return;
			}
			PropertyInfo property = value.GetType().GetProperty("CharacterController", BindingFlags.Instance | BindingFlags.Public);
			if (property != null)
			{
				object value2 = property.GetValue(value);
				_cachedCharController = (CharacterController)((value2 is CharacterController) ? value2 : null);
			}
			else
			{
				FieldInfo field2 = value.GetType().GetField("CharacterController", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field2 != null)
				{
					object value3 = field2.GetValue(value);
					_cachedCharController = (CharacterController)((value3 is CharacterController) ? value3 : null);
				}
			}
			if ((Object)(object)_cachedCharController != (Object)null)
			{
				_characterHeight = _cachedCharController.height;
				_characterRadius = _cachedCharController.radius;
				_charControllerCached = true;
				Log.LogInfo((object)$"[SWIM] CharacterController cached: height={_characterHeight:F2}, radius={_characterRadius:F2}");
			}
			else
			{
				Log.LogWarning((object)"[SWIM] CharacterController not found, using defaults");
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[SWIM] Failed to cache CharacterController: " + ex.Message));
		}
	}

	private void PatchProcessFallOffStage()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		if (_playerControllerType != null)
		{
			MethodInfo methodInfo = AccessTools.Method(_playerControllerType, "ProcessFallOffStage_NetFrame", (Type[])null, (Type[])null);
			if (methodInfo != null)
			{
				HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("ProcessFallOffStage_Prefix", BindingFlags.Static | BindingFlags.Public));
				_harmony.Patch((MethodBase)methodInfo, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				Log.LogInfo((object)"[INJECTOR] Patched PlayerController.ProcessFallOffStage_NetFrame");
			}
		}
	}

	private void PatchProcessPhysicsNetFrame()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		if (_playerControllerType != null)
		{
			MethodInfo methodInfo = AccessTools.Method(_playerControllerType, "ProcessPhysics_NetFrame", (Type[])null, (Type[])null);
			if (methodInfo != null)
			{
				HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("ProcessPhysics_Postfix_Swimming", BindingFlags.Static | BindingFlags.Public));
				_harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				Log.LogInfo((object)"[INJECTOR] Patched PlayerController.ProcessPhysics_NetFrame (v6.1.0 Swimming Postfix)");
			}
			else
			{
				Log.LogError((object)"[INJECTOR] ProcessPhysics_NetFrame method not found!");
			}
		}
	}

	private void PatchProcessJumpPrefix()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		if (_playerControllerType != null)
		{
			MethodInfo methodInfo = AccessTools.Method(_playerControllerType, "ProcessJump_NetFrame", new Type[1] { AccessTools.TypeByName("Endless.Gameplay.NetInput") }, (Type[])null);
			if (methodInfo != null)
			{
				HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("ProcessJump_Prefix", BindingFlags.Static | BindingFlags.Public));
				_harmony.Patch((MethodBase)methodInfo, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				Log.LogInfo((object)"[INJECTOR] Patched PlayerController.ProcessJump_NetFrame (prefix)");
			}
		}
	}

	private void PatchAppearanceAnimator()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		if (_appearanceAnimatorType != null)
		{
			MethodInfo methodInfo = AccessTools.Method(_appearanceAnimatorType, "SetAnimationState", (Type[])null, (Type[])null);
			if (methodInfo != null)
			{
				HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("SetAnimationState_Prefix", BindingFlags.Static | BindingFlags.Public));
				_harmony.Patch((MethodBase)methodInfo, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				Log.LogInfo((object)"[INJECTOR] Patched AppearanceAnimator.SetAnimationState");
			}
		}
	}

	private void PatchDepthPlaneStart()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		if (_depthPlaneType != null)
		{
			MethodInfo methodInfo = AccessTools.Method(_depthPlaneType, "Start", (Type[])null, (Type[])null);
			if (methodInfo != null)
			{
				HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("DepthPlane_Start_Postfix", BindingFlags.Static | BindingFlags.Public));
				_harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				Log.LogInfo((object)"[INJECTOR] Patched DepthPlane.Start");
			}
		}
	}

	private void PatchFilterStartTransition()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		if (_filterType != null)
		{
			MethodInfo methodInfo = AccessTools.Method(_filterType, "StartTransition", (Type[])null, (Type[])null);
			if (methodInfo != null)
			{
				HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("StartTransition_Prefix", BindingFlags.Static | BindingFlags.Public));
				_harmony.Patch((MethodBase)methodInfo, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				Log.LogInfo((object)"[INJECTOR] Patched Filter.StartTransition");
			}
		}
	}

	public static void ProcessPhysics_Postfix_Swimming(object __instance)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (_currentSwimState != SwimState.SWIMMING && _currentSwimState != SwimState.ENTERING)
			{
				return;
			}
			object obj = ((__instance is MonoBehaviour) ? __instance : null);
			Transform val = ((obj != null) ? ((Component)obj).transform : null);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			CacheCharacterController(__instance);
			float y = val.position.y;
			_ = _characterHeight;
			_ = _characterRadius;
			if (WaterSurfaceY == float.MinValue)
			{
				return;
			}
			if (_currentStateField == null)
			{
				_currentStateField = __instance.GetType().GetField("currentState", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
			if (_currentStateField == null)
			{
				return;
			}
			object value = _currentStateField.GetValue(__instance);
			if (value == null)
			{
				return;
			}
			if (_totalForceField == null)
			{
				_totalForceField = value.GetType().GetField("TotalForce", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
			if (_calculatedMotionField == null)
			{
				_calculatedMotionField = value.GetType().GetField("CalculatedMotion", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
			FieldInfo field = value.GetType().GetField("FallFrames", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (_totalForceField == null || _calculatedMotionField == null)
			{
				Log.LogError((object)"[SWIM] Missing required fields!");
				return;
			}
			float num = WaterSurfaceY - 0.5f - (_characterHeight * 0.5f - _characterRadius);
			float num2 = WaterSurfaceY - MaxSwimDepth;
			float num3 = SwimmingVerticalSpeed * ((float)_swimVerticalMotor / (float)_swimMotorFrames);
			if (y >= num)
			{
				if (num3 >= 0f)
				{
					num3 = -2f;
				}
			}
			else if (y <= num2 && num3 < 0f)
			{
				num3 = 0f;
			}
			Vector3 val2 = (Vector3)_totalForceField.GetValue(value);
			val2.y = 0f;
			_totalForceField.SetValue(value, val2);
			if (field != null)
			{
				field.SetValue(value, 0);
			}
			Vector3 val3 = (Vector3)_calculatedMotionField.GetValue(value);
			val3.y = num3;
			_calculatedMotionField.SetValue(value, val3);
			_currentStateField.SetValue(__instance, value);
			if (!_swimLoggedOnce)
			{
				Log.LogInfo((object)$"[SWIM] v10.3.0 Swimming active (state={_currentSwimState})");
				Log.LogInfo((object)$"[SWIM] Water surface from planeParent.position.y = {WaterSurfaceY:F2}");
				Log.LogInfo((object)$"[SWIM] CharHeight={_characterHeight:F2}, CharRadius={_characterRadius:F2}");
				Log.LogInfo((object)$"[SWIM] SwimCeiling(feet)={num:F2}, MaxDepth={num2:F2}");
				Log.LogInfo((object)"[SWIM] Controls: Q=up, E=down, Space=jump at surface");
				_swimLoggedOnce = true;
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[SWIM] Physics error: " + ex.Message));
		}
	}

	public static bool ProcessFallOffStage_Prefix(object __instance)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			object obj = ((__instance is MonoBehaviour) ? __instance : null);
			Transform val = ((obj != null) ? ((Component)obj).transform : null);
			if ((Object)(object)val == (Object)null)
			{
				return true;
			}
			Vector3 position = val.position;
			float y = position.y;
			if (_positionInitialized)
			{
				float num = Vector3.Distance(position, _lastKnownPosition);
				if (num > 5f)
				{
					Log.LogInfo((object)$"[SWIM] Teleport detected (delta={num:F2}), resetting state machine");
					FullSwimmingStateReset();
					_lastKnownPosition = position;
					return true;
				}
			}
			_lastKnownPosition = position;
			_positionInitialized = true;
			CacheCharacterController(__instance);
			float num2 = y + (_characterHeight * 0.5f - _characterRadius);
			_cachedPlayerController = __instance;
			if (!_cacheInitialized)
			{
				Type type = AccessTools.TypeByName("Endless.Gameplay.LevelEditing.Level.StageManager");
				if (type == null)
				{
					return true;
				}
				PropertyInfo property = type.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
				if (property == null)
				{
					Type baseType = type.BaseType;
					while (baseType != null && property == null)
					{
						property = baseType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
						baseType = baseType.BaseType;
					}
				}
				if (property == null)
				{
					return true;
				}
				_cachedStageManager = property.GetValue(null);
				_cachedActiveStageProperty = type.GetProperty("ActiveStage");
				_cacheInitialized = true;
				Log.LogInfo((object)"[SWIM] StageManager cache initialized (v10.0.0)");
			}
			if (_cachedStageManager == null || _cachedActiveStageProperty == null)
			{
				return true;
			}
			object value = _cachedActiveStageProperty.GetValue(_cachedStageManager);
			if (value == null)
			{
				return true;
			}
			if (_cachedFallOffHeightProperty == null)
			{
				_cachedFallOffHeightProperty = value.GetType().GetProperty("StageFallOffHeight");
			}
			if (_cachedFallOffHeightProperty == null)
			{
				return true;
			}
			float num3 = (DepthPlaneY = (float)_cachedFallOffHeightProperty.GetValue(value));
			float waterSurfaceY = GetWaterSurfaceY(value, num3);
			if (waterSurfaceY == float.MinValue)
			{
				return true;
			}
			if (WaterSurfaceY == float.MinValue || _currentSwimState == SwimState.SURFACE)
			{
				WaterSurfaceY = waterSurfaceY;
			}
			switch (_currentSwimState)
			{
			case SwimState.SURFACE:
				if (num2 < waterSurfaceY)
				{
					_currentSwimState = SwimState.ENTERING;
					WaterEntryY = y;
					WaterSurfaceY = waterSurfaceY;
					IsPlayerUnderwater = true;
					Log.LogInfo((object)$"[SWIM] SURFACE -> ENTERING: waist {num2:F2} < surface {waterSurfaceY:F2}");
					Log.LogInfo((object)$"[SWIM] (Death zone is at Y={num3:F2}, KillOffset={num3 - waterSurfaceY:F2})");
					return false;
				}
				return true;
			case SwimState.ENTERING:
				if (y < waterSurfaceY)
				{
					_currentSwimState = SwimState.SWIMMING;
					IsSwimmingActive = true;
					_swimLoggedOnce = false;
					Log.LogInfo((object)$"[SWIM] ENTERING -> SWIMMING: feet {y:F2} < surface {waterSurfaceY:F2}");
				}
				return false;
			case SwimState.SWIMMING:
				IsPlayerUnderwater = true;
				IsSwimmingActive = true;
				UnderwaterDepth = waterSurfaceY - y;
				if (num2 > waterSurfaceY)
				{
					_currentSwimState = SwimState.EXITING;
					_exitGraceFrames = 30;
					IsSwimmingActive = false;
					Log.LogInfo((object)$"[SWIM] SWIMMING -> EXITING: waist {num2:F2} > surface {waterSurfaceY:F2}, grace={30}");
				}
				return false;
			case SwimState.EXITING:
			{
				_exitGraceFrames--;
				if (_jumpImmunityFrames > 0)
				{
					_jumpImmunityFrames--;
				}
				if (_jumpImmunityFrames <= 0 && num2 < waterSurfaceY)
				{
					_currentSwimState = SwimState.SWIMMING;
					IsSwimmingActive = true;
					IsPlayerUnderwater = true;
					Log.LogInfo((object)$"[SWIM] EXITING -> SWIMMING (re-entry): waist {num2:F2} < surface {waterSurfaceY:F2}");
					return false;
				}
				if (_exitGraceFrames > 0)
				{
					return false;
				}
				bool groundedState = GetGroundedState(__instance);
				bool flag = y > waterSurfaceY + 0.1f;
				if (groundedState || flag)
				{
					_currentSwimState = SwimState.SURFACE;
					FullSwimmingStateReset();
					Log.LogInfo((object)$"[SWIM] EXITING -> SURFACE: grounded={groundedState}, aboveWater={flag}, feet={y:F2}");
					return true;
				}
				if (_exitGraceFrames > -60)
				{
					_exitGraceFrames = 10;
					return false;
				}
				Log.LogWarning((object)"[SWIM] EXITING timeout, forcing reset");
				_currentSwimState = SwimState.SURFACE;
				FullSwimmingStateReset();
				return true;
			}
			default:
				return true;
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[SWIM] ProcessFallOffStage error: " + ex.Message));
			return true;
		}
	}

	private static float GetWaterSurfaceY(object activeStage, float fallbackHeight)
	{
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			PropertyInfo property = activeStage.GetType().GetProperty("DepthPlane", BindingFlags.Instance | BindingFlags.Public);
			if (property == null)
			{
				Log.LogWarning((object)"[WATER] Stage.DepthPlane property not found, trying field fallback");
				FieldInfo field = activeStage.GetType().GetField("depthPlane", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field == null)
				{
					return float.MinValue;
				}
				_cachedDepthPlane = field.GetValue(activeStage);
			}
			else
			{
				_cachedDepthPlane = property.GetValue(activeStage);
			}
			if (_cachedDepthPlane == null)
			{
				return float.MinValue;
			}
			PropertyInfo property2 = _cachedDepthPlane.GetType().GetProperty("OverrideFallOffHeight", BindingFlags.Instance | BindingFlags.Public);
			if (property2 != null && !(bool)property2.GetValue(_cachedDepthPlane))
			{
				return float.MinValue;
			}
			FieldInfo field2 = _cachedDepthPlane.GetType().GetField("planeParent", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field2 == null)
			{
				Log.LogError((object)"[WATER] planeParent field not found!");
				return float.MinValue;
			}
			object value = field2.GetValue(_cachedDepthPlane);
			Transform val = (Transform)((value is Transform) ? value : null);
			if ((Object)(object)val == (Object)null)
			{
				Log.LogError((object)"[WATER] planeParent is null!");
				return float.MinValue;
			}
			float y = val.position.y;
			if (!_waterDebugLogged)
			{
				LogWaterDebugInfo(_cachedDepthPlane, y, fallbackHeight);
				_waterDebugLogged = true;
			}
			return y;
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[WATER] GetWaterSurfaceY error: " + ex.Message));
			return float.MinValue;
		}
	}

	private static void LogWaterDebugInfo(object depthPlane, float waterSurfaceY, float deathZoneY)
	{
		try
		{
			Log.LogInfo((object)"========== WATER DEBUG INFO (v10.0.0) ==========");
			Log.LogInfo((object)$"[WATER-DEBUG] Water Surface Y (planeParent.position.y): {waterSurfaceY:F2}");
			Log.LogInfo((object)$"[WATER-DEBUG] Death Zone Y (StageFallOffHeight): {deathZoneY:F2}");
			Log.LogInfo((object)$"[WATER-DEBUG] Calculated KillOffset: {deathZoneY - waterSurfaceY:F2}");
			Log.LogInfo((object)"[WATER-DEBUG]   (If negative: death below surface = swimming safe)");
			Log.LogInfo((object)"[WATER-DEBUG]   (If positive: death above surface = dangerous!)");
			Log.LogInfo((object)"[WATER-DEBUG]   (If zero: death at surface = no swim zone)");
			if (depthPlane.GetType().GetField("visuals", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(depthPlane) is IList { Count: >0 } list)
			{
				Log.LogInfo((object)"[WATER-DEBUG] DepthPlaneInfo entries:");
				foreach (object item in list)
				{
					if (item != null)
					{
						FieldInfo field = item.GetType().GetField("PlaneType");
						FieldInfo field2 = item.GetType().GetField("KillOffset");
						if (field != null && field2 != null)
						{
							int num = (int)field.GetValue(item);
							float num2 = (float)field2.GetValue(item);
							string arg = num switch
							{
								1 => "Ocean", 
								0 => "Empty", 
								_ => $"Unknown({num})", 
							};
							Log.LogInfo((object)$"[WATER-DEBUG]   PlaneType={arg}, KillOffset={num2:F2}");
						}
					}
				}
			}
			else
			{
				Log.LogWarning((object)"[WATER-DEBUG] Could not read visuals list");
			}
			Log.LogInfo((object)"=================================================");
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[WATER-DEBUG] Error logging debug info: " + ex.Message));
		}
	}

	private static bool GetGroundedState(object playerController)
	{
		try
		{
			if (_currentStateField == null)
			{
				return false;
			}
			object value = _currentStateField.GetValue(playerController);
			if (value == null)
			{
				return false;
			}
			FieldInfo field = value.GetType().GetField("Grounded", BindingFlags.Instance | BindingFlags.Public);
			if (field == null)
			{
				return false;
			}
			return (bool)field.GetValue(value);
		}
		catch
		{
			return false;
		}
	}

	public static void ProcessJump_Prefix(object __instance)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (_currentSwimState != SwimState.SWIMMING || _currentStateField == null)
			{
				return;
			}
			object obj = ((__instance is MonoBehaviour) ? __instance : null);
			Transform val = ((obj != null) ? ((Component)obj).transform : null);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			CacheCharacterController(__instance);
			float num = val.position.y + (_characterHeight * 0.5f - _characterRadius);
			float num2 = WaterSurfaceY - 0.5f;
			if (num < num2 - 0.3f)
			{
				return;
			}
			object value = _currentStateField.GetValue(__instance);
			if (value == null)
			{
				return;
			}
			if (_framesSinceStableGroundField != null)
			{
				_framesSinceStableGroundField.SetValue(value, 0);
			}
			if (Input.GetKey((KeyCode)32))
			{
				if (_totalForceField != null)
				{
					Vector3 val2 = (Vector3)_totalForceField.GetValue(value);
					val2.y = 2.5f;
					_totalForceField.SetValue(value, val2);
				}
				_currentSwimState = SwimState.EXITING;
				_exitGraceFrames = 60;
				IsSwimmingActive = false;
				_jumpImmunityFrames = 15;
				Log.LogInfo((object)$"[SWIM-JUMP] Surface jump! waist={num:F2}, ceiling={num2:F2}, force={2.5f}");
			}
			_currentStateField.SetValue(__instance, value);
		}
		catch (Exception)
		{
		}
	}

	public static void SetAnimationState_Prefix(object __instance, ref float rotation, ref bool moving, ref bool walking, ref bool grounded, ref float slopeAngle, ref float airTime, ref float fallTime, ref Vector2 worldVelocity, ref float velX, ref float velY, ref float velZ, ref float angularVelocity, ref float horizontalVelMagnitude, ref string interactorToggleString, ref int comboBookmark, ref bool ghostmode, ref bool ads, ref float playerAngleDot, ref Vector3 aimPoint, ref bool useIK)
	{
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_06dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e2: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!grounded && velY < -5f && fallTime > 0.5f && (fallTime > _lastFallTime + 0.3f || !_fallAnimLogged))
			{
				Log.LogInfo((object)"========== FALLING ANIMATION CAPTURE (v10.5.0) ==========");
				Log.LogInfo((object)$"[FALL-ANIM] grounded={grounded}, airTime={airTime:F2}, fallTime={fallTime:F2}");
				Log.LogInfo((object)$"[FALL-ANIM] velX={velX:F2}, velY={velY:F2}, velZ={velZ:F2}");
				Log.LogInfo((object)$"[FALL-ANIM] horizontalVelMagnitude={horizontalVelMagnitude:F2}");
				Log.LogInfo((object)$"[FALL-ANIM] moving={moving}, walking={walking}, ghostmode={ghostmode}");
				Log.LogInfo((object)$"[FALL-ANIM] slopeAngle={slopeAngle:F2}, rotation={rotation:F2}");
				Log.LogInfo((object)$"[FALL-ANIM] playerAngleDot={playerAngleDot:F2}, useIK={useIK}");
				CacheAndLogAnimator(__instance);
				Log.LogInfo((object)"==========================================================");
				_fallAnimLogged = true;
				_lastFallTime = fallTime;
			}
			if (grounded && _fallAnimLogged)
			{
				Log.LogInfo((object)"[FALL-ANIM] Landed - resetting fall animation capture");
				_fallAnimLogged = false;
				_lastFallTime = 0f;
			}
			if (!IsSwimmingActive && (Object)(object)_cachedAnimator != (Object)null)
			{
				if (grounded)
				{
					_framesSinceGrounded++;
					if (((_framesSinceGrounded > 30) & moving) && horizontalVelMagnitude > 0.5f && !_idleHashCaptured)
					{
						AnimatorStateInfo currentAnimatorStateInfo = _cachedAnimator.GetCurrentAnimatorStateInfo(0);
						_idleStateHash = currentAnimatorStateInfo.shortNameHash;
						_idleHashCaptured = true;
						Log.LogInfo((object)$"[HASH-CAPTURE] LOCOMOTION state captured (moving): hash={_idleStateHash}");
					}
				}
				else
				{
					_framesSinceGrounded = 0;
				}
				if (!grounded && velY < -2f && fallTime > 0.1f && !_fallHashCaptured)
				{
					AnimatorStateInfo currentAnimatorStateInfo2 = _cachedAnimator.GetCurrentAnimatorStateInfo(0);
					_fallStateHash = currentAnimatorStateInfo2.shortNameHash;
					_fallHashCaptured = true;
					Log.LogInfo((object)$"[HASH-CAPTURE] FALL state captured: hash={_fallStateHash}");
				}
			}
			if (_currentSwimState != _lastLoggedSwimState)
			{
				Log.LogInfo((object)$"[ANIM-DIAG] SwimState changed: {_lastLoggedSwimState} -> {_currentSwimState}");
				Log.LogInfo((object)$"[ANIM-DIAG]   IsSwimmingActive={IsSwimmingActive}, grounded(input)={grounded}, airTime={airTime:F2}");
				_lastLoggedSwimState = _currentSwimState;
				_animLogCounter = 0;
			}
			if (grounded != _lastGroundedState)
			{
				_lastGroundedState = grounded;
			}
			if (!_animatorCached || (Object)(object)_cachedAnimator == (Object)null)
			{
				MonoBehaviour val = (MonoBehaviour)((__instance is MonoBehaviour) ? __instance : null);
				if ((Object)(object)val != (Object)null)
				{
					_cachedAnimator = ((Component)val).GetComponent<Animator>();
					if ((Object)(object)_cachedAnimator == (Object)null)
					{
						_cachedAnimator = ((Component)val).GetComponentInChildren<Animator>();
					}
				}
				_animatorCached = true;
				if ((Object)(object)_cachedAnimator != (Object)null)
				{
					Log.LogInfo((object)("[ANIM-DIAG] Animator cached: " + ((Object)_cachedAnimator).name));
				}
				else
				{
					Log.LogWarning((object)"[ANIM-DIAG] Failed to cache animator - null!");
				}
			}
			if (!IsSwimmingActive)
			{
				if (_wasInWater && (Object)(object)_cachedAnimator != (Object)null)
				{
					Log.LogInfo((object)"[SWIM-ANIM] Exiting water - firing WaterExit trigger");
					_cachedAnimator.SetBool("WaterIn", false);
					_cachedAnimator.SetBool("WaterInBelow", false);
					_cachedAnimator.SetBool("WaterBelow", false);
					_cachedAnimator.SetTrigger("WaterExit");
				}
				_wasInWater = false;
				_wasUnderwater = false;
				_isMovingInWater = false;
				_smoothedVelY = 0f;
				if (moving && (Object)(object)_cachedAnimator != (Object)null && _animLogCounter++ % 200 == 0)
				{
					int integer = _cachedAnimator.GetInteger("EquippedItem");
					AnimatorStateInfo currentAnimatorStateInfo3 = _cachedAnimator.GetCurrentAnimatorStateInfo(0);
					Log.LogInfo((object)$"[ANIM-STATE] NotSwimming: moving={moving}, grounded={grounded}, velX={velX:F2}, velZ={velZ:F2}, HorizVel={horizontalVelMagnitude:F2}");
					Log.LogInfo((object)$"[ANIM-STATE]   EquippedItem={integer}, StateHash={currentAnimatorStateInfo3.shortNameHash}, NormTime={currentAnimatorStateInfo3.normalizedTime:F2}");
				}
				return;
			}
			grounded = false;
			walking = false;
			float underwaterDepth = UnderwaterDepth;
			bool flag = underwaterDepth > 1.5f;
			bool flag2 = horizontalVelMagnitude > 0.1f;
			if ((Object)(object)_cachedAnimator != (Object)null)
			{
				if (!_wasInWater)
				{
					Log.LogInfo((object)$"[SWIM-ANIM] Entering water - depth={underwaterDepth:F2}, fallTime={fallTime:F2}");
					_cachedAnimator.SetBool("WaterIn", true);
					_cachedAnimator.SetBool("WaterBelow", true);
					_cachedAnimator.SetTrigger("WaterEnter");
					Log.LogInfo((object)"[SWIM-ANIM] Fired WaterEnter trigger");
					if (flag)
					{
						_cachedAnimator.SetBool("WaterInBelow", true);
						_cachedAnimator.SetTrigger("WaterDive");
						Log.LogInfo((object)"[SWIM-ANIM] Deep entry - also fired WaterDive trigger");
					}
					_wasInWater = true;
					_wasUnderwater = flag;
				}
				else
				{
					_cachedAnimator.SetBool("WaterIn", true);
					_cachedAnimator.SetBool("WaterBelow", true);
					_cachedAnimator.SetBool("WaterInBelow", flag);
					if (flag != _wasUnderwater)
					{
						if (flag)
						{
							Log.LogInfo((object)$"[SWIM-ANIM] DIVING - depth={underwaterDepth:F2}");
							_cachedAnimator.SetTrigger("WaterDive");
							_cachedAnimator.CrossFadeInFixedTime(HASH_DIVE, 0.3f, 0);
						}
						else
						{
							Log.LogInfo((object)$"[SWIM-ANIM] BREACHING - depth={underwaterDepth:F2}");
							_cachedAnimator.SetTrigger("WaterBreach");
							_cachedAnimator.CrossFadeInFixedTime(HASH_BREACH, 0.3f, 0);
						}
						_wasUnderwater = flag;
					}
					if (flag2 != _isMovingInWater)
					{
						if (flag2)
						{
							int num = (flag ? HASH_BELOW_WATER_SWIM_START : HASH_ABOVE_WATER_SWIM_START);
							Log.LogInfo((object)$"[SWIM-ANIM] Started swimming - underwater={flag}");
							_cachedAnimator.CrossFadeInFixedTime(num, 0.2f, 0);
						}
						_isMovingInWater = flag2;
					}
				}
				if (_diveLogThrottle++ % 100 == 0)
				{
					AnimatorStateInfo currentAnimatorStateInfo4 = _cachedAnimator.GetCurrentAnimatorStateInfo(0);
					Log.LogInfo((object)$"[SWIM-STATE] hash={currentAnimatorStateInfo4.shortNameHash}, depth={underwaterDepth:F2}, underwater={flag}, moving={flag2}");
				}
			}
			float num2 = 0f;
			if (flag)
			{
				num2 = (float)_swimVerticalMotor / (float)_swimMotorFrames * SwimmingVerticalSpeed;
				airTime = 0.5f;
				fallTime = (flag2 ? 0f : 1.5f);
			}
			else
			{
				airTime = 0.2f;
				fallTime = 0f;
				num2 = 0f;
			}
			_smoothedVelY = Mathf.Lerp(_smoothedVelY, num2, Time.deltaTime * 3f);
			velY = _smoothedVelY;
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[ANIM] Error: " + ex.Message));
		}
	}

	private static void CacheAnimatorAndLogStates(object appearanceAnimator)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!_animatorCached)
			{
				MonoBehaviour val = (MonoBehaviour)((appearanceAnimator is MonoBehaviour) ? appearanceAnimator : null);
				if ((Object)(object)val != (Object)null)
				{
					_cachedAnimator = ((Component)val).GetComponent<Animator>();
					if ((Object)(object)_cachedAnimator == (Object)null)
					{
						_cachedAnimator = ((Component)val).GetComponentInChildren<Animator>();
					}
				}
				_animatorCached = true;
				if ((Object)(object)_cachedAnimator != (Object)null)
				{
					Log.LogInfo((object)("[ANIM-CTRL] Animator cached: " + ((Object)_cachedAnimator).name));
				}
			}
			if (!((Object)(object)_cachedAnimator != (Object)null) || _animatorStatesLogged)
			{
				return;
			}
			_animatorStatesLogged = true;
			Log.LogInfo((object)"========== ANIMATOR STATES DISCOVERY (v10.8.0) ==========");
			AnimatorStateInfo currentAnimatorStateInfo = _cachedAnimator.GetCurrentAnimatorStateInfo(0);
			Log.LogInfo((object)$"[ANIM-CTRL] Current state hash: {currentAnimatorStateInfo.shortNameHash}");
			Log.LogInfo((object)$"[ANIM-CTRL] Current fullPath hash: {currentAnimatorStateInfo.fullPathHash}");
			AnimatorClipInfo[] currentAnimatorClipInfo = _cachedAnimator.GetCurrentAnimatorClipInfo(0);
			for (int i = 0; i < currentAnimatorClipInfo.Length; i++)
			{
				AnimatorClipInfo val2 = currentAnimatorClipInfo[i];
				if ((Object)(object)val2.clip != (Object)null)
				{
					Log.LogInfo((object)("[ANIM-CTRL] Current clip: " + ((Object)val2.clip).name));
				}
			}
			Log.LogInfo((object)"[ANIM-CTRL] Parameters:");
			AnimatorControllerParameter[] parameters = _cachedAnimator.parameters;
			foreach (AnimatorControllerParameter val3 in parameters)
			{
				Log.LogInfo((object)$"[ANIM-CTRL]   {val3.name} ({val3.type})");
			}
			string[] array = new string[7] { "Fall", "Falling", "FallLoop", "AirFall", "FreeFall", "Dive", "FallIdle" };
			string[] array2 = new string[6] { "Idle", "Standing", "Grounded", "Locomotion", "Blend Tree", "Movement" };
			string[] array3 = array;
			foreach (string text in array3)
			{
				int num = Animator.StringToHash(text);
				if (_cachedAnimator.HasState(0, num))
				{
					Log.LogInfo((object)$"[ANIM-CTRL] FOUND FALL STATE: {text} (hash={num})");
					if (_fallStateHash == 0)
					{
						_fallStateHash = num;
					}
				}
			}
			array3 = array2;
			foreach (string text2 in array3)
			{
				int num2 = Animator.StringToHash(text2);
				if (_cachedAnimator.HasState(0, num2))
				{
					Log.LogInfo((object)$"[ANIM-CTRL] FOUND IDLE STATE: {text2} (hash={num2})");
					if (_idleStateHash == 0)
					{
						_idleStateHash = num2;
					}
				}
			}
			Log.LogInfo((object)$"[ANIM-CTRL] Using fallStateHash={_fallStateHash}, idleStateHash={_idleStateHash}");
			Log.LogInfo((object)"==========================================================");
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[ANIM-CTRL] Error: " + ex.Message));
		}
	}

	private static void CacheAndLogAnimator(object appearanceAnimator)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Expected I4, but got Unknown
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Invalid comparison between Unknown and I4
		try
		{
			if (!_animatorCached)
			{
				MonoBehaviour val = (MonoBehaviour)((appearanceAnimator is MonoBehaviour) ? appearanceAnimator : null);
				if ((Object)(object)val != (Object)null)
				{
					_cachedAnimator = ((Component)val).GetComponent<Animator>();
					if ((Object)(object)_cachedAnimator == (Object)null)
					{
						_cachedAnimator = ((Component)val).GetComponentInChildren<Animator>();
					}
				}
				_animatorCached = true;
				if ((Object)(object)_cachedAnimator != (Object)null)
				{
					Log.LogInfo((object)("[FALL-ANIM] Animator found: " + ((Object)_cachedAnimator).name));
				}
				else
				{
					Log.LogWarning((object)"[FALL-ANIM] No Animator component found");
				}
			}
			if (!((Object)(object)_cachedAnimator != (Object)null) || !((Behaviour)_cachedAnimator).isActiveAndEnabled)
			{
				return;
			}
			AnimatorStateInfo currentAnimatorStateInfo = _cachedAnimator.GetCurrentAnimatorStateInfo(0);
			AnimatorClipInfo[] currentAnimatorClipInfo = _cachedAnimator.GetCurrentAnimatorClipInfo(0);
			Log.LogInfo((object)"[FALL-ANIM] Animator Layer 0 State:");
			Log.LogInfo((object)$"[FALL-ANIM]   stateInfo.fullPathHash={currentAnimatorStateInfo.fullPathHash}");
			Log.LogInfo((object)$"[FALL-ANIM]   stateInfo.shortNameHash={currentAnimatorStateInfo.shortNameHash}");
			Log.LogInfo((object)$"[FALL-ANIM]   stateInfo.normalizedTime={currentAnimatorStateInfo.normalizedTime:F2}");
			Log.LogInfo((object)$"[FALL-ANIM]   stateInfo.length={currentAnimatorStateInfo.length:F2}");
			Log.LogInfo((object)$"[FALL-ANIM]   stateInfo.speed={currentAnimatorStateInfo.speed:F2}");
			Log.LogInfo((object)$"[FALL-ANIM]   stateInfo.loop={currentAnimatorStateInfo.loop}");
			if (currentAnimatorClipInfo.Length != 0)
			{
				AnimatorClipInfo[] array = currentAnimatorClipInfo;
				for (int i = 0; i < array.Length; i++)
				{
					AnimatorClipInfo val2 = array[i];
					if ((Object)(object)val2.clip != (Object)null)
					{
						Log.LogInfo((object)$"[FALL-ANIM]   CLIP: {((Object)val2.clip).name} (weight={val2.weight:F2})");
					}
				}
			}
			else
			{
				Log.LogInfo((object)"[FALL-ANIM]   No clip info available (may be using state name)");
			}
			Log.LogInfo((object)"[FALL-ANIM] Animator Parameters:");
			AnimatorControllerParameter[] parameters = _cachedAnimator.parameters;
			foreach (AnimatorControllerParameter val3 in parameters)
			{
				AnimatorControllerParameterType type = val3.type;
				switch ((int)type - 1)
				{
				case 0:
					Log.LogInfo((object)$"[FALL-ANIM]   {val3.name} (float) = {_cachedAnimator.GetFloat(val3.name):F2}");
					continue;
				case 2:
					Log.LogInfo((object)$"[FALL-ANIM]   {val3.name} (int) = {_cachedAnimator.GetInteger(val3.name)}");
					continue;
				case 3:
					Log.LogInfo((object)$"[FALL-ANIM]   {val3.name} (bool) = {_cachedAnimator.GetBool(val3.name)}");
					continue;
				case 1:
					continue;
				}
				if ((int)type == 9)
				{
					Log.LogInfo((object)("[FALL-ANIM]   " + val3.name + " (trigger)"));
				}
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[FALL-ANIM] Animator access error: " + ex.Message));
		}
	}

	public static void DepthPlane_Start_Postfix(object __instance)
	{
		try
		{
			if (_shaderAlreadyModified)
			{
				Log.LogInfo((object)"[SHADER] Already modified this session, skipping");
				return;
			}
			Log.LogInfo((object)"[SHADER] DepthPlane.Start - Modifying Ocean_Plane shader");
			MonoBehaviour val = (MonoBehaviour)((__instance is MonoBehaviour) ? __instance : null);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			Transform val2 = FindChildRecursive(((Component)val).transform, "Ocean_Plane");
			if ((Object)(object)val2 == (Object)null)
			{
				Log.LogWarning((object)"[SHADER] Could not find Ocean_Plane child");
				return;
			}
			MeshRenderer component = ((Component)val2).GetComponent<MeshRenderer>();
			if ((Object)(object)component == (Object)null)
			{
				Log.LogWarning((object)"[SHADER] Ocean_Plane has no MeshRenderer");
				return;
			}
			Material[] sharedMaterials = ((Renderer)component).sharedMaterials;
			bool flag = false;
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				Material val3 = sharedMaterials[i];
				if (!((Object)(object)val3 == (Object)null))
				{
					ManualLogSource log = Log;
					object arg = i;
					string name = ((Object)val3).name;
					Shader shader = val3.shader;
					log.LogInfo((object)$"[SHADER] Material[{arg}]: {name}, shader: {((shader != null) ? ((Object)shader).name : null)}");
					if (val3.HasProperty("_Refraction"))
					{
						float num = val3.GetFloat("_Refraction");
						val3.SetFloat("_Refraction", 0f);
						Log.LogInfo((object)$"[SHADER]   _Refraction: {num:F3} -> 0.000");
						flag = true;
					}
					if (val3.HasProperty("_Depth"))
					{
						float num2 = val3.GetFloat("_Depth");
						val3.SetFloat("_Depth", num2 * 0.2f);
						Log.LogInfo((object)$"[SHADER]   _Depth: {num2:F3} -> {num2 * 0.2f:F3}");
						flag = true;
					}
					if (val3.HasProperty("_Displacement"))
					{
						float num3 = val3.GetFloat("_Displacement");
						val3.SetFloat("_Displacement", num3 * 0.1f);
						Log.LogInfo((object)$"[SHADER]   _Displacement: {num3:F3} -> {num3 * 0.1f:F3}");
						flag = true;
					}
				}
			}
			if (flag)
			{
				_shaderAlreadyModified = true;
			}
			DisableDeeperPlane(val);
		}
		catch (Exception arg2)
		{
			Log.LogError((object)$"[SHADER] Error: {arg2}");
		}
	}

	private static void DisableDeeperPlane(MonoBehaviour depthPlane)
	{
		try
		{
			FieldInfo fieldInfo = AccessTools.Field(_depthPlaneType, "visuals");
			if (fieldInfo == null || !(fieldInfo.GetValue(depthPlane) is IList { Count: not 0 } list))
			{
				return;
			}
			foreach (object item in list)
			{
				if (item == null)
				{
					continue;
				}
				FieldInfo field = item.GetType().GetField("DeeperPlane");
				if (field == null)
				{
					continue;
				}
				object value = field.GetValue(item);
				GameObject val = (GameObject)((value is GameObject) ? value : null);
				if (!((Object)(object)val == (Object)null))
				{
					MeshRenderer component = val.GetComponent<MeshRenderer>();
					if ((Object)(object)component != (Object)null)
					{
						((Renderer)component).enabled = false;
						Log.LogInfo((object)("[DEEPER] Disabled DeeperPlane renderer: " + ((Object)val).name));
					}
				}
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[DEEPER] Error: " + ex.Message));
		}
	}

	public static bool StartTransition_Prefix(object filterType)
	{
		try
		{
			if ((int)filterType == 3)
			{
				if (!_filterLoggedOnce)
				{
					Log.LogInfo((object)"[FILTER] Blocked Blurred filter transition");
					_filterLoggedOnce = true;
				}
				return false;
			}
			return true;
		}
		catch (Exception)
		{
			return true;
		}
	}

	private static void LogAllVolumeData()
	{
		if (_volumeDiagnosticsLogged)
		{
			return;
		}
		_volumeDiagnosticsLogged = true;
		try
		{
			Log.LogInfo((object)"==========================================================");
			Log.LogInfo((object)"[VOLUME-DIAG] ===== POST-PROCESSING VOLUME DIAGNOSTICS =====");
			Log.LogInfo((object)"==========================================================");
			Type type = AccessTools.TypeByName("UnityEngine.Rendering.Volume");
			if (type == null)
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly assembly in assemblies)
				{
					if (assembly.FullName.Contains("RenderPipelines") || assembly.FullName.Contains("Rendering"))
					{
						type = assembly.GetType("UnityEngine.Rendering.Volume");
						if (type != null)
						{
							break;
						}
					}
				}
			}
			if (type == null)
			{
				Log.LogWarning((object)"[VOLUME-DIAG] Volume type NOT FOUND in any assembly");
				return;
			}
			Log.LogInfo((object)("[VOLUME-DIAG] Volume type found: " + type.FullName));
			PropertyInfo property = type.GetProperty("weight");
			PropertyInfo property2 = type.GetProperty("priority");
			PropertyInfo property3 = type.GetProperty("isGlobal");
			PropertyInfo property4 = type.GetProperty("profile");
			PropertyInfo property5 = type.GetProperty("sharedProfile");
			Log.LogInfo((object)$"[VOLUME-DIAG] Properties found: weight={property != null}, priority={property2 != null}, isGlobal={property3 != null}, profile={property4 != null}");
			Object[] array = Object.FindObjectsOfType(type);
			Log.LogInfo((object)$"[VOLUME-DIAG] Total Volumes in scene: {array.Length}");
			Log.LogInfo((object)"----------------------------------------------------------");
			int num = 0;
			Object[] array2 = array;
			foreach (Object val in array2)
			{
				MonoBehaviour val2 = (MonoBehaviour)(object)((val is MonoBehaviour) ? val : null);
				if ((Object)(object)val2 == (Object)null)
				{
					continue;
				}
				num++;
				string name = ((Object)val2).name;
				string gameObjectPath = GetGameObjectPath(((Component)val2).gameObject);
				bool enabled = ((Behaviour)val2).enabled;
				Log.LogInfo((object)$"[VOLUME-DIAG] [{num}] === {name} ===");
				Log.LogInfo((object)("[VOLUME-DIAG]     Path: " + gameObjectPath));
				Log.LogInfo((object)$"[VOLUME-DIAG]     Enabled: {enabled}");
				if (property != null)
				{
					float num2 = (float)property.GetValue(val);
					Log.LogInfo((object)$"[VOLUME-DIAG]     Weight: {num2:F3}");
				}
				if (property2 != null)
				{
					float num3 = (float)property2.GetValue(val);
					Log.LogInfo((object)$"[VOLUME-DIAG]     Priority: {num3}");
				}
				if (property3 != null)
				{
					bool flag = (bool)property3.GetValue(val);
					Log.LogInfo((object)$"[VOLUME-DIAG]     IsGlobal: {flag}");
				}
				if (property4 != null)
				{
					object value = property4.GetValue(val);
					if (value != null)
					{
						string text = value.ToString();
						Log.LogInfo((object)("[VOLUME-DIAG]     Profile: " + text));
						LogVolumeProfileContents(value);
					}
					else
					{
						Log.LogInfo((object)"[VOLUME-DIAG]     Profile: NULL");
					}
				}
				if (property5 != null)
				{
					object value2 = property5.GetValue(val);
					if (value2 != null)
					{
						string text2 = value2.ToString();
						Log.LogInfo((object)("[VOLUME-DIAG]     SharedProfile: " + text2));
					}
				}
				Log.LogInfo((object)"----------------------------------------------------------");
			}
			LogAllVolumeProfiles();
			Log.LogInfo((object)"==========================================================");
			Log.LogInfo((object)"[VOLUME-DIAG] ===== END VOLUME DIAGNOSTICS =====");
			Log.LogInfo((object)"==========================================================");
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[VOLUME-DIAG] Error: " + ex.Message + "\n" + ex.StackTrace));
		}
	}

	private static string GetGameObjectPath(GameObject go)
	{
		string text = ((Object)go).name;
		Transform parent = go.transform.parent;
		while ((Object)(object)parent != (Object)null)
		{
			text = ((Object)parent).name + "/" + text;
			parent = parent.parent;
		}
		return text;
	}

	private static void LogVolumeProfileContents(object profile)
	{
		try
		{
			Type type = profile.GetType();
			PropertyInfo property = type.GetProperty("components");
			if (property == null)
			{
				FieldInfo field = type.GetField("components", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (!(field != null) || !(field.GetValue(profile) is IList list))
				{
					return;
				}
				Log.LogInfo((object)$"[VOLUME-DIAG]     Effects ({list.Count}):");
				{
					foreach (object item in list)
					{
						LogVolumeComponent(item);
					}
					return;
				}
			}
			if (!(property.GetValue(profile) is IList list2))
			{
				return;
			}
			Log.LogInfo((object)$"[VOLUME-DIAG]     Effects ({list2.Count}):");
			foreach (object item2 in list2)
			{
				LogVolumeComponent(item2);
			}
		}
		catch (Exception ex)
		{
			Log.LogWarning((object)("[VOLUME-DIAG]     Could not read profile contents: " + ex.Message));
		}
	}

	private static void LogVolumeComponent(object comp)
	{
		try
		{
			if (comp != null)
			{
				Type type = comp.GetType();
				string name = type.Name;
				PropertyInfo property = type.GetProperty("active");
				bool flag = !(property != null) || (bool)property.GetValue(comp);
				Log.LogInfo((object)$"[VOLUME-DIAG]       - {name} (active={flag})");
				if (name.Contains("Bloom"))
				{
					LogEffectParameter(comp, "intensity");
					LogEffectParameter(comp, "threshold");
					LogEffectParameter(comp, "scatter");
				}
				else if (name.Contains("DepthOfField") || name.Contains("DOF"))
				{
					LogEffectParameter(comp, "mode");
					LogEffectParameter(comp, "focusDistance");
					LogEffectParameter(comp, "aperture");
					LogEffectParameter(comp, "focalLength");
				}
				else if (name.Contains("ColorAdjustments") || name.Contains("ColorGrading"))
				{
					LogEffectParameter(comp, "postExposure");
					LogEffectParameter(comp, "contrast");
					LogEffectParameter(comp, "saturation");
				}
				else if (name.Contains("Vignette"))
				{
					LogEffectParameter(comp, "intensity");
					LogEffectParameter(comp, "smoothness");
				}
				else if (name.Contains("ChromaticAberration"))
				{
					LogEffectParameter(comp, "intensity");
				}
				else if (name.Contains("Fog") || name.Contains("Underwater"))
				{
					LogAllEffectParameters(comp);
				}
			}
		}
		catch (Exception ex)
		{
			Log.LogWarning((object)("[VOLUME-DIAG]       Error reading component: " + ex.Message));
		}
	}

	private static void LogEffectParameter(object comp, string paramName)
	{
		try
		{
			FieldInfo field = comp.GetType().GetField(paramName, BindingFlags.Instance | BindingFlags.Public);
			if (!(field != null))
			{
				return;
			}
			object value = field.GetValue(comp);
			if (value != null)
			{
				PropertyInfo property = value.GetType().GetProperty("value");
				if (property != null)
				{
					object value2 = property.GetValue(value);
					Log.LogInfo((object)$"[VOLUME-DIAG]           {paramName}: {value2}");
				}
				else
				{
					Log.LogInfo((object)$"[VOLUME-DIAG]           {paramName}: {value}");
				}
			}
		}
		catch
		{
		}
	}

	private static void LogAllEffectParameters(object comp)
	{
		try
		{
			FieldInfo[] fields = comp.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.Name == "active")
				{
					continue;
				}
				try
				{
					object value = fieldInfo.GetValue(comp);
					if (value != null)
					{
						PropertyInfo property = value.GetType().GetProperty("value");
						if (property != null)
						{
							object value2 = property.GetValue(value);
							Log.LogInfo((object)$"[VOLUME-DIAG]           {fieldInfo.Name}: {value2}");
						}
					}
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}

	private static void LogAllVolumeProfiles()
	{
		try
		{
			Type type = AccessTools.TypeByName("UnityEngine.Rendering.VolumeProfile");
			if (!(type == null))
			{
				Object[] array = Resources.FindObjectsOfTypeAll(type);
				Log.LogInfo((object)$"[VOLUME-DIAG] Found {array.Length} VolumeProfile assets in memory:");
				Object[] array2 = array;
				foreach (Object obj in array2)
				{
					string text = ((obj != null) ? obj.name : null) ?? "Unknown";
					Log.LogInfo((object)("[VOLUME-DIAG]   - " + text));
				}
			}
		}
		catch (Exception ex)
		{
			Log.LogWarning((object)("[VOLUME-DIAG] Could not enumerate VolumeProfiles: " + ex.Message));
		}
	}

	private static void TryReduceBlur()
	{
		LogAllVolumeData();
		try
		{
			Type type = null;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				if (assembly.FullName.Contains("RenderPipelines") || assembly.FullName.Contains("Rendering"))
				{
					type = assembly.GetType("UnityEngine.Rendering.Volume");
					if (type != null)
					{
						break;
					}
				}
			}
			if (type == null)
			{
				type = AccessTools.TypeByName("UnityEngine.Rendering.Volume");
			}
			if (type == null)
			{
				Log.LogWarning((object)"[BLUR] Volume type not found");
				_blurReductionApplied = true;
				return;
			}
			FieldInfo field = type.GetField("weight", BindingFlags.Instance | BindingFlags.Public);
			if (field == null)
			{
				PropertyInfo property = type.GetProperty("weight", BindingFlags.Instance | BindingFlags.Public);
				if (property != null)
				{
					Log.LogInfo((object)"[BLUR] weight is a property, not a field");
					TryReduceBlurViaProperty(type, property);
				}
				else
				{
					Log.LogWarning((object)"[BLUR] Volume.weight field/property not found");
					_blurReductionApplied = true;
				}
				return;
			}
			Log.LogInfo((object)"[BLUR] Found Volume.weight as FIELD");
			Object[] array = Object.FindObjectsOfType(type);
			Log.LogInfo((object)$"[BLUR] Found {array.Length} Volume components");
			Object[] array2 = array;
			foreach (Object val in array2)
			{
				MonoBehaviour val2 = (MonoBehaviour)(object)((val is MonoBehaviour) ? val : null);
				if ((Object)(object)val2 == (Object)null || !((Behaviour)val2).enabled)
				{
					continue;
				}
				string text = ((Object)val2).name.ToLower();
				if (text.Contains("under") && text.Contains("water"))
				{
					float num = (float)field.GetValue(val);
					if (!_originalVolumeWeights.ContainsKey(val))
					{
						_originalVolumeWeights[val] = num;
					}
					field.SetValue(val, 0.35f);
					Log.LogInfo((object)$"[BLUR] Underwater volume '{((Object)val2).name}' weight: {num:F2} -> {0.35f:F2}");
					ApplyUnderwaterColorTint(val);
				}
				else
				{
					Log.LogInfo((object)("[BLUR] Keeping '" + ((Object)val2).name + "' at original weight (not underwater)"));
				}
			}
			_blurReductionApplied = true;
			Log.LogInfo((object)$"[BLUR] Modified {_originalVolumeWeights.Count} underwater volume(s), kept other volumes intact");
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[BLUR] Error: " + ex.Message));
			_blurReductionApplied = true;
		}
	}

	private static void TryReduceBlurViaProperty(Type volumeType, PropertyInfo weightProp)
	{
		try
		{
			Object[] array = Object.FindObjectsOfType(volumeType);
			foreach (Object val in array)
			{
				MonoBehaviour val2 = (MonoBehaviour)(object)((val is MonoBehaviour) ? val : null);
				if ((Object)(object)val2 == (Object)null || !((Behaviour)val2).enabled)
				{
					continue;
				}
				string text = ((Object)val2).name.ToLower();
				if (text.Contains("under") && text.Contains("water"))
				{
					float num = (float)weightProp.GetValue(val);
					if (!_originalVolumeWeights.ContainsKey(val))
					{
						_originalVolumeWeights[val] = num;
					}
					weightProp.SetValue(val, 0.35f);
					Log.LogInfo((object)$"[BLUR] Underwater volume '{((Object)val2).name}' weight (via prop): {num:F2} -> {0.35f:F2}");
				}
				else
				{
					Log.LogInfo((object)("[BLUR] Keeping '" + ((Object)val2).name + "' at original weight (not underwater)"));
				}
			}
			_blurReductionApplied = true;
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[BLUR] Error via property: " + ex.Message));
			_blurReductionApplied = true;
		}
	}

	private static void TryRestoreBlur()
	{
		try
		{
			Type type = AccessTools.TypeByName("UnityEngine.Rendering.Volume");
			if (type == null)
			{
				_originalVolumeWeights.Clear();
				_blurReductionApplied = false;
				return;
			}
			FieldInfo field = type.GetField("weight", BindingFlags.Instance | BindingFlags.Public);
			PropertyInfo propertyInfo = ((field == null) ? type.GetProperty("weight") : null);
			foreach (KeyValuePair<object, float> originalVolumeWeight in _originalVolumeWeights)
			{
				if (originalVolumeWeight.Key != null)
				{
					if (field != null)
					{
						field.SetValue(originalVolumeWeight.Key, originalVolumeWeight.Value);
					}
					else if (propertyInfo != null)
					{
						propertyInfo.SetValue(originalVolumeWeight.Key, originalVolumeWeight.Value);
					}
				}
			}
			Log.LogInfo((object)$"[BLUR] Restored {_originalVolumeWeights.Count} volumes");
			_originalVolumeWeights.Clear();
			_blurReductionApplied = false;
			RestoreUnderwaterColorTint();
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[BLUR] Restore error: " + ex.Message));
			_originalVolumeWeights.Clear();
			_blurReductionApplied = false;
		}
	}

	private static void ApplyUnderwaterColorTint(object volume)
	{
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			PropertyInfo property = volume.GetType().GetProperty("profile");
			if (property == null)
			{
				return;
			}
			object value = property.GetValue(volume);
			if (value == null)
			{
				return;
			}
			FieldInfo field = value.GetType().GetField("components", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null || !(field.GetValue(value) is IList list))
			{
				return;
			}
			foreach (object item in list)
			{
				if (item == null || !item.GetType().Name.Contains("ColorAdjustments"))
				{
					continue;
				}
				_cachedColorAdjustments = item;
				_colorFilterField = item.GetType().GetField("colorFilter", BindingFlags.Instance | BindingFlags.Public);
				if (_colorFilterField == null)
				{
					Log.LogWarning((object)"[UNDERWATER] ColorAdjustments found but no colorFilter field");
					return;
				}
				object value2 = _colorFilterField.GetValue(item);
				if (value2 == null)
				{
					Log.LogWarning((object)"[UNDERWATER] colorFilter parameter is null");
					return;
				}
				PropertyInfo property2 = value2.GetType().GetProperty("value");
				if (property2 != null)
				{
					if (!_colorFilterModified)
					{
						_originalColorFilter = property2.GetValue(value2);
					}
					property2.SetValue(value2, UNDERWATER_TINT);
					_colorFilterModified = true;
					Log.LogInfo((object)$"[UNDERWATER] Applied ocean blue tint: {UNDERWATER_TINT}");
				}
				PropertyInfo property3 = value2.GetType().GetProperty("overrideState");
				if (property3 != null)
				{
					property3.SetValue(value2, true);
				}
				return;
			}
			Log.LogInfo((object)"[UNDERWATER] No ColorAdjustments component found in underwater volume");
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[UNDERWATER] Error applying color tint: " + ex.Message));
		}
	}

	private static void RestoreUnderwaterColorTint()
	{
		try
		{
			if (!_colorFilterModified || _cachedColorAdjustments == null || _colorFilterField == null)
			{
				return;
			}
			object value = _colorFilterField.GetValue(_cachedColorAdjustments);
			if (value != null)
			{
				PropertyInfo property = value.GetType().GetProperty("value");
				if (property != null && _originalColorFilter != null)
				{
					property.SetValue(value, _originalColorFilter);
					Log.LogInfo((object)"[UNDERWATER] Restored original color filter");
				}
				_colorFilterModified = false;
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[UNDERWATER] Error restoring color: " + ex.Message));
		}
	}

	private static Transform FindChildRecursive(Transform parent, string name)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		foreach (Transform item in parent)
		{
			Transform val = item;
			if (((Object)val).name == name)
			{
				return val;
			}
			Transform val2 = FindChildRecursive(val, name);
			if ((Object)(object)val2 != (Object)null)
			{
				return val2;
			}
		}
		return null;
	}

	private void PatchStageManager()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		if (_stageManagerType != null)
		{
			MethodInfo methodInfo = AccessTools.Method(_stageManagerType, "Awake", (Type[])null, (Type[])null);
			if (methodInfo != null)
			{
				HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("StageManager_Awake_Postfix", BindingFlags.Static | BindingFlags.Public));
				_harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				Log.LogInfo((object)"[INJECTOR] Patched StageManager.Awake");
			}
		}
	}

	private void PatchEndlessPropBuildPrefab()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		if (_endlessPropType != null)
		{
			MethodInfo methodInfo = AccessTools.Method(_endlessPropType, "BuildPrefab", (Type[])null, (Type[])null);
			if (methodInfo != null)
			{
				HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("BuildPrefab_Prefix", BindingFlags.Static | BindingFlags.Public));
				HarmonyMethod val2 = new HarmonyMethod(typeof(Plugin).GetMethod("BuildPrefab_Postfix", BindingFlags.Static | BindingFlags.Public));
				_harmony.Patch((MethodBase)methodInfo, val, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				Log.LogInfo((object)"[INJECTOR] Patched EndlessProp.BuildPrefab");
			}
		}
	}

	private void PatchSpawnPreview()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		if (_propBasedToolType != null)
		{
			MethodInfo methodInfo = AccessTools.Method(_propBasedToolType, "SpawnPreview", (Type[])null, (Type[])null);
			if (methodInfo != null)
			{
				HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("SpawnPreview_Prefix", BindingFlags.Static | BindingFlags.Public));
				HarmonyMethod val2 = new HarmonyMethod(typeof(Plugin).GetMethod("SpawnPreview_Postfix", BindingFlags.Static | BindingFlags.Public));
				_harmony.Patch((MethodBase)methodInfo, val, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				Log.LogInfo((object)"[INJECTOR] Patched PropBasedTool.SpawnPreview");
			}
		}
	}

	private void PatchFindAndManageChildRenderers()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		if (_endlessPropType != null)
		{
			MethodInfo methodInfo = AccessTools.Method(_endlessPropType, "FindAndManageChildRenderers", (Type[])null, (Type[])null);
			if (methodInfo != null)
			{
				HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("FindAndManageChildRenderers_Prefix", BindingFlags.Static | BindingFlags.Public));
				HarmonyMethod val2 = new HarmonyMethod(typeof(Plugin).GetMethod("FindAndManageChildRenderers_Postfix", BindingFlags.Static | BindingFlags.Public));
				_harmony.Patch((MethodBase)methodInfo, val, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				Log.LogInfo((object)"[INJECTOR] Patched EndlessProp.FindAndManageChildRenderers");
			}
		}
	}

	public static void BuildPrefab_Prefix(object __instance, object prop, GameObject testPrefab, object testScript)
	{
		try
		{
			string text = ((AccessTools.TypeByName("Endless.Assets.AssetCore")?.GetField("Name"))?.GetValue(prop) as string) ?? "UNKNOWN";
			if (((AccessTools.Field(_propType, "baseTypeId")?.GetValue(prop) as string) ?? "UNKNOWN") == CustomBaseTypeGuid)
			{
				Log.LogInfo((object)("[TRACE] Building custom prop: " + text));
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[TRACE] BuildPrefab_Prefix error: " + ex.Message));
		}
	}

	public static void BuildPrefab_Postfix(object __instance)
	{
	}

	public static void SpawnPreview_Prefix(object __instance, object runtimePropInfo)
	{
	}

	public static void SpawnPreview_Postfix(object __instance)
	{
	}

	public static void FindAndManageChildRenderers_Prefix(object __instance, GameObject targetObject)
	{
	}

	public static void FindAndManageChildRenderers_Postfix(object __instance)
	{
	}

	public static void StageManager_Awake_Postfix(object __instance)
	{
		try
		{
			Log.LogInfo((object)"[INJECTOR] StageManager.Awake - Creating custom prop");
			LogAvailableShaders();
			CreateCustomPrefab();
			CreateCustomBaseTypeDefinition();
			InjectDefinitionIntoBaseTypeList(__instance);
			InjectCustomProp(__instance);
			MonoBehaviour val = (MonoBehaviour)((__instance is MonoBehaviour) ? __instance : null);
			if (val != null)
			{
				val.StartCoroutine(DelayedCollectibleResearch());
			}
		}
		catch (Exception arg)
		{
			Log.LogError((object)$"[INJECTOR] StageManager.Awake failed: {arg}");
		}
	}

	private static IEnumerator DelayedCollectibleResearch()
	{
		yield return null;
		yield return null;
		yield return (object)new WaitForSeconds(2f);
		ResearchCollectibleSystem();
	}

	private static void InjectDefinitionIntoBaseTypeList(object stageManager)
	{
		try
		{
			object obj = AccessTools.Field(_stageManagerType, "baseTypeList")?.GetValue(stageManager);
			if (obj == null)
			{
				return;
			}
			object obj2 = AccessTools.Field(_abstractComponentListBaseType, "components")?.GetValue(obj);
			if (obj2 != null)
			{
				obj2.GetType().GetMethod("Add")?.Invoke(obj2, new object[1] { _customBaseTypeDefinition });
				FieldInfo fieldInfo = AccessTools.Field(_abstractComponentListBaseType, "definitionMap");
				if ((object)fieldInfo != null && fieldInfo.GetValue(obj) != null)
				{
					fieldInfo.SetValue(obj, null);
				}
			}
		}
		catch (Exception arg)
		{
			Log.LogError((object)$"[INJECTOR] InjectDefinitionIntoBaseTypeList failed: {arg}");
		}
	}

	private static void FindTreasureDefinition()
	{
		if (_cachedTreasureDefinition != null)
		{
			return;
		}
		try
		{
			Type type = AccessTools.TypeByName("Endless.Gameplay.InventoryUsableDefinition");
			if (type == null)
			{
				type = _genericUsableDefinitionType?.BaseType;
			}
			if (type == null)
			{
				Log.LogWarning((object)"[COLLECT] InventoryUsableDefinition type not found");
				return;
			}
			Object[] array = Resources.FindObjectsOfTypeAll(type);
			Log.LogInfo((object)$"[COLLECT] Found {array.Length} InventoryUsableDefinition instances");
			Object[] array2 = array;
			foreach (Object val in array2)
			{
				ScriptableObject val2 = (ScriptableObject)(object)((val is ScriptableObject) ? val : null);
				if ((Object)(object)val2 != (Object)null && ((Object)val2).name.Contains("Treasure") && ((Object)val2).name.Contains("Anachronist"))
				{
					_cachedTreasureDefinition = val;
					Log.LogInfo((object)("[COLLECT] Cached Treasure Definition: " + ((Object)val2).name));
					break;
				}
			}
			if (_cachedTreasureDefinition == null)
			{
				array2 = array;
				foreach (Object val3 in array2)
				{
					ScriptableObject val4 = (ScriptableObject)(object)((val3 is ScriptableObject) ? val3 : null);
					if ((Object)(object)val4 != (Object)null && ((Object)val4).name.Contains("Treasure"))
					{
						_cachedTreasureDefinition = val3;
						Log.LogInfo((object)("[COLLECT] Cached Treasure Definition (fallback): " + ((Object)val4).name));
						break;
					}
				}
			}
			if (_cachedTreasureDefinition == null)
			{
				Log.LogWarning((object)"[COLLECT] No Treasure Usable Definition found!");
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[COLLECT] Error finding Treasure Definition: " + ex.Message));
		}
	}

	private static void CreateCustomPrefab()
	{
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Expected O, but got Unknown
		if ((Object)(object)_customPrefab != (Object)null)
		{
			return;
		}
		if (!LoadAssetBundle())
		{
			Log.LogWarning((object)"[COLLECT] Failed to load asset bundle, using fallback");
			CreateFallbackPrefab();
			return;
		}
		GameObject val = FindTreasurePrefabSource();
		if ((Object)(object)val == (Object)null)
		{
			Log.LogWarning((object)"[COLLECT] No Treasure source found, using StaticProp fallback");
			CreateStaticPropFallback();
			return;
		}
		_customPrefab = Object.Instantiate<GameObject>(val);
		((Object)_customPrefab).name = "CustomCollectibleProp";
		_customPrefab.SetActive(false);
		Log.LogInfo((object)("[COLLECT] Cloned Treasure prefab: " + ((Object)val).name));
		LogHierarchy(_customPrefab, 0);
		if (_endlessVisualsType != null)
		{
			Component component = _customPrefab.GetComponent(_endlessVisualsType);
			MonoBehaviour val2 = (MonoBehaviour)(object)((component is MonoBehaviour) ? component : null);
			if ((Object)(object)val2 != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)val2);
				Log.LogInfo((object)"[COLLECT] DESTROYED EndlessVisuals component (prevents default mesh spawn)");
			}
			else
			{
				Log.LogWarning((object)"[COLLECT] EndlessVisuals component not found on prefab");
			}
		}
		else
		{
			Log.LogWarning((object)"[COLLECT] _endlessVisualsType is null - cannot destroy EndlessVisuals");
		}
		Renderer[] componentsInChildren = _customPrefab.GetComponentsInChildren<Renderer>(true);
		Renderer[] array = componentsInChildren;
		foreach (Renderer val3 in array)
		{
			Log.LogInfo((object)("[COLLECT] Destroying existing renderer: " + ((Object)((Component)val3).gameObject).name));
			Object.DestroyImmediate((Object)(object)((Component)val3).gameObject);
		}
		Log.LogInfo((object)$"[COLLECT] Destroyed {componentsInChildren.Length} existing renderer objects");
		Log.LogInfo((object)"[COLLECT] BaseType prefab ready (visuals will come from InjectProp testPrefab)");
		if (_treasureItemType != null && _itemType != null)
		{
			Component component2 = _customPrefab.GetComponent(_treasureItemType);
			if ((Object)(object)component2 != (Object)null)
			{
				if ((Object)(object)_emptyVisualPlaceholder == (Object)null)
				{
					_emptyVisualPlaceholder = new GameObject("EmptyVisualPlaceholder");
					_emptyVisualPlaceholder.SetActive(false);
					Object.DontDestroyOnLoad((Object)(object)_emptyVisualPlaceholder);
					Log.LogInfo((object)"[COLLECT] Created empty visual placeholder GameObject");
				}
				GameObject value = CreateGroundVisualWithRotation();
				try
				{
					FieldInfo fieldInfo = AccessTools.Field(_treasureItemType, "tempVisualsInfoGround");
					if (fieldInfo != null)
					{
						object value2 = fieldInfo.GetValue(component2);
						if (value2 != null)
						{
							FieldInfo field = value2.GetType().GetField("GameObject");
							if (field != null)
							{
								field.SetValue(value2, value);
								fieldInfo.SetValue(component2, value2);
								Log.LogInfo((object)"[COLLECT] Set tempVisualsInfoGround.GameObject to Pearl Basket with SimpleYawSpin");
							}
						}
					}
					FieldInfo fieldInfo2 = AccessTools.Field(_treasureItemType, "tempVisualsInfoEqupped");
					if (fieldInfo2 != null)
					{
						object value3 = fieldInfo2.GetValue(component2);
						if (value3 != null)
						{
							FieldInfo field2 = value3.GetType().GetField("GameObject");
							if (field2 != null)
							{
								field2.SetValue(value3, _emptyVisualPlaceholder);
								fieldInfo2.SetValue(component2, value3);
								Log.LogInfo((object)"[COLLECT] Set tempVisualsInfoEqupped.GameObject to empty placeholder");
							}
						}
					}
					Log.LogInfo((object)"[COLLECT] Configured VisualsInfo for proper rotation behavior");
				}
				catch (Exception ex)
				{
					Log.LogError((object)("[COLLECT] Failed to clear VisualsInfo: " + ex.Message));
				}
				FieldInfo fieldInfo3 = AccessTools.Field(_itemType, "assetID");
				if (fieldInfo3 != null)
				{
					ConstructorInfo constructor = fieldInfo3.FieldType.GetConstructor(new Type[1] { typeof(string) });
					if (constructor != null)
					{
						object value4 = constructor.Invoke(new object[1] { CustomPropGuid.ToString() });
						fieldInfo3.SetValue(component2, value4);
						Log.LogInfo((object)("[COLLECT] Updated assetID to " + CustomPropGuid));
					}
				}
			}
		}
		_customPrefab.SetActive(true);
		Object.DontDestroyOnLoad((Object)(object)_customPrefab);
		RegisterPrefabWithNetworkManager(_customPrefab);
		if ((Object)(object)_loadedPrefab != (Object)null)
		{
			_visualPrefab = _loadedPrefab;
		}
		Log.LogInfo((object)("[COLLECT] Created collectible prefab: " + ((Object)_customPrefab).name));
	}

	private static GameObject FindTreasurePrefabSource()
	{
		Object[] array;
		if (_treasureItemType != null)
		{
			array = Object.FindObjectsOfType(_treasureItemType);
			foreach (Object obj in array)
			{
				MonoBehaviour val = (MonoBehaviour)(object)((obj is MonoBehaviour) ? obj : null);
				if ((Object)(object)val != (Object)null && (Object)(object)((Component)val).gameObject != (Object)null)
				{
					Log.LogInfo((object)("[COLLECT] Found TreasureItem instance: " + ((Object)((Component)val).gameObject).name));
					return ((Component)val).gameObject;
				}
			}
		}
		array = Resources.FindObjectsOfTypeAll(_treasureItemType);
		foreach (Object obj2 in array)
		{
			MonoBehaviour val2 = (MonoBehaviour)(object)((obj2 is MonoBehaviour) ? obj2 : null);
			if ((Object)(object)val2 != (Object)null && (Object)(object)((Component)val2).gameObject != (Object)null)
			{
				Log.LogInfo((object)("[COLLECT] Found TreasureItem via Resources: " + ((Object)((Component)val2).gameObject).name));
				return ((Component)val2).gameObject;
			}
		}
		Log.LogWarning((object)"[COLLECT] No TreasureItem found in scene or resources");
		return null;
	}

	private static void LogHierarchy(GameObject obj, int depth)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		string text = new string(' ', depth * 2);
		Component[] components = obj.GetComponents<Component>();
		string text2 = string.Join(", ", components.Select((Component c) => ((object)c)?.GetType().Name ?? "null"));
		Log.LogInfo((object)$"[HIERARCHY] {text}{((Object)obj).name} (layer={obj.layer}) [{text2}]");
		foreach (Transform item in obj.transform)
		{
			LogHierarchy(((Component)item).gameObject, depth + 1);
		}
	}

	private static void InitializeNetworkTypes()
	{
		if (!_networkTypesInitialized)
		{
			_networkManagerType = AccessTools.TypeByName("Unity.Netcode.NetworkManager");
			_networkObjectType = AccessTools.TypeByName("Unity.Netcode.NetworkObject");
			_networkTypesInitialized = true;
			Log.LogInfo((object)("[NETWORK] NetworkManager type: " + ((_networkManagerType != null) ? "Found" : "NOT FOUND")));
			Log.LogInfo((object)("[NETWORK] NetworkObject type: " + ((_networkObjectType != null) ? "Found" : "NOT FOUND")));
		}
	}

	private static void RegisterPrefabWithNetworkManager(GameObject prefab)
	{
		try
		{
			InitializeNetworkTypes();
			if (_networkManagerType == null)
			{
				Log.LogWarning((object)"[NETWORK] NetworkManager type not found - cannot register prefab");
				return;
			}
			PropertyInfo property = _networkManagerType.GetProperty("Singleton", BindingFlags.Static | BindingFlags.Public);
			if (property == null)
			{
				Log.LogWarning((object)"[NETWORK] NetworkManager.Singleton property not found");
				return;
			}
			object value = property.GetValue(null);
			if (value == null)
			{
				Log.LogWarning((object)"[NETWORK] NetworkManager.Singleton is null - network not initialized yet");
				return;
			}
			MethodInfo method = _networkManagerType.GetMethod("AddNetworkPrefab", new Type[1] { typeof(GameObject) });
			if (method == null)
			{
				Log.LogWarning((object)"[NETWORK] AddNetworkPrefab method not found");
				return;
			}
			if (_networkObjectType != null && (Object)(object)prefab.GetComponent(_networkObjectType) == (Object)null)
			{
				Log.LogWarning((object)"[NETWORK] Prefab has no NetworkObject component - cannot register");
				return;
			}
			method.Invoke(value, new object[1] { prefab });
			Log.LogInfo((object)("[NETWORK] Registered prefab '" + ((Object)prefab).name + "' with NetworkManager"));
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[NETWORK] Failed to register prefab: " + ex.Message));
		}
	}

	public static void SpawnNetworkObject(GameObject instance)
	{
		try
		{
			InitializeNetworkTypes();
			if (_networkObjectType == null)
			{
				Log.LogWarning((object)"[NETWORK] NetworkObject type not found - cannot spawn");
				return;
			}
			Component component = instance.GetComponent(_networkObjectType);
			if ((Object)(object)component == (Object)null)
			{
				Log.LogWarning((object)("[NETWORK] No NetworkObject on '" + ((Object)instance).name + "' - cannot spawn"));
				return;
			}
			MethodInfo method = _networkObjectType.GetMethod("Spawn", new Type[1] { typeof(bool) });
			if (method == null)
			{
				method = _networkObjectType.GetMethod("Spawn", Type.EmptyTypes);
			}
			if (method == null)
			{
				Log.LogWarning((object)"[NETWORK] Spawn method not found on NetworkObject");
				return;
			}
			if (method.GetParameters().Length != 0)
			{
				method.Invoke(component, new object[1] { false });
			}
			else
			{
				method.Invoke(component, null);
			}
			Log.LogInfo((object)("[NETWORK] Spawned NetworkObject on '" + ((Object)instance).name + "'"));
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[NETWORK] Failed to spawn NetworkObject: " + ex.Message));
		}
	}

	private static GameObject CreateGroundVisualWithRotation()
	{
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Expected O, but got Unknown
		//IL_0141: Expected O, but got Unknown
		try
		{
			GameObject val = _loadedPrefab ?? _visualPrefab;
			if ((Object)(object)val == (Object)null)
			{
				Log.LogWarning((object)"[ROTATION] No visual source - creating fallback cube");
				val = GameObject.CreatePrimitive((PrimitiveType)3);
				((Object)val).name = "FallbackVisual";
				Collider component = val.GetComponent<Collider>();
				if ((Object)(object)component != (Object)null)
				{
					Object.DestroyImmediate((Object)(object)component);
				}
			}
			GameObject val2 = Object.Instantiate<GameObject>(val);
			((Object)val2).name = "GroundVisual_WithRotation";

			// DIAGNOSTIC: Log transform hierarchy for rotation debugging
			Log.LogInfo((object)$"[DIAG-ROTATION] Source prefab: '{val.name}'");
			LogTransformHierarchy(val.transform, "[DIAG-ROTATION] Source: ", 0);
			Log.LogInfo((object)$"[DIAG-ROTATION] Instantiated: '{val2.name}'");
			LogTransformHierarchy(val2.transform, "[DIAG-ROTATION] Instance: ", 0);

			if (_simpleYawSpinType != null)
			{
				Component val3 = val2.AddComponent(_simpleYawSpinType);
				if ((Object)(object)val3 != (Object)null)
				{
					FieldInfo fieldInfo = AccessTools.Field(_simpleYawSpinType, "spinDegreesPerSecond");
					if (fieldInfo != null)
					{
						fieldInfo.SetValue(val3, 180f);
					}
					FieldInfo fieldInfo2 = AccessTools.Field(_simpleYawSpinType, "randomizeInitialRotation");
					if (fieldInfo2 != null)
					{
						fieldInfo2.SetValue(val3, true);
					}
					Log.LogInfo((object)"[ROTATION] Added SimpleYawSpin to ground visual");
				}
			}
			else
			{
				Log.LogWarning((object)"[ROTATION] SimpleYawSpin type not found - rotation won't work");
			}
			val2.SetActive(false);
			Object.DontDestroyOnLoad((Object)(object)val2);
			return val2;
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[ROTATION] Failed to create ground visual: " + ex.Message));
			GameObject val4 = new GameObject("FallbackGroundVisual");
			val4.SetActive(false);
			Object.DontDestroyOnLoad((Object)val4);
			return val4;
		}
	}

	private static void CreateCustomInventoryDefinition(object treasureItem)
	{
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Expected O, but got Unknown
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Expected O, but got Unknown
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		if (treasureItem == null)
		{
			return;
		}
		try
		{
			if (_genericUsableDefinitionType == null)
			{
				Log.LogWarning((object)"[ICON] GenericUsableDefinition type not found");
				return;
			}
			if (_customUsableDefinition == null)
			{
				FieldInfo fieldInfo = AccessTools.Field(_itemType, "inventoryUsableDefinition");
				object obj = null;
				if (fieldInfo != null)
				{
					obj = fieldInfo.GetValue(treasureItem);
				}
				if (obj != null)
				{
					_customUsableDefinition = ScriptableObject.CreateInstance(_genericUsableDefinitionType);
					if (_customUsableDefinition == null)
					{
						Log.LogError((object)"[ICON] Failed to create GenericUsableDefinition instance");
						return;
					}
					CopyDefinitionFields(obj, _customUsableDefinition);
					Log.LogInfo((object)"[ICON] Copied all fields from original treasure definition");
				}
				else
				{
					_customUsableDefinition = ScriptableObject.CreateInstance(_genericUsableDefinitionType);
					if (_customUsableDefinition == null)
					{
						Log.LogError((object)"[ICON] Failed to create GenericUsableDefinition instance");
						return;
					}
					Log.LogWarning((object)"[ICON] No original definition found, creating from scratch");
				}
				((Object)(ScriptableObject)_customUsableDefinition).name = "Pearl Basket Usable Definition";
				if (_usableDefinitionType != null)
				{
					FieldInfo fieldInfo2 = AccessTools.Field(_usableDefinitionType, "guid");
					if (fieldInfo2 != null)
					{
						ConstructorInfo constructor = fieldInfo2.FieldType.GetConstructor(new Type[1] { typeof(string) });
						if (constructor != null)
						{
							object value = constructor.Invoke(new object[1] { CustomPropGuid.ToString() });
							fieldInfo2.SetValue(_customUsableDefinition, value);
							Log.LogInfo((object)("[ICON] Set definition GUID to " + CustomPropGuid));
						}
					}
					FieldInfo fieldInfo3 = AccessTools.Field(_usableDefinitionType, "displayName");
					if (fieldInfo3 != null)
					{
						fieldInfo3.SetValue(_customUsableDefinition, "Pearl Basket");
						Log.LogInfo((object)"[ICON] Set definition display name to 'Pearl Basket'");
					}
					FieldInfo fieldInfo4 = AccessTools.Field(_usableDefinitionType, "sprite");
					if (fieldInfo4 != null && (Object)(object)_loadedIcon != (Object)null)
					{
						fieldInfo4.SetValue(_customUsableDefinition, _loadedIcon);
						Log.LogInfo((object)"[ICON] Set definition sprite to loaded icon");
					}
					else if ((Object)(object)_loadedIcon == (Object)null)
					{
						Texture2D val = new Texture2D(64, 64);
						Color[] array = (Color[])(object)new Color[4096];
						for (int i = 0; i < array.Length; i++)
						{
							array[i] = new Color(0.8f, 0.6f, 0.9f);
						}
						val.SetPixels(array);
						val.Apply();
						Sprite value2 = Sprite.Create(val, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f));
						fieldInfo4.SetValue(_customUsableDefinition, value2);
						Log.LogInfo((object)"[ICON] Set definition sprite to placeholder (no icon loaded)");
					}
				}
				Object.DontDestroyOnLoad((Object)_customUsableDefinition);
				Log.LogInfo((object)"[ICON] Created custom GenericUsableDefinition for Pearl Basket");
			}
			FieldInfo fieldInfo5 = AccessTools.Field(_itemType, "inventoryUsableDefinition");
			if (fieldInfo5 != null)
			{
				fieldInfo5.SetValue(treasureItem, _customUsableDefinition);
				Log.LogInfo((object)"[ICON] Assigned custom definition to TreasureItem");
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[ICON] Failed to create custom inventory definition: " + ex.Message));
		}
	}

	private static void CopyDefinitionFields(object source, object target)
	{
		if (source == null || target == null)
		{
			return;
		}
		try
		{
			Type type = source.GetType();
			while (type != null && type != typeof(Object))
			{
				FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo in fields)
				{
					try
					{
						if (!fieldInfo.Name.StartsWith("m_") || !(fieldInfo.DeclaringType == typeof(Object)))
						{
							object value = fieldInfo.GetValue(source);
							fieldInfo.SetValue(target, value);
						}
					}
					catch
					{
					}
				}
				type = type.BaseType;
			}
			Log.LogInfo((object)"[ICON] Copied definition fields via reflection");
		}
		catch (Exception ex)
		{
			Log.LogWarning((object)("[ICON] Field copy partially failed: " + ex.Message));
		}
	}

	private static void StripOldVisuals(GameObject obj)
	{
		MeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<MeshRenderer>(true);
		MeshFilter[] componentsInChildren2 = obj.GetComponentsInChildren<MeshFilter>(true);
		SkinnedMeshRenderer[] componentsInChildren3 = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		int num = 0;
		MeshRenderer[] array = componentsInChildren;
		foreach (MeshRenderer val in array)
		{
			if ((Object)(object)((Component)val).gameObject != (Object)(object)obj)
			{
				Object.DestroyImmediate((Object)(object)((Component)val).gameObject);
				num++;
			}
			else
			{
				Object.DestroyImmediate((Object)(object)val);
			}
		}
		MeshFilter[] array2 = componentsInChildren2;
		foreach (MeshFilter val2 in array2)
		{
			if ((Object)(object)val2 != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)val2);
			}
		}
		SkinnedMeshRenderer[] array3 = componentsInChildren3;
		foreach (SkinnedMeshRenderer val3 in array3)
		{
			if ((Object)(object)((Component)val3).gameObject != (Object)(object)obj)
			{
				Object.DestroyImmediate((Object)(object)((Component)val3).gameObject);
				num++;
			}
			else
			{
				Object.DestroyImmediate((Object)(object)val3);
			}
		}
		Log.LogInfo((object)$"[COLLECT] Stripped {num} visual objects from clone");
	}

	private static void CreateStaticPropFallback()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		if (LoadAssetBundle())
		{
			_customPrefab = new GameObject("CustomProp");
			_customPrefab.SetActive(false);
			Type type = AccessTools.TypeByName("Endless.Gameplay.StaticProp");
			if (type != null)
			{
				_customPrefab.AddComponent(type);
				Log.LogInfo((object)"[INJECTOR] Added StaticProp component (fallback)");
			}
			_customPrefab.SetActive(true);
			Object.DontDestroyOnLoad((Object)(object)_customPrefab);
			if ((Object)(object)_loadedPrefab != (Object)null)
			{
				_visualPrefab = _loadedPrefab;
			}
			Log.LogInfo((object)("[INJECTOR] Created fallback prop prefab: " + ((Object)_customPrefab).name));
		}
	}

	private static bool LoadAssetBundle()
	{
		if ((Object)(object)_customPropsBundle != (Object)null && _loadedPropDefinitions.Count > 0)
		{
			return true;
		}
		try
		{
			string text = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "custom_props.bundle");
			if (!File.Exists(text))
			{
				Log.LogWarning((object)("[BUNDLE] custom_props.bundle not found at: " + text));
				return false;
			}
			_customPropsBundle = AssetBundle.LoadFromFile(text);
			if ((Object)(object)_customPropsBundle == (Object)null)
			{
				Log.LogError((object)"[BUNDLE] Failed to load asset bundle");
				return false;
			}
			string[] allAssetNames = _customPropsBundle.GetAllAssetNames();
			Log.LogInfo((object)$"[BUNDLE] v10.52.0: Found {allAssetNames.Length} assets in bundle:");
			string[] array = allAssetNames;
			foreach (string text2 in array)
			{
				Log.LogInfo((object)("[BUNDLE]   - " + text2));
			}
			_loadedPropDefinitions.Clear();
			Dictionary<string, GameObject> dictionary = new Dictionary<string, GameObject>();
			array = allAssetNames;
			foreach (string text3 in array)
			{
				if (text3.EndsWith(".prefab"))
				{
					GameObject val = _customPropsBundle.LoadAsset<GameObject>(text3);
					if ((Object)(object)val != (Object)null)
					{
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text3);
						dictionary[fileNameWithoutExtension.ToLowerInvariant()] = val;
						Log.LogInfo((object)("[BUNDLE] Loaded prefab: '" + fileNameWithoutExtension + "' from " + text3));

						// v10.54.0: Fix catapulting - Set colliders to PlayerInteractable layer
						// This uses the game's native layer system where PlayerInteractable layer
						// "does NOT cause physical collisions" but allows interaction
						int interactableLayer = LayerMask.NameToLayer("PlayerInteractable");
						if (interactableLayer != -1)
						{
							Collider[] prefabColliders = val.GetComponentsInChildren<Collider>(true);
							foreach (Collider col in prefabColliders)
							{
								col.gameObject.layer = interactableLayer;
								Log.LogInfo((object)$"[BUNDLE] Set collider '{col.name}' to PlayerInteractable layer ({interactableLayer})");
							}
							Log.LogInfo((object)$"[BUNDLE] Fixed {prefabColliders.Length} collider(s) on '{fileNameWithoutExtension}'");
						}
						else
						{
							Log.LogWarning((object)"[BUNDLE] PlayerInteractable layer not found - colliders may cause catapulting");
						}
					}
				}
			}
			Log.LogInfo((object)$"[BUNDLE] Loaded {dictionary.Count} prefabs");
			Dictionary<string, Sprite> dictionary2 = new Dictionary<string, Sprite>();
			Sprite[] array2 = _customPropsBundle.LoadAllAssets<Sprite>();
			Log.LogInfo((object)$"[DIAG-ICON] LoadAllAssets<Sprite> returned {array2.Length} sprites");
			foreach (Sprite val2 in array2)
			{
				string text4 = ((Object)val2).name.ToLowerInvariant();
				if (text4.EndsWith("-icon"))
				{
					text4 = text4.Substring(0, text4.Length - 5);
				}
				dictionary2[text4] = val2;
				// DIAGNOSTIC: Log detailed sprite info
				Texture2D tex = val2.texture;
				string texInfo = (tex != null)
					? $"tex={tex.width}x{tex.height}, format={tex.format}, readable={tex.isReadable}"
					: "tex=NULL";
				Log.LogInfo((object)$"[DIAG-ICON] Sprite '{val2.name}' -> baseName '{text4}' | {texInfo} | rect={val2.rect}");
			}
			// DIAGNOSTIC: Also try loading Texture2D directly to see what's in bundle
			Texture2D[] textures = _customPropsBundle.LoadAllAssets<Texture2D>();
			Log.LogInfo((object)$"[DIAG-ICON] LoadAllAssets<Texture2D> returned {textures.Length} textures");
			foreach (Texture2D t in textures)
			{
				Log.LogInfo((object)$"[DIAG-ICON] Texture2D '{t.name}' | {t.width}x{t.height} | format={t.format} | readable={t.isReadable}");
			}
			Log.LogInfo((object)$"[BUNDLE] Loaded {dictionary2.Count} sprites");
			int num = 0;
			foreach (KeyValuePair<string, GameObject> item in dictionary)
			{
				string key = item.Key;
				GameObject value = item.Value;

				// v10.67.0: Filter out particle effect prefabs
				// Particle effects have ParticleSystem but no MeshRenderer/SkinnedMeshRenderer at root
				// They should be embedded components within props, not separate props (like ResourcePickup uses SwappableParticleSystem)
				ParticleSystem[] particleSystems = value.GetComponentsInChildren<ParticleSystem>(true);
				MeshRenderer meshRenderer = value.GetComponent<MeshRenderer>();
				SkinnedMeshRenderer skinnedRenderer = value.GetComponent<SkinnedMeshRenderer>();
				MeshFilter meshFilter = value.GetComponent<MeshFilter>();

				bool hasParticles = particleSystems != null && particleSystems.Length > 0;
				bool hasMainVisual = meshRenderer != null || skinnedRenderer != null || meshFilter != null;

				// Skip if it's ONLY a particle effect (has particles but no main mesh visual at root)
				if (hasParticles && !hasMainVisual)
				{
					Log.LogInfo((object)$"[BUNDLE] v10.67.0: Skipping particle effect prefab '{key}' (has {particleSystems.Length} ParticleSystem(s), no mesh at root)");
					continue;
				}

				Sprite value2 = null;
				if (dictionary2.TryGetValue(key, out value2))
				{
					Log.LogInfo((object)("[BUNDLE] Matched icon for '" + key + "': " + ((Object)value2).name));
				}
				else
				{
					Log.LogWarning((object)("[BUNDLE] No matching icon found for '" + key + "'"));
				}
				string text5 = GenerateDeterministicGuid(key);
				string name = ((Object)value).name;
				LoadedPropDefinition loadedPropDefinition = new LoadedPropDefinition
				{
					PropId = text5,
					DisplayName = name,
					VisualPrefab = value,
					Icon = value2,
					BaseType = "TreasureItem"
				};
				_loadedPropDefinitions[loadedPropDefinition.PropId] = loadedPropDefinition;
				Log.LogInfo((object)("[BUNDLE] Created definition: '" + name + "' (ID: " + text5 + ", Icon: " + (((Object)(object)value2 != (Object)null) ? ((Object)value2).name : "NONE") + ")"));
				num++;
			}
			Log.LogInfo((object)$"[BUNDLE] v10.52.0: Loaded {_loadedPropDefinitions.Count} prop definition(s)");
			if (_loadedPropDefinitions.Count > 0)
			{
				LoadedPropDefinition loadedPropDefinition2 = _loadedPropDefinitions.Values.First();
				_loadedPrefab = loadedPropDefinition2.VisualPrefab;
				_loadedIcon = loadedPropDefinition2.Icon;
			}
			return _loadedPropDefinitions.Count > 0;
		}
		catch (Exception arg)
		{
			Log.LogError((object)$"[INJECTOR] Error loading AssetBundle: {arg}");
			return false;
		}
	}

	private static string GenerateDeterministicGuid(string input)
	{
		using MD5 mD = MD5.Create();
		return new Guid(mD.ComputeHash(Encoding.UTF8.GetBytes("EndstarCustomProp:" + input))).ToString();
	}

	private static void CreateFallbackPrefab()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		_customPrefab = new GameObject("CustomInjectedProp_BaseType");
		_customPrefab.SetActive(false);
		Type type = AccessTools.TypeByName("Endless.Gameplay.StaticProp");
		if (type != null)
		{
			_customPrefab.AddComponent(type);
		}
		_customPrefab.SetActive(true);
		Object.DontDestroyOnLoad((Object)(object)_customPrefab);
		_visualPrefab = new GameObject("CustomInjectedProp_Visual");
		_visualPrefab.transform.position = new Vector3(0f, -10000f, 0f);
		MeshFilter obj = _visualPrefab.AddComponent<MeshFilter>();
		MeshRenderer val = _visualPrefab.AddComponent<MeshRenderer>();
		obj.mesh = CreateCubeMesh();
		GameObject val2 = GameObject.CreatePrimitive((PrimitiveType)3);
		((Renderer)val).material = new Material(((Renderer)val2.GetComponent<MeshRenderer>()).sharedMaterial);
		((Renderer)val).material.color = Color.magenta;
		Object.DestroyImmediate((Object)(object)val2);
		Object.DontDestroyOnLoad((Object)(object)_visualPrefab);
	}

	private static Mesh CreateCubeMesh()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		Mesh val = new Mesh();
		Vector3[] vertices = (Vector3[])(object)new Vector3[8]
		{
			new Vector3(-0.5f, -0.5f, -0.5f),
			new Vector3(0.5f, -0.5f, -0.5f),
			new Vector3(0.5f, 0.5f, -0.5f),
			new Vector3(-0.5f, 0.5f, -0.5f),
			new Vector3(-0.5f, -0.5f, 0.5f),
			new Vector3(0.5f, -0.5f, 0.5f),
			new Vector3(0.5f, 0.5f, 0.5f),
			new Vector3(-0.5f, 0.5f, 0.5f)
		};
		int[] triangles = new int[36]
		{
			0, 2, 1, 0, 3, 2, 1, 6, 5, 1,
			2, 6, 5, 7, 4, 5, 6, 7, 4, 3,
			0, 4, 7, 3, 3, 6, 2, 3, 7, 6,
			4, 1, 5, 4, 0, 1
		};
		val.vertices = vertices;
		val.triangles = triangles;
		val.RecalculateNormals();
		return val;
	}

	private static void CreateCustomBaseTypeDefinition()
	{
		if (_customBaseTypeDefinition != null)
		{
			return;
		}
		_customBaseTypeDefinition = ScriptableObject.CreateInstance(_baseTypeDefinitionType);
		AccessTools.Field(_componentDefinitionType, "prefab")?.SetValue(_customBaseTypeDefinition, _customPrefab);
		FieldInfo fieldInfo = AccessTools.Field(_componentDefinitionType, "componentId");
		if (fieldInfo != null)
		{
			object obj = CreateSerializableGuid(CustomBaseTypeGuid);
			if (obj != null)
			{
				fieldInfo.SetValue(_customBaseTypeDefinition, obj);
			}
		}
		AccessTools.Field(_baseTypeDefinitionType, "isUserExposed")?.SetValue(_customBaseTypeDefinition, true);
		AccessTools.Field(_componentDefinitionType, "isNetworked")?.SetValue(_customBaseTypeDefinition, true);
		CustomBaseTypes[CustomBaseTypeGuid] = _customBaseTypeDefinition;
		object customBaseTypeDefinition = _customBaseTypeDefinition;
		Object.DontDestroyOnLoad((Object)((customBaseTypeDefinition is Object) ? customBaseTypeDefinition : null));
	}

	private static object CreateSerializableGuid(string guidString)
	{
		try
		{
			ConstructorInfo constructor = _serializableGuidType.GetConstructor(new Type[1] { typeof(string) });
			if (constructor != null)
			{
				return constructor.Invoke(new object[1] { guidString });
			}
			MethodInfo method = _serializableGuidType.GetMethod("op_Implicit", new Type[1] { typeof(string) });
			if (method != null)
			{
				return method.Invoke(null, new object[1] { guidString });
			}
			ConstructorInfo constructor2 = _serializableGuidType.GetConstructor(new Type[1] { typeof(Guid) });
			if (constructor2 != null)
			{
				return constructor2.Invoke(new object[1] { Guid.Parse(guidString) });
			}
		}
		catch (Exception arg)
		{
			Log.LogError((object)$"[INJECTOR] CreateSerializableGuid failed: {arg}");
		}
		return null;
	}

	private static void InjectCustomProp(object stageManager)
	{
		if (_loadedPropDefinitions.Count == 0)
		{
			Log.LogWarning((object)"[INJECT] No prop definitions loaded, nothing to inject");
			return;
		}
		Log.LogInfo((object)$"[INJECT] v10.51.0: Injecting {_loadedPropDefinitions.Count} custom prop(s)...");
		int num = 0;
		foreach (KeyValuePair<string, LoadedPropDefinition> loadedPropDefinition in _loadedPropDefinitions)
		{
			try
			{
				InjectSingleProp(stageManager, loadedPropDefinition.Value);
				num++;
			}
			catch (Exception ex)
			{
				Log.LogError((object)("[INJECT] Failed to inject prop '" + loadedPropDefinition.Value.DisplayName + "': " + ex.Message));
			}
		}
		Log.LogInfo((object)$"[INJECT] v10.51.0: Successfully injected {num}/{_loadedPropDefinitions.Count} props");
	}

	private static void InjectSingleProp(object stageManager, LoadedPropDefinition definition)
	{
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Expected O, but got Unknown
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		object obj = Activator.CreateInstance(_propType);
		Type type = AccessTools.TypeByName("Endless.Assets.AssetCore");
		(type?.GetField("Name"))?.SetValue(obj, definition.DisplayName);
		(type?.GetField("AssetID"))?.SetValue(obj, definition.PropId);
		(type?.GetField("AssetType"))?.SetValue(obj, "Prop");
		AccessTools.Field(_propType, "baseTypeId")?.SetValue(obj, CustomBaseTypeGuid);
		AccessTools.Field(_propType, "componentIds")?.SetValue(obj, new List<string>());
		FieldInfo fieldInfo = AccessTools.Field(_propType, "propLocationOffsets");
		if (fieldInfo != null)
		{
			Type type2 = AccessTools.TypeByName("Endless.Props.Assets.PropLocationOffset");
			if (type2 != null)
			{
				object obj2 = Activator.CreateInstance(type2);
				type2.GetField("Offset")?.SetValue(obj2, Vector3Int.zero);
				Array array = Array.CreateInstance(type2, 1);
				array.SetValue(obj2, 0);
				fieldInfo.SetValue(obj, array);
			}
		}
		CustomProps[definition.PropId] = obj;
		MethodInfo methodInfo = AccessTools.Method(_stageManagerType, "InjectProp", (Type[])null, (Type[])null);
		if (!(methodInfo != null))
		{
			return;
		}
		Sprite val = definition.Icon;
		if ((Object)(object)val == (Object)null)
		{
			Texture2D val2 = new Texture2D(64, 64);
			Color[] array2 = (Color[])(object)new Color[4096];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = Color.magenta;
			}
			val2.SetPixels(array2);
			val2.Apply();
			val = Sprite.Create(val2, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f));
		}
		methodInfo.Invoke(stageManager, new object[4] { obj, null, null, val });
		Log.LogInfo((object)("[INJECT] Injected prop: " + definition.DisplayName + " (ID: " + definition.PropId + ")"));
	}

	private void PatchItemComponentInitialize()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		if (_itemType != null)
		{
			MethodInfo methodInfo = AccessTools.Method(_itemType, "ComponentInitialize", (Type[])null, (Type[])null);
			if (methodInfo != null)
			{
				HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("Item_ComponentInitialize_Prefix", BindingFlags.Static | BindingFlags.Public));
				_harmony.Patch((MethodBase)methodInfo, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				Log.LogInfo((object)"[INJECTOR] Patched Item.ComponentInitialize (v10.33.0 prefix only)");
			}
			else
			{
				Log.LogWarning((object)"[INJECTOR] Item.ComponentInitialize method not found");
			}
			// v10.60.0: Removed HandleNetStateChanged patch - private struct params cause Harmony issues
			// Particle stopping now handled in Item_Pickup_Prefix which is proven to work
		}
		PatchTrackNonNetworkedObject();
	}

	private void PatchTrackNonNetworkedObject()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		_stageType = AccessTools.TypeByName("Endless.Gameplay.LevelEditing.Level.Stage");
		if (_stageType == null)
		{
			Log.LogWarning((object)"[INJECTOR] Stage type not found for TrackNonNetworkedObject patch");
			return;
		}
		MethodInfo methodInfo = AccessTools.Method(_stageType, "TrackNonNetworkedObject", (Type[])null, (Type[])null);
		if (methodInfo != null)
		{
			HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("TrackNonNetworkedObject_Postfix", BindingFlags.Static | BindingFlags.Public));
			_harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			Log.LogInfo((object)"[INJECTOR] Patched Stage.TrackNonNetworkedObject (v10.45.0 - diagnostic only)");
		}
		else
		{
			Log.LogWarning((object)"[INJECTOR] Stage.TrackNonNetworkedObject method not found");
		}
	}

	public static void TrackNonNetworkedObject_Postfix(object assetId, object instanceId, GameObject newObject)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		try
		{
			if ((Object)(object)newObject == (Object)null || !((Object)(object)newObject.GetComponent<CustomPropMarker>() != (Object)null))
			{
				return;
			}
			Log.LogInfo((object)"[TRACK-POSTFIX] v10.45.0: Custom prop PLACED via asset bundle system");
			Component componentInChildren = newObject.GetComponentInChildren(_itemType);
			MonoBehaviour val = (MonoBehaviour)(object)((componentInChildren is MonoBehaviour) ? componentInChildren : null);
			if (!((Object)(object)val != (Object)null))
			{
				return;
			}
			object obj = _itemType.GetField("runtimeGroundVisuals", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(val);
			GameObject val2 = (GameObject)((obj is GameObject) ? obj : null);
			Log.LogInfo((object)("[TRACK-POSTFIX] runtimeGroundVisuals: " + (((val2 != null) ? ((Object)val2).name : null) ?? "null")));
			int num = 0;
			foreach (Transform item in ((Component)val).transform)
			{
				Transform val3 = item;
				num++;
				Log.LogInfo((object)("[TRACK-POSTFIX]   Child: " + ((Object)val3).name));
			}
			Log.LogInfo((object)$"[TRACK-POSTFIX] Total children under Item: {num}");
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[TRACK-POSTFIX] Error: " + ex.Message));
		}
	}

	private static void LogHierarchy(Transform t, int depth)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		string text = new string(' ', depth * 2);
		Component[] components = ((Component)t).GetComponents<Component>();
		string text2 = string.Join(", ", components.Select((Component c) => ((object)c).GetType().Name));
		Log.LogInfo((object)("[HIERARCHY] " + text + ((Object)t).name + " [" + text2 + "]"));
		foreach (Transform item in t)
		{
			LogHierarchy(item, depth + 1);
		}
	}

	private static string GetTransformPath(Transform t)
	{
		string text = ((Object)t).name;
		while ((Object)(object)t.parent != (Object)null)
		{
			t = t.parent;
			text = ((Object)t).name + "/" + text;
		}
		return text;
	}

	private void PatchGetAssetBundleAsync()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		if (_loadedFileManagerType == null)
		{
			Log.LogWarning((object)"[INJECTOR] LoadedFileManager type not found for GetAssetBundleAsync patch");
			return;
		}
		MethodInfo methodInfo = AccessTools.Method(_loadedFileManagerType, "GetAssetBundleAsync", (Type[])null, (Type[])null);
		if (methodInfo != null)
		{
			HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("GetAssetBundleAsync_Prefix", BindingFlags.Static | BindingFlags.Public));
			_harmony.Patch((MethodBase)methodInfo, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			Log.LogInfo((object)"[INJECTOR] Patched LoadedFileManager.GetAssetBundleAsync (v10.45.0 PREFIX)");
		}
		else
		{
			Log.LogWarning((object)"[INJECTOR] LoadedFileManager.GetAssetBundleAsync method not found");
		}
	}

	public static bool GetAssetBundleAsync_Prefix(object __instance, int fileInstanceId)
	{
		if (fileInstanceId != -9999)
		{
			return true;
		}
		if ((Object)(object)_customPropsBundle == (Object)null)
		{
			Log.LogWarning((object)"[BUNDLE-HOOK] Custom bundle not loaded!");
			return true;
		}
		if (_loadedFilesField == null)
		{
			Log.LogWarning((object)"[BUNDLE-HOOK] loadedFiles field not cached!");
			return true;
		}
		try
		{
			IDictionary dictionary = (IDictionary)_loadedFilesField.GetValue(__instance);
			if (dictionary == null)
			{
				Log.LogWarning((object)"[BUNDLE-HOOK] loadedFiles dictionary is null!");
				return true;
			}
			if (!dictionary.Contains(fileInstanceId))
			{
				dictionary.Add(fileInstanceId, _customPropsBundle);
				Log.LogInfo((object)$"[BUNDLE-HOOK] Injected bundle at fileInstanceId={fileInstanceId}");
			}
			else
			{
				Log.LogInfo((object)$"[BUNDLE-HOOK] Bundle already in cache at fileInstanceId={fileInstanceId}");
			}
			return true;
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[BUNDLE-HOOK] Error injecting bundle: " + ex.Message));
			return true;
		}
	}

	private static void SetupAssetBundleReferences()
	{
		if ((Object)(object)_customPropsBundle == (Object)null)
		{
			Log.LogError((object)"[ASSET-BUNDLE] Bundle not loaded, cannot setup references");
			return;
		}
		if (_endlessPrefabAssetType == null || _fileAssetInstanceType == null || _assetReferenceType == null)
		{
			Log.LogError((object)"[ASSET-BUNDLE] Required types not cached");
			return;
		}
		try
		{
			string[] allAssetNames = _customPropsBundle.GetAllAssetNames();
			string text = null;
			string[] array = allAssetNames;
			foreach (string text2 in array)
			{
				if (text2.EndsWith(".prefab"))
				{
					text = Path.GetFileName(text2);
					break;
				}
			}
			if (text == null)
			{
				Log.LogError((object)"[ASSET-BUNDLE] No prefab found in bundle!");
				return;
			}
			Log.LogInfo((object)("[ASSET-BUNDLE] Prefab file name: " + text));
			object obj = Activator.CreateInstance(_fileAssetInstanceType);
			AccessTools.Field(_fileAssetInstanceType, "Label").SetValue(obj, "custom_props_bundle");
			AccessTools.Field(_fileAssetInstanceType, "AssetFileInstanceId").SetValue(obj, -9999);
			Log.LogInfo((object)$"[ASSET-BUNDLE] Created FileAssetInstance with ID={-9999}");
			object obj2 = Activator.CreateInstance(_endlessPrefabAssetType);
			Type type = _endlessPrefabAssetType.BaseType?.BaseType;
			if (type == null)
			{
				type = AccessTools.TypeByName("Endless.Assets.AssetCore");
			}
			if (type != null)
			{
				AccessTools.Field(type, "Name")?.SetValue(obj2, "CustomPropPrefab");
				AccessTools.Field(type, "AssetID")?.SetValue(obj2, "33333333-3333-3333-3333-333333333333");
				AccessTools.Field(type, "AssetVersion")?.SetValue(obj2, "1.0.0");
				AccessTools.Field(type, "AssetType")?.SetValue(obj2, "endless-prefab");
			}
			else
			{
				Log.LogWarning((object)"[ASSET-BUNDLE] Could not find AssetCore type, trying direct field access");
				FieldInfo fieldInfo = AccessTools.Field(_endlessPrefabAssetType, "Name");
				FieldInfo fieldInfo2 = AccessTools.Field(_endlessPrefabAssetType, "AssetID");
				FieldInfo fieldInfo3 = AccessTools.Field(_endlessPrefabAssetType, "AssetVersion");
				FieldInfo fieldInfo4 = AccessTools.Field(_endlessPrefabAssetType, "AssetType");
				fieldInfo?.SetValue(obj2, "CustomPropPrefab");
				fieldInfo2?.SetValue(obj2, "33333333-3333-3333-3333-333333333333");
				fieldInfo3?.SetValue(obj2, "1.0.0");
				fieldInfo4?.SetValue(obj2, "endless-prefab");
			}
			AccessTools.Field(_endlessPrefabAssetType, "PrefabFileName")?.SetValue(obj2, text);
			AccessTools.Field(_endlessPrefabAssetType, "WindowsStandaloneBundleFile")?.SetValue(obj2, obj);
			Log.LogInfo((object)"[ASSET-BUNDLE] Created EndlessPrefabAsset");
			Type type2 = AccessTools.TypeByName("Endless.Gameplay.EndlessAssetCache");
			if (type2 != null)
			{
				MethodInfo methodInfo = AccessTools.Method(type2, "AddNewVersionToCache", (Type[])null, (Type[])null);
				if (methodInfo != null)
				{
					methodInfo.MakeGenericMethod(_endlessPrefabAssetType).Invoke(null, new object[1] { obj2 });
					Log.LogInfo((object)"[ASSET-BUNDLE] Added EndlessPrefabAsset to EndlessAssetCache");
				}
				else
				{
					Log.LogWarning((object)"[ASSET-BUNDLE] AddNewVersionToCache method not found");
				}
			}
			else
			{
				Log.LogWarning((object)"[ASSET-BUNDLE] EndlessAssetCache type not found");
			}
			_customPrefabAssetReference = Activator.CreateInstance(_assetReferenceType);
			AccessTools.Field(_assetReferenceType, "AssetID")?.SetValue(_customPrefabAssetReference, "33333333-3333-3333-3333-333333333333");
			AccessTools.Field(_assetReferenceType, "AssetVersion")?.SetValue(_customPrefabAssetReference, "1.0.0");
			AccessTools.Field(_assetReferenceType, "AssetType")?.SetValue(_customPrefabAssetReference, "endless-prefab");
			Log.LogInfo((object)"[ASSET-BUNDLE] Created AssetReference for prop.prefabBundle");
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[ASSET-BUNDLE] Error setting up references: " + ex.Message + "\n" + ex.StackTrace));
		}
	}

	public static void Item_ComponentInitialize_Prefix(object __instance, object referenceBase, object endlessProp)
	{
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (endlessProp == null || __instance == null)
			{
				return;
			}
			PropertyInfo propertyInfo = AccessTools.Property(_endlessPropType, "Prop");
			if (propertyInfo == null)
			{
				Log.LogError((object)"[VISUAL-INJECT] Could not find Prop property on EndlessProp");
				return;
			}
			object value = propertyInfo.GetValue(endlessProp);
			if (value == null)
			{
				Log.LogWarning((object)"[VISUAL-INJECT] Prop is null on EndlessProp");
				return;
			}
			PropertyInfo propertyInfo2 = AccessTools.Property(_propType, "AssetID");
			object obj = null;
			if (propertyInfo2 != null)
			{
				obj = propertyInfo2.GetValue(value);
			}
			else
			{
				FieldInfo fieldInfo = AccessTools.Field(_propType, "AssetID");
				if (fieldInfo != null)
				{
					obj = fieldInfo.GetValue(value);
				}
			}
			if (obj == null)
			{
				Log.LogWarning((object)"[VISUAL-INJECT] Could not get AssetID from Prop");
				return;
			}
			string text = obj.ToString();
			if (!_loadedPropDefinitions.TryGetValue(text, out var value2))
			{
				return;
			}
			Log.LogInfo((object)("[VISUAL-INJECT] v10.51.0: ComponentInitialize for custom prop '" + value2.DisplayName + "' (ID: " + text + ")"));
			MonoBehaviour val = (MonoBehaviour)((endlessProp is MonoBehaviour) ? endlessProp : null);
			if ((Object)(object)val != (Object)null && (Object)(object)((Component)val).GetComponent<CustomPropMarker>() == (Object)null)
			{
				((Component)val).gameObject.AddComponent<CustomPropMarker>();
				Log.LogInfo((object)"[VISUAL-INJECT] Added CustomPropMarker to EndlessProp");
			}
			GameObject visualPrefab = value2.VisualPrefab;
			if ((Object)(object)visualPrefab == (Object)null)
			{
				Log.LogError((object)("[VISUAL-INJECT] v10.51.0: Visual prefab is null for prop '" + value2.DisplayName + "'!"));
				return;
			}
			Type nestedType = _itemType.GetNestedType("VisualsInfo", BindingFlags.Public | BindingFlags.NonPublic);
			if (nestedType == null)
			{
				Log.LogError((object)"[VISUAL-INJECT] Could not find VisualsInfo type");
				return;
			}
			FieldInfo fieldInfo2 = AccessTools.Field(_treasureItemType, "tempVisualsInfoGround");
			if (fieldInfo2 != null)
			{
				object value3 = fieldInfo2.GetValue(__instance);
				if (value3 != null)
				{
					FieldInfo field = nestedType.GetField("GameObject");
					FieldInfo posField = nestedType.GetField("Position");
					FieldInfo anglesField = nestedType.GetField("Angles");
					if (field != null)
					{
						field.SetValue(value3, visualPrefab);
						// v10.55.0: Reset Position and Angles to zero - the prefab's hierarchy
						// should already have correct orientation. Don't apply template's angles.
						if (posField != null)
						{
							posField.SetValue(value3, Vector3.zero);
							Log.LogInfo((object)"[VISUAL-INJECT] Reset GroundVisualsInfo.Position to zero");
						}
						if (anglesField != null)
						{
							anglesField.SetValue(value3, Vector3.zero);
							Log.LogInfo((object)"[VISUAL-INJECT] Reset GroundVisualsInfo.Angles to zero");
						}
						fieldInfo2.SetValue(__instance, value3);
						Log.LogInfo((object)("[VISUAL-INJECT] v10.55.0: Injected '" + ((Object)visualPrefab).name + "' into GroundVisualsInfo (with zero offset/angles)"));
					}
				}
			}
			FieldInfo fieldInfo3 = AccessTools.Field(_treasureItemType, "tempVisualsInfoEqupped");
			if (fieldInfo3 != null)
			{
				object value4 = fieldInfo3.GetValue(__instance);
				if (value4 != null)
				{
					FieldInfo field2 = nestedType.GetField("GameObject");
					FieldInfo posField2 = nestedType.GetField("Position");
					FieldInfo anglesField2 = nestedType.GetField("Angles");
					if (field2 != null)
					{
						field2.SetValue(value4, visualPrefab);
						// v10.55.0: Reset Position and Angles to zero for equipped visuals too
						if (posField2 != null)
						{
							posField2.SetValue(value4, Vector3.zero);
							Log.LogInfo((object)"[VISUAL-INJECT] Reset EquippedVisualsInfo.Position to zero");
						}
						if (anglesField2 != null)
						{
							anglesField2.SetValue(value4, Vector3.zero);
							Log.LogInfo((object)"[VISUAL-INJECT] Reset EquippedVisualsInfo.Angles to zero");
						}
						fieldInfo3.SetValue(__instance, value4);
						Log.LogInfo((object)("[VISUAL-INJECT] v10.55.0: Injected '" + ((Object)visualPrefab).name + "' into EquippedVisualsInfo (with zero offset/angles)"));
					}
				}
			}
			// v10.56.0: Reset equippedItemParamName to empty string to disable holding animation
			// Native TreasureItem uses empty string by default, but template prefab may have it set
			FieldInfo equippedItemParamField = AccessTools.Field(_itemType, "equippedItemParamName");
			if (equippedItemParamField != null)
			{
				equippedItemParamField.SetValue(__instance, string.Empty);
				Log.LogInfo((object)"[VISUAL-INJECT] v10.56.0: Reset equippedItemParamName to empty (no holding animation)");
			}
			FieldInfo fieldInfo4 = AccessTools.Field(_itemType, "inventoryUsableDefinition");
			if (fieldInfo4 != null)
			{
				object value5 = fieldInfo4.GetValue(__instance);
				if (value5 != null && _genericUsableDefinitionType != null)
				{
					Log.LogInfo((object)("[VISUAL-INJECT] Found original definition: " + ((Object)(ScriptableObject)value5).name));
					ScriptableObject val2 = ScriptableObject.CreateInstance(_genericUsableDefinitionType);
					if ((Object)(object)val2 != (Object)null)
					{
						CopyDefinitionFields(value5, val2);
						if (_usableDefinitionType != null)
						{
							FieldInfo fieldInfo5 = AccessTools.Field(_usableDefinitionType, "sprite");
							if (fieldInfo5 != null && (Object)(object)value2.Icon != (Object)null)
							{
								fieldInfo5.SetValue(val2, value2.Icon);
								Log.LogInfo((object)("[VISUAL-INJECT] Set custom icon for '" + value2.DisplayName + "'"));
							}
							else if ((Object)(object)value2.Icon == (Object)null)
							{
								Log.LogWarning((object)("[VISUAL-INJECT] Icon is null for prop '" + value2.DisplayName + "'"));
							}
							FieldInfo fieldInfo6 = AccessTools.Field(_usableDefinitionType, "displayName");
							if (fieldInfo6 != null)
							{
								fieldInfo6.SetValue(val2, value2.DisplayName);
								Log.LogInfo((object)("[VISUAL-INJECT] Set display name to '" + value2.DisplayName + "'"));
							}
							FieldInfo fieldInfo7 = AccessTools.Field(_usableDefinitionType, "guid");
							if (fieldInfo7 != null)
							{
								ConstructorInfo constructor = fieldInfo7.FieldType.GetConstructor(new Type[1] { typeof(string) });
								if (constructor != null)
								{
									object value6 = constructor.Invoke(new object[1] { value2.PropId });
									fieldInfo7.SetValue(val2, value6);
									Log.LogInfo((object)("[VISUAL-INJECT] Set definition GUID to " + value2.PropId));
								}
							}
						}
						fieldInfo4.SetValue(__instance, val2);
						((Object)val2).name = value2.DisplayName + " Usable Definition";
						Object.DontDestroyOnLoad((Object)(object)val2);
						Log.LogInfo((object)("[VISUAL-INJECT] Assigned cloned definition to Item for '" + value2.DisplayName + "'"));
						RegisterDefinitionWithRuntimeDatabase(val2);
					}
					else
					{
						Log.LogError((object)"[VISUAL-INJECT] Failed to create cloned definition");
					}
				}
				else
				{
					Log.LogWarning((object)$"[VISUAL-INJECT] originalDefinition={value5 != null}, _genericUsableDefinitionType={_genericUsableDefinitionType != null}");
				}
			}
			Log.LogInfo((object)("[VISUAL-INJECT] v10.51.0: Successfully injected visuals for '" + value2.DisplayName + "'"));
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[VISUAL-INJECT] Error in ComponentInitialize prefix: " + ex.Message + "\n" + ex.StackTrace));
		}
	}

	public static void Item_ComponentInitialize_Postfix(object __instance, object referenceBase, object endlessProp)
	{
		try
		{
			if (endlessProp == null || __instance == null)
			{
				return;
			}
			PropertyInfo propertyInfo = AccessTools.Property(_endlessPropType, "Prop");
			if (propertyInfo == null)
			{
				return;
			}
			object value = propertyInfo.GetValue(endlessProp);
			if (value == null)
			{
				return;
			}
			PropertyInfo propertyInfo2 = AccessTools.Property(_propType, "AssetID");
			object obj = null;
			if (propertyInfo2 != null)
			{
				obj = propertyInfo2.GetValue(value);
			}
			else
			{
				FieldInfo fieldInfo = AccessTools.Field(_propType, "AssetID");
				if (fieldInfo != null)
				{
					obj = fieldInfo.GetValue(value);
				}
			}
			if (obj == null || obj.ToString() != CustomPropGuid.ToString())
			{
				return;
			}
			Log.LogInfo((object)"[VISUAL-POSTFIX] v10.37.0: Processing postfix for our custom prop");
			FieldInfo fieldInfo2 = AccessTools.Field(_itemType, "runtimeGroundVisuals");
			FieldInfo fieldInfo3 = AccessTools.Field(_itemType, "runtimeEquippedVisuals");
			GameObject val = null;
			GameObject val2 = null;
			if (fieldInfo2 != null)
			{
				object value2 = fieldInfo2.GetValue(__instance);
				val = (GameObject)((value2 is GameObject) ? value2 : null);
				Log.LogInfo((object)("[VISUAL-POSTFIX] runtimeGroundVisuals: " + (((val != null) ? ((Object)val).name : null) ?? "null")));
			}
			if (fieldInfo3 != null)
			{
				object value3 = fieldInfo3.GetValue(__instance);
				val2 = (GameObject)((value3 is GameObject) ? value3 : null);
				Log.LogInfo((object)("[VISUAL-POSTFIX] runtimeEquippedVisuals: " + (((val2 != null) ? ((Object)val2).name : null) ?? "null")));
			}
			HashSet<Renderer> hashSet = new HashSet<Renderer>();
			Renderer[] componentsInChildren;
			if ((Object)(object)val != (Object)null)
			{
				componentsInChildren = val.GetComponentsInChildren<Renderer>(true);
				foreach (Renderer item in componentsInChildren)
				{
					hashSet.Add(item);
				}
			}
			if ((Object)(object)val2 != (Object)null)
			{
				componentsInChildren = val2.GetComponentsInChildren<Renderer>(true);
				foreach (Renderer item2 in componentsInChildren)
				{
					hashSet.Add(item2);
				}
			}
			Log.LogInfo((object)$"[VISUAL-POSTFIX] Found {hashSet.Count} renderers to keep (from runtimeGroundVisuals/runtimeEquippedVisuals)");
			MonoBehaviour val3 = (MonoBehaviour)((endlessProp is MonoBehaviour) ? endlessProp : null);
			if ((Object)(object)val3 == (Object)null)
			{
				return;
			}
			Renderer[] componentsInChildren2 = ((Component)val3).GetComponentsInChildren<Renderer>(true);
			Log.LogInfo((object)$"[VISUAL-POSTFIX] Found {componentsInChildren2.Length} total renderers in EndlessProp");
			int num = 0;
			componentsInChildren = componentsInChildren2;
			foreach (Renderer val4 in componentsInChildren)
			{
				if (hashSet.Contains(val4))
				{
					Log.LogInfo((object)("[VISUAL-POSTFIX] Keeping renderer '" + ((Object)((Component)val4).gameObject).name + "' (part of runtime visuals)"));
					continue;
				}
				val4.enabled = false;
				num++;
				Log.LogInfo((object)("[VISUAL-POSTFIX] Hidden renderer '" + ((Object)((Component)val4).gameObject).name + "' (testPrefab visual)"));
			}
			Log.LogInfo((object)$"[VISUAL-POSTFIX] v10.37.0: Hidden {num} testPrefab renderers, preview should still work");
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[VISUAL-POSTFIX] Error in ComponentInitialize postfix: " + ex.Message + "\n" + ex.StackTrace));
		}
	}

	private static int _registrationAttemptCount = 0;

	private static void RegisterDefinitionWithRuntimeDatabase(object clonedDefinition)
	{
		_registrationAttemptCount++;
		try
		{
			// Get GUID first so we can check if already registered
			PropertyInfo guidProp = clonedDefinition.GetType().GetProperty("Guid");
			string attemptedGuid = "UNKNOWN";
			if (guidProp != null)
			{
				object guidVal = guidProp.GetValue(clonedDefinition);
				attemptedGuid = guidVal?.ToString() ?? "NULL";
			}

			Log.LogInfo((object)$"[RUNTIME-DB] Registration attempt #{_registrationAttemptCount} for GUID: {attemptedGuid}");

			// Check if THIS SPECIFIC GUID is already registered (not a global flag!)
			if (_registeredDefinitionGuids.Contains(attemptedGuid))
			{
				Log.LogInfo((object)$"[RUNTIME-DB] GUID {attemptedGuid} already registered, skipping (this is normal for re-spawned items)");
				return;
			}
			Type type = null;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				type = assemblies[i].GetType("Endless.Gameplay.RuntimeDatabase");
				if (type != null)
				{
					break;
				}
			}
			if (type == null)
			{
				Log.LogError((object)"[RUNTIME-DB] Could not find RuntimeDatabase type!");
				return;
			}
			FieldInfo field = type.GetField("usableDefinitionMap", BindingFlags.Static | BindingFlags.NonPublic);
			if (field == null)
			{
				Log.LogError((object)"[RUNTIME-DB] Could not find usableDefinitionMap field!");
				return;
			}
			object value = field.GetValue(null);
			if (value == null)
			{
				Log.LogError((object)"[RUNTIME-DB] usableDefinitionMap is null!");
				return;
			}
			PropertyInfo property = clonedDefinition.GetType().GetProperty("Guid");
			if (property == null)
			{
				Log.LogError((object)"[RUNTIME-DB] Could not find Guid property on definition!");
				return;
			}
			object value2 = property.GetValue(clonedDefinition);
			if (value2 == null)
			{
				Log.LogError((object)"[RUNTIME-DB] Definition Guid is null!");
				return;
			}
			Type type2 = value.GetType();
			PropertyInfo property2 = type2.GetProperty("Item");
			if (property2 != null)
			{
				MethodInfo method = type2.GetMethod("ContainsKey");
				if (method != null && (bool)method.Invoke(value, new object[1] { value2 }))
				{
					Log.LogInfo((object)$"[RUNTIME-DB] Definition GUID {value2} already in RuntimeDatabase, updating");
				}
				property2.SetValue(value, clonedDefinition, new object[1] { value2 });
				_registeredDefinitionGuids.Add(value2.ToString());
				Log.LogInfo((object)$"[RUNTIME-DB] Successfully registered definition with GUID {value2} in RuntimeDatabase!");
				Log.LogInfo((object)$"[RUNTIME-DB] Total registered GUIDs: {_registeredDefinitionGuids.Count}");
				MethodInfo method2 = type2.GetMethod("TryGetValue");
				if (method2 != null)
				{
					object[] parameters = new object[2] { value2, null };
					bool flag = (bool)method2.Invoke(value, parameters);
					Log.LogInfo((object)$"[RUNTIME-DB] Verification: TryGetValue returned {flag}");
				}
			}
			else
			{
				Log.LogError((object)"[RUNTIME-DB] Could not find dictionary indexer!");
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[RUNTIME-DB] Error registering definition: " + ex.Message + "\n" + ex.StackTrace));
		}
	}

	private static void CreatePearlBasketVisualPrefab()
	{
		try
		{
			if (!LoadAssetBundle())
			{
				Log.LogWarning((object)"[VISUAL-INJECT] Could not load asset bundle, using fallback");
				CreateFallbackVisualPrefab();
				return;
			}
			GameObject val = _loadedPrefab ?? _visualPrefab;
			if ((Object)(object)val == (Object)null)
			{
				Log.LogWarning((object)"[VISUAL-INJECT] No source visual found, using fallback");
				CreateFallbackVisualPrefab();
				return;
			}
			_pearlBasketVisualPrefab = val;
			if (_simpleYawSpinType != null)
			{
				Component component = _pearlBasketVisualPrefab.GetComponent(_simpleYawSpinType);
				if ((Object)(object)component != (Object)null)
				{
					Object.DestroyImmediate((Object)(object)component);
					Log.LogInfo((object)"[VISUAL-INJECT] Removed SimpleYawSpin from prefab asset");
				}
			}
			Log.LogInfo((object)("[VISUAL-INJECT] v10.50.0: Using prefab asset directly: " + ((Object)_pearlBasketVisualPrefab).name));
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[VISUAL-INJECT] Failed to setup visual prefab: " + ex.Message));
			CreateFallbackVisualPrefab();
		}
	}

	private static void CreateFallbackVisualPrefab()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		_pearlBasketVisualPrefab = GameObject.CreatePrimitive((PrimitiveType)0);
		((Object)_pearlBasketVisualPrefab).name = "PearlBasket_Fallback";
		_pearlBasketVisualPrefab.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
		Collider component = _pearlBasketVisualPrefab.GetComponent<Collider>();
		if ((Object)(object)component != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)component);
		}
		_pearlBasketVisualPrefab.transform.position = new Vector3(0f, -10000f, 0f);
		Object.DontDestroyOnLoad((Object)(object)_pearlBasketVisualPrefab);
		Log.LogWarning((object)"[VISUAL-INJECT] Using fallback sphere - asset bundle failed to load");
	}

	private void PatchItemPickup()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		if (_itemType != null)
		{
			MethodInfo methodInfo = AccessTools.Method(_itemType, "Pickup", (Type[])null, (Type[])null);
			if (methodInfo != null)
			{
				HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("Item_Pickup_Prefix", BindingFlags.Static | BindingFlags.Public));
				HarmonyMethod val2 = new HarmonyMethod(typeof(Plugin).GetMethod("Item_Pickup_Postfix", BindingFlags.Static | BindingFlags.Public));
				_harmony.Patch((MethodBase)methodInfo, val, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				Log.LogInfo((object)"[INJECTOR] Patched Item.Pickup (prefix+postfix)");
			}
			else
			{
				Log.LogWarning((object)"[INJECTOR] Item.Pickup method not found");
			}
		}
	}

	private void PatchInventoryAttemptPickup()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		if (_inventoryType != null)
		{
			MethodInfo methodInfo = AccessTools.Method(_inventoryType, "AttemptPickupItem", new Type[2]
			{
				_itemType,
				typeof(bool)
			}, (Type[])null);
			if (methodInfo != null)
			{
				HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("Inventory_AttemptPickupItem_Prefix", BindingFlags.Static | BindingFlags.Public));
				HarmonyMethod val2 = new HarmonyMethod(typeof(Plugin).GetMethod("Inventory_AttemptPickupItem_Postfix", BindingFlags.Static | BindingFlags.Public));
				_harmony.Patch((MethodBase)methodInfo, val, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				Log.LogInfo((object)"[INJECTOR] Patched Inventory.AttemptPickupItem(Item) (prefix+postfix)");
			}
			else
			{
				Log.LogWarning((object)"[INJECTOR] Inventory.AttemptPickupItem(Item) method not found");
			}
		}
	}

	private void PatchItemInteractable()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		if (_itemInteractableType != null)
		{
			MethodInfo methodInfo = AccessTools.Method(_itemInteractableType, "AttemptInteract_ServerLogic", (Type[])null, (Type[])null);
			if (methodInfo != null)
			{
				HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("ItemInteractable_AttemptInteract_Prefix", BindingFlags.Static | BindingFlags.Public));
				HarmonyMethod val2 = new HarmonyMethod(typeof(Plugin).GetMethod("ItemInteractable_AttemptInteract_Postfix", BindingFlags.Static | BindingFlags.Public));
				_harmony.Patch((MethodBase)methodInfo, val, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				Log.LogInfo((object)"[INJECTOR] Patched ItemInteractable.AttemptInteract_ServerLogic (prefix+postfix)");
			}
			else
			{
				Log.LogWarning((object)"[INJECTOR] ItemInteractable.AttemptInteract_ServerLogic method not found");
			}
		}
	}

	private void PatchToggleLocalVisibility()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		if (_itemType != null)
		{
			MethodInfo methodInfo = AccessTools.Method(_itemType, "ToggleLocalVisibility", (Type[])null, (Type[])null);
			if (methodInfo != null)
			{
				HarmonyMethod val = new HarmonyMethod(typeof(Plugin).GetMethod("ToggleLocalVisibility_Postfix", BindingFlags.Static | BindingFlags.Public));
				_harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, val, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				Log.LogInfo((object)"[INJECTOR] Patched Item.ToggleLocalVisibility (v10.34.0 animator diagnostic)");
			}
			else
			{
				Log.LogWarning((object)"[INJECTOR] Item.ToggleLocalVisibility method not found");
			}
		}
	}

	public static void ToggleLocalVisibility_Postfix(object __instance, object playerReferences, bool visible, bool useEquipmentAnimation)
	{
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			object obj = ((__instance is MonoBehaviour) ? __instance : null);
			string text = ((obj != null) ? ((Object)obj).name : null) ?? "Unknown";
			Log.LogInfo((object)"==========================================================");
			Log.LogInfo((object)"[EQUIP-ANIM] ===== Item.ToggleLocalVisibility called =====");
			Log.LogInfo((object)("[EQUIP-ANIM] Item: " + text));
			Log.LogInfo((object)$"[EQUIP-ANIM] Visible: {visible}, UseEquipmentAnimation: {useEquipmentAnimation}");
			if (playerReferences != null)
			{
				object obj2 = playerReferences.GetType().GetProperty("ApperanceController")?.GetValue(playerReferences);
				if (obj2 != null)
				{
					object obj3 = obj2.GetType().GetProperty("AppearanceAnimator")?.GetValue(obj2);
					if (obj3 != null)
					{
						object obj4 = obj3.GetType().GetProperty("Animator")?.GetValue(obj3);
						Animator val = (Animator)((obj4 is Animator) ? obj4 : null);
						if ((Object)(object)val != (Object)null)
						{
							AnimatorStateInfo currentAnimatorStateInfo = val.GetCurrentAnimatorStateInfo(0);
							Log.LogInfo((object)$"[EQUIP-ANIM] Animator State Hash: {currentAnimatorStateInfo.shortNameHash}");
							Log.LogInfo((object)$"[EQUIP-ANIM] Animator FullPath Hash: {currentAnimatorStateInfo.fullPathHash}");
							Log.LogInfo((object)$"[EQUIP-ANIM] Animator normalizedTime: {currentAnimatorStateInfo.normalizedTime:F3}");
							int integer = val.GetInteger("EquippedItem");
							bool flag = val.GetBool("Moving");
							bool flag2 = val.GetBool("Walking");
							bool flag3 = val.GetBool("Grounded");
							float num = val.GetFloat("VelX");
							float num2 = val.GetFloat("VelZ");
							float num3 = val.GetFloat("HorizVelMagnitude");
							Log.LogInfo((object)$"[EQUIP-ANIM] EquippedItem: {integer}");
							Log.LogInfo((object)$"[EQUIP-ANIM] Moving: {flag}, Walking: {flag2}, Grounded: {flag3}");
							Log.LogInfo((object)$"[EQUIP-ANIM] VelX: {num:F3}, VelZ: {num2:F3}, HorizVel: {num3:F3}");
							int layerCount = val.layerCount;
							Log.LogInfo((object)$"[EQUIP-ANIM] Animator Layers ({layerCount}):");
							for (int i = 0; i < layerCount; i++)
							{
								float layerWeight = val.GetLayerWeight(i);
								string layerName = val.GetLayerName(i);
								AnimatorStateInfo currentAnimatorStateInfo2 = val.GetCurrentAnimatorStateInfo(i);
								Log.LogInfo((object)$"[EQUIP-ANIM]   Layer {i} '{layerName}': weight={layerWeight:F2}, stateHash={currentAnimatorStateInfo2.shortNameHash}");
							}
							bool flag4 = val.IsInTransition(0);
							Log.LogInfo((object)$"[EQUIP-ANIM] IsInTransition: {flag4}");
							if (flag4)
							{
								AnimatorStateInfo nextAnimatorStateInfo = val.GetNextAnimatorStateInfo(0);
								Log.LogInfo((object)$"[EQUIP-ANIM] NextState Hash: {nextAnimatorStateInfo.shortNameHash}");
							}
						}
						else
						{
							Log.LogWarning((object)"[EQUIP-ANIM] Could not get Animator component");
						}
					}
				}
			}
			Type type = __instance.GetType();
			FieldInfo fieldInfo = AccessTools.Field(type.BaseType ?? type, "equippedItemID");
			int num4 = ((fieldInfo != null) ? ((int)fieldInfo.GetValue(__instance)) : (-1));
			string text2 = AccessTools.Field(type.BaseType ?? type, "equippedItemParamName")?.GetValue(__instance)?.ToString() ?? "Unknown";
			Log.LogInfo((object)$"[EQUIP-ANIM] Item's equippedItemID: {num4}");
			Log.LogInfo((object)("[EQUIP-ANIM] Item's equippedItemParamName: " + text2));
			Log.LogInfo((object)"==========================================================");
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[EQUIP-ANIM] Error in postfix: " + ex.Message));
		}
	}

	public static void Item_Pickup_Prefix(object __instance, object player)
	{
		try
		{
			object obj = ((__instance is MonoBehaviour) ? __instance : null);
			string text = ((obj != null) ? ((Object)obj).name : null) ?? "Unknown";
			string text2 = "Unknown";
			if (player != null)
			{
				PropertyInfo property = player.GetType().GetProperty("name");
				if (property != null)
				{
					text2 = property.GetValue(player)?.ToString() ?? "Unknown";
				}
			}
			Type type = __instance.GetType();
			string name = type.Name;
			string text3 = AccessTools.Field(type, "assetID")?.GetValue(__instance)?.ToString() ?? "Unknown";
			PropertyInfo propertyInfo = AccessTools.Property(type, "IsPickupable");
			bool flag = propertyInfo != null && (bool)propertyInfo.GetValue(__instance);
			string text4 = AccessTools.Property(type, "ItemState")?.GetValue(__instance)?.ToString() ?? "Unknown";
			string text5 = AccessTools.Property(type, "InventorySlot")?.GetValue(__instance)?.ToString() ?? "Unknown";
			PropertyInfo propertyInfo2 = AccessTools.Property(type, "IsStackable");
			bool flag2 = propertyInfo2 != null && (bool)propertyInfo2.GetValue(__instance);
			PropertyInfo propertyInfo3 = AccessTools.Property(type, "StackCount");
			int num = ((!(propertyInfo3 != null)) ? 1 : ((int)propertyInfo3.GetValue(__instance)));
			object obj2 = AccessTools.Property(type, "InventoryUsableDefinition")?.GetValue(__instance);
			string text6 = "None";
			string text7 = "None";
			if (obj2 != null)
			{
				object obj3 = ((obj2 is Object) ? obj2 : null);
				text6 = ((obj3 != null) ? ((Object)obj3).name : null) ?? "Unknown";
				text7 = obj2.GetType().GetProperty("Guid")?.GetValue(obj2)?.ToString() ?? "Unknown";
			}
			Log.LogInfo((object)"==========================================================");
			Log.LogInfo((object)"[ITEM-PICKUP] ===== Item.Pickup CALLED =====");
			Log.LogInfo((object)("[ITEM-PICKUP] Item: " + text + " (Type: " + name + ")"));
			Log.LogInfo((object)("[ITEM-PICKUP] Player: " + text2));
			Log.LogInfo((object)("[ITEM-PICKUP] AssetID: " + text3));
			Log.LogInfo((object)("[ITEM-PICKUP] State: " + text4));
			Log.LogInfo((object)$"[ITEM-PICKUP] IsPickupable: {flag}");
			Log.LogInfo((object)("[ITEM-PICKUP] InventorySlotType: " + text5));
			Log.LogInfo((object)$"[ITEM-PICKUP] IsStackable: {flag2}, StackCount: {num}");
			Log.LogInfo((object)("[ITEM-PICKUP] InventoryUsableDefinition: " + text6));
			Log.LogInfo((object)("[ITEM-PICKUP] Definition GUID: " + text7));
			if (obj2 != null)
			{
				LogInventoryUsableDefinition(obj2);
			}
			Log.LogInfo((object)"==========================================================");
			// v10.63.0: Particle destruction moved to POSTFIX on __result (see Item_Pickup_Postfix)
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[ITEM-PICKUP] Error in prefix: " + ex.Message));
		}
	}

	// v10.63.0: Helper method to permanently destroy particles on ground visuals
	// Uses DestroyImmediate for synchronous destruction
	// v10.65.0: Disable particles on BOTH ground and equipped visuals
	private static void StopAllVisualParticles(object itemInstance, string itemName)
	{
		try
		{
			if (itemInstance == null || _itemType == null)
				return;

			// Disable particles on runtimeGroundVisuals
			DisableParticlesOnVisuals(itemInstance, itemName, "runtimeGroundVisuals", "ground");

			// v10.65.0: Also disable particles on runtimeEquippedVisuals
			DisableParticlesOnVisuals(itemInstance, itemName, "runtimeEquippedVisuals", "equipped");
		}
		catch (Exception ex)
		{
			Log.LogError($"[PARTICLE] Error stopping particles: {ex.Message}\n{ex.StackTrace}");
		}
	}

	private static void DisableParticlesOnVisuals(object itemInstance, string itemName, string fieldName, string visualType)
	{
		FieldInfo visualsField = AccessTools.Field(_itemType, fieldName);
		if (visualsField == null)
		{
			Log.LogWarning($"[PARTICLE] {fieldName} field not found on Item type");
			return;
		}

		GameObject visuals = visualsField.GetValue(itemInstance) as GameObject;
		if (visuals == null)
		{
			Log.LogInfo($"[PARTICLE] No {visualType} visuals for '{itemName}'");
			return;
		}

		// Get all particle systems (including inactive ones)
		ParticleSystem[] particles = visuals.GetComponentsInChildren<ParticleSystem>(true);
		if (particles.Length == 0)
		{
			Log.LogInfo($"[PARTICLE] No particles on '{itemName}' {visualType} visuals");
			return;
		}

		Log.LogInfo($"[PARTICLE] v10.65.0: Disabling {particles.Length} particle(s) on '{itemName}' {visualType} visuals");

		// Disable particles - SetActive(false) persists even when parent is activated
		for (int i = particles.Length - 1; i >= 0; i--)
		{
			ParticleSystem ps = particles[i];
			if (ps != null)
			{
				string psName = ps.name;
				GameObject psGameObject = ps.gameObject;

				// Stop and clear first
				ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
				ps.Clear(true);

				// Disable playOnAwake as backup
				var main = ps.main;
				main.playOnAwake = false;

				// v10.64.0: Disable instead of destroy
				// SetActive(false) persists even when parent is activated
				psGameObject.SetActive(false);

				Log.LogInfo($"[PARTICLE] v10.65.0: Disabled '{psName}' on {visualType} (SetActive false)");
			}
		}
	}

	public static void Item_Pickup_Postfix(object __instance, object __result, object player)
	{
		try
		{
			string itemName = "null";
			if (__result != null)
			{
				object obj = ((__result is MonoBehaviour) ? __result : null);
				itemName = ((obj != null) ? ((Object)obj).name : null) ?? "Unknown";
			}
			Log.LogInfo((object)("[ITEM-PICKUP] Pickup returned: " + itemName));

			// v10.63.0: Destroy particles on the RETURNED item (correct instance for Level Items)
			bool isCustomProp = false;
			if (__result != null)
			{
				// Check if this is a custom prop
				Component resultComponent = __result as Component;
				if (resultComponent != null)
				{
					// v10.63.0 FIX: Use GetComponentInParent because CustomPropMarker is on EndlessProp (parent), not Item (child)
					if (resultComponent.GetComponentInParent<CustomPropMarker>() != null)
					{
						isCustomProp = true;
						Log.LogInfo("[PARTICLE] v10.63.0: Found CustomPropMarker in parent, destroying particles on __result");
						StopAllVisualParticles(__result, itemName);
					}
					else
					{
						// For Level Items, the NEW copy won't have CustomPropMarker in hierarchy yet
						// Check if __instance's parent had it (means it's a custom prop pickup)
						Component instanceComponent = __instance as Component;
						if (instanceComponent != null)
						{
							// v10.63.0 FIX: Use GetComponentInParent because marker is on EndlessProp (parent)
							if (instanceComponent.GetComponentInParent<CustomPropMarker>() != null)
							{
								isCustomProp = true;
								// Add marker to new copy's parent (EndlessProp)
								Transform endlessPropTransform = resultComponent.transform.parent;
								if (endlessPropTransform != null && endlessPropTransform.GetComponent<CustomPropMarker>() == null)
								{
									endlessPropTransform.gameObject.AddComponent<CustomPropMarker>();
								}
								Log.LogInfo("[PARTICLE] v10.63.0: Original had CustomPropMarker, destroying particles on new copy");
								StopAllVisualParticles(__result, itemName);
							}
						}
					}
				}
			}

			// v10.66.0: Check for first pickup popup (custom props only)
			if (isCustomProp && __result != null)
			{
				CheckFirstPickupPopup(__result);
			}

			Log.LogInfo((object)"----------------------------------------------------------");
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[ITEM-PICKUP] Error in postfix: " + ex.Message + "\n" + ex.StackTrace));
		}
	}

	public static void Inventory_AttemptPickupItem_Prefix(object __instance, object item, bool lockItem)
	{
		try
		{
			object obj = ((item is MonoBehaviour) ? item : null);
			string text = ((obj != null) ? ((Object)obj).name : null) ?? "Unknown";
			Type type = __instance.GetType();
			PropertyInfo propertyInfo = AccessTools.Property(type, "TotalInventorySlotCount");
			int num = ((propertyInfo != null) ? ((int)propertyInfo.GetValue(__instance)) : (-1));
			FieldInfo fieldInfo = AccessTools.Field(type, "slots");
			int num2 = -1;
			if (fieldInfo != null && fieldInfo.GetValue(__instance) is IList list)
			{
				num2 = 0;
				foreach (object item2 in list)
				{
					if (item2 == null)
					{
						continue;
					}
					PropertyInfo property = item2.GetType().GetProperty("DefinitionGuid");
					if (property != null)
					{
						object value = property.GetValue(item2);
						object obj2 = (value?.GetType().GetField("Empty", BindingFlags.Static | BindingFlags.Public))?.GetValue(null);
						if (value != null && value.Equals(obj2))
						{
							num2++;
						}
					}
				}
			}
			Log.LogInfo((object)"==========================================================");
			Log.LogInfo((object)"[INVENTORY] ===== Inventory.AttemptPickupItem CALLED =====");
			Log.LogInfo((object)("[INVENTORY] Item: " + text));
			Log.LogInfo((object)$"[INVENTORY] LockItem: {lockItem}");
			Log.LogInfo((object)$"[INVENTORY] TotalSlots: {num}");
			Log.LogInfo((object)$"[INVENTORY] EmptySlots: {num2}");
			Log.LogInfo((object)"==========================================================");
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[INVENTORY] Error in prefix: " + ex.Message));
		}
	}

	public static void Inventory_AttemptPickupItem_Postfix(object __instance, bool __result, object item)
	{
		try
		{
			object obj = ((item is MonoBehaviour) ? item : null);
			string arg = ((obj != null) ? ((Object)obj).name : null) ?? "Unknown";
			Log.LogInfo((object)$"[INVENTORY] AttemptPickupItem result: {__result} (item: {arg})");
			if (__result)
			{
				LogInventoryState(__instance);
			}
			Log.LogInfo((object)"----------------------------------------------------------");
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[INVENTORY] Error in postfix: " + ex.Message));
		}
	}

	public static void ItemInteractable_AttemptInteract_Prefix(object __instance, object interactor)
	{
		try
		{
			object obj = ((__instance is MonoBehaviour) ? __instance : null);
			string text = ((obj != null) ? ((Object)obj).name : null) ?? "Unknown";
			object obj2 = __instance.GetType().GetField("item", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(__instance);
			object obj3 = ((obj2 is MonoBehaviour) ? obj2 : null);
			string text2 = ((obj3 != null) ? ((Object)obj3).name : null) ?? "Unknown";
			Log.LogInfo((object)"==========================================================");
			Log.LogInfo((object)"[INTERACT] ===== ItemInteractable.AttemptInteract =====");
			Log.LogInfo((object)("[INTERACT] Interactable: " + text));
			Log.LogInfo((object)("[INTERACT] Item: " + text2));
			if (obj2 != null)
			{
				Type type = obj2.GetType();
				string text3 = AccessTools.Field(type, "assetID")?.GetValue(obj2)?.ToString() ?? "Unknown";
				Log.LogInfo((object)("[INTERACT] Item AssetID: " + text3));
				Log.LogInfo((object)("[INTERACT] Item Type: " + type.Name));
			}
			Log.LogInfo((object)"==========================================================");
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[INTERACT] Error in prefix: " + ex.Message));
		}
	}

	public static void ItemInteractable_AttemptInteract_Postfix(object __instance, bool __result)
	{
		try
		{
			object obj = ((__instance is MonoBehaviour) ? __instance : null);
			string arg = ((obj != null) ? ((Object)obj).name : null) ?? "Unknown";
			Log.LogInfo((object)$"[INTERACT] AttemptInteract result: {__result} (interactable: {arg})");
			Log.LogInfo((object)"----------------------------------------------------------");
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[INTERACT] Error in postfix: " + ex.Message));
		}
	}

	private static void LogInventoryState(object inventory)
	{
		try
		{
			if (!(AccessTools.Field(inventory.GetType(), "slots")?.GetValue(inventory) is IList list))
			{
				Log.LogInfo((object)"[INVENTORY] Could not read slots");
				return;
			}
			Log.LogInfo((object)$"[INVENTORY] Current inventory state ({list.Count} slots):");
			int num = 0;
			foreach (object item in list)
			{
				if (item == null)
				{
					Log.LogInfo((object)$"[INVENTORY]   Slot {num}: null");
					num++;
					continue;
				}
				Type type = item.GetType();
				string text = type.GetProperty("AssetID")?.GetValue(item)?.ToString() ?? "Empty";
				string text2 = type.GetProperty("DefinitionGuid")?.GetValue(item)?.ToString() ?? "Empty";
				PropertyInfo property = type.GetProperty("Count");
				int num2 = ((property != null) ? ((int)property.GetValue(item)) : 0);
				object obj = type.GetProperty("Item")?.GetValue(item);
				object obj2 = ((obj is MonoBehaviour) ? obj : null);
				string text3 = ((obj2 != null) ? ((Object)obj2).name : null) ?? "null";
				if (!text2.Contains("00000000-0000-0000-0000-000000000000") && obj != null)
				{
					Log.LogInfo((object)$"[INVENTORY]   Slot {num}: {text3} (Count: {num2}, AssetID: {text})");
				}
				else
				{
					Log.LogInfo((object)$"[INVENTORY]   Slot {num}: [EMPTY]");
				}
				num++;
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[INVENTORY] Error logging state: " + ex.Message));
		}
	}

	private static void LogInventoryUsableDefinition(object definition)
	{
		try
		{
			if (definition == null)
			{
				return;
			}
			Type type = definition.GetType();
			Log.LogInfo((object)"[ITEM-DEF] ===== InventoryUsableDefinition Dump =====");
			Log.LogInfo((object)("[ITEM-DEF] Type: " + type.FullName));
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
			Log.LogInfo((object)$"[ITEM-DEF] --- Public Fields ({fields.Length}) ---");
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				try
				{
					object value = fieldInfo.GetValue(definition);
					string text = value?.ToString() ?? "null";
					Object val = (Object)((value is Object) ? value : null);
					if (val != null)
					{
						text = val.name + " (" + ((object)val).GetType().Name + ")";
					}
					Log.LogInfo((object)("[ITEM-DEF]   " + fieldInfo.Name + " (" + fieldInfo.FieldType.Name + "): " + text));
				}
				catch (Exception ex)
				{
					Log.LogInfo((object)("[ITEM-DEF]   " + fieldInfo.Name + ": <error: " + ex.Message + ">"));
				}
			}
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			Log.LogInfo((object)$"[ITEM-DEF] --- Public Properties ({properties.Length}) ---");
			PropertyInfo[] array2 = properties;
			foreach (PropertyInfo propertyInfo in array2)
			{
				if (!propertyInfo.CanRead)
				{
					continue;
				}
				try
				{
					object value2 = propertyInfo.GetValue(definition);
					string text2 = value2?.ToString() ?? "null";
					Object val2 = (Object)((value2 is Object) ? value2 : null);
					if (val2 != null)
					{
						text2 = val2.name + " (" + ((object)val2).GetType().Name + ")";
					}
					Log.LogInfo((object)("[ITEM-DEF]   " + propertyInfo.Name + " (" + propertyInfo.PropertyType.Name + "): " + text2));
				}
				catch (Exception ex2)
				{
					Log.LogInfo((object)("[ITEM-DEF]   " + propertyInfo.Name + ": <error: " + ex2.Message + ">"));
				}
			}
			FieldInfo[] fields2 = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
			Log.LogInfo((object)$"[ITEM-DEF] --- Private Fields ({fields2.Length}) ---");
			array = fields2;
			foreach (FieldInfo fieldInfo2 in array)
			{
				try
				{
					object value3 = fieldInfo2.GetValue(definition);
					string text3 = value3?.ToString() ?? "null";
					Object val3 = (Object)((value3 is Object) ? value3 : null);
					if (val3 != null)
					{
						text3 = val3.name + " (" + ((object)val3).GetType().Name + ")";
					}
					else if (value3 != null && fieldInfo2.FieldType.Name.Contains("Guid"))
					{
						text3 = value3.ToString();
					}
					Log.LogInfo((object)("[ITEM-DEF]   " + fieldInfo2.Name + " (" + fieldInfo2.FieldType.Name + "): " + text3));
				}
				catch (Exception ex3)
				{
					Log.LogInfo((object)("[ITEM-DEF]   " + fieldInfo2.Name + ": <error: " + ex3.Message + ">"));
				}
			}
			Type baseType = type.BaseType;
			if (baseType != null && baseType != typeof(Object) && baseType != typeof(object))
			{
				Log.LogInfo((object)("[ITEM-DEF] --- Base Type: " + baseType.Name + " ---"));
				array = baseType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo3 in array)
				{
					try
					{
						object value4 = fieldInfo3.GetValue(definition);
						string text4 = value4?.ToString() ?? "null";
						Object val4 = (Object)((value4 is Object) ? value4 : null);
						if (val4 != null)
						{
							text4 = val4.name + " (" + ((object)val4).GetType().Name + ")";
						}
						Log.LogInfo((object)("[ITEM-DEF]   (base) " + fieldInfo3.Name + " (" + fieldInfo3.FieldType.Name + "): " + text4));
					}
					catch
					{
					}
				}
			}
			Log.LogInfo((object)"[ITEM-DEF] ===== End Definition Dump =====");
		}
		catch (Exception ex4)
		{
			Log.LogError((object)("[ITEM-DEF] Error dumping definition: " + ex4.Message));
		}
	}

	private static void ResearchCollectibleSystem()
	{
		if (!_collectibleResearchDone)
		{
			_collectibleResearchDone = true;
			Log.LogInfo((object)"==========================================================");
			Log.LogInfo((object)"[COLLECT-RESEARCH] ===== COLLECTIBLE SYSTEM RESEARCH =====");
			Log.LogInfo((object)"==========================================================");
			ResearchTreasureItemStructure();
			ResearchDefinitionRegistry();
			ResearchItemInteractableStructure();
			ResearchInteractionLayers();
			Log.LogInfo((object)"==========================================================");
			Log.LogInfo((object)"[COLLECT-RESEARCH] ===== END COLLECTIBLE RESEARCH =====");
			Log.LogInfo((object)"==========================================================");
		}
	}

	private static void ResearchTreasureItemStructure()
	{
		Log.LogInfo((object)"[COLLECT-RESEARCH] --- TreasureItem Structure ---");
		try
		{
			Type type = null;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				type = assemblies[i].GetType("Endless.Gameplay.TreasureItem");
				if (type != null)
				{
					break;
				}
			}
			if (type == null)
			{
				Log.LogWarning((object)"[COLLECT-RESEARCH] TreasureItem type NOT FOUND");
				return;
			}
			Log.LogInfo((object)("[COLLECT-RESEARCH] TreasureItem found: " + type.FullName));
			Log.LogInfo((object)("[COLLECT-RESEARCH] Base type: " + type.BaseType?.FullName));
			FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			Log.LogInfo((object)$"[COLLECT-RESEARCH] TreasureItem Fields ({fields.Length}):");
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				object[] customAttributes = fieldInfo.GetCustomAttributes(inherit: true);
				string text = ((customAttributes.Length != 0) ? (" [" + string.Join(",", customAttributes.Select((object a) => a.GetType().Name)) + "]") : "");
				Log.LogInfo((object)("[COLLECT-RESEARCH]   " + fieldInfo.Name + " (" + fieldInfo.FieldType.Name + ")" + text));
			}
			PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			Log.LogInfo((object)$"[COLLECT-RESEARCH] TreasureItem Properties ({properties.Length}):");
			PropertyInfo[] array2 = properties;
			foreach (PropertyInfo propertyInfo in array2)
			{
				Log.LogInfo((object)$"[COLLECT-RESEARCH]   {propertyInfo.Name} ({propertyInfo.PropertyType.Name}) get={propertyInfo.CanRead} set={propertyInfo.CanWrite}");
			}
			Type baseType = type.BaseType;
			if (baseType != null && baseType.Name == "Item")
			{
				Log.LogInfo((object)"[COLLECT-RESEARCH] --- Base Item class fields ---");
				array = baseType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo2 in array)
				{
					object[] customAttributes2 = fieldInfo2.GetCustomAttributes(inherit: true);
					string text2 = ((customAttributes2.Length != 0) ? (" [" + string.Join(",", customAttributes2.Select((object a) => a.GetType().Name)) + "]") : "");
					Log.LogInfo((object)("[COLLECT-RESEARCH]   (base) " + fieldInfo2.Name + " (" + fieldInfo2.FieldType.Name + ")" + text2));
				}
			}
			Object[] array3 = Object.FindObjectsOfType(type);
			Log.LogInfo((object)$"[COLLECT-RESEARCH] Found {array3.Length} TreasureItem instances in scene");
			if (array3.Length == 0)
			{
				return;
			}
			Object val = array3[0];
			MonoBehaviour val2 = (MonoBehaviour)(object)((val is MonoBehaviour) ? val : null);
			Log.LogInfo((object)("[COLLECT-RESEARCH] --- Dumping first TreasureItem instance: " + ((val2 != null) ? ((Object)val2).name : null) + " ---"));
			array = fields;
			foreach (FieldInfo fieldInfo3 in array)
			{
				try
				{
					object value = fieldInfo3.GetValue(val);
					string text3 = value?.ToString() ?? "null";
					Object val3 = (Object)((value is Object) ? value : null);
					if (val3 != null)
					{
						text3 = val3.name + " (" + ((object)val3).GetType().Name + ")";
					}
					Log.LogInfo((object)("[COLLECT-RESEARCH]   " + fieldInfo3.Name + " = " + text3));
				}
				catch
				{
				}
			}
			if (baseType != null)
			{
				array = baseType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo4 in array)
				{
					try
					{
						object value2 = fieldInfo4.GetValue(val);
						string text4 = value2?.ToString() ?? "null";
						Object val4 = (Object)((value2 is Object) ? value2 : null);
						if (val4 != null)
						{
							text4 = val4.name + " (" + ((object)val4).GetType().Name + ")";
						}
						Log.LogInfo((object)("[COLLECT-RESEARCH]   (base) " + fieldInfo4.Name + " = " + text4));
					}
					catch
					{
					}
				}
			}
			if (!((Object)(object)val2 != (Object)null))
			{
				return;
			}
			Log.LogInfo((object)$"[COLLECT-RESEARCH] GameObject layer: {((Component)val2).gameObject.layer} ({LayerMask.LayerToName(((Component)val2).gameObject.layer)})");
			Log.LogInfo((object)("[COLLECT-RESEARCH] GameObject tag: " + ((Component)val2).gameObject.tag));
			Collider[] components = ((Component)val2).GetComponents<Collider>();
			Log.LogInfo((object)$"[COLLECT-RESEARCH] Colliders on object: {components.Length}");
			Collider[] array4 = components;
			foreach (Collider val5 in array4)
			{
				Log.LogInfo((object)$"[COLLECT-RESEARCH]   Collider: {((object)val5).GetType().Name}, isTrigger={val5.isTrigger}, enabled={val5.enabled}");
			}
			Collider[] componentsInChildren = ((Component)val2).GetComponentsInChildren<Collider>();
			if (componentsInChildren.Length <= components.Length)
			{
				return;
			}
			Log.LogInfo((object)$"[COLLECT-RESEARCH] Child colliders: {componentsInChildren.Length - components.Length}");
			array4 = componentsInChildren;
			foreach (Collider val6 in array4)
			{
				if (!components.Contains(val6))
				{
					Log.LogInfo((object)$"[COLLECT-RESEARCH]   Child {((Object)((Component)val6).gameObject).name}: {((object)val6).GetType().Name}, layer={((Component)val6).gameObject.layer}, isTrigger={val6.isTrigger}");
				}
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[COLLECT-RESEARCH] TreasureItem research error: " + ex.Message));
		}
	}

	private static void ResearchDefinitionRegistry()
	{
		Log.LogInfo((object)"[COLLECT-RESEARCH] --- Definition Registry Search ---");
		try
		{
			string[] array = new string[5] { "Endless.Gameplay.Inventory.InventoryUsableDefinitionList", "Endless.Gameplay.Inventory.InventoryDefinitionRegistry", "Endless.Gameplay.InventoryUsableDefinitionList", "Endless.Gameplay.ItemDefinitionList", "Endless.Gameplay.RuntimeDatabase" };
			Assembly[] assemblies;
			foreach (string name in array)
			{
				Type type = null;
				assemblies = AppDomain.CurrentDomain.GetAssemblies();
				for (int j = 0; j < assemblies.Length; j++)
				{
					type = assemblies[j].GetType(name);
					if (type != null)
					{
						break;
					}
				}
				if (!(type != null))
				{
					continue;
				}
				Log.LogInfo((object)("[COLLECT-RESEARCH] Found registry type: " + type.FullName));
				PropertyInfo property = type.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
				if (property != null)
				{
					object value = property.GetValue(null);
					Log.LogInfo((object)$"[COLLECT-RESEARCH]   Has Instance singleton: {value != null}");
					if (value != null)
					{
						DumpRegistryContents(value, type);
					}
				}
				FieldInfo field = type.GetField("Instance", BindingFlags.Static | BindingFlags.Public);
				if (field != null)
				{
					object value2 = field.GetValue(null);
					Log.LogInfo((object)$"[COLLECT-RESEARCH]   Has Instance field: {value2 != null}");
					if (value2 != null)
					{
						DumpRegistryContents(value2, type);
					}
				}
				FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo in fields)
				{
					if (fieldInfo.FieldType.Name.Contains("List") || fieldInfo.FieldType.Name.Contains("Dictionary") || fieldInfo.FieldType.Name.Contains("Array"))
					{
						Log.LogInfo((object)("[COLLECT-RESEARCH]   Collection field: " + fieldInfo.Name + " (" + fieldInfo.FieldType.Name + ")"));
					}
				}
			}
			List<ScriptableObject> list = (from so in Resources.FindObjectsOfTypeAll<ScriptableObject>()
				where ((Object)so).name.Contains("Definition") || ((object)so).GetType().Name.Contains("Definition")
				select so).ToList();
			Log.LogInfo((object)$"[COLLECT-RESEARCH] Found {list.Count} Definition ScriptableObjects:");
			foreach (ScriptableObject item in list.Take(20))
			{
				Log.LogInfo((object)("[COLLECT-RESEARCH]   " + ((Object)item).name + " (" + ((object)item).GetType().Name + ")"));
			}
			if (list.Count > 20)
			{
				Log.LogInfo((object)$"[COLLECT-RESEARCH]   ... and {list.Count - 20} more");
			}
			Type type2 = null;
			assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				type2 = assemblies[i].GetType("Endless.Gameplay.Inventory.InventoryUsableDefinition");
				if (type2 != null)
				{
					break;
				}
			}
			if (!(type2 != null))
			{
				return;
			}
			Log.LogInfo((object)("[COLLECT-RESEARCH] InventoryUsableDefinition type: " + type2.FullName));
			Object[] array2 = Resources.FindObjectsOfTypeAll(type2);
			Log.LogInfo((object)$"[COLLECT-RESEARCH] Found {array2.Length} InventoryUsableDefinition instances:");
			Object[] array3 = array2;
			foreach (Object val in array3)
			{
				ScriptableObject val2 = (ScriptableObject)(object)((val is ScriptableObject) ? val : null);
				if ((Object)(object)val2 != (Object)null)
				{
					string text = type2.GetProperty("Guid")?.GetValue(val)?.ToString() ?? "unknown";
					Log.LogInfo((object)("[COLLECT-RESEARCH]   " + ((Object)val2).name + " (GUID: " + text + ")"));
				}
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[COLLECT-RESEARCH] Registry research error: " + ex.Message));
		}
	}

	private static void DumpRegistryContents(object instance, Type registryType)
	{
		try
		{
			FieldInfo[] fields = registryType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				object value = fieldInfo.GetValue(instance);
				if (value is IList list)
				{
					Log.LogInfo((object)$"[COLLECT-RESEARCH]     {fieldInfo.Name}: List with {list.Count} items");
					int num = 0;
					foreach (object item in list)
					{
						if (num >= 10)
						{
							Log.LogInfo((object)$"[COLLECT-RESEARCH]       ... and {list.Count - 10} more");
							break;
						}
						object obj = ((item is Object) ? item : null);
						string arg = ((obj != null) ? ((Object)obj).name : null) ?? item?.ToString() ?? "null";
						Log.LogInfo((object)$"[COLLECT-RESEARCH]       [{num}] {arg}");
						num++;
					}
				}
				else if (value is IDictionary dictionary)
				{
					Log.LogInfo((object)$"[COLLECT-RESEARCH]     {fieldInfo.Name}: Dictionary with {dictionary.Count} entries");
				}
			}
		}
		catch
		{
		}
	}

	private static void ResearchItemInteractableStructure()
	{
		Log.LogInfo((object)"[COLLECT-RESEARCH] --- ItemInteractable Structure ---");
		try
		{
			Type type = null;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				type = assemblies[i].GetType("Endless.Gameplay.ItemInteractable");
				if (type != null)
				{
					break;
				}
			}
			if (type == null)
			{
				Log.LogWarning((object)"[COLLECT-RESEARCH] ItemInteractable type NOT FOUND");
				return;
			}
			Log.LogInfo((object)("[COLLECT-RESEARCH] ItemInteractable: " + type.FullName));
			Log.LogInfo((object)("[COLLECT-RESEARCH] Base type: " + type.BaseType?.FullName));
			FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			Log.LogInfo((object)$"[COLLECT-RESEARCH] Fields ({fields.Length}):");
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				object[] customAttributes = fieldInfo.GetCustomAttributes(inherit: true);
				string text = ((customAttributes.Length != 0) ? (" [" + string.Join(",", customAttributes.Select((object a) => a.GetType().Name)) + "]") : "");
				Log.LogInfo((object)("[COLLECT-RESEARCH]   " + fieldInfo.Name + " (" + fieldInfo.FieldType.Name + ")" + text));
			}
			Object[] array2 = Object.FindObjectsOfType(type);
			Log.LogInfo((object)$"[COLLECT-RESEARCH] Found {array2.Length} ItemInteractable instances");
			if (array2.Length == 0)
			{
				return;
			}
			Object val = array2[0];
			MonoBehaviour val2 = (MonoBehaviour)(object)((val is MonoBehaviour) ? val : null);
			Log.LogInfo((object)("[COLLECT-RESEARCH] First instance: " + ((val2 != null) ? ((Object)val2).name : null)));
			array = fields;
			foreach (FieldInfo fieldInfo2 in array)
			{
				try
				{
					object value = fieldInfo2.GetValue(val);
					string text2 = value?.ToString() ?? "null";
					Object val3 = (Object)((value is Object) ? value : null);
					if (val3 != null)
					{
						text2 = val3.name + " (" + ((object)val3).GetType().Name + ")";
					}
					Log.LogInfo((object)("[COLLECT-RESEARCH]   " + fieldInfo2.Name + " = " + text2));
				}
				catch
				{
				}
			}
			if (!(type.BaseType != null))
			{
				return;
			}
			FieldInfo[] fields2 = type.BaseType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			Log.LogInfo((object)"[COLLECT-RESEARCH] Base class fields:");
			array = fields2;
			foreach (FieldInfo fieldInfo3 in array)
			{
				try
				{
					object value2 = fieldInfo3.GetValue(val);
					string text3 = value2?.ToString() ?? "null";
					Object val4 = (Object)((value2 is Object) ? value2 : null);
					if (val4 != null)
					{
						text3 = val4.name + " (" + ((object)val4).GetType().Name + ")";
					}
					Log.LogInfo((object)("[COLLECT-RESEARCH]   (base) " + fieldInfo3.Name + " = " + text3));
				}
				catch
				{
				}
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[COLLECT-RESEARCH] ItemInteractable research error: " + ex.Message));
		}
	}

	private static void ResearchInteractionLayers()
	{
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		Log.LogInfo((object)"[COLLECT-RESEARCH] --- Interaction Layers ---");
		try
		{
			Log.LogInfo((object)"[COLLECT-RESEARCH] All Unity Layers:");
			for (int i = 0; i < 32; i++)
			{
				string text = LayerMask.LayerToName(i);
				if (!string.IsNullOrEmpty(text))
				{
					Log.LogInfo((object)$"[COLLECT-RESEARCH]   Layer {i}: {text}");
				}
			}
			Type type = null;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int j = 0; j < assemblies.Length; j++)
			{
				type = assemblies[j].GetType("Endless.Gameplay.PlayerInteractor");
				if (type != null)
				{
					break;
				}
			}
			FieldInfo[] array;
			if (type != null)
			{
				Log.LogInfo((object)("[COLLECT-RESEARCH] PlayerInteractor found: " + type.FullName));
				FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				array = fields;
				foreach (FieldInfo fieldInfo in array)
				{
					if (fieldInfo.FieldType == typeof(LayerMask) || fieldInfo.Name.ToLower().Contains("layer") || fieldInfo.Name.ToLower().Contains("mask"))
					{
						Log.LogInfo((object)("[COLLECT-RESEARCH]   Layer field: " + fieldInfo.Name + " (" + fieldInfo.FieldType.Name + ")"));
					}
				}
				Object[] array2 = Object.FindObjectsOfType(type);
				if (array2.Length != 0)
				{
					Object obj = array2[0];
					array = fields;
					foreach (FieldInfo fieldInfo2 in array)
					{
						if (fieldInfo2.FieldType == typeof(LayerMask))
						{
							try
							{
								LayerMask val = (LayerMask)fieldInfo2.GetValue(obj);
								Log.LogInfo((object)$"[COLLECT-RESEARCH]   {fieldInfo2.Name} = {val.value} (binary: {Convert.ToString(val.value, 2)})");
							}
							catch
							{
							}
						}
					}
				}
			}
			Type type2 = null;
			assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int j = 0; j < assemblies.Length; j++)
			{
				type2 = assemblies[j].GetType("Endless.Gameplay.InteractableBase");
				if (type2 != null)
				{
					break;
				}
			}
			if (!(type2 != null))
			{
				return;
			}
			array = type2.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo3 in array)
			{
				if (fieldInfo3.FieldType == typeof(LayerMask) || fieldInfo3.Name.ToLower().Contains("layer"))
				{
					Log.LogInfo((object)("[COLLECT-RESEARCH]   InteractableBase." + fieldInfo3.Name + " (" + fieldInfo3.FieldType.Name + ")"));
				}
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)("[COLLECT-RESEARCH] Layer research error: " + ex.Message));
		}
	}

	private static void LogAvailableShaders()
	{
		try
		{
			Type type = AccessTools.TypeByName("Endless.Gameplay.VisualManagement.ShaderClusterManager");
			if (type == null)
			{
				return;
			}
			object obj = type.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)?.GetValue(null);
			if (obj == null || !(AccessTools.Field(type, "shaderClusterList")?.GetValue(obj) is IList list))
			{
				return;
			}
			foreach (object item in list)
			{
				if (item != null)
				{
					string text = (item.GetType().GetField("DisplayId")?.GetValue(item) as string) ?? "Unknown";
					object obj2 = item.GetType().GetField("primaryShader")?.GetValue(item);
					Shader val = (Shader)((obj2 is Shader) ? obj2 : null);
					Log.LogInfo((object)("[INJECTOR] Cluster " + text + ": " + (((val != null) ? ((Object)val).name : null) ?? "null")));
				}
			}
		}
		catch (Exception arg)
		{
			Log.LogError((object)$"[INJECTOR] LogAvailableShaders failed: {arg}");
		}
	}

	// ========== DIAGNOSTIC: Transform hierarchy logging ==========

	private static void LogTransformHierarchy(Transform t, string prefix, int depth)
	{
		if (t == null || depth > 5) return;

		string indent = new string(' ', depth * 2);
		Vector3 localPos = t.localPosition;
		Quaternion localRot = t.localRotation;
		Vector3 euler = localRot.eulerAngles;

		Log.LogInfo((object)$"{prefix}{indent}'{t.name}' localPos=({localPos.x:F2},{localPos.y:F2},{localPos.z:F2}) localRot=({euler.x:F1},{euler.y:F1},{euler.z:F1})");

		foreach (Transform child in t)
		{
			LogTransformHierarchy(child, prefix, depth + 1);
		}
	}

	// ========== DIAGNOSTIC: Inventory.EquipSlot logging ==========

	private void PatchInventoryEquipSlotDiagnostic()
	{
		try
		{
			Type inventoryType = AccessTools.TypeByName("Endless.Gameplay.Inventory");
			if (inventoryType == null)
			{
				Log.LogWarning((object)"[DIAG-EQUIP] Inventory type not found");
				return;
			}

			MethodInfo equipMethod = AccessTools.Method(inventoryType, "EquipSlot_ServerRPC");
			if (equipMethod == null)
			{
				Log.LogWarning((object)"[DIAG-EQUIP] EquipSlot_ServerRPC method not found");
				return;
			}

			HarmonyMethod prefix = new HarmonyMethod(typeof(Plugin).GetMethod(nameof(EquipSlot_Diagnostic_Prefix), BindingFlags.Static | BindingFlags.Public));
			_harmony.Patch(equipMethod, prefix, null);
			Log.LogInfo((object)"[DIAG-EQUIP] Patched Inventory.EquipSlot_ServerRPC for diagnostics");
		}
		catch (Exception ex)
		{
			Log.LogError((object)$"[DIAG-EQUIP] Failed to patch: {ex.Message}");
		}
	}

	public static void EquipSlot_Diagnostic_Prefix(object __instance, int slotIndex, int equipmentSlot)
	{
		try
		{
			Log.LogInfo((object)$"[DIAG-EQUIP] EquipSlot_ServerRPC called: slotIndex={slotIndex}, equipmentSlot={equipmentSlot}");

			// Get slots field
			FieldInfo slotsField = __instance.GetType().GetField("slots", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (slotsField == null)
			{
				Log.LogWarning((object)"[DIAG-EQUIP] Could not find slots field");
				return;
			}

			object slots = slotsField.GetValue(__instance);
			if (slots == null)
			{
				Log.LogWarning((object)"[DIAG-EQUIP] slots is null");
				return;
			}

			// Get the slot at slotIndex
			PropertyInfo indexer = slots.GetType().GetProperty("Item");
			if (indexer == null)
			{
				Log.LogWarning((object)"[DIAG-EQUIP] Could not find indexer on slots");
				return;
			}

			object slot = indexer.GetValue(slots, new object[] { slotIndex });
			if (slot == null)
			{
				Log.LogWarning((object)$"[DIAG-EQUIP] Slot at index {slotIndex} is null");
				return;
			}

			// Get DefinitionGuid from slot
			PropertyInfo guidProp = slot.GetType().GetProperty("DefinitionGuid");
			if (guidProp == null)
			{
				Log.LogWarning((object)"[DIAG-EQUIP] Could not find DefinitionGuid property");
				return;
			}

			object guid = guidProp.GetValue(slot);
			Log.LogInfo((object)$"[DIAG-EQUIP] Slot {slotIndex} has DefinitionGuid: {guid}");

			// Now check if this GUID exists in RuntimeDatabase
			DumpRuntimeDatabaseEntry(guid);
		}
		catch (Exception ex)
		{
			Log.LogError((object)$"[DIAG-EQUIP] Error in diagnostic: {ex.Message}");
		}
	}

	private static void DumpRuntimeDatabaseEntry(object guid)
	{
		try
		{
			Type runtimeDbType = AccessTools.TypeByName("Endless.Gameplay.RuntimeDatabase");
			if (runtimeDbType == null)
			{
				Log.LogWarning((object)"[DIAG-DB] RuntimeDatabase type not found");
				return;
			}

			FieldInfo mapField = runtimeDbType.GetField("usableDefinitionMap", BindingFlags.Static | BindingFlags.NonPublic);
			if (mapField == null)
			{
				Log.LogWarning((object)"[DIAG-DB] usableDefinitionMap field not found");
				return;
			}

			object map = mapField.GetValue(null);
			if (map == null)
			{
				Log.LogWarning((object)"[DIAG-DB] usableDefinitionMap is null");
				return;
			}

			// Check if GUID exists
			MethodInfo containsKey = map.GetType().GetMethod("ContainsKey");
			bool exists = (bool)containsKey.Invoke(map, new object[] { guid });

			if (exists)
			{
				Log.LogInfo((object)$"[DIAG-DB] GUID {guid} EXISTS in RuntimeDatabase ✓");
				MethodInfo tryGetValue = map.GetType().GetMethod("TryGetValue");
				object[] args = new object[] { guid, null };
				tryGetValue.Invoke(map, args);
				object def = args[1];
				if (def != null)
				{
					PropertyInfo nameProp = def.GetType().GetProperty("name");
					string defName = nameProp?.GetValue(def)?.ToString() ?? "UNKNOWN";
					Log.LogInfo((object)$"[DIAG-DB] Definition name: {defName}");
				}
			}
			else
			{
				Log.LogError((object)$"[DIAG-DB] GUID {guid} NOT FOUND in RuntimeDatabase! This will cause KeyNotFoundException!");
				Log.LogError((object)"[DIAG-DB] Number key equip will FAIL for this item!");

				// Dump all registered GUIDs for comparison
				Log.LogInfo((object)"[DIAG-DB] Dumping all registered GUIDs:");
				PropertyInfo keysProp = map.GetType().GetProperty("Keys");
				if (keysProp != null)
				{
					IEnumerable keys = (IEnumerable)keysProp.GetValue(map);
					int count = 0;
					foreach (object key in keys)
					{
						count++;
						Log.LogInfo((object)$"[DIAG-DB]   #{count}: {key}");
						if (count >= 20)
						{
							Log.LogInfo((object)"[DIAG-DB]   ... (truncated)");
							break;
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Log.LogError((object)$"[DIAG-DB] Error dumping: {ex.Message}");
		}
	}

	// ============================================================================
	// UI DIAGNOSTIC SYSTEM - Press F9 while prop tool is active to capture UI data
	// ============================================================================

	/// <summary>
	/// Diagnoses the prop tool window UI at runtime and writes the hierarchy to a file.
	/// Call this while the prop tool panel is visible in Creator mode.
	/// </summary>
	private void DiagnoseUIPropToolWindow()
	{
		Log.LogInfo((object)"[DIAG-UI] Starting UI diagnostic...");

		try
		{
			// Try to find any UI panel that might be the prop tool or detail view
			// We'll search for common UI patterns
			StringBuilder report = new StringBuilder();
			report.AppendLine("=== ENDSTAR UI DIAGNOSTIC REPORT ===");
			report.AppendLine($"Timestamp: {DateTime.Now}");
			report.AppendLine();

			// Find all Canvas objects in scene
			Canvas[] allCanvases = Object.FindObjectsOfType<Canvas>();
			report.AppendLine($"Found {allCanvases.Length} Canvas objects in scene");
			report.AppendLine();

			foreach (Canvas canvas in allCanvases)
			{
				if (!canvas.gameObject.activeInHierarchy) continue;

				report.AppendLine($"=== CANVAS: {canvas.gameObject.name} ===");
				report.AppendLine($"  renderMode: {canvas.renderMode}");
				report.AppendLine($"  sortingOrder: {canvas.sortingOrder}");
				report.AppendLine();

				// Look for tool panels or detail views
				var panels = canvas.GetComponentsInChildren<MonoBehaviour>(false);
				foreach (var panel in panels)
				{
					string typeName = panel.GetType().Name;
					// Look for panels that might be prop tool related
					if (typeName.Contains("Prop") || typeName.Contains("Tool") ||
					    typeName.Contains("Panel") || typeName.Contains("Detail") ||
					    typeName.Contains("Runtime") || typeName.Contains("Info"))
					{
						report.AppendLine($"--- Found interesting component: {typeName} on {panel.gameObject.name} ---");
						CaptureUIHierarchy(panel.gameObject, report, 1);
						report.AppendLine();
					}
				}
			}

			// Also search for any object with "PropTool" or "RuntimeProp" in name
			var allGameObjects = Object.FindObjectsOfType<GameObject>();
			report.AppendLine("=== SEARCHING FOR PROP TOOL RELATED OBJECTS ===");
			foreach (var go in allGameObjects)
			{
				if (!go.activeInHierarchy) continue;
				if (go.name.Contains("PropTool") || go.name.Contains("RuntimeProp") ||
				    go.name.Contains("PropInfo") || go.name.Contains("PropDetail"))
				{
					report.AppendLine($"--- Found: {go.name} ---");
					CaptureUIHierarchy(go, report, 1);
					report.AppendLine();
				}
			}

			// Write to file
			string configDir = Path.Combine(Paths.PluginPath, "Config");
			if (!Directory.Exists(configDir))
			{
				Directory.CreateDirectory(configDir);
			}
			string path = Path.Combine(configDir, "PropToolDiagnostic.txt");
			File.WriteAllText(path, report.ToString());
			Log.LogInfo((object)$"[DIAG-UI] UI diagnostic written to: {path}");
		}
		catch (Exception ex)
		{
			Log.LogError((object)$"[DIAG-UI] Error during diagnostic: {ex.Message}\n{ex.StackTrace}");
		}
	}

	/// <summary>
	/// Recursively captures UI hierarchy data from a GameObject.
	/// </summary>
	private void CaptureUIHierarchy(GameObject go, StringBuilder sb, int indent)
	{
		string prefix = new string(' ', indent * 2);
		sb.AppendLine($"{prefix}[{go.name}] (active: {go.activeSelf})");

		// RectTransform
		RectTransform rt = go.GetComponent<RectTransform>();
		if (rt != null)
		{
			sb.AppendLine($"{prefix}  RectTransform:");
			sb.AppendLine($"{prefix}    anchorMin: ({rt.anchorMin.x:F3}, {rt.anchorMin.y:F3})");
			sb.AppendLine($"{prefix}    anchorMax: ({rt.anchorMax.x:F3}, {rt.anchorMax.y:F3})");
			sb.AppendLine($"{prefix}    pivot: ({rt.pivot.x:F3}, {rt.pivot.y:F3})");
			sb.AppendLine($"{prefix}    sizeDelta: ({rt.sizeDelta.x:F1}, {rt.sizeDelta.y:F1})");
			sb.AppendLine($"{prefix}    anchoredPosition: ({rt.anchoredPosition.x:F1}, {rt.anchoredPosition.y:F1})");
			sb.AppendLine($"{prefix}    localScale: ({rt.localScale.x:F2}, {rt.localScale.y:F2}, {rt.localScale.z:F2})");
		}

		// Image
		Image img = go.GetComponent<Image>();
		if (img != null)
		{
			sb.AppendLine($"{prefix}  Image:");
			sb.AppendLine($"{prefix}    color: RGBA({img.color.r:F3}, {img.color.g:F3}, {img.color.b:F3}, {img.color.a:F3})");
			sb.AppendLine($"{prefix}    sprite: {(img.sprite != null ? img.sprite.name : "null")}");
			sb.AppendLine($"{prefix}    type: {img.type}");
			sb.AppendLine($"{prefix}    raycastTarget: {img.raycastTarget}");
		}

		// RawImage
		RawImage rawImg = go.GetComponent<RawImage>();
		if (rawImg != null)
		{
			sb.AppendLine($"{prefix}  RawImage:");
			sb.AppendLine($"{prefix}    color: RGBA({rawImg.color.r:F3}, {rawImg.color.g:F3}, {rawImg.color.b:F3}, {rawImg.color.a:F3})");
			sb.AppendLine($"{prefix}    texture: {(rawImg.texture != null ? rawImg.texture.name : "null")}");
		}

		// TextMeshProUGUI
		TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
		if (tmp != null)
		{
			sb.AppendLine($"{prefix}  TextMeshProUGUI:");
			string text = tmp.text.Length > 50 ? tmp.text.Substring(0, 50) + "..." : tmp.text;
			sb.AppendLine($"{prefix}    text: \"{text.Replace("\n", "\\n")}\"");
			sb.AppendLine($"{prefix}    color: RGBA({tmp.color.r:F3}, {tmp.color.g:F3}, {tmp.color.b:F3}, {tmp.color.a:F3})");
			sb.AppendLine($"{prefix}    fontSize: {tmp.fontSize}");
			sb.AppendLine($"{prefix}    fontStyle: {tmp.fontStyle}");
			sb.AppendLine($"{prefix}    alignment: {tmp.alignment}");
			sb.AppendLine($"{prefix}    font: {(tmp.font != null ? tmp.font.name : "null")}");
		}

		// Legacy Text
		Text textComp = go.GetComponent<Text>();
		if (textComp != null)
		{
			sb.AppendLine($"{prefix}  Text (Legacy):");
			string text = textComp.text.Length > 50 ? textComp.text.Substring(0, 50) + "..." : textComp.text;
			sb.AppendLine($"{prefix}    text: \"{text.Replace("\n", "\\n")}\"");
			sb.AppendLine($"{prefix}    color: RGBA({textComp.color.r:F3}, {textComp.color.g:F3}, {textComp.color.b:F3}, {textComp.color.a:F3})");
			sb.AppendLine($"{prefix}    fontSize: {textComp.fontSize}");
			sb.AppendLine($"{prefix}    fontStyle: {textComp.fontStyle}");
			sb.AppendLine($"{prefix}    alignment: {textComp.alignment}");
		}

		// VerticalLayoutGroup
		VerticalLayoutGroup vlg = go.GetComponent<VerticalLayoutGroup>();
		if (vlg != null)
		{
			sb.AppendLine($"{prefix}  VerticalLayoutGroup:");
			sb.AppendLine($"{prefix}    spacing: {vlg.spacing}");
			sb.AppendLine($"{prefix}    padding: L:{vlg.padding.left} R:{vlg.padding.right} T:{vlg.padding.top} B:{vlg.padding.bottom}");
			sb.AppendLine($"{prefix}    childAlignment: {vlg.childAlignment}");
			sb.AppendLine($"{prefix}    childForceExpandWidth: {vlg.childForceExpandWidth}");
			sb.AppendLine($"{prefix}    childForceExpandHeight: {vlg.childForceExpandHeight}");
		}

		// HorizontalLayoutGroup
		HorizontalLayoutGroup hlg = go.GetComponent<HorizontalLayoutGroup>();
		if (hlg != null)
		{
			sb.AppendLine($"{prefix}  HorizontalLayoutGroup:");
			sb.AppendLine($"{prefix}    spacing: {hlg.spacing}");
			sb.AppendLine($"{prefix}    padding: L:{hlg.padding.left} R:{hlg.padding.right} T:{hlg.padding.top} B:{hlg.padding.bottom}");
			sb.AppendLine($"{prefix}    childAlignment: {hlg.childAlignment}");
		}

		// GridLayoutGroup
		GridLayoutGroup glg = go.GetComponent<GridLayoutGroup>();
		if (glg != null)
		{
			sb.AppendLine($"{prefix}  GridLayoutGroup:");
			sb.AppendLine($"{prefix}    cellSize: ({glg.cellSize.x}, {glg.cellSize.y})");
			sb.AppendLine($"{prefix}    spacing: ({glg.spacing.x}, {glg.spacing.y})");
			sb.AppendLine($"{prefix}    padding: L:{glg.padding.left} R:{glg.padding.right} T:{glg.padding.top} B:{glg.padding.bottom}");
		}

		// LayoutElement
		LayoutElement le = go.GetComponent<LayoutElement>();
		if (le != null)
		{
			sb.AppendLine($"{prefix}  LayoutElement:");
			sb.AppendLine($"{prefix}    minWidth: {le.minWidth}, minHeight: {le.minHeight}");
			sb.AppendLine($"{prefix}    preferredWidth: {le.preferredWidth}, preferredHeight: {le.preferredHeight}");
			sb.AppendLine($"{prefix}    flexibleWidth: {le.flexibleWidth}, flexibleHeight: {le.flexibleHeight}");
		}

		// ContentSizeFitter
		ContentSizeFitter csf = go.GetComponent<ContentSizeFitter>();
		if (csf != null)
		{
			sb.AppendLine($"{prefix}  ContentSizeFitter:");
			sb.AppendLine($"{prefix}    horizontalFit: {csf.horizontalFit}");
			sb.AppendLine($"{prefix}    verticalFit: {csf.verticalFit}");
		}

		// Button
		Button btn = go.GetComponent<Button>();
		if (btn != null)
		{
			sb.AppendLine($"{prefix}  Button:");
			sb.AppendLine($"{prefix}    interactable: {btn.interactable}");
			sb.AppendLine($"{prefix}    transition: {btn.transition}");
			if (btn.transition == Selectable.Transition.ColorTint)
			{
				ColorBlock colors = btn.colors;
				sb.AppendLine($"{prefix}    normalColor: RGBA({colors.normalColor.r:F3}, {colors.normalColor.g:F3}, {colors.normalColor.b:F3}, {colors.normalColor.a:F3})");
				sb.AppendLine($"{prefix}    highlightedColor: RGBA({colors.highlightedColor.r:F3}, {colors.highlightedColor.g:F3}, {colors.highlightedColor.b:F3}, {colors.highlightedColor.a:F3})");
				sb.AppendLine($"{prefix}    pressedColor: RGBA({colors.pressedColor.r:F3}, {colors.pressedColor.g:F3}, {colors.pressedColor.b:F3}, {colors.pressedColor.a:F3})");
			}
		}

		// CanvasGroup
		CanvasGroup cg = go.GetComponent<CanvasGroup>();
		if (cg != null)
		{
			sb.AppendLine($"{prefix}  CanvasGroup:");
			sb.AppendLine($"{prefix}    alpha: {cg.alpha}");
			sb.AppendLine($"{prefix}    interactable: {cg.interactable}");
			sb.AppendLine($"{prefix}    blocksRaycasts: {cg.blocksRaycasts}");
		}

		// List all other components
		Component[] components = go.GetComponents<Component>();
		List<string> otherComponents = new List<string>();
		foreach (Component comp in components)
		{
			if (comp == null) continue;
			string typeName = comp.GetType().Name;
			// Skip already logged components
			if (typeName != "RectTransform" && typeName != "Image" && typeName != "RawImage" &&
			    typeName != "TextMeshProUGUI" && typeName != "Text" && typeName != "VerticalLayoutGroup" &&
			    typeName != "HorizontalLayoutGroup" && typeName != "GridLayoutGroup" && typeName != "LayoutElement" &&
			    typeName != "ContentSizeFitter" && typeName != "Button" && typeName != "CanvasGroup" &&
			    typeName != "CanvasRenderer" && typeName != "Canvas")
			{
				otherComponents.Add(typeName);
			}
		}
		if (otherComponents.Count > 0)
		{
			sb.AppendLine($"{prefix}  OtherComponents: [{string.Join(", ", otherComponents)}]");
		}

		// Recurse children (limit depth to prevent huge outputs)
		if (indent < 10)
		{
			foreach (Transform child in go.transform)
			{
				CaptureUIHierarchy(child.gameObject, sb, indent + 1);
			}
		}
		else if (go.transform.childCount > 0)
		{
			sb.AppendLine($"{prefix}  ... ({go.transform.childCount} children, max depth reached)");
		}
	}

	private void OnDestroy()
	{
		Harmony harmony = _harmony;
		if (harmony != null)
		{
			harmony.UnpatchSelf();
		}
	}
}

// Marker component to identify custom props from asset bundles
public class CustomPropMarker : MonoBehaviour
{
}

// ============================================================================
// FIRST PICKUP INFO POPUP SYSTEM
// ============================================================================

/// <summary>
/// Configuration for the first pickup info popup system.
/// Loaded from Config/ItemInfoConfig.json at plugin startup.
/// </summary>
[Serializable]
public class ItemInfoConfig
{
	public string version;
	public List<ItemInfoEntry> items = new List<ItemInfoEntry>();
}

/// <summary>
/// Entry for a single item's popup information.
/// </summary>
[Serializable]
public class ItemInfoEntry
{
	public string itemId;
	public string title;
	public string description;
	public string imagePath;
	public bool showOnFirstPickup;
}

/// <summary>
/// Helper MonoBehaviour to run coroutines from static methods.
/// </summary>
public class CoroutineRunner : MonoBehaviour
{
	private static CoroutineRunner _instance;

	public static CoroutineRunner Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject obj = new GameObject("EndstarPopupCoroutineRunner");
				_instance = obj.AddComponent<CoroutineRunner>();
				Object.DontDestroyOnLoad(obj);
			}
			return _instance;
		}
	}
}
