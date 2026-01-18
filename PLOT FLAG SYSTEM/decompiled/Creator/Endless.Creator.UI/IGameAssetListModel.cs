using System.Collections.Generic;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.UI;

public interface IGameAssetListModel
{
	GameObject GameObject { get; }

	AssetContexts Context { get; }

	void Synchronize();

	void SetAssetIdsToIgnore(HashSet<SerializableGuid> assetIdsToIgnore);

	void SetAssetTypeFilter(UIGameAssetTypes value, bool triggerRequest);
}
