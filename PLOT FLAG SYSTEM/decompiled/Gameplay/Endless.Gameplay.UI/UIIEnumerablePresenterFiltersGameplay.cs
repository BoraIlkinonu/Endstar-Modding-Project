using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.UI;

namespace Endless.Gameplay.UI;

public static class UIIEnumerablePresenterFiltersGameplay
{
	private const bool VERBOSE_LOGGING = false;

	private static readonly Dictionary<Type, Func<string, object, bool>> typeFilterDictionary;

	static UIIEnumerablePresenterFiltersGameplay()
	{
		typeFilterDictionary = new Dictionary<Type, Func<string, object, bool>>
		{
			{
				typeof(PropEntry),
				FilterPropEntry
			},
			{
				typeof(CharacterCosmeticsDefinition),
				FilterCharacterCosmeticsDefinition
			},
			{
				typeof(AudioReference),
				FilterAudioReference
			},
			{
				typeof(PropLibrary.RuntimePropInfo),
				FilterRuntimePropInfo
			}
		};
	}

	public static void Register()
	{
		foreach (KeyValuePair<Type, Func<string, object, bool>> item in typeFilterDictionary)
		{
			UIIEnumerablePresenter.RegisterTypeFilter(item.Key, item.Value);
		}
	}

	public static bool FilterPropEntry(string filter, object item)
	{
		if (string.IsNullOrEmpty(filter) || item == null)
		{
			return false;
		}
		if (item is PropEntry propEntry)
		{
			return propEntry.Label.Contains(filter, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public static bool FilterCharacterCosmeticsDefinition(string filter, object item)
	{
		if (string.IsNullOrEmpty(filter) || item == null)
		{
			return false;
		}
		if (item is CharacterCosmeticsDefinition characterCosmeticsDefinition)
		{
			if (characterCosmeticsDefinition.IsMissingAsset)
			{
				return false;
			}
			return characterCosmeticsDefinition.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public static bool FilterAudioReference(string filter, object item)
	{
		if (string.IsNullOrEmpty(filter) || item == null)
		{
			return false;
		}
		if (item is AudioReference audioReference)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(audioReference.Id, out var metadata))
			{
				return metadata.AudioAsset.Name.Contains(filter, StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}
		return false;
	}

	public static bool FilterRuntimePropInfo(string filter, object item)
	{
		if (string.IsNullOrEmpty(filter) || item == null)
		{
			return false;
		}
		if (item is PropLibrary.RuntimePropInfo runtimePropInfo)
		{
			return runtimePropInfo.EndlessProp.Prop.Name.Contains(filter, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}
}
