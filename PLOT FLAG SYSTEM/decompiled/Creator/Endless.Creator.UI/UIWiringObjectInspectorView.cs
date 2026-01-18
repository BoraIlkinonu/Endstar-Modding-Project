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

namespace Endless.Creator.UI;

[DefaultExecutionOrder(-10)]
[RequireComponent(typeof(TweenRectTransform))]
public class UIWiringObjectInspectorView : UIGameObject, IBackable, IValidatable
{
	public UnityEvent OnDisplay = new UnityEvent();

	public UnityEvent OnHide = new UnityEvent();

	[SerializeField]
	private WireColor color = WireColor.DodgerBlue;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI infoText;

	[SerializeField]
	private Image flowInfoImage;

	[SerializeField]
	private int flowInfoImageTile = 15;

	[SerializeField]
	private UIWireNodeView nodeSource;

	[SerializeField]
	private RectTransform nodeContainer;

	[SerializeField]
	private GameObject emptyFlair;

	[SerializeField]
	private TweenCollection displayTweens;

	[SerializeField]
	private TweenCollection hideTweens;

	[SerializeField]
	private WireColorDictionary wireColorDictionary;

	[SerializeField]
	private UIWireShaderPropertiesDictionary wireShaderPropertiesDictionary;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private readonly List<UIWireNodeView> spawnedNodes = new List<UIWireNodeView>();

	private readonly Dictionary<string, UIWireNodeView> spawnedNodeLookUp = new Dictionary<string, UIWireNodeView>();

	private List<EndlessEventInfo> nodeEvents = new List<EndlessEventInfo>();

	private bool applicationIsQuitting;

	private TweenRectTransform tweenRectTransform;

	public Transform TargetTransform { get; private set; }

	public SerializableGuid TargetId { get; private set; } = SerializableGuid.Empty;

	public bool IsOnLeftOfScreen { get; private set; }

	public bool IsReceiver { get; private set; }

	public bool IsOpen { get; private set; }

	public IReadOnlyList<EndlessEventInfo> NodeEvents => nodeEvents;

	public IReadOnlyList<UIWireNodeView> SpawnedNodes => spawnedNodes;

	public TweenRectTransform TweenRectTransform
	{
		get
		{
			if (!tweenRectTransform)
			{
				TryGetComponent<TweenRectTransform>(out tweenRectTransform);
			}
			return tweenRectTransform;
		}
	}

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		nameText.color = wireColorDictionary[color].Color;
		infoText.color = wireColorDictionary[color].Color;
		flowInfoImage.material = wireColorDictionary[color].UIMaterial;
		flowInfoImage.material.SetFloat(wireShaderPropertiesDictionary.Tiling, flowInfoImageTile);
		hideTweens.OnAllTweenCompleted.AddListener(ToggleOff);
		hideTweens.SetToEnd();
	}

	private void OnDisable()
	{
		if (!applicationIsQuitting)
		{
			if (verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable");
			}
			DespawnSpawnedNodes();
		}
	}

	public void OnBack()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		MonoBehaviourSingleton<UIWiringManager>.Instance.HideWiringInspector(this, displayToolPrompt: true);
	}

	[ContextMenu("Validate")]
	public void Validate()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Validate");
		}
		hideTweens.ValidateForNumberOfTweens();
	}

	public void Display(Transform targetTransform, SerializableGuid targetId, List<UIEndlessEventList> nodeEventList, bool isOnLeftOfScreen, bool isReceiver, RectTransform rect)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Display", targetTransform.name, targetId, nodeEvents.Count, isOnLeftOfScreen, isReceiver, rect.name);
		}
		TargetTransform = targetTransform;
		TargetId = targetId;
		nodeEvents = nodeEventList.SelectMany((UIEndlessEventList e) => e.EndlessEventInfos).ToList();
		IsOnLeftOfScreen = isOnLeftOfScreen;
		IsReceiver = isReceiver;
		IsOpen = true;
		if (spawnedNodes.Count > 0)
		{
			DespawnSpawnedNodes();
		}
		HashSet<string> duplicateMemberNames = GetDuplicateMemberNames(nodeEventList);
		int num = 0;
		for (int num2 = 0; num2 < nodeEventList.Count; num2++)
		{
			for (int num3 = 0; num3 < nodeEventList[num2].EndlessEventInfos.Count; num3++)
			{
				PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
				UIWireNodeView prefab = nodeSource;
				Transform parent = nodeContainer;
				UIWireNodeView uIWireNodeView = instance.Spawn(prefab, default(Vector3), default(Quaternion), parent);
				EndlessEventInfo endlessEventInfo = nodeEventList[num2].EndlessEventInfos[num3];
				bool showComponentName = duplicateMemberNames.Contains(endlessEventInfo.MemberName);
				uIWireNodeView.Initialize(targetId, endlessEventInfo, num, nodeEventList[num2].AssemblyQualifiedTypeName, this, wireColorDictionary[color].Color, showComponentName);
				spawnedNodes.Add(uIWireNodeView);
				string key = nodeEventList[num2].AssemblyQualifiedTypeName + "." + endlessEventInfo.MemberName;
				spawnedNodeLookUp.Add(key, uIWireNodeView);
				num++;
			}
		}
		nodeContainer.gameObject.SetActive(spawnedNodes.Count > 0);
		emptyFlair.SetActive(spawnedNodes.Count == 0);
		SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(targetId);
		PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetIdFromInstanceId);
		nameText.text = runtimePropInfo.PropData.Name;
		SetIsOnLeftOfScreen(isOnLeftOfScreen);
		new RectTransformValue(rect).ApplyTo(base.RectTransform);
		displayTweens.Tween();
		base.gameObject.SetActive(value: true);
		OnDisplay.Invoke();
		if (!MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
		{
			MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
		}
	}

	public void SetIsOnLeftOfScreen(bool isOnLeftOfScreen)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetIsOnLeftOfScreen", isOnLeftOfScreen);
		}
		IsOnLeftOfScreen = isOnLeftOfScreen;
		for (int i = 0; i < spawnedNodes.Count; i++)
		{
			spawnedNodes[i].SetIsOnLeftOfScreen(isOnLeftOfScreen);
		}
		bool flag = (!IsReceiver && !isOnLeftOfScreen) || (IsReceiver && isOnLeftOfScreen);
		flowInfoImage.transform.localEulerAngles = new Vector3(0f, 0f, flag ? 180 : 0);
	}

	public void TweenTo(RectTransform target)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "TweenTo", target.name);
		}
		TweenRectTransform.To = target;
		TweenRectTransform.Tween();
	}

	public void ToggleOnNothingSelectedTweens(bool state)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleOnNothingSelectedTweens", state);
		}
		for (int i = 0; i < spawnedNodes.Count; i++)
		{
			spawnedNodes[i].ToggleOnNothingSelectedTweens(state);
		}
	}

	public void ToggleNodeSelectedVisuals(int index, bool state)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleNodeSelectedVisuals", index, state);
		}
		spawnedNodes[index].ToggleSelectedVisuals(state);
	}

	public void ToggleDropHandlers(bool state, UIWireNodeView except)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleDropHandlers", state, except.DebugSafeName());
		}
		for (int i = 0; i < spawnedNodes.Count; i++)
		{
			if (spawnedNodes[i] != except)
			{
				spawnedNodes[i].ToggleDropHandler(state);
			}
		}
	}

	public UIWireNodeView GetNodeView(string withTypeName, string withMemberName)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "GetNodeView", withTypeName, withMemberName);
		}
		string text = withTypeName + "." + withMemberName;
		if (!spawnedNodeLookUp.ContainsKey(text))
		{
			DebugUtility.LogError(this, "GetNodeView", "Could not find that node!", text);
			return null;
		}
		return spawnedNodeLookUp[text];
	}

	public void UpdateWireCounts()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateWireCounts");
		}
		for (int i = 0; i < spawnedNodes.Count; i++)
		{
			spawnedNodes[i].UpdateWireCount();
		}
	}

	public void Hide()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Hide");
		}
		IsOpen = false;
		TargetTransform = null;
		TargetId = SerializableGuid.Empty;
		nodeEvents.Clear();
		hideTweens.Tween();
		OnHide.Invoke();
		MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
	}

	private void ToggleOff()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleOff");
		}
		base.gameObject.SetActive(value: false);
	}

	private void DespawnSpawnedNodes()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DespawnSpawnedNodes");
		}
		for (int i = 0; i < spawnedNodes.Count; i++)
		{
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(spawnedNodes[i]);
		}
		spawnedNodes.Clear();
		spawnedNodeLookUp.Clear();
	}

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
}
