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

namespace Endless.Gameplay.UI
{
	// Token: 0x0200041A RID: 1050
	public class UIPerPlayerStatGrid : UIGameObject, IPoolableT
	{
		// Token: 0x17000543 RID: 1347
		// (get) Token: 0x06001A18 RID: 6680 RVA: 0x00017586 File Offset: 0x00015786
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000544 RID: 1348
		// (get) Token: 0x06001A19 RID: 6681 RVA: 0x000781A9 File Offset: 0x000763A9
		// (set) Token: 0x06001A1A RID: 6682 RVA: 0x000781B1 File Offset: 0x000763B1
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x06001A1B RID: 6683 RVA: 0x000781BA File Offset: 0x000763BA
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDespawn", Array.Empty<object>());
			}
			this.spawnedUserPortraits.DespawnAllItemsAndClear<UserPortrait>();
			this.spawnedTexts.DespawnAllItemsAndClear<UIText>();
		}

		// Token: 0x06001A1C RID: 6684 RVA: 0x000781EC File Offset: 0x000763EC
		public void Initialize(PerPlayerStat[] perPlayerStats)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { perPlayerStats.Length });
			}
			HashSet<int> hashSet = new HashSet<int>();
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("I found {0} unique player Id", hashSet.Count));
			}
			foreach (int num in NetworkBehaviourSingleton<UserIdManager>.Instance.ConnectedUserIds)
			{
				hashSet.Add(num);
			}
			for (int i = 0; i < perPlayerStats.Length; i++)
			{
				int[] userIds = perPlayerStats[i].GetUserIds();
				for (int j = 0; j < userIds.Length; j++)
				{
					hashSet.Add(userIds[j]);
				}
			}
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("I found {0} unique player Id", hashSet.Count));
			}
			int count = hashSet.Count;
			if (count == 0)
			{
				Debug.LogWarning("No users found for per-player stat grid");
				return;
			}
			float num2 = ((count > 1) ? (this.valueGrid.spacing.x * (float)(count - 1)) : 0f);
			float num3 = this.valueGrid.cellSize.x * (float)count + num2;
			this.playerIconParent.sizeDelta = new Vector2(num3, this.playerIconParent.sizeDelta.y);
			this.valueGridParent.sizeDelta = new Vector2(num3, this.valueGridParent.sizeDelta.y);
			float num4 = this.playerIconParent.sizeDelta.y + 25f;
			float num5 = ((perPlayerStats.Length > 1) ? (this.valueGrid.spacing.y * (float)(perPlayerStats.Length - 1)) : 0f);
			this.layoutElement.PreferredHeightLayoutDimension.ExplicitValue = num4 + (float)perPlayerStats.Length * this.valueGrid.cellSize.y + num5;
			foreach (int num6 in hashSet)
			{
				PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
				UserPortrait userPortrait = this.playerIconPrefab;
				Transform transform = this.playerIconParent;
				UserPortrait userPortrait2 = instance.Spawn<UserPortrait>(userPortrait, default(Vector3), default(Quaternion), transform);
				userPortrait2.Initialize(num6, false);
				this.spawnedUserPortraits.Add(userPortrait2);
			}
			for (int k = 0; k < perPlayerStats.Length; k++)
			{
				string localizedString = perPlayerStats[k].Message.GetLocalizedString();
				PoolManagerT instance2 = MonoBehaviourSingleton<PoolManagerT>.Instance;
				UIText uitext = this.statLabelPrefab;
				Transform transform = this.statLabelParent;
				UIText uitext2 = instance2.Spawn<UIText>(uitext, default(Vector3), default(Quaternion), transform);
				uitext2.Value = localizedString;
				this.spawnedTexts.Add(uitext2);
			}
			foreach (PerPlayerStat perPlayerStat in perPlayerStats)
			{
				foreach (int num7 in hashSet)
				{
					PoolManagerT instance3 = MonoBehaviourSingleton<PoolManagerT>.Instance;
					UIText uitext3 = this.valuePrefab;
					Transform transform = this.valueGridParent;
					UIText uitext4 = instance3.Spawn<UIText>(uitext3, default(Vector3), default(Quaternion), transform);
					this.spawnedTexts.Add(uitext4);
					float num8;
					if (perPlayerStat.TryGetValue(num7, out num8))
					{
						uitext4.Value = StatBase.GetFormattedString(num8, perPlayerStat.DisplayFormat);
					}
					else
					{
						if (this.verboseLogging)
						{
							Debug.Log(string.Format("No value found for {0}, default is '{1}'.", num7, perPlayerStat.DefaultValue));
						}
						if (string.IsNullOrEmpty(perPlayerStat.DefaultValue))
						{
							uitext4.Value = "-";
						}
						else
						{
							uitext4.Value = perPlayerStat.DefaultValue;
						}
					}
				}
			}
		}

		// Token: 0x06001A1D RID: 6685 RVA: 0x000785DC File Offset: 0x000767DC
		public void PlayDisplayTween(float delay = 0f, Action onComplete = null)
		{
			this.displayTween.SetDelay(delay);
			this.displayTween.Tween(onComplete);
		}

		// Token: 0x040014D5 RID: 5333
		[SerializeField]
		private UILayoutElement layoutElement;

		// Token: 0x040014D6 RID: 5334
		[SerializeField]
		private RectTransform playerIconParent;

		// Token: 0x040014D7 RID: 5335
		[SerializeField]
		private UserPortrait playerIconPrefab;

		// Token: 0x040014D8 RID: 5336
		[SerializeField]
		private RectTransform statLabelParent;

		// Token: 0x040014D9 RID: 5337
		[SerializeField]
		private UIText statLabelPrefab;

		// Token: 0x040014DA RID: 5338
		[SerializeField]
		private GridLayoutGroup valueGrid;

		// Token: 0x040014DB RID: 5339
		[FormerlySerializedAs("valueGridTransform")]
		[SerializeField]
		private RectTransform valueGridParent;

		// Token: 0x040014DC RID: 5340
		[SerializeField]
		private UIText valuePrefab;

		// Token: 0x040014DD RID: 5341
		[SerializeField]
		private TweenCollection displayTween;

		// Token: 0x040014DE RID: 5342
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040014DF RID: 5343
		private readonly List<UserPortrait> spawnedUserPortraits = new List<UserPortrait>();

		// Token: 0x040014E0 RID: 5344
		private readonly List<UIText> spawnedTexts = new List<UIText>();
	}
}
