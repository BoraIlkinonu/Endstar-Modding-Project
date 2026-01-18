using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace Endless.Creator;

public class CreatorManager : NetworkBehaviourSingleton<CreatorManager>
{
	private struct LevelDataFragment : INetworkSerializable
	{
		public int Index;

		public byte[] Bytes;

		public int BytesWritten;

		public LevelDataFragment(int index, byte[] bytes, int bytesWritten)
		{
			Index = index;
			Bytes = bytes;
			BytesWritten = bytesWritten;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			if (serializer.IsWriter)
			{
				Compression.SerializeInt(serializer, Index);
				Compression.SerializeInt(serializer, BytesWritten);
				for (int i = 0; i < Bytes.Length && i < BytesWritten; i++)
				{
					byte value = Bytes[i];
					serializer.SerializeValue(ref value);
				}
				return;
			}
			Index = Compression.DeserializeInt(serializer);
			BytesWritten = Compression.DeserializeInt(serializer);
			Bytes = new byte[BytesWritten];
			for (int j = 0; j < Bytes.Length; j++)
			{
				byte value2 = 0;
				serializer.SerializeValue(ref value2);
				Bytes[j] = value2;
			}
		}
	}

	private const float FRAGMENT_SEND_DELAY = 0.05f;

	private const int FRAGMENT_BYTES = 8192;

	[SerializeField]
	private LevelEditor levelEditor;

	private SaveLoadManager saveLoadManager;

	private List<Action> cachedRpcs = new List<Action>();

	private bool waitingForTargetLevelData;

	private bool waitingForLevel;

	private string loadedLevelData;

	private Patch[] patches;

	private LevelState cloudLevelState;

	private bool canSendClientLevelDetails;

	private SerializableGuid previousProjectId;

	private SerializableGuid targetLevelId;

	private List<UserRole> userRolesForLevel;

	private CancellationTokenSource terrainRepopulationCancelTokenSource;

	private CancellationTokenSource propLibraryCancelTokenSource;

	private CancellationTokenSource audioLibraryCancelTokenSource;

	private CancellationTokenSource autoRefreshUserRoleCancelTokenSource;

	private CancellationTokenSource gameUpdatedCancelTokenSource = new CancellationTokenSource();

	private SerializableGuid originalSubscribedAssetId;

	private List<ulong> clientsAwaitingLevelDetails = new List<ulong>();

	private Coroutine refreshUserRolesCoroutine;

	private Endless.Gameplay.LevelEditing.Level.Game gamePreRepopulate;

	public UnityEvent OnCreatorStarted = new UnityEvent();

	public UnityEvent OnCreatorEnded = new UnityEvent();

	public UnityEvent OnLeavingSession = new UnityEvent();

	[FormerlySerializedAs("LocalClientRightsChanged")]
	public UnityEvent<Roles> LocalClientRoleChanged = new UnityEvent<Roles>();

	public UnityEvent LevelReverted;

	private CancellationTokenSource assetVersionCancellationSource;

	private CancellationTokenSource serverLoadLevelCancellationSource;

	private List<LevelDataFragment> levelDataFragments = new List<LevelDataFragment>();

	private List<Coroutine> activeLevelSendCoroutines = new List<Coroutine>();

	public RpcReceiveState RpcReceiveState { get; private set; } = RpcReceiveState.Ignore;

	public SaveLoadManager SaveLoadManager => saveLoadManager;

	public LevelEditor LevelEditor => levelEditor;

	public event Action OnRepopulate;

	public event Action OnTerrainRepopulated;

	public event Action OnPropsRepopulated;

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		userRolesForLevel = new List<UserRole>();
		saveLoadManager = new SaveLoadManager();
	}

	private void Update()
	{
		if (NetworkManager.Singleton.IsServer && saveLoadManager != null)
		{
			saveLoadManager.UpdateSaveLoad();
		}
	}

	private void Start()
	{
		GraphQlRequest.OnWebsocketReconnected += OnWebsocketReconnected;
		ExitManager.OnQuitting += CancelTokens;
	}

	private async void OnWebsocketReconnected()
	{
		if (MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame == null)
		{
			return;
		}
		MonoBehaviourSingleton<GameEditor>.Instance.GetUpdatedGame(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID, async delegate(Endless.Gameplay.LevelEditing.Level.Game newGame, Endless.Gameplay.LevelEditing.Level.Game oldGame)
		{
			SemanticVersion semanticVersion = SemanticVersion.Parse(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetVersion);
			if (SemanticVersion.Parse(newGame.AssetVersion) > semanticVersion)
			{
				await HandleGameUpdated(newGame, oldGame);
			}
		}, delegate
		{
			Debug.LogError("Failed to get updated game!");
		}, gameUpdatedCancelTokenSource.Token);
	}

	public void EnteringCreator()
	{
		if (base.IsServer)
		{
			autoRefreshUserRoleCancelTokenSource = new CancellationTokenSource();
			AutoRefreshUserRoles(autoRefreshUserRoleCancelTokenSource.Token);
			canSendClientLevelDetails = false;
			Debug.Log("setting canSendClientLevelDetails");
			NetworkManager.Singleton.OnClientConnectedCallback += HandleNewPlayer;
		}
		if (!originalSubscribedAssetId.IsEmpty)
		{
			MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(originalSubscribedAssetId, OnLevelRightsChanged);
		}
		originalSubscribedAssetId = MatchmakingClientController.Instance.LocalMatch?.GetLevelId();
		MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(originalSubscribedAssetId, MatchmakingClientController.Instance.ActiveGameId, OnLevelRightsChanged);
	}

	public void LeavingCreator(bool forceSave = false)
	{
		NetworkBehaviourSingleton<UserScriptingConsole>.Instance.ClearMessages();
		if (!originalSubscribedAssetId.IsEmpty)
		{
			MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(originalSubscribedAssetId, OnLevelRightsChanged);
			originalSubscribedAssetId = SerializableGuid.Empty;
		}
		autoRefreshUserRoleCancelTokenSource?.Cancel();
		clientsAwaitingLevelDetails.Clear();
		if (refreshUserRolesCoroutine != null)
		{
			StopCoroutine(refreshUserRolesCoroutine);
			refreshUserRolesCoroutine = null;
		}
		if (base.IsServer)
		{
			canSendClientLevelDetails = false;
			if ((bool)NetworkManager.Singleton)
			{
				NetworkManager.Singleton.OnClientConnectedCallback -= HandleNewPlayer;
			}
		}
		else
		{
			cachedRpcs.Clear();
		}
		RpcReceiveState = RpcReceiveState.Ignore;
		if (forceSave)
		{
			saveLoadManager.ForceSaveIfNeeded();
		}
		OnCreatorEnded.Invoke();
	}

	private void OnLevelRightsChanged(IReadOnlyList<UserRole> roles)
	{
		int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
		Debug.Log("CreatorManager.OnLevelRightsChanged: Active Roles:");
		foreach (UserRole role in roles)
		{
			Debug.Log(string.Format("{0}. Is this user local user ({1})? {2}", role, activeUserId, (role.UserId == activeUserId) ? "Yes" : "No"));
		}
		UserRole userRole = roles.FirstOrDefault((UserRole role) => role.UserId == activeUserId);
		if (userRole == null)
		{
			MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Kicked From Match", null, "You no longer have rights to edit the game.", UIModalManagerStackActions.ClearStack);
			MonoBehaviourSingleton<ConnectionActions>.Instance.EndMatch();
		}
		else
		{
			LocalClientRoleChanged.Invoke(userRole.Role);
		}
	}

	private void HandleNewPlayer(ulong clientId)
	{
		if ((bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage)
		{
			Debug.Log($"Patch: Client {clientId}");
			ClientRpcParams rpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new List<ulong> { clientId }
				}
			};
			ReceiveCachedLevelDetails_ClientRpc(saveLoadManager.CachedLevelStateId, saveLoadManager.CachedLevelStateVersion, rpcParams);
		}
	}

	public void CreatorLoaded()
	{
		OnCreatorStarted.Invoke();
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LoadExistingWires();
	}

	public void LeavingSession()
	{
		gameUpdatedCancelTokenSource.Cancel();
		gameUpdatedCancelTokenSource = new CancellationTokenSource();
		if (assetVersionCancellationSource != null)
		{
			assetVersionCancellationSource.Cancel();
			assetVersionCancellationSource = null;
		}
		userRolesForLevel.Clear();
		if ((bool)NetworkManager.Singleton)
		{
			NetworkManager.Singleton.OnClientConnectedCallback -= HandleNewPlayer;
		}
		saveLoadManager.ForceSaveIfNeeded();
		OnLeavingSession.Invoke();
		MonoBehaviourSingleton<StageManager>.Instance.LeavingSession();
	}

	public async Task PerformInitialLevelLoad(SerializableGuid levelId, Action<string> progressCallback, CancellationToken cancelToken)
	{
		MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("PerformInitialLevelLoad");
		RpcReceiveState = RpcReceiveState.Allow;
		Debug.Log(string.Format("{0}.{1} loading LevelId: {2}", "CreatorManager", "PerformInitialLevelLoad", levelId));
		canSendClientLevelDetails = false;
		progressCallback?.Invoke("Fetching latest data...");
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(levelId);
		if (cancelToken.IsCancellationRequested)
		{
			return;
		}
		if (graphQlResult.HasErrors)
		{
			MonoBehaviourSingleton<ConnectionActions>.Instance.EndMatch();
			Exception errorMessage = graphQlResult.GetErrorMessage();
			ErrorHandler.HandleError((errorMessage is TimeoutException) ? ErrorCodes.CreatorManager_InitialLevelFetchTimeout : ErrorCodes.CreatorManager_GetLevelAssetInitialLoad, errorMessage, displayModal: true, leaveMatch: true);
			return;
		}
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("LevelFetch", "PerformInitialLevelLoad");
		string text = graphQlResult.GetDataMember().ToString();
		LevelState levelState;
		try
		{
			levelState = LevelStateLoader.Load(text);
		}
		catch (Exception innerException)
		{
			Exception exception = new Exception("Result from Get Level call was not returned as expected?\n" + text, innerException);
			ErrorHandler.HandleError(ErrorCodes.CreatorManager_InvalidLevelState, exception, displayModal: true, leaveMatch: true);
			return;
		}
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("LevelLoad", "PerformInitialLevelLoad");
		progressCallback?.Invoke("Loading library prefabs...");
		await MonoBehaviourSingleton<StageManager>.Instance.LoadLibraryPrefabs(levelState, cancelToken, progressCallback);
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("LoadLibraryPrefabs", "PerformInitialLevelLoad");
		if (cancelToken.IsCancellationRequested)
		{
			return;
		}
		if (levelState.RevisionMetaData == null)
		{
			levelState.RevisionMetaData = new RevisionMetaData();
		}
		levelState.RevisionMetaData.Changes.Clear();
		canSendClientLevelDetails = true;
		progressCallback?.Invoke("Validating received data...");
		(LevelState, SanitizationResult) tuple = Sanitize(levelState);
		var (asset, _) = tuple;
		switch (tuple.Item2)
		{
		case SanitizationResult.Failure:
			return;
		case SanitizationResult.Sanitized:
		{
			GraphQlResult graphQlResult2 = await EndlessServices.Instance.CloudService.UpdateAssetAsync(asset, MatchmakingClientController.Instance.ActiveGameId);
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}
			if (graphQlResult2.HasErrors)
			{
				ErrorHandler.HandleError(ErrorCodes.CreatorManager_FailedBadDataCleanup, graphQlResult2.GetErrorMessage(), displayModal: true, leaveMatch: true);
			}
			levelState = JsonConvert.DeserializeObject<LevelState>(graphQlResult2.GetDataMember().ToString());
			break;
		}
		}
		saveLoadManager.SetCachedLevelState(levelState.AssetID, levelState.AssetVersion);
		if (serverLoadLevelCancellationSource != null)
		{
			serverLoadLevelCancellationSource.Cancel();
		}
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Sanitize", "PerformInitialLevelLoad");
		serverLoadLevelCancellationSource = new CancellationTokenSource();
		await MonoBehaviourSingleton<StageManager>.Instance.LoadLevel(levelState, loadLibraryPrefabs: false, serverLoadLevelCancellationSource.Token, progressCallback);
		NetworkBehaviourSingleton<UISaveStatusManager>.Instance.UpdateLevelVersion(levelState.AssetVersion, isVersionDirty: false);
		NetworkBehaviourSingleton<UISaveStatusManager>.Instance.UpdateGameVersion(MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetVersion, isVersionDirty: false);
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("LoadLevel", "PerformInitialLevelLoad");
		if (!cancelToken.IsCancellationRequested)
		{
			SendDetailsToClients(clientsAwaitingLevelDetails);
			clientsAwaitingLevelDetails.Clear();
			MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("PerformInitialLevelLoad");
		}
	}

	private (LevelState, SanitizationResult) Sanitize(LevelState levelState)
	{
		bool flag = false;
		try
		{
			flag = SanitizeMemberChanges(levelState);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		bool flag2 = false;
		try
		{
			flag2 = SanitizeWireBundles(levelState);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
		if (flag || flag2)
		{
			SemanticVersion semanticVersion = SemanticVersion.Parse(levelState.AssetVersion);
			levelState.AssetVersion = $"{semanticVersion.Major}.{semanticVersion.Minor}.{semanticVersion.Patch + 1}";
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
			return (levelState, SanitizationResult.Sanitized);
		}
		return (levelState, SanitizationResult.NoOp);
	}

	private bool SanitizeMemberChanges(LevelState levelState)
	{
		bool result = false;
		foreach (PropEntry propEntry in levelState.PropEntries)
		{
			foreach (ComponentEntry componentEntry in propEntry.ComponentEntries)
			{
				for (int num = componentEntry.Changes.Count - 1; num >= 0; num--)
				{
					Type type = Type.GetType(componentEntry.AssemblyQualifiedName);
					MemberChange memberChange = componentEntry.Changes[num];
					if (MonoBehaviourSingleton<StageManager>.Instance.TryGetComponentDefinition(type, out var definition) && definition.HasMember(memberChange))
					{
						MemberInfo[] member = type.GetMember(memberChange.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (member[0].MemberType == MemberTypes.Property)
						{
							if ((member[0] as PropertyInfo).SetMethod != null)
							{
								continue;
							}
						}
						else if (member.Length != 0)
						{
							continue;
						}
					}
					result = true;
					componentEntry.Changes.RemoveAt(num);
				}
			}
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(propEntry.AssetId, out var metadata) || metadata.IsMissingObject)
			{
				continue;
			}
			EndlessScriptComponent scriptComponent = metadata.EndlessProp.ScriptComponent;
			if (!scriptComponent)
			{
				continue;
			}
			for (int num2 = propEntry.LuaMemberChanges.Count - 1; num2 >= 0; num2--)
			{
				MemberChange memberChange2 = propEntry.LuaMemberChanges[num2];
				if (scriptComponent.Script.InspectorValues.FirstOrDefault((InspectorScriptValue inspectorValue) => inspectorValue.Name == memberChange2.MemberName && AcceptsDataType(inspectorValue.DataType, memberChange2.DataType)) == null)
				{
					propEntry.LuaMemberChanges.RemoveAt(num2);
					result = true;
				}
			}
		}
		return result;
		static bool AcceptsDataType(int inspectorDataTypeId, int memberChangeDataTypeId)
		{
			if (inspectorDataTypeId == memberChangeDataTypeId)
			{
				return true;
			}
			Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(inspectorDataTypeId);
			Type typeFromId2 = EndlessTypeMapping.Instance.GetTypeFromId(memberChangeDataTypeId);
			Type baseType = typeFromId.BaseType;
			Type baseType2 = typeFromId2.BaseType;
			if (baseType == null || baseType2 == null || IsValueType(typeFromId) || IsValueType(typeFromId2))
			{
				return false;
			}
			if (baseType == baseType2 && baseType != typeof(object))
			{
				return baseType2 != typeof(object);
			}
			return false;
		}
		static bool IsValueType(Type baseType)
		{
			while (baseType.BaseType != null && baseType.BaseType != typeof(object))
			{
				if (baseType == typeof(ValueType))
				{
					return true;
				}
				baseType = baseType.BaseType;
			}
			return false;
		}
	}

	private bool SanitizeWireBundles(LevelState levelState)
	{
		IEnumerable<PropLibrary.RuntimePropInfo> enumerable = from prop in MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetAllRuntimeProps()
			where !prop.IsLoading && !prop.IsMissingObject
			select prop;
		Dictionary<SerializableGuid, Script> dictionary = new Dictionary<SerializableGuid, Script>();
		foreach (PropLibrary.RuntimePropInfo item in enumerable)
		{
			dictionary.TryAdd(item.EndlessProp.Prop.AssetID, item.EndlessProp.ScriptComponent.Script);
		}
		bool result = false;
		for (int num = levelState.WireBundles.Count - 1; num >= 0; num--)
		{
			WireBundle wireBundle = levelState.WireBundles[num];
			PropEntry propEntry = levelState.GetPropEntry(wireBundle.EmitterInstanceId);
			PropEntry propEntry2 = levelState.GetPropEntry(wireBundle.ReceiverInstanceId);
			SerializableGuid assetId = propEntry.AssetId;
			SerializableGuid assetId2 = propEntry2.AssetId;
			PropLibrary.RuntimePropInfo metadata2;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out var metadata) || metadata.IsMissingObject)
			{
				levelState.RemoveBundle(levelState.WireBundles[num]);
			}
			else if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId2, out metadata2) || metadata2.IsMissingObject)
			{
				levelState.RemoveBundle(levelState.WireBundles[num]);
			}
			else
			{
				for (int num2 = wireBundle.Wires.Count - 1; num2 >= 0; num2--)
				{
					WireEntry wireEntry = wireBundle.Wires[num2];
					if (dictionary.TryGetValue(propEntry.AssetId, out var value))
					{
						WireOrganizationData wireOrganizationData = value.EventOrganizationData.FirstOrDefault((WireOrganizationData orgData) => orgData.MemberName == wireEntry.EmitterMemberName && orgData.ComponentId == wireEntry.EmitterComponentTypeId);
						if (wireOrganizationData != null && wireOrganizationData.Disabled)
						{
							result = true;
							wireBundle.Wires.RemoveAt(num2);
							continue;
						}
					}
					if (dictionary.TryGetValue(propEntry2.AssetId, out var value2))
					{
						WireOrganizationData wireOrganizationData2 = value2.ReceiverOrganizationData.FirstOrDefault((WireOrganizationData orgData) => orgData.MemberName == wireEntry.ReceiverMemberName && orgData.ComponentId == wireEntry.ReceiverComponentTypeId);
						if (wireOrganizationData2 != null && wireOrganizationData2.Disabled)
						{
							result = true;
							wireBundle.Wires.RemoveAt(num2);
							continue;
						}
					}
					bool num3 = string.IsNullOrEmpty(wireEntry.EmitterComponentAssemblyQualifiedTypeName);
					bool flag = string.IsNullOrEmpty(wireEntry.ReceiverComponentAssemblyQualifiedTypeName);
					Type type = ((!num3) ? Type.GetType(wireEntry.EmitterComponentAssemblyQualifiedTypeName) : typeof(EndlessScriptComponent));
					Type type2 = ((!flag) ? Type.GetType(wireEntry.ReceiverComponentAssemblyQualifiedTypeName) : typeof(EndlessScriptComponent));
					int[] signature;
					if (num3)
					{
						EndlessScriptComponent scriptComponent = metadata.EndlessProp.ScriptComponent;
						if (!scriptComponent)
						{
							Debug.LogWarning("Emitter Component isn't a script component");
							result = true;
							wireBundle.Wires.RemoveAt(num2);
							continue;
						}
						EndlessEventInfo eventInfo = scriptComponent.GetEventInfo(wireEntry.EmitterMemberName);
						if (eventInfo == null)
						{
							Debug.LogWarning("luaEvent " + wireEntry.EmitterMemberName + " no longer exists in the script");
							result = true;
							wireBundle.Wires.RemoveAt(num2);
							continue;
						}
						signature = eventInfo.ParamList.Select((EndlessParameterInfo param) => param.DataType).ToArray();
					}
					else
					{
						if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetComponentDefinition(type, out var definition))
						{
							Debug.LogWarning("The entire component definition could not be found " + type.Name + " " + wireEntry.EmitterComponentAssemblyQualifiedTypeName);
							result = true;
							wireBundle.Wires.RemoveAt(num2);
							continue;
						}
						if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetDataTypeSignature(definition.AvailableEvents, wireEntry.EmitterMemberName, out signature))
						{
							Debug.LogWarning("The signature of the emitter couldn't be found");
							result = true;
							wireBundle.Wires.RemoveAt(num2);
							continue;
						}
					}
					int[] signature2;
					if (flag)
					{
						EndlessScriptComponent scriptComponent2 = metadata2.EndlessProp.ScriptComponent;
						if (!scriptComponent2)
						{
							Debug.LogWarning("Receiver Component isn't a script component");
							result = true;
							wireBundle.Wires.RemoveAt(num2);
							continue;
						}
						EndlessEventInfo endlessEventInfo = scriptComponent2.Script.Receivers.FirstOrDefault((EndlessEventInfo r) => r.MemberName == wireEntry.ReceiverMemberName);
						if (endlessEventInfo == null)
						{
							Debug.LogWarning("luaEvent no longer exists in the script");
							result = true;
							wireBundle.Wires.RemoveAt(num2);
							continue;
						}
						signature2 = endlessEventInfo.ParamList.Select((EndlessParameterInfo param) => param.DataType).ToArray();
					}
					else
					{
						if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetComponentDefinition(type2, out var definition2))
						{
							Debug.LogWarning("The entire component definition could not be found");
							result = true;
							wireBundle.Wires.RemoveAt(num2);
							continue;
						}
						if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetDataTypeSignature(definition2.AvailableReceivers, wireEntry.ReceiverMemberName, out signature2))
						{
							Debug.LogWarning("The signature of the receiver couldn't be found");
							result = true;
							wireBundle.Wires.RemoveAt(num2);
							continue;
						}
					}
					if (signature2.Length == 0)
					{
						if (wireEntry.StaticParameters.Length != 0)
						{
							Debug.LogWarning("Receiver receives no parameters but we had leftover static parameters");
							result = true;
							wireBundle.Wires.RemoveAt(num2);
						}
					}
					else if (wireEntry.StaticParameters.Length != 0)
					{
						int[] signatureTwo = wireEntry.StaticParameters.Select((StoredParameter staticParam) => staticParam.DataType).ToArray();
						if (!MonoBehaviourSingleton<StageManager>.Instance.SignaturesMatch(signature2, signatureTwo))
						{
							Debug.LogWarning("Receiver did not match the static parameter data type signature");
							result = true;
							wireBundle.Wires.RemoveAt(num2);
						}
					}
					else if (!MonoBehaviourSingleton<StageManager>.Instance.SignaturesMatch(signature2, signature))
					{
						Debug.LogWarning("Receiver did not match the emitter's parameter data type signature");
						result = true;
						wireBundle.Wires.RemoveAt(num2);
					}
				}
				if (wireBundle.Wires.Count == 0)
				{
					result = true;
					levelState.RemoveBundle(levelState.WireBundles[num]);
				}
			}
		}
		return result;
	}

	private async void ValidateConnectedUsers()
	{
		List<ulong> clientIdsTokick = new List<ulong>();
		Debug.Log("Validating connected users to level id: " + MatchmakingClientController.Instance.LocalMatch?.GetLevelId());
		foreach (int connectedUserId in NetworkBehaviourSingleton<UserIdManager>.Instance.ConnectedUserIds)
		{
			if (!(await MonoBehaviourSingleton<RightsManager>.Instance.HasRoleOrGreaterForAssetAsync(MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID, connectedUserId, Roles.Viewer)).PassedCheck)
			{
				Debug.Log($"Connected user with ID: {connectedUserId} did not have a valid role and is being removed.");
				clientIdsTokick.Add(NetworkBehaviourSingleton<UserIdManager>.Instance.GetClientId(connectedUserId));
			}
		}
		if (clientIdsTokick.Any((ulong clientId) => base.IsServer && clientId == NetworkManager.Singleton.LocalClientId))
		{
			MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Kicked From Match", null, "You no longer have rights to edit this game.", UIModalManagerStackActions.ClearStack);
			MonoBehaviourSingleton<ConnectionActions>.Instance.EndMatch();
			return;
		}
		foreach (ulong item in clientIdsTokick)
		{
			MatchSession.Instance.KickClient(item, "You do not have permission to edit this game");
		}
	}

	private async void AutoRefreshUserRoles(CancellationToken cancelToken)
	{
		if (!base.IsServer)
		{
			return;
		}
		TimeSpan delay = TimeSpan.FromSeconds(30.0);
		while (true)
		{
			await Task.Delay(delay, cancelToken).ContinueWith(delegate
			{
			});
			if (!cancelToken.IsCancellationRequested && (bool)MatchmakingClientController.Instance && MatchmakingClientController.Instance.LocalMatch != null)
			{
				Debug.Log(string.Format("{0}.{1}: Executing GetAllUserRolesForAssetAsync with Level Id: {2}, GameId: {3}", "CreatorManager", "AutoRefreshUserRoles", MatchmakingClientController.Instance.LocalMatch.GetLevelId(), MatchmakingClientController.Instance.ActiveGameId));
				await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(MatchmakingClientController.Instance.LocalMatch.GetLevelId(), MatchmakingClientController.Instance.ActiveGameId, null, forceRefresh: true);
				if (!cancelToken.IsCancellationRequested)
				{
					ValidateConnectedUsers();
					continue;
				}
				break;
			}
			break;
		}
	}

	public async Task RetrieveAndLoadServerLevel(CancellationToken cancelToken, Action<string> progressCallback)
	{
		RpcReceiveState = RpcReceiveState.Ignore;
		waitingForTargetLevelData = true;
		waitingForLevel = true;
		cloudLevelState = null;
		loadedLevelData = string.Empty;
		levelDataFragments.Clear();
		RequestLevelDetails_ServerRpc();
		progressCallback?.Invoke("Awaiting level info from server...");
		while (waitingForTargetLevelData || waitingForLevel)
		{
			await Task.Yield();
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}
		}
		LevelState levelState = await Task.Run(() => LevelStateLoader.Load(loadedLevelData));
		if (!cancelToken.IsCancellationRequested)
		{
			loadedLevelData = string.Empty;
			levelState.RevisionMetaData.Changes.Clear();
			await MonoBehaviourSingleton<StageManager>.Instance.LoadLevel(levelState, loadLibraryPrefabs: true, cancelToken, progressCallback);
		}
	}

	public IEnumerator SendLevelData(byte[] bytes, ClientRpcParams rpcParams)
	{
		Debug.Log($"Sending level data: {bytes.Length} bytes | {bytes.Length / 8192} fragments | {(float)(bytes.Length / 8192) * 0.05f} seconds ");
		int byteIndex = 0;
		int fragmentID = 0;
		StartingLevelFragment_ClientRpc(rpcParams);
		byte[] fragment = new byte[8192];
		int bytesWritten = 0;
		while (byteIndex < bytes.Length)
		{
			for (int i = 0; i < 8192; i++)
			{
				if (byteIndex >= bytes.Length)
				{
					break;
				}
				fragment[i] = bytes[byteIndex];
				byteIndex++;
				bytesWritten++;
			}
			LevelFragmentSend_ClientRpc(new LevelDataFragment(fragmentID, fragment, bytesWritten), rpcParams);
			bytesWritten = 0;
			fragmentID++;
			yield return new WaitForSecondsRealtime(0.05f);
		}
		foreach (ulong targetClientId in rpcParams.Send.TargetClientIds)
		{
			Debug.Log($"Finished sending level fragments for clientId: {targetClientId}");
		}
		EndingLevelFragments_ClientRpc(complete: true, rpcParams);
	}

	[ClientRpc(Delivery = RpcDelivery.Reliable)]
	private void StartingLevelFragment_ClientRpc(ClientRpcParams rpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendClientRpc(1438970432u, rpcParams, RpcDelivery.Reliable);
				__endSendClientRpc(ref bufferWriter, 1438970432u, rpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				loadedLevelData = string.Empty;
				levelDataFragments.Clear();
				Debug.Log("Starting level fragment send.");
			}
		}
	}

	[ClientRpc(Delivery = RpcDelivery.Reliable)]
	private void LevelFragmentSend_ClientRpc(LevelDataFragment data, ClientRpcParams rpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendClientRpc(1020812181u, rpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in data, default(FastBufferWriter.ForNetworkSerializable));
				__endSendClientRpc(ref bufferWriter, 1020812181u, rpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				Debug.Log($"Level Fragment received: {data.Index}");
				levelDataFragments.Add(data);
			}
		}
	}

	[ClientRpc(Delivery = RpcDelivery.Reliable)]
	private void EndingLevelFragments_ClientRpc(bool complete, ClientRpcParams rpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendClientRpc(2786258557u, rpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in complete, default(FastBufferWriter.ForPrimitives));
			__endSendClientRpc(ref bufferWriter, 2786258557u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		if (complete)
		{
			loadedLevelData = string.Empty;
			for (int i = 0; i < levelDataFragments.Count; i++)
			{
				loadedLevelData += Encoding.ASCII.GetString(levelDataFragments[i].Bytes);
			}
			waitingForLevel = false;
			Debug.Log($"All level fragments received: {levelDataFragments.Count}");
		}
	}

	private async void SendDetailsToClient(ClientRpcParams rpcParams)
	{
		ReceiveCachedLevelDetails_ClientRpc(saveLoadManager.CachedLevelStateId, saveLoadManager.CachedLevelStateVersion, rpcParams);
		if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage == null)
		{
			Debug.Log("StageManager.Instance.ActiveStage is null");
			return;
		}
		string s = await MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.SerializeLevelAsync();
		EnterCachingState_ClientRpc(rpcParams);
		activeLevelSendCoroutines.Add(StartCoroutine(SendLevelData(Encoding.ASCII.GetBytes(s), rpcParams)));
	}

	[ClientRpc]
	private void EnterCachingState_ClientRpc(ClientRpcParams rpcParams)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendClientRpc(919501897u, rpcParams, RpcDelivery.Reliable);
				__endSendClientRpc(ref bufferWriter, 919501897u, rpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				RpcReceiveState = RpcReceiveState.Cache;
			}
		}
	}

	private void SendDetailsToClients(List<ulong> awaitingClients)
	{
		if (awaitingClients.Count == 0)
		{
			return;
		}
		foreach (ulong awaitingClient in awaitingClients)
		{
			Debug.Log($"Starting level fragments for client: {awaitingClient}");
		}
		ClientRpcParams rpcParams = new ClientRpcParams
		{
			Send = new ClientRpcSendParams
			{
				TargetClientIds = new List<ulong>(awaitingClients)
			}
		};
		SendDetailsToClient(rpcParams);
	}

	[ServerRpc(RequireOwnership = false)]
	private void RequestLevelDetails_ServerRpc(ServerRpcParams rpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(1636973562u, rpcParams, RpcDelivery.Reliable);
			__endSendServerRpc(ref bufferWriter, 1636973562u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			Debug.Log("RequestLevelDetails_ServerRpc:: Client is requesting level details");
			if (!canSendClientLevelDetails)
			{
				clientsAwaitingLevelDetails.Add(rpcParams.Receive.SenderClientId);
				return;
			}
			Debug.Log($"Starting level fragments for client: {rpcParams.Receive.SenderClientId}");
			ClientRpcParams rpcParams2 = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new List<ulong> { rpcParams.Receive.SenderClientId }
				}
			};
			SendDetailsToClient(rpcParams2);
		}
	}

	[ClientRpc]
	public void ReceiveCachedLevelDetails_ClientRpc(SerializableGuid levelId, string version, ClientRpcParams rpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendClientRpc(3369545666u, rpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in levelId, default(FastBufferWriter.ForNetworkSerializable));
			bool value = version != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(version);
			}
			__endSendClientRpc(ref bufferWriter, 3369545666u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			RetrieveLevelStateOnClient(levelId, version);
		}
	}

	private async void RetrieveLevelStateOnClient(SerializableGuid levelId, string version)
	{
		Debug.Log(string.Format("{0}.{1} retrieving level {2}, version: {3} from back end", "CreatorManager", "ReceiveCachedLevelDetails_ClientRpc", levelId, version));
		MonoBehaviourSingleton<StageManager>.Instance.PrepareForLevelChange(levelId);
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(levelId, version, null, debugQuery: false, 60);
		if (graphQlResult.HasErrors)
		{
			Exception errorMessage = graphQlResult.GetErrorMessage();
			ErrorHandler.HandleError((errorMessage is TimeoutException) ? ErrorCodes.CreatorManager_ErrorGettingLevelTimeout : ErrorCodes.CreatorManager_ErrorGettingLevel, errorMessage, displayModal: true, leaveMatch: true);
		}
		else
		{
			cloudLevelState = LevelStateLoader.Load(graphQlResult.GetDataMember().ToString());
			waitingForTargetLevelData = false;
		}
	}

	public async Task ApplyCachedRpcs(CancellationToken cancelToken)
	{
		for (int index = 0; index < cachedRpcs.Count; index++)
		{
			try
			{
				cachedRpcs[index]?.Invoke();
				await Task.Yield();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}
		}
		cachedRpcs.Clear();
		RpcReceiveState = RpcReceiveState.Allow;
	}

	public void ClearCachedRPCs()
	{
		cachedRpcs.Clear();
	}

	public void AddCachedRpc(Action cachedRpc)
	{
		if (RpcReceiveState == RpcReceiveState.Cache)
		{
			cachedRpcs.Add(cachedRpc);
		}
	}

	private void OnMatchmakingStarted()
	{
		UnHookOnMatchStartEventsAndHideScreenCover();
	}

	private void UnHookOnMatchStartEventsAndHideScreenCover()
	{
		MatchmakingClientController.OnMatchmakingStarted -= OnMatchmakingStarted;
	}

	[ServerRpc(RequireOwnership = false)]
	public void ForcePlayersToReload_ServerRpc()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				ServerRpcParams serverRpcParams = default(ServerRpcParams);
				FastBufferWriter bufferWriter = __beginSendServerRpc(236590287u, serverRpcParams, RpcDelivery.Reliable);
				__endSendServerRpc(ref bufferWriter, 236590287u, serverRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				MonoBehaviourSingleton<StageManager>.Instance.FlushLoadedAndSpawnedStages(destroyStageObjects: true);
				ForcePlayersToReload_ClientRpc();
				LevelReverted?.Invoke();
			}
		}
	}

	[ClientRpc]
	private void ForcePlayersToReload_ClientRpc()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(2990444095u, clientRpcParams, RpcDelivery.Reliable);
				__endSendClientRpc(ref bufferWriter, 2990444095u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				MonoBehaviourSingleton<StageManager>.Instance.FlushLoadedAndSpawnedStages(destroyStageObjects: false);
			}
		}
	}

	public async Task<bool> UserCanEditLevel(ulong clientId)
	{
		int userId = NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserId(clientId);
		return await UserCanEditLevel(userId);
	}

	public async Task<bool> UserCanEditLevel(int userId)
	{
		return (await MonoBehaviourSingleton<RightsManager>.Instance.HasRoleOrGreaterForAssetAsync(MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID, userId, Roles.Editor)).PassedCheck;
	}

	public async Task HandleGameUpdated(Endless.Gameplay.LevelEditing.Level.Game newGame, Endless.Gameplay.LevelEditing.Level.Game oldGame)
	{
		if ((object)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage == null || MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.RuntimePalette == null)
		{
			return;
		}
		SemanticVersion semanticVersion = SemanticVersion.Parse(newGame.AssetVersion);
		SemanticVersion semanticVersion2 = SemanticVersion.Parse(oldGame.AssetVersion);
		if (semanticVersion <= semanticVersion2)
		{
			Debug.LogWarning("Received Game Versions out of order.");
			return;
		}
		NetworkBehaviourSingleton<UISaveStatusManager>.Instance.UpdateGameVersion(newGame.AssetVersion, isVersionDirty: false);
		Debug.Log("Updating game " + newGame.AssetID + " from " + oldGame.AssetVersion + " to " + newGame.AssetVersion);
		bool didTerrainRepopulate = MonoBehaviourSingleton<StageManager>.Instance.ActiveTerrainPalette.IsRepopulateRequired(newGame, oldGame);
		if (didTerrainRepopulate)
		{
			terrainRepopulationCancelTokenSource?.Cancel();
			terrainRepopulationCancelTokenSource = new CancellationTokenSource();
			await MonoBehaviourSingleton<StageManager>.Instance.ActiveTerrainPalette.Repopulate(newGame, oldGame, terrainRepopulationCancelTokenSource.Token);
		}
		bool didPropsRepopulate = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.IsRepopulateRequired(newGame, oldGame);
		bool propsWereSanitized = false;
		if (didPropsRepopulate)
		{
			propLibraryCancelTokenSource?.Cancel();
			propLibraryCancelTokenSource = new CancellationTokenSource();
			propsWereSanitized = await MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.Repopulate(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage, propLibraryCancelTokenSource.Token);
		}
		bool didAudioRepopulate = MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.IsRepopulateRequired(newGame, oldGame);
		if (didAudioRepopulate)
		{
			if (audioLibraryCancelTokenSource != null)
			{
				audioLibraryCancelTokenSource.Cancel();
			}
			audioLibraryCancelTokenSource = new CancellationTokenSource();
			await MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.RepopulateAudio(audioLibraryCancelTokenSource.Token);
		}
		if (propsWereSanitized)
		{
			if (base.IsServer)
			{
				saveLoadManager.SaveLevel();
			}
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.ValidatePhysicalWiresStillAlive();
		}
		if (didPropsRepopulate || didTerrainRepopulate || didAudioRepopulate)
		{
			if (didPropsRepopulate && MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool.ToolType == ToolType.Prop)
			{
				this.OnPropsRepopulated?.Invoke();
			}
			if (didTerrainRepopulate && MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool.ToolType == ToolType.Painting)
			{
				this.OnTerrainRepopulated?.Invoke();
			}
			this.OnRepopulate?.Invoke();
		}
	}

	public async void AssetUpdated(AssetUpdatedMetaData assetUpdatedMetaData)
	{
		if (assetUpdatedMetaData == null || MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame == null || assetUpdatedMetaData.AssetType != "game" || assetUpdatedMetaData.AssetId != MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID)
		{
			if (assetUpdatedMetaData == null)
			{
				Debug.LogException(new NullReferenceException("assetUpdatedMetaData was null in handling a GraphQL matchmaking response which should never be possible"));
			}
			return;
		}
		SemanticVersion semanticVersion = SemanticVersion.Parse(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetVersion);
		if (SemanticVersion.Parse(assetUpdatedMetaData.LastAssetVersion) > semanticVersion)
		{
			await MonoBehaviourSingleton<GameEditor>.Instance.GetUpdatedGame(assetUpdatedMetaData.AssetId, HandleGameUpdated, delegate
			{
				Debug.LogError("AssetUpdated to get updated game!");
			}, gameUpdatedCancelTokenSource.Token);
		}
	}

	public void SetRPCReceiveState(RpcReceiveState rpcReceiveState)
	{
		RpcReceiveState = rpcReceiveState;
	}

	public CancellationToken GetAssetVersionCancellationToken()
	{
		assetVersionCancellationSource?.Cancel();
		assetVersionCancellationSource = new CancellationTokenSource();
		return assetVersionCancellationSource.Token;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void CancelTokens()
	{
		assetVersionCancellationSource?.Cancel();
		autoRefreshUserRoleCancelTokenSource?.Cancel();
		propLibraryCancelTokenSource?.Cancel();
		terrainRepopulationCancelTokenSource?.Cancel();
		serverLoadLevelCancellationSource?.Cancel();
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(1438970432u, __rpc_handler_1438970432, "StartingLevelFragment_ClientRpc");
		__registerRpc(1020812181u, __rpc_handler_1020812181, "LevelFragmentSend_ClientRpc");
		__registerRpc(2786258557u, __rpc_handler_2786258557, "EndingLevelFragments_ClientRpc");
		__registerRpc(919501897u, __rpc_handler_919501897, "EnterCachingState_ClientRpc");
		__registerRpc(1636973562u, __rpc_handler_1636973562, "RequestLevelDetails_ServerRpc");
		__registerRpc(3369545666u, __rpc_handler_3369545666, "ReceiveCachedLevelDetails_ClientRpc");
		__registerRpc(236590287u, __rpc_handler_236590287, "ForcePlayersToReload_ServerRpc");
		__registerRpc(2990444095u, __rpc_handler_2990444095, "ForcePlayersToReload_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_1438970432(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CreatorManager)target).StartingLevelFragment_ClientRpc(client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1020812181(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out LevelDataFragment value, default(FastBufferWriter.ForNetworkSerializable));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CreatorManager)target).LevelFragmentSend_ClientRpc(value, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2786258557(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CreatorManager)target).EndingLevelFragments_ClientRpc(value, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_919501897(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CreatorManager)target).EnterCachingState_ClientRpc(client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1636973562(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CreatorManager)target).RequestLevelDetails_ServerRpc(server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3369545666(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value2, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value2)
			{
				reader.ReadValueSafe(out s, oneByteChars: false);
			}
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CreatorManager)target).ReceiveCachedLevelDetails_ClientRpc(value, s, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_236590287(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CreatorManager)target).ForcePlayersToReload_ServerRpc();
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2990444095(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CreatorManager)target).ForcePlayersToReload_ClientRpc();
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "CreatorManager";
	}
}
