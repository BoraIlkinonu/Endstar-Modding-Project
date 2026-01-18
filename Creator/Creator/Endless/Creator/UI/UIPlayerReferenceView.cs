using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000242 RID: 578
	public class UIPlayerReferenceView : UIBaseView<PlayerReference, UIPlayerReferenceView.Styles>, IUIInteractable
	{
		// Token: 0x1400001D RID: 29
		// (add) Token: 0x06000957 RID: 2391 RVA: 0x0002BB04 File Offset: 0x00029D04
		// (remove) Token: 0x06000958 RID: 2392 RVA: 0x0002BB3C File Offset: 0x00029D3C
		public event Action<bool> OnUseContextChanged;

		// Token: 0x1400001E RID: 30
		// (add) Token: 0x06000959 RID: 2393 RVA: 0x0002BB74 File Offset: 0x00029D74
		// (remove) Token: 0x0600095A RID: 2394 RVA: 0x0002BBAC File Offset: 0x00029DAC
		public event Action<int> OnPlayerNumberChanged;

		// Token: 0x17000131 RID: 305
		// (get) Token: 0x0600095B RID: 2395 RVA: 0x0002BBE1 File Offset: 0x00029DE1
		// (set) Token: 0x0600095C RID: 2396 RVA: 0x0002BBE9 File Offset: 0x00029DE9
		public override UIPlayerReferenceView.Styles Style { get; protected set; }

		// Token: 0x0600095D RID: 2397 RVA: 0x0002BBF4 File Offset: 0x00029DF4
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.useContextToggle.OnChange.AddListener(new UnityAction<bool>(this.InvokeOnUseContextChanged));
			this.playerNumberInputField.DeselectAndValueChangedUnityEvent.AddListener(new UnityAction<string>(this.InvokeOnPlayerNumberChanged));
		}

		// Token: 0x0600095E RID: 2398 RVA: 0x0002BC54 File Offset: 0x00029E54
		public override void View(PlayerReference model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			bool useContext = PlayerReferenceUtility.GetUseContext(model);
			this.useContextToggle.SetIsOn(useContext, true, true);
			this.playerNumberContainer.SetActive(!useContext);
			int playerNumber = PlayerReferenceUtility.GetPlayerNumber(model);
			this.playerNumberInputField.SetTextWithoutNotify(playerNumber.ToString());
		}

		// Token: 0x0600095F RID: 2399 RVA: 0x0002BCBB File Offset: 0x00029EBB
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.playerNumberInputField.Clear(false);
		}

		// Token: 0x06000960 RID: 2400 RVA: 0x0002BCE1 File Offset: 0x00029EE1
		public void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { interactable });
			}
			this.useContextToggle.SetInteractable(interactable, false);
			this.playerNumberInputField.interactable = interactable;
		}

		// Token: 0x06000961 RID: 2401 RVA: 0x0002BD1E File Offset: 0x00029F1E
		private void InvokeOnUseContextChanged(bool useContext)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeOnUseContextChanged", new object[] { useContext });
			}
			Action<bool> onUseContextChanged = this.OnUseContextChanged;
			if (onUseContextChanged == null)
			{
				return;
			}
			onUseContextChanged(useContext);
		}

		// Token: 0x06000962 RID: 2402 RVA: 0x0002BD54 File Offset: 0x00029F54
		private void InvokeOnPlayerNumberChanged(string playerNumberAsString)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeOnPlayerNumberChanged", new object[] { playerNumberAsString });
			}
			int num;
			if (!int.TryParse(playerNumberAsString, out num))
			{
				DebugUtility.LogError(this, "InvokeOnPlayerNumberChanged", "Could not convert to int!", new object[] { playerNumberAsString });
				return;
			}
			Action<int> onPlayerNumberChanged = this.OnPlayerNumberChanged;
			if (onPlayerNumberChanged == null)
			{
				return;
			}
			onPlayerNumberChanged(num);
		}

		// Token: 0x040007B2 RID: 1970
		[SerializeField]
		private UIToggle useContextToggle;

		// Token: 0x040007B3 RID: 1971
		[SerializeField]
		private GameObject playerNumberContainer;

		// Token: 0x040007B4 RID: 1972
		[SerializeField]
		private UIInputField playerNumberInputField;

		// Token: 0x02000243 RID: 579
		public enum Styles
		{
			// Token: 0x040007B6 RID: 1974
			Default
		}
	}
}
