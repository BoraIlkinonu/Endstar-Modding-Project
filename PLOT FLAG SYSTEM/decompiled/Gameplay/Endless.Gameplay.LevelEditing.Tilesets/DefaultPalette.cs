using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Serialization;

namespace Endless.Gameplay.LevelEditing.Tilesets;

[CreateAssetMenu(menuName = "ScriptableObject/Default Palette")]
public class DefaultPalette : ScriptableObject
{
	[FormerlySerializedAs("defaultTilesetEntries")]
	[SerializeField]
	private List<DefaultPaletteEntry> defaultEntries;

	public IReadOnlyList<DefaultPaletteEntry> DefaultEntries => defaultEntries;

	[ContextMenu("LoadPropsFromCurrentGame")]
	public void LoadPropsFromCurrentGame()
	{
		PropLibrary.RuntimePropInfo[] allRuntimeProps = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetAllRuntimeProps();
		foreach (PropLibrary.RuntimePropInfo runtimePropInfo in allRuntimeProps)
		{
			DefaultPaletteEntry defaultPaletteEntry = defaultEntries.FirstOrDefault((DefaultPaletteEntry entry) => entry.Id == runtimePropInfo.PropData.AssetID);
			if (defaultPaletteEntry != null)
			{
				defaultPaletteEntry.Name = runtimePropInfo.PropData.Name;
				continue;
			}
			defaultEntries.Add(new DefaultPaletteEntry
			{
				Name = runtimePropInfo.PropData.Name,
				Id = runtimePropInfo.PropData.AssetID
			});
		}
	}

	[ContextMenu("LoadTerrainFromCurrentGame")]
	public void LoadTerrainFromCurrentGame()
	{
		throw new NotImplementedException();
	}

	public string GetIdByName(string name)
	{
		name = name.ToLower();
		return defaultEntries.FirstOrDefault((DefaultPaletteEntry entry) => entry.Name.ToLower() == name)?.Id;
	}
}
