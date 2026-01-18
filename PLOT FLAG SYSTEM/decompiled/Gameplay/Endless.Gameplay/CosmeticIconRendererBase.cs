using System.Collections.Generic;
using System.Text.RegularExpressions;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay;

public abstract class CosmeticIconRendererBase<TIcon> : MonoBehaviour where TIcon : MonoBehaviour, IPoolableT
{
	private static readonly Regex guidIntPattern = new Regex("^([0-9a-fA-F-]+)(?:#(\\d+))?$", RegexOptions.Compiled);

	[Header("Finder")]
	[Tooltip("Reference to the InlineTextSpanFinder that emits span events.")]
	[SerializeField]
	protected InlineTextSpanFinder spanFinder;

	[Header("Filtering")]
	[Tooltip("Only spans with this scheme are rendered (e.g., \"cosmetic\").")]
	[SerializeField]
	protected string scheme = "cosmetic";

	[Header("Sizing")]
	[Tooltip("Uniform scale applied on top of the finder’s suggested side length.")]
	[SerializeField]
	protected float sizeMultiplier = 1f;

	[Header("Prefab")]
	[SerializeField]
	protected TIcon iconPrefab;

	protected readonly Dictionary<int, TIcon> activeIcons = new Dictionary<int, TIcon>();

	private static readonly List<int> tempKeys = new List<int>();

	protected virtual void Awake()
	{
		if ((object)spanFinder == null)
		{
			spanFinder = GetComponent<InlineTextSpanFinder>();
		}
		if (spanFinder != null)
		{
			spanFinder.SpansReady += ApplySpans;
		}
	}

	protected virtual void OnDestroy()
	{
		if (spanFinder != null)
		{
			spanFinder.SpansReady -= ApplySpans;
		}
	}

	protected virtual void Start()
	{
		if (MonoBehaviourSingleton<PoolManagerT>.Instance != null && iconPrefab != null)
		{
			MonoBehaviourSingleton<PoolManagerT>.Instance.PrewarmPool(iconPrefab, 4);
		}
	}

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
			InlineSpan span = spans[i];
			if (!(span.scheme != scheme))
			{
				hashSet.Add(span.linkIndex);
				if (!activeIcons.TryGetValue(span.linkIndex, out var value))
				{
					value = SpawnIcon(span, source, num);
					activeIcons[span.linkIndex] = value;
				}
				UpdateIcon(value, span, source, num);
				num++;
			}
		}
		tempKeys.Clear();
		foreach (KeyValuePair<int, TIcon> activeIcon in activeIcons)
		{
			if (!hashSet.Contains(activeIcon.Key))
			{
				tempKeys.Add(activeIcon.Key);
			}
		}
		foreach (int tempKey in tempKeys)
		{
			DespawnIcon(activeIcons[tempKey]);
			activeIcons.Remove(tempKey);
		}
	}

	protected virtual TIcon SpawnIcon(InlineSpan span, TMP_Text source, int displayIndex)
	{
		return MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn(iconPrefab);
	}

	protected virtual void DespawnIcon(TIcon icon)
	{
		MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(icon);
	}

	protected abstract void UpdateIcon(TIcon icon, InlineSpan span, TMP_Text source, int displayIndex);

	protected bool TryParseCompoundKey(string key, out SerializableGuid guid, out int? number)
	{
		guid = SerializableGuid.Empty;
		number = null;
		if (string.IsNullOrEmpty(key))
		{
			return false;
		}
		Match match = guidIntPattern.Match(key);
		if (!match.Success)
		{
			return false;
		}
		guid = match.Groups[1].Value;
		if (match.Groups[2].Success && int.TryParse(match.Groups[2].Value, out var result))
		{
			number = result;
		}
		return true;
	}

	protected void ResolveSpriteAndQuantity(InlineSpan span, out Sprite sprite, out string quantityText)
	{
		sprite = MonoBehaviourSingleton<DefaultContentManager>.Instance.MissingPropDisplayIcon;
		quantityText = "";
		if (TryParseCompoundKey(span.key, out var guid, out var number))
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(guid, out var metadata))
			{
				sprite = metadata.Icon;
			}
			quantityText = ((!number.HasValue) ? "" : ("x" + StringUtility.AbbreviateQuantity(number.Value)));
		}
	}
}
