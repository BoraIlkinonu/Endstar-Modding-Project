using System;
using Endless.Matchmaking;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001A7 RID: 423
	public class UIModerationFlagDropdown : UIBaseDropdown<ModerationFlag>
	{
		// Token: 0x06000631 RID: 1585 RVA: 0x000200EE File Offset: 0x0001E2EE
		protected override void Start()
		{
			base.Start();
			base.SetOptionsAndValue(EndlessCloudService.AllModerationFlags, Array.Empty<ModerationFlag>(), false);
		}

		// Token: 0x06000632 RID: 1586 RVA: 0x00020107 File Offset: 0x0001E307
		protected override string GetLabelFromOption(int optionIndex)
		{
			return base.Options[optionIndex].NiceName;
		}

		// Token: 0x06000633 RID: 1587 RVA: 0x0002011A File Offset: 0x0001E31A
		protected override Sprite GetIconFromOption(int optionIndex)
		{
			return null;
		}

		// Token: 0x06000634 RID: 1588 RVA: 0x0002011D File Offset: 0x0001E31D
		public override bool OptionShouldBeHidden(int index)
		{
			return !base.Options[index].IsActive;
		}
	}
}
