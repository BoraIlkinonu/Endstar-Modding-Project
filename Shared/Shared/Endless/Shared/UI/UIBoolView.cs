using System;
using Endless.Shared.Debugging;
using Runtime.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x020001DA RID: 474
	public class UIBoolView : UIBaseView<bool, UIBoolView.Styles>, IUIInteractable
	{
		// Token: 0x1700022D RID: 557
		// (get) Token: 0x06000BA2 RID: 2978 RVA: 0x000323C7 File Offset: 0x000305C7
		// (set) Token: 0x06000BA3 RID: 2979 RVA: 0x000323CF File Offset: 0x000305CF
		public override UIBoolView.Styles Style { get; protected set; }

		// Token: 0x14000033 RID: 51
		// (add) Token: 0x06000BA4 RID: 2980 RVA: 0x000323D8 File Offset: 0x000305D8
		// (remove) Token: 0x06000BA5 RID: 2981 RVA: 0x00032410 File Offset: 0x00030610
		public event Action<bool> OnUserChangedModel;

		// Token: 0x06000BA6 RID: 2982 RVA: 0x00032445 File Offset: 0x00030645
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.toggle.OnChange.AddListener(new UnityAction<bool>(this.OnTogglePressed));
		}

		// Token: 0x06000BA7 RID: 2983 RVA: 0x0003247B File Offset: 0x0003067B
		public override void View(bool model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			this.toggle.SetIsOn(model, true, true);
		}

		// Token: 0x06000BA8 RID: 2984 RVA: 0x000324AD File Offset: 0x000306AD
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.toggle.SetIsOn(false, true, true);
		}

		// Token: 0x06000BA9 RID: 2985 RVA: 0x000324D5 File Offset: 0x000306D5
		public void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { interactable });
			}
			this.toggle.SetInteractable(interactable, false);
		}

		// Token: 0x06000BAA RID: 2986 RVA: 0x00032506 File Offset: 0x00030706
		private void OnTogglePressed(bool model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnTogglePressed", new object[] { model });
			}
			Action<bool> onUserChangedModel = this.OnUserChangedModel;
			if (onUserChangedModel == null)
			{
				return;
			}
			onUserChangedModel(model);
		}

		// Token: 0x04000790 RID: 1936
		[SerializeField]
		private UIToggle toggle;

		// Token: 0x020001DB RID: 475
		public enum Styles
		{
			// Token: 0x04000793 RID: 1939
			Default
		}
	}
}
