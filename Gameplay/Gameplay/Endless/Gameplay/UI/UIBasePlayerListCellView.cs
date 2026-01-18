using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003BD RID: 957
	public class UIBasePlayerListCellView : UIBaseListCellView<PlayerReferenceManager>, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000505 RID: 1285
		// (get) Token: 0x06001864 RID: 6244 RVA: 0x0007122F File Offset: 0x0006F42F
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000506 RID: 1286
		// (get) Token: 0x06001865 RID: 6245 RVA: 0x00071237 File Offset: 0x0006F437
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x17000507 RID: 1287
		// (get) Token: 0x06001866 RID: 6246 RVA: 0x0007123F File Offset: 0x0006F43F
		// (set) Token: 0x06001867 RID: 6247 RVA: 0x00071247 File Offset: 0x0006F447
		private protected PlayerReferenceManager PlayerReferenceManager { protected get; private set; }

		// Token: 0x06001868 RID: 6248 RVA: 0x00071250 File Offset: 0x0006F450
		public override void OnDespawn()
		{
			base.OnDespawn();
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.viewAsyncCancellationTokenSource);
			if (!this.PlayerReferenceManager)
			{
				return;
			}
			this.PlayerReferenceManager.OnCharacterCosmeticsChanged.RemoveListener(new UnityAction<CharacterCosmeticsDefinition>(this.ViewCharacterCosmeticsDefinition));
			this.PlayerReferenceManager = null;
		}

		// Token: 0x06001869 RID: 6249 RVA: 0x0007129F File Offset: 0x0006F49F
		public override void View(UIBaseListView<PlayerReferenceManager> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.ViewAsync();
		}

		// Token: 0x0600186A RID: 6250 RVA: 0x000712B0 File Offset: 0x0006F4B0
		private Task ViewAsync()
		{
			UIBasePlayerListCellView.<ViewAsync>d__22 <ViewAsync>d__;
			<ViewAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<ViewAsync>d__.<>4__this = this;
			<ViewAsync>d__.<>1__state = -1;
			<ViewAsync>d__.<>t__builder.Start<UIBasePlayerListCellView.<ViewAsync>d__22>(ref <ViewAsync>d__);
			return <ViewAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600186B RID: 6251 RVA: 0x000712F3 File Offset: 0x0006F4F3
		private void ViewCharacterCosmeticsDefinition(CharacterCosmeticsDefinition characterCosmeticsDefinition)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewCharacterCosmeticsDefinition", "characterCosmeticsDefinition", characterCosmeticsDefinition), this);
			}
			this.characterCosmeticsDefinitionPortrait.Display(characterCosmeticsDefinition.AssetId);
		}

		// Token: 0x0400138D RID: 5005
		[Header("UIBasePlayerListCellView")]
		[SerializeField]
		private TextMeshProUGUI userSlotText;

		// Token: 0x0400138E RID: 5006
		[SerializeField]
		private UICharacterCosmeticsDefinitionPortraitView characterCosmeticsDefinitionPortrait;

		// Token: 0x0400138F RID: 5007
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x04001390 RID: 5008
		[SerializeField]
		private TweenCollection onInitializeTweens;

		// Token: 0x04001391 RID: 5009
		[SerializeField]
		private Color textColorIfLocalClient = Color.white;

		// Token: 0x04001392 RID: 5010
		[FormerlySerializedAs("textdColorIfNotLocalClient")]
		[SerializeField]
		private Color textColorIfNotLocalClient = Color.blue;

		// Token: 0x04001393 RID: 5011
		[Header("Background")]
		[SerializeField]
		private Image backgroundImage;

		// Token: 0x04001394 RID: 5012
		[SerializeField]
		private Color backgroundColorIfLocalClient = Color.blue;

		// Token: 0x04001395 RID: 5013
		[SerializeField]
		private Color backgroundColorIfNotLocalClient = Color.black;

		// Token: 0x04001396 RID: 5014
		private CancellationTokenSource viewAsyncCancellationTokenSource;
	}
}
