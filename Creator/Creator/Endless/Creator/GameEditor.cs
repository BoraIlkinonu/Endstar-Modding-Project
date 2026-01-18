using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Endless.Creator
{
	// Token: 0x0200002A RID: 42
	public class GameEditor : MonoBehaviourSingleton<GameEditor>
	{
		// Token: 0x17000011 RID: 17
		// (get) Token: 0x060000B8 RID: 184 RVA: 0x00007306 File Offset: 0x00005506
		// (set) Token: 0x060000B9 RID: 185 RVA: 0x00007312 File Offset: 0x00005512
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

		// Token: 0x060000BA RID: 186 RVA: 0x0000731F File Offset: 0x0000551F
		public List<LevelReference> GetLevelReferences()
		{
			return this.ActiveGame.levels;
		}

		// Token: 0x060000BB RID: 187 RVA: 0x0000732C File Offset: 0x0000552C
		[return: TupleElementNames(new string[] { "Mininum", "Maximum" })]
		public ValueTuple<int, int> GetNumberOfPlayers()
		{
			return new ValueTuple<int, int>(this.ActiveGame.MininumNumberOfPlayers, this.ActiveGame.MaximumNumberOfPlayers);
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00007349 File Offset: 0x00005549
		public void NewSession()
		{
			this.cancellationTokenSource = new CancellationTokenSource();
			this.cancellationToken = this.cancellationTokenSource.Token;
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00007367 File Offset: 0x00005567
		public void EndSession()
		{
			this.cancellationTokenSource.Cancel();
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00007374 File Offset: 0x00005574
		public async Task<bool> AddLevel(LevelState newLevelState)
		{
			GameEditor.<>c__DisplayClass10_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass10_0();
			CS$<>8__locals1.newLevelState = newLevelState;
			DebugUtility.LogMethod(this, "AddLevel", new object[] { CS$<>8__locals1.newLevelState });
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameNewLevelAdded,
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			return await this.UpdateGame(new Func<Game, Task<ValueTuple<bool, object>>>(CS$<>8__locals1.<AddLevel>g__ApplyChange|0), changeData, this.cancellationToken);
		}

		// Token: 0x060000BF RID: 191 RVA: 0x000073C0 File Offset: 0x000055C0
		public async Task<bool> ReorderGameLevels(List<LevelReference> newOrder)
		{
			GameEditor.<>c__DisplayClass11_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass11_0();
			CS$<>8__locals1.newOrder = newOrder;
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameLevelReorder,
				Metadata = "",
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			bool flag;
			try
			{
				flag = await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<ReorderGameLevels>g__ReorderLevels|0), changeData, this.cancellationToken);
			}
			catch (Exception ex)
			{
				ErrorHandler.HandleError(ErrorCodes.GameEditor_ReorderingLevels, ex, true, false);
				flag = false;
			}
			return flag;
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x000056F3 File Offset: 0x000038F3
		public void SetLevelArchived(SerializableGuid levelId, bool levelArchived)
		{
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x0000740C File Offset: 0x0000560C
		public async Task<bool> SetPlayerCount(int min, int max)
		{
			GameEditor.<>c__DisplayClass13_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass13_0();
			CS$<>8__locals1.min = min;
			CS$<>8__locals1.max = max;
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GamePlayerCountUpdated,
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			return await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<SetPlayerCount>g__UpdatePlayerCount|0), changeData, this.cancellationToken);
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x00007460 File Offset: 0x00005660
		public async Task<bool> UpdateGameName(string newValue)
		{
			GameEditor.<>c__DisplayClass14_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass14_0();
			CS$<>8__locals1.newValue = newValue;
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameNameUpdated,
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			return await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<UpdateGameName>g__ApplyNameChange|0), changeData, this.cancellationToken);
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x000074AC File Offset: 0x000056AC
		public async Task<bool> UpdateDescription(string newValue, Action successCallback = null, Action failureCallback = null)
		{
			GameEditor.<>c__DisplayClass15_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass15_0();
			CS$<>8__locals1.newValue = newValue;
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameDescriptionUpdated,
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			return await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<UpdateDescription>g__ApplyDescriptionChange|0), changeData, this.cancellationToken);
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x000074F8 File Offset: 0x000056F8
		public async Task<bool> AddScreenshotsToGame(List<ScreenshotFileInstances> newValue)
		{
			GameEditor.<>c__DisplayClass16_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass16_0();
			CS$<>8__locals1.newValue = newValue;
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameScreenshotsAdded,
				Metadata = "",
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			bool flag;
			try
			{
				flag = await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<AddScreenshotsToGame>g__AddScreenshots|0), changeData, this.cancellationToken);
			}
			catch (Exception ex)
			{
				ErrorHandler.HandleError(ErrorCodes.GameEditor_AddingScreenshots, ex, true, false);
				flag = false;
			}
			return flag;
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x00007544 File Offset: 0x00005744
		public async Task<bool> RemoveGameScreenshotAt(int index)
		{
			GameEditor.<>c__DisplayClass17_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass17_0();
			CS$<>8__locals1.index = index;
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameScreenshotsRemoved,
				Metadata = "",
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			bool flag;
			try
			{
				flag = await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<RemoveGameScreenshotAt>g__RemoveScreenshotAt|0), changeData, this.cancellationToken);
			}
			catch (Exception ex)
			{
				ErrorHandler.HandleError(ErrorCodes.GameEditor_RemovingScreenshots, ex, true, false);
				flag = false;
			}
			return flag;
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x00007590 File Offset: 0x00005790
		public async Task<bool> ReorderGameScreenshot(List<ScreenshotFileInstances> newOrder)
		{
			GameEditor.<>c__DisplayClass18_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass18_0();
			CS$<>8__locals1.newOrder = newOrder;
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameScreenshotsReorder,
				Metadata = "",
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			bool flag;
			try
			{
				flag = await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<ReorderGameScreenshot>g__ReorderScreenshot|0), changeData, this.cancellationToken);
			}
			catch (Exception ex)
			{
				ErrorHandler.HandleError(ErrorCodes.GameEditor_ReorderingScreenshots, ex, true, false);
				flag = false;
			}
			return flag;
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x000075DC File Offset: 0x000057DC
		public async Task<bool> GetUpdatedGame(string gameId, Func<Game, Game, Task> success, Action failure, CancellationToken cancelToken = default(CancellationToken))
		{
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(gameId, "", null, false, 10);
			bool flag;
			if (cancelToken.IsCancellationRequested)
			{
				flag = false;
			}
			else
			{
				int maxRetries = 5;
				if (graphQlResult.HasErrors)
				{
					bool succeeded = false;
					if (Regex.IsMatch(graphQlResult.GetErrorMessage(0).Message, "Asset [A-Za-z0-9\\-\"]* version [0-9.\"]* not found"))
					{
						global::UnityEngine.Debug.LogWarning("Failed to get latest version of game (originally error 2024). Executing fallback workaround and re-trying");
						int attempt = 0;
						while (attempt < maxRetries)
						{
							await Task.Delay(500 + attempt * 100);
							if (cancelToken.IsCancellationRequested)
							{
								return false;
							}
							attempt++;
							graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(gameId, "", null, false, 10);
							if (!graphQlResult.HasErrors)
							{
								succeeded = true;
								break;
							}
						}
					}
					if (!succeeded)
					{
						ErrorHandler.HandleError(ErrorCodes.GameEditor_GetUpdatedGame_GraphQLError, graphQlResult.GetErrorMessage(0), true, false);
						failure();
						return false;
					}
				}
				try
				{
					if (this.ActiveGame == null)
					{
						flag = false;
					}
					else
					{
						Game activeGame = this.ActiveGame;
						Game game = GameLoader.Load(graphQlResult.GetDataMember().ToString());
						if (SemanticVersion.Parse(game.AssetVersion) > SemanticVersion.Parse(this.ActiveGame.AssetVersion))
						{
							this.ActiveGame = game;
							if (success != null)
							{
								success(this.ActiveGame, activeGame);
							}
						}
						flag = true;
					}
				}
				catch (Exception ex)
				{
					ErrorHandler.HandleError(ErrorCodes.GameEditor_GetGameInitialResolveConflict, ex, true, false);
					if (failure != null)
					{
						failure();
					}
					flag = false;
				}
			}
			return flag;
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x00007640 File Offset: 0x00005840
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
			GameLibrary.RemoveTerrainEntryResult result = new GameLibrary.RemoveTerrainEntryResult(false, -1);
			await this.UpdateGame(delegate(Game game)
			{
				result = base.<RemoveTerrainEntryFromGameLibrary>g__RemoveTerrainEntry|1(game);
				return Task.FromResult<bool>(result.Success);
			}, changeData, this.cancellationToken);
			return result;
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x00007694 File Offset: 0x00005894
		public async Task<bool> AddTerrainUsageToGameLibrary(SerializableGuid tilesetId, string version)
		{
			GameEditor.<>c__DisplayClass21_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass21_0();
			CS$<>8__locals1.tilesetId = tilesetId;
			CS$<>8__locals1.version = version;
			if (CS$<>8__locals1.tilesetId == SerializableGuid.Empty)
			{
				throw new ArgumentNullException("You cannot add a TerrainUsage without an ID!");
			}
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameLibraryTerrainAdded,
				Metadata = "",
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			return await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<AddTerrainUsageToGameLibrary>g__AddTerrainUsage|0), changeData, this.cancellationToken);
		}

		// Token: 0x060000CA RID: 202 RVA: 0x000076E8 File Offset: 0x000058E8
		public async Task<bool> AddTerrainUsagesToGameLibrary(IEnumerable<TerrainTilesetCosmeticAsset> terrainUsages)
		{
			AssetReference[] array = terrainUsages.Select((TerrainTilesetCosmeticAsset usage) => usage.ToAssetReference()).ToArray<AssetReference>();
			return await this.AddTerrainUsagesToGameLibrary(array);
		}

		// Token: 0x060000CB RID: 203 RVA: 0x00007734 File Offset: 0x00005934
		public async Task<bool> AddTerrainUsagesToGameLibrary(AssetReference[] assetReferences)
		{
			GameEditor.<>c__DisplayClass23_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass23_0();
			CS$<>8__locals1.assetReferences = assetReferences;
			if (CS$<>8__locals1.assetReferences.Any((AssetReference reference) => reference.AssetID == SerializableGuid.Empty || string.IsNullOrEmpty(reference.AssetVersion)))
			{
				throw new ArgumentNullException("You cannot add a TerrainUsage without a valid ID and version!");
			}
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameLibraryTerrainAdded,
				Metadata = "",
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			return await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<AddTerrainUsagesToGameLibrary>g__AddTerrainUsages|1), changeData, this.cancellationToken);
		}

		// Token: 0x060000CC RID: 204 RVA: 0x00007780 File Offset: 0x00005980
		public async Task<bool> SetTerrainUsageVersionInGameLibrary(SerializableGuid id, string newTerrainVersion)
		{
			GameEditor.<>c__DisplayClass24_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass24_0();
			CS$<>8__locals1.id = id;
			CS$<>8__locals1.newTerrainVersion = newTerrainVersion;
			if (CS$<>8__locals1.id == SerializableGuid.Empty)
			{
				throw new ArgumentNullException("You cannot set a TerrainUsage version without an ID!");
			}
			if (CS$<>8__locals1.newTerrainVersion.IsNullOrEmptyOrWhiteSpace())
			{
				throw new ArgumentNullException("You cannot set a TerrainUsage version without an version!");
			}
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameLibraryTerrainVersionChanged,
				Metadata = "",
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			return await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<SetTerrainUsageVersionInGameLibrary>g__SetTerrainUsageVersion|0), changeData, this.cancellationToken);
		}

		// Token: 0x060000CD RID: 205 RVA: 0x000077D4 File Offset: 0x000059D4
		public async Task<bool> SetPropVersionInGameLibrary(string id, string newAssetVersion)
		{
			GameEditor.<>c__DisplayClass25_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass25_0();
			CS$<>8__locals1.id = id;
			CS$<>8__locals1.newAssetVersion = newAssetVersion;
			if (CS$<>8__locals1.id == SerializableGuid.Empty)
			{
				throw new ArgumentNullException("You cannot set a Prop version without an ID!");
			}
			if (CS$<>8__locals1.newAssetVersion.IsNullOrEmptyOrWhiteSpace())
			{
				throw new ArgumentNullException("You cannot set a Prop version without an version!");
			}
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameLibraryPropVersionChanged,
				Metadata = "",
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			return await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<SetPropVersionInGameLibrary>g__SetPropVersion|0), changeData, this.cancellationToken);
		}

		// Token: 0x060000CE RID: 206 RVA: 0x00007828 File Offset: 0x00005A28
		public async Task<bool> SetAudioVersionInGameLibrary(string id, string newAssetVersion)
		{
			GameEditor.<>c__DisplayClass26_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass26_0();
			CS$<>8__locals1.id = id;
			CS$<>8__locals1.newAssetVersion = newAssetVersion;
			if (CS$<>8__locals1.id == SerializableGuid.Empty)
			{
				throw new ArgumentNullException("You cannot set a AudioAsset version without an ID!");
			}
			if (CS$<>8__locals1.newAssetVersion.IsNullOrEmptyOrWhiteSpace())
			{
				throw new ArgumentNullException("You cannot set a AudioAsset version without an version!");
			}
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameLibraryAudioVersionChanged,
				Metadata = "",
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			return await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<SetAudioVersionInGameLibrary>g__SetAudioVersion|0), changeData, this.cancellationToken);
		}

		// Token: 0x060000CF RID: 207 RVA: 0x0000787C File Offset: 0x00005A7C
		public async Task<bool> AddPropToGameLibrary(Prop prop)
		{
			return await this.AddPropToGameLibrary(prop.ToAssetReference());
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x000078C8 File Offset: 0x00005AC8
		public async Task<bool> AddPropsToGameLibrary(IEnumerable<Prop> props)
		{
			return await this.AddPropsToGameLibrary(props.Select((Prop prop) => prop.ToAssetReference()).ToArray<AssetReference>(), this.cancellationToken);
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x00007914 File Offset: 0x00005B14
		public async Task<bool> AddPropToGameLibrary(AssetReference assetReference)
		{
			return await this.AddPropToGameLibrary(assetReference, this.cancellationToken);
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x00007960 File Offset: 0x00005B60
		private async Task<bool> AddPropToGameLibrary(AssetReference assetReference, CancellationToken sessionToken)
		{
			GameEditor.<>c__DisplayClass30_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass30_0();
			CS$<>8__locals1.assetReference = assetReference;
			if (CS$<>8__locals1.assetReference.AssetID == SerializableGuid.Empty)
			{
				throw new ArgumentException("You cannot add an asset without an ID!");
			}
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameLibraryPropAdded,
				Metadata = "",
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			return await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<AddPropToGameLibrary>g__AddProp|0), changeData, sessionToken);
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x000079B4 File Offset: 0x00005BB4
		private async Task<bool> AddPropsToGameLibrary(AssetReference[] assetReferences, CancellationToken sessionToken)
		{
			GameEditor.<>c__DisplayClass31_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass31_0();
			CS$<>8__locals1.assetReferences = assetReferences;
			if (CS$<>8__locals1.assetReferences.Any((AssetReference reference) => reference.AssetID == SerializableGuid.Empty))
			{
				throw new ArgumentException("You cannot add an asset without an ID!");
			}
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameLibraryPropAdded,
				Metadata = "",
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			return await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<AddPropsToGameLibrary>g__AddProp|1), changeData, sessionToken);
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x00007A08 File Offset: 0x00005C08
		public async Task<bool> AddAudioToGameLibrary(AssetReference assetReference)
		{
			return await this.AddAudioToGameLibrary(assetReference, this.cancellationToken);
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x00007A54 File Offset: 0x00005C54
		private async Task<bool> AddAudioToGameLibrary(AssetReference assetReference, CancellationToken sessionToken)
		{
			GameEditor.<>c__DisplayClass33_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass33_0();
			CS$<>8__locals1.assetReference = assetReference;
			if (CS$<>8__locals1.assetReference.AssetID == SerializableGuid.Empty)
			{
				throw new ArgumentException("You cannot add an asset without an ID!");
			}
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameLibraryAudioAdded,
				Metadata = "",
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			return await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<AddAudioToGameLibrary>g__AddAudio|0), changeData, sessionToken);
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x00007AA8 File Offset: 0x00005CA8
		public async Task<bool> RemovePropFromGameLibrary(SerializableGuid assetID)
		{
			GameEditor.<>c__DisplayClass34_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass34_0();
			CS$<>8__locals1.assetID = assetID;
			if (CS$<>8__locals1.assetID == SerializableGuid.Empty)
			{
				throw new ArgumentNullException("You cannot remove a Prop without an ID!");
			}
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameLibraryPropRemoved,
				Metadata = "",
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			return await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<RemovePropFromGameLibrary>g__RemoveProp|0), changeData, this.cancellationToken);
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x00007AF4 File Offset: 0x00005CF4
		public async Task<bool> RemoveAudioFromGameLibrary(SerializableGuid assetID)
		{
			GameEditor.<>c__DisplayClass35_0 CS$<>8__locals1 = new GameEditor.<>c__DisplayClass35_0();
			CS$<>8__locals1.assetID = assetID;
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameLibraryAudioRemoved,
				Metadata = "",
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			return await this.UpdateGame(new Func<Game, Task<bool>>(CS$<>8__locals1.<RemoveAudioFromGameLibrary>g__RemoveAsset|0), changeData, this.cancellationToken);
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x00007B40 File Offset: 0x00005D40
		public async Task<bool> UpdateAllGameLibrary()
		{
			ChangeData changeData = new ChangeData
			{
				ChangeType = ChangeType.GameLibraryUpdateAll,
				Metadata = "",
				UserId = EndlessServices.Instance.CloudService.ActiveUserId
			};
			bool flag;
			try
			{
				global::UnityEngine.Debug.Log("Update Game Complete");
				flag = await this.UpdateGame(new Func<Game, Task<bool>>(GameEditor.<UpdateAllGameLibrary>g__UpdateAll|36_0), changeData, this.cancellationToken);
			}
			catch (Exception ex)
			{
				global::UnityEngine.Debug.LogException(ex);
				ErrorHandler.HandleError(ErrorCodes.GameEditor_UpdateAll, ex, true, false);
				flag = false;
			}
			return flag;
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x00007B84 File Offset: 0x00005D84
		private async Task<bool> UpdateGame(Func<Game, Task<bool>> updateFunction, ChangeData changeData, CancellationToken sessionToken)
		{
			Game oldGame = this.ActiveGame.Clone();
			Game game = this.ActiveGame.Clone();
			SemanticVersion expectedVersion = SemanticVersion.Parse(game.AssetVersion).IncrementPatch();
			game.AssetVersion = expectedVersion.ToString();
			bool flag = await updateFunction(game);
			bool flag2;
			if (sessionToken.IsCancellationRequested || !flag)
			{
				flag2 = false;
			}
			else
			{
				game.RevisionMetaData = new RevisionMetaData(new HashSet<ChangeData> { changeData });
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync(game, false, false);
				if (graphQlResult.HasErrors)
				{
					if (!(graphQlResult.GetErrorMessage(0) is DuplicateAssetVersionException))
					{
						throw graphQlResult.GetErrorMessage(0);
					}
					Stopwatch waitStopwatch = new Stopwatch();
					waitStopwatch.Start();
					while (!sessionToken.IsCancellationRequested && SemanticVersion.Parse(this.ActiveGame.AssetVersion) < expectedVersion)
					{
						if (waitStopwatch.ElapsedMilliseconds > 60000L)
						{
							TaskAwaiter<bool> taskAwaiter = this.GetUpdatedGame(this.ActiveGame.AssetID, new Func<Game, Game, Task>(NetworkBehaviourSingleton<CreatorManager>.Instance.HandleGameUpdated), null, sessionToken).GetAwaiter();
							if (!taskAwaiter.IsCompleted)
							{
								await taskAwaiter;
								TaskAwaiter<bool> taskAwaiter2;
								taskAwaiter = taskAwaiter2;
								taskAwaiter2 = default(TaskAwaiter<bool>);
							}
							if (taskAwaiter.GetResult() && SemanticVersion.Parse(this.ActiveGame.AssetVersion) >= expectedVersion)
							{
								global::UnityEngine.Debug.LogWarning("Timed out waiting for game version, but was able to recover!");
								break;
							}
							throw new TimeoutException("Failed to get expected game version!");
						}
						else
						{
							await Task.Yield();
						}
					}
					if (sessionToken.IsCancellationRequested)
					{
						flag2 = false;
					}
					else
					{
						global::UnityEngine.Debug.Log(string.Format("Expected version retrieved: {0} (expected {1})", this.ActiveGame.AssetVersion, expectedVersion));
						flag2 = await this.UpdateGame(updateFunction, changeData, sessionToken);
					}
				}
				else
				{
					this.ActiveGame = GameLoader.Load(graphQlResult.GetDataMember().ToString());
					await NetworkBehaviourSingleton<CreatorManager>.Instance.HandleGameUpdated(this.ActiveGame, oldGame);
					flag2 = true;
				}
			}
			return flag2;
		}

		// Token: 0x060000DA RID: 218 RVA: 0x00007BE0 File Offset: 0x00005DE0
		private async Task<bool> UpdateGame([TupleElementNames(new string[] { "updateNeeded", "data" })] Func<Game, Task<ValueTuple<bool, object>>> updateFunction, ChangeData changeData, CancellationToken sessionToken)
		{
			Game oldGame = this.ActiveGame.Clone();
			Game game = this.ActiveGame.Clone();
			SemanticVersion expectedVersion = SemanticVersion.Parse(game.AssetVersion).IncrementPatch();
			game.AssetVersion = expectedVersion.ToString();
			ValueTuple<bool, object> valueTuple = await updateFunction(game);
			bool flag;
			if (sessionToken.IsCancellationRequested || !valueTuple.Item1)
			{
				flag = false;
			}
			else
			{
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync(valueTuple.Item2, false, false);
				if (graphQlResult.HasErrors)
				{
					if (!(graphQlResult.GetErrorMessage(0) is DuplicateAssetVersionException))
					{
						throw graphQlResult.GetErrorMessage(0);
					}
					Stopwatch waitStopwatch = new Stopwatch();
					waitStopwatch.Start();
					while (!sessionToken.IsCancellationRequested && SemanticVersion.Parse(this.ActiveGame.AssetVersion) < expectedVersion)
					{
						if (waitStopwatch.ElapsedMilliseconds > 60000L)
						{
							TaskAwaiter<bool> taskAwaiter = this.GetUpdatedGame(this.ActiveGame.AssetID, new Func<Game, Game, Task>(NetworkBehaviourSingleton<CreatorManager>.Instance.HandleGameUpdated), null, sessionToken).GetAwaiter();
							if (!taskAwaiter.IsCompleted)
							{
								await taskAwaiter;
								TaskAwaiter<bool> taskAwaiter2;
								taskAwaiter = taskAwaiter2;
								taskAwaiter2 = default(TaskAwaiter<bool>);
							}
							if (taskAwaiter.GetResult() && SemanticVersion.Parse(this.ActiveGame.AssetVersion) >= expectedVersion)
							{
								global::UnityEngine.Debug.LogWarning("Timed out waiting for game version, but was able to recover!");
								break;
							}
							throw new TimeoutException("Failed to get expected game version!");
						}
						else
						{
							await Task.Yield();
						}
					}
					if (sessionToken.IsCancellationRequested)
					{
						flag = false;
					}
					else
					{
						global::UnityEngine.Debug.Log(string.Format("Expected version retrieved: {0} (expected {1})", this.ActiveGame.AssetVersion, expectedVersion));
						flag = await this.UpdateGame(updateFunction, changeData, sessionToken);
					}
				}
				else
				{
					this.ActiveGame = GameLoader.Load(graphQlResult.GetDataMember().ToString());
					await NetworkBehaviourSingleton<CreatorManager>.Instance.HandleGameUpdated(this.ActiveGame, oldGame);
					flag = true;
				}
			}
			return flag;
		}

		// Token: 0x060000DC RID: 220 RVA: 0x00007C5C File Offset: 0x00005E5C
		[CompilerGenerated]
		internal static async Task<bool> <UpdateAllGameLibrary>g__UpdateAll|36_0(Game game)
		{
			foreach (AssetReference prop in game.GameLibrary.PropReferences)
			{
				global::UnityEngine.Debug.Log("Updating prop: " + prop.AssetID);
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(prop.AssetID, "", null, false, 10);
				if (graphQlResult.HasErrors)
				{
					throw graphQlResult.GetErrorMessage(0);
				}
				Prop prop2 = JsonConvert.DeserializeObject<Prop>(graphQlResult.GetDataMember().ToString());
				global::UnityEngine.Debug.Log("Current Version: " + prop.AssetVersion + ", New Version: " + prop.AssetVersion);
				prop.AssetVersion = prop2.AssetVersion;
				prop = null;
			}
			IEnumerator<AssetReference> enumerator = null;
			foreach (TerrainUsage terrainUsage in game.GameLibrary.TerrainEntries)
			{
				if (terrainUsage.TerrainAssetReference != null)
				{
					global::UnityEngine.Debug.Log("Updating terrain: " + terrainUsage.TerrainAssetReference.AssetID);
					GraphQlResult graphQlResult2 = await EndlessServices.Instance.CloudService.GetAssetAsync(terrainUsage.TerrainAssetReference.AssetID, "", null, false, 10);
					if (graphQlResult2.HasErrors)
					{
						throw graphQlResult2.GetErrorMessage(0);
					}
					global::UnityEngine.Debug.Log("Current Version: " + terrainUsage.TerrainAssetReference.AssetVersion + ", New Version: " + terrainUsage.TerrainAssetReference.AssetVersion);
					TerrainTilesetCosmeticAsset terrainTilesetCosmeticAsset = JsonConvert.DeserializeObject<TerrainTilesetCosmeticAsset>(graphQlResult2.GetDataMember().ToString());
					terrainUsage.TerrainAssetReference.AssetVersion = terrainTilesetCosmeticAsset.AssetVersion;
					terrainUsage = null;
				}
			}
			List<TerrainUsage>.Enumerator enumerator2 = default(List<TerrainUsage>.Enumerator);
			return true;
		}

		// Token: 0x040000C2 RID: 194
		private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

		// Token: 0x040000C3 RID: 195
		private CancellationToken cancellationToken;

		// Token: 0x040000C4 RID: 196
		private List<LevelState> levelStates = new List<LevelState>();
	}
}
