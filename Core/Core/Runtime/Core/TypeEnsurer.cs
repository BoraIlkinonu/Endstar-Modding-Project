using System;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.Assets;
using Endless.Shared.DataTypes;
using Newtonsoft.Json.Utilities;
using Runtime.Gameplay.LevelEditing;
using UnityEngine;
using UnityEngine.Scripting;

namespace Runtime.Core
{
	// Token: 0x0200000A RID: 10
	public class TypeEnsurer : MonoBehaviour
	{
		// Token: 0x06000027 RID: 39 RVA: 0x00002754 File Offset: 0x00000954
		[Preserve]
		private void EnsureTypes()
		{
			AotHelper.EnsureType<SerializableGuid>();
			AotHelper.EnsureType<Asset>();
			AotHelper.EnsureType<Game>();
			AotHelper.EnsureType<GameLibrary>();
			AotHelper.EnsureType<PropLocationOffset>();
			AotHelper.EnsureType<StringPairDictionary>();
			AotHelper.EnsureType<Prop>();
			AotHelper.EnsureType<Script>();
			AotHelper.EnsureType<EndlessPrefabAsset>();
			AotHelper.EnsureList<Asset>();
			AotHelper.EnsureList<AssetReference>();
			AotHelper.EnsureList<ScreenshotFileInstances>();
			AotHelper.EnsureList<PropLocationOffset>();
			AotHelper.EnsureList<string>();
			AotHelper.EnsureList<StringPair>();
			AotHelper.EnsureList<TerrainUsage>();
		}

		// Token: 0x06000028 RID: 40 RVA: 0x000027B1 File Offset: 0x000009B1
		private void Awake()
		{
			this.EnsureTypes();
		}
	}
}
