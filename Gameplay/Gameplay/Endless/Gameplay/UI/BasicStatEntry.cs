using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Endless.Gameplay.Stats;
using Endless.Shared;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000416 RID: 1046
	public class BasicStatEntry : UIGameObject, IPoolableT
	{
		// Token: 0x17000540 RID: 1344
		// (get) Token: 0x06001A00 RID: 6656 RVA: 0x00017586 File Offset: 0x00015786
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000541 RID: 1345
		// (get) Token: 0x06001A01 RID: 6657 RVA: 0x000773E1 File Offset: 0x000755E1
		// (set) Token: 0x06001A02 RID: 6658 RVA: 0x000773E9 File Offset: 0x000755E9
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x06001A03 RID: 6659 RVA: 0x000773F2 File Offset: 0x000755F2
		public void OnDespawn()
		{
			this.spawnedText.DespawnAllItemsAndClear<UIText>();
			this.spawnedUserPortraits.DespawnAllItemsAndClear<UserPortrait>();
		}

		// Token: 0x06001A04 RID: 6660 RVA: 0x0007740C File Offset: 0x0007560C
		public async void Initialize(BasicStat basicStat)
		{
			string text = "(%[^%]+%)";
			string[] array = Regex.Split(basicStat.Message.GetLocalizedString(), text);
			List<string> list = new List<string>();
			foreach (string text2 in array)
			{
				if (!string.IsNullOrEmpty(text2))
				{
					list.Add(text2);
				}
			}
			bool foundValue = false;
			foreach (string text3 in list)
			{
				if (text3.StartsWith('%') && text3.EndsWith('%'))
				{
					if (!(text3 == "%PLAYERNAME%"))
					{
						if (!(text3 == "%PLAYER%"))
						{
							if (text3 == "%VALUE%")
							{
								foundValue = true;
								this.SpawnNewText(basicStat.Value);
							}
						}
						else if (basicStat.UserId != 0)
						{
							PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
							UserPortrait userPortrait = this.userPortraitPrefab;
							Transform transform = this.statSectionParent;
							UserPortrait userPortrait2 = instance.Spawn<UserPortrait>(userPortrait, default(Vector3), default(Quaternion), transform);
							userPortrait2.Initialize(basicStat.UserId, false);
							this.spawnedUserPortraits.Add(userPortrait2);
						}
					}
					else if (basicStat.UserId != 0)
					{
						string text4;
						if (basicStat.UserId > 0)
						{
							PoolManagerT instance2 = MonoBehaviourSingleton<PoolManagerT>.Instance;
							UserPortrait userPortrait3 = this.userPortraitPrefab;
							Transform transform = this.statSectionParent;
							UserPortrait userPortrait4 = instance2.Spawn<UserPortrait>(userPortrait3, default(Vector3), default(Quaternion), transform);
							this.spawnedUserPortraits.Add(userPortrait4);
							userPortrait4.Initialize(basicStat.UserId, false);
							text4 = await MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserName(basicStat.UserId);
						}
						else
						{
							text4 = "Someone";
						}
						this.SpawnNewText(text4);
					}
				}
				else
				{
					this.SpawnNewText(text3);
				}
			}
			List<string>.Enumerator enumerator = default(List<string>.Enumerator);
			if (!foundValue && !string.IsNullOrWhiteSpace(basicStat.Value))
			{
				PoolManagerT instance3 = MonoBehaviourSingleton<PoolManagerT>.Instance;
				UIText uitext = this.textPrefab;
				Transform transform = this.statSectionParent;
				UIText uitext2 = instance3.Spawn<UIText>(uitext, default(Vector3), default(Quaternion), transform);
				uitext2.Value = basicStat.Value;
				this.spawnedText.Add(uitext2);
			}
		}

		// Token: 0x06001A05 RID: 6661 RVA: 0x0007744B File Offset: 0x0007564B
		public void PlayDisplayTween(float delay = 0f, Action onComplete = null)
		{
			this.displayTween.SetDelay(delay);
			this.displayTween.Tween(onComplete);
		}

		// Token: 0x06001A06 RID: 6662 RVA: 0x00077468 File Offset: 0x00075668
		private void SpawnNewText(string text)
		{
			PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
			UIText uitext = this.textPrefab;
			Transform transform = this.statSectionParent;
			UIText uitext2 = instance.Spawn<UIText>(uitext, default(Vector3), default(Quaternion), transform);
			uitext2.Value = text;
			this.spawnedText.Add(uitext2);
		}

		// Token: 0x040014A8 RID: 5288
		[SerializeField]
		private UIText textPrefab;

		// Token: 0x040014A9 RID: 5289
		[SerializeField]
		private UserPortrait userPortraitPrefab;

		// Token: 0x040014AA RID: 5290
		[SerializeField]
		private RectTransform statSectionParent;

		// Token: 0x040014AB RID: 5291
		[SerializeField]
		private TweenCollection displayTween;

		// Token: 0x040014AC RID: 5292
		private readonly List<UIText> spawnedText = new List<UIText>();

		// Token: 0x040014AD RID: 5293
		private readonly List<UserPortrait> spawnedUserPortraits = new List<UserPortrait>();
	}
}
