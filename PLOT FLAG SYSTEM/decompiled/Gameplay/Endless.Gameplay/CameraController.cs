using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Data;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Mobile;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.EndlessQualitySettings;
using Endless.Shared.UI;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace Endless.Gameplay;

public class CameraController : EndlessBehaviourSingleton<CameraController>, IGameEndSubscriber
{
	public enum CameraState
	{
		Normal,
		TransitionToFullADS,
		FullADS,
		TransitionToNormal,
		SoftADS,
		TransitionToSoftADS
	}

	public enum CameraType : byte
	{
		Normal,
		FullADS,
		SoftADS
	}

	public enum CameraInstanceType
	{
		Gameplay,
		Menu
	}

	[Serializable]
	public struct CameraSettings
	{
		[SerializeField]
		private float yawDragSpeed;

		[SerializeField]
		private float pitchDragSpeed;

		[SerializeField]
		private float pitchMin;

		[SerializeField]
		private float pitchMax;

		[SerializeField]
		private float zoomMin;

		[SerializeField]
		private float zoomMax;

		[SerializeField]
		private float zoomSpeed;

		public float YawDragSpeed => yawDragSpeed;

		public float PitchDragSpeed => pitchDragSpeed;

		public float PitchMin => pitchMin;

		public float PitchMax => pitchMax;

		public float ZoomMin => zoomMin;

		public float ZoomMax => zoomMax;

		public float ZoomSpeed => zoomSpeed;
	}

	private const float FAR_SHOT_DISTANCE = 30f;

	private const int INTERCEPTION_AIM_CORRECTION_INCREMENTS = 5;

	private const float YAW_SMOOTHING_TIME = 0.1f;

	private const int OFF_PRIORITY = -1;

	private const int ON_PRIORITY = 1001;

	[SerializeField]
	private Transform freeCamFollowTransform;

	[SerializeField]
	private float followTransformOffset = 0.3f;

	[SerializeField]
	private float followOffsetLerpSpeed = 8f;

	[SerializeField]
	private float headroom = 0.25f;

	[NonSerialized]
	private float initialFollowTransformLocalY;

	[NonSerialized]
	private float followOffsetPercent;

	[SerializeField]
	private CameraSettings creatorCameraSettings;

	[SerializeField]
	private CameraSettings inGameCameraSettings;

	[SerializeField]
	private CinemachineBlenderSettings blenderSettings;

	[Header("ADS")]
	[SerializeField]
	private CinemachineCamera adsCam;

	[SerializeField]
	private CinemachineCamera adsSoftCam;

	[SerializeField]
	private Transform adsLookAtTransform;

	[SerializeField]
	private Transform adsFollowTransform;

	[SerializeField]
	private Transform softADSFollowTransform;

	[Tooltip("If enabled, camera climb will start to reset on its own. I left this disabled because that's not standard behavior and can feel weird if the player starts moving the aim")]
	[SerializeField]
	private bool autoResetClimb;

	[Header("ADS Transition")]
	[SerializeField]
	private float adsTransitionDuration = 0.3f;

	[SerializeField]
	private AnimationCurve adsTransitionCurve;

	[Header("ADS Aim Rays")]
	[SerializeField]
	private LayerMask adsRayHitLayer;

	[SerializeField]
	private LayerMask adsRayWallsLayer;

	[SerializeField]
	private LayerMask adsCloseTargetRayHitLayer;

	[SerializeField]
	private float adsCloseTargetRayDistance = 2.5f;

	[Header("Cinemachine Collider")]
	[SerializeField]
	private CinemachineDeoccluder mainPlayerCameraDeoccluder;

	[SerializeField]
	private CinemachineDeoccluder adsCamDeoccluder;

	[SerializeField]
	private CinemachineDeoccluder softAdsCamDeoccluder;

	[SerializeField]
	private LayerMask cameraCollisionLayer;

	[SerializeField]
	private LayerMask ghostModeCollisionLayers;

	[SerializeField]
	private float minDisplaceDistanceForSoftAdsOverride = 0.3f;

	[Header("Mobile")]
	[SerializeField]
	private PinchInputHandler pinchInputHandler;

	private bool cameraDeccluderEnabled;

	[HideInInspector]
	public CrosshairUI CrosshairObject;

	[HideInInspector]
	public CameraType GameStateADS;

	private bool adsActive;

	private AppearanceController appearanceController;

	public static string StateInfo;

	private CameraSettings activeCameraSettings;

	private PlayerInputActions playerInputActions;

	private float freeYaw;

	private float freePitch;

	private float zoomAmount;

	private float basePitch;

	private float preCutsceneYaw;

	private float preCutscenePitch;

	private float preCutsceneZoom;

	private int currentFixedCam;

	private bool isMobile;

	private bool updateYawAndPitch;

	private float yawInput;

	private float pitchInput;

	private float scrollInput;

	private bool inGame;

	private List<CinemachineCamera> runtimeCameraList;

	public float runtimeTransitionTimer;

	public CameraState runtimeCameraState;

	private float camRotVelocity;

	private Vector3 lastAimPosition;

	[NonSerialized]
	private int aimUpdateFrame = -1;

	[NonSerialized]
	private float currentClimbAngle;

	[NonSerialized]
	private float lastShotTime;

	[NonSerialized]
	private float climbSettleAmount;

	[NonSerialized]
	private float climbSettleDelay;

	private readonly HashSet<string> playerCamHashSet = new HashSet<string>();

	[SerializeField]
	private string mainMenuBackgroundCameraName;

	[SerializeField]
	private string cutsceneCamerasName;

	private CinemachineBlendDefinition blend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 0f);

	private Coroutine transitionCameraHolderCoroutine;

	private bool cameraLockedForCutscene;

	private CutsceneCamera currentCutsceneCutsceneCamera;

	[field: SerializeField]
	public CinemachineCamera MainPlayerCamera { get; private set; }

	public float AimPitch => freePitch;

	public float AimYaw => freeYaw;

	public bool AimUsingADS => adsActive;

	public static float Rotation { get; protected set; }

	public Vector3 LastAimPosition => lastAimPosition;

	public Vector3 CurrentAimPosition
	{
		get
		{
			UpdateAimPosition();
			return lastAimPosition;
		}
	}

	[field: SerializeField]
	public Camera GameplayCamera { get; private set; }

	[field: SerializeField]
	public CinemachineBrain GameplayCameraBrain { get; private set; }

	[field: SerializeField]
	public Camera MenuCamera { get; private set; }

	[field: SerializeField]
	public CinemachineBrain MenuCameraBrain { get; private set; }

	public bool CharacterCameraActive => playerCamHashSet.Contains(ActiveCameraName);

	public string ActiveCameraName { get; private set; }

	public ICinemachineCamera ActiveCamera { get; private set; }

	public bool IsInCutsceneTransition
	{
		get
		{
			if (GameplayCameraBrain.ActiveBlend == null || GameplayCameraBrain.ActiveBlend.CamA == null || GameplayCameraBrain.ActiveBlend.CamB == null)
			{
				return false;
			}
			return GameplayCameraBrain.ActiveBlend.CamA.Name == cutsceneCamerasName != (GameplayCameraBrain.ActiveBlend.CamB.Name == cutsceneCamerasName);
		}
	}

	public float CameraForwardRotation
	{
		get
		{
			Vector3 vector = GameplayCamera.transform.forward;
			if (vector == Vector3.down || vector == Vector3.up)
			{
				vector = GameplayCamera.transform.up;
			}
			return Vector3.SignedAngle(Vector3.forward, new Vector3(vector.x, 0f, vector.z).normalized, Vector3.up);
		}
	}

	private InputAction CameraPanInputAction => playerInputActions.Player.CameraPanWindows;

	public bool IsAPlayerCamera(string cameraName)
	{
		return playerCamHashSet.Contains(cameraName);
	}

	protected override void Awake()
	{
		base.Awake();
		playerInputActions = new PlayerInputActions();
		ActiveCamera = MainPlayerCamera;
		ActiveCameraName = MainPlayerCamera.name;
		runtimeCameraList = new List<CinemachineCamera> { MainPlayerCamera, adsCam, adsSoftCam };
		foreach (CinemachineCamera runtimeCamera in runtimeCameraList)
		{
			playerCamHashSet.Add(runtimeCamera.name);
		}
		isMobile = MobileUtility.IsMobile;
		if (!isMobile)
		{
			playerInputActions.Player.Look.AddBinding("<Pointer>/delta");
		}
		PostProcessQuality.PostProcessQualityLevelChanged.AddListener(HandlePostProcessingQualityChanged);
		Endless.Shared.EndlessQualitySettings.AntialiasingQuality.AntialiasingQualityChanged.AddListener(HandleAntialiasingQualityChanged);
		HandlePostProcessingQualityChanged(PostProcessQuality.CurrentQualityLevel);
		HandleAntialiasingQualityChanged(Endless.Shared.EndlessQualitySettings.AntialiasingQuality.CurrentAntialiasingMode, Endless.Shared.EndlessQualitySettings.AntialiasingQuality.CurrentAntialiasingQuality);
		initialFollowTransformLocalY = freeCamFollowTransform.localPosition.y;
	}

	private void HandlePostProcessingQualityChanged(PostProcessQuality.PostProcessQualityLevel quality)
	{
		bool renderPostProcessing = quality > PostProcessQuality.PostProcessQualityLevel.Disabled;
		GameplayCamera.GetUniversalAdditionalCameraData().renderPostProcessing = renderPostProcessing;
		MenuCamera.GetUniversalAdditionalCameraData().renderPostProcessing = renderPostProcessing;
	}

	private void HandleAntialiasingQualityChanged(AntialiasingMode mode, UnityEngine.Rendering.Universal.AntialiasingQuality quality)
	{
		UniversalAdditionalCameraData universalAdditionalCameraData = GameplayCamera.GetUniversalAdditionalCameraData();
		universalAdditionalCameraData.antialiasing = mode;
		universalAdditionalCameraData.antialiasingQuality = quality;
		UniversalAdditionalCameraData universalAdditionalCameraData2 = MenuCamera.GetUniversalAdditionalCameraData();
		universalAdditionalCameraData2.antialiasing = mode;
		universalAdditionalCameraData2.antialiasingQuality = quality;
	}

	private new void Start()
	{
		base.Start();
		MainPlayerCamera.transform.SetParent(null);
		adsCam.transform.SetParent(null);
		adsSoftCam.transform.SetParent(null);
		ModifyZoom(0f);
		UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemOpen, new Action(DisableFreeCam));
		UIScreenManager.OnScreenSystemClose = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemClose, new Action(EnableFreeCam));
		CinemachineCore.GetBlendOverride = GetBlendOverrideDelegate;
		if (isMobile)
		{
			pinchInputHandler.OnPinchUnityEvent.AddListener(OnPinchZoom);
		}
	}

	private void OnEnable()
	{
		playerInputActions.Player.Enable();
	}

	private void OnDisable()
	{
		playerInputActions.Player.Disable();
	}

	public void SetMainCamera(CameraInstanceType cameraType)
	{
		if (cameraType == CameraInstanceType.Gameplay)
		{
			GameplayCamera.enabled = true;
			MenuCameraBrain.OutputCamera.enabled = false;
		}
		else
		{
			GameplayCamera.enabled = false;
			MenuCameraBrain.OutputCamera.enabled = true;
		}
	}

	public void ToggleInput(bool state)
	{
		if (state && !playerInputActions.Player.enabled)
		{
			playerInputActions.Player.Enable();
		}
		else if (!state && playerInputActions.Player.enabled)
		{
			playerInputActions.Player.Disable();
		}
	}

	private void UpdateCameraTransition()
	{
		runtimeTransitionTimer = Mathf.Max(0f, runtimeTransitionTimer - Time.deltaTime);
		CameraType cameraType = GameStateADS;
		if (GameStateADS == CameraType.FullADS && IsAdsCamDisplaced())
		{
			cameraType = CameraType.SoftADS;
		}
		switch (runtimeCameraState)
		{
		case CameraState.Normal:
			if (cameraType != CameraType.Normal)
			{
				StartCameraTransition(cameraType, adsTransitionDuration);
			}
			break;
		case CameraState.SoftADS:
			if (cameraType != CameraType.SoftADS)
			{
				StartCameraTransition(cameraType, adsTransitionDuration);
			}
			break;
		case CameraState.FullADS:
			if (cameraType != CameraType.FullADS)
			{
				StartCameraTransition(cameraType, adsTransitionDuration);
			}
			break;
		case CameraState.TransitionToNormal:
			if (cameraType != CameraType.Normal)
			{
				StartCameraTransition(cameraType, adsTransitionDuration);
			}
			else if (runtimeTransitionTimer == 0f)
			{
				runtimeCameraState = CameraState.Normal;
				adsActive = false;
			}
			break;
		case CameraState.TransitionToSoftADS:
			if (cameraType != CameraType.SoftADS)
			{
				StartCameraTransition(cameraType, adsTransitionDuration);
			}
			else if (runtimeTransitionTimer == 0f)
			{
				runtimeCameraState = CameraState.SoftADS;
				adsActive = true;
			}
			break;
		case CameraState.TransitionToFullADS:
			if (cameraType != CameraType.FullADS)
			{
				StartCameraTransition(cameraType, adsTransitionDuration);
			}
			else if (runtimeTransitionTimer == 0f)
			{
				runtimeCameraState = CameraState.FullADS;
				adsActive = true;
			}
			break;
		default:
			Debug.LogWarning("Camera state not handled by transition manager");
			break;
		}
	}

	private void StartCameraTransition(CameraType toCam, float transitionDuration)
	{
		runtimeTransitionTimer = transitionDuration;
		if (!cameraLockedForCutscene)
		{
			for (int i = 0; i < runtimeCameraList.Count; i++)
			{
				runtimeCameraList[i].Priority.Value = ((i == (int)toCam) ? 1000 : i);
			}
		}
		switch (toCam)
		{
		case CameraType.Normal:
			runtimeCameraState = CameraState.TransitionToNormal;
			break;
		case CameraType.FullADS:
			runtimeCameraState = CameraState.TransitionToFullADS;
			break;
		case CameraType.SoftADS:
			runtimeCameraState = CameraState.TransitionToSoftADS;
			break;
		}
		if (toCam != CameraType.Normal && CharacterCameraActive)
		{
			CrosshairObject?.Show();
		}
		else
		{
			CrosshairObject?.Hide();
		}
	}

	private IEnumerator HoldForTransition(float duration)
	{
		if (duration > 0f)
		{
			yield return new WaitForSeconds(duration);
		}
		else
		{
			while (GameplayCameraBrain.IsBlending)
			{
				yield return null;
			}
		}
		cameraLockedForCutscene = false;
		freeYaw = Rotation;
		StartCameraTransition(GameStateADS, adsTransitionDuration);
	}

	private CinemachineBlendDefinition GetBlendOverrideDelegate(ICinemachineCamera fromVcam, ICinemachineCamera toVcam, CinemachineBlendDefinition defaultBlend, UnityEngine.Object owner)
	{
		if ((CinemachineBrain)owner != GameplayCameraBrain)
		{
			return defaultBlend;
		}
		ActiveCameraName = toVcam.Name;
		ActiveCamera = toVcam;
		if (fromVcam.Name == mainMenuBackgroundCameraName || toVcam.Name == mainMenuBackgroundCameraName)
		{
			return new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
		}
		bool flag = IsAPlayerCamera(fromVcam.Name);
		bool flag2 = IsAPlayerCamera(toVcam.Name);
		if (flag && !flag2)
		{
			if (transitionCameraHolderCoroutine != null)
			{
				StopCoroutine(transitionCameraHolderCoroutine);
				transitionCameraHolderCoroutine = null;
			}
			cameraLockedForCutscene = true;
		}
		else if (!flag && flag2)
		{
			float duration = defaultBlend.BlendTime;
			if (GameplayCameraBrain.IsBlending)
			{
				duration = ((GameplayCameraBrain.ActiveBlend.CamA != toVcam) ? (-1f) : GameplayCameraBrain.ActiveBlend.TimeInBlend);
			}
			transitionCameraHolderCoroutine = StartCoroutine(HoldForTransition(duration));
		}
		if (!flag || !flag2)
		{
			return defaultBlend;
		}
		blend.Time = runtimeTransitionTimer;
		blend.CustomCurve = adsTransitionCurve;
		return blend;
	}

	private void SetCameraDeoccluderEnabled(bool value)
	{
		if (value != cameraDeccluderEnabled)
		{
			cameraDeccluderEnabled = value;
			mainPlayerCameraDeoccluder.CollideAgainst = (cameraDeccluderEnabled ? cameraCollisionLayer : ghostModeCollisionLayers);
			adsCamDeoccluder.CollideAgainst = mainPlayerCameraDeoccluder.CollideAgainst;
			softAdsCamDeoccluder.CollideAgainst = mainPlayerCameraDeoccluder.CollideAgainst;
		}
	}

	public void EndlessGameEnd()
	{
		ActiveCamera = MainPlayerCamera;
		ActiveCameraName = MainPlayerCamera.name;
		ExitCutscene(CameraTransition.Cut, 0f);
		cameraLockedForCutscene = false;
		if (transitionCameraHolderCoroutine != null)
		{
			StopCoroutine(transitionCameraHolderCoroutine);
			transitionCameraHolderCoroutine = null;
		}
	}

	public void TransitionToCutsceneCamera(CutsceneCamera nextCutsceneCutsceneCamera, CameraTransition transitionType, float transitionDuration, CutsceneCamera.TargetInfo followInfo, CutsceneCamera.TargetInfo lookAtInfo)
	{
		if (transitionType != CameraTransition.Fade)
		{
			SetCutsceneBlend(transitionType, transitionDuration);
			if ((bool)currentCutsceneCutsceneCamera)
			{
				currentCutsceneCutsceneCamera.CinemachineCamera.Priority.Value = -1;
				currentCutsceneCutsceneCamera.DisabledLocally();
			}
			nextCutsceneCutsceneCamera.CinemachineCamera.Priority.Value = 1001;
			nextCutsceneCutsceneCamera.EnabledLocally(followInfo, lookAtInfo);
			currentCutsceneCutsceneCamera = nextCutsceneCutsceneCamera;
		}
		else
		{
			NetworkBehaviourSingleton<CameraFadeManager>.Instance.FadeOutIn(transitionDuration, delegate
			{
				TransitionToCutsceneCamera(nextCutsceneCutsceneCamera, CameraTransition.Cut, 0f, followInfo, lookAtInfo);
			});
		}
	}

	public void ExitCutscene(CameraTransition transitionType, float transitionDuration)
	{
		if (!currentCutsceneCutsceneCamera)
		{
			return;
		}
		if (transitionType != CameraTransition.Fade)
		{
			SetCutsceneBlend(transitionType, transitionDuration);
			currentCutsceneCutsceneCamera.CinemachineCamera.Priority.Value = -1;
			currentCutsceneCutsceneCamera.DisabledLocally();
			currentCutsceneCutsceneCamera = null;
			return;
		}
		NetworkBehaviourSingleton<CameraFadeManager>.Instance.FadeOutIn(transitionDuration, delegate
		{
			SetCutsceneBlend(CameraTransition.Cut, transitionDuration);
			currentCutsceneCutsceneCamera.CinemachineCamera.Priority.Value = -1;
			currentCutsceneCutsceneCamera.DisabledLocally();
			currentCutsceneCutsceneCamera = null;
		});
	}

	private void SetCutsceneBlend(CameraTransition transitionType, float transitionDuration)
	{
		CinemachineBlendDefinition.Styles blendStyle = GetBlendStyle(transitionType);
		blenderSettings.CustomBlends[0].Blend.Style = blendStyle;
		blenderSettings.CustomBlends[0].Blend.Time = transitionDuration;
		blenderSettings.CustomBlends[1].Blend.Style = blendStyle;
		blenderSettings.CustomBlends[1].Blend.Time = transitionDuration;
	}

	private CinemachineBlendDefinition.Styles GetBlendStyle(CameraTransition transition)
	{
		return transition switch
		{
			CameraTransition.Ease => CinemachineBlendDefinition.Styles.EaseInOut, 
			CameraTransition.Linear => CinemachineBlendDefinition.Styles.Linear, 
			_ => CinemachineBlendDefinition.Styles.Cut, 
		};
	}

	public void HandleGameplayStarted()
	{
		inGame = true;
		ForceResetToMainCamera();
	}

	public void HandleGameplayStopped()
	{
		inGame = false;
		ForceResetToMainCamera();
	}

	public void ForceResetToMainCamera()
	{
		StartCameraTransition(CameraType.Normal, 0.1f);
		CrosshairObject?.Hide();
	}

	public void InitAppearance(AppearanceController initAppearance, PlayerNetworkController.ControllerType controllerType)
	{
		appearanceController = initAppearance;
		if (controllerType == PlayerNetworkController.ControllerType.Creator)
		{
			activeCameraSettings = creatorCameraSettings;
		}
		else
		{
			activeCameraSettings = inGameCameraSettings;
		}
		ResetCamera();
	}

	public void ResetCamera(float yaw = 0f)
	{
		freeYaw = yaw;
		freePitch = 0f;
		basePitch = 0f;
		zoomAmount = (activeCameraSettings.ZoomMax + activeCameraSettings.ZoomMin) / 2f;
		currentClimbAngle = 0f;
		ModifyZoom(0f);
		EnableFreeCam();
	}

	private void Update()
	{
		UpdateCameraTransition();
		if (autoResetClimb)
		{
			climbSettleDelay -= Time.deltaTime;
			if (climbSettleDelay <= 0f)
			{
				currentClimbAngle = Mathf.MoveTowards(currentClimbAngle, 0f, climbSettleAmount * Time.deltaTime);
			}
		}
	}

	private void LateUpdate()
	{
		UpdateCameraFollowOffset();
		if (cameraLockedForCutscene)
		{
			if (!(appearanceController != null))
			{
				return;
			}
			base.transform.position = appearanceController.transform.position;
			if (isMobile)
			{
				updateYawAndPitch = playerInputActions.Player.Move.IsPressed();
			}
			else if (inGame)
			{
				updateYawAndPitch = !playerInputActions.Player.EnableCursor.IsPressed();
			}
			else
			{
				updateYawAndPitch = CameraPanInputAction.IsPressed();
			}
			if (updateYawAndPitch)
			{
				if (GameStateADS == CameraType.Normal)
				{
					freeYaw = appearanceController.CurrentCharRotation;
				}
				else
				{
					yawInput = playerInputActions.Player.Look.ReadValue<Vector2>().x;
					freeYaw += activeCameraSettings.YawDragSpeed * yawInput;
				}
			}
			freePitch = Mathf.Clamp(basePitch - currentClimbAngle, activeCameraSettings.PitchMin, activeCameraSettings.PitchMax);
			UpdateAimPosition();
			return;
		}
		if (appearanceController != null)
		{
			SetCameraDeoccluderEnabled(!appearanceController.GhostModeActive);
			base.transform.position = appearanceController.transform.position;
			Vector2 vector = playerInputActions.Player.Look.ReadValue<Vector2>() * Time.timeScale;
			yawInput = vector.x;
			pitchInput = vector.y;
			scrollInput = playerInputActions.Player.Zoom.ReadValue<float>() * Time.timeScale;
			if (isMobile)
			{
				bool isPinching = pinchInputHandler.IsPinching;
				updateYawAndPitch = playerInputActions.Player.Move.IsPressed() || !isPinching;
			}
			else
			{
				updateYawAndPitch = inGame || CameraPanInputAction.IsPressed();
				if (updateYawAndPitch && playerInputActions.Player.EnableCursor.IsPressed())
				{
					updateYawAndPitch = false;
				}
			}
			if (updateYawAndPitch)
			{
				freeYaw += activeCameraSettings.YawDragSpeed * yawInput;
				basePitch = Mathf.Clamp(basePitch - activeCameraSettings.PitchDragSpeed * pitchInput, activeCameraSettings.PitchMin, activeCameraSettings.PitchMax);
			}
			freePitch = Mathf.Clamp(basePitch - currentClimbAngle, activeCameraSettings.PitchMin, activeCameraSettings.PitchMax);
			Rotation = Mathf.Repeat(freeYaw, 360f);
			UpdateVirtualCameras(freePitch, freeYaw);
			if (!isMobile && scrollInput != 0f && !EventSystem.current.IsPointerOverGameObject())
			{
				ModifyZoom(scrollInput * activeCameraSettings.ZoomSpeed);
			}
		}
		UpdateAimPosition();
	}

	private void UpdateCameraFollowOffset()
	{
		if (PlayerReferenceManager.LocalInstance != null)
		{
			Vector3 localPosition = freeCamFollowTransform.localPosition;
			NetState currentState = PlayerReferenceManager.LocalInstance.PlayerController.CurrentState;
			float num = 0.2f;
			float cameraRadius = mainPlayerCameraDeoccluder.AvoidObstacles.CameraRadius;
			Vector3 origin = base.transform.position + Vector3.up * (initialFollowTransformLocalY - num);
			float num2 = cameraRadius + num + headroom;
			if (!currentState.Ghost && Physics.SphereCast(new Ray(origin, Vector3.up), cameraRadius, out var hitInfo, num2, cameraCollisionLayer))
			{
				float num3 = 2f * (1f - hitInfo.distance / num2);
				followOffsetPercent = Mathf.MoveTowards(followOffsetPercent, 1f, Time.deltaTime * (followOffsetLerpSpeed + num3));
				localPosition.y = Mathf.Lerp(initialFollowTransformLocalY, initialFollowTransformLocalY - followTransformOffset, adsTransitionCurve.Evaluate(followOffsetPercent));
				freeCamFollowTransform.localPosition = localPosition;
			}
			else
			{
				followOffsetPercent = Mathf.MoveTowards(followOffsetPercent, 0f, Time.deltaTime * followOffsetLerpSpeed);
				localPosition.y = Mathf.Lerp(initialFollowTransformLocalY, initialFollowTransformLocalY - followTransformOffset, adsTransitionCurve.Evaluate(followOffsetPercent));
				freeCamFollowTransform.localPosition = localPosition;
			}
		}
	}

	public Vector2 GetAimFixADS(Vector3 shotStartPosition, float projectileRadius = 0.15f)
	{
		Ray ray = GameplayCamera.ScreenPointToRay(new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 30f));
		ray.origin = ray.GetPoint(4f);
		Debug.DrawLine(ray.origin, ray.origin + ray.direction * 30f);
		Vector3 point = ray.GetPoint(30f);
		if (Physics.Raycast(ray, out var hitInfo, 30f, adsRayHitLayer) && hitInfo.distance > adsCloseTargetRayDistance)
		{
			point = hitInfo.point;
		}
		Vector3 eulerAngles = Quaternion.LookRotation(point - shotStartPosition).eulerAngles;
		float x = Mathf.DeltaAngle(freePitch, eulerAngles.x);
		float y = Mathf.DeltaAngle(freeYaw, eulerAngles.y);
		return new Vector2(x, y);
	}

	public void SetYawInput(float input)
	{
		yawInput = input;
	}

	public void SetPitchInput(float input)
	{
		pitchInput = input;
	}

	public void SetScrollInput(float input)
	{
		scrollInput = input;
	}

	private void OnPinchZoom(float delta)
	{
		if (!EventSystem.current.IsPointerOverGameObject())
		{
			ModifyZoom(delta * activeCameraSettings.ZoomSpeed);
		}
	}

	private void UpdateVirtualCameras(float pitch, float yaw)
	{
		base.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
		Quaternion localRotation = Quaternion.Euler(pitch, 0f, 0f);
		freeCamFollowTransform.localRotation = localRotation;
		adsFollowTransform.localRotation = localRotation;
		softADSFollowTransform.localRotation = localRotation;
		adsLookAtTransform.position = base.transform.position + new Vector3(0f, freeCamFollowTransform.localPosition.y, 0f) + Quaternion.Euler(pitch, yaw, 0f) * (Vector3.forward * 30f);
	}

	private void EnableFreeCam()
	{
		GameplayCamera.enabled = true;
		MenuCameraBrain.OutputCamera.enabled = false;
	}

	private void UpdateAimPosition()
	{
		if (!(PlayerReferenceManager.LocalInstance == null) && aimUpdateFrame != Time.frameCount)
		{
			aimUpdateFrame = Time.frameCount;
			Ray ray;
			Vector3 vector2;
			if (cameraLockedForCutscene)
			{
				Vector3 vector = Quaternion.Euler(Mathf.Clamp(0f - currentClimbAngle, activeCameraSettings.PitchMin, activeCameraSettings.PitchMax), appearanceController.CurrentCharRotation, 0f) * Vector3.forward;
				ray = new Ray(PlayerReferenceManager.LocalInstance.PlayerController.transform.position + Vector3.up + vector, vector);
				vector2 = ray.GetPoint(30f);
			}
			else
			{
				ray = GameplayCamera.ScreenPointToRay(new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 4f));
				vector2 = ray.GetPoint(30f);
				Vector3 vector3 = Vector3.Project(PlayerReferenceManager.LocalInstance.PlayerController.transform.position + Vector3.up + ray.direction - ray.origin, ray.direction);
				ray.origin += vector3;
			}
			if (Physics.Raycast(ray, out var hitInfo, 30f, adsRayHitLayer))
			{
				vector2 = ((!(hitInfo.distance > 0.5f)) ? (ray.origin + ray.direction * 0.5f) : hitInfo.point);
			}
			lastAimPosition = vector2;
		}
	}

	private void DisableFreeCam()
	{
		GameplayCamera.enabled = false;
		MenuCameraBrain.OutputCamera.enabled = true;
	}

	public void ModifyZoom(float delta)
	{
		zoomAmount = Mathf.Clamp(zoomAmount - delta, activeCameraSettings.ZoomMin, activeCameraSettings.ZoomMax);
		CinemachineComponentBase cinemachineComponent = MainPlayerCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
		if (cinemachineComponent is CinemachineThirdPersonFollow)
		{
			(cinemachineComponent as CinemachineThirdPersonFollow).CameraDistance = zoomAmount;
		}
	}

	public void OnShotFired(float climbAmount, float maxClimb, float climbSettle, float settleDelayTime = 0.05f)
	{
		climbSettleAmount = climbSettle;
		climbSettleDelay = settleDelayTime;
		lastShotTime = Time.time;
		if (autoResetClimb)
		{
			currentClimbAngle = Mathf.Min(currentClimbAngle + climbAmount, maxClimb);
		}
		else
		{
			basePitch = Mathf.Clamp(basePitch - climbAmount, activeCameraSettings.PitchMin, activeCameraSettings.PitchMax);
		}
	}

	private bool IsAdsCamDisplaced()
	{
		return adsCamDeoccluder.GetCameraDisplacementDistance(adsCam) > minDisplaceDistanceForSoftAdsOverride;
	}
}
