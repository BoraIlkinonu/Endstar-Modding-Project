using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Runtime.Gameplay.LevelEditing;
using UnityEngine;

namespace Endless.Creator
{
	// Token: 0x02000060 RID: 96
	[CreateAssetMenu(menuName = "ScriptableObject/Level States/Procedural Template Source", fileName = "Procedural Template Source")]
	public class ProceduralTemplateSource : LevelStateTemplateSourceBase
	{
		// Token: 0x06000156 RID: 342 RVA: 0x0000B752 File Offset: 0x00009952
		private ProceduralTemplateSource.Amplitude GetAmplitude()
		{
			if (this.AmplitudeConfiguration != ProceduralTemplateSource.Amplitude.Random)
			{
				return this.AmplitudeConfiguration;
			}
			return ProceduralTemplateSource.GetRandomEnum<ProceduralTemplateSource.Amplitude>(1, 0);
		}

		// Token: 0x06000157 RID: 343 RVA: 0x0000B76B File Offset: 0x0000996B
		private ProceduralTemplateSource.WorldSize GetWorldSize()
		{
			if (this.WorldSizeConfiguration != ProceduralTemplateSource.WorldSize.Random)
			{
				return this.WorldSizeConfiguration;
			}
			return ProceduralTemplateSource.GetRandomEnum<ProceduralTemplateSource.WorldSize>(1, 0);
		}

		// Token: 0x06000158 RID: 344 RVA: 0x0000B784 File Offset: 0x00009984
		private ProceduralTemplateSource.WorldHeight GetWorldHeight()
		{
			if (this.WorldHeightConfiguration != ProceduralTemplateSource.WorldHeight.Random)
			{
				return this.WorldHeightConfiguration;
			}
			return ProceduralTemplateSource.GetRandomEnum<ProceduralTemplateSource.WorldHeight>(1, 0);
		}

		// Token: 0x06000159 RID: 345 RVA: 0x0000B7A0 File Offset: 0x000099A0
		public override async Task<string> GetDisplayName()
		{
			return "Random Level";
		}

		// Token: 0x0600015A RID: 346 RVA: 0x0000B7DC File Offset: 0x000099DC
		public override Task<string> GetDescription()
		{
			ProceduralTemplateSource.<GetDescription>d__12 <GetDescription>d__;
			<GetDescription>d__.<>t__builder = AsyncTaskMethodBuilder<string>.Create();
			<GetDescription>d__.<>1__state = -1;
			<GetDescription>d__.<>t__builder.Start<ProceduralTemplateSource.<GetDescription>d__12>(ref <GetDescription>d__);
			return <GetDescription>d__.<>t__builder.Task;
		}

		// Token: 0x0600015B RID: 347 RVA: 0x0000B818 File Offset: 0x00009A18
		public override async Task<Sprite> GetDisplaySprite()
		{
			return this.displaySprite;
		}

		// Token: 0x0600015C RID: 348 RVA: 0x0000B85C File Offset: 0x00009A5C
		public override async Task<LevelState> GetLevelState(Game game)
		{
			LevelState levelState = new LevelState
			{
				AssetType = "level"
			};
			await this.BuildTerrain(game, levelState);
			return levelState;
		}

		// Token: 0x0600015D RID: 349 RVA: 0x0000B8A8 File Offset: 0x00009AA8
		private async Task BuildTerrain(Game game, LevelState levelState)
		{
			ProceduralTemplateSource.WorldSize worldSize = this.GetWorldSize();
			ProceduralTemplateSource.WorldHeight worldHeight = this.GetWorldHeight();
			ProceduralTemplateSource.Amplitude amplitude = this.GetAmplitude();
			Debug.Log(string.Concat(new string[]
			{
				"Creating template with size: ",
				worldSize.ToString(),
				", height: ",
				worldHeight.ToString(),
				", amplitude: ",
				amplitude.ToString()
			}));
			int num = (int)worldSize;
			int num2 = (int)((float)worldHeight * global::UnityEngine.Random.Range(0.95f, 1.05f));
			float num3 = (float)amplitude * global::UnityEngine.Random.Range(0.95f, 1.05f);
			Vector3 vector = new Vector3Int(num, num2, num);
			await this.GenerateGrassLandHills(game, levelState, vector, num3);
		}

		// Token: 0x0600015E RID: 350 RVA: 0x0000B8FC File Offset: 0x00009AFC
		private async Task GenerateGrassLandHills(Game game, LevelState levelState, Vector3 generationBounds, float scale)
		{
			string idByName = MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultTerrain.GetIdByName("Grass");
			string idByName2 = MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultTerrain.GetIdByName("Ancient Ruins");
			string idByName3 = MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultTerrain.GetIdByName("Dirt");
			int tilesetIdInGame = this.GetTilesetIdInGame(game, idByName);
			int stoneIndex = this.GetTilesetIdInGame(game, idByName2);
			int tilesetIdInGame2 = this.GetTilesetIdInGame(game, idByName3);
			float xOffset = global::UnityEngine.Random.Range(-10000f, 10000f);
			float yOffset = global::UnityEngine.Random.Range(-10000f, 10000f);
			int maxGrassDepth = global::UnityEngine.Random.Range(2, 5) + 1;
			Debug.Log(string.Format("Creating template with max grassDepth: {0}", maxGrassDepth));
			Vector2Int spawnPointPosition = new Vector2Int(global::UnityEngine.Random.Range(0, (int)generationBounds.x), global::UnityEngine.Random.Range(0, (int)generationBounds.z));
			int soilIndex = ((global::UnityEngine.Random.Range(0f, 1f) > 0.1f) ? tilesetIdInGame : tilesetIdInGame2);
			int x = 0;
			while ((float)x < generationBounds.x)
			{
				int z = 0;
				while ((float)z < generationBounds.z)
				{
					float num = xOffset + (float)x / generationBounds.x * scale;
					float num2 = yOffset + (float)z / generationBounds.z * scale;
					int num3;
					if (generationBounds.y <= 1f)
					{
						num3 = 1;
					}
					else
					{
						float num4 = Mathf.PerlinNoise(num, num2);
						num3 = (int)(generationBounds.y * num4);
					}
					float num5 = xOffset + 10000f + (float)x / generationBounds.x * scale;
					float num6 = yOffset + 10000f + (float)z / generationBounds.z * scale;
					float num7 = Mathf.PerlinNoise(num5, num6);
					int num8 = (int)(num7 * (float)maxGrassDepth);
					if (num8 == 0 && num7 > 0.1f)
					{
						num8 = (int)(ProceduralTemplateSource.RemapFloatRange(num7, 0f, 1f, 0.35f, 1f) * (float)maxGrassDepth);
					}
					for (int i = 0; i < num3; i++)
					{
						int num9;
						if (i < num8)
						{
							num9 = soilIndex;
						}
						else
						{
							num9 = stoneIndex;
						}
						levelState.AddTerrainCell(new Vector3Int(x, num3 - i, z), num9);
					}
					if (x == spawnPointPosition.x && z == spawnPointPosition.y)
					{
						Debug.Log("Template: Adding spawn point");
						PropEntry propEntry = new PropEntry
						{
							Position = new Vector3((float)x, (float)(num3 + 1), (float)z),
							Label = "Basic Spawn Point",
							InstanceId = SerializableGuid.NewGuid(),
							AssetId = this.basicSpawnPointAssetId,
							Rotation = Quaternion.Euler(0f, (float)(90 * global::UnityEngine.Random.Range(0, 4)), 0f),
							ComponentEntries = new List<ComponentEntry>(),
							LuaMemberChanges = new List<MemberChange>()
						};
						levelState.AddProp(propEntry, true, null);
					}
					if (x * z % 100 == 0)
					{
						await Task.Yield();
					}
					z++;
				}
				x++;
			}
		}

		// Token: 0x0600015F RID: 351 RVA: 0x0000B960 File Offset: 0x00009B60
		public override async Task<List<SerializableGuid>> GetRequiredTerrainAssets(Game game)
		{
			List<SerializableGuid> list = new List<SerializableGuid>();
			list.Add(MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultTerrain.GetIdByName("Grass"));
			list.Add(MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultTerrain.GetIdByName("Dirt"));
			list.Add(MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultTerrain.GetIdByName("Ancient Ruins"));
			return list.Except(game.GameLibrary.TerrainEntries.Select((TerrainUsage entry) => entry.TilesetId)).ToList<SerializableGuid>();
		}

		// Token: 0x06000160 RID: 352 RVA: 0x0000B9A4 File Offset: 0x00009BA4
		public override async Task<List<SerializableGuid>> GetRequiredPropAssets(Game game)
		{
			List<SerializableGuid> list = new List<SerializableGuid>();
			list.Add(this.basicSpawnPointAssetId);
			return list.Except(game.GameLibrary.PropReferences.Select((AssetReference entry) => entry.AssetID)).ToList<SerializableGuid>();
		}

		// Token: 0x06000161 RID: 353 RVA: 0x0000B9F0 File Offset: 0x00009BF0
		public override async Task Prepare()
		{
		}

		// Token: 0x06000162 RID: 354 RVA: 0x0000BA2C File Offset: 0x00009C2C
		private int GetTilesetIdInGame(Game game, SerializableGuid tilesetAssetId)
		{
			int num = game.GameLibrary.TerrainEntries.FindIndex((TerrainUsage usage) => usage.IsActive && usage.TerrainAssetReference.AssetID == tilesetAssetId);
			return Mathf.Max(0, num);
		}

		// Token: 0x06000163 RID: 355 RVA: 0x0000BA6C File Offset: 0x00009C6C
		private static T GetRandomEnum<T>(int minTrim = 1, int maxTrim = 0) where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum)
			{
				throw new Exception("random enum variable is not an enum");
			}
			T[] array = (from T entry in Enum.GetValues(typeof(T))
				orderby entry
				select entry).ToArray<T>();
			int num = global::UnityEngine.Random.Range(minTrim, array.Length - maxTrim);
			return array[num];
		}

		// Token: 0x06000164 RID: 356 RVA: 0x0000BAE6 File Offset: 0x00009CE6
		private void Reset()
		{
			this.WorldSizeConfiguration = ProceduralTemplateSource.WorldSize.Random;
			this.AmplitudeConfiguration = ProceduralTemplateSource.Amplitude.Random;
			this.WorldHeightConfiguration = ProceduralTemplateSource.WorldHeight.Random;
		}

		// Token: 0x06000165 RID: 357 RVA: 0x0000BAFD File Offset: 0x00009CFD
		public static float RemapFloatRange(float value, float from1, float to1, float from2, float to2)
		{
			return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
		}

		// Token: 0x040001B0 RID: 432
		[SerializeField]
		private Sprite displaySprite;

		// Token: 0x040001B1 RID: 433
		private SerializableGuid basicSpawnPointAssetId = "8e53a1d9-b7b5-4873-96ce-2566332b4a5e";

		// Token: 0x040001B2 RID: 434
		private ProceduralTemplateSource.Amplitude AmplitudeConfiguration = ProceduralTemplateSource.Amplitude.Random;

		// Token: 0x040001B3 RID: 435
		private ProceduralTemplateSource.WorldSize WorldSizeConfiguration = ProceduralTemplateSource.WorldSize.Random;

		// Token: 0x040001B4 RID: 436
		private ProceduralTemplateSource.WorldHeight WorldHeightConfiguration = ProceduralTemplateSource.WorldHeight.Random;

		// Token: 0x02000061 RID: 97
		public enum Amplitude
		{
			// Token: 0x040001B6 RID: 438
			Random = -1,
			// Token: 0x040001B7 RID: 439
			Low = 1,
			// Token: 0x040001B8 RID: 440
			Medium = 4,
			// Token: 0x040001B9 RID: 441
			High = 8,
			// Token: 0x040001BA RID: 442
			Extreme = 12
		}

		// Token: 0x02000062 RID: 98
		public enum WorldSize
		{
			// Token: 0x040001BC RID: 444
			Random = -1,
			// Token: 0x040001BD RID: 445
			Micro = 10,
			// Token: 0x040001BE RID: 446
			Small = 25,
			// Token: 0x040001BF RID: 447
			Medium = 50,
			// Token: 0x040001C0 RID: 448
			Large = 75,
			// Token: 0x040001C1 RID: 449
			Extreme = 100
		}

		// Token: 0x02000063 RID: 99
		public enum WorldHeight
		{
			// Token: 0x040001C3 RID: 451
			Random = -1,
			// Token: 0x040001C4 RID: 452
			Flattened = 1,
			// Token: 0x040001C5 RID: 453
			Low = 10,
			// Token: 0x040001C6 RID: 454
			Medium = 20,
			// Token: 0x040001C7 RID: 455
			High = 40
		}
	}
}
