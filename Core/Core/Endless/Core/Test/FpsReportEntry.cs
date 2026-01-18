using System;
using TMPro;
using UnityEngine;

namespace Endless.Core.Test
{
	// Token: 0x020000D5 RID: 213
	public class FpsReportEntry : MonoBehaviour
	{
		// Token: 0x060004DC RID: 1244 RVA: 0x00017A30 File Offset: 0x00015C30
		public void UpdateDisplay(string[] strings)
		{
			for (int i = 0; i < this.stringDisplays.Length; i++)
			{
				if (i < strings.Length)
				{
					this.stringDisplays[i].SetText(strings[i], true);
				}
				else
				{
					this.stringDisplays[i].SetText(string.Empty, true);
				}
			}
		}

		// Token: 0x04000337 RID: 823
		[SerializeField]
		private TextMeshProUGUI[] stringDisplays;
	}
}
