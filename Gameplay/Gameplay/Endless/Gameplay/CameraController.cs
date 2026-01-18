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
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace Endless.Gameplay
{
	// Token: 0x02000070 RID: 112
	public class CameraController : EndlessBehaviourSingleton<CameraController>, IGameEndSubscriber
	{
		// Token: 0x1700004F RID: 79
		// (get) Token: 0x060001CC RID: 460 RVA: 0x0000AC8A File Offset: 0x00008E8A
		// (set) Token: 0x060001CD RID: 461 RVA: 0x0000AC92 File Offset: 0x00008E92
		public CinemachineCamera MainPlayerCamera { get; private set; }

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x060001CE RID: 462 RVA: 0x0000AC9B File Offset: 0x00008E9B
		public float AimPitch
		{
			get
			{
				return this.freePitch;
			}
		}

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x060001CF RID: 463 RVA: 0x0000ACA3 File Offset: 0x00008EA3
		public float AimYaw
		{
			get
			{
				return this.freeYaw;
			}
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x060001D0 RID: 464 RVA: 0x0000ACAB File Offset: 0x00008EAB
		public bool AimUsingADS
		{
			get
			{
				return this.adsActive;
			}
		}

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x060001D1 RID: 465 RVA: 0x0000ACB3 File Offset: 0x00008EB3
		// (set) Token: 0x060001D2 RID: 466 RVA: 0x0000ACBA File Offset: 0x00008EBA
		public static float Rotation { get; protected set; }

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x060001D3 RID: 467 RVA: 0x0000ACC2 File Offset: 0x00008EC2
		public Vector3 LastAimPosition
		{
			get
			{
				return this.lastAimPosition;
			}
		}

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x060001D4 RID: 468 RVA: 0x0000ACCA File Offset: 0x00008ECA
		public Vector3 CurrentAimPosition
		{
			get
			{
				this.UpdateAimPosition();
				return this.lastAimPosition;
			}
		}

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x060001D5 RID: 469 RVA: 0x0000ACD8 File Offset: 0x00008ED8
		// (set) Token: 0x060001D6 RID: 470 RVA: 0x0000ACE0 File Offset: 0x00008EE0
		public Camera GameplayCamera { get; private set; }

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x060001D7 RID: 471 RVA: 0x0000ACE9 File Offset: 0x00008EE9
		// (set) Token: 0x060001D8 RID: 472 RVA: 0x0000ACF1 File Offset: 0x00008EF1
		public CinemachineBrain GameplayCameraBrain { get; private set; }

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x060001D9 RID: 473 RVA: 0x0000ACFA File Offset: 0x00008EFA
		// (set) Token: 0x060001DA RID: 474 RVA: 0x0000AD02 File Offset: 0x00008F02
		public Camera MenuCamera { get; private set; }

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x060001DB RID: 475 RVA: 0x0000AD0B File Offset: 0x00008F0B
		// (set) Token: 0x060001DC RID: 476 RVA: 0x0000AD13 File Offset: 0x00008F13
		public CinemachineBrain MenuCameraBrain { get; private set; }

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x060001DD RID: 477 RVA: 0x0000AD1C File Offset: 0x00008F1C
		public bool CharacterCameraActive
		{
			get
			{
				return this.playerCamHashSet.Contains(this.ActiveCameraName);
			}
		}

		// Token: 0x060001DE RID: 478 RVA: 0x0000AD2F File Offset: 0x00008F2F
		public bool IsAPlayerCamera(string cameraName)
		{
			return this.playerCamHashSet.Contains(cameraName);
		}

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x060001DF RID: 479 RVA: 0x0000AD3D File Offset: 0x00008F3D
		// (set) Token: 0x060001E0 RID: 480 RVA: 0x0000AD45 File Offset: 0x00008F45
		public string ActiveCameraName { get; private set; }

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x060001E1 RID: 481 RVA: 0x0000AD4E File Offset: 0x00008F4E
		// (set) Token: 0x060001E2 RID: 482 RVA: 0x0000AD56 File Offset: 0x00008F56
		public ICinemachineCamera ActiveCamera { get; private set; }

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x060001E3 RID: 483 RVA: 0x0000AD60 File Offset: 0x00008F60
		public bool IsInCutsceneTransition
		{
			get
			{
				return this.GameplayCameraBrain.ActiveBlend != null && this.GameplayCameraBrain.ActiveBlend.CamA != null && this.GameplayCameraBrain.ActiveBlend.CamB != null && this.GameplayCameraBrain.ActiveBlend.CamA.Name == this.cutsceneCamerasName != (this.GameplayCameraBrain.ActiveBlend.CamB.Name == this.cutsceneCamerasName);
			}
		}

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x060001E4 RID: 484 RVA: 0x0000ADE8 File Offset: 0x00008FE8
		public float CameraForwardRotation
		{
			get
			{
				Vector3 vector = this.GameplayCamera.transform.forward;
				if (vector == Vector3.down || vector == Vector3.up)
				{
					vector = this.GameplayCamera.transform.up;
				}
				return Vector3.SignedAngle(Vector3.forward, new Vector3(vector.x, 0f, vector.z).normalized, Vector3.up);
			}
		}

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x060001E5 RID: 485 RVA: 0x0000AE60 File Offset: 0x00009060
		private InputAction CameraPanInputAction
		{
			get
			{
				return this.playerInputActions.Player.CameraPanWindows;
			}
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x0000AE80 File Offset: 0x00009080
		protected override void Awake()
		{
			base.Awake();
			this.playerInputActions = new PlayerInputActions();
			this.ActiveCamera = this.MainPlayerCamera;
			this.ActiveCameraName = this.MainPlayerCamera.name;
			this.runtimeCameraList = new List<CinemachineCamera> { this.MainPlayerCamera, this.adsCam, this.adsSoftCam };
			foreach (CinemachineCamera cinemachineCamera in this.runtimeCameraList)
			{
				this.playerCamHashSet.Add(cinemachineCamera.name);
			}
			this.isMobile = MobileUtility.IsMobile;
			if (!this.isMobile)
			{
				this.playerInputActions.Player.Look.AddBinding("<Pointer>/delta", null, null, null);
			}
			PostProcessQuality.PostProcessQualityLevelChanged.AddListener(new UnityAction<PostProcessQuality.PostProcessQualityLevel>(this.HandlePostProcessingQualityChanged));
			Endless.Shared.EndlessQualitySettings.AntialiasingQuality.AntialiasingQualityChanged.AddListener(new UnityAction<AntialiasingMode, global::UnityEngine.Rendering.Universal.AntialiasingQuality>(this.HandleAntialiasingQualityChanged));
			this.HandlePostProcessingQualityChanged(PostProcessQuality.CurrentQualityLevel);
			this.HandleAntialiasingQualityChanged(Endless.Shared.EndlessQualitySettings.AntialiasingQuality.CurrentAntialiasingMode, Endless.Shared.EndlessQualitySettings.AntialiasingQuality.CurrentAntialiasingQuality);
			this.initialFollowTransformLocalY = this.freeCamFollowTransform.localPosition.y;
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x0000AFCC File Offset: 0x000091CC
		private void HandlePostProcessingQualityChanged(PostProcessQuality.PostProcessQualityLevel quality)
		{
			bool flag = quality > PostProcessQuality.PostProcessQualityLevel.Disabled;
			this.GameplayCamera.GetUniversalAdditionalCameraData().renderPostProcessing = flag;
			this.MenuCamera.GetUniversalAdditionalCameraData().renderPostProcessing = flag;
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x0000B000 File Offset: 0x00009200
		private void HandleAntialiasingQualityChanged(AntialiasingMode mode, global::UnityEngine.Rendering.Universal.AntialiasingQuality quality)
		{
			UniversalAdditionalCameraData universalAdditionalCameraData = this.GameplayCamera.GetUniversalAdditionalCameraData();
			universalAdditionalCameraData.antialiasing = mode;
			universalAdditionalCameraData.antialiasingQuality = quality;
			UniversalAdditionalCameraData universalAdditionalCameraData2 = this.MenuCamera.GetUniversalAdditionalCameraData();
			universalAdditionalCameraData2.antialiasing = mode;
			universalAdditionalCameraData2.antialiasingQuality = quality;
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x0000B034 File Offset: 0x00009234
		private new void Start()
		{
			base.Start();
			this.MainPlayerCamera.transform.SetParent(null);
			this.adsCam.transform.SetParent(null);
			this.adsSoftCam.transform.SetParent(null);
			this.ModifyZoom(0f);
			UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemOpen, new Action(this.DisableFreeCam));
			UIScreenManager.OnScreenSystemClose = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemClose, new Action(this.EnableFreeCam));
			CinemachineCore.GetBlendOverride = new CinemachineCore.GetBlendOverrideDelegate(this.GetBlendOverrideDelegate);
			if (this.isMobile)
			{
				this.pinchInputHandler.OnPinchUnityEvent.AddListener(new UnityAction<float>(this.OnPinchZoom));
			}
		}

		// Token: 0x060001EA RID: 490 RVA: 0x0000B0FC File Offset: 0x000092FC
		private void OnEnable()
		{
			this.playerInputActions.Player.Enable();
		}

		// Token: 0x060001EB RID: 491 RVA: 0x0000B11C File Offset: 0x0000931C
		private void OnDisable()
		{
			this.playerInputActions.Player.Disable();
		}

		// Token: 0x060001EC RID: 492 RVA: 0x0000B13C File Offset: 0x0000933C
		public void SetMainCamera(CameraController.CameraInstanceType cameraType)
		{
			if (cameraType == CameraController.CameraInstanceType.Gameplay)
			{
				this.GameplayCamera.enabled = true;
				this.MenuCameraBrain.OutputCamera.enabled = false;
				return;
			}
			this.GameplayCamera.enabled = false;
			this.MenuCameraBrain.OutputCamera.enabled = true;
		}

		// Token: 0x060001ED RID: 493 RVA: 0x0000B17C File Offset: 0x0000937C
		public void ToggleInput(bool state)
		{
			if (state && !this.playerInputActions.Player.enabled)
			{
				this.playerInputActions.Player.Enable();
				return;
			}
			if (!state && this.playerInputActions.Player.enabled)
			{
				this.playerInputActions.Player.Disable();
			}
		}

		// Token: 0x060001EE RID: 494 RVA: 0x0000B1E0 File Offset: 0x000093E0
		private void UpdateCameraTransition()
		{
			this.runtimeTransitionTimer = Mathf.Max(0f, this.runtimeTransitionTimer - Time.deltaTime);
			CameraController.CameraType cameraType = this.GameStateADS;
			if (this.GameStateADS == CameraController.CameraType.FullADS && this.IsAdsCamDisplaced())
			{
				cameraType = CameraController.CameraType.SoftADS;
			}
			switch (this.runtimeCameraState)
			{
			case CameraController.CameraState.Normal:
				if (cameraType != CameraController.CameraType.Normal)
				{
					this.StartCameraTransition(cameraType, this.adsTransitionDuration);
					return;
				}
				break;
			case CameraController.CameraState.TransitionToFullADS:
				if (cameraType != CameraController.CameraType.FullADS)
				{
					this.StartCameraTransition(cameraType, this.adsTransitionDuration);
					return;
				}
				if (this.runtimeTransitionTimer == 0f)
				{
					this.runtimeCameraState = CameraController.CameraState.FullADS;
					this.adsActive = true;
					return;
				}
				break;
			case CameraController.CameraState.FullADS:
				if (cameraType != CameraController.CameraType.FullADS)
				{
					this.StartCameraTransition(cameraType, this.adsTransitionDuration);
					return;
				}
				break;
			case CameraController.CameraState.TransitionToNormal:
				if (cameraType != CameraController.CameraType.Normal)
				{
					this.StartCameraTransition(cameraType, this.adsTransitionDuration);
					return;
				}
				if (this.runtimeTransitionTimer == 0f)
				{
					this.runtimeCameraState = CameraController.CameraState.Normal;
					this.adsActive = false;
					return;
				}
				break;
			case CameraController.CameraState.SoftADS:
				if (cameraType != CameraController.CameraType.SoftADS)
				{
					this.StartCameraTransition(cameraType, this.adsTransitionDuration);
					return;
				}
				break;
			case CameraController.CameraState.TransitionToSoftADS:
				if (cameraType != CameraController.CameraType.SoftADS)
				{
					this.StartCameraTransition(cameraType, this.adsTransitionDuration);
					return;
				}
				if (this.runtimeTransitionTimer == 0f)
				{
					this.runtimeCameraState = CameraController.CameraState.SoftADS;
					this.adsActive = true;
					return;
				}
				break;
			default:
				Debug.LogWarning("Camera state not handled by transition manager");
				break;
			}
		}

		// Token: 0x060001EF RID: 495 RVA: 0x0000B320 File Offset: 0x00009520
		private void StartCameraTransition(CameraController.CameraType toCam, float transitionDuration)
		{
			this.runtimeTransitionTimer = transitionDuration;
			if (!this.cameraLockedForCutscene)
			{
				for (int i = 0; i < this.runtimeCameraList.Count; i++)
				{
					this.runtimeCameraList[i].Priority.Value = ((i == (int)toCam) ? 1000 : i);
				}
			}
			if (toCam == CameraController.CameraType.Normal)
			{
				this.runtimeCameraState = CameraController.CameraState.TransitionToNormal;
			}
			else if (toCam == CameraController.CameraType.FullADS)
			{
				this.runtimeCameraState = CameraController.CameraState.TransitionToFullADS;
			}
			else if (toCam == CameraController.CameraType.SoftADS)
			{
				this.runtimeCameraState = CameraController.CameraState.TransitionToSoftADS;
			}
			if (toCam != CameraController.CameraType.Normal && this.CharacterCameraActive)
			{
				CrosshairUI crosshairObject = this.CrosshairObject;
				if (crosshairObject == null)
				{
					return;
				}
				crosshairObject.Show();
				return;
			}
			else
			{
				CrosshairUI crosshairObject2 = this.CrosshairObject;
				if (crosshairObject2 == null)
				{
					return;
				}
				crosshairObject2.Hide();
				return;
			}
		}

		// Token: 0x060001F0 RID: 496 RVA: 0x0000B3C6 File Offset: 0x000095C6
		private IEnumerator HoldForTransition(float duration)
		{
			if (duration > 0f)
			{
				yield return new WaitForSeconds(duration);
			}
			else
			{
				while (this.GameplayCameraBrain.IsBlending)
				{
					yield return null;
				}
			}
			this.cameraLockedForCutscene = false;
			this.freeYaw = CameraController.Rotation;
			this.StartCameraTransition(this.GameStateADS, this.adsTransitionDuration);
			yield break;
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x0000B3DC File Offset: 0x000095DC
		private CinemachineBlendDefinition GetBlendOverrideDelegate(ICinemachineCamera fromVcam, ICinemachineCamera toVcam, CinemachineBlendDefinition defaultBlend, global::UnityEngine.Object owner)
		{
			if ((CinemachineBrain)owner != this.GameplayCameraBrain)
			{
				return defaultBlend;
			}
			this.ActiveCameraName = toVcam.Name;
			this.ActiveCamera = toVcam;
			if (fromVcam.Name == this.mainMenuBackgroundCameraName || toVcam.Name == this.mainMenuBackgroundCameraName)
			{
				return new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
			}
			bool flag = this.IsAPlayerCamera(fromVcam.Name);
			bool flag2 = this.IsAPlayerCamera(toVcam.Name);
			if (flag && !flag2)
			{
				if (this.transitionCameraHolderCoroutine != null)
				{
					base.StopCoroutine(this.transitionCameraHolderCoroutine);
					this.transitionCameraHolderCoroutine = null;
				}
				this.cameraLockedForCutscene = true;
			}
			else if (!flag && flag2)
			{
				float num = defaultBlend.BlendTime;
				if (this.GameplayCameraBrain.IsBlending)
				{
					if (this.GameplayCameraBrain.ActiveBlend.CamA == toVcam)
					{
						num = this.GameplayCameraBrain.ActiveBlend.TimeInBlend;
					}
					else
					{
						num = -1f;
					}
				}
				this.transitionCameraHolderCoroutine = base.StartCoroutine(this.HoldForTransition(num));
			}
			if (!flag || !flag2)
			{
				return defaultBlend;
			}
			this.blend.Time = this.runtimeTransitionTimer;
			this.blend.CustomCurve = this.adsTransitionCurve;
			return this.blend;
		}

		// Token: 0x060001F2 RID: 498 RVA: 0x0000B514 File Offset: 0x00009714
		private void SetCameraDeoccluderEnabled(bool value)
		{
			if (value != this.cameraDeccluderEnabled)
			{
				this.cameraDeccluderEnabled = value;
				this.mainPlayerCameraDeoccluder.CollideAgainst = (this.cameraDeccluderEnabled ? this.cameraCollisionLayer : this.ghostModeCollisionLayers);
				this.adsCamDeoccluder.CollideAgainst = this.mainPlayerCameraDeoccluder.CollideAgainst;
				this.softAdsCamDeoccluder.CollideAgainst = this.mainPlayerCameraDeoccluder.CollideAgainst;
			}
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x0000B580 File Offset: 0x00009780
		public void EndlessGameEnd()
		{
			this.ActiveCamera = this.MainPlayerCamera;
			this.ActiveCameraName = this.MainPlayerCamera.name;
			this.ExitCutscene(CameraTransition.Cut, 0f);
			this.cameraLockedForCutscene = false;
			if (this.transitionCameraHolderCoroutine != null)
			{
				base.StopCoroutine(this.transitionCameraHolderCoroutine);
				this.transitionCameraHolderCoroutine = null;
			}
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x0000B5D8 File Offset: 0x000097D8
		public void TransitionToCutsceneCamera(CutsceneCamera nextCutsceneCutsceneCamera, CameraTransition transitionType, float transitionDuration, CutsceneCamera.TargetInfo followInfo, CutsceneCamera.TargetInfo lookAtInfo)
		{
			if (transitionType != CameraTransition.Fade)
			{
				this.SetCutsceneBlend(transitionType, transitionDuration);
				if (this.currentCutsceneCutsceneCamera)
				{
					this.currentCutsceneCutsceneCamera.CinemachineCamera.Priority.Value = -1;
					this.currentCutsceneCutsceneCamera.DisabledLocally();
				}
				nextCutsceneCutsceneCamera.CinemachineCamera.Priority.Value = 1001;
				nextCutsceneCutsceneCamera.EnabledLocally(followInfo, lookAtInfo);
				this.currentCutsceneCutsceneCamera = nextCutsceneCutsceneCamera;
				return;
			}
			NetworkBehaviourSingleton<CameraFadeManager>.Instance.FadeOutIn(transitionDuration, delegate
			{
				this.TransitionToCutsceneCamera(nextCutsceneCutsceneCamera, CameraTransition.Cut, 0f, followInfo, lookAtInfo);
			}, null);
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x0000B69C File Offset: 0x0000989C
		public void ExitCutscene(CameraTransition transitionType, float transitionDuration)
		{
			if (this.currentCutsceneCutsceneCamera)
			{
				if (transitionType != CameraTransition.Fade)
				{
					this.SetCutsceneBlend(transitionType, transitionDuration);
					this.currentCutsceneCutsceneCamera.CinemachineCamera.Priority.Value = -1;
					this.currentCutsceneCutsceneCamera.DisabledLocally();
					this.currentCutsceneCutsceneCamera = null;
					return;
				}
				NetworkBehaviourSingleton<CameraFadeManager>.Instance.FadeOutIn(transitionDuration, delegate
				{
					this.SetCutsceneBlend(CameraTransition.Cut, transitionDuration);
					this.currentCutsceneCutsceneCamera.CinemachineCamera.Priority.Value = -1;
					this.currentCutsceneCutsceneCamera.DisabledLocally();
					this.currentCutsceneCutsceneCamera = null;
				}, null);
			}
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x0000B724 File Offset: 0x00009924
		private void SetCutsceneBlend(CameraTransition transitionType, float transitionDuration)
		{
			CinemachineBlendDefinition.Styles blendStyle = this.GetBlendStyle(transitionType);
			this.blenderSettings.CustomBlends[0].Blend.Style = blendStyle;
			this.blenderSettings.CustomBlends[0].Blend.Time = transitionDuration;
			this.blenderSettings.CustomBlends[1].Blend.Style = blendStyle;
			this.blenderSettings.CustomBlends[1].Blend.Time = transitionDuration;
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x0000B7AC File Offset: 0x000099AC
		private CinemachineBlendDefinition.Styles GetBlendStyle(CameraTransition transition)
		{
			CinemachineBlendDefinition.Styles styles;
			if (transition != CameraTransition.Ease)
			{
				if (transition != CameraTransition.Linear)
				{
					styles = CinemachineBlendDefinition.Styles.Cut;
				}
				else
				{
					styles = CinemachineBlendDefinition.Styles.Linear;
				}
			}
			else
			{
				styles = CinemachineBlendDefinition.Styles.EaseInOut;
			}
			return styles;
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x0000B7CD File Offset: 0x000099CD
		public void HandleGameplayStarted()
		{
			this.inGame = true;
			this.ForceResetToMainCamera();
		}

		// Token: 0x060001F9 RID: 505 RVA: 0x0000B7DC File Offset: 0x000099DC
		public void HandleGameplayStopped()
		{
			this.inGame = false;
			this.ForceResetToMainCamera();
		}

		// Token: 0x060001FA RID: 506 RVA: 0x0000B7EB File Offset: 0x000099EB
		public void ForceResetToMainCamera()
		{
			this.StartCameraTransition(CameraController.CameraType.Normal, 0.1f);
			CrosshairUI crosshairObject = this.CrosshairObject;
			if (crosshairObject == null)
			{
				return;
			}
			crosshairObject.Hide();
		}

		// Token: 0x060001FB RID: 507 RVA: 0x0000B809 File Offset: 0x00009A09
		public void InitAppearance(AppearanceController initAppearance, PlayerNetworkController.ControllerType controllerType)
		{
			this.appearanceController = initAppearance;
			if (controllerType == PlayerNetworkController.ControllerType.Creator)
			{
				this.activeCameraSettings = this.creatorCameraSettings;
			}
			else
			{
				this.activeCameraSettings = this.inGameCameraSettings;
			}
			this.ResetCamera(0f);
		}

		// Token: 0x060001FC RID: 508 RVA: 0x0000B83C File Offset: 0x00009A3C
		public void ResetCamera(float yaw = 0f)
		{
			this.freeYaw = yaw;
			this.freePitch = 0f;
			this.basePitch = 0f;
			this.zoomAmount = (this.activeCameraSettings.ZoomMax + this.activeCameraSettings.ZoomMin) / 2f;
			this.currentClimbAngle = 0f;
			this.ModifyZoom(0f);
			this.EnableFreeCam();
		}

		// Token: 0x060001FD RID: 509 RVA: 0x0000B8A8 File Offset: 0x00009AA8
		private void Update()
		{
			this.UpdateCameraTransition();
			if (this.autoResetClimb)
			{
				this.climbSettleDelay -= Time.deltaTime;
				if (this.climbSettleDelay <= 0f)
				{
					this.currentClimbAngle = Mathf.MoveTowards(this.currentClimbAngle, 0f, this.climbSettleAmount * Time.deltaTime);
				}
			}
		}

		// Token: 0x060001FE RID: 510 RVA: 0x0000B904 File Offset: 0x00009B04
		private void LateUpdate()
		{
			this.UpdateCameraFollowOffset();
			if (this.cameraLockedForCutscene)
			{
				if (this.appearanceController != null)
				{
					base.transform.position = this.appearanceController.transform.position;
					if (this.isMobile)
					{
						this.updateYawAndPitch = this.playerInputActions.Player.Move.IsPressed();
					}
					else if (this.inGame)
					{
						this.updateYawAndPitch = !this.playerInputActions.Player.EnableCursor.IsPressed();
					}
					else
					{
						this.updateYawAndPitch = this.CameraPanInputAction.IsPressed();
					}
					if (this.updateYawAndPitch)
					{
						if (this.GameStateADS == CameraController.CameraType.Normal)
						{
							this.freeYaw = this.appearanceController.CurrentCharRotation;
						}
						else
						{
							Vector2 vector = this.playerInputActions.Player.Look.ReadValue<Vector2>();
							this.yawInput = vector.x;
							this.freeYaw += this.activeCameraSettings.YawDragSpeed * this.yawInput;
						}
					}
					this.freePitch = Mathf.Clamp(this.basePitch - this.currentClimbAngle, this.activeCameraSettings.PitchMin, this.activeCameraSettings.PitchMax);
					this.UpdateAimPosition();
				}
				return;
			}
			if (this.appearanceController != null)
			{
				this.SetCameraDeoccluderEnabled(!this.appearanceController.GhostModeActive);
				base.transform.position = this.appearanceController.transform.position;
				Vector2 vector2 = this.playerInputActions.Player.Look.ReadValue<Vector2>() * Time.timeScale;
				this.yawInput = vector2.x;
				this.pitchInput = vector2.y;
				this.scrollInput = this.playerInputActions.Player.Zoom.ReadValue<float>() * Time.timeScale;
				if (this.isMobile)
				{
					bool isPinching = this.pinchInputHandler.IsPinching;
					this.updateYawAndPitch = this.playerInputActions.Player.Move.IsPressed() || !isPinching;
				}
				else
				{
					this.updateYawAndPitch = this.inGame || this.CameraPanInputAction.IsPressed();
					if (this.updateYawAndPitch && this.playerInputActions.Player.EnableCursor.IsPressed())
					{
						this.updateYawAndPitch = false;
					}
				}
				if (this.updateYawAndPitch)
				{
					this.freeYaw += this.activeCameraSettings.YawDragSpeed * this.yawInput;
					this.basePitch = Mathf.Clamp(this.basePitch - this.activeCameraSettings.PitchDragSpeed * this.pitchInput, this.activeCameraSettings.PitchMin, this.activeCameraSettings.PitchMax);
				}
				this.freePitch = Mathf.Clamp(this.basePitch - this.currentClimbAngle, this.activeCameraSettings.PitchMin, this.activeCameraSettings.PitchMax);
				CameraController.Rotation = Mathf.Repeat(this.freeYaw, 360f);
				this.UpdateVirtualCameras(this.freePitch, this.freeYaw);
				if (!this.isMobile && this.scrollInput != 0f && !EventSystem.current.IsPointerOverGameObject())
				{
					this.ModifyZoom(this.scrollInput * this.activeCameraSettings.ZoomSpeed);
				}
			}
			this.UpdateAimPosition();
		}

		// Token: 0x060001FF RID: 511 RVA: 0x0000BC64 File Offset: 0x00009E64
		private void UpdateCameraFollowOffset()
		{
			if (PlayerReferenceManager.LocalInstance != null)
			{
				Vector3 localPosition = this.freeCamFollowTransform.localPosition;
				ref NetState currentState = PlayerReferenceManager.LocalInstance.PlayerController.CurrentState;
				float num = 0.2f;
				float cameraRadius = this.mainPlayerCameraDeoccluder.AvoidObstacles.CameraRadius;
				Vector3 vector = base.transform.position + Vector3.up * (this.initialFollowTransformLocalY - num);
				float num2 = cameraRadius + num + this.headroom;
				RaycastHit raycastHit;
				if (!currentState.Ghost && Physics.SphereCast(new Ray(vector, Vector3.up), cameraRadius, out raycastHit, num2, this.cameraCollisionLayer))
				{
					float num3 = 2f * (1f - raycastHit.distance / num2);
					this.followOffsetPercent = Mathf.MoveTowards(this.followOffsetPercent, 1f, Time.deltaTime * (this.followOffsetLerpSpeed + num3));
					localPosition.y = Mathf.Lerp(this.initialFollowTransformLocalY, this.initialFollowTransformLocalY - this.followTransformOffset, this.adsTransitionCurve.Evaluate(this.followOffsetPercent));
					this.freeCamFollowTransform.localPosition = localPosition;
					return;
				}
				this.followOffsetPercent = Mathf.MoveTowards(this.followOffsetPercent, 0f, Time.deltaTime * this.followOffsetLerpSpeed);
				localPosition.y = Mathf.Lerp(this.initialFollowTransformLocalY, this.initialFollowTransformLocalY - this.followTransformOffset, this.adsTransitionCurve.Evaluate(this.followOffsetPercent));
				this.freeCamFollowTransform.localPosition = localPosition;
			}
		}

		// Token: 0x06000200 RID: 512 RVA: 0x0000BDE8 File Offset: 0x00009FE8
		public Vector2 GetAimFixADS(Vector3 shotStartPosition, float projectileRadius = 0.15f)
		{
			Ray ray = this.GameplayCamera.ScreenPointToRay(new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 30f));
			ray.origin = ray.GetPoint(4f);
			Debug.DrawLine(ray.origin, ray.origin + ray.direction * 30f);
			Vector3 vector = ray.GetPoint(30f);
			RaycastHit raycastHit;
			if (Physics.Raycast(ray, out raycastHit, 30f, this.adsRayHitLayer) && raycastHit.distance > this.adsCloseTargetRayDistance)
			{
				vector = raycastHit.point;
			}
			Vector3 eulerAngles = Quaternion.LookRotation(vector - shotStartPosition).eulerAngles;
			float num = Mathf.DeltaAngle(this.freePitch, eulerAngles.x);
			float num2 = Mathf.DeltaAngle(this.freeYaw, eulerAngles.y);
			return new Vector2(num, num2);
		}

		// Token: 0x06000201 RID: 513 RVA: 0x0000BEDE File Offset: 0x0000A0DE
		public void SetYawInput(float input)
		{
			this.yawInput = input;
		}

		// Token: 0x06000202 RID: 514 RVA: 0x0000BEE7 File Offset: 0x0000A0E7
		public void SetPitchInput(float input)
		{
			this.pitchInput = input;
		}

		// Token: 0x06000203 RID: 515 RVA: 0x0000BEF0 File Offset: 0x0000A0F0
		public void SetScrollInput(float input)
		{
			this.scrollInput = input;
		}

		// Token: 0x06000204 RID: 516 RVA: 0x0000BEF9 File Offset: 0x0000A0F9
		private void OnPinchZoom(float delta)
		{
			if (!EventSystem.current.IsPointerOverGameObject())
			{
				this.ModifyZoom(delta * this.activeCameraSettings.ZoomSpeed);
			}
		}

		// Token: 0x06000205 RID: 517 RVA: 0x0000BF1C File Offset: 0x0000A11C
		private void UpdateVirtualCameras(float pitch, float yaw)
		{
			base.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
			Quaternion quaternion = Quaternion.Euler(pitch, 0f, 0f);
			this.freeCamFollowTransform.localRotation = quaternion;
			this.adsFollowTransform.localRotation = quaternion;
			this.softADSFollowTransform.localRotation = quaternion;
			this.adsLookAtTransform.position = base.transform.position + new Vector3(0f, this.freeCamFollowTransform.localPosition.y, 0f) + Quaternion.Euler(pitch, yaw, 0f) * (Vector3.forward * 30f);
		}

		// Token: 0x06000206 RID: 518 RVA: 0x0000BFD8 File Offset: 0x0000A1D8
		private void EnableFreeCam()
		{
			this.GameplayCamera.enabled = true;
			this.MenuCameraBrain.OutputCamera.enabled = false;
		}

		// Token: 0x06000207 RID: 519 RVA: 0x0000BFF8 File Offset: 0x0000A1F8
		private void UpdateAimPosition()
		{
			if (PlayerReferenceManager.LocalInstance == null)
			{
				return;
			}
			if (this.aimUpdateFrame == Time.frameCount)
			{
				return;
			}
			this.aimUpdateFrame = Time.frameCount;
			Ray ray;
			Vector3 vector2;
			if (this.cameraLockedForCutscene)
			{
				Vector3 vector = Quaternion.Euler(Mathf.Clamp(-this.currentClimbAngle, this.activeCameraSettings.PitchMin, this.activeCameraSettings.PitchMax), this.appearanceController.CurrentCharRotation, 0f) * Vector3.forward;
				ray = new Ray(PlayerReferenceManager.LocalInstance.PlayerController.transform.position + Vector3.up + vector, vector);
				vector2 = ray.GetPoint(30f);
			}
			else
			{
				ray = this.GameplayCamera.ScreenPointToRay(new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 4f));
				vector2 = ray.GetPoint(30f);
				Vector3 vector3 = Vector3.Project(PlayerReferenceManager.LocalInstance.PlayerController.transform.position + Vector3.up + ray.direction - ray.origin, ray.direction);
				ray.origin += vector3;
			}
			RaycastHit raycastHit;
			if (Physics.Raycast(ray, out raycastHit, 30f, this.adsRayHitLayer))
			{
				if (raycastHit.distance > 0.5f)
				{
					vector2 = raycastHit.point;
				}
				else
				{
					vector2 = ray.origin + ray.direction * 0.5f;
				}
			}
			this.lastAimPosition = vector2;
		}

		// Token: 0x06000208 RID: 520 RVA: 0x0000C1A1 File Offset: 0x0000A3A1
		private void DisableFreeCam()
		{
			this.GameplayCamera.enabled = false;
			this.MenuCameraBrain.OutputCamera.enabled = true;
		}

		// Token: 0x06000209 RID: 521 RVA: 0x0000C1C0 File Offset: 0x0000A3C0
		public void ModifyZoom(float delta)
		{
			this.zoomAmount = Mathf.Clamp(this.zoomAmount - delta, this.activeCameraSettings.ZoomMin, this.activeCameraSettings.ZoomMax);
			CinemachineComponentBase cinemachineComponent = this.MainPlayerCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
			if (cinemachineComponent is CinemachineThirdPersonFollow)
			{
				(cinemachineComponent as CinemachineThirdPersonFollow).CameraDistance = this.zoomAmount;
			}
		}

		// Token: 0x0600020A RID: 522 RVA: 0x0000C21C File Offset: 0x0000A41C
		public void OnShotFired(float climbAmount, float maxClimb, float climbSettle, float settleDelayTime = 0.05f)
		{
			this.climbSettleAmount = climbSettle;
			this.climbSettleDelay = settleDelayTime;
			this.lastShotTime = Time.time;
			if (this.autoResetClimb)
			{
				this.currentClimbAngle = Mathf.Min(this.currentClimbAngle + climbAmount, maxClimb);
				return;
			}
			this.basePitch = Mathf.Clamp(this.basePitch - climbAmount, this.activeCameraSettings.PitchMin, this.activeCameraSettings.PitchMax);
		}

		// Token: 0x0600020B RID: 523 RVA: 0x0000C289 File Offset: 0x0000A489
		private bool IsAdsCamDisplaced()
		{
			return this.adsCamDeoccluder.GetCameraDisplacementDistance(this.adsCam) > this.minDisplaceDistanceForSoftAdsOverride;
		}

		// Token: 0x040001AA RID: 426
		private const float FAR_SHOT_DISTANCE = 30f;

		// Token: 0x040001AB RID: 427
		private const int INTERCEPTION_AIM_CORRECTION_INCREMENTS = 5;

		// Token: 0x040001AC RID: 428
		private const float YAW_SMOOTHING_TIME = 0.1f;

		// Token: 0x040001AD RID: 429
		private const int OFF_PRIORITY = -1;

		// Token: 0x040001AE RID: 430
		private const int ON_PRIORITY = 1001;

		// Token: 0x040001B0 RID: 432
		[SerializeField]
		private Transform freeCamFollowTransform;

		// Token: 0x040001B1 RID: 433
		[SerializeField]
		private float followTransformOffset = 0.3f;

		// Token: 0x040001B2 RID: 434
		[SerializeField]
		private float followOffsetLerpSpeed = 8f;

		// Token: 0x040001B3 RID: 435
		[SerializeField]
		private float headroom = 0.25f;

		// Token: 0x040001B4 RID: 436
		[NonSerialized]
		private float initialFollowTransformLocalY;

		// Token: 0x040001B5 RID: 437
		[NonSerialized]
		private float followOffsetPercent;

		// Token: 0x040001B6 RID: 438
		[SerializeField]
		private CameraController.CameraSettings creatorCameraSettings;

		// Token: 0x040001B7 RID: 439
		[SerializeField]
		private CameraController.CameraSettings inGameCameraSettings;

		// Token: 0x040001B8 RID: 440
		[SerializeField]
		private CinemachineBlenderSettings blenderSettings;

		// Token: 0x040001B9 RID: 441
		[Header("ADS")]
		[SerializeField]
		private CinemachineCamera adsCam;

		// Token: 0x040001BA RID: 442
		[SerializeField]
		private CinemachineCamera adsSoftCam;

		// Token: 0x040001BB RID: 443
		[SerializeField]
		private Transform adsLookAtTransform;

		// Token: 0x040001BC RID: 444
		[SerializeField]
		private Transform adsFollowTransform;

		// Token: 0x040001BD RID: 445
		[SerializeField]
		private Transform softADSFollowTransform;

		// Token: 0x040001BE RID: 446
		[Tooltip("If enabled, camera climb will start to reset on its own. I left this disabled because that's not standard behavior and can feel weird if the player starts moving the aim")]
		[SerializeField]
		private bool autoResetClimb;

		// Token: 0x040001BF RID: 447
		[Header("ADS Transition")]
		[SerializeField]
		private float adsTransitionDuration = 0.3f;

		// Token: 0x040001C0 RID: 448
		[SerializeField]
		private AnimationCurve adsTransitionCurve;

		// Token: 0x040001C1 RID: 449
		[Header("ADS Aim Rays")]
		[SerializeField]
		private LayerMask adsRayHitLayer;

		// Token: 0x040001C2 RID: 450
		[SerializeField]
		private LayerMask adsRayWallsLayer;

		// Token: 0x040001C3 RID: 451
		[SerializeField]
		private LayerMask adsCloseTargetRayHitLayer;

		// Token: 0x040001C4 RID: 452
		[SerializeField]
		private float adsCloseTargetRayDistance = 2.5f;

		// Token: 0x040001C5 RID: 453
		[Header("Cinemachine Collider")]
		[SerializeField]
		private CinemachineDeoccluder mainPlayerCameraDeoccluder;

		// Token: 0x040001C6 RID: 454
		[SerializeField]
		private CinemachineDeoccluder adsCamDeoccluder;

		// Token: 0x040001C7 RID: 455
		[SerializeField]
		private CinemachineDeoccluder softAdsCamDeoccluder;

		// Token: 0x040001C8 RID: 456
		[SerializeField]
		private LayerMask cameraCollisionLayer;

		// Token: 0x040001C9 RID: 457
		[SerializeField]
		private LayerMask ghostModeCollisionLayers;

		// Token: 0x040001CA RID: 458
		[SerializeField]
		private float minDisplaceDistanceForSoftAdsOverride = 0.3f;

		// Token: 0x040001CB RID: 459
		[Header("Mobile")]
		[SerializeField]
		private PinchInputHandler pinchInputHandler;

		// Token: 0x040001CC RID: 460
		private bool cameraDeccluderEnabled;

		// Token: 0x040001CD RID: 461
		[HideInInspector]
		public CrosshairUI CrosshairObject;

		// Token: 0x040001CE RID: 462
		[HideInInspector]
		public CameraController.CameraType GameStateADS;

		// Token: 0x040001CF RID: 463
		private bool adsActive;

		// Token: 0x040001D0 RID: 464
		private AppearanceController appearanceController;

		// Token: 0x040001D2 RID: 466
		public static string StateInfo;

		// Token: 0x040001D3 RID: 467
		private CameraController.CameraSettings activeCameraSettings;

		// Token: 0x040001D4 RID: 468
		private PlayerInputActions playerInputActions;

		// Token: 0x040001D5 RID: 469
		private float freeYaw;

		// Token: 0x040001D6 RID: 470
		private float freePitch;

		// Token: 0x040001D7 RID: 471
		private float zoomAmount;

		// Token: 0x040001D8 RID: 472
		private float basePitch;

		// Token: 0x040001D9 RID: 473
		private float preCutsceneYaw;

		// Token: 0x040001DA RID: 474
		private float preCutscenePitch;

		// Token: 0x040001DB RID: 475
		private float preCutsceneZoom;

		// Token: 0x040001DC RID: 476
		private int currentFixedCam;

		// Token: 0x040001DD RID: 477
		private bool isMobile;

		// Token: 0x040001DE RID: 478
		private bool updateYawAndPitch;

		// Token: 0x040001DF RID: 479
		private float yawInput;

		// Token: 0x040001E0 RID: 480
		private float pitchInput;

		// Token: 0x040001E1 RID: 481
		private float scrollInput;

		// Token: 0x040001E2 RID: 482
		private bool inGame;

		// Token: 0x040001E3 RID: 483
		private List<CinemachineCamera> runtimeCameraList;

		// Token: 0x040001E4 RID: 484
		public float runtimeTransitionTimer;

		// Token: 0x040001E5 RID: 485
		public CameraController.CameraState runtimeCameraState;

		// Token: 0x040001E6 RID: 486
		private float camRotVelocity;

		// Token: 0x040001E7 RID: 487
		private Vector3 lastAimPosition;

		// Token: 0x040001E8 RID: 488
		[NonSerialized]
		private int aimUpdateFrame = -1;

		// Token: 0x040001E9 RID: 489
		[NonSerialized]
		private float currentClimbAngle;

		// Token: 0x040001EA RID: 490
		[NonSerialized]
		private float lastShotTime;

		// Token: 0x040001EB RID: 491
		[NonSerialized]
		private float climbSettleAmount;

		// Token: 0x040001EC RID: 492
		[NonSerialized]
		private float climbSettleDelay;

		// Token: 0x040001ED RID: 493
		private readonly HashSet<string> playerCamHashSet = new HashSet<string>();

		// Token: 0x040001F2 RID: 498
		[SerializeField]
		private string mainMenuBackgroundCameraName;

		// Token: 0x040001F3 RID: 499
		[SerializeField]
		private string cutsceneCamerasName;

		// Token: 0x040001F4 RID: 500
		private CinemachineBlendDefinition blend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 0f);

		// Token: 0x040001F7 RID: 503
		private Coroutine transitionCameraHolderCoroutine;

		// Token: 0x040001F8 RID: 504
		private bool cameraLockedForCutscene;

		// Token: 0x040001F9 RID: 505
		private CutsceneCamera currentCutsceneCutsceneCamera;

		// Token: 0x02000071 RID: 113
		public enum CameraState
		{
			// Token: 0x040001FB RID: 507
			Normal,
			// Token: 0x040001FC RID: 508
			TransitionToFullADS,
			// Token: 0x040001FD RID: 509
			FullADS,
			// Token: 0x040001FE RID: 510
			TransitionToNormal,
			// Token: 0x040001FF RID: 511
			SoftADS,
			// Token: 0x04000200 RID: 512
			TransitionToSoftADS
		}

		// Token: 0x02000072 RID: 114
		public enum CameraType : byte
		{
			// Token: 0x04000202 RID: 514
			Normal,
			// Token: 0x04000203 RID: 515
			FullADS,
			// Token: 0x04000204 RID: 516
			SoftADS
		}

		// Token: 0x02000073 RID: 115
		public enum CameraInstanceType
		{
			// Token: 0x04000206 RID: 518
			Gameplay,
			// Token: 0x04000207 RID: 519
			Menu
		}

		// Token: 0x02000074 RID: 116
		[Serializable]
		public struct CameraSettings
		{
			// Token: 0x17000060 RID: 96
			// (get) Token: 0x0600020D RID: 525 RVA: 0x0000C31C File Offset: 0x0000A51C
			public float YawDragSpeed
			{
				get
				{
					return this.yawDragSpeed;
				}
			}

			// Token: 0x17000061 RID: 97
			// (get) Token: 0x0600020E RID: 526 RVA: 0x0000C324 File Offset: 0x0000A524
			public float PitchDragSpeed
			{
				get
				{
					return this.pitchDragSpeed;
				}
			}

			// Token: 0x17000062 RID: 98
			// (get) Token: 0x0600020F RID: 527 RVA: 0x0000C32C File Offset: 0x0000A52C
			public float PitchMin
			{
				get
				{
					return this.pitchMin;
				}
			}

			// Token: 0x17000063 RID: 99
			// (get) Token: 0x06000210 RID: 528 RVA: 0x0000C334 File Offset: 0x0000A534
			public float PitchMax
			{
				get
				{
					return this.pitchMax;
				}
			}

			// Token: 0x17000064 RID: 100
			// (get) Token: 0x06000211 RID: 529 RVA: 0x0000C33C File Offset: 0x0000A53C
			public float ZoomMin
			{
				get
				{
					return this.zoomMin;
				}
			}

			// Token: 0x17000065 RID: 101
			// (get) Token: 0x06000212 RID: 530 RVA: 0x0000C344 File Offset: 0x0000A544
			public float ZoomMax
			{
				get
				{
					return this.zoomMax;
				}
			}

			// Token: 0x17000066 RID: 102
			// (get) Token: 0x06000213 RID: 531 RVA: 0x0000C34C File Offset: 0x0000A54C
			public float ZoomSpeed
			{
				get
				{
					return this.zoomSpeed;
				}
			}

			// Token: 0x04000208 RID: 520
			[SerializeField]
			private float yawDragSpeed;

			// Token: 0x04000209 RID: 521
			[SerializeField]
			private float pitchDragSpeed;

			// Token: 0x0400020A RID: 522
			[SerializeField]
			private float pitchMin;

			// Token: 0x0400020B RID: 523
			[SerializeField]
			private float pitchMax;

			// Token: 0x0400020C RID: 524
			[SerializeField]
			private float zoomMin;

			// Token: 0x0400020D RID: 525
			[SerializeField]
			private float zoomMax;

			// Token: 0x0400020E RID: 526
			[SerializeField]
			private float zoomSpeed;
		}
	}
}
