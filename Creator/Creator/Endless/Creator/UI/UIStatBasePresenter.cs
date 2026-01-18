using System;
using Endless.Gameplay;
using Endless.Gameplay.Stats;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000252 RID: 594
	public abstract class UIStatBasePresenter<TModel> : UIBasePresenter<TModel> where TModel : StatBase
	{
		// Token: 0x060009A4 RID: 2468 RVA: 0x0002CC14 File Offset: 0x0002AE14
		protected override void Start()
		{
			base.Start();
			this.statBaseView = base.View.Interface as UIStatBaseView<TModel>;
			this.statBaseView.IdentifierChanged += this.SetIdentifier;
			this.statBaseView.MessageChanged += this.SetMessage;
			this.statBaseView.OrderChanged += this.SetOrder;
			this.statBaseView.InventoryIconChanged += this.SetInventoryIcon;
		}

		// Token: 0x060009A5 RID: 2469 RVA: 0x0002CC9C File Offset: 0x0002AE9C
		protected virtual void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnDestroy", this);
			}
			this.statBaseView.IdentifierChanged -= this.SetIdentifier;
			this.statBaseView.MessageChanged -= this.SetMessage;
			this.statBaseView.OrderChanged -= this.SetOrder;
			this.statBaseView.InventoryIconChanged -= this.SetInventoryIcon;
		}

		// Token: 0x060009A6 RID: 2470 RVA: 0x0002CD18 File Offset: 0x0002AF18
		private void SetIdentifier(string identifier)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("SetIdentifier ( identifier: " + identifier + " )", this);
			}
			base.Model.Identifier = identifier;
			base.InvokeOnModelChanged();
		}

		// Token: 0x060009A7 RID: 2471 RVA: 0x0002CD4F File Offset: 0x0002AF4F
		private void SetMessage(LocalizedString message)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetMessage", "message", message), this);
			}
			base.Model.Message = message;
			base.InvokeOnModelChanged();
		}

		// Token: 0x060009A8 RID: 2472 RVA: 0x0002CD8C File Offset: 0x0002AF8C
		private void SetOrder(int order)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetOrder", "order", order), this);
			}
			base.Model.Order = order;
			base.InvokeOnModelChanged();
		}

		// Token: 0x060009A9 RID: 2473 RVA: 0x0002CDD8 File Offset: 0x0002AFD8
		private void SetInventoryIcon(InventoryLibraryReference inventoryIcon)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetInventoryIcon", "inventoryIcon", inventoryIcon), this);
			}
			base.Model.InventoryIcon = inventoryIcon;
			base.InvokeOnModelChanged();
		}

		// Token: 0x040007ED RID: 2029
		private UIStatBaseView<TModel> statBaseView;
	}
}
