using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000230 RID: 560
	public class UIKeyLibraryReferenceView : UIBaseInventoryLibraryReferenceView<KeyLibraryReference, UIKeyLibraryReferenceView.Styles>, IUIInteractable
	{
		// Token: 0x1400001B RID: 27
		// (add) Token: 0x06000912 RID: 2322 RVA: 0x0002B2F0 File Offset: 0x000294F0
		// (remove) Token: 0x06000913 RID: 2323 RVA: 0x0002B328 File Offset: 0x00029528
		public event Action<bool> OnLockedChanged;

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x06000914 RID: 2324 RVA: 0x0002B35D File Offset: 0x0002955D
		// (set) Token: 0x06000915 RID: 2325 RVA: 0x0002B365 File Offset: 0x00029565
		public override UIKeyLibraryReferenceView.Styles Style { get; protected set; }

		// Token: 0x1700011F RID: 287
		// (get) Token: 0x06000916 RID: 2326 RVA: 0x0002B36E File Offset: 0x0002956E
		protected override ReferenceFilter ReferenceFilter
		{
			get
			{
				return ReferenceFilter.Key;
			}
		}

		// Token: 0x06000917 RID: 2327 RVA: 0x0002B372 File Offset: 0x00029572
		protected override void Start()
		{
			base.Start();
			this.lockedToggle.OnChange.AddListener(new UnityAction<bool>(this.InvokeOnLockedChanged));
		}

		// Token: 0x06000918 RID: 2328 RVA: 0x0002B398 File Offset: 0x00029598
		public override void View(KeyLibraryReference model)
		{
			bool flag = model != null && !model.IsReferenceEmpty() && !InspectorReferenceUtility.GetId(model).IsEmpty;
			this.lockedToggle.SetIsOn(flag, false, true);
		}

		// Token: 0x06000919 RID: 2329 RVA: 0x0002B3D9 File Offset: 0x000295D9
		public override void SetInteractable(bool interactable)
		{
			base.SetInteractable(interactable);
			this.lockedToggle.SetInteractable(interactable, false);
		}

		// Token: 0x0600091A RID: 2330 RVA: 0x0002B3EF File Offset: 0x000295EF
		protected override string GetReferenceName(KeyLibraryReference model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetReferenceName", new object[] { model });
			}
			return base.GetPropLibraryReferenceName(model);
		}

		// Token: 0x0600091B RID: 2331 RVA: 0x0002B415 File Offset: 0x00029615
		private void InvokeOnLockedChanged(bool locked)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeOnLockedChanged", new object[] { locked });
			}
			Action<bool> onLockedChanged = this.OnLockedChanged;
			if (onLockedChanged == null)
			{
				return;
			}
			onLockedChanged(locked);
		}

		// Token: 0x04000795 RID: 1941
		[SerializeField]
		private UIToggle lockedToggle;

		// Token: 0x02000231 RID: 561
		public enum Styles
		{
			// Token: 0x04000797 RID: 1943
			Default
		}
	}
}
