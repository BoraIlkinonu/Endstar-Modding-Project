using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Endless.Gameplay.Stats;
using Endless.Shared;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class BasicStatEntry : UIGameObject, IPoolableT
{
	[SerializeField]
	private UIText textPrefab;

	[SerializeField]
	private UserPortrait userPortraitPrefab;

	[SerializeField]
	private RectTransform statSectionParent;

	[SerializeField]
	private TweenCollection displayTween;

	private readonly List<UIText> spawnedText = new List<UIText>();

	private readonly List<UserPortrait> spawnedUserPortraits = new List<UserPortrait>();

	public bool IsUi => true;

	public MonoBehaviour Prefab { get; set; }

	public void OnDespawn()
	{
		spawnedText.DespawnAllItemsAndClear();
		spawnedUserPortraits.DespawnAllItemsAndClear();
	}

	public async void Initialize(BasicStat basicStat)
	{
		string pattern = "(%[^%]+%)";
		string[] array = Regex.Split(basicStat.Message.GetLocalizedString(), pattern);
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!string.IsNullOrEmpty(text))
			{
				list.Add(text);
			}
		}
		bool foundValue = false;
		foreach (string item in list)
		{
			if (item.StartsWith('%') && item.EndsWith('%'))
			{
				switch (item)
				{
				case "%PLAYERNAME%":
					if (basicStat.UserId != 0)
					{
						string text2;
						if (basicStat.UserId > 0)
						{
							PoolManagerT instance2 = MonoBehaviourSingleton<PoolManagerT>.Instance;
							UserPortrait prefab2 = userPortraitPrefab;
							Transform parent = statSectionParent;
							UserPortrait userPortrait2 = instance2.Spawn(prefab2, default(Vector3), default(Quaternion), parent);
							spawnedUserPortraits.Add(userPortrait2);
							userPortrait2.Initialize(basicStat.UserId, showHostAndParty: false);
							text2 = await MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserName(basicStat.UserId);
						}
						else
						{
							text2 = "Someone";
						}
						SpawnNewText(text2);
					}
					break;
				case "%PLAYER%":
					if (basicStat.UserId != 0)
					{
						PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
						UserPortrait prefab = userPortraitPrefab;
						Transform parent = statSectionParent;
						UserPortrait userPortrait = instance.Spawn(prefab, default(Vector3), default(Quaternion), parent);
						userPortrait.Initialize(basicStat.UserId, showHostAndParty: false);
						spawnedUserPortraits.Add(userPortrait);
					}
					break;
				case "%VALUE%":
					foundValue = true;
					SpawnNewText(basicStat.Value);
					break;
				}
			}
			else
			{
				SpawnNewText(item);
			}
		}
		if (!foundValue && !string.IsNullOrWhiteSpace(basicStat.Value))
		{
			PoolManagerT instance3 = MonoBehaviourSingleton<PoolManagerT>.Instance;
			UIText prefab3 = textPrefab;
			Transform parent = statSectionParent;
			UIText uIText = instance3.Spawn(prefab3, default(Vector3), default(Quaternion), parent);
			uIText.Value = basicStat.Value;
			spawnedText.Add(uIText);
		}
	}

	public void PlayDisplayTween(float delay = 0f, Action onComplete = null)
	{
		displayTween.SetDelay(delay);
		displayTween.Tween(onComplete);
	}

	private void SpawnNewText(string text)
	{
		PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
		UIText prefab = textPrefab;
		Transform parent = statSectionParent;
		UIText uIText = instance.Spawn(prefab, default(Vector3), default(Quaternion), parent);
		uIText.Value = text;
		spawnedText.Add(uIText);
	}
}
