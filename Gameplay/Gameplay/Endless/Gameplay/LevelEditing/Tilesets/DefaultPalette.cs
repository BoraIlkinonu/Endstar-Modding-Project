using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Serialization;

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x0200050F RID: 1295
	[CreateAssetMenu(menuName = "ScriptableObject/Default Palette")]
	public class DefaultPalette : ScriptableObject
	{
		// Token: 0x1700060D RID: 1549
		// (get) Token: 0x06001F66 RID: 8038 RVA: 0x0008B423 File Offset: 0x00089623
		public IReadOnlyList<DefaultPaletteEntry> DefaultEntries
		{
			get
			{
				return this.defaultEntries;
			}
		}

		// Token: 0x06001F67 RID: 8039 RVA: 0x0008B42C File Offset: 0x0008962C
		[ContextMenu("LoadPropsFromCurrentGame")]
		public void LoadPropsFromCurrentGame()
		{
			PropLibrary.RuntimePropInfo[] allRuntimeProps = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetAllRuntimeProps();
			for (int i = 0; i < allRuntimeProps.Length; i++)
			{
				PropLibrary.RuntimePropInfo runtimePropInfo = allRuntimeProps[i];
				DefaultPaletteEntry defaultPaletteEntry = this.defaultEntries.FirstOrDefault((DefaultPaletteEntry entry) => entry.Id == runtimePropInfo.PropData.AssetID);
				if (defaultPaletteEntry != null)
				{
					defaultPaletteEntry.Name = runtimePropInfo.PropData.Name;
				}
				else
				{
					this.defaultEntries.Add(new DefaultPaletteEntry
					{
						Name = runtimePropInfo.PropData.Name,
						Id = runtimePropInfo.PropData.AssetID
					});
				}
			}
		}

		// Token: 0x06001F68 RID: 8040 RVA: 0x0008452B File Offset: 0x0008272B
		[ContextMenu("LoadTerrainFromCurrentGame")]
		public void LoadTerrainFromCurrentGame()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001F69 RID: 8041 RVA: 0x0008B4DC File Offset: 0x000896DC
		public string GetIdByName(string name)
		{
			name = name.ToLower();
			DefaultPaletteEntry defaultPaletteEntry = this.defaultEntries.FirstOrDefault((DefaultPaletteEntry entry) => entry.Name.ToLower() == name);
			if (defaultPaletteEntry == null)
			{
				return null;
			}
			return defaultPaletteEntry.Id;
		}

		// Token: 0x040018EF RID: 6383
		[FormerlySerializedAs("defaultTilesetEntries")]
		[SerializeField]
		private List<DefaultPaletteEntry> defaultEntries;
	}
}
