using System;
using Endless.Matchmaking;
using Endless.Shared;

namespace Endless.Assets
{
	// Token: 0x02000053 RID: 83
	public abstract class NetworkEnvironmentAssetListDictionary<TAssetList> : BaseEnumKeyScriptableObjectDictionary<NetworkEnvironment, AssetListScriptableObject<TAssetList>> where TAssetList : AssetList
	{
	}
}
