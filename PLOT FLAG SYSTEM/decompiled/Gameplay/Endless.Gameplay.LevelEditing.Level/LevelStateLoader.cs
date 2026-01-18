using Endless.Assets;
using Endless.Gameplay.Serialization;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

public static class LevelStateLoader
{
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
			foreach (SemanticVersion versionUpgrade in EndlessTypeMapping.Instance.VersionUpgrades)
			{
				if (versionUpgrade <= semanticVersion)
				{
					continue;
				}
				if (versionUpgrade > LevelState.INTERNAL_VERSION)
				{
					break;
				}
				Debug.Log($"Upgrading level state to {versionUpgrade}");
				foreach (PropEntry propEntry in levelState.PropEntries)
				{
					foreach (ComponentEntry componentEntry in propEntry.ComponentEntries)
					{
						foreach (MemberChange change in componentEntry.Changes)
						{
							UpdateMemberChange(change, versionUpgrade, isLua: false);
						}
					}
					foreach (MemberChange luaMemberChange in propEntry.LuaMemberChanges)
					{
						UpdateMemberChange(luaMemberChange, versionUpgrade, isLua: true);
					}
				}
				levelState.InternalVersion = versionUpgrade.ToString();
			}
			levelState.InternalVersion = LevelState.INTERNAL_VERSION.ToString();
		}
		return levelState;
	}

	private static void UpdateMemberChange(MemberChange memberChange, SemanticVersion version, bool isLua)
	{
		EndlessTypeMapping.Instance.Upgrade(memberChange, version, isLua);
	}
}
