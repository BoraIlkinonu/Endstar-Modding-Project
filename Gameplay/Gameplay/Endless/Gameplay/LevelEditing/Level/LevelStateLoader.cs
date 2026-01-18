using System;
using Endless.Assets;
using Endless.Gameplay.Serialization;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000557 RID: 1367
	public static class LevelStateLoader
	{
		// Token: 0x060020EF RID: 8431 RVA: 0x000946B0 File Offset: 0x000928B0
		public static LevelState Load(string assetJson)
		{
			SemanticVersion semanticVersion = SemanticVersion.Parse(JsonConvert.DeserializeObject<Asset>(assetJson).InternalVersion);
			SemanticVersion semanticVersion2 = new SemanticVersion(1, 0, 0);
			LevelState levelState;
			if (semanticVersion < semanticVersion2)
			{
				Debug.Log(string.Format("{0} was older version {1}. Upgrading to {2}", "LevelState", semanticVersion, semanticVersion2));
				levelState = LevelState.Upgrade(JsonConvert.DeserializeObject<LevelState_0_0>(assetJson));
				semanticVersion = SemanticVersion.Parse(levelState.InternalVersion);
			}
			else
			{
				levelState = JsonConvert.DeserializeObject<LevelState>(assetJson);
			}
			if (semanticVersion < LevelState.INTERNAL_VERSION)
			{
				foreach (SemanticVersion semanticVersion3 in EndlessTypeMapping.Instance.VersionUpgrades)
				{
					if (!(semanticVersion3 <= semanticVersion))
					{
						if (semanticVersion3 > LevelState.INTERNAL_VERSION)
						{
							break;
						}
						Debug.Log(string.Format("Upgrading level state to {0}", semanticVersion3));
						foreach (PropEntry propEntry in levelState.PropEntries)
						{
							foreach (ComponentEntry componentEntry in propEntry.ComponentEntries)
							{
								foreach (MemberChange memberChange in componentEntry.Changes)
								{
									LevelStateLoader.UpdateMemberChange(memberChange, semanticVersion3, false);
								}
							}
							foreach (MemberChange memberChange2 in propEntry.LuaMemberChanges)
							{
								LevelStateLoader.UpdateMemberChange(memberChange2, semanticVersion3, true);
							}
						}
						levelState.InternalVersion = semanticVersion3.ToString();
					}
				}
				levelState.InternalVersion = LevelState.INTERNAL_VERSION.ToString();
			}
			return levelState;
		}

		// Token: 0x060020F0 RID: 8432 RVA: 0x000948FC File Offset: 0x00092AFC
		private static void UpdateMemberChange(MemberChange memberChange, SemanticVersion version, bool isLua)
		{
			EndlessTypeMapping.Instance.Upgrade(memberChange, version, isLua);
		}
	}
}
