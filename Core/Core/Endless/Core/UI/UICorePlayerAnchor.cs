using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Endless.Gameplay;
using Endless.Gameplay.UI;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000039 RID: 57
	public class UICorePlayerAnchor : UIBaseAnchor, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000111 RID: 273 RVA: 0x00007AA9 File Offset: 0x00005CA9
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000112 RID: 274 RVA: 0x00007AB1 File Offset: 0x00005CB1
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000113 RID: 275 RVA: 0x00007AB9 File Offset: 0x00005CB9
		public static UICorePlayerAnchor CreateInstance(UICorePlayerAnchor prefab, Transform target, RectTransform container, ClientData clientData, Vector3? offset = null)
		{
			UICorePlayerAnchor uicorePlayerAnchor = UIBaseAnchor.CreateAndInitialize<UICorePlayerAnchor>(prefab, target, container, offset);
			uicorePlayerAnchor.SetClientData(clientData);
			return uicorePlayerAnchor;
		}

		// Token: 0x06000114 RID: 276 RVA: 0x00007ACC File Offset: 0x00005CCC
		public void SetClientData(ClientData clientData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetClientData", new object[] { clientData });
			}
			this.InitializeAsync(clientData);
		}

		// Token: 0x06000115 RID: 277 RVA: 0x00007AF8 File Offset: 0x00005CF8
		public override void Close()
		{
			if (this.displayAndHideHandler.IsTweeningHide)
			{
				return;
			}
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.initializeCancellationTokenSource);
			if (this.isLocalClient)
			{
				CharacterCosmeticsDefinitionUtility.ClientCharacterCosmeticsDefinitionAssetSetAction = (Action<SerializableGuid>)Delegate.Remove(CharacterCosmeticsDefinitionUtility.ClientCharacterCosmeticsDefinitionAssetSetAction, new Action<SerializableGuid>(this.UpdateCharacterVisualPortrait));
			}
			base.Close();
		}

		// Token: 0x06000116 RID: 278 RVA: 0x00007B4C File Offset: 0x00005D4C
		private Task InitializeAsync(ClientData clientData)
		{
			UICorePlayerAnchor.<InitializeAsync>d__13 <InitializeAsync>d__;
			<InitializeAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<InitializeAsync>d__.<>4__this = this;
			<InitializeAsync>d__.clientData = clientData;
			<InitializeAsync>d__.<>1__state = -1;
			<InitializeAsync>d__.<>t__builder.Start<UICorePlayerAnchor.<InitializeAsync>d__13>(ref <InitializeAsync>d__);
			return <InitializeAsync>d__.<>t__builder.Task;
		}

		// Token: 0x06000117 RID: 279 RVA: 0x00007B97 File Offset: 0x00005D97
		private void UpdateCharacterVisualPortrait(SerializableGuid characterVisualId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateCharacterVisualPortrait", new object[] { characterVisualId });
			}
			this.characterCosmeticsDefinitionPortrait.Display(characterVisualId);
		}

		// Token: 0x040000B1 RID: 177
		[Header("UICorePlayerAnchor")]
		[SerializeField]
		private UICharacterCosmeticsDefinitionPortraitView characterCosmeticsDefinitionPortrait;

		// Token: 0x040000B2 RID: 178
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x040000B5 RID: 181
		private bool isLocalClient;

		// Token: 0x040000B6 RID: 182
		private CancellationTokenSource initializeCancellationTokenSource;
	}
}
