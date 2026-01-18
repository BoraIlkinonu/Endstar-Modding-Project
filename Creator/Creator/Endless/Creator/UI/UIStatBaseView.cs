using System;
using Endless.Gameplay;
using Endless.Gameplay.Stats;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000253 RID: 595
	public abstract class UIStatBaseView<TModel> : UIBaseView<TModel, UIStatBaseView<TModel>.Styles>, IUIInteractable where TModel : StatBase
	{
		// Token: 0x14000023 RID: 35
		// (add) Token: 0x060009AB RID: 2475 RVA: 0x0002CE1C File Offset: 0x0002B01C
		// (remove) Token: 0x060009AC RID: 2476 RVA: 0x0002CE54 File Offset: 0x0002B054
		public event Action<string> IdentifierChanged;

		// Token: 0x14000024 RID: 36
		// (add) Token: 0x060009AD RID: 2477 RVA: 0x0002CE8C File Offset: 0x0002B08C
		// (remove) Token: 0x060009AE RID: 2478 RVA: 0x0002CEC4 File Offset: 0x0002B0C4
		public event Action<LocalizedString> MessageChanged;

		// Token: 0x14000025 RID: 37
		// (add) Token: 0x060009AF RID: 2479 RVA: 0x0002CEFC File Offset: 0x0002B0FC
		// (remove) Token: 0x060009B0 RID: 2480 RVA: 0x0002CF34 File Offset: 0x0002B134
		public event Action<int> OrderChanged;

		// Token: 0x14000026 RID: 38
		// (add) Token: 0x060009B1 RID: 2481 RVA: 0x0002CF6C File Offset: 0x0002B16C
		// (remove) Token: 0x060009B2 RID: 2482 RVA: 0x0002CFA4 File Offset: 0x0002B1A4
		public event Action<InventoryLibraryReference> InventoryIconChanged;

		// Token: 0x17000139 RID: 313
		// (get) Token: 0x060009B3 RID: 2483 RVA: 0x0002CFD9 File Offset: 0x0002B1D9
		// (set) Token: 0x060009B4 RID: 2484 RVA: 0x0002CFE1 File Offset: 0x0002B1E1
		public override UIStatBaseView<TModel>.Styles Style { get; protected set; }

		// Token: 0x060009B5 RID: 2485 RVA: 0x0002CFEC File Offset: 0x0002B1EC
		protected virtual void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.identifierInputField.onValueChanged.AddListener(new UnityAction<string>(this.InvokeIdentifierChanged));
			this.messageInputField.OnModelChanged += this.InvokeMessageChanged;
			this.priorityControl.OnModelChanged += this.InvokePriorityChanged;
			this.inventoryIconControl.OnModelChanged += this.InvokeInventoryIconChanged;
		}

		// Token: 0x060009B6 RID: 2486 RVA: 0x0002D070 File Offset: 0x0002B270
		protected virtual void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnDestroy", this);
			}
			this.identifierInputField.onValueChanged.RemoveListener(new UnityAction<string>(this.InvokeIdentifierChanged));
			this.messageInputField.OnModelChanged -= this.InvokeMessageChanged;
			this.priorityControl.OnModelChanged -= this.InvokePriorityChanged;
			this.inventoryIconControl.OnModelChanged -= this.InvokeInventoryIconChanged;
		}

		// Token: 0x060009B7 RID: 2487 RVA: 0x0002D0F4 File Offset: 0x0002B2F4
		public override void View(TModel model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "model", model), this);
			}
			this.identifierInputField.SetTextWithoutNotify(model.Identifier);
			this.messageInputField.SetModel(model.Message, false);
			this.priorityControl.SetModel(model.Order, false);
			this.inventoryIconControl.SetModel(model.InventoryIcon, false);
		}

		// Token: 0x060009B8 RID: 2488 RVA: 0x0002D184 File Offset: 0x0002B384
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Clear", this);
			}
			this.identifierInputField.Clear(false);
			this.messageInputField.Clear();
			this.priorityControl.Clear();
			this.inventoryIconControl.Clear();
		}

		// Token: 0x060009B9 RID: 2489 RVA: 0x0002D1D4 File Offset: 0x0002B3D4
		public void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetInteractable", "interactable", interactable), this);
			}
			this.identifierInputField.interactable = interactable;
			IUIInteractable iuiinteractable = this.messageInputField.Viewable as IUIInteractable;
			if (iuiinteractable != null)
			{
				iuiinteractable.SetInteractable(interactable);
			}
			IUIInteractable iuiinteractable2 = this.priorityControl.Viewable as IUIInteractable;
			if (iuiinteractable2 != null)
			{
				iuiinteractable2.SetInteractable(interactable);
			}
			IUIInteractable iuiinteractable3 = this.inventoryIconControl.Viewable as IUIInteractable;
			if (iuiinteractable3 != null)
			{
				iuiinteractable3.SetInteractable(interactable);
			}
		}

		// Token: 0x060009BA RID: 2490 RVA: 0x0002D266 File Offset: 0x0002B466
		private void InvokeIdentifierChanged(string identifier)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("InvokeIdentifierChanged ( identifier: " + identifier + " )", this);
			}
			Action<string> identifierChanged = this.IdentifierChanged;
			if (identifierChanged == null)
			{
				return;
			}
			identifierChanged(identifier);
		}

		// Token: 0x060009BB RID: 2491 RVA: 0x0002D297 File Offset: 0x0002B497
		private void InvokeMessageChanged(object message)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeMessageChanged", "message", message), this);
			}
			Action<LocalizedString> messageChanged = this.MessageChanged;
			if (messageChanged == null)
			{
				return;
			}
			messageChanged((LocalizedString)message);
		}

		// Token: 0x060009BC RID: 2492 RVA: 0x0002D2D4 File Offset: 0x0002B4D4
		private void InvokePriorityChanged(object priority)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokePriorityChanged", "priority", priority), this);
			}
			int num = (int)priority;
			Action<int> orderChanged = this.OrderChanged;
			if (orderChanged == null)
			{
				return;
			}
			orderChanged(num);
		}

		// Token: 0x060009BD RID: 2493 RVA: 0x0002D31C File Offset: 0x0002B51C
		private void InvokeInventoryIconChanged(object inventoryIcon)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeInventoryIconChanged", "inventoryIcon", inventoryIcon), this);
			}
			InventoryLibraryReference inventoryLibraryReference = inventoryIcon as InventoryLibraryReference;
			Action<InventoryLibraryReference> inventoryIconChanged = this.InventoryIconChanged;
			if (inventoryIconChanged == null)
			{
				return;
			}
			inventoryIconChanged(inventoryLibraryReference);
		}

		// Token: 0x040007F3 RID: 2035
		[SerializeField]
		private UIInputField identifierInputField;

		// Token: 0x040007F4 RID: 2036
		[SerializeField]
		private UILocalizedStringPresenter messageInputField;

		// Token: 0x040007F5 RID: 2037
		[SerializeField]
		private UIIntPresenter priorityControl;

		// Token: 0x040007F6 RID: 2038
		[SerializeField]
		private UIInventoryLibraryReferencePresenter inventoryIconControl;

		// Token: 0x02000254 RID: 596
		public enum Styles
		{
			// Token: 0x040007F8 RID: 2040
			Default
		}
	}
}
