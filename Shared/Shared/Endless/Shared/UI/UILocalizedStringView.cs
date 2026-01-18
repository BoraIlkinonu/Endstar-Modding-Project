using System;
using Endless.Shared.Debugging;
using Runtime.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000204 RID: 516
	public class UILocalizedStringView : UIBaseView<LocalizedString, UILocalizedStringView.Styles>, IUIInteractable
	{
		// Token: 0x14000048 RID: 72
		// (add) Token: 0x06000D68 RID: 3432 RVA: 0x0003AFF0 File Offset: 0x000391F0
		// (remove) Token: 0x06000D69 RID: 3433 RVA: 0x0003B028 File Offset: 0x00039228
		public event Action<string> OnEnglishChanged;

		// Token: 0x14000049 RID: 73
		// (add) Token: 0x06000D6A RID: 3434 RVA: 0x0003B060 File Offset: 0x00039260
		// (remove) Token: 0x06000D6B RID: 3435 RVA: 0x0003B098 File Offset: 0x00039298
		public event Action<string> OnSpanishChanged;

		// Token: 0x1400004A RID: 74
		// (add) Token: 0x06000D6C RID: 3436 RVA: 0x0003B0D0 File Offset: 0x000392D0
		// (remove) Token: 0x06000D6D RID: 3437 RVA: 0x0003B108 File Offset: 0x00039308
		public event Action<string> OnArabicChanged;

		// Token: 0x17000276 RID: 630
		// (get) Token: 0x06000D6E RID: 3438 RVA: 0x0003B13D File Offset: 0x0003933D
		// (set) Token: 0x06000D6F RID: 3439 RVA: 0x0003B145 File Offset: 0x00039345
		public override UILocalizedStringView.Styles Style { get; protected set; }

		// Token: 0x06000D70 RID: 3440 RVA: 0x0003B14E File Offset: 0x0003934E
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.InitializeListeners();
		}

		// Token: 0x06000D71 RID: 3441 RVA: 0x0003B170 File Offset: 0x00039370
		public override void View(LocalizedString model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model.DebugSafeJson() });
			}
			this.englishInputField.SetTextWithoutNotify(model.GetString(Language.English));
			this.spanishInputField.SetTextWithoutNotify(model.GetString(Language.Spanish));
			this.arabicInputField.SetTextWithoutNotify(model.GetString(Language.Arabic));
		}

		// Token: 0x06000D72 RID: 3442 RVA: 0x0003B1D5 File Offset: 0x000393D5
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.englishInputField.Clear(false);
			this.spanishInputField.Clear(false);
			this.arabicInputField.Clear(false);
		}

		// Token: 0x06000D73 RID: 3443 RVA: 0x0003B214 File Offset: 0x00039414
		public void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { interactable });
			}
			this.englishInputField.interactable = interactable;
			this.spanishInputField.interactable = interactable;
			this.arabicInputField.interactable = interactable;
		}

		// Token: 0x06000D74 RID: 3444 RVA: 0x0003B268 File Offset: 0x00039468
		private void InitializeListeners()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InitializeListeners", Array.Empty<object>());
			}
			this.AddListeners(this.englishInputField, new UnityAction<string>(this.InvokeOnEnglishChanged));
			this.AddListeners(this.spanishInputField, new UnityAction<string>(this.InvokeOnSpanishChanged));
			this.AddListeners(this.arabicInputField, new UnityAction<string>(this.InvokeOnArabicChanged));
		}

		// Token: 0x06000D75 RID: 3445 RVA: 0x0003B2D8 File Offset: 0x000394D8
		private void AddListeners(UIInputField inputField, UnityAction<string> onChanged)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "AddListeners", new object[]
				{
					inputField.DebugSafeName(true),
					onChanged.Method.Name
				});
			}
			inputField.onValueChanged.AddListener(onChanged);
			inputField.onEndEdit.AddListener(onChanged);
			inputField.onSubmit.AddListener(onChanged);
			inputField.DeselectAndValueChangedUnityEvent.AddListener(onChanged);
		}

		// Token: 0x06000D76 RID: 3446 RVA: 0x0003B346 File Offset: 0x00039546
		private void InvokeOnEnglishChanged(string value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeOnEnglishChanged", new object[] { value });
			}
			Action<string> onEnglishChanged = this.OnEnglishChanged;
			if (onEnglishChanged == null)
			{
				return;
			}
			onEnglishChanged(value);
		}

		// Token: 0x06000D77 RID: 3447 RVA: 0x0003B376 File Offset: 0x00039576
		private void InvokeOnSpanishChanged(string value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeOnSpanishChanged", new object[] { value });
			}
			Action<string> onSpanishChanged = this.OnSpanishChanged;
			if (onSpanishChanged == null)
			{
				return;
			}
			onSpanishChanged(value);
		}

		// Token: 0x06000D78 RID: 3448 RVA: 0x0003B3A6 File Offset: 0x000395A6
		private void InvokeOnArabicChanged(string value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeOnArabicChanged", new object[] { value });
			}
			Action<string> onArabicChanged = this.OnArabicChanged;
			if (onArabicChanged == null)
			{
				return;
			}
			onArabicChanged(value);
		}

		// Token: 0x040008B6 RID: 2230
		[SerializeField]
		private UIInputField englishInputField;

		// Token: 0x040008B7 RID: 2231
		[SerializeField]
		private UIInputField spanishInputField;

		// Token: 0x040008B8 RID: 2232
		[SerializeField]
		private UIInputField arabicInputField;

		// Token: 0x02000205 RID: 517
		public enum Styles
		{
			// Token: 0x040008BA RID: 2234
			Default
		}
	}
}
