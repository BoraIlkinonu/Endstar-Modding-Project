using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x0200040A RID: 1034
	public static class UIIEnumerablePresenterFiltersGameplay
	{
		// Token: 0x060019D7 RID: 6615 RVA: 0x00076C54 File Offset: 0x00074E54
		public static void Register()
		{
			foreach (KeyValuePair<Type, Func<string, object, bool>> keyValuePair in UIIEnumerablePresenterFiltersGameplay.typeFilterDictionary)
			{
				UIIEnumerablePresenter.RegisterTypeFilter(keyValuePair.Key, keyValuePair.Value);
			}
		}

		// Token: 0x060019D8 RID: 6616 RVA: 0x00076CB4 File Offset: 0x00074EB4
		public static bool FilterPropEntry(string filter, object item)
		{
			if (string.IsNullOrEmpty(filter) || item == null)
			{
				return false;
			}
			PropEntry propEntry = item as PropEntry;
			return propEntry != null && propEntry.Label.Contains(filter, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x060019D9 RID: 6617 RVA: 0x00076CE8 File Offset: 0x00074EE8
		public static bool FilterCharacterCosmeticsDefinition(string filter, object item)
		{
			if (string.IsNullOrEmpty(filter) || item == null)
			{
				return false;
			}
			CharacterCosmeticsDefinition characterCosmeticsDefinition = item as CharacterCosmeticsDefinition;
			return characterCosmeticsDefinition != null && !characterCosmeticsDefinition.IsMissingAsset && characterCosmeticsDefinition.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x060019DA RID: 6618 RVA: 0x00076D28 File Offset: 0x00074F28
		public static bool FilterAudioReference(string filter, object item)
		{
			if (string.IsNullOrEmpty(filter) || item == null)
			{
				return false;
			}
			AudioReference audioReference = item as AudioReference;
			RuntimeAudioInfo runtimeAudioInfo;
			return audioReference != null && MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(audioReference.Id, out runtimeAudioInfo) && runtimeAudioInfo.AudioAsset.Name.Contains(filter, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x060019DB RID: 6619 RVA: 0x00076D7C File Offset: 0x00074F7C
		public static bool FilterRuntimePropInfo(string filter, object item)
		{
			if (string.IsNullOrEmpty(filter) || item == null)
			{
				return false;
			}
			PropLibrary.RuntimePropInfo runtimePropInfo = item as PropLibrary.RuntimePropInfo;
			return runtimePropInfo != null && runtimePropInfo.EndlessProp.Prop.Name.Contains(filter, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x04001486 RID: 5254
		private const bool VERBOSE_LOGGING = false;

		// Token: 0x04001487 RID: 5255
		private static readonly Dictionary<Type, Func<string, object, bool>> typeFilterDictionary = new Dictionary<Type, Func<string, object, bool>>
		{
			{
				typeof(PropEntry),
				new Func<string, object, bool>(UIIEnumerablePresenterFiltersGameplay.FilterPropEntry)
			},
			{
				typeof(CharacterCosmeticsDefinition),
				new Func<string, object, bool>(UIIEnumerablePresenterFiltersGameplay.FilterCharacterCosmeticsDefinition)
			},
			{
				typeof(AudioReference),
				new Func<string, object, bool>(UIIEnumerablePresenterFiltersGameplay.FilterAudioReference)
			},
			{
				typeof(PropLibrary.RuntimePropInfo),
				new Func<string, object, bool>(UIIEnumerablePresenterFiltersGameplay.FilterRuntimePropInfo)
			}
		};
	}
}
