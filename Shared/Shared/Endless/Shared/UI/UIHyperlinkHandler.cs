using System;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Endless.Shared.UI
{
	// Token: 0x02000143 RID: 323
	public class UIHyperlinkHandler : UIGameObject, IPointerClickHandler, IEventSystemHandler
	{
		// Token: 0x060007FF RID: 2047 RVA: 0x00021BD4 File Offset: 0x0001FDD4
		public void OnPointerClick(PointerEventData pointerEventData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerClick", new object[] { pointerEventData });
			}
			int num = TMP_TextUtilities.FindNearestLink(this.text, pointerEventData.position, Camera.main);
			if (num == -1)
			{
				DebugUtility.LogError("Could not find link in " + this.text.text + "!", this);
				return;
			}
			TMP_LinkInfo tmp_LinkInfo = this.text.textInfo.linkInfo[num];
			Application.OpenURL(tmp_LinkInfo.GetLinkID());
		}

		// Token: 0x040004D5 RID: 1237
		[SerializeField]
		private TextMeshProUGUI text;

		// Token: 0x040004D6 RID: 1238
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
