using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Runtime.Gameplay.LevelEditing;
using UnityEngine;

namespace Endless.Creator;

[CreateAssetMenu(menuName = "ScriptableObject/Level States/Procedural Template Source", fileName = "Procedural Template Source")]
public class ProceduralTemplateSource : LevelStateTemplateSourceBase
{
	public enum Amplitude
	{
		Random = -1,
		Low = 1,
		Medium = 4,
		High = 8,
		Extreme = 12
	}

	public enum WorldSize
	{
		Random = -1,
		Micro = 10,
		Small = 25,
		Medium = 50,
		Large = 75,
		Extreme = 100
	}

	public enum WorldHeight
	{
		Random = -1,
		Flattened = 1,
		Low = 10,
		Medium = 20,
		High = 40
	}

	[SerializeField]
	private Sprite displaySprite;

	private SerializableGuid basicSpawnPointAssetId = "8e53a1d9-b7b5-4873-96ce-2566332b4a5e";

	private Amplitude AmplitudeConfiguration = Amplitude.Random;

	private WorldSize WorldSizeConfiguration = WorldSize.Random;

	private WorldHeight WorldHeightConfiguration = WorldHeight.Random;

	private Amplitude GetAmplitude()
	{
		if (AmplitudeConfiguration != Amplitude.Random)
		{
			return AmplitudeConfiguration;
		}
		return GetRandomEnum<Amplitude>();
	}

	private WorldSize GetWorldSize()
	{
		if (WorldSizeConfiguration != WorldSize.Random)
		{
			return WorldSizeConfiguration;
		}
		return GetRandomEnum<WorldSize>();
	}

	private WorldHeight GetWorldHeight()
	{
		if (WorldHeightConfiguration != WorldHeight.Random)
		{
			return WorldHeightConfiguration;
		}
		return GetRandomEnum<WorldHeight>();
	}

	public override async Task<string> GetDisplayName()
	{
		return "Random Level";
	}

	public override async Task<string> GetDescription()
	{
		throw new NotImplementedException();
	}

	public override async Task<Sprite> GetDisplaySprite()
	{
		return displaySprite;
	}

	public override async Task<LevelState> GetLevelState(Game game)
	{
		LevelState levelState = new LevelState
		{
			AssetType = "level"
		};
		await BuildTerrain(game, levelState);
		return levelState;
	}

	private async Task BuildTerrain(Game game, LevelState levelState)
	{
		WorldSize worldSize = GetWorldSize();
		WorldHeight worldHeight = GetWorldHeight();
		Amplitude amplitude = GetAmplitude();
		Debug.Log("Creating template with size: " + worldSize.ToString() + ", height: " + worldHeight.ToString() + ", amplitude: " + amplitude);
		int num = (int)worldSize;
		int y = (int)((float)worldHeight * UnityEngine.Random.Range(0.95f, 1.05f));
		float scale = (float)amplitude * UnityEngine.Random.Range(0.95f, 1.05f);
		Vector3 generationBounds = new Vector3Int(num, y, num);
		await GenerateGrassLandHills(game, levelState, generationBounds, scale);
	}

	private async Task GenerateGrassLandHills(Game game, LevelState levelState, Vector3 generationBounds, float scale)
	{
		string idByName = MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultTerrain.GetIdByName("Grass");
		string idByName2 = MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultTerrain.GetIdByName("Ancient Ruins");
		string idByName3 = MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultTerrain.GetIdByName("Dirt");
		int tilesetIdInGame = GetTilesetIdInGame(game, idByName);
		int stoneIndex = GetTilesetIdInGame(game, idByName2);
		int tilesetIdInGame2 = GetTilesetIdInGame(game, idByName3);
		float xOffset = UnityEngine.Random.Range(-10000f, 10000f);
		float yOffset = UnityEngine.Random.Range(-10000f, 10000f);
		int maxGrassDepth = UnityEngine.Random.Range(2, 5) + 1;
		Debug.Log($"Creating template with max grassDepth: {maxGrassDepth}");
		Vector2Int spawnPointPosition = new Vector2Int(UnityEngine.Random.Range(0, (int)generationBounds.x), UnityEngine.Random.Range(0, (int)generationBounds.z));
		int soilIndex = ((UnityEngine.Random.Range(0f, 1f) > 0.1f) ? tilesetIdInGame : tilesetIdInGame2);
		for (int x = 0; (float)x < generationBounds.x; x++)
		{
			for (int z = 0; (float)z < generationBounds.z; z++)
			{
				float x2 = xOffset + (float)x / generationBounds.x * scale;
				float y = yOffset + (float)z / generationBounds.z * scale;
				int num;
				if (generationBounds.y <= 1f)
				{
					num = 1;
				}
				else
				{
					float num2 = Mathf.PerlinNoise(x2, y);
					num = (int)(generationBounds.y * num2);
				}
				float x3 = xOffset + 10000f + (float)x / generationBounds.x * scale;
				float y2 = yOffset + 10000f + (float)z / generationBounds.z * scale;
				float num3 = Mathf.PerlinNoise(x3, y2);
				int num4 = (int)(num3 * (float)maxGrassDepth);
				if (num4 == 0 && num3 > 0.1f)
				{
					num4 = (int)(RemapFloatRange(num3, 0f, 1f, 0.35f, 1f) * (float)maxGrassDepth);
				}
				for (int i = 0; i < num; i++)
				{
					int tilesetIndex = ((i >= num4) ? stoneIndex : soilIndex);
					levelState.AddTerrainCell(new Vector3Int(x, num - i, z), tilesetIndex);
				}
				if (x == spawnPointPosition.x && z == spawnPointPosition.y)
				{
					Debug.Log("Template: Adding spawn point");
					PropEntry propEntry = new PropEntry
					{
						Position = new Vector3(x, num + 1, z),
						Label = "Basic Spawn Point",
						InstanceId = SerializableGuid.NewGuid(),
						AssetId = basicSpawnPointAssetId,
						Rotation = Quaternion.Euler(0f, 90 * UnityEngine.Random.Range(0, 4), 0f),
						ComponentEntries = new List<ComponentEntry>(),
						LuaMemberChanges = new List<MemberChange>()
					};
					levelState.AddProp(propEntry, isSpawnPoint: true, null);
				}
				if (x * z % 100 == 0)
				{
					await Task.Yield();
				}
			}
		}
	}

	public override async Task<List<SerializableGuid>> GetRequiredTerrainAssets(Game game)
	{
		return new List<SerializableGuid>
		{
			MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultTerrain.GetIdByName("Grass"),
			MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultTerrain.GetIdByName("Dirt"),
			MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultTerrain.GetIdByName("Ancient Ruins")
		}.Except(game.GameLibrary.TerrainEntries.Select((TerrainUsage entry) => entry.TilesetId)).ToList();
	}

	public override async Task<List<SerializableGuid>> GetRequiredPropAssets(Game game)
	{
		return new List<SerializableGuid> { basicSpawnPointAssetId }.Except(((IEnumerable<AssetReference>)game.GameLibrary.PropReferences).Select((Func<AssetReference, SerializableGuid>)((AssetReference entry) => entry.AssetID))).ToList();
	}

	public override async Task Prepare()
	{
	}

	private int GetTilesetIdInGame(Game game, SerializableGuid tilesetAssetId)
	{
		int b = game.GameLibrary.TerrainEntries.FindIndex((TerrainUsage usage) => usage.IsActive && (SerializableGuid)usage.TerrainAssetReference.AssetID == tilesetAssetId);
		return Mathf.Max(0, b);
	}

	private static T GetRandomEnum<T>(int minTrim = 1, int maxTrim = 0) where T : struct, IConvertible
	{
		if (!typeof(T).IsEnum)
		{
			throw new Exception("random enum variable is not an enum");
		}
		T[] array = (from T entry in Enum.GetValues(typeof(T))
			orderby entry
			select entry).ToArray();
		int num = UnityEngine.Random.Range(minTrim, array.Length - maxTrim);
		return array[num];
	}

	private void Reset()
	{
		WorldSizeConfiguration = WorldSize.Random;
		AmplitudeConfiguration = Amplitude.Random;
		WorldHeightConfiguration = WorldHeight.Random;
	}

	public static float RemapFloatRange(float value, float from1, float to1, float from2, float to2)
	{
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}
}
