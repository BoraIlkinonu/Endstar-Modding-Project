using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000266 RID: 614
	public class UISpriteAndEnumTab : UIBaseTab<SpriteAndEnum>
	{
		// Token: 0x06000F91 RID: 3985 RVA: 0x00042E48 File Offset: 0x00041048
		protected override void ViewOption(SpriteAndEnum option)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewOption", new object[] { option });
			}
			this.image.gameObject.SetActive(option.Sprite);
			this.text.gameObject.SetActive(option.Enum != null);
			this.image.sprite = option.Sprite;
			if (option.Enum == null)
			{
				return;
			}
			UIText uitext = this.text;
			UserFacingTextAttribute attributeOfType = option.Enum.GetAttributeOfType<UserFacingTextAttribute>();
			uitext.Value = ((attributeOfType != null) ? attributeOfType.UserFacingText : null) ?? option.Enum.ToString();
		}

		// Token: 0x040009ED RID: 2541
		[Header("UISpriteAndEnumTab")]
		[SerializeField]
		private Image image;

		// Token: 0x040009EE RID: 2542
		[SerializeField]
		private UIText text;
	}
}
