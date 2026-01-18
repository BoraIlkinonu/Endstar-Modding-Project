using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000369 RID: 873
	public abstract class CosmeticIconRendererBase<TIcon> : MonoBehaviour where TIcon : MonoBehaviour, IPoolableT
	{
		// Token: 0x0600166C RID: 5740 RVA: 0x0006955B File Offset: 0x0006775B
		protected virtual void Awake()
		{
			if (this.spanFinder == null)
			{
				this.spanFinder = base.GetComponent<InlineTextSpanFinder>();
			}
			if (this.spanFinder != null)
			{
				this.spanFinder.SpansReady += this.ApplySpans;
			}
		}

		// Token: 0x0600166D RID: 5741 RVA: 0x00069596 File Offset: 0x00067796
		protected virtual void OnDestroy()
		{
			if (this.spanFinder != null)
			{
				this.spanFinder.SpansReady -= this.ApplySpans;
			}
		}

		// Token: 0x0600166E RID: 5742 RVA: 0x000695BD File Offset: 0x000677BD
		protected virtual void Start()
		{
			if (MonoBehaviourSingleton<PoolManagerT>.Instance != null && this.iconPrefab != null)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.PrewarmPool<TIcon>(this.iconPrefab, 4);
			}
		}

		// Token: 0x0600166F RID: 5743 RVA: 0x000695F0 File Offset: 0x000677F0
		private void ApplySpans(IReadOnlyList<InlineSpan> spans, TMP_Text source)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance == null || MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary == null)
			{
				return;
			}
			HashSet<int> hashSet = new HashSet<int>();
			int num = 0;
			for (int i = 0; i < spans.Count; i++)
			{
				InlineSpan inlineSpan = spans[i];
				if (!(inlineSpan.scheme != this.scheme))
				{
					hashSet.Add(inlineSpan.linkIndex);
					TIcon ticon;
					if (!this.activeIcons.TryGetValue(inlineSpan.linkIndex, out ticon))
					{
						ticon = this.SpawnIcon(inlineSpan, source, num);
						this.activeIcons[inlineSpan.linkIndex] = ticon;
					}
					this.UpdateIcon(ticon, inlineSpan, source, num);
					num++;
				}
			}
			CosmeticIconRendererBase<TIcon>.tempKeys.Clear();
			foreach (KeyValuePair<int, TIcon> keyValuePair in this.activeIcons)
			{
				if (!hashSet.Contains(keyValuePair.Key))
				{
					CosmeticIconRendererBase<TIcon>.tempKeys.Add(keyValuePair.Key);
				}
			}
			foreach (int num2 in CosmeticIconRendererBase<TIcon>.tempKeys)
			{
				this.DespawnIcon(this.activeIcons[num2]);
				this.activeIcons.Remove(num2);
			}
		}

		// Token: 0x06001670 RID: 5744 RVA: 0x00069768 File Offset: 0x00067968
		protected virtual TIcon SpawnIcon(InlineSpan span, TMP_Text source, int displayIndex)
		{
			return MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<TIcon>(this.iconPrefab, default(Vector3), default(Quaternion), null);
		}

		// Token: 0x06001671 RID: 5745 RVA: 0x00069798 File Offset: 0x00067998
		protected virtual void DespawnIcon(TIcon icon)
		{
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<TIcon>(icon);
		}

		// Token: 0x06001672 RID: 5746
		protected abstract void UpdateIcon(TIcon icon, InlineSpan span, TMP_Text source, int displayIndex);

		// Token: 0x06001673 RID: 5747 RVA: 0x000697A8 File Offset: 0x000679A8
		protected bool TryParseCompoundKey(string key, out SerializableGuid guid, out int? number)
		{
			guid = SerializableGuid.Empty;
			number = null;
			if (string.IsNullOrEmpty(key))
			{
				return false;
			}
			Match match = CosmeticIconRendererBase<TIcon>.guidIntPattern.Match(key);
			if (!match.Success)
			{
				return false;
			}
			guid = match.Groups[1].Value;
			int num;
			if (match.Groups[2].Success && int.TryParse(match.Groups[2].Value, out num))
			{
				number = new int?(num);
			}
			return true;
		}

		// Token: 0x06001674 RID: 5748 RVA: 0x00069840 File Offset: 0x00067A40
		protected void ResolveSpriteAndQuantity(InlineSpan span, out Sprite sprite, out string quantityText)
		{
			sprite = MonoBehaviourSingleton<DefaultContentManager>.Instance.MissingPropDisplayIcon;
			quantityText = "";
			SerializableGuid serializableGuid;
			int? num;
			if (!this.TryParseCompoundKey(span.key, out serializableGuid, out num))
			{
				return;
			}
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(serializableGuid, out runtimePropInfo))
			{
				sprite = runtimePropInfo.Icon;
			}
			quantityText = ((num == null) ? "" : ("x" + StringUtility.AbbreviateQuantity(num.Value)));
		}

		// Token: 0x04001216 RID: 4630
		private static readonly Regex guidIntPattern = new Regex("^([0-9a-fA-F-]+)(?:#(\\d+))?$", RegexOptions.Compiled);

		// Token: 0x04001217 RID: 4631
		[Header("Finder")]
		[Tooltip("Reference to the InlineTextSpanFinder that emits span events.")]
		[SerializeField]
		protected InlineTextSpanFinder spanFinder;

		// Token: 0x04001218 RID: 4632
		[Header("Filtering")]
		[Tooltip("Only spans with this scheme are rendered (e.g., \"cosmetic\").")]
		[SerializeField]
		protected string scheme = "cosmetic";

		// Token: 0x04001219 RID: 4633
		[Header("Sizing")]
		[Tooltip("Uniform scale applied on top of the finder’s suggested side length.")]
		[SerializeField]
		protected float sizeMultiplier = 1f;

		// Token: 0x0400121A RID: 4634
		[Header("Prefab")]
		[SerializeField]
		protected TIcon iconPrefab;

		// Token: 0x0400121B RID: 4635
		protected readonly Dictionary<int, TIcon> activeIcons = new Dictionary<int, TIcon>();

		// Token: 0x0400121C RID: 4636
		private static readonly List<int> tempKeys = new List<int>();
	}
}
