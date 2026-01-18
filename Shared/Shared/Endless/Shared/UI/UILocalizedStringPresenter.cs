using System;
using Endless.Shared.Debugging;

namespace Endless.Shared.UI
{
	// Token: 0x02000203 RID: 515
	public class UILocalizedStringPresenter : UIBasePresenter<LocalizedString>
	{
		// Token: 0x06000D62 RID: 3426 RVA: 0x0003AE68 File Offset: 0x00039068
		protected override void Start()
		{
			base.Start();
			UILocalizedStringView uilocalizedStringView = base.View.Interface as UILocalizedStringView;
			uilocalizedStringView.OnEnglishChanged += this.SetEnglish;
			uilocalizedStringView.OnSpanishChanged += this.SetSpanish;
			uilocalizedStringView.OnArabicChanged += this.SetArabic;
		}

		// Token: 0x06000D63 RID: 3427 RVA: 0x0003AEC0 File Offset: 0x000390C0
		public override void SetModel(LocalizedString model, bool triggerOnModelChanged)
		{
			if (model == null)
			{
				model = new LocalizedString();
			}
			base.SetModel(model, triggerOnModelChanged);
		}

		// Token: 0x06000D64 RID: 3428 RVA: 0x0003AED4 File Offset: 0x000390D4
		public void SetEnglish(string newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetEnglish", new object[] { newValue });
			}
			if (base.Model.GetString(Language.English) == newValue)
			{
				return;
			}
			base.Model.SetStringValue(newValue, Language.English);
			this.SetModel(base.Model, true);
		}

		// Token: 0x06000D65 RID: 3429 RVA: 0x0003AF30 File Offset: 0x00039130
		public void SetSpanish(string newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetSpanish", new object[] { newValue });
			}
			if (base.Model.GetString(Language.Spanish) == newValue)
			{
				return;
			}
			base.Model.SetStringValue(newValue, Language.Spanish);
			this.SetModel(base.Model, true);
		}

		// Token: 0x06000D66 RID: 3430 RVA: 0x0003AF8C File Offset: 0x0003918C
		public void SetArabic(string newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetArabic", new object[] { newValue });
			}
			if (base.Model.GetString(Language.Arabic) == newValue)
			{
				return;
			}
			base.Model.SetStringValue(newValue, Language.Arabic);
			this.SetModel(base.Model, true);
		}
	}
}
