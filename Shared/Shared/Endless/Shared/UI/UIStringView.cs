using System;
using Endless.Shared.Debugging;
using Runtime.Shared;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x0200023B RID: 571
	public class UIStringView : UIBaseView<string, UIStringView.Styles>, IUIInteractable
	{
		// Token: 0x170002B7 RID: 695
		// (get) Token: 0x06000E8A RID: 3722 RVA: 0x0003F34D File Offset: 0x0003D54D
		// (set) Token: 0x06000E8B RID: 3723 RVA: 0x0003F355 File Offset: 0x0003D555
		public override UIStringView.Styles Style { get; protected set; }

		// Token: 0x170002B8 RID: 696
		// (get) Token: 0x06000E8C RID: 3724 RVA: 0x0003F35E File Offset: 0x0003D55E
		// (set) Token: 0x06000E8D RID: 3725 RVA: 0x0003F366 File Offset: 0x0003D566
		public UIInputField InputField { get; private set; }

		// Token: 0x06000E8E RID: 3726 RVA: 0x0003F370 File Offset: 0x0003D570
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.InputField.onValueChanged.AddListener(new UnityAction<string>(this.InvokeOnValueChanged));
			this.InputField.onEndEdit.AddListener(new UnityAction<string>(this.InvokeOnValueChanged));
			this.InputField.onSubmit.AddListener(new UnityAction<string>(this.InvokeOnValueChanged));
		}

		// Token: 0x1400004E RID: 78
		// (add) Token: 0x06000E8F RID: 3727 RVA: 0x0003F3EC File Offset: 0x0003D5EC
		// (remove) Token: 0x06000E90 RID: 3728 RVA: 0x0003F424 File Offset: 0x0003D624
		public event Action<string> OnValueChanged;

		// Token: 0x06000E91 RID: 3729 RVA: 0x0003F459 File Offset: 0x0003D659
		public override void View(string model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			this.InputField.SetTextWithoutNotify(model);
		}

		// Token: 0x06000E92 RID: 3730 RVA: 0x0003F484 File Offset: 0x0003D684
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.InputField.SetTextWithoutNotify(string.Empty);
		}

		// Token: 0x06000E93 RID: 3731 RVA: 0x0003F4AE File Offset: 0x0003D6AE
		public void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { interactable });
			}
			this.InputField.interactable = interactable;
		}

		// Token: 0x06000E94 RID: 3732 RVA: 0x0003F4DE File Offset: 0x0003D6DE
		private void InvokeOnValueChanged(string model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeOnValueChanged", new object[] { model });
			}
			Action<string> onValueChanged = this.OnValueChanged;
			if (onValueChanged == null)
			{
				return;
			}
			onValueChanged(model);
		}

		// Token: 0x0200023C RID: 572
		public enum Styles
		{
			// Token: 0x04000937 RID: 2359
			Default,
			// Token: 0x04000938 RID: 2360
			Multiline
		}
	}
}
