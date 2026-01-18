using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200030C RID: 780
	[DefaultExecutionOrder(-10)]
	[RequireComponent(typeof(TweenRectTransform))]
	public class UIWiringObjectInspectorView : UIGameObject, IBackable, IValidatable
	{
		// Token: 0x170001EF RID: 495
		// (get) Token: 0x06000DDE RID: 3550 RVA: 0x000420B2 File Offset: 0x000402B2
		// (set) Token: 0x06000DDF RID: 3551 RVA: 0x000420BA File Offset: 0x000402BA
		public Transform TargetTransform { get; private set; }

		// Token: 0x170001F0 RID: 496
		// (get) Token: 0x06000DE0 RID: 3552 RVA: 0x000420C3 File Offset: 0x000402C3
		// (set) Token: 0x06000DE1 RID: 3553 RVA: 0x000420CB File Offset: 0x000402CB
		public SerializableGuid TargetId { get; private set; } = SerializableGuid.Empty;

		// Token: 0x170001F1 RID: 497
		// (get) Token: 0x06000DE2 RID: 3554 RVA: 0x000420D4 File Offset: 0x000402D4
		// (set) Token: 0x06000DE3 RID: 3555 RVA: 0x000420DC File Offset: 0x000402DC
		public bool IsOnLeftOfScreen { get; private set; }

		// Token: 0x170001F2 RID: 498
		// (get) Token: 0x06000DE4 RID: 3556 RVA: 0x000420E5 File Offset: 0x000402E5
		// (set) Token: 0x06000DE5 RID: 3557 RVA: 0x000420ED File Offset: 0x000402ED
		public bool IsReceiver { get; private set; }

		// Token: 0x170001F3 RID: 499
		// (get) Token: 0x06000DE6 RID: 3558 RVA: 0x000420F6 File Offset: 0x000402F6
		// (set) Token: 0x06000DE7 RID: 3559 RVA: 0x000420FE File Offset: 0x000402FE
		public bool IsOpen { get; private set; }

		// Token: 0x170001F4 RID: 500
		// (get) Token: 0x06000DE8 RID: 3560 RVA: 0x00042107 File Offset: 0x00040307
		public IReadOnlyList<EndlessEventInfo> NodeEvents
		{
			get
			{
				return this.nodeEvents;
			}
		}

		// Token: 0x170001F5 RID: 501
		// (get) Token: 0x06000DE9 RID: 3561 RVA: 0x0004210F File Offset: 0x0004030F
		public IReadOnlyList<UIWireNodeView> SpawnedNodes
		{
			get
			{
				return this.spawnedNodes;
			}
		}

		// Token: 0x170001F6 RID: 502
		// (get) Token: 0x06000DEA RID: 3562 RVA: 0x00042117 File Offset: 0x00040317
		public TweenRectTransform TweenRectTransform
		{
			get
			{
				if (!this.tweenRectTransform)
				{
					base.TryGetComponent<TweenRectTransform>(out this.tweenRectTransform);
				}
				return this.tweenRectTransform;
			}
		}

		// Token: 0x06000DEB RID: 3563 RVA: 0x0004213C File Offset: 0x0004033C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.nameText.color = this.wireColorDictionary[this.color].Color;
			this.infoText.color = this.wireColorDictionary[this.color].Color;
			this.flowInfoImage.material = this.wireColorDictionary[this.color].UIMaterial;
			this.flowInfoImage.material.SetFloat(this.wireShaderPropertiesDictionary.Tiling, (float)this.flowInfoImageTile);
			this.hideTweens.OnAllTweenCompleted.AddListener(new UnityAction(this.ToggleOff));
			this.hideTweens.SetToEnd();
		}

		// Token: 0x06000DEC RID: 3564 RVA: 0x0004220D File Offset: 0x0004040D
		private void OnDisable()
		{
			if (this.applicationIsQuitting)
			{
				return;
			}
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			this.DespawnSpawnedNodes();
		}

		// Token: 0x06000DED RID: 3565 RVA: 0x00042236 File Offset: 0x00040436
		public void OnBack()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIWiringManager>.Instance.HideWiringInspector(this, true);
		}

		// Token: 0x06000DEE RID: 3566 RVA: 0x0004225C File Offset: 0x0004045C
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			this.hideTweens.ValidateForNumberOfTweens(1);
		}

		// Token: 0x06000DEF RID: 3567 RVA: 0x00042284 File Offset: 0x00040484
		public void Display(Transform targetTransform, SerializableGuid targetId, List<UIEndlessEventList> nodeEventList, bool isOnLeftOfScreen, bool isReceiver, RectTransform rect)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", new object[]
				{
					targetTransform.name,
					targetId,
					this.nodeEvents.Count,
					isOnLeftOfScreen,
					isReceiver,
					rect.name
				});
			}
			this.TargetTransform = targetTransform;
			this.TargetId = targetId;
			this.nodeEvents = nodeEventList.SelectMany((UIEndlessEventList e) => e.EndlessEventInfos).ToList<EndlessEventInfo>();
			this.IsOnLeftOfScreen = isOnLeftOfScreen;
			this.IsReceiver = isReceiver;
			this.IsOpen = true;
			if (this.spawnedNodes.Count > 0)
			{
				this.DespawnSpawnedNodes();
			}
			HashSet<string> duplicateMemberNames = this.GetDuplicateMemberNames(nodeEventList);
			int num = 0;
			for (int i = 0; i < nodeEventList.Count; i++)
			{
				for (int j = 0; j < nodeEventList[i].EndlessEventInfos.Count; j++)
				{
					PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
					UIWireNodeView uiwireNodeView = this.nodeSource;
					Transform transform = this.nodeContainer;
					UIWireNodeView uiwireNodeView2 = instance.Spawn<UIWireNodeView>(uiwireNodeView, default(Vector3), default(Quaternion), transform);
					EndlessEventInfo endlessEventInfo = nodeEventList[i].EndlessEventInfos[j];
					bool flag = duplicateMemberNames.Contains(endlessEventInfo.MemberName);
					uiwireNodeView2.Initialize(targetId, endlessEventInfo, num, nodeEventList[i].AssemblyQualifiedTypeName, this, this.wireColorDictionary[this.color].Color, flag);
					this.spawnedNodes.Add(uiwireNodeView2);
					string text = nodeEventList[i].AssemblyQualifiedTypeName + "." + endlessEventInfo.MemberName;
					this.spawnedNodeLookUp.Add(text, uiwireNodeView2);
					num++;
				}
			}
			this.nodeContainer.gameObject.SetActive(this.spawnedNodes.Count > 0);
			this.emptyFlair.SetActive(this.spawnedNodes.Count == 0);
			SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(targetId);
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
			this.nameText.text = runtimePropInfo.PropData.Name;
			this.SetIsOnLeftOfScreen(isOnLeftOfScreen);
			RectTransformValue rectTransformValue = new RectTransformValue(rect);
			rectTransformValue.ApplyTo(base.RectTransform);
			this.displayTweens.Tween();
			base.gameObject.SetActive(true);
			this.OnDisplay.Invoke();
			if (!MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
			}
		}

		// Token: 0x06000DF0 RID: 3568 RVA: 0x00042530 File Offset: 0x00040730
		public void SetIsOnLeftOfScreen(bool isOnLeftOfScreen)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetIsOnLeftOfScreen", new object[] { isOnLeftOfScreen });
			}
			this.IsOnLeftOfScreen = isOnLeftOfScreen;
			for (int i = 0; i < this.spawnedNodes.Count; i++)
			{
				this.spawnedNodes[i].SetIsOnLeftOfScreen(isOnLeftOfScreen);
			}
			bool flag = (!this.IsReceiver && !isOnLeftOfScreen) || (this.IsReceiver && isOnLeftOfScreen);
			this.flowInfoImage.transform.localEulerAngles = new Vector3(0f, 0f, (float)(flag ? 180 : 0));
		}

		// Token: 0x06000DF1 RID: 3569 RVA: 0x000425D0 File Offset: 0x000407D0
		public void TweenTo(RectTransform target)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "TweenTo", new object[] { target.name });
			}
			this.TweenRectTransform.To = target;
			this.TweenRectTransform.Tween();
		}

		// Token: 0x06000DF2 RID: 3570 RVA: 0x0004260C File Offset: 0x0004080C
		public void ToggleOnNothingSelectedTweens(bool state)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleOnNothingSelectedTweens", new object[] { state });
			}
			for (int i = 0; i < this.spawnedNodes.Count; i++)
			{
				this.spawnedNodes[i].ToggleOnNothingSelectedTweens(state);
			}
		}

		// Token: 0x06000DF3 RID: 3571 RVA: 0x00042663 File Offset: 0x00040863
		public void ToggleNodeSelectedVisuals(int index, bool state)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleNodeSelectedVisuals", new object[] { index, state });
			}
			this.spawnedNodes[index].ToggleSelectedVisuals(state);
		}

		// Token: 0x06000DF4 RID: 3572 RVA: 0x000426A4 File Offset: 0x000408A4
		public void ToggleDropHandlers(bool state, UIWireNodeView except)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleDropHandlers", new object[]
				{
					state,
					except.DebugSafeName(true)
				});
			}
			for (int i = 0; i < this.spawnedNodes.Count; i++)
			{
				if (this.spawnedNodes[i] != except)
				{
					this.spawnedNodes[i].ToggleDropHandler(state);
				}
			}
		}

		// Token: 0x06000DF5 RID: 3573 RVA: 0x0004271C File Offset: 0x0004091C
		public UIWireNodeView GetNodeView(string withTypeName, string withMemberName)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetNodeView", new object[] { withTypeName, withMemberName });
			}
			string text = withTypeName + "." + withMemberName;
			if (!this.spawnedNodeLookUp.ContainsKey(text))
			{
				DebugUtility.LogError(this, "GetNodeView", "Could not find that node!", new object[] { text });
				return null;
			}
			return this.spawnedNodeLookUp[text];
		}

		// Token: 0x06000DF6 RID: 3574 RVA: 0x00042790 File Offset: 0x00040990
		public void UpdateWireCounts()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateWireCounts", Array.Empty<object>());
			}
			for (int i = 0; i < this.spawnedNodes.Count; i++)
			{
				this.spawnedNodes[i].UpdateWireCount();
			}
		}

		// Token: 0x06000DF7 RID: 3575 RVA: 0x000427DC File Offset: 0x000409DC
		public void Hide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Hide", Array.Empty<object>());
			}
			this.IsOpen = false;
			this.TargetTransform = null;
			this.TargetId = SerializableGuid.Empty;
			this.nodeEvents.Clear();
			this.hideTweens.Tween();
			this.OnHide.Invoke();
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
		}

		// Token: 0x06000DF8 RID: 3576 RVA: 0x00042846 File Offset: 0x00040A46
		private void ToggleOff()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleOff", Array.Empty<object>());
			}
			base.gameObject.SetActive(false);
		}

		// Token: 0x06000DF9 RID: 3577 RVA: 0x0004286C File Offset: 0x00040A6C
		private void DespawnSpawnedNodes()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DespawnSpawnedNodes", Array.Empty<object>());
			}
			for (int i = 0; i < this.spawnedNodes.Count; i++)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIWireNodeView>(this.spawnedNodes[i]);
			}
			this.spawnedNodes.Clear();
			this.spawnedNodeLookUp.Clear();
		}

		// Token: 0x06000DFA RID: 3578 RVA: 0x000428D4 File Offset: 0x00040AD4
		private HashSet<string> GetDuplicateMemberNames(List<UIEndlessEventList> nodeEventList)
		{
			HashSet<string> hashSet = new HashSet<string>();
			HashSet<string> hashSet2 = new HashSet<string>();
			for (int i = 0; i < nodeEventList.Count; i++)
			{
				for (int j = 0; j < nodeEventList[i].EndlessEventInfos.Count; j++)
				{
					string memberName = nodeEventList[i].EndlessEventInfos[j].MemberName;
					if (hashSet2.Contains(memberName))
					{
						hashSet.Add(memberName);
					}
					else
					{
						hashSet2.Add(memberName);
					}
				}
			}
			return hashSet;
		}

		// Token: 0x04000BD5 RID: 3029
		public UnityEvent OnDisplay = new UnityEvent();

		// Token: 0x04000BD6 RID: 3030
		public UnityEvent OnHide = new UnityEvent();

		// Token: 0x04000BD7 RID: 3031
		[SerializeField]
		private WireColor color = WireColor.DodgerBlue;

		// Token: 0x04000BD8 RID: 3032
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x04000BD9 RID: 3033
		[SerializeField]
		private TextMeshProUGUI infoText;

		// Token: 0x04000BDA RID: 3034
		[SerializeField]
		private Image flowInfoImage;

		// Token: 0x04000BDB RID: 3035
		[SerializeField]
		private int flowInfoImageTile = 15;

		// Token: 0x04000BDC RID: 3036
		[SerializeField]
		private UIWireNodeView nodeSource;

		// Token: 0x04000BDD RID: 3037
		[SerializeField]
		private RectTransform nodeContainer;

		// Token: 0x04000BDE RID: 3038
		[SerializeField]
		private GameObject emptyFlair;

		// Token: 0x04000BDF RID: 3039
		[SerializeField]
		private TweenCollection displayTweens;

		// Token: 0x04000BE0 RID: 3040
		[SerializeField]
		private TweenCollection hideTweens;

		// Token: 0x04000BE1 RID: 3041
		[SerializeField]
		private WireColorDictionary wireColorDictionary;

		// Token: 0x04000BE2 RID: 3042
		[SerializeField]
		private UIWireShaderPropertiesDictionary wireShaderPropertiesDictionary;

		// Token: 0x04000BE3 RID: 3043
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000BE4 RID: 3044
		private readonly List<UIWireNodeView> spawnedNodes = new List<UIWireNodeView>();

		// Token: 0x04000BE5 RID: 3045
		private readonly Dictionary<string, UIWireNodeView> spawnedNodeLookUp = new Dictionary<string, UIWireNodeView>();

		// Token: 0x04000BE6 RID: 3046
		private List<EndlessEventInfo> nodeEvents = new List<EndlessEventInfo>();

		// Token: 0x04000BE7 RID: 3047
		private bool applicationIsQuitting;

		// Token: 0x04000BE8 RID: 3048
		private TweenRectTransform tweenRectTransform;
	}
}
