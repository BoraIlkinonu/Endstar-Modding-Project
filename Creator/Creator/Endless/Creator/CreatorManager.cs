using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Creator.UI;
using Endless.Data;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.Serialization;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.DiffMatchPatch;
using Runtime.Shared;
using Runtime.Shared.Matchmaking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Endless.Creator
{
	// Token: 0x02000016 RID: 22
	public class CreatorManager : NetworkBehaviourSingleton<CreatorManager>
	{
		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000040 RID: 64 RVA: 0x00003208 File Offset: 0x00001408
		// (remove) Token: 0x06000041 RID: 65 RVA: 0x00003240 File Offset: 0x00001440
		public event Action OnRepopulate;

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x06000042 RID: 66 RVA: 0x00003278 File Offset: 0x00001478
		// (remove) Token: 0x06000043 RID: 67 RVA: 0x000032B0 File Offset: 0x000014B0
		public event Action OnTerrainRepopulated;

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x06000044 RID: 68 RVA: 0x000032E8 File Offset: 0x000014E8
		// (remove) Token: 0x06000045 RID: 69 RVA: 0x00003320 File Offset: 0x00001520
		public event Action OnPropsRepopulated;

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000046 RID: 70 RVA: 0x00003355 File Offset: 0x00001555
		// (set) Token: 0x06000047 RID: 71 RVA: 0x0000335D File Offset: 0x0000155D
		public RpcReceiveState RpcReceiveState { get; private set; } = RpcReceiveState.Ignore;

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000048 RID: 72 RVA: 0x00003366 File Offset: 0x00001566
		public SaveLoadManager SaveLoadManager
		{
			get
			{
				return this.saveLoadManager;
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000049 RID: 73 RVA: 0x0000336E File Offset: 0x0000156E
		public LevelEditor LevelEditor
		{
			get
			{
				return this.levelEditor;
			}
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00003376 File Offset: 0x00001576
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			this.userRolesForLevel = new List<UserRole>();
			this.saveLoadManager = new SaveLoadManager();
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00003394 File Offset: 0x00001594
		private void Update()
		{
			if (NetworkManager.Singleton.IsServer && this.saveLoadManager != null)
			{
				this.saveLoadManager.UpdateSaveLoad();
			}
		}

		// Token: 0x0600004C RID: 76 RVA: 0x000033B5 File Offset: 0x000015B5
		private void Start()
		{
			GraphQlRequest.OnWebsocketReconnected += this.OnWebsocketReconnected;
			ExitManager.OnQuitting += this.CancelTokens;
		}

		// Token: 0x0600004D RID: 77 RVA: 0x000033DC File Offset: 0x000015DC
		private async void OnWebsocketReconnected()
		{
			if (MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame != null)
			{
				MonoBehaviourSingleton<GameEditor>.Instance.GetUpdatedGame(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID, async delegate(Endless.Gameplay.LevelEditing.Level.Game newGame, Endless.Gameplay.LevelEditing.Level.Game oldGame)
				{
					SemanticVersion semanticVersion = SemanticVersion.Parse(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetVersion);
					if (SemanticVersion.Parse(newGame.AssetVersion) > semanticVersion)
					{
						await this.HandleGameUpdated(newGame, oldGame);
					}
				}, delegate
				{
					Debug.LogError("Failed to get updated game!");
				}, this.gameUpdatedCancelTokenSource.Token);
			}
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00003414 File Offset: 0x00001614
		public void EnteringCreator()
		{
			if (base.IsServer)
			{
				this.autoRefreshUserRoleCancelTokenSource = new CancellationTokenSource();
				this.AutoRefreshUserRoles(this.autoRefreshUserRoleCancelTokenSource.Token);
				this.canSendClientLevelDetails = false;
				Debug.Log("setting canSendClientLevelDetails");
				NetworkManager.Singleton.OnClientConnectedCallback += this.HandleNewPlayer;
			}
			if (!this.originalSubscribedAssetId.IsEmpty)
			{
				MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(this.originalSubscribedAssetId, new Action<IReadOnlyList<UserRole>>(this.OnLevelRightsChanged));
			}
			MatchInfo localMatch = MatchmakingClientController.Instance.LocalMatch;
			this.originalSubscribedAssetId = ((localMatch != null) ? localMatch.GetLevelId() : null);
			MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(this.originalSubscribedAssetId, MatchmakingClientController.Instance.ActiveGameId, new Action<IReadOnlyList<UserRole>>(this.OnLevelRightsChanged));
		}

		// Token: 0x0600004F RID: 79 RVA: 0x000034DC File Offset: 0x000016DC
		public void LeavingCreator(bool forceSave = false)
		{
			NetworkBehaviourSingleton<UserScriptingConsole>.Instance.ClearMessages();
			if (!this.originalSubscribedAssetId.IsEmpty)
			{
				MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(this.originalSubscribedAssetId, new Action<IReadOnlyList<UserRole>>(this.OnLevelRightsChanged));
				this.originalSubscribedAssetId = SerializableGuid.Empty;
			}
			CancellationTokenSource cancellationTokenSource = this.autoRefreshUserRoleCancelTokenSource;
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
			this.clientsAwaitingLevelDetails.Clear();
			if (this.refreshUserRolesCoroutine != null)
			{
				base.StopCoroutine(this.refreshUserRolesCoroutine);
				this.refreshUserRolesCoroutine = null;
			}
			if (base.IsServer)
			{
				this.canSendClientLevelDetails = false;
				if (NetworkManager.Singleton)
				{
					NetworkManager.Singleton.OnClientConnectedCallback -= this.HandleNewPlayer;
				}
			}
			else
			{
				this.cachedRpcs.Clear();
			}
			this.RpcReceiveState = RpcReceiveState.Ignore;
			if (forceSave)
			{
				this.saveLoadManager.ForceSaveIfNeeded();
			}
			this.OnCreatorEnded.Invoke();
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000035BC File Offset: 0x000017BC
		private void OnLevelRightsChanged(IReadOnlyList<UserRole> roles)
		{
			int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			Debug.Log("CreatorManager.OnLevelRightsChanged: Active Roles:");
			foreach (UserRole userRole in roles)
			{
				Debug.Log(string.Format("{0}. Is this user local user ({1})? {2}", userRole, activeUserId, (userRole.UserId == activeUserId) ? "Yes" : "No"));
			}
			UserRole userRole2 = roles.FirstOrDefault((UserRole role) => role.UserId == activeUserId);
			if (userRole2 == null)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Kicked From Match", null, "You no longer have rights to edit the game.", UIModalManagerStackActions.ClearStack, Array.Empty<UIModalGenericViewAction>());
				MonoBehaviourSingleton<ConnectionActions>.Instance.EndMatch(null);
				return;
			}
			this.LocalClientRoleChanged.Invoke(userRole2.Role);
		}

		// Token: 0x06000051 RID: 81 RVA: 0x000036A8 File Offset: 0x000018A8
		private void HandleNewPlayer(ulong clientId)
		{
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage)
			{
				return;
			}
			Debug.Log(string.Format("Patch: Client {0}", clientId));
			ClientRpcParams clientRpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new List<ulong> { clientId }
				}
			};
			this.ReceiveCachedLevelDetails_ClientRpc(this.saveLoadManager.CachedLevelStateId, this.saveLoadManager.CachedLevelStateVersion, clientRpcParams);
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00003726 File Offset: 0x00001926
		public void CreatorLoaded()
		{
			this.OnCreatorStarted.Invoke();
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LoadExistingWires(false);
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00003744 File Offset: 0x00001944
		public void LeavingSession()
		{
			this.gameUpdatedCancelTokenSource.Cancel();
			this.gameUpdatedCancelTokenSource = new CancellationTokenSource();
			if (this.assetVersionCancellationSource != null)
			{
				this.assetVersionCancellationSource.Cancel();
				this.assetVersionCancellationSource = null;
			}
			this.userRolesForLevel.Clear();
			if (NetworkManager.Singleton)
			{
				NetworkManager.Singleton.OnClientConnectedCallback -= this.HandleNewPlayer;
			}
			this.saveLoadManager.ForceSaveIfNeeded();
			this.OnLeavingSession.Invoke();
			MonoBehaviourSingleton<StageManager>.Instance.LeavingSession();
		}

		// Token: 0x06000054 RID: 84 RVA: 0x000037D0 File Offset: 0x000019D0
		public async Task PerformInitialLevelLoad(SerializableGuid levelId, Action<string> progressCallback, CancellationToken cancelToken)
		{
			MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("PerformInitialLevelLoad");
			this.RpcReceiveState = RpcReceiveState.Allow;
			Debug.Log(string.Format("{0}.{1} loading LevelId: {2}", "CreatorManager", "PerformInitialLevelLoad", levelId));
			this.canSendClientLevelDetails = false;
			if (progressCallback != null)
			{
				progressCallback("Fetching latest data...");
			}
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(levelId, "", null, false, 10);
			if (!cancelToken.IsCancellationRequested)
			{
				if (graphQlResult.HasErrors)
				{
					MonoBehaviourSingleton<ConnectionActions>.Instance.EndMatch(null);
					Exception errorMessage = graphQlResult.GetErrorMessage(0);
					ErrorHandler.HandleError((errorMessage is TimeoutException) ? ErrorCodes.CreatorManager_InitialLevelFetchTimeout : ErrorCodes.CreatorManager_GetLevelAssetInitialLoad, errorMessage, true, true);
				}
				else
				{
					MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("LevelFetch", "PerformInitialLevelLoad");
					string text = graphQlResult.GetDataMember().ToString();
					LevelState levelState;
					try
					{
						levelState = LevelStateLoader.Load(text);
					}
					catch (Exception ex)
					{
						ErrorHandler.HandleError(ErrorCodes.CreatorManager_InvalidLevelState, new Exception("Result from Get Level call was not returned as expected?\n" + text, ex), true, true);
						return;
					}
					MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("LevelLoad", "PerformInitialLevelLoad");
					if (progressCallback != null)
					{
						progressCallback("Loading library prefabs...");
					}
					await MonoBehaviourSingleton<StageManager>.Instance.LoadLibraryPrefabs(levelState, cancelToken, progressCallback);
					MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("LoadLibraryPrefabs", "PerformInitialLevelLoad");
					if (!cancelToken.IsCancellationRequested)
					{
						if (levelState.RevisionMetaData == null)
						{
							levelState.RevisionMetaData = new RevisionMetaData();
						}
						levelState.RevisionMetaData.Changes.Clear();
						this.canSendClientLevelDetails = true;
						if (progressCallback != null)
						{
							progressCallback("Validating received data...");
						}
						ValueTuple<LevelState, SanitizationResult> valueTuple = this.Sanitize(levelState);
						LevelState item = valueTuple.Item1;
						SanitizationResult item2 = valueTuple.Item2;
						if (item2 != SanitizationResult.Failure)
						{
							if (item2 == SanitizationResult.Sanitized)
							{
								GraphQlResult graphQlResult2 = await EndlessServices.Instance.CloudService.UpdateAssetAsync(item, MatchmakingClientController.Instance.ActiveGameId, false, false);
								if (cancelToken.IsCancellationRequested)
								{
									return;
								}
								if (graphQlResult2.HasErrors)
								{
									ErrorHandler.HandleError(ErrorCodes.CreatorManager_FailedBadDataCleanup, graphQlResult2.GetErrorMessage(0), true, true);
								}
								levelState = JsonConvert.DeserializeObject<LevelState>(graphQlResult2.GetDataMember().ToString());
							}
							this.saveLoadManager.SetCachedLevelState(levelState.AssetID, levelState.AssetVersion);
							if (this.serverLoadLevelCancellationSource != null)
							{
								this.serverLoadLevelCancellationSource.Cancel();
							}
							MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Sanitize", "PerformInitialLevelLoad");
							this.serverLoadLevelCancellationSource = new CancellationTokenSource();
							await MonoBehaviourSingleton<StageManager>.Instance.LoadLevel(levelState, false, this.serverLoadLevelCancellationSource.Token, progressCallback);
							NetworkBehaviourSingleton<UISaveStatusManager>.Instance.UpdateLevelVersion(levelState.AssetVersion, false);
							NetworkBehaviourSingleton<UISaveStatusManager>.Instance.UpdateGameVersion(MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetVersion, false);
							MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("LoadLevel", "PerformInitialLevelLoad");
							if (!cancelToken.IsCancellationRequested)
							{
								this.SendDetailsToClients(this.clientsAwaitingLevelDetails);
								this.clientsAwaitingLevelDetails.Clear();
								MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("PerformInitialLevelLoad");
							}
						}
					}
				}
			}
		}

		// Token: 0x06000055 RID: 85 RVA: 0x0000382C File Offset: 0x00001A2C
		private ValueTuple<LevelState, SanitizationResult> Sanitize(LevelState levelState)
		{
			bool flag = false;
			try
			{
				flag = this.SanitizeMemberChanges(levelState);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			bool flag2 = false;
			try
			{
				flag2 = this.SanitizeWireBundles(levelState);
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			if (flag || flag2)
			{
				SemanticVersion semanticVersion = SemanticVersion.Parse(levelState.AssetVersion);
				levelState.AssetVersion = string.Format("{0}.{1}.{2}", semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch + 1);
				levelState.RevisionMetaData.Changes.Clear();
				levelState.RevisionMetaData.RevisionTimestamp = DateTime.Now.Ticks;
				levelState.RevisionMetaData.Changes.Add(new ChangeData
				{
					UserId = EndlessServices.Instance.CloudService.ActiveUserId,
					Metadata = null,
					ChangeType = ChangeType.BadDataAutoFix
				});
				Debug.Log(string.Format("{0}: {1}", "sanitizeMemberChangesOccurred", flag));
				Debug.Log(string.Format("{0}: {1}", "sanitizeWireBundlesOccurred", flag2));
				return new ValueTuple<LevelState, SanitizationResult>(levelState, SanitizationResult.Sanitized);
			}
			return new ValueTuple<LevelState, SanitizationResult>(levelState, SanitizationResult.NoOp);
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00003968 File Offset: 0x00001B68
		private bool SanitizeMemberChanges(LevelState levelState)
		{
			bool flag = false;
			foreach (PropEntry propEntry in levelState.PropEntries)
			{
				foreach (ComponentEntry componentEntry in propEntry.ComponentEntries)
				{
					int i = componentEntry.Changes.Count - 1;
					while (i >= 0)
					{
						Type type = Type.GetType(componentEntry.AssemblyQualifiedName);
						MemberChange memberChange2 = componentEntry.Changes[i];
						ComponentDefinition componentDefinition;
						if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetComponentDefinition(type, out componentDefinition) || !componentDefinition.HasMember(memberChange2))
						{
							goto IL_00BF;
						}
						MemberInfo[] member = type.GetMember(memberChange2.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (member[0].MemberType == MemberTypes.Property)
						{
							if (!((member[0] as PropertyInfo).SetMethod != null))
							{
								goto IL_00BF;
							}
						}
						else if (member.Length == 0)
						{
							goto IL_00BF;
						}
						IL_00CF:
						i--;
						continue;
						IL_00BF:
						flag = true;
						componentEntry.Changes.RemoveAt(i);
						goto IL_00CF;
					}
				}
				PropLibrary.RuntimePropInfo runtimePropInfo;
				if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(propEntry.AssetId, out runtimePropInfo) && !runtimePropInfo.IsMissingObject)
				{
					EndlessScriptComponent scriptComponent = runtimePropInfo.EndlessProp.ScriptComponent;
					if (scriptComponent)
					{
						for (int j = propEntry.LuaMemberChanges.Count - 1; j >= 0; j--)
						{
							MemberChange memberChange = propEntry.LuaMemberChanges[j];
							if (scriptComponent.Script.InspectorValues.FirstOrDefault((InspectorScriptValue inspectorValue) => inspectorValue.Name == memberChange.MemberName && CreatorManager.<SanitizeMemberChanges>g__AcceptsDataType|62_1(inspectorValue.DataType, memberChange.DataType)) == null)
							{
								propEntry.LuaMemberChanges.RemoveAt(j);
								flag = true;
							}
						}
					}
				}
			}
			return flag;
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00003B5C File Offset: 0x00001D5C
		private bool SanitizeWireBundles(LevelState levelState)
		{
			IEnumerable<PropLibrary.RuntimePropInfo> enumerable = from prop in MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetAllRuntimeProps()
				where !prop.IsLoading && !prop.IsMissingObject
				select prop;
			Dictionary<SerializableGuid, Script> dictionary = new Dictionary<SerializableGuid, Script>();
			foreach (PropLibrary.RuntimePropInfo runtimePropInfo in enumerable)
			{
				dictionary.TryAdd(runtimePropInfo.EndlessProp.Prop.AssetID, runtimePropInfo.EndlessProp.ScriptComponent.Script);
			}
			bool flag = false;
			for (int i = levelState.WireBundles.Count - 1; i >= 0; i--)
			{
				WireBundle wireBundle = levelState.WireBundles[i];
				PropEntry propEntry = levelState.GetPropEntry(wireBundle.EmitterInstanceId);
				PropEntry propEntry2 = levelState.GetPropEntry(wireBundle.ReceiverInstanceId);
				SerializableGuid assetId = propEntry.AssetId;
				SerializableGuid assetId2 = propEntry2.AssetId;
				PropLibrary.RuntimePropInfo runtimePropInfo2;
				PropLibrary.RuntimePropInfo runtimePropInfo3;
				if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out runtimePropInfo2) || runtimePropInfo2.IsMissingObject)
				{
					levelState.RemoveBundle(levelState.WireBundles[i]);
				}
				else if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId2, out runtimePropInfo3) || runtimePropInfo3.IsMissingObject)
				{
					levelState.RemoveBundle(levelState.WireBundles[i]);
				}
				else
				{
					int j = wireBundle.Wires.Count - 1;
					while (j >= 0)
					{
						WireEntry wireEntry = wireBundle.Wires[j];
						Script script;
						if (!dictionary.TryGetValue(propEntry.AssetId, out script))
						{
							goto IL_01C8;
						}
						WireOrganizationData wireOrganizationData = script.EventOrganizationData.FirstOrDefault((WireOrganizationData orgData) => orgData.MemberName == wireEntry.EmitterMemberName && orgData.ComponentId == wireEntry.EmitterComponentTypeId);
						if (wireOrganizationData == null || !wireOrganizationData.Disabled)
						{
							goto IL_01C8;
						}
						flag = true;
						wireBundle.Wires.RemoveAt(j);
						IL_05C4:
						j--;
						continue;
						IL_01C8:
						Script script2;
						if (dictionary.TryGetValue(propEntry2.AssetId, out script2))
						{
							WireOrganizationData wireOrganizationData2 = script2.ReceiverOrganizationData.FirstOrDefault((WireOrganizationData orgData) => orgData.MemberName == wireEntry.ReceiverMemberName && orgData.ComponentId == wireEntry.ReceiverComponentTypeId);
							if (wireOrganizationData2 != null && wireOrganizationData2.Disabled)
							{
								flag = true;
								wireBundle.Wires.RemoveAt(j);
								goto IL_05C4;
							}
						}
						bool flag2 = string.IsNullOrEmpty(wireEntry.EmitterComponentAssemblyQualifiedTypeName);
						bool flag3 = string.IsNullOrEmpty(wireEntry.ReceiverComponentAssemblyQualifiedTypeName);
						Type type;
						if (flag2)
						{
							type = typeof(EndlessScriptComponent);
						}
						else
						{
							type = Type.GetType(wireEntry.EmitterComponentAssemblyQualifiedTypeName);
						}
						Type type2;
						if (flag3)
						{
							type2 = typeof(EndlessScriptComponent);
						}
						else
						{
							type2 = Type.GetType(wireEntry.ReceiverComponentAssemblyQualifiedTypeName);
						}
						int[] array;
						if (flag2)
						{
							EndlessScriptComponent scriptComponent = runtimePropInfo2.EndlessProp.ScriptComponent;
							if (!scriptComponent)
							{
								Debug.LogWarning("Emitter Component isn't a script component");
								flag = true;
								wireBundle.Wires.RemoveAt(j);
								goto IL_05C4;
							}
							EndlessEventInfo eventInfo = scriptComponent.GetEventInfo(wireEntry.EmitterMemberName);
							if (eventInfo == null)
							{
								Debug.LogWarning("luaEvent " + wireEntry.EmitterMemberName + " no longer exists in the script");
								flag = true;
								wireBundle.Wires.RemoveAt(j);
								goto IL_05C4;
							}
							array = eventInfo.ParamList.Select((EndlessParameterInfo param) => param.DataType).ToArray<int>();
						}
						else
						{
							ComponentDefinition componentDefinition;
							if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetComponentDefinition(type, out componentDefinition))
							{
								Debug.LogWarning("The entire component definition could not be found " + type.Name + " " + wireEntry.EmitterComponentAssemblyQualifiedTypeName);
								flag = true;
								wireBundle.Wires.RemoveAt(j);
								goto IL_05C4;
							}
							if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetDataTypeSignature(componentDefinition.AvailableEvents, wireEntry.EmitterMemberName, out array))
							{
								Debug.LogWarning("The signature of the emitter couldn't be found");
								flag = true;
								wireBundle.Wires.RemoveAt(j);
								goto IL_05C4;
							}
						}
						int[] array2;
						if (flag3)
						{
							EndlessScriptComponent scriptComponent2 = runtimePropInfo3.EndlessProp.ScriptComponent;
							if (!scriptComponent2)
							{
								Debug.LogWarning("Receiver Component isn't a script component");
								flag = true;
								wireBundle.Wires.RemoveAt(j);
								goto IL_05C4;
							}
							EndlessEventInfo endlessEventInfo = scriptComponent2.Script.Receivers.FirstOrDefault((EndlessEventInfo r) => r.MemberName == wireEntry.ReceiverMemberName);
							if (endlessEventInfo == null)
							{
								Debug.LogWarning("luaEvent no longer exists in the script");
								flag = true;
								wireBundle.Wires.RemoveAt(j);
								goto IL_05C4;
							}
							array2 = endlessEventInfo.ParamList.Select((EndlessParameterInfo param) => param.DataType).ToArray<int>();
						}
						else
						{
							ComponentDefinition componentDefinition2;
							if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetComponentDefinition(type2, out componentDefinition2))
							{
								Debug.LogWarning("The entire component definition could not be found");
								flag = true;
								wireBundle.Wires.RemoveAt(j);
								goto IL_05C4;
							}
							if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetDataTypeSignature(componentDefinition2.AvailableReceivers, wireEntry.ReceiverMemberName, out array2))
							{
								Debug.LogWarning("The signature of the receiver couldn't be found");
								flag = true;
								wireBundle.Wires.RemoveAt(j);
								goto IL_05C4;
							}
						}
						if (array2.Length == 0)
						{
							if (wireEntry.StaticParameters.Length != 0)
							{
								Debug.LogWarning("Receiver receives no parameters but we had leftover static parameters");
								flag = true;
								wireBundle.Wires.RemoveAt(j);
								goto IL_05C4;
							}
							goto IL_05C4;
						}
						else if (wireEntry.StaticParameters.Length != 0)
						{
							int[] array3 = wireEntry.StaticParameters.Select((StoredParameter staticParam) => staticParam.DataType).ToArray<int>();
							if (!MonoBehaviourSingleton<StageManager>.Instance.SignaturesMatch(array2, array3))
							{
								Debug.LogWarning("Receiver did not match the static parameter data type signature");
								flag = true;
								wireBundle.Wires.RemoveAt(j);
								goto IL_05C4;
							}
							goto IL_05C4;
						}
						else
						{
							if (!MonoBehaviourSingleton<StageManager>.Instance.SignaturesMatch(array2, array))
							{
								Debug.LogWarning("Receiver did not match the emitter's parameter data type signature");
								flag = true;
								wireBundle.Wires.RemoveAt(j);
								goto IL_05C4;
							}
							goto IL_05C4;
						}
					}
					if (wireBundle.Wires.Count == 0)
					{
						flag = true;
						levelState.RemoveBundle(levelState.WireBundles[i]);
					}
				}
			}
			return flag;
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00004180 File Offset: 0x00002380
		private async void ValidateConnectedUsers()
		{
			List<ulong> clientIdsTokick = new List<ulong>();
			string text = "Validating connected users to level id: ";
			MatchInfo localMatch = MatchmakingClientController.Instance.LocalMatch;
			Debug.Log(text + ((localMatch != null) ? localMatch.GetLevelId() : null));
			foreach (int connectedUserId in NetworkBehaviourSingleton<UserIdManager>.Instance.ConnectedUserIds)
			{
				TaskAwaiter<UserRoleRequestResult> taskAwaiter = MonoBehaviourSingleton<RightsManager>.Instance.HasRoleOrGreaterForAssetAsync(MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID, connectedUserId, Roles.Viewer, null, false).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					TaskAwaiter<UserRoleRequestResult> taskAwaiter2;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<UserRoleRequestResult>);
				}
				if (!taskAwaiter.GetResult().PassedCheck)
				{
					Debug.Log(string.Format("Connected user with ID: {0} did not have a valid role and is being removed.", connectedUserId));
					clientIdsTokick.Add(NetworkBehaviourSingleton<UserIdManager>.Instance.GetClientId(connectedUserId));
				}
			}
			IEnumerator<int> enumerator = null;
			if (clientIdsTokick.Any((ulong clientId) => base.IsServer && clientId == NetworkManager.Singleton.LocalClientId))
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Kicked From Match", null, "You no longer have rights to edit this game.", UIModalManagerStackActions.ClearStack, Array.Empty<UIModalGenericViewAction>());
				MonoBehaviourSingleton<ConnectionActions>.Instance.EndMatch(null);
			}
			else
			{
				foreach (ulong num in clientIdsTokick)
				{
					MatchSession.Instance.KickClient(num, "You do not have permission to edit this game");
				}
			}
		}

		// Token: 0x06000059 RID: 89 RVA: 0x000041B8 File Offset: 0x000023B8
		private async void AutoRefreshUserRoles(CancellationToken cancelToken)
		{
			if (base.IsServer)
			{
				TimeSpan delay = TimeSpan.FromSeconds(30.0);
				for (;;)
				{
					await Task.Delay(delay, cancelToken).ContinueWith(delegate(Task x)
					{
					});
					if (cancelToken.IsCancellationRequested || !MatchmakingClientController.Instance || MatchmakingClientController.Instance.LocalMatch == null)
					{
						break;
					}
					Debug.Log(string.Format("{0}.{1}: Executing GetAllUserRolesForAssetAsync with Level Id: {2}, GameId: {3}", new object[]
					{
						"CreatorManager",
						"AutoRefreshUserRoles",
						MatchmakingClientController.Instance.LocalMatch.GetLevelId(),
						MatchmakingClientController.Instance.ActiveGameId
					}));
					await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(MatchmakingClientController.Instance.LocalMatch.GetLevelId(), MatchmakingClientController.Instance.ActiveGameId, null, true);
					if (cancelToken.IsCancellationRequested)
					{
						break;
					}
					this.ValidateConnectedUsers();
				}
			}
		}

		// Token: 0x0600005A RID: 90 RVA: 0x000041F8 File Offset: 0x000023F8
		public async Task RetrieveAndLoadServerLevel(CancellationToken cancelToken, Action<string> progressCallback)
		{
			this.RpcReceiveState = RpcReceiveState.Ignore;
			this.waitingForTargetLevelData = true;
			this.waitingForLevel = true;
			this.cloudLevelState = null;
			this.loadedLevelData = string.Empty;
			this.levelDataFragments.Clear();
			this.RequestLevelDetails_ServerRpc(default(ServerRpcParams));
			if (progressCallback != null)
			{
				progressCallback("Awaiting level info from server...");
			}
			while (this.waitingForTargetLevelData || this.waitingForLevel)
			{
				await Task.Yield();
				if (cancelToken.IsCancellationRequested)
				{
					return;
				}
			}
			LevelState levelState = await Task.Run<LevelState>(() => LevelStateLoader.Load(this.loadedLevelData));
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}
			this.loadedLevelData = string.Empty;
			levelState.RevisionMetaData.Changes.Clear();
			await MonoBehaviourSingleton<StageManager>.Instance.LoadLevel(levelState, true, cancelToken, progressCallback);
		}

		// Token: 0x0600005B RID: 91 RVA: 0x0000424B File Offset: 0x0000244B
		public IEnumerator SendLevelData(byte[] bytes, ClientRpcParams rpcParams)
		{
			Debug.Log(string.Format("Sending level data: {0} bytes | {1} fragments | {2} seconds ", bytes.Length, bytes.Length / 8192, (float)(bytes.Length / 8192) * 0.05f));
			int byteIndex = 0;
			int fragmentID = 0;
			this.StartingLevelFragment_ClientRpc(rpcParams);
			byte[] fragment = new byte[8192];
			int bytesWritten = 0;
			while (byteIndex < bytes.Length)
			{
				int num = 0;
				int num2;
				while (num < 8192 && byteIndex < bytes.Length)
				{
					fragment[num] = bytes[byteIndex];
					num2 = byteIndex;
					byteIndex = num2 + 1;
					num2 = bytesWritten;
					bytesWritten = num2 + 1;
					num++;
				}
				this.LevelFragmentSend_ClientRpc(new CreatorManager.LevelDataFragment(fragmentID, fragment, bytesWritten), rpcParams);
				bytesWritten = 0;
				num2 = fragmentID;
				fragmentID = num2 + 1;
				yield return new WaitForSecondsRealtime(0.05f);
			}
			foreach (ulong num3 in rpcParams.Send.TargetClientIds)
			{
				Debug.Log(string.Format("Finished sending level fragments for clientId: {0}", num3));
			}
			this.EndingLevelFragments_ClientRpc(true, rpcParams);
			yield break;
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00004268 File Offset: 0x00002468
		[ClientRpc(Delivery = RpcDelivery.Reliable)]
		private void StartingLevelFragment_ClientRpc(ClientRpcParams rpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1438970432U, rpcParams, RpcDelivery.Reliable);
				base.__endSendClientRpc(ref fastBufferWriter, 1438970432U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.loadedLevelData = string.Empty;
			this.levelDataFragments.Clear();
			Debug.Log("Starting level fragment send.");
		}

		// Token: 0x0600005D RID: 93 RVA: 0x0000435C File Offset: 0x0000255C
		[ClientRpc(Delivery = RpcDelivery.Reliable)]
		private void LevelFragmentSend_ClientRpc(CreatorManager.LevelDataFragment data, ClientRpcParams rpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1020812181U, rpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<CreatorManager.LevelDataFragment>(in data, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendClientRpc(ref fastBufferWriter, 1020812181U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			Debug.Log(string.Format("Level Fragment received: {0}", data.Index));
			this.levelDataFragments.Add(data);
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00004470 File Offset: 0x00002670
		[ClientRpc(Delivery = RpcDelivery.Reliable)]
		private void EndingLevelFragments_ClientRpc(bool complete, ClientRpcParams rpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2786258557U, rpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<bool>(in complete, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 2786258557U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (complete)
			{
				this.loadedLevelData = string.Empty;
				for (int i = 0; i < this.levelDataFragments.Count; i++)
				{
					this.loadedLevelData += Encoding.ASCII.GetString(this.levelDataFragments[i].Bytes);
				}
				this.waitingForLevel = false;
				Debug.Log(string.Format("All level fragments received: {0}", this.levelDataFragments.Count));
			}
		}

		// Token: 0x0600005F RID: 95 RVA: 0x000045D4 File Offset: 0x000027D4
		private async void SendDetailsToClient(ClientRpcParams rpcParams)
		{
			this.ReceiveCachedLevelDetails_ClientRpc(this.saveLoadManager.CachedLevelStateId, this.saveLoadManager.CachedLevelStateVersion, rpcParams);
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage == null)
			{
				Debug.Log("StageManager.Instance.ActiveStage is null");
			}
			else
			{
				string text = await MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.SerializeLevelAsync();
				this.EnterCachingState_ClientRpc(rpcParams);
				this.activeLevelSendCoroutines.Add(base.StartCoroutine(this.SendLevelData(Encoding.ASCII.GetBytes(text), rpcParams)));
			}
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00004614 File Offset: 0x00002814
		[ClientRpc]
		private void EnterCachingState_ClientRpc(ClientRpcParams rpcParams)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(919501897U, rpcParams, RpcDelivery.Reliable);
				base.__endSendClientRpc(ref fastBufferWriter, 919501897U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.RpcReceiveState = RpcReceiveState.Cache;
		}

		// Token: 0x06000061 RID: 97 RVA: 0x000046F0 File Offset: 0x000028F0
		private void SendDetailsToClients(List<ulong> awaitingClients)
		{
			if (awaitingClients.Count == 0)
			{
				return;
			}
			foreach (ulong num in awaitingClients)
			{
				Debug.Log(string.Format("Starting level fragments for client: {0}", num));
			}
			ClientRpcParams clientRpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new List<ulong>(awaitingClients)
				}
			};
			this.SendDetailsToClient(clientRpcParams);
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00004784 File Offset: 0x00002984
		[ServerRpc(RequireOwnership = false)]
		private void RequestLevelDetails_ServerRpc(ServerRpcParams rpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(1636973562U, rpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 1636973562U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			Debug.Log("RequestLevelDetails_ServerRpc:: Client is requesting level details");
			if (!this.canSendClientLevelDetails)
			{
				this.clientsAwaitingLevelDetails.Add(rpcParams.Receive.SenderClientId);
				return;
			}
			Debug.Log(string.Format("Starting level fragments for client: {0}", rpcParams.Receive.SenderClientId));
			ClientRpcParams clientRpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new List<ulong> { rpcParams.Receive.SenderClientId }
				}
			};
			this.SendDetailsToClient(clientRpcParams);
		}

		// Token: 0x06000063 RID: 99 RVA: 0x000048E0 File Offset: 0x00002AE0
		[ClientRpc]
		public void ReceiveCachedLevelDetails_ClientRpc(SerializableGuid levelId, string version, ClientRpcParams rpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3369545666U, rpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in levelId, default(FastBufferWriter.ForNetworkSerializable));
				bool flag = version != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(version, false);
				}
				base.__endSendClientRpc(ref fastBufferWriter, 3369545666U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.RetrieveLevelStateOnClient(levelId, version);
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00004A14 File Offset: 0x00002C14
		private async void RetrieveLevelStateOnClient(SerializableGuid levelId, string version)
		{
			Debug.Log(string.Format("{0}.{1} retrieving level {2}, version: {3} from back end", new object[] { "CreatorManager", "ReceiveCachedLevelDetails_ClientRpc", levelId, version }));
			MonoBehaviourSingleton<StageManager>.Instance.PrepareForLevelChange(levelId);
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(levelId, version, null, false, 60);
			if (graphQlResult.HasErrors)
			{
				Exception errorMessage = graphQlResult.GetErrorMessage(0);
				ErrorHandler.HandleError((errorMessage is TimeoutException) ? ErrorCodes.CreatorManager_ErrorGettingLevelTimeout : ErrorCodes.CreatorManager_ErrorGettingLevel, errorMessage, true, true);
			}
			else
			{
				this.cloudLevelState = LevelStateLoader.Load(graphQlResult.GetDataMember().ToString());
				this.waitingForTargetLevelData = false;
			}
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00004A5C File Offset: 0x00002C5C
		public async Task ApplyCachedRpcs(CancellationToken cancelToken)
		{
			for (int index = 0; index < this.cachedRpcs.Count; index++)
			{
				try
				{
					Action action = this.cachedRpcs[index];
					if (action != null)
					{
						action();
					}
					await Task.Yield();
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
				if (cancelToken.IsCancellationRequested)
				{
					return;
				}
			}
			this.cachedRpcs.Clear();
			this.RpcReceiveState = RpcReceiveState.Allow;
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00004AA7 File Offset: 0x00002CA7
		public void ClearCachedRPCs()
		{
			this.cachedRpcs.Clear();
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00004AB4 File Offset: 0x00002CB4
		public void AddCachedRpc(Action cachedRpc)
		{
			if (this.RpcReceiveState != RpcReceiveState.Cache)
			{
				return;
			}
			this.cachedRpcs.Add(cachedRpc);
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00004ACB File Offset: 0x00002CCB
		private void OnMatchmakingStarted()
		{
			this.UnHookOnMatchStartEventsAndHideScreenCover();
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00004AD3 File Offset: 0x00002CD3
		private void UnHookOnMatchStartEventsAndHideScreenCover()
		{
			MatchmakingClientController.OnMatchmakingStarted -= this.OnMatchmakingStarted;
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00004AE8 File Offset: 0x00002CE8
		[ServerRpc(RequireOwnership = false)]
		public void ForcePlayersToReload_ServerRpc()
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				ServerRpcParams serverRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(236590287U, serverRpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 236590287U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			MonoBehaviourSingleton<StageManager>.Instance.FlushLoadedAndSpawnedStages(true);
			this.ForcePlayersToReload_ClientRpc();
			UnityEvent levelReverted = this.LevelReverted;
			if (levelReverted == null)
			{
				return;
			}
			levelReverted.Invoke();
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00004BDC File Offset: 0x00002DDC
		[ClientRpc]
		private void ForcePlayersToReload_ClientRpc()
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2990444095U, clientRpcParams, RpcDelivery.Reliable);
				base.__endSendClientRpc(ref fastBufferWriter, 2990444095U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			MonoBehaviourSingleton<StageManager>.Instance.FlushLoadedAndSpawnedStages(false);
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00004CBC File Offset: 0x00002EBC
		public async Task<bool> UserCanEditLevel(ulong clientId)
		{
			int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(clientId);
			return await this.UserCanEditLevel(userId);
		}

		// Token: 0x0600006D RID: 109 RVA: 0x00004D08 File Offset: 0x00002F08
		public async Task<bool> UserCanEditLevel(int userId)
		{
			TaskAwaiter<UserRoleRequestResult> taskAwaiter = MonoBehaviourSingleton<RightsManager>.Instance.HasRoleOrGreaterForAssetAsync(MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID, userId, Roles.Editor, null, false).GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				TaskAwaiter<UserRoleRequestResult> taskAwaiter2;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<UserRoleRequestResult>);
			}
			return taskAwaiter.GetResult().PassedCheck;
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00004D4C File Offset: 0x00002F4C
		public async Task HandleGameUpdated(Endless.Gameplay.LevelEditing.Level.Game newGame, Endless.Gameplay.LevelEditing.Level.Game oldGame)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage != null && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.RuntimePalette != null)
			{
				SemanticVersion semanticVersion = SemanticVersion.Parse(newGame.AssetVersion);
				SemanticVersion semanticVersion2 = SemanticVersion.Parse(oldGame.AssetVersion);
				if (semanticVersion <= semanticVersion2)
				{
					Debug.LogWarning("Received Game Versions out of order.");
				}
				else
				{
					NetworkBehaviourSingleton<UISaveStatusManager>.Instance.UpdateGameVersion(newGame.AssetVersion, false);
					Debug.Log(string.Concat(new string[] { "Updating game ", newGame.AssetID, " from ", oldGame.AssetVersion, " to ", newGame.AssetVersion }));
					bool didTerrainRepopulate = MonoBehaviourSingleton<StageManager>.Instance.ActiveTerrainPalette.IsRepopulateRequired(newGame, oldGame);
					if (didTerrainRepopulate)
					{
						CancellationTokenSource cancellationTokenSource = this.terrainRepopulationCancelTokenSource;
						if (cancellationTokenSource != null)
						{
							cancellationTokenSource.Cancel();
						}
						this.terrainRepopulationCancelTokenSource = new CancellationTokenSource();
						await MonoBehaviourSingleton<StageManager>.Instance.ActiveTerrainPalette.Repopulate(newGame, oldGame, this.terrainRepopulationCancelTokenSource.Token);
					}
					bool didPropsRepopulate = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.IsRepopulateRequired(newGame, oldGame);
					bool propsWereSanitized = false;
					if (didPropsRepopulate)
					{
						CancellationTokenSource cancellationTokenSource2 = this.propLibraryCancelTokenSource;
						if (cancellationTokenSource2 != null)
						{
							cancellationTokenSource2.Cancel();
						}
						this.propLibraryCancelTokenSource = new CancellationTokenSource();
						propsWereSanitized = await MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.Repopulate(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage, this.propLibraryCancelTokenSource.Token);
					}
					bool didAudioRepopulate = MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.IsRepopulateRequired(newGame, oldGame);
					if (didAudioRepopulate)
					{
						if (this.audioLibraryCancelTokenSource != null)
						{
							this.audioLibraryCancelTokenSource.Cancel();
						}
						this.audioLibraryCancelTokenSource = new CancellationTokenSource();
						await MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.RepopulateAudio(this.audioLibraryCancelTokenSource.Token);
					}
					if (propsWereSanitized)
					{
						if (base.IsServer)
						{
							this.saveLoadManager.SaveLevel();
						}
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.ValidatePhysicalWiresStillAlive();
					}
					if (didPropsRepopulate || didTerrainRepopulate || didAudioRepopulate)
					{
						if (didPropsRepopulate && MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool.ToolType == ToolType.Prop)
						{
							Action onPropsRepopulated = this.OnPropsRepopulated;
							if (onPropsRepopulated != null)
							{
								onPropsRepopulated();
							}
						}
						if (didTerrainRepopulate && MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool.ToolType == ToolType.Painting)
						{
							Action onTerrainRepopulated = this.OnTerrainRepopulated;
							if (onTerrainRepopulated != null)
							{
								onTerrainRepopulated();
							}
						}
						Action onRepopulate = this.OnRepopulate;
						if (onRepopulate != null)
						{
							onRepopulate();
						}
					}
				}
			}
		}

		// Token: 0x0600006F RID: 111 RVA: 0x00004DA0 File Offset: 0x00002FA0
		public async void AssetUpdated(AssetUpdatedMetaData assetUpdatedMetaData)
		{
			if (assetUpdatedMetaData == null || MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame == null || assetUpdatedMetaData.AssetType != "game" || assetUpdatedMetaData.AssetId != MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID)
			{
				if (assetUpdatedMetaData == null)
				{
					Debug.LogException(new NullReferenceException("assetUpdatedMetaData was null in handling a GraphQL matchmaking response which should never be possible"));
				}
			}
			else
			{
				SemanticVersion semanticVersion = SemanticVersion.Parse(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetVersion);
				if (SemanticVersion.Parse(assetUpdatedMetaData.LastAssetVersion) > semanticVersion)
				{
					await MonoBehaviourSingleton<GameEditor>.Instance.GetUpdatedGame(assetUpdatedMetaData.AssetId, new Func<Endless.Gameplay.LevelEditing.Level.Game, Endless.Gameplay.LevelEditing.Level.Game, Task>(this.HandleGameUpdated), delegate
					{
						Debug.LogError("AssetUpdated to get updated game!");
					}, this.gameUpdatedCancelTokenSource.Token);
				}
			}
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00004DDF File Offset: 0x00002FDF
		public void SetRPCReceiveState(RpcReceiveState rpcReceiveState)
		{
			this.RpcReceiveState = rpcReceiveState;
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00004DE8 File Offset: 0x00002FE8
		public CancellationToken GetAssetVersionCancellationToken()
		{
			CancellationTokenSource cancellationTokenSource = this.assetVersionCancellationSource;
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
			this.assetVersionCancellationSource = new CancellationTokenSource();
			return this.assetVersionCancellationSource.Token;
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00004E11 File Offset: 0x00003011
		public override void OnDestroy()
		{
			base.OnDestroy();
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00004E1C File Offset: 0x0000301C
		private void CancelTokens()
		{
			CancellationTokenSource cancellationTokenSource = this.assetVersionCancellationSource;
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
			CancellationTokenSource cancellationTokenSource2 = this.autoRefreshUserRoleCancelTokenSource;
			if (cancellationTokenSource2 != null)
			{
				cancellationTokenSource2.Cancel();
			}
			CancellationTokenSource cancellationTokenSource3 = this.propLibraryCancelTokenSource;
			if (cancellationTokenSource3 != null)
			{
				cancellationTokenSource3.Cancel();
			}
			CancellationTokenSource cancellationTokenSource4 = this.terrainRepopulationCancelTokenSource;
			if (cancellationTokenSource4 != null)
			{
				cancellationTokenSource4.Cancel();
			}
			CancellationTokenSource cancellationTokenSource5 = this.serverLoadLevelCancellationSource;
			if (cancellationTokenSource5 == null)
			{
				return;
			}
			cancellationTokenSource5.Cancel();
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00004F54 File Offset: 0x00003154
		[CompilerGenerated]
		internal static bool <SanitizeMemberChanges>g__IsValueType|62_0(Type type)
		{
			while (type.BaseType != null && type.BaseType != typeof(object))
			{
				if (type == typeof(ValueType))
				{
					return true;
				}
				type = type.BaseType;
			}
			return false;
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00004FA8 File Offset: 0x000031A8
		[CompilerGenerated]
		internal static bool <SanitizeMemberChanges>g__AcceptsDataType|62_1(int inspectorDataTypeId, int memberChangeDataTypeId)
		{
			if (inspectorDataTypeId == memberChangeDataTypeId)
			{
				return true;
			}
			Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(inspectorDataTypeId);
			Type typeFromId2 = EndlessTypeMapping.Instance.GetTypeFromId(memberChangeDataTypeId);
			Type baseType = typeFromId.BaseType;
			Type baseType2 = typeFromId2.BaseType;
			return !(baseType == null) && !(baseType2 == null) && !CreatorManager.<SanitizeMemberChanges>g__IsValueType|62_0(typeFromId) && !CreatorManager.<SanitizeMemberChanges>g__IsValueType|62_0(typeFromId2) && (baseType == baseType2 && baseType != typeof(object)) && baseType2 != typeof(object);
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00005058 File Offset: 0x00003258
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00005070 File Offset: 0x00003270
		protected override void __initializeRpcs()
		{
			base.__registerRpc(1438970432U, new NetworkBehaviour.RpcReceiveHandler(CreatorManager.__rpc_handler_1438970432), "StartingLevelFragment_ClientRpc");
			base.__registerRpc(1020812181U, new NetworkBehaviour.RpcReceiveHandler(CreatorManager.__rpc_handler_1020812181), "LevelFragmentSend_ClientRpc");
			base.__registerRpc(2786258557U, new NetworkBehaviour.RpcReceiveHandler(CreatorManager.__rpc_handler_2786258557), "EndingLevelFragments_ClientRpc");
			base.__registerRpc(919501897U, new NetworkBehaviour.RpcReceiveHandler(CreatorManager.__rpc_handler_919501897), "EnterCachingState_ClientRpc");
			base.__registerRpc(1636973562U, new NetworkBehaviour.RpcReceiveHandler(CreatorManager.__rpc_handler_1636973562), "RequestLevelDetails_ServerRpc");
			base.__registerRpc(3369545666U, new NetworkBehaviour.RpcReceiveHandler(CreatorManager.__rpc_handler_3369545666), "ReceiveCachedLevelDetails_ClientRpc");
			base.__registerRpc(236590287U, new NetworkBehaviour.RpcReceiveHandler(CreatorManager.__rpc_handler_236590287), "ForcePlayersToReload_ServerRpc");
			base.__registerRpc(2990444095U, new NetworkBehaviour.RpcReceiveHandler(CreatorManager.__rpc_handler_2990444095), "ForcePlayersToReload_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00005168 File Offset: 0x00003368
		private static void __rpc_handler_1438970432(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CreatorManager)target).StartingLevelFragment_ClientRpc(client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600007D RID: 125 RVA: 0x000051C8 File Offset: 0x000033C8
		private static void __rpc_handler_1020812181(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			CreatorManager.LevelDataFragment levelDataFragment;
			reader.ReadValueSafe<CreatorManager.LevelDataFragment>(out levelDataFragment, default(FastBufferWriter.ForNetworkSerializable));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CreatorManager)target).LevelFragmentSend_ClientRpc(levelDataFragment, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00005248 File Offset: 0x00003448
		private static void __rpc_handler_2786258557(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CreatorManager)target).EndingLevelFragments_ClientRpc(flag, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600007F RID: 127 RVA: 0x000052C8 File Offset: 0x000034C8
		private static void __rpc_handler_919501897(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CreatorManager)target).EnterCachingState_ClientRpc(client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00005328 File Offset: 0x00003528
		private static void __rpc_handler_1636973562(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CreatorManager)target).RequestLevelDetails_ServerRpc(server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00005388 File Offset: 0x00003588
		private static void __rpc_handler_3369545666(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			string text = null;
			if (flag)
			{
				reader.ReadValueSafe(out text, false);
			}
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CreatorManager)target).ReceiveCachedLevelDetails_ClientRpc(serializableGuid, text, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00005444 File Offset: 0x00003644
		private static void __rpc_handler_236590287(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CreatorManager)target).ForcePlayersToReload_ServerRpc();
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00005498 File Offset: 0x00003698
		private static void __rpc_handler_2990444095(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CreatorManager)target).ForcePlayersToReload_ClientRpc();
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000084 RID: 132 RVA: 0x000054E9 File Offset: 0x000036E9
		protected internal override string __getTypeName()
		{
			return "CreatorManager";
		}

		// Token: 0x04000035 RID: 53
		private const float FRAGMENT_SEND_DELAY = 0.05f;

		// Token: 0x04000036 RID: 54
		private const int FRAGMENT_BYTES = 8192;

		// Token: 0x0400003A RID: 58
		[SerializeField]
		private LevelEditor levelEditor;

		// Token: 0x0400003B RID: 59
		private SaveLoadManager saveLoadManager;

		// Token: 0x0400003C RID: 60
		private List<Action> cachedRpcs = new List<Action>();

		// Token: 0x0400003D RID: 61
		private bool waitingForTargetLevelData;

		// Token: 0x0400003E RID: 62
		private bool waitingForLevel;

		// Token: 0x0400003F RID: 63
		private string loadedLevelData;

		// Token: 0x04000040 RID: 64
		private Patch[] patches;

		// Token: 0x04000041 RID: 65
		private LevelState cloudLevelState;

		// Token: 0x04000042 RID: 66
		private bool canSendClientLevelDetails;

		// Token: 0x04000043 RID: 67
		private SerializableGuid previousProjectId;

		// Token: 0x04000044 RID: 68
		private SerializableGuid targetLevelId;

		// Token: 0x04000045 RID: 69
		private List<UserRole> userRolesForLevel;

		// Token: 0x04000046 RID: 70
		private CancellationTokenSource terrainRepopulationCancelTokenSource;

		// Token: 0x04000047 RID: 71
		private CancellationTokenSource propLibraryCancelTokenSource;

		// Token: 0x04000048 RID: 72
		private CancellationTokenSource audioLibraryCancelTokenSource;

		// Token: 0x04000049 RID: 73
		private CancellationTokenSource autoRefreshUserRoleCancelTokenSource;

		// Token: 0x0400004A RID: 74
		private CancellationTokenSource gameUpdatedCancelTokenSource = new CancellationTokenSource();

		// Token: 0x0400004B RID: 75
		private SerializableGuid originalSubscribedAssetId;

		// Token: 0x0400004C RID: 76
		private List<ulong> clientsAwaitingLevelDetails = new List<ulong>();

		// Token: 0x0400004D RID: 77
		private Coroutine refreshUserRolesCoroutine;

		// Token: 0x0400004E RID: 78
		private Endless.Gameplay.LevelEditing.Level.Game gamePreRepopulate;

		// Token: 0x0400004F RID: 79
		public UnityEvent OnCreatorStarted = new UnityEvent();

		// Token: 0x04000050 RID: 80
		public UnityEvent OnCreatorEnded = new UnityEvent();

		// Token: 0x04000051 RID: 81
		public UnityEvent OnLeavingSession = new UnityEvent();

		// Token: 0x04000052 RID: 82
		[FormerlySerializedAs("LocalClientRightsChanged")]
		public UnityEvent<Roles> LocalClientRoleChanged = new UnityEvent<Roles>();

		// Token: 0x04000054 RID: 84
		public UnityEvent LevelReverted;

		// Token: 0x04000055 RID: 85
		private CancellationTokenSource assetVersionCancellationSource;

		// Token: 0x04000056 RID: 86
		private CancellationTokenSource serverLoadLevelCancellationSource;

		// Token: 0x04000057 RID: 87
		private List<CreatorManager.LevelDataFragment> levelDataFragments = new List<CreatorManager.LevelDataFragment>();

		// Token: 0x04000058 RID: 88
		private List<Coroutine> activeLevelSendCoroutines = new List<Coroutine>();

		// Token: 0x02000017 RID: 23
		private struct LevelDataFragment : INetworkSerializable
		{
			// Token: 0x06000085 RID: 133 RVA: 0x000054F0 File Offset: 0x000036F0
			public LevelDataFragment(int index, byte[] bytes, int bytesWritten)
			{
				this.Index = index;
				this.Bytes = bytes;
				this.BytesWritten = bytesWritten;
			}

			// Token: 0x06000086 RID: 134 RVA: 0x00005508 File Offset: 0x00003708
			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
			{
				if (serializer.IsWriter)
				{
					Compression.SerializeInt<T>(serializer, this.Index);
					Compression.SerializeInt<T>(serializer, this.BytesWritten);
					for (int i = 0; i < this.Bytes.Length; i++)
					{
						if (i >= this.BytesWritten)
						{
							return;
						}
						byte b = this.Bytes[i];
						serializer.SerializeValue(ref b);
					}
				}
				else
				{
					this.Index = Compression.DeserializeInt<T>(serializer);
					this.BytesWritten = Compression.DeserializeInt<T>(serializer);
					this.Bytes = new byte[this.BytesWritten];
					for (int j = 0; j < this.Bytes.Length; j++)
					{
						byte b2 = 0;
						serializer.SerializeValue(ref b2);
						this.Bytes[j] = b2;
					}
				}
			}

			// Token: 0x04000059 RID: 89
			public int Index;

			// Token: 0x0400005A RID: 90
			public byte[] Bytes;

			// Token: 0x0400005B RID: 91
			public int BytesWritten;
		}
	}
}
