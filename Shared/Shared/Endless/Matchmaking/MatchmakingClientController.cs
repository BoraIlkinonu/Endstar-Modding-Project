using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Endless.GraphQl;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.FileManagement;
using Endless.Shared.UI;
using MatchmakingAPI.Matches;
using MatchmakingAPI.Notifications;
using MatchmakingClientSDK;
using MatchmakingClientSDK.Errors;
using Mono.Data.Sqlite;
using Runtime.Shared.Matchmaking;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Endless.Matchmaking
{
	// Token: 0x0200003B RID: 59
	public class MatchmakingClientController : MonoBehaviour
	{
		// Token: 0x1700004A RID: 74
		// (get) Token: 0x0600018D RID: 397 RVA: 0x00009AA4 File Offset: 0x00007CA4
		public static MatchmakingClientController Instance
		{
			get
			{
				if (MatchmakingClientController.instance == null)
				{
					if (MatchmakingClientController.prefab == null)
					{
						MatchmakingClientController.prefab = Resources.Load<MatchmakingClientController>("Networking/MatchmakingClientController");
					}
					if (MatchmakingClientController.prefab != null)
					{
						global::UnityEngine.Object.Instantiate<MatchmakingClientController>(MatchmakingClientController.prefab, null);
					}
					else
					{
						Debug.LogException(new Exception("MatchmakingClientController prefab could not be found at Networking/MatchmakingClientController"));
					}
				}
				return MatchmakingClientController.instance;
			}
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x0600018E RID: 398 RVA: 0x00009B09 File Offset: 0x00007D09
		public NetworkEnvironment NetworkEnvironment
		{
			get
			{
				return this.networkEnv;
			}
		}

		// Token: 0x1400000B RID: 11
		// (add) Token: 0x0600018F RID: 399 RVA: 0x00009B14 File Offset: 0x00007D14
		// (remove) Token: 0x06000190 RID: 400 RVA: 0x00009B48 File Offset: 0x00007D48
		public static event Action OnInitialized;

		// Token: 0x1400000C RID: 12
		// (add) Token: 0x06000191 RID: 401 RVA: 0x00009B7C File Offset: 0x00007D7C
		// (remove) Token: 0x06000192 RID: 402 RVA: 0x00009BB0 File Offset: 0x00007DB0
		public static event Action OnMissingIdentity;

		// Token: 0x1400000D RID: 13
		// (add) Token: 0x06000193 RID: 403 RVA: 0x00009BE4 File Offset: 0x00007DE4
		// (remove) Token: 0x06000194 RID: 404 RVA: 0x00009C18 File Offset: 0x00007E18
		public static event Action OnStartedConnectionToServer;

		// Token: 0x1400000E RID: 14
		// (add) Token: 0x06000195 RID: 405 RVA: 0x00009C4C File Offset: 0x00007E4C
		// (remove) Token: 0x06000196 RID: 406 RVA: 0x00009C80 File Offset: 0x00007E80
		public static event Action<string> OnConnectionToServerFailed;

		// Token: 0x1400000F RID: 15
		// (add) Token: 0x06000197 RID: 407 RVA: 0x00009CB4 File Offset: 0x00007EB4
		// (remove) Token: 0x06000198 RID: 408 RVA: 0x00009CE8 File Offset: 0x00007EE8
		public static event Action OnConnectedToServer;

		// Token: 0x14000010 RID: 16
		// (add) Token: 0x06000199 RID: 409 RVA: 0x00009D1C File Offset: 0x00007F1C
		// (remove) Token: 0x0600019A RID: 410 RVA: 0x00009D50 File Offset: 0x00007F50
		public static event Action<string> OnDisconnectedFromServer;

		// Token: 0x14000011 RID: 17
		// (add) Token: 0x0600019B RID: 411 RVA: 0x00009D84 File Offset: 0x00007F84
		// (remove) Token: 0x0600019C RID: 412 RVA: 0x00009DB8 File Offset: 0x00007FB8
		public static event Action OnAuthenticationProcessStarted;

		// Token: 0x14000012 RID: 18
		// (add) Token: 0x0600019D RID: 413 RVA: 0x00009DEC File Offset: 0x00007FEC
		// (remove) Token: 0x0600019E RID: 414 RVA: 0x00009E20 File Offset: 0x00008020
		public static event Action OnMatchmakingStarted;

		// Token: 0x14000013 RID: 19
		// (add) Token: 0x0600019F RID: 415 RVA: 0x00009E54 File Offset: 0x00008054
		// (remove) Token: 0x060001A0 RID: 416 RVA: 0x00009E88 File Offset: 0x00008088
		public static event Action<string, string> GroupInviteReceived;

		// Token: 0x14000014 RID: 20
		// (add) Token: 0x060001A1 RID: 417 RVA: 0x00009EBC File Offset: 0x000080BC
		// (remove) Token: 0x060001A2 RID: 418 RVA: 0x00009EF0 File Offset: 0x000080F0
		public static event Action GroupJoined;

		// Token: 0x14000015 RID: 21
		// (add) Token: 0x060001A3 RID: 419 RVA: 0x00009F24 File Offset: 0x00008124
		// (remove) Token: 0x060001A4 RID: 420 RVA: 0x00009F58 File Offset: 0x00008158
		public static event Action<string> GroupJoin;

		// Token: 0x14000016 RID: 22
		// (add) Token: 0x060001A5 RID: 421 RVA: 0x00009F8C File Offset: 0x0000818C
		// (remove) Token: 0x060001A6 RID: 422 RVA: 0x00009FC0 File Offset: 0x000081C0
		public static event Action<string> GroupHostChanged;

		// Token: 0x14000017 RID: 23
		// (add) Token: 0x060001A7 RID: 423 RVA: 0x00009FF4 File Offset: 0x000081F4
		// (remove) Token: 0x060001A8 RID: 424 RVA: 0x0000A028 File Offset: 0x00008228
		public static event Action<string> GroupLeave;

		// Token: 0x14000018 RID: 24
		// (add) Token: 0x060001A9 RID: 425 RVA: 0x0000A05C File Offset: 0x0000825C
		// (remove) Token: 0x060001AA RID: 426 RVA: 0x0000A090 File Offset: 0x00008290
		public static event Action GroupLeft;

		// Token: 0x14000019 RID: 25
		// (add) Token: 0x060001AB RID: 427 RVA: 0x0000A0C4 File Offset: 0x000082C4
		// (remove) Token: 0x060001AC RID: 428 RVA: 0x0000A0F8 File Offset: 0x000082F8
		public static event Action MatchStart;

		// Token: 0x1400001A RID: 26
		// (add) Token: 0x060001AD RID: 429 RVA: 0x0000A12C File Offset: 0x0000832C
		// (remove) Token: 0x060001AE RID: 430 RVA: 0x0000A160 File Offset: 0x00008360
		public static event Action MatchAllocated;

		// Token: 0x1400001B RID: 27
		// (add) Token: 0x060001AF RID: 431 RVA: 0x0000A194 File Offset: 0x00008394
		// (remove) Token: 0x060001B0 RID: 432 RVA: 0x0000A1C8 File Offset: 0x000083C8
		public static event Action<int, string> MatchAllocationError;

		// Token: 0x1400001C RID: 28
		// (add) Token: 0x060001B1 RID: 433 RVA: 0x0000A1FC File Offset: 0x000083FC
		// (remove) Token: 0x060001B2 RID: 434 RVA: 0x0000A230 File Offset: 0x00008430
		public static event Action<string> MatchJoin;

		// Token: 0x1400001D RID: 29
		// (add) Token: 0x060001B3 RID: 435 RVA: 0x0000A264 File Offset: 0x00008464
		// (remove) Token: 0x060001B4 RID: 436 RVA: 0x0000A298 File Offset: 0x00008498
		public static event Action<string> MatchHostChanged;

		// Token: 0x1400001E RID: 30
		// (add) Token: 0x060001B5 RID: 437 RVA: 0x0000A2CC File Offset: 0x000084CC
		// (remove) Token: 0x060001B6 RID: 438 RVA: 0x0000A300 File Offset: 0x00008500
		public static event Action MatchHostMigration;

		// Token: 0x1400001F RID: 31
		// (add) Token: 0x060001B7 RID: 439 RVA: 0x0000A334 File Offset: 0x00008534
		// (remove) Token: 0x060001B8 RID: 440 RVA: 0x0000A368 File Offset: 0x00008568
		public static event Action<string> MatchLeave;

		// Token: 0x14000020 RID: 32
		// (add) Token: 0x060001B9 RID: 441 RVA: 0x0000A39C File Offset: 0x0000859C
		// (remove) Token: 0x060001BA RID: 442 RVA: 0x0000A3D0 File Offset: 0x000085D0
		public static event Action MatchLeft;

		// Token: 0x14000021 RID: 33
		// (add) Token: 0x060001BB RID: 443 RVA: 0x0000A404 File Offset: 0x00008604
		// (remove) Token: 0x060001BC RID: 444 RVA: 0x0000A438 File Offset: 0x00008638
		public static event Action<string, string> UserChatReceived;

		// Token: 0x14000022 RID: 34
		// (add) Token: 0x060001BD RID: 445 RVA: 0x0000A46C File Offset: 0x0000866C
		// (remove) Token: 0x060001BE RID: 446 RVA: 0x0000A4A0 File Offset: 0x000086A0
		public static event Action<string, string> GroupChatReceived;

		// Token: 0x14000023 RID: 35
		// (add) Token: 0x060001BF RID: 447 RVA: 0x0000A4D4 File Offset: 0x000086D4
		// (remove) Token: 0x060001C0 RID: 448 RVA: 0x0000A508 File Offset: 0x00008708
		public static event Action<string, string> MatchChatReceived;

		// Token: 0x14000024 RID: 36
		// (add) Token: 0x060001C1 RID: 449 RVA: 0x0000A53C File Offset: 0x0000873C
		// (remove) Token: 0x060001C2 RID: 450 RVA: 0x0000A570 File Offset: 0x00008770
		public static event Action<NotificationTypes, Document> NotificationReceived;

		// Token: 0x14000025 RID: 37
		// (add) Token: 0x060001C3 RID: 451 RVA: 0x0000A5A4 File Offset: 0x000087A4
		// (remove) Token: 0x060001C4 RID: 452 RVA: 0x0000A5D8 File Offset: 0x000087D8
		public static event Action OnShutdown;

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x060001C5 RID: 453 RVA: 0x0000A60B File Offset: 0x0000880B
		// (set) Token: 0x060001C6 RID: 454 RVA: 0x0000A613 File Offset: 0x00008813
		public string UserToken { get; private set; }

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x060001C7 RID: 455 RVA: 0x0000A61C File Offset: 0x0000881C
		// (set) Token: 0x060001C8 RID: 456 RVA: 0x0000A624 File Offset: 0x00008824
		public TargetPlatforms UserPlatform { get; private set; } = TargetPlatforms.Test;

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x060001C9 RID: 457 RVA: 0x00009B09 File Offset: 0x00007D09
		public NetworkEnvironment NetworkEnv
		{
			get
			{
				return this.networkEnv;
			}
		}

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x060001CA RID: 458 RVA: 0x0000A62D File Offset: 0x0000882D
		// (set) Token: 0x060001CB RID: 459 RVA: 0x0000A635 File Offset: 0x00008835
		public bool UseHostName { get; set; } = true;

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x060001CC RID: 460 RVA: 0x0000A63E File Offset: 0x0000883E
		public bool IsInitialized
		{
			get
			{
				return this.isInitialized;
			}
		}

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x060001CD RID: 461 RVA: 0x0000A646 File Offset: 0x00008846
		public bool IsMissingIdentity
		{
			get
			{
				return this.missingIdentity;
			}
		}

		// Token: 0x060001CE RID: 462 RVA: 0x0000A650 File Offset: 0x00008850
		private void Awake()
		{
			Debug.Log("Awake", this);
			Debug.Log("Forcing Network Environment to Prod");
			this.networkEnv = NetworkEnvironment.PROD;
			if (MatchmakingClientController.instance != null)
			{
				global::UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			MatchmakingClientController.instance = this;
			this.allocator = new ServerAllocator();
			this.ValidateVersion(delegate
			{
				global::UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
				Debug.Log(string.Format("Network Environment: {0}", this.networkEnv));
				switch (this.networkEnv)
				{
				case NetworkEnvironment.DEV:
					GraphQlRequest.Initialize("https://endstar-api-dev.endlessstudios.com/graphql");
					break;
				case NetworkEnvironment.STAGING:
					GraphQlRequest.Initialize("https://endstar-api-stage.endlessstudios.com/graphql");
					break;
				case NetworkEnvironment.PROD:
					GraphQlRequest.Initialize("https://endstar-api.endlessstudios.com/graphql");
					break;
				}
				this.UserToken = null;
				this.UserPlatform = TargetPlatforms.Test;
				this.unityServicesManager = new UnityServicesManager();
				this.unityServicesManager.OnServicesInitialized += this.Startup;
				this.unityServicesManager.Initialize(this.networkEnv == NetworkEnvironment.PROD);
			});
		}

		// Token: 0x060001CF RID: 463 RVA: 0x0000A6B8 File Offset: 0x000088B8
		private void Start()
		{
			if (MatchmakingClientController.instance != this)
			{
				return;
			}
			try
			{
				MonoBehaviourSingleton<LocalFileDatabase>.Instance.Setup();
			}
			catch (SqliteException ex)
			{
				if (ex.ErrorCode == SQLiteErrorCode.Busy)
				{
					MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Multiple Instances of Game Running!", null, "Running multiple instances of Endstar on a single machine is not supported at this time.", UIModalManagerStackActions.ClearStack, new UIModalGenericViewAction[]
					{
						new UIModalGenericViewAction(Color.red, "Quit", new Action(Application.Quit))
					});
				}
				else
				{
					MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Error initializing local content database!", null, "An error occurred initializing the local content database. If errors persist, please try re-installing Endstar or contacting support.", UIModalManagerStackActions.ClearStack, new UIModalGenericViewAction[]
					{
						new UIModalGenericViewAction(Color.red, "Quit", new Action(Application.Quit))
					});
				}
			}
			catch (Exception)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("Error initializing local content database!", null, "An error occurred initializing the local content database. If errors persist, please try re-installing Endstar or contacting support.", UIModalManagerStackActions.ClearStack, new UIModalGenericViewAction[]
				{
					new UIModalGenericViewAction(Color.red, "Quit", new Action(Application.Quit))
				});
			}
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x0000A7C8 File Offset: 0x000089C8
		private async void ValidateVersion(Action successCallback)
		{
			TaskAwaiter<bool> taskAwaiter = BuildUtilities.Initialize().GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				TaskAwaiter<bool> taskAwaiter2;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<bool>);
			}
			if (taskAwaiter.GetResult())
			{
				Debug.Log("Successful version validation!");
				if (successCallback != null)
				{
					successCallback();
				}
			}
		}

		// Token: 0x060001D1 RID: 465 RVA: 0x0000A7FF File Offset: 0x000089FF
		private void Startup()
		{
			this.isInitialized = true;
			Action onInitialized = MatchmakingClientController.OnInitialized;
			if (onInitialized != null)
			{
				onInitialized();
			}
			if (this.UserToken != null)
			{
				this.StartConnection();
				return;
			}
			this.missingIdentity = true;
			Action onMissingIdentity = MatchmakingClientController.OnMissingIdentity;
			if (onMissingIdentity == null)
			{
				return;
			}
			onMissingIdentity();
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x0000A840 File Offset: 0x00008A40
		public bool TrySetUserToken(string newToken, TargetPlatforms userPlatform = TargetPlatforms.Endless)
		{
			if (!this.missingIdentity)
			{
				Debug.LogException(new Exception("Cannot set user token! It is not missing identity!"));
				return false;
			}
			if (string.IsNullOrWhiteSpace(newToken))
			{
				Debug.LogException(new Exception("Provided token is of invalid format!"));
				return false;
			}
			Debug.Log("User Token: " + newToken);
			this.UserToken = newToken;
			this.UserPlatform = userPlatform;
			this.missingIdentity = false;
			this.StartConnection();
			return true;
		}

		// Token: 0x060001D3 RID: 467 RVA: 0x0000A8AB File Offset: 0x00008AAB
		private void StartConnection()
		{
			Action onStartedConnectionToServer = MatchmakingClientController.OnStartedConnectionToServer;
			if (onStartedConnectionToServer != null)
			{
				onStartedConnectionToServer();
			}
			this.Initialize();
		}

		// Token: 0x060001D4 RID: 468 RVA: 0x000050BB File Offset: 0x000032BB
		[Obsolete("Use EndlessCloudService.ClearCachedCredentials")]
		public void ClearUserToken()
		{
		}

		// Token: 0x060001D5 RID: 469 RVA: 0x0000A8C4 File Offset: 0x00008AC4
		private void Initialize()
		{
			IMatchmakingClient matchmakingClient = this.client;
			if (((matchmakingClient != null) ? matchmakingClient.WebsocketClient : null) != null)
			{
				this.client.Dispose();
				this.client = null;
			}
			this.client = new MatchmakingClient(this.networkEnv.ToString(), delegate(string t, bool e)
			{
				if (e)
				{
					Debug.LogError(t);
					return;
				}
				Debug.Log(t);
			}, () => Time.time);
			this.client.WebsocketClient.OnConnectedToServer += this.OnConnected;
			this.client.WebsocketClient.OnConnectionToServerFailed += this.OnConnectionFailed;
			this.client.WebsocketClient.OnDisconnectedFromServer += this.OnDisconnected;
			this.client.UsersService.OnAuthenticationSuccess += this.OnAuthenticated;
			this.client.UsersService.OnAuthenticationFailed += this.OnAuthenticationFailed;
			this.client.NotificationsService.OnNotificationReceived += this.OnNotificationReceived;
			this.client.GroupsService.OnGroupInvite += this.OnGroupInvite;
			this.client.GroupsService.OnGroupJoined += this.OnGroupJoined;
			this.client.GroupsService.OnGroupJoin += this.OnGroupJoin;
			this.client.GroupsService.OnGroupHostChanged += this.OnGroupHostChanged;
			this.client.GroupsService.OnGroupUpdated += this.OnGroupUpdated;
			this.client.GroupsService.OnGroupLeave += this.OnGroupLeave;
			this.client.GroupsService.OnGroupLeft += this.OnGroupLeft;
			this.client.MatchesService.OnMatchJoined += this.OnMatchStarted;
			this.client.MatchesService.OnMatchJoin += this.OnMatchJoin;
			this.client.MatchesService.OnMatchLeave += this.OnMatchLeave;
			this.client.MatchesService.OnMatchHostChanged += this.OnMatchHostChanged;
			this.client.MatchesService.OnMatchUpdated += this.OnMatchUpdated;
			this.client.MatchesService.OnMatchAllocated += this.OnMatchAllocated;
			this.client.MatchesService.OnMatchAllocationError += this.OnMatchAllocationError;
			this.client.MatchesService.OnHostMigration += this.OnHostMigration;
			this.client.MatchesService.OnMatchLeft += this.OnMatchLeft;
			this.client.ChatService.OnUserChatReceived += this.OnUserChatReceived;
			this.client.ChatService.OnGroupChatReceived += this.OnGroupChatReceived;
			this.client.ChatService.OnMatchChatReceived += this.OnMatchChatReceived;
			this.client.Connect(this.UserToken, this.UserPlatform.ToString().ToUpper());
		}

		// Token: 0x060001D6 RID: 470 RVA: 0x0000AC37 File Offset: 0x00008E37
		private void Update()
		{
			if (MatchmakingClientController.instance != this)
			{
				return;
			}
			IMatchmakingClient matchmakingClient = this.client;
			if (matchmakingClient == null)
			{
				return;
			}
			matchmakingClient.Update();
		}

		// Token: 0x060001D7 RID: 471 RVA: 0x0000AC58 File Offset: 0x00008E58
		private void OnGUI()
		{
			if (this.LocalClientData == null)
			{
				return;
			}
			MatchInfo localMatch = this.LocalMatch;
		}

		// Token: 0x060001D8 RID: 472 RVA: 0x0000AC80 File Offset: 0x00008E80
		private void OnDestroy()
		{
			if (MatchmakingClientController.instance != this)
			{
				return;
			}
			MatchmakingClientController.instance = null;
			IMatchmakingClient matchmakingClient = this.client;
			if (matchmakingClient != null)
			{
				matchmakingClient.Dispose();
			}
			this.console.Dispose();
			EndlessServices.Remove();
			Action onShutdown = MatchmakingClientController.OnShutdown;
			if (onShutdown == null)
			{
				return;
			}
			onShutdown();
		}

		// Token: 0x060001D9 RID: 473 RVA: 0x0000ACD1 File Offset: 0x00008ED1
		private void OnConnected()
		{
			Action onConnectedToServer = MatchmakingClientController.OnConnectedToServer;
			if (onConnectedToServer != null)
			{
				onConnectedToServer();
			}
			Action onAuthenticationProcessStarted = MatchmakingClientController.OnAuthenticationProcessStarted;
			if (onAuthenticationProcessStarted == null)
			{
				return;
			}
			onAuthenticationProcessStarted();
		}

		// Token: 0x060001DA RID: 474 RVA: 0x0000ACF4 File Offset: 0x00008EF4
		private void OnConnectionFailed()
		{
			IMatchmakingClient matchmakingClient = this.client;
			if (matchmakingClient != null)
			{
				matchmakingClient.Dispose();
			}
			this.client = null;
			this.UserToken = null;
			Action<string> onConnectionToServerFailed = MatchmakingClientController.OnConnectionToServerFailed;
			if (onConnectionToServerFailed != null)
			{
				onConnectionToServerFailed("Connection failed! Retrying...");
			}
			if (this != null)
			{
				base.Invoke("Startup", 3f);
			}
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x060001DB RID: 475 RVA: 0x0000AD4E File Offset: 0x00008F4E
		// (set) Token: 0x060001DC RID: 476 RVA: 0x0000AD56 File Offset: 0x00008F56
		public ClientData? LocalUser { get; private set; }

		// Token: 0x060001DD RID: 477 RVA: 0x0000AD60 File Offset: 0x00008F60
		private async void OnAuthenticated()
		{
			this.LocalClientData = new ClientData?(new ClientData(this.client.UsersService.GetUserId(), TargetPlatforms.Endless, this.client.UsersService.GetUserName()));
			EndlessServices.New(base.transform);
			EndlessServices.Instance.Initialize(this.UserToken, this.UserPlatform);
			await EndlessServices.Instance.CloudService.CheckUserElevatedRolesAndRestrictions(EndlessServices.Instance.CloudService.AuthToken);
			base.Invoke("StartMatchmaking", 3f);
			Action<ClientData> onAuthenticationProcessSuccessful = MatchmakingClientController.OnAuthenticationProcessSuccessful;
			if (onAuthenticationProcessSuccessful != null)
			{
				onAuthenticationProcessSuccessful(this.LocalClientData.Value);
			}
		}

		// Token: 0x060001DE RID: 478 RVA: 0x0000AD98 File Offset: 0x00008F98
		private void OnAuthenticationFailed()
		{
			this.LocalClientData = null;
			this.UserToken = null;
			Action<string> onAuthenticationProcessFailed = MatchmakingClientController.OnAuthenticationProcessFailed;
			if (onAuthenticationProcessFailed != null)
			{
				onAuthenticationProcessFailed("Authentication failed.");
			}
			if (this != null)
			{
				base.Invoke("Disconnect", 3f);
			}
		}

		// Token: 0x060001DF RID: 479 RVA: 0x0000ADE9 File Offset: 0x00008FE9
		public void Disconnect()
		{
			IMatchmakingClient matchmakingClient = this.client;
			if (matchmakingClient == null)
			{
				return;
			}
			matchmakingClient.Dispose();
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x0000ADFC File Offset: 0x00008FFC
		private void OnDisconnected()
		{
			this.UserToken = null;
			this.LocalClientData = null;
			EndlessServices.Remove();
			this.CleanMatchData();
			this.ClearUserGroupData();
			base.CancelInvoke();
			IMatchmakingClient matchmakingClient = this.client;
			if (matchmakingClient != null)
			{
				matchmakingClient.Dispose();
			}
			this.client = null;
			Action<string> onDisconnectedFromServer = MatchmakingClientController.OnDisconnectedFromServer;
			if (onDisconnectedFromServer != null)
			{
				onDisconnectedFromServer("Disconnected from matchmaking server! Reconnecting...");
			}
			if (this != null)
			{
				base.Invoke("Startup", 3f);
			}
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x0000AE7C File Offset: 0x0000907C
		private void StartMatchmaking()
		{
			if (NetworkManager.Singleton.ShutdownInProgress)
			{
				base.Invoke("StartMatchmaking", 1f);
				return;
			}
			Action onMatchmakingStarted = MatchmakingClientController.OnMatchmakingStarted;
			if (onMatchmakingStarted == null)
			{
				return;
			}
			onMatchmakingStarted();
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x0000AEAA File Offset: 0x000090AA
		private void OnNotificationReceived(NotificationTypes notificationType, Document document)
		{
			Action<NotificationTypes, Document> notificationReceived = MatchmakingClientController.NotificationReceived;
			if (notificationReceived == null)
			{
				return;
			}
			notificationReceived(notificationType, document);
		}

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x060001E3 RID: 483 RVA: 0x0000AEBD File Offset: 0x000090BD
		// (set) Token: 0x060001E4 RID: 484 RVA: 0x0000AEC5 File Offset: 0x000090C5
		public ClientData? LocalClientData { get; private set; }

		// Token: 0x060001E5 RID: 485 RVA: 0x000050BB File Offset: 0x000032BB
		private void OnUserConnected(string userId)
		{
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x000050BB File Offset: 0x000032BB
		private void OnUserUpdated(ReadOnlyDictionary<string, AttributeValue> oldAttributes)
		{
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x000050BB File Offset: 0x000032BB
		private void OnUserDisconnected(string userId)
		{
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x0000AED0 File Offset: 0x000090D0
		public void GetEncryptedKey(string publicKey, Action<string> callback)
		{
			this.client.UsersService.EncryptUser(publicKey, delegate(Document doc)
			{
				string text;
				HttpStatusCode httpStatusCode;
				int num;
				if (ErrorsService.TryGetError(doc, out text, out httpStatusCode, out num))
				{
					Debug.LogError(string.Format("User encryption failed! Code: {0}, Message: {1}, Index: {2}", text, httpStatusCode, num));
					Action<string> callback2 = callback;
					if (callback2 == null)
					{
						return;
					}
					callback2(null);
					return;
				}
				else
				{
					Action<string> callback3 = callback;
					if (callback3 == null)
					{
						return;
					}
					callback3(doc["encryptedKey"].AsString());
					return;
				}
			});
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x0000AF08 File Offset: 0x00009108
		public void GetUserFromEncryptedKey(string publicKey, string encryptedKey, Action<string> callback)
		{
			this.client.UsersService.DecryptUser(publicKey, encryptedKey, delegate(Document doc)
			{
				string text;
				HttpStatusCode httpStatusCode;
				int num;
				if (ErrorsService.TryGetError(doc, out text, out httpStatusCode, out num))
				{
					Debug.LogError(string.Format("User encryption failed! Code: {0}, Message: {1}, Index: {2}", text, httpStatusCode, num));
					Action<string> callback2 = callback;
					if (callback2 == null)
					{
						return;
					}
					callback2(null);
					return;
				}
				else
				{
					Action<string> callback3 = callback;
					if (callback3 == null)
					{
						return;
					}
					callback3(doc["userId"].AsString());
					return;
				}
			});
		}

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x060001EA RID: 490 RVA: 0x0000AF40 File Offset: 0x00009140
		// (set) Token: 0x060001EB RID: 491 RVA: 0x0000AF48 File Offset: 0x00009148
		public GroupInfo LocalGroup { get; private set; }

		// Token: 0x060001EC RID: 492 RVA: 0x0000AF51 File Offset: 0x00009151
		private void OnGroupInvite(string inviteId, string hostId)
		{
			Action<string, string> groupInviteReceived = MatchmakingClientController.GroupInviteReceived;
			if (groupInviteReceived == null)
			{
				return;
			}
			groupInviteReceived(inviteId, hostId);
		}

		// Token: 0x060001ED RID: 493 RVA: 0x0000AF64 File Offset: 0x00009164
		private void OnGroupJoined()
		{
			ReadOnlyDictionary<string, AttributeValue> localAttributes = this.client.GroupsService.GetLocalAttributes();
			this.LocalGroup = new GroupInfo();
			AttributeValue attributeValue;
			if (localAttributes.TryGetValue("groupId", out attributeValue))
			{
				this.LocalGroup.Id = attributeValue.S;
			}
			AttributeValue attributeValue2;
			if (localAttributes.TryGetValue("groupHost", out attributeValue2))
			{
				this.LocalGroup.Host = new CoreClientData(attributeValue2.S, TargetPlatforms.Endless);
			}
			this.LocalGroup.Members = new List<CoreClientData>();
			AttributeValue attributeValue3;
			if (localAttributes.TryGetValue("groupMembers", out attributeValue3))
			{
				foreach (AttributeValue attributeValue4 in attributeValue3.L)
				{
					this.LocalGroup.Members.Add(new CoreClientData(attributeValue4.S, TargetPlatforms.Endless));
				}
			}
			Action groupJoined = MatchmakingClientController.GroupJoined;
			if (groupJoined == null)
			{
				return;
			}
			groupJoined();
		}

		// Token: 0x060001EE RID: 494 RVA: 0x0000B05C File Offset: 0x0000925C
		private void OnGroupJoin(string userId)
		{
			this.LocalGroup.Members.Add(new CoreClientData(userId, TargetPlatforms.Endless));
			Action<string> groupJoin = MatchmakingClientController.GroupJoin;
			if (groupJoin == null)
			{
				return;
			}
			groupJoin(userId);
		}

		// Token: 0x060001EF RID: 495 RVA: 0x0000B088 File Offset: 0x00009288
		private void OnGroupLeave(string userId)
		{
			CoreClientData coreClientData = new CoreClientData(userId, TargetPlatforms.Endless);
			this.LocalGroup.Members.Remove(coreClientData);
			Action<string> groupLeave = MatchmakingClientController.GroupLeave;
			if (groupLeave == null)
			{
				return;
			}
			groupLeave(userId);
		}

		// Token: 0x060001F0 RID: 496 RVA: 0x0000B0C0 File Offset: 0x000092C0
		private void OnGroupHostChanged(string newHostId)
		{
			this.LocalGroup.Host = new CoreClientData(newHostId, TargetPlatforms.Endless);
			Action<string> groupHostChanged = MatchmakingClientController.GroupHostChanged;
			if (groupHostChanged == null)
			{
				return;
			}
			groupHostChanged(newHostId);
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x000050BB File Offset: 0x000032BB
		private void OnGroupUpdated(ReadOnlyDictionary<string, AttributeValue> oldAttributes)
		{
		}

		// Token: 0x060001F2 RID: 498 RVA: 0x0000B0E4 File Offset: 0x000092E4
		private void OnGroupLeft()
		{
			this.ClearUserGroupData();
			Action groupLeft = MatchmakingClientController.GroupLeft;
			if (groupLeft == null)
			{
				return;
			}
			groupLeft();
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x0000B0FB File Offset: 0x000092FB
		private void ClearUserGroupData()
		{
			this.LocalGroup = null;
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x0000B104 File Offset: 0x00009304
		public void InviteToGroup(string userId, Action<int, string> onError = null)
		{
			this.client.GroupsService.InviteToGroup(userId, onError);
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x0000B118 File Offset: 0x00009318
		public void JoinGroup(string inviteId, Action<int, string> onError = null)
		{
			this.client.GroupsService.JoinGroup(inviteId, onError);
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x0000B12C File Offset: 0x0000932C
		public void RemoveFromGroup(string userId, Action<int, string> onError = null)
		{
			this.client.GroupsService.RemoveFromGroup(userId, onError);
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x0000B140 File Offset: 0x00009340
		public void LeaveGroup(bool stayInMatch = true, Action<int, string> onError = null)
		{
			this.client.GroupsService.LeaveGroup(stayInMatch, onError);
		}

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x060001F8 RID: 504 RVA: 0x0000B154 File Offset: 0x00009354
		// (set) Token: 0x060001F9 RID: 505 RVA: 0x0000B15C File Offset: 0x0000935C
		public MatchSession LocalMatchSession { get; private set; }

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x060001FA RID: 506 RVA: 0x0000B165 File Offset: 0x00009365
		// (set) Token: 0x060001FB RID: 507 RVA: 0x0000B16D File Offset: 0x0000936D
		public MatchInfo LocalMatch { get; private set; }

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x060001FC RID: 508 RVA: 0x0000B176 File Offset: 0x00009376
		// (set) Token: 0x060001FD RID: 509 RVA: 0x0000B17E File Offset: 0x0000937E
		public SerializableGuid ActiveGameId { get; private set; } = SerializableGuid.Empty;

		// Token: 0x060001FE RID: 510 RVA: 0x0000B188 File Offset: 0x00009388
		private void OnMatchStarted()
		{
			ReadOnlyDictionary<string, AttributeValue> localAttributes = this.client.MatchesService.GetLocalAttributes();
			this.LocalMatch = new MatchInfo(this.client);
			AttributeValue attributeValue;
			if (localAttributes.TryGetValue("matchId", out attributeValue))
			{
				this.LocalMatch.Id = attributeValue.S;
			}
			AttributeValue attributeValue2;
			if (localAttributes.TryGetValue("matchHost", out attributeValue2))
			{
				this.LocalMatch.Host = attributeValue2.S;
			}
			this.LocalMatch.Members = new List<string>();
			AttributeValue attributeValue3;
			if (localAttributes.TryGetValue("matchMembers", out attributeValue3))
			{
				foreach (AttributeValue attributeValue4 in attributeValue3.L)
				{
					this.LocalMatch.Members.Add(attributeValue4.S);
				}
			}
			AttributeValue attributeValue5;
			MatchServerTypes matchServerTypes;
			if (localAttributes.TryGetValue("serverType", out attributeValue5) && Enum.TryParse<MatchServerTypes>(attributeValue5.S, out matchServerTypes))
			{
				this.LocalMatch.ServerType = matchServerTypes;
			}
			this.ActiveGameId = localAttributes["gameId"].S;
			Action matchStart = MatchmakingClientController.MatchStart;
			if (matchStart != null)
			{
				matchStart();
			}
			if (this.LocalMatch.ServerType == MatchServerTypes.USER)
			{
				this.TryAllocateMatch();
			}
		}

		// Token: 0x060001FF RID: 511 RVA: 0x0000B2DC File Offset: 0x000094DC
		private void OnMatchJoin(string groupId)
		{
			this.LocalMatch.Members.Add(groupId);
			Action<string> matchJoin = MatchmakingClientController.MatchJoin;
			if (matchJoin == null)
			{
				return;
			}
			matchJoin(groupId);
		}

		// Token: 0x06000200 RID: 512 RVA: 0x0000B300 File Offset: 0x00009500
		private void OnMatchLeave(string groupId)
		{
			this.LocalMatch.Members.Remove(groupId);
			Action<string> matchLeave = MatchmakingClientController.MatchLeave;
			if (matchLeave == null)
			{
				return;
			}
			matchLeave(groupId);
		}

		// Token: 0x06000201 RID: 513 RVA: 0x000050BB File Offset: 0x000032BB
		private void OnMatchUpdated(ReadOnlyDictionary<string, AttributeValue> oldAttributes)
		{
		}

		// Token: 0x06000202 RID: 514 RVA: 0x0000B334 File Offset: 0x00009534
		private void OnHostMigration()
		{
			Action matchHostMigration = MatchmakingClientController.MatchHostMigration;
			if (matchHostMigration != null)
			{
				matchHostMigration();
			}
			if (this.LocalMatchSession != null)
			{
				global::UnityEngine.Object.Destroy(this.LocalMatchSession);
			}
			if (this.LocalMatch.ServerType == MatchServerTypes.USER)
			{
				this.hostMigrationGuid = Guid.NewGuid();
				this.HostMigrationSequence();
			}
		}

		// Token: 0x06000203 RID: 515 RVA: 0x0000B388 File Offset: 0x00009588
		private async void HostMigrationSequence()
		{
			Guid guid = this.hostMigrationGuid;
			while (!this.TryAllocateMatch())
			{
				await Task.Yield();
				if (guid != this.hostMigrationGuid)
				{
					return;
				}
			}
		}

		// Token: 0x06000204 RID: 516 RVA: 0x0000B3C0 File Offset: 0x000095C0
		private bool TryAllocateMatch()
		{
			bool flag = true;
			string userId = this.client.UsersService.GetUserId();
			string stringProperty = this.client.GroupsService.GetStringProperty("groupId");
			string stringProperty2 = this.client.GroupsService.GetStringProperty("groupHost");
			if (userId != stringProperty2)
			{
				flag = false;
			}
			string stringProperty3 = this.client.MatchesService.GetStringProperty("matchHost");
			if (stringProperty != stringProperty3)
			{
				flag = false;
			}
			if (flag)
			{
				this.allocator.OnMatchAllocated += this.OnRelayAllocated;
				this.allocator.Allocate();
				return true;
			}
			return false;
		}

		// Token: 0x06000205 RID: 517 RVA: 0x0000B460 File Offset: 0x00009660
		private void OnRelayAllocated()
		{
			this.allocator.OnMatchAllocated -= this.OnRelayAllocated;
			if (this.allocator.LastAllocation == null)
			{
				this.OnMatchAllocationError(500, "Relay allocation failed!");
				return;
			}
			this.AllocateMatch(this.allocator.PublicIp, this.allocator.LocalIp, this.LocalClientData.Value.DisplayName + "'s Server", this.allocator.Port, this.allocator.Key, this.allocator.LastServerType, new Action<int, string>(this.OnMatchAllocationError));
		}

		// Token: 0x06000206 RID: 518 RVA: 0x0000B508 File Offset: 0x00009708
		private void OnMatchAllocationError(int errorCode, string errorMessage)
		{
			if (this.LocalMatchSession != null)
			{
				global::UnityEngine.Object.Destroy(this.LocalMatchSession);
			}
			this.hostMigrationGuid = Guid.Empty;
			this.allocator.Reset();
			Action<int, string> matchAllocationError = MatchmakingClientController.MatchAllocationError;
			if (matchAllocationError != null)
			{
				matchAllocationError(errorCode, errorMessage);
			}
			if (this.LocalMatch.ServerType == MatchServerTypes.USER)
			{
				this.EndMatch(null);
			}
		}

		// Token: 0x06000207 RID: 519 RVA: 0x0000B56C File Offset: 0x0000976C
		private void OnMatchAllocated(string publicIp, string localIp, string name, int port, string key, string serverType)
		{
			if (this.LocalMatchSession != null)
			{
				global::UnityEngine.Object.Destroy(this.LocalMatchSession);
			}
			Runtime.Shared.Matchmaking.MatchData matchData = this.LocalMatch.GetMatchData();
			bool flag = matchData.MatchServerType == MatchServerTypes.USER && matchData.MatchAuthKey == this.allocator.Key;
			this.hostMigrationGuid = Guid.Empty;
			object lastAllocation = this.allocator.LastAllocation;
			this.allocator.Reset();
			if (!(serverType == "UnityRelay"))
			{
				if (!(serverType == "EndlessRelay") && !(serverType == "LAN"))
				{
					return;
				}
				global::MatchmakingClientSDK.AllocationData allocationData = new global::MatchmakingClientSDK.AllocationData
				{
					publicIp = publicIp,
					localIp = localIp,
					name = name,
					port = port,
					key = key
				};
				EndlessClientMatchSession endlessClientMatchSession = new GameObject("Match Session").AddComponent<EndlessClientMatchSession>();
				this.LocalMatchSession = endlessClientMatchSession;
				endlessClientMatchSession.transform.SetParent(base.transform);
				endlessClientMatchSession.Initialize(this.LocalClientData.Value, this.UserToken, matchData, this.ActiveGameId, flag, allocationData, serverType);
				Action matchAllocated = MatchmakingClientController.MatchAllocated;
				if (matchAllocated == null)
				{
					return;
				}
				matchAllocated();
				return;
			}
			else
			{
				UnityClientMatchSession unityClientMatchSession = new GameObject("Match Session").AddComponent<UnityClientMatchSession>();
				this.LocalMatchSession = unityClientMatchSession;
				unityClientMatchSession.transform.SetParent(base.transform);
				unityClientMatchSession.Initialize(this.LocalClientData.Value, matchData, this.ActiveGameId, flag ? (lastAllocation as Allocation) : null);
				Action matchAllocated2 = MatchmakingClientController.MatchAllocated;
				if (matchAllocated2 == null)
				{
					return;
				}
				matchAllocated2();
				return;
			}
		}

		// Token: 0x06000208 RID: 520 RVA: 0x0000B705 File Offset: 0x00009905
		private void OnMatchHostChanged(string groupId)
		{
			this.LocalMatch.Host = groupId;
			Action<string> matchHostChanged = MatchmakingClientController.MatchHostChanged;
			if (matchHostChanged == null)
			{
				return;
			}
			matchHostChanged(groupId);
		}

		// Token: 0x06000209 RID: 521 RVA: 0x0000B723 File Offset: 0x00009923
		private void OnMatchLeft()
		{
			this.CleanMatchData();
			Action matchLeft = MatchmakingClientController.MatchLeft;
			if (matchLeft == null)
			{
				return;
			}
			matchLeft();
		}

		// Token: 0x0600020A RID: 522 RVA: 0x0000B73C File Offset: 0x0000993C
		private void CleanMatchData()
		{
			this.LocalMatch = null;
			this.ActiveGameId = SerializableGuid.Empty;
			this.hostMigrationGuid = Guid.Empty;
			this.allocator.OnMatchAllocated -= this.OnRelayAllocated;
			this.allocator.Reset();
			if (this.LocalMatchSession != null)
			{
				global::UnityEngine.Object.Destroy(this.LocalMatchSession.gameObject);
			}
		}

		// Token: 0x0600020B RID: 523 RVA: 0x0000B7A8 File Offset: 0x000099A8
		public void StartMatch(string gameId, string levelId, bool isEditSession, string version, string matchServerType = null, string customData = null, Action<int, string> onError = null)
		{
			if (string.IsNullOrWhiteSpace(version))
			{
				if (!isEditSession)
				{
					throw new ArgumentException("This is invalid. you must specify a version when attempting to play.");
				}
				version = string.Empty;
			}
			if (string.IsNullOrWhiteSpace(matchServerType))
			{
				matchServerType = this.defaultMatchServerType;
			}
			this.client.MatchesService.StartMatch(gameId, levelId, isEditSession, version, matchServerType, customData, onError);
		}

		// Token: 0x0600020C RID: 524 RVA: 0x0000B7FF File Offset: 0x000099FF
		public void ChangeMatchHost(string groupId, Action<int, string> onError = null)
		{
			this.client.MatchesService.ChangeHost(groupId, onError);
		}

		// Token: 0x0600020D RID: 525 RVA: 0x0000B813 File Offset: 0x00009A13
		public void GetServerWhitelist(Action<List<string>> onSuccess, Action<int, string> onError = null)
		{
			this.client.MatchesService.GetServerWhitelist(onSuccess, onError);
		}

		// Token: 0x0600020E RID: 526 RVA: 0x0000B827 File Offset: 0x00009A27
		public void AllocateMatch(string publicIp, string localIp, string name, int port, string key, string serverType, Action<int, string> onError = null)
		{
			this.client.MatchesService.AllocateMatch(publicIp, localIp, name, port, key, serverType, onError);
		}

		// Token: 0x0600020F RID: 527 RVA: 0x0000B844 File Offset: 0x00009A44
		public static string[] FormatUserIdsForChangeMatch(IEnumerable<int> idsToUse)
		{
			List<int> list = idsToUse.ToList<int>();
			int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			string[] array;
			if (list.Remove(activeUserId))
			{
				array = new string[] { activeUserId.ToString() };
				array = array.Concat(list.Select((int userId) => userId.ToString())).ToArray<string>();
			}
			else
			{
				array = list.Select((int userId) => userId.ToString()).ToArray<string>();
			}
			return array;
		}

		// Token: 0x06000210 RID: 528 RVA: 0x0000B8E4 File Offset: 0x00009AE4
		public void ChangeMatch(string gameId, string levelId, bool isEditSession, string version, string matchServerType = null, string customData = null, bool endMatch = false, Action<int, string> onError = null, params string[] userIds)
		{
			if (string.IsNullOrWhiteSpace(version))
			{
				if (!isEditSession)
				{
					throw new ArgumentException("This is invalid. you must specify a version when attempting to play.");
				}
				version = string.Empty;
			}
			if (string.IsNullOrWhiteSpace(matchServerType))
			{
				matchServerType = this.defaultMatchServerType;
			}
			this.client.MatchesService.ChangeMatch(gameId, levelId, isEditSession, version, matchServerType, customData, onError, endMatch, userIds);
		}

		// Token: 0x06000211 RID: 529 RVA: 0x0000B93F File Offset: 0x00009B3F
		public void RemoveFromMatch(string groupId, Action<int, string> onError = null)
		{
			this.client.MatchesService.RemoveFromMatch(groupId, onError);
		}

		// Token: 0x06000212 RID: 530 RVA: 0x0000B953 File Offset: 0x00009B53
		public void EndMatch(Action<int, string> onError = null)
		{
			this.client.MatchesService.EndMatch(onError);
		}

		// Token: 0x06000213 RID: 531 RVA: 0x0000B966 File Offset: 0x00009B66
		private void OnUserChatReceived(string senderId, string message)
		{
			Action<string, string> userChatReceived = MatchmakingClientController.UserChatReceived;
			if (userChatReceived == null)
			{
				return;
			}
			userChatReceived(senderId, message);
		}

		// Token: 0x06000214 RID: 532 RVA: 0x0000B979 File Offset: 0x00009B79
		private void OnGroupChatReceived(string senderId, string message)
		{
			Action<string, string> groupChatReceived = MatchmakingClientController.GroupChatReceived;
			if (groupChatReceived == null)
			{
				return;
			}
			groupChatReceived(senderId, message);
		}

		// Token: 0x06000215 RID: 533 RVA: 0x0000B98C File Offset: 0x00009B8C
		private void OnMatchChatReceived(string senderId, string message)
		{
			Action<string, string> matchChatReceived = MatchmakingClientController.MatchChatReceived;
			if (matchChatReceived == null)
			{
				return;
			}
			matchChatReceived(senderId, message);
		}

		// Token: 0x06000216 RID: 534 RVA: 0x0000B99F File Offset: 0x00009B9F
		public void SendUserChat(string message, string recipientId, Action<int, string> onError = null)
		{
			this.client.ChatService.SendChatMessage(message, ChatChannel.User, recipientId, onError);
		}

		// Token: 0x06000217 RID: 535 RVA: 0x0000B9B5 File Offset: 0x00009BB5
		public void SendGroupChat(string message, Action<int, string> onError = null)
		{
			this.client.ChatService.SendChatMessage(message, ChatChannel.Group, null, onError);
		}

		// Token: 0x06000218 RID: 536 RVA: 0x0000B9CB File Offset: 0x00009BCB
		public void SendMatchChat(string message, Action<int, string> onError = null)
		{
			this.client.ChatService.SendChatMessage(message, ChatChannel.Match, null, onError);
		}

		// Token: 0x06000219 RID: 537 RVA: 0x0000B9E1 File Offset: 0x00009BE1
		public void Shutdown()
		{
			if (this == null)
			{
				Debug.LogException(new Exception("MatchmakingClientController is already shut down."));
				return;
			}
			global::UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x040000E9 RID: 233
		public const string PREFAB_RESOURCES_PATH = "Networking/MatchmakingClientController";

		// Token: 0x040000EA RID: 234
		private static MatchmakingClientController instance;

		// Token: 0x040000EB RID: 235
		private static MatchmakingClientController prefab;

		// Token: 0x040000EC RID: 236
		[SerializeField]
		private NetworkEnvironment networkEnv;

		// Token: 0x040000ED RID: 237
		[SerializeField]
		private string defaultMatchServerType = "USER";

		// Token: 0x040000F5 RID: 245
		public static Action<string> OnAuthenticationProcessFailed;

		// Token: 0x040000F6 RID: 246
		public static Action<ClientData> OnAuthenticationProcessSuccessful;

		// Token: 0x0400010E RID: 270
		private bool isInitialized;

		// Token: 0x0400010F RID: 271
		private bool missingIdentity;

		// Token: 0x04000110 RID: 272
		private IMatchmakingClient client;

		// Token: 0x04000111 RID: 273
		private UnityServicesManager unityServicesManager;

		// Token: 0x04000112 RID: 274
		private MatchmakingConsole console = new MatchmakingConsole();

		// Token: 0x04000118 RID: 280
		private ServerAllocator allocator;

		// Token: 0x0400011A RID: 282
		private Guid hostMigrationGuid = Guid.Empty;
	}
}
