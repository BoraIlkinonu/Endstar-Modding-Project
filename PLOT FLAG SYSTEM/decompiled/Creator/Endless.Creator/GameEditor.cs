using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Data;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.GraphQl;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.TerrainCosmetics;
using Newtonsoft.Json;
using Runtime.Gameplay.LevelEditing;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Creator;

public class GameEditor : MonoBehaviourSingleton<GameEditor>
{
	private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

	private CancellationToken cancellationToken;

	private List<LevelState> levelStates = new List<LevelState>();

	public Game ActiveGame
	{
		get
		{
			return MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame;
		}
		private set
		{
			MonoBehaviourSingleton<RuntimeDatabase>.Instance.SetGame(value);
		}
	}

	public List<LevelReference> GetLevelReferences()
	{
		return ActiveGame.levels;
	}

	public (int Mininum, int Maximum) GetNumberOfPlayers()
	{
		return (Mininum: ActiveGame.MininumNumberOfPlayers, Maximum: ActiveGame.MaximumNumberOfPlayers);
	}

	public void NewSession()
	{
		cancellationTokenSource = new CancellationTokenSource();
		cancellationToken = cancellationTokenSource.Token;
	}

	public void EndSession()
	{
		cancellationTokenSource.Cancel();
	}

	public async Task<bool> AddLevel(LevelState newLevelState)
	{
		DebugUtility.LogMethod(this, "AddLevel", newLevelState);
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameNewLevelAdded,
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		return await UpdateGame((Func<Game, Task<(bool updateNeeded, object data)>>)ApplyChange, changeData, cancellationToken);
		Task<(bool, object)> ApplyChange(Game game)
		{
			return Task.FromResult((true, game.GetAnonymousObjectForUpload(newLevelState)));
		}
	}

	public async Task<bool> ReorderGameLevels(List<LevelReference> newOrder)
	{
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameLevelReorder,
			Metadata = "",
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		try
		{
			return await UpdateGame((Func<Game, Task<bool>>)ReorderLevels, changeData, cancellationToken);
		}
		catch (Exception exception)
		{
			ErrorHandler.HandleError(ErrorCodes.GameEditor_ReorderingLevels, exception);
			return false;
		}
		Task<bool> ReorderLevels(Game game)
		{
			return Task.FromResult(game.ReorderLevels(newOrder));
		}
	}

	public void SetLevelArchived(SerializableGuid levelId, bool levelArchived)
	{
	}

	public async Task<bool> SetPlayerCount(int min, int max)
	{
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GamePlayerCountUpdated,
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		return await UpdateGame((Func<Game, Task<bool>>)UpdatePlayerCount, changeData, cancellationToken);
		Task<bool> UpdatePlayerCount(Game game)
		{
			if (game.MininumNumberOfPlayers != min || game.MaximumNumberOfPlayers != max)
			{
				game.MininumNumberOfPlayers = min;
				game.MaximumNumberOfPlayers = max;
				return Task.FromResult(result: true);
			}
			return Task.FromResult(result: false);
		}
	}

	public async Task<bool> UpdateGameName(string newValue)
	{
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameNameUpdated,
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		return await UpdateGame((Func<Game, Task<bool>>)ApplyNameChange, changeData, cancellationToken);
		Task<bool> ApplyNameChange(Game game)
		{
			if (game.Name != newValue)
			{
				game.Name = newValue;
				return Task.FromResult(result: true);
			}
			return Task.FromResult(result: false);
		}
	}

	public async Task<bool> UpdateDescription(string newValue, Action successCallback = null, Action failureCallback = null)
	{
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameDescriptionUpdated,
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		return await UpdateGame((Func<Game, Task<bool>>)ApplyDescriptionChange, changeData, cancellationToken);
		Task<bool> ApplyDescriptionChange(Game game)
		{
			if (game.Description != newValue)
			{
				game.Description = newValue;
				return Task.FromResult(result: true);
			}
			return Task.FromResult(result: false);
		}
	}

	public async Task<bool> AddScreenshotsToGame(List<ScreenshotFileInstances> newValue)
	{
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameScreenshotsAdded,
			Metadata = "",
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		try
		{
			return await UpdateGame((Func<Game, Task<bool>>)AddScreenshots, changeData, cancellationToken);
		}
		catch (Exception exception)
		{
			ErrorHandler.HandleError(ErrorCodes.GameEditor_AddingScreenshots, exception);
			return false;
		}
		Task<bool> AddScreenshots(Game game)
		{
			game.AddScreenshots(newValue);
			return Task.FromResult(result: true);
		}
	}

	public async Task<bool> RemoveGameScreenshotAt(int index)
	{
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameScreenshotsRemoved,
			Metadata = "",
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		try
		{
			return await UpdateGame((Func<Game, Task<bool>>)RemoveScreenshotAt, changeData, cancellationToken);
		}
		catch (Exception exception)
		{
			ErrorHandler.HandleError(ErrorCodes.GameEditor_RemovingScreenshots, exception);
			return false;
		}
		Task<bool> RemoveScreenshotAt(Game game)
		{
			game.RemoveScreenshotAt(index);
			return Task.FromResult(result: true);
		}
	}

	public async Task<bool> ReorderGameScreenshot(List<ScreenshotFileInstances> newOrder)
	{
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameScreenshotsReorder,
			Metadata = "",
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		try
		{
			return await UpdateGame((Func<Game, Task<bool>>)ReorderScreenshot, changeData, cancellationToken);
		}
		catch (Exception exception)
		{
			ErrorHandler.HandleError(ErrorCodes.GameEditor_ReorderingScreenshots, exception);
			return false;
		}
		Task<bool> ReorderScreenshot(Game game)
		{
			game.ReorderScreenshot(newOrder);
			return Task.FromResult(result: true);
		}
	}

	public async Task<bool> GetUpdatedGame(string gameId, Func<Game, Game, Task> success, Action failure, CancellationToken cancelToken = default(CancellationToken))
	{
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(gameId);
		if (cancelToken.IsCancellationRequested)
		{
			return false;
		}
		int maxRetries = 5;
		if (graphQlResult.HasErrors)
		{
			bool succeeded = false;
			if (Regex.IsMatch(graphQlResult.GetErrorMessage().Message, "Asset [A-Za-z0-9\\-\"]* version [0-9.\"]* not found"))
			{
				UnityEngine.Debug.LogWarning("Failed to get latest version of game (originally error 2024). Executing fallback workaround and re-trying");
				int attempt = 0;
				while (attempt < maxRetries)
				{
					await Task.Delay(500 + attempt * 100);
					if (cancelToken.IsCancellationRequested)
					{
						return false;
					}
					attempt++;
					graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(gameId);
					if (!graphQlResult.HasErrors)
					{
						succeeded = true;
						break;
					}
				}
			}
			if (!succeeded)
			{
				ErrorHandler.HandleError(ErrorCodes.GameEditor_GetUpdatedGame_GraphQLError, graphQlResult.GetErrorMessage());
				failure();
				return false;
			}
		}
		try
		{
			if (ActiveGame == null)
			{
				return false;
			}
			Game activeGame = ActiveGame;
			Game game = GameLoader.Load(graphQlResult.GetDataMember().ToString());
			if (SemanticVersion.Parse(game.AssetVersion) > SemanticVersion.Parse(ActiveGame.AssetVersion))
			{
				ActiveGame = game;
				success?.Invoke(ActiveGame, activeGame);
			}
			return true;
		}
		catch (Exception exception)
		{
			ErrorHandler.HandleError(ErrorCodes.GameEditor_GetGameInitialResolveConflict, exception);
			failure?.Invoke();
			return false;
		}
	}

	public async Task<GameLibrary.RemoveTerrainEntryResult> RemoveTerrainEntryFromGameLibrary(SerializableGuid toRemoveId, SerializableGuid redirectId)
	{
		if (toRemoveId == SerializableGuid.Empty)
		{
			throw new ArgumentNullException("You cannot remove a TerrainUsage without an ID!");
		}
		if (redirectId == SerializableGuid.Empty)
		{
			throw new ArgumentNullException("You cannot redirect to a TerrainUsage without an ID!");
		}
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameLibraryTerrainRemoved,
			Metadata = "",
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		GameLibrary.RemoveTerrainEntryResult result = new GameLibrary.RemoveTerrainEntryResult(success: false, -1);
		await UpdateGame(delegate(Game game)
		{
			result = RemoveTerrainEntry(game);
			return Task.FromResult(result.Success);
		}, changeData, cancellationToken);
		return result;
		GameLibrary.RemoveTerrainEntryResult RemoveTerrainEntry(Game game)
		{
			return game.GameLibrary.RemoveTerrainEntry(toRemoveId, redirectId);
		}
	}

	public async Task<bool> AddTerrainUsageToGameLibrary(SerializableGuid tilesetId, string version)
	{
		if (tilesetId == SerializableGuid.Empty)
		{
			throw new ArgumentNullException("You cannot add a TerrainUsage without an ID!");
		}
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameLibraryTerrainAdded,
			Metadata = "",
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		return await UpdateGame((Func<Game, Task<bool>>)AddTerrainUsage, changeData, cancellationToken);
		Task<bool> AddTerrainUsage(Game game)
		{
			return Task.FromResult(game.GameLibrary.AddTerrainUsage(tilesetId, version));
		}
	}

	public async Task<bool> AddTerrainUsagesToGameLibrary(IEnumerable<TerrainTilesetCosmeticAsset> terrainUsages)
	{
		AssetReference[] assetReferences = terrainUsages.Select((TerrainTilesetCosmeticAsset usage) => usage.ToAssetReference()).ToArray();
		return await AddTerrainUsagesToGameLibrary(assetReferences);
	}

	public async Task<bool> AddTerrainUsagesToGameLibrary(AssetReference[] assetReferences)
	{
		if (assetReferences.Any((AssetReference reference) => (SerializableGuid)reference.AssetID == SerializableGuid.Empty || string.IsNullOrEmpty(reference.AssetVersion)))
		{
			throw new ArgumentNullException("You cannot add a TerrainUsage without a valid ID and version!");
		}
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameLibraryTerrainAdded,
			Metadata = "",
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		return await UpdateGame((Func<Game, Task<bool>>)AddTerrainUsages, changeData, cancellationToken);
		Task<bool> AddTerrainUsages(Game game)
		{
			return Task.FromResult(game.GameLibrary.AddTerrainUsages(assetReferences));
		}
	}

	public async Task<bool> SetTerrainUsageVersionInGameLibrary(SerializableGuid id, string newTerrainVersion)
	{
		if (id == SerializableGuid.Empty)
		{
			throw new ArgumentNullException("You cannot set a TerrainUsage version without an ID!");
		}
		if (newTerrainVersion.IsNullOrEmptyOrWhiteSpace())
		{
			throw new ArgumentNullException("You cannot set a TerrainUsage version without an version!");
		}
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameLibraryTerrainVersionChanged,
			Metadata = "",
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		return await UpdateGame((Func<Game, Task<bool>>)SetTerrainUsageVersion, changeData, cancellationToken);
		Task<bool> SetTerrainUsageVersion(Game game)
		{
			return Task.FromResult(game.GameLibrary.SetTerrainUsageVersion(id, newTerrainVersion));
		}
	}

	public async Task<bool> SetPropVersionInGameLibrary(string id, string newAssetVersion)
	{
		if ((SerializableGuid)id == SerializableGuid.Empty)
		{
			throw new ArgumentNullException("You cannot set a Prop version without an ID!");
		}
		if (newAssetVersion.IsNullOrEmptyOrWhiteSpace())
		{
			throw new ArgumentNullException("You cannot set a Prop version without an version!");
		}
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameLibraryPropVersionChanged,
			Metadata = "",
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		return await UpdateGame((Func<Game, Task<bool>>)SetPropVersion, changeData, cancellationToken);
		Task<bool> SetPropVersion(Game game)
		{
			return Task.FromResult(game.GameLibrary.SetPropVersion(id, newAssetVersion));
		}
	}

	public async Task<bool> SetAudioVersionInGameLibrary(string id, string newAssetVersion)
	{
		if ((SerializableGuid)id == SerializableGuid.Empty)
		{
			throw new ArgumentNullException("You cannot set a AudioAsset version without an ID!");
		}
		if (newAssetVersion.IsNullOrEmptyOrWhiteSpace())
		{
			throw new ArgumentNullException("You cannot set a AudioAsset version without an version!");
		}
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameLibraryAudioVersionChanged,
			Metadata = "",
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		return await UpdateGame((Func<Game, Task<bool>>)SetAudioVersion, changeData, cancellationToken);
		Task<bool> SetAudioVersion(Game game)
		{
			return Task.FromResult(game.GameLibrary.SetAudioVersion(id, newAssetVersion));
		}
	}

	public async Task<bool> AddPropToGameLibrary(Prop prop)
	{
		return await AddPropToGameLibrary(prop.ToAssetReference());
	}

	public async Task<bool> AddPropsToGameLibrary(IEnumerable<Prop> props)
	{
		return await AddPropsToGameLibrary(props.Select((Prop prop) => prop.ToAssetReference()).ToArray(), cancellationToken);
	}

	public async Task<bool> AddPropToGameLibrary(AssetReference assetReference)
	{
		return await AddPropToGameLibrary(assetReference, cancellationToken);
	}

	private async Task<bool> AddPropToGameLibrary(AssetReference assetReference, CancellationToken sessionToken)
	{
		if ((SerializableGuid)assetReference.AssetID == SerializableGuid.Empty)
		{
			throw new ArgumentException("You cannot add an asset without an ID!");
		}
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameLibraryPropAdded,
			Metadata = "",
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		return await UpdateGame((Func<Game, Task<bool>>)AddProp, changeData, sessionToken);
		Task<bool> AddProp(Game game)
		{
			return Task.FromResult(game.GameLibrary.AddProp(assetReference));
		}
	}

	private async Task<bool> AddPropsToGameLibrary(AssetReference[] assetReferences, CancellationToken sessionToken)
	{
		if (assetReferences.Any((AssetReference reference) => (SerializableGuid)reference.AssetID == SerializableGuid.Empty))
		{
			throw new ArgumentException("You cannot add an asset without an ID!");
		}
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameLibraryPropAdded,
			Metadata = "",
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		return await UpdateGame((Func<Game, Task<bool>>)AddProp, changeData, sessionToken);
		Task<bool> AddProp(Game game)
		{
			return Task.FromResult(game.GameLibrary.AddProps(assetReferences));
		}
	}

	public async Task<bool> AddAudioToGameLibrary(AssetReference assetReference)
	{
		return await AddAudioToGameLibrary(assetReference, cancellationToken);
	}

	private async Task<bool> AddAudioToGameLibrary(AssetReference assetReference, CancellationToken sessionToken)
	{
		if ((SerializableGuid)assetReference.AssetID == SerializableGuid.Empty)
		{
			throw new ArgumentException("You cannot add an asset without an ID!");
		}
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameLibraryAudioAdded,
			Metadata = "",
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		return await UpdateGame((Func<Game, Task<bool>>)AddAudio, changeData, sessionToken);
		Task<bool> AddAudio(Game game)
		{
			return Task.FromResult(game.GameLibrary.AddAudio(assetReference));
		}
	}

	public async Task<bool> RemovePropFromGameLibrary(SerializableGuid assetID)
	{
		if (assetID == SerializableGuid.Empty)
		{
			throw new ArgumentNullException("You cannot remove a Prop without an ID!");
		}
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameLibraryPropRemoved,
			Metadata = "",
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		return await UpdateGame((Func<Game, Task<bool>>)RemoveProp, changeData, cancellationToken);
		Task<bool> RemoveProp(Game game)
		{
			return Task.FromResult(game.GameLibrary.RemoveProp(assetID));
		}
	}

	public async Task<bool> RemoveAudioFromGameLibrary(SerializableGuid assetID)
	{
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameLibraryAudioRemoved,
			Metadata = "",
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		return await UpdateGame((Func<Game, Task<bool>>)RemoveAsset, changeData, cancellationToken);
		Task<bool> RemoveAsset(Game game)
		{
			return Task.FromResult(game.GameLibrary.RemoveAudio(assetID));
		}
	}

	public async Task<bool> UpdateAllGameLibrary()
	{
		ChangeData changeData = new ChangeData
		{
			ChangeType = ChangeType.GameLibraryUpdateAll,
			Metadata = "",
			UserId = EndlessServices.Instance.CloudService.ActiveUserId
		};
		try
		{
			UnityEngine.Debug.Log("Update Game Complete");
			return await UpdateGame((Func<Game, Task<bool>>)UpdateAll, changeData, cancellationToken);
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
			ErrorHandler.HandleError(ErrorCodes.GameEditor_UpdateAll, exception);
			return false;
		}
		static async Task<bool> UpdateAll(Game game)
		{
			foreach (AssetReference prop in game.GameLibrary.PropReferences)
			{
				UnityEngine.Debug.Log("Updating prop: " + prop.AssetID);
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(prop.AssetID);
				if (graphQlResult.HasErrors)
				{
					throw graphQlResult.GetErrorMessage();
				}
				Prop prop2 = JsonConvert.DeserializeObject<Prop>(graphQlResult.GetDataMember().ToString());
				UnityEngine.Debug.Log("Current Version: " + prop.AssetVersion + ", New Version: " + prop.AssetVersion);
				prop.AssetVersion = prop2.AssetVersion;
			}
			foreach (TerrainUsage terrainUsage in game.GameLibrary.TerrainEntries)
			{
				if ((object)terrainUsage.TerrainAssetReference != null)
				{
					UnityEngine.Debug.Log("Updating terrain: " + terrainUsage.TerrainAssetReference.AssetID);
					GraphQlResult graphQlResult2 = await EndlessServices.Instance.CloudService.GetAssetAsync(terrainUsage.TerrainAssetReference.AssetID);
					if (graphQlResult2.HasErrors)
					{
						throw graphQlResult2.GetErrorMessage();
					}
					UnityEngine.Debug.Log("Current Version: " + terrainUsage.TerrainAssetReference.AssetVersion + ", New Version: " + terrainUsage.TerrainAssetReference.AssetVersion);
					TerrainTilesetCosmeticAsset terrainTilesetCosmeticAsset = JsonConvert.DeserializeObject<TerrainTilesetCosmeticAsset>(graphQlResult2.GetDataMember().ToString());
					terrainUsage.TerrainAssetReference.AssetVersion = terrainTilesetCosmeticAsset.AssetVersion;
				}
			}
			return true;
		}
	}

	private async Task<bool> UpdateGame(Func<Game, Task<bool>> updateFunction, ChangeData changeData, CancellationToken sessionToken)
	{
		Game oldGame = ActiveGame.Clone();
		Game game = ActiveGame.Clone();
		SemanticVersion expectedVersion = SemanticVersion.Parse(game.AssetVersion).IncrementPatch();
		game.AssetVersion = expectedVersion.ToString();
		bool flag = await updateFunction(game);
		if (sessionToken.IsCancellationRequested || !flag)
		{
			return false;
		}
		game.RevisionMetaData = new RevisionMetaData(new HashSet<ChangeData> { changeData });
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync(game);
		if (graphQlResult.HasErrors)
		{
			if (graphQlResult.GetErrorMessage() is DuplicateAssetVersionException)
			{
				Stopwatch waitStopwatch = new Stopwatch();
				waitStopwatch.Start();
				while (!sessionToken.IsCancellationRequested && SemanticVersion.Parse(ActiveGame.AssetVersion) < expectedVersion)
				{
					if (waitStopwatch.ElapsedMilliseconds > 60000)
					{
						if (await GetUpdatedGame(ActiveGame.AssetID, NetworkBehaviourSingleton<CreatorManager>.Instance.HandleGameUpdated, null, sessionToken) && SemanticVersion.Parse(ActiveGame.AssetVersion) >= expectedVersion)
						{
							UnityEngine.Debug.LogWarning("Timed out waiting for game version, but was able to recover!");
							break;
						}
						throw new TimeoutException("Failed to get expected game version!");
					}
					await Task.Yield();
				}
				if (sessionToken.IsCancellationRequested)
				{
					return false;
				}
				UnityEngine.Debug.Log($"Expected version retrieved: {ActiveGame.AssetVersion} (expected {expectedVersion})");
				return await UpdateGame(updateFunction, changeData, sessionToken);
			}
			throw graphQlResult.GetErrorMessage();
		}
		ActiveGame = GameLoader.Load(graphQlResult.GetDataMember().ToString());
		await NetworkBehaviourSingleton<CreatorManager>.Instance.HandleGameUpdated(ActiveGame, oldGame);
		return true;
	}

	private async Task<bool> UpdateGame(Func<Game, Task<(bool updateNeeded, object data)>> updateFunction, ChangeData changeData, CancellationToken sessionToken)
	{
		Game oldGame = ActiveGame.Clone();
		Game game = ActiveGame.Clone();
		SemanticVersion expectedVersion = SemanticVersion.Parse(game.AssetVersion).IncrementPatch();
		game.AssetVersion = expectedVersion.ToString();
		(bool, object) tuple = await updateFunction(game);
		if (sessionToken.IsCancellationRequested || !tuple.Item1)
		{
			return false;
		}
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync(tuple.Item2);
		if (graphQlResult.HasErrors)
		{
			if (graphQlResult.GetErrorMessage() is DuplicateAssetVersionException)
			{
				Stopwatch waitStopwatch = new Stopwatch();
				waitStopwatch.Start();
				while (!sessionToken.IsCancellationRequested && SemanticVersion.Parse(ActiveGame.AssetVersion) < expectedVersion)
				{
					if (waitStopwatch.ElapsedMilliseconds > 60000)
					{
						if (await GetUpdatedGame(ActiveGame.AssetID, NetworkBehaviourSingleton<CreatorManager>.Instance.HandleGameUpdated, null, sessionToken) && SemanticVersion.Parse(ActiveGame.AssetVersion) >= expectedVersion)
						{
							UnityEngine.Debug.LogWarning("Timed out waiting for game version, but was able to recover!");
							break;
						}
						throw new TimeoutException("Failed to get expected game version!");
					}
					await Task.Yield();
				}
				if (sessionToken.IsCancellationRequested)
				{
					return false;
				}
				UnityEngine.Debug.Log($"Expected version retrieved: {ActiveGame.AssetVersion} (expected {expectedVersion})");
				return await UpdateGame(updateFunction, changeData, sessionToken);
			}
			throw graphQlResult.GetErrorMessage();
		}
		ActiveGame = GameLoader.Load(graphQlResult.GetDataMember().ToString());
		await NetworkBehaviourSingleton<CreatorManager>.Instance.HandleGameUpdated(ActiveGame, oldGame);
		return true;
	}
}
