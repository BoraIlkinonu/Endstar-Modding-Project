using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003DC RID: 988
	public class UICharacterCosmeticsDefinitionView : UIBaseView<CharacterCosmeticsDefinition, UICharacterCosmeticsDefinitionView.Styles>
	{
		// Token: 0x1700051A RID: 1306
		// (get) Token: 0x060018F2 RID: 6386 RVA: 0x00073CFF File Offset: 0x00071EFF
		// (set) Token: 0x060018F3 RID: 6387 RVA: 0x00073D07 File Offset: 0x00071F07
		public override UICharacterCosmeticsDefinitionView.Styles Style { get; protected set; }

		// Token: 0x060018F4 RID: 6388 RVA: 0x00073D10 File Offset: 0x00071F10
		public override void View(CharacterCosmeticsDefinition model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model.DisplayName });
			}
			this.characterCosmeticsDefinitionPortrait.Display(model.AssetId);
		}

		// Token: 0x060018F5 RID: 6389 RVA: 0x00073D45 File Offset: 0x00071F45
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
		}

		// Token: 0x0400140E RID: 5134
		[SerializeField]
		private UICharacterCosmeticsDefinitionPortraitView characterCosmeticsDefinitionPortrait;

		// Token: 0x020003DD RID: 989
		public enum Styles
		{
			// Token: 0x04001410 RID: 5136
			Default
		}
	}
}
