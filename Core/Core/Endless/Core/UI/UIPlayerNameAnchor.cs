using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Endless.Gameplay;
using Endless.Gameplay.Screenshotting;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.Shared.UI.Anchors;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000036 RID: 54
	public class UIPlayerNameAnchor : MonoBehaviour, IUIAnchor, IPoolableT, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x1700000F RID: 15
		// (get) Token: 0x060000F3 RID: 243 RVA: 0x00006F91 File Offset: 0x00005191
		// (set) Token: 0x060000F4 RID: 244 RVA: 0x00006F99 File Offset: 0x00005199
		public Transform Target { get; set; }

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x060000F5 RID: 245 RVA: 0x00006FA2 File Offset: 0x000051A2
		// (set) Token: 0x060000F6 RID: 246 RVA: 0x00006FAA File Offset: 0x000051AA
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x060000F7 RID: 247 RVA: 0x00006FB3 File Offset: 0x000051B3
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x060000F8 RID: 248 RVA: 0x00006FBB File Offset: 0x000051BB
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x060000F9 RID: 249 RVA: 0x00006FC4 File Offset: 0x000051C4
		public static UIPlayerNameAnchor CreateInstance(UIPlayerNameAnchor prefab, Transform target, RectTransform container, PlayerReferenceManager playerReferenceManager, Vector3? offset = null)
		{
			UIPlayerNameAnchor uiplayerNameAnchor = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<UIPlayerNameAnchor>(prefab, default(Vector3), default(Quaternion), null);
			uiplayerNameAnchor.transform.SetParent(container, false);
			uiplayerNameAnchor.SetTarget(target);
			uiplayerNameAnchor.SetOffset(offset);
			uiplayerNameAnchor.SetPlayerReferenceManager(playerReferenceManager);
			uiplayerNameAnchor.PlayDisplayTween();
			MonoBehaviourSingleton<UIAnchorManager>.Instance.Register(uiplayerNameAnchor);
			return uiplayerNameAnchor;
		}

		// Token: 0x060000FA RID: 250 RVA: 0x00007028 File Offset: 0x00005228
		public void SetTarget(Transform target)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetTarget", new object[] { target.DebugSafeName(true) });
			}
			if (target == null)
			{
				Debug.LogException(new NullReferenceException("Target cannot be null"));
				this.Close();
				return;
			}
			this.Target = target;
		}

		// Token: 0x060000FB RID: 251 RVA: 0x00007080 File Offset: 0x00005280
		public void SetOffset(Vector3? offset)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetOffset", new object[] { offset });
			}
			this.positioner.Offset = offset ?? Vector3.zero;
		}

		// Token: 0x060000FC RID: 252 RVA: 0x000070D3 File Offset: 0x000052D3
		public void SetPlayerReferenceManager(PlayerReferenceManager playerReferenceManager)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetPlayerReferenceManager", new object[] { playerReferenceManager });
			}
			this.playerReferenceManager = playerReferenceManager;
			this.InitializeAsync(playerReferenceManager);
		}

		// Token: 0x060000FD RID: 253 RVA: 0x00007101 File Offset: 0x00005301
		public void OnSpawn()
		{
			DebugUtility.LogMethod(this, "OnSpawn", Array.Empty<object>());
		}

		// Token: 0x060000FE RID: 254 RVA: 0x00007114 File Offset: 0x00005314
		public void UpdatePosition()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdatePosition", Array.Empty<object>());
			}
			if (!this.Target)
			{
				this.Close();
				return;
			}
			Vector3 screenPosition = this.positioner.GetScreenPosition(this.Target);
			float num = this.visualRootRectTransform.rect.width / 2f * this.visualRootRectTransform.lossyScale.x;
			float num2 = this.visualRootRectTransform.rect.height / 2f * this.visualRootRectTransform.lossyScale.y;
			bool flag = UIAnchorPositioner.IsScreenPositionVisible(screenPosition, num, num2);
			if (this.canvas.enabled != flag)
			{
				this.canvas.enabled = flag;
				if (!flag)
				{
					return;
				}
			}
			this.positioner.SetScreenPosition(screenPosition);
			this.anchorScaler.UpdateScale(this.Target, screenPosition);
		}

		// Token: 0x060000FF RID: 255 RVA: 0x000071FC File Offset: 0x000053FC
		public void Close()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Close", Array.Empty<object>());
			}
			if (this.displayAndHideHandler.IsTweeningHide)
			{
				return;
			}
			if (this.playerReferenceManager)
			{
				this.playerReferenceManager.OnCharacterCosmeticsChanged.RemoveListener(new UnityAction<CharacterCosmeticsDefinition>(this.ViewCharacterCosmetic));
			}
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.initializeCancellationTokenSource);
			MonoBehaviourSingleton<UIAnchorManager>.Instance.UnregisterAnchor(this);
			this.displayAndHideHandler.Hide(new Action(this.Despawn));
		}

		// Token: 0x06000100 RID: 256 RVA: 0x00007285 File Offset: 0x00005485
		public void PlayDisplayTween()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "PlayDisplayTween", Array.Empty<object>());
			}
			this.displayAndHideHandler.Display();
		}

		// Token: 0x06000101 RID: 257 RVA: 0x000072AA File Offset: 0x000054AA
		private void Despawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Despawn", Array.Empty<object>());
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIPlayerNameAnchor>(this);
		}

		// Token: 0x06000102 RID: 258 RVA: 0x000072CF File Offset: 0x000054CF
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnToggleUiVisibility.AddListener(new UnityAction<bool>(this.OnToggleUiVisibility));
		}

		// Token: 0x06000103 RID: 259 RVA: 0x00007304 File Offset: 0x00005504
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnToggleUiVisibility.RemoveListener(new UnityAction<bool>(this.OnToggleUiVisibility));
		}

		// Token: 0x06000104 RID: 260 RVA: 0x0000733C File Offset: 0x0000553C
		private Task InitializeAsync(PlayerReferenceManager playerReferenceManager)
		{
			UIPlayerNameAnchor.<InitializeAsync>d__36 <InitializeAsync>d__;
			<InitializeAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<InitializeAsync>d__.<>4__this = this;
			<InitializeAsync>d__.playerReferenceManager = playerReferenceManager;
			<InitializeAsync>d__.<>1__state = -1;
			<InitializeAsync>d__.<>t__builder.Start<UIPlayerNameAnchor.<InitializeAsync>d__36>(ref <InitializeAsync>d__);
			return <InitializeAsync>d__.<>t__builder.Task;
		}

		// Token: 0x06000105 RID: 261 RVA: 0x00007387 File Offset: 0x00005587
		private void ViewCharacterCosmetic(CharacterCosmeticsDefinition characterCosmeticsDefinition)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewCharacterCosmetic", new object[] { characterCosmeticsDefinition.DisplayName });
			}
			this.characterCosmeticsDefinitionPortrait.Display(characterCosmeticsDefinition.AssetId);
		}

		// Token: 0x06000106 RID: 262 RVA: 0x000073BC File Offset: 0x000055BC
		private void OnToggleUiVisibility(bool hide)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnToggleUiVisibility", new object[] { hide });
			}
			this.toggleVisibilityObject.SetActive(!hide);
		}

		// Token: 0x04000096 RID: 150
		[Header("UIPlayerNameAnchor")]
		[SerializeField]
		private Canvas canvas;

		// Token: 0x04000097 RID: 151
		[SerializeField]
		private RectTransform visualRootRectTransform;

		// Token: 0x04000098 RID: 152
		[SerializeField]
		private UICharacterCosmeticsDefinitionPortraitView characterCosmeticsDefinitionPortrait;

		// Token: 0x04000099 RID: 153
		[SerializeField]
		private TextMeshProUGUI playerNameText;

		// Token: 0x0400009A RID: 154
		[SerializeField]
		private GameObject toggleVisibilityObject;

		// Token: 0x0400009B RID: 155
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x0400009C RID: 156
		[SerializeField]
		private UIAnchorPositioner positioner;

		// Token: 0x0400009D RID: 157
		[SerializeField]
		private UIAnchorScaler anchorScaler;

		// Token: 0x0400009E RID: 158
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040000A3 RID: 163
		private PlayerReferenceManager playerReferenceManager;

		// Token: 0x040000A4 RID: 164
		private CancellationTokenSource initializeCancellationTokenSource;
	}
}
