using System;
using System.Collections.Generic;
using Endless.Gameplay.Stats;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Endless.Gameplay.UI;

public class UIPerPlayerStatGrid : UIGameObject, IPoolableT
{
	[SerializeField]
	private UILayoutElement layoutElement;

	[SerializeField]
	private RectTransform playerIconParent;

	[SerializeField]
	private UserPortrait playerIconPrefab;

	[SerializeField]
	private RectTransform statLabelParent;

	[SerializeField]
	private UIText statLabelPrefab;

	[SerializeField]
	private GridLayoutGroup valueGrid;

	[FormerlySerializedAs("valueGridTransform")]
	[SerializeField]
	private RectTransform valueGridParent;

	[SerializeField]
	private UIText valuePrefab;

	[SerializeField]
	private TweenCollection displayTween;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private readonly List<UserPortrait> spawnedUserPortraits = new List<UserPortrait>();

	private readonly List<UIText> spawnedTexts = new List<UIText>();

	public bool IsUi => true;

	public MonoBehaviour Prefab { get; set; }

	public void OnDespawn()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDespawn");
		}
		spawnedUserPortraits.DespawnAllItemsAndClear();
		spawnedTexts.DespawnAllItemsAndClear();
	}

	public void Initialize(PerPlayerStat[] perPlayerStats)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", perPlayerStats.Length);
		}
		HashSet<int> hashSet = new HashSet<int>();
		if (verboseLogging)
		{
			Debug.Log($"I found {hashSet.Count} unique player Id");
		}
		foreach (int connectedUserId in NetworkBehaviourSingleton<UserIdManager>.Instance.ConnectedUserIds)
		{
			hashSet.Add(connectedUserId);
		}
		for (int i = 0; i < perPlayerStats.Length; i++)
		{
			int[] userIds = perPlayerStats[i].GetUserIds();
			for (int j = 0; j < userIds.Length; j++)
			{
				hashSet.Add(userIds[j]);
			}
		}
		if (verboseLogging)
		{
			Debug.Log($"I found {hashSet.Count} unique player Id");
		}
		int count = hashSet.Count;
		if (count == 0)
		{
			Debug.LogWarning("No users found for per-player stat grid");
			return;
		}
		float num = ((count > 1) ? (valueGrid.spacing.x * (float)(count - 1)) : 0f);
		float x = valueGrid.cellSize.x * (float)count + num;
		playerIconParent.sizeDelta = new Vector2(x, playerIconParent.sizeDelta.y);
		valueGridParent.sizeDelta = new Vector2(x, valueGridParent.sizeDelta.y);
		float num2 = playerIconParent.sizeDelta.y + 25f;
		float num3 = ((perPlayerStats.Length > 1) ? (valueGrid.spacing.y * (float)(perPlayerStats.Length - 1)) : 0f);
		layoutElement.PreferredHeightLayoutDimension.ExplicitValue = num2 + (float)perPlayerStats.Length * valueGrid.cellSize.y + num3;
		foreach (int item in hashSet)
		{
			PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
			UserPortrait prefab = playerIconPrefab;
			Transform parent = playerIconParent;
			UserPortrait userPortrait = instance.Spawn(prefab, default(Vector3), default(Quaternion), parent);
			userPortrait.Initialize(item, showHostAndParty: false);
			spawnedUserPortraits.Add(userPortrait);
		}
		for (int k = 0; k < perPlayerStats.Length; k++)
		{
			string localizedString = perPlayerStats[k].Message.GetLocalizedString();
			PoolManagerT instance2 = MonoBehaviourSingleton<PoolManagerT>.Instance;
			UIText prefab2 = statLabelPrefab;
			Transform parent = statLabelParent;
			UIText uIText = instance2.Spawn(prefab2, default(Vector3), default(Quaternion), parent);
			uIText.Value = localizedString;
			spawnedTexts.Add(uIText);
		}
		foreach (PerPlayerStat perPlayerStat in perPlayerStats)
		{
			foreach (int item2 in hashSet)
			{
				PoolManagerT instance3 = MonoBehaviourSingleton<PoolManagerT>.Instance;
				UIText prefab3 = valuePrefab;
				Transform parent = valueGridParent;
				UIText uIText2 = instance3.Spawn(prefab3, default(Vector3), default(Quaternion), parent);
				spawnedTexts.Add(uIText2);
				if (perPlayerStat.TryGetValue(item2, out var value))
				{
					uIText2.Value = StatBase.GetFormattedString(value, perPlayerStat.DisplayFormat);
					continue;
				}
				if (verboseLogging)
				{
					Debug.Log($"No value found for {item2}, default is '{perPlayerStat.DefaultValue}'.");
				}
				if (string.IsNullOrEmpty(perPlayerStat.DefaultValue))
				{
					uIText2.Value = "-";
				}
				else
				{
					uIText2.Value = perPlayerStat.DefaultValue;
				}
			}
		}
	}

	public void PlayDisplayTween(float delay = 0f, Action onComplete = null)
	{
		displayTween.SetDelay(delay);
		displayTween.Tween(onComplete);
	}
}
