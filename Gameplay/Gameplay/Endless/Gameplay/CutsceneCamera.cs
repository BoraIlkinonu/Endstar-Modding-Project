using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002FF RID: 767
	public class CutsceneCamera : EndlessNetworkBehaviour, IBaseType, IComponentBase, IScriptInjector
	{
		// Token: 0x17000370 RID: 880
		// (get) Token: 0x06001179 RID: 4473 RVA: 0x00057165 File Offset: 0x00055365
		public InputSettings InputAllowed
		{
			get
			{
				return this.InputAllowedDuringShot;
			}
		}

		// Token: 0x17000371 RID: 881
		// (get) Token: 0x0600117A RID: 4474 RVA: 0x0005716D File Offset: 0x0005536D
		public bool InvulnerablePlayer
		{
			get
			{
				return this.InvulnerablePlayerDuringShot;
			}
		}

		// Token: 0x17000372 RID: 882
		// (get) Token: 0x0600117B RID: 4475 RVA: 0x00057175 File Offset: 0x00055375
		public float BaseShotDuration
		{
			get
			{
				return this.baseShotDuration;
			}
		}

		// Token: 0x17000373 RID: 883
		// (get) Token: 0x0600117C RID: 4476 RVA: 0x0005717D File Offset: 0x0005537D
		public float SecondaryShotDuration
		{
			get
			{
				return this.secondaryShotDuration;
			}
		}

		// Token: 0x17000374 RID: 884
		// (get) Token: 0x0600117D RID: 4477 RVA: 0x00057185 File Offset: 0x00055385
		public float TotalShotDuration
		{
			get
			{
				return this.baseShotDuration + this.secondaryShotDuration;
			}
		}

		// Token: 0x17000375 RID: 885
		// (get) Token: 0x0600117E RID: 4478 RVA: 0x00057194 File Offset: 0x00055394
		public CutsceneCamera.TargetInfo CurrentFollowInfo_Server
		{
			get
			{
				return this.currentFollowInfo_Server;
			}
		}

		// Token: 0x17000376 RID: 886
		// (get) Token: 0x0600117F RID: 4479 RVA: 0x0005719C File Offset: 0x0005539C
		public CutsceneCamera.TargetInfo CurrentLookAtInfo_Server
		{
			get
			{
				return this.currentLookAtInfo_Server;
			}
		}

		// Token: 0x17000377 RID: 887
		// (get) Token: 0x06001180 RID: 4480 RVA: 0x000571A4 File Offset: 0x000553A4
		// (set) Token: 0x06001181 RID: 4481 RVA: 0x000571AC File Offset: 0x000553AC
		public CutsceneCamera.TransitionInfo TransitionIn_Server { get; private set; } = new CutsceneCamera.TransitionInfo(CameraTransition.Ease, 3f);

		// Token: 0x17000378 RID: 888
		// (get) Token: 0x06001182 RID: 4482 RVA: 0x000571B5 File Offset: 0x000553B5
		// (set) Token: 0x06001183 RID: 4483 RVA: 0x000571BD File Offset: 0x000553BD
		public CutsceneCamera.TransitionInfo TransitionOut_Server { get; private set; } = new CutsceneCamera.TransitionInfo(CameraTransition.Ease, 3f);

		// Token: 0x17000379 RID: 889
		// (get) Token: 0x06001184 RID: 4484 RVA: 0x000571C6 File Offset: 0x000553C6
		public CutsceneCamera.MoveToInfo MoveToInfo_Server
		{
			get
			{
				return this.moveToInfo_server;
			}
		}

		// Token: 0x1700037A RID: 890
		// (get) Token: 0x06001185 RID: 4485 RVA: 0x000571CE File Offset: 0x000553CE
		public CinemachineCamera CinemachineCamera
		{
			get
			{
				return this.cinemachineCamera;
			}
		}

		// Token: 0x1700037B RID: 891
		// (get) Token: 0x06001186 RID: 4486 RVA: 0x000571D6 File Offset: 0x000553D6
		// (set) Token: 0x06001187 RID: 4487 RVA: 0x000571E3 File Offset: 0x000553E3
		public float Pitch
		{
			get
			{
				return this.pitch.Value;
			}
			set
			{
				if (base.IsServer)
				{
					this.pitch.Value = value;
				}
			}
		}

		// Token: 0x1700037C RID: 892
		// (get) Token: 0x06001188 RID: 4488 RVA: 0x000571F9 File Offset: 0x000553F9
		// (set) Token: 0x06001189 RID: 4489 RVA: 0x00057206 File Offset: 0x00055406
		public float Yaw
		{
			get
			{
				return this.yaw.Value;
			}
			set
			{
				if (base.IsServer)
				{
					this.yaw.Value = value;
				}
			}
		}

		// Token: 0x1700037D RID: 893
		// (get) Token: 0x0600118A RID: 4490 RVA: 0x0005721C File Offset: 0x0005541C
		// (set) Token: 0x0600118B RID: 4491 RVA: 0x00057229 File Offset: 0x00055429
		public float FieldOfView
		{
			get
			{
				return this.fieldOfView.Value;
			}
			set
			{
				if (base.IsServer)
				{
					this.fieldOfView.Value = value;
				}
			}
		}

		// Token: 0x1700037E RID: 894
		// (get) Token: 0x0600118C RID: 4492 RVA: 0x0005723F File Offset: 0x0005543F
		// (set) Token: 0x0600118D RID: 4493 RVA: 0x0005724C File Offset: 0x0005544C
		public global::UnityEngine.Vector3 PositionOffset
		{
			get
			{
				return this.positionOffset.Value;
			}
			set
			{
				if (base.IsServer)
				{
					this.positionOffset.Value = value;
				}
			}
		}

		// Token: 0x0600118E RID: 4494 RVA: 0x00057264 File Offset: 0x00055464
		private new void Start()
		{
			base.Start();
			NetworkVariable<float> networkVariable = this.followDampening;
			networkVariable.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(this.HandleFollowDampeningChanged));
			NetworkVariable<float> networkVariable2 = this.lookDampening;
			networkVariable2.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable2.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(this.HandleLookDampeningChanged));
		}

		// Token: 0x0600118F RID: 4495 RVA: 0x000572C8 File Offset: 0x000554C8
		private void UpdatePreviewData()
		{
			if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				this.CinemachineCamera.transform.localEulerAngles = new global::UnityEngine.Vector3(this.pitch.Value, this.yaw.Value, 0f);
				this.CinemachineCamera.Lens.FieldOfView = this.fieldOfView.Value;
				this.CinemachineCamera.transform.localPosition = this.PositionOffset;
			}
		}

		// Token: 0x06001190 RID: 4496 RVA: 0x00057342 File Offset: 0x00055542
		public void SetPitch(float value)
		{
			if (base.IsServer)
			{
				this.pitch.Value = -value;
			}
		}

		// Token: 0x06001191 RID: 4497 RVA: 0x00057206 File Offset: 0x00055406
		public void SetYaw(float value)
		{
			if (base.IsServer)
			{
				this.yaw.Value = value;
			}
		}

		// Token: 0x06001192 RID: 4498 RVA: 0x00057229 File Offset: 0x00055429
		public void SetFieldOfView(float value)
		{
			if (base.IsServer)
			{
				this.fieldOfView.Value = value;
			}
		}

		// Token: 0x06001193 RID: 4499 RVA: 0x0005724C File Offset: 0x0005544C
		public void SetPositionOffset(global::UnityEngine.Vector3 value)
		{
			if (base.IsServer)
			{
				this.positionOffset.Value = value;
			}
		}

		// Token: 0x06001194 RID: 4500 RVA: 0x00057359 File Offset: 0x00055559
		public void SetFollowTarget(Context target)
		{
			if (base.IsServer)
			{
				if (target != null)
				{
					this.currentFollowInfo_Server = new CutsceneCamera.TargetInfo(target.WorldObject);
					return;
				}
				this.currentFollowInfo_Server = new CutsceneCamera.TargetInfo(null);
			}
		}

		// Token: 0x06001195 RID: 4501 RVA: 0x00057384 File Offset: 0x00055584
		public void SetLookAtTarget(Context target)
		{
			if (base.IsServer)
			{
				if (target != null)
				{
					this.currentLookAtInfo_Server = new CutsceneCamera.TargetInfo(target.WorldObject);
					return;
				}
				this.currentLookAtInfo_Server = new CutsceneCamera.TargetInfo(null);
			}
		}

		// Token: 0x06001196 RID: 4502 RVA: 0x000573AF File Offset: 0x000555AF
		public void SetFollowOffset(global::UnityEngine.Vector3 offset)
		{
			if (base.IsServer)
			{
				this.followOffset.Value = offset;
			}
		}

		// Token: 0x06001197 RID: 4503 RVA: 0x000573C5 File Offset: 0x000555C5
		public void SetLookOffset(global::UnityEngine.Vector3 offset)
		{
			if (base.IsServer)
			{
				this.lookOffset.Value = offset;
			}
		}

		// Token: 0x06001198 RID: 4504 RVA: 0x000573DB File Offset: 0x000555DB
		public void SetFollowDampening(float value)
		{
			if (base.IsServer)
			{
				this.followDampening.Value = Mathf.Clamp(value, 0f, 20f);
			}
		}

		// Token: 0x06001199 RID: 4505 RVA: 0x00057400 File Offset: 0x00055600
		public void SetLookDampening(float value)
		{
			if (base.IsServer)
			{
				this.lookDampening.Value = Mathf.Clamp(value, 0f, 20f);
			}
		}

		// Token: 0x0600119A RID: 4506 RVA: 0x00057425 File Offset: 0x00055625
		public void SetDuration(float baseDuration, float secondaryDuration = 0f)
		{
			if (base.IsServer)
			{
				this.baseShotDuration = baseDuration;
				this.secondaryShotDuration = secondaryDuration;
			}
		}

		// Token: 0x0600119B RID: 4507 RVA: 0x0005743D File Offset: 0x0005563D
		public void SetMoveToPosition(global::UnityEngine.Vector3 position)
		{
			if (base.IsServer)
			{
				this.moveToInfo_server.MoveToPosition = position;
				this.moveToInfo_server.HasValue = true;
			}
		}

		// Token: 0x0600119C RID: 4508 RVA: 0x0005745F File Offset: 0x0005565F
		public void SetMoveToRotation(float pitch, float yaw)
		{
			if (base.IsServer)
			{
				this.moveToInfo_server.MoveToPitch = pitch;
				this.moveToInfo_server.MoveToYaw = yaw;
				this.moveToInfo_server.HasValue = true;
			}
		}

		// Token: 0x0600119D RID: 4509 RVA: 0x0005748D File Offset: 0x0005568D
		public void SetTransitionIn(CameraTransition type, float duration)
		{
			if (base.IsServer)
			{
				this.TransitionIn_Server = new CutsceneCamera.TransitionInfo(type, duration);
			}
		}

		// Token: 0x0600119E RID: 4510 RVA: 0x000574A4 File Offset: 0x000556A4
		public void SetTransitionOut(CameraTransition type, float duration)
		{
			if (base.IsServer)
			{
				this.TransitionOut_Server = new CutsceneCamera.TransitionInfo(type, duration);
			}
		}

		// Token: 0x0600119F RID: 4511 RVA: 0x000574BC File Offset: 0x000556BC
		public void JoinInProgress(CutsceneManager.InProgressState inProgressState)
		{
			this.localUseFollowInfo = inProgressState.FollowInfo;
			this.localUseMoveToInfo = inProgressState.MoveToInfo;
			this.localUseShotDuration = inProgressState.ShotDuration;
			float num = ((this.TransitionIn_Server.Type != CameraTransition.Cut) ? this.TransitionIn_Server.Duration : 0f);
			if (inProgressState.LateJoin)
			{
				this.localStartTime = num + Time.realtimeSinceStartup - (float)(base.NetworkManager.ServerTime.Time - inProgressState.StartTime);
				return;
			}
			this.localStartTime = num + Time.realtimeSinceStartup;
		}

		// Token: 0x060011A0 RID: 4512 RVA: 0x0005754D File Offset: 0x0005574D
		public void EnabledLocally(CutsceneCamera.TargetInfo followInfo, CutsceneCamera.TargetInfo lookAtInfo)
		{
			this.locallyActive = true;
			this.localUseFollowInfo = followInfo;
			this.localUseLookAtInfo = lookAtInfo;
			this.InitializeShot();
		}

		// Token: 0x060011A1 RID: 4513 RVA: 0x0005756A File Offset: 0x0005576A
		public void DisabledLocally()
		{
			this.locallyActive = false;
		}

		// Token: 0x060011A2 RID: 4514 RVA: 0x00057574 File Offset: 0x00055774
		private void InitializeShot()
		{
			this.CinemachineCamera.transform.localPosition = this.PositionOffset;
			this.CinemachineCamera.transform.localEulerAngles = new global::UnityEngine.Vector3(this.pitch.Value, this.yaw.Value, 0f);
			this.CinemachineCamera.Lens.FieldOfView = this.fieldOfView.Value;
			if (this.localUseFollowInfo.HasTarget)
			{
				Transform transform;
				if (this.localUseFollowInfo.TargetObject.BaseType is PlayerLuaComponent)
				{
					transform = this.localUseFollowInfo.TargetObject.GetComponent<PlayerReferenceManager>().ApperanceController.transform;
				}
				else
				{
					transform = this.localUseFollowInfo.TargetObject.transform.GetChild(0);
				}
				this.cinemachineThirdPersonFollow.ShoulderOffset = this.followOffset.Value;
				this.CinemachineCamera.Follow = transform;
			}
			else
			{
				this.cinemachineThirdPersonFollow.ShoulderOffset = global::UnityEngine.Vector3.zero;
				this.CinemachineCamera.Follow = null;
			}
			if (this.localUseLookAtInfo.HasTarget)
			{
				Transform transform2;
				if (this.localUseLookAtInfo.TargetObject.BaseType is PlayerLuaComponent)
				{
					transform2 = this.localUseLookAtInfo.TargetObject.GetComponent<PlayerReferenceManager>().ApperanceController.transform;
				}
				else
				{
					transform2 = this.localUseLookAtInfo.TargetObject.transform.GetChild(0);
				}
				this.CinemachineCamera.LookAt = transform2;
				this.cinemachineRotationComposer.TargetOffset = this.lookOffset.Value;
				return;
			}
			this.CinemachineCamera.LookAt = null;
			this.cinemachineRotationComposer.TargetOffset = global::UnityEngine.Vector3.zero;
		}

		// Token: 0x060011A3 RID: 4515 RVA: 0x00057711 File Offset: 0x00055911
		private void HandleFollowDampeningChanged(float previousValue, float newValue)
		{
			this.cinemachineThirdPersonFollow.Damping = new global::UnityEngine.Vector3(newValue, newValue, newValue);
		}

		// Token: 0x060011A4 RID: 4516 RVA: 0x00057726 File Offset: 0x00055926
		private void HandleLookDampeningChanged(float previousValue, float newValue)
		{
			this.cinemachineRotationComposer.Damping.x = newValue;
			this.cinemachineRotationComposer.Damping.y = newValue;
		}

		// Token: 0x060011A5 RID: 4517 RVA: 0x0005774C File Offset: 0x0005594C
		private void Update()
		{
			if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				this.UpdatePreviewData();
			}
			if (!this.locallyActive)
			{
				return;
			}
			if (!this.localUseFollowInfo.HasTarget && this.localUseMoveToInfo.HasValue)
			{
				float num = (Time.realtimeSinceStartup - this.localStartTime) / this.localUseShotDuration;
				global::UnityEngine.Vector3 vector = global::UnityEngine.Vector3.Lerp(base.transform.position + this.PositionOffset, this.localUseMoveToInfo.MoveToPosition, num);
				Quaternion quaternion = Quaternion.Lerp(Quaternion.Euler(this.pitch.Value, this.yaw.Value, 0f), Quaternion.Euler(this.localUseMoveToInfo.MoveToPitch, this.localUseMoveToInfo.MoveToYaw, 0f), num);
				this.CinemachineCamera.transform.position = vector;
				this.CinemachineCamera.transform.localRotation = quaternion;
			}
		}

		// Token: 0x1700037F RID: 895
		// (get) Token: 0x060011A6 RID: 4518 RVA: 0x0005783C File Offset: 0x00055A3C
		public Context Context
		{
			get
			{
				Context context;
				if ((context = this.context) == null)
				{
					context = (this.context = new Context(this.WorldObject));
				}
				return context;
			}
		}

		// Token: 0x17000380 RID: 896
		// (get) Token: 0x060011A7 RID: 4519 RVA: 0x00057867 File Offset: 0x00055A67
		// (set) Token: 0x060011A8 RID: 4520 RVA: 0x0005786F File Offset: 0x00055A6F
		public WorldObject WorldObject { get; private set; }

		// Token: 0x17000381 RID: 897
		// (get) Token: 0x060011A9 RID: 4521 RVA: 0x00017586 File Offset: 0x00015786
		public ReferenceFilter Filter
		{
			get
			{
				return ReferenceFilter.NonStatic;
			}
		}

		// Token: 0x060011AA RID: 4522 RVA: 0x00057878 File Offset: 0x00055A78
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x17000382 RID: 898
		// (get) Token: 0x060011AB RID: 4523 RVA: 0x00057881 File Offset: 0x00055A81
		public object LuaObject
		{
			get
			{
				if (this.luaInterface == null)
				{
					this.luaInterface = new Endless.Gameplay.LuaInterfaces.Camera(this);
				}
				return this.luaInterface;
			}
		}

		// Token: 0x17000383 RID: 899
		// (get) Token: 0x060011AC RID: 4524 RVA: 0x0005789D File Offset: 0x00055A9D
		public Type LuaObjectType
		{
			get
			{
				return typeof(Endless.Gameplay.LuaInterfaces.Camera);
			}
		}

		// Token: 0x060011AD RID: 4525 RVA: 0x000578A9 File Offset: 0x00055AA9
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x060011AF RID: 4527 RVA: 0x000579A8 File Offset: 0x00055BA8
		protected override void __initializeVariables()
		{
			bool flag = this.pitch == null;
			if (flag)
			{
				throw new Exception("CutsceneCamera.pitch cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.pitch.Initialize(this);
			base.__nameNetworkVariable(this.pitch, "pitch");
			this.NetworkVariableFields.Add(this.pitch);
			flag = this.yaw == null;
			if (flag)
			{
				throw new Exception("CutsceneCamera.yaw cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.yaw.Initialize(this);
			base.__nameNetworkVariable(this.yaw, "yaw");
			this.NetworkVariableFields.Add(this.yaw);
			flag = this.fieldOfView == null;
			if (flag)
			{
				throw new Exception("CutsceneCamera.fieldOfView cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.fieldOfView.Initialize(this);
			base.__nameNetworkVariable(this.fieldOfView, "fieldOfView");
			this.NetworkVariableFields.Add(this.fieldOfView);
			flag = this.followOffset == null;
			if (flag)
			{
				throw new Exception("CutsceneCamera.followOffset cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.followOffset.Initialize(this);
			base.__nameNetworkVariable(this.followOffset, "followOffset");
			this.NetworkVariableFields.Add(this.followOffset);
			flag = this.lookOffset == null;
			if (flag)
			{
				throw new Exception("CutsceneCamera.lookOffset cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.lookOffset.Initialize(this);
			base.__nameNetworkVariable(this.lookOffset, "lookOffset");
			this.NetworkVariableFields.Add(this.lookOffset);
			flag = this.followDampening == null;
			if (flag)
			{
				throw new Exception("CutsceneCamera.followDampening cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.followDampening.Initialize(this);
			base.__nameNetworkVariable(this.followDampening, "followDampening");
			this.NetworkVariableFields.Add(this.followDampening);
			flag = this.lookDampening == null;
			if (flag)
			{
				throw new Exception("CutsceneCamera.lookDampening cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.lookDampening.Initialize(this);
			base.__nameNetworkVariable(this.lookDampening, "lookDampening");
			this.NetworkVariableFields.Add(this.lookDampening);
			flag = this.positionOffset == null;
			if (flag)
			{
				throw new Exception("CutsceneCamera.positionOffset cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.positionOffset.Initialize(this);
			base.__nameNetworkVariable(this.positionOffset, "positionOffset");
			this.NetworkVariableFields.Add(this.positionOffset);
			base.__initializeVariables();
		}

		// Token: 0x060011B0 RID: 4528 RVA: 0x0001E813 File Offset: 0x0001CA13
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x060011B1 RID: 4529 RVA: 0x00057C26 File Offset: 0x00055E26
		protected internal override string __getTypeName()
		{
			return "CutsceneCamera";
		}

		// Token: 0x04000EFA RID: 3834
		public EndlessEvent OnShotStarted = new EndlessEvent();

		// Token: 0x04000EFB RID: 3835
		public EndlessEvent OnShotFinished = new EndlessEvent();

		// Token: 0x04000EFC RID: 3836
		public EndlessEvent OnShotInterrupted = new EndlessEvent();

		// Token: 0x04000EFD RID: 3837
		[SerializeField]
		private CinemachineCamera cinemachineCamera;

		// Token: 0x04000EFE RID: 3838
		[SerializeField]
		private CinemachineThirdPersonFollow cinemachineThirdPersonFollow;

		// Token: 0x04000EFF RID: 3839
		[SerializeField]
		private CinemachineRotationComposer cinemachineRotationComposer;

		// Token: 0x04000F00 RID: 3840
		[SerializeField]
		private InputSettings InputAllowedDuringShot;

		// Token: 0x04000F01 RID: 3841
		[SerializeField]
		private bool InvulnerablePlayerDuringShot;

		// Token: 0x04000F02 RID: 3842
		private NetworkVariable<float> pitch = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000F03 RID: 3843
		private NetworkVariable<float> yaw = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000F04 RID: 3844
		private NetworkVariable<float> fieldOfView = new NetworkVariable<float>(60f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000F05 RID: 3845
		private NetworkVariable<global::UnityEngine.Vector3> followOffset = new NetworkVariable<global::UnityEngine.Vector3>(default(global::UnityEngine.Vector3), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000F06 RID: 3846
		private NetworkVariable<global::UnityEngine.Vector3> lookOffset = new NetworkVariable<global::UnityEngine.Vector3>(default(global::UnityEngine.Vector3), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000F07 RID: 3847
		private NetworkVariable<float> followDampening = new NetworkVariable<float>(2f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000F08 RID: 3848
		private NetworkVariable<float> lookDampening = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000F09 RID: 3849
		public NetworkVariable<global::UnityEngine.Vector3> positionOffset = new NetworkVariable<global::UnityEngine.Vector3>(default(global::UnityEngine.Vector3), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000F0A RID: 3850
		private float baseShotDuration;

		// Token: 0x04000F0B RID: 3851
		private float secondaryShotDuration;

		// Token: 0x04000F0C RID: 3852
		private bool locallyActive;

		// Token: 0x04000F0D RID: 3853
		private CutsceneCamera.TargetInfo currentFollowInfo_Server;

		// Token: 0x04000F0E RID: 3854
		private CutsceneCamera.TargetInfo currentLookAtInfo_Server;

		// Token: 0x04000F11 RID: 3857
		private CutsceneCamera.MoveToInfo moveToInfo_server;

		// Token: 0x04000F12 RID: 3858
		private CutsceneCamera.TargetInfo localUseFollowInfo;

		// Token: 0x04000F13 RID: 3859
		private CutsceneCamera.TargetInfo localUseLookAtInfo;

		// Token: 0x04000F14 RID: 3860
		private CutsceneCamera.MoveToInfo localUseMoveToInfo;

		// Token: 0x04000F15 RID: 3861
		private float localStartTime;

		// Token: 0x04000F16 RID: 3862
		private float localUseShotDuration;

		// Token: 0x04000F17 RID: 3863
		private Context context;

		// Token: 0x04000F19 RID: 3865
		private Endless.Gameplay.LuaInterfaces.Camera luaInterface;

		// Token: 0x04000F1A RID: 3866
		private EndlessScriptComponent scriptComponent;

		// Token: 0x02000300 RID: 768
		public struct TargetInfo : INetworkSerializable
		{
			// Token: 0x17000384 RID: 900
			// (get) Token: 0x060011B2 RID: 4530 RVA: 0x00057C2D File Offset: 0x00055E2D
			// (set) Token: 0x060011B3 RID: 4531 RVA: 0x00057C35 File Offset: 0x00055E35
			public WorldObject TargetObject { readonly get; private set; }

			// Token: 0x17000385 RID: 901
			// (get) Token: 0x060011B4 RID: 4532 RVA: 0x00057C3E File Offset: 0x00055E3E
			public bool HasTarget
			{
				get
				{
					return this.hasTarget;
				}
			}

			// Token: 0x060011B5 RID: 4533 RVA: 0x00057C48 File Offset: 0x00055E48
			public TargetInfo(WorldObject worldObject)
			{
				if (worldObject == null)
				{
					this.TargetObject = null;
					this.hasTarget = false;
					this.networkId = 0UL;
					this.instanceId = SerializableGuid.Empty;
					return;
				}
				this.TargetObject = worldObject;
				this.hasTarget = true;
				if (worldObject.NetworkObject != null)
				{
					this.networkId = worldObject.NetworkObject.NetworkObjectId;
					this.instanceId = SerializableGuid.Empty;
					return;
				}
				this.networkId = 0UL;
				this.instanceId = worldObject.InstanceId;
			}

			// Token: 0x060011B6 RID: 4534 RVA: 0x00057CD0 File Offset: 0x00055ED0
			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
			{
				serializer.SerializeValue<bool>(ref this.hasTarget, default(FastBufferWriter.ForPrimitives));
				if (this.hasTarget)
				{
					serializer.SerializeValue<ulong>(ref this.networkId, default(FastBufferWriter.ForPrimitives));
					if (this.networkId == 0UL)
					{
						serializer.SerializeValue<SerializableGuid>(ref this.instanceId, default(FastBufferWriter.ForNetworkSerializable));
					}
					if (serializer.IsReader)
					{
						GameObject gameObject = null;
						if (this.networkId != 0UL)
						{
							gameObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[this.networkId].gameObject;
						}
						else if (this.instanceId != SerializableGuid.Empty)
						{
							gameObject = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(this.instanceId);
						}
						if (gameObject != null)
						{
							this.TargetObject = gameObject.GetComponent<WorldObject>();
						}
					}
				}
			}

			// Token: 0x04000F1C RID: 3868
			private bool hasTarget;

			// Token: 0x04000F1D RID: 3869
			private ulong networkId;

			// Token: 0x04000F1E RID: 3870
			private SerializableGuid instanceId;
		}

		// Token: 0x02000301 RID: 769
		public struct MoveToInfo : INetworkSerializable
		{
			// Token: 0x060011B7 RID: 4535 RVA: 0x00057DA4 File Offset: 0x00055FA4
			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
			{
				serializer.SerializeValue<bool>(ref this.HasValue, default(FastBufferWriter.ForPrimitives));
				if (this.HasValue)
				{
					serializer.SerializeValue(ref this.MoveToPosition);
					serializer.SerializeValue<float>(ref this.MoveToPitch, default(FastBufferWriter.ForPrimitives));
					serializer.SerializeValue<float>(ref this.MoveToYaw, default(FastBufferWriter.ForPrimitives));
				}
			}

			// Token: 0x04000F1F RID: 3871
			public bool HasValue;

			// Token: 0x04000F20 RID: 3872
			public global::UnityEngine.Vector3 MoveToPosition;

			// Token: 0x04000F21 RID: 3873
			public float MoveToPitch;

			// Token: 0x04000F22 RID: 3874
			public float MoveToYaw;
		}

		// Token: 0x02000302 RID: 770
		public struct TransitionInfo : INetworkSerializable
		{
			// Token: 0x060011B8 RID: 4536 RVA: 0x00057E08 File Offset: 0x00056008
			public TransitionInfo(CameraTransition type, float duration)
			{
				this.Type = type;
				this.Duration = Mathf.Max(0f, duration);
			}

			// Token: 0x060011B9 RID: 4537 RVA: 0x00057E24 File Offset: 0x00056024
			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
			{
				serializer.SerializeValue<CameraTransition>(ref this.Type, default(FastBufferWriter.ForEnums));
				serializer.SerializeValue<float>(ref this.Duration, default(FastBufferWriter.ForPrimitives));
			}

			// Token: 0x04000F23 RID: 3875
			public CameraTransition Type;

			// Token: 0x04000F24 RID: 3876
			public float Duration;
		}
	}
}
