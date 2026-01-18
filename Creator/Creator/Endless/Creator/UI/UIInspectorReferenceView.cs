using System;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200021C RID: 540
	public abstract class UIInspectorReferenceView<TModel, TViewStyle> : UIBaseView<TModel, TViewStyle>, IUIInteractable, IReadAndWritePermissible, IInspectorReferenceViewable where TModel : InspectorReference where TViewStyle : Enum
	{
		// Token: 0x14000013 RID: 19
		// (add) Token: 0x060008A7 RID: 2215 RVA: 0x00029FC4 File Offset: 0x000281C4
		// (remove) Token: 0x060008A8 RID: 2216 RVA: 0x00029FFC File Offset: 0x000281FC
		public event Action OnClear;

		// Token: 0x14000014 RID: 20
		// (add) Token: 0x060008A9 RID: 2217 RVA: 0x0002A034 File Offset: 0x00028234
		// (remove) Token: 0x060008AA RID: 2218 RVA: 0x0002A06C File Offset: 0x0002826C
		public event Action OnOpenIEnumerableWindow;

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x060008AB RID: 2219 RVA: 0x0002A0A1 File Offset: 0x000282A1
		// (set) Token: 0x060008AC RID: 2220 RVA: 0x0002A0A9 File Offset: 0x000282A9
		private protected Permissions Permission { protected get; private set; } = Permissions.ReadWrite;

		// Token: 0x060008AD RID: 2221 RVA: 0x0002A0B4 File Offset: 0x000282B4
		protected virtual void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.clearButton.onClick.AddListener(new UnityAction(this.OnClearButtonPressed));
			this.openIEnumerableWindowButton.onClick.AddListener(new UnityAction(this.OnOpenSelectionWindowButtonPressed));
			this.SetPermission(this.Permission);
		}

		// Token: 0x060008AE RID: 2222 RVA: 0x0002A118 File Offset: 0x00028318
		public override void View(TModel model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "model", model), this);
			}
			this.referenceName.text = this.GetReferenceName(model);
			this.referenceIsEmpty = this.GetReferenceIsEmpty(model);
			this.HandleClearButtonVisibility();
			this.Layout();
		}

		// Token: 0x060008AF RID: 2223 RVA: 0x0002A178 File Offset: 0x00028378
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Clear", this);
			}
			this.Layout();
		}

		// Token: 0x060008B0 RID: 2224 RVA: 0x0002A194 File Offset: 0x00028394
		public virtual void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetInteractable", "interactable", interactable), this);
			}
			this.clearButton.interactable = interactable;
			this.openIEnumerableWindowButton.interactable = interactable;
		}

		// Token: 0x060008B1 RID: 2225 RVA: 0x0002A1E4 File Offset: 0x000283E4
		public virtual void SetPermission(Permissions permission)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetPermission", "permission", permission), this);
			}
			this.Permission = permission;
			this.HandleClearButtonVisibility();
			this.openIEnumerableWindowButton.gameObject.SetActive(permission == Permissions.ReadWrite);
			this.Layout();
		}

		// Token: 0x060008B2 RID: 2226
		protected abstract string GetReferenceName(TModel model);

		// Token: 0x060008B3 RID: 2227 RVA: 0x0002A240 File Offset: 0x00028440
		protected virtual bool GetReferenceIsEmpty(TModel model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetReferenceIsEmpty", "model", model), this);
			}
			return model.IsReferenceEmpty();
		}

		// Token: 0x060008B4 RID: 2228 RVA: 0x0002A278 File Offset: 0x00028478
		protected void SetObjectNameAndClearButtonVisibility(string objectName, bool visible)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetObjectNameAndClearButtonVisibility", "objectName", objectName, "visible", visible }), this);
			}
			this.referenceName.text = objectName;
			this.clearButton.gameObject.SetActive(visible);
			this.Layout();
		}

		// Token: 0x060008B5 RID: 2229 RVA: 0x0002A2EB File Offset: 0x000284EB
		protected void Layout()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Layout", this);
			}
			this.layoutables.Layout();
			this.layoutables.RequestLayout();
		}

		// Token: 0x060008B6 RID: 2230 RVA: 0x0002A316 File Offset: 0x00028516
		private void OnClearButtonPressed()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnClearButtonPressed", this);
			}
			Action onClear = this.OnClear;
			if (onClear == null)
			{
				return;
			}
			onClear();
		}

		// Token: 0x060008B7 RID: 2231 RVA: 0x0002A33B File Offset: 0x0002853B
		private void OnOpenSelectionWindowButtonPressed()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnOpenSelectionWindowButtonPressed", this);
			}
			Action onOpenIEnumerableWindow = this.OnOpenIEnumerableWindow;
			if (onOpenIEnumerableWindow == null)
			{
				return;
			}
			onOpenIEnumerableWindow();
		}

		// Token: 0x060008B8 RID: 2232 RVA: 0x0002A360 File Offset: 0x00028560
		private void HandleClearButtonVisibility()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("HandleClearButtonVisibility", this);
			}
			this.clearButton.gameObject.SetActive(!this.referenceIsEmpty && this.Permission == Permissions.ReadWrite);
		}

		// Token: 0x04000777 RID: 1911
		[SerializeField]
		private TextMeshProUGUI referenceName;

		// Token: 0x04000778 RID: 1912
		[SerializeField]
		private UIButton clearButton;

		// Token: 0x04000779 RID: 1913
		[SerializeField]
		private UIButton openIEnumerableWindowButton;

		// Token: 0x0400077A RID: 1914
		[SerializeField]
		private InterfaceReference<IUILayoutable>[] layoutables = Array.Empty<InterfaceReference<IUILayoutable>>();

		// Token: 0x0400077B RID: 1915
		private bool referenceIsEmpty;
	}
}
