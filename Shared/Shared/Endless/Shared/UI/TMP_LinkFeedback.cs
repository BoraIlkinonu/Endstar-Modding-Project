using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Endless.Shared.UI
{
	// Token: 0x02000142 RID: 322
	public class TMP_LinkFeedback : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerMoveHandler, IPointerClickHandler
	{
		// Token: 0x060007F7 RID: 2039 RVA: 0x00021A13 File Offset: 0x0001FC13
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.Process(eventData);
		}

		// Token: 0x060007F8 RID: 2040 RVA: 0x00021A13 File Offset: 0x0001FC13
		public void OnPointerMove(PointerEventData eventData)
		{
			this.Process(eventData);
		}

		// Token: 0x060007F9 RID: 2041 RVA: 0x00021A1C File Offset: 0x0001FC1C
		public void OnPointerExit(PointerEventData eventData)
		{
			this.ResetLinkColour();
			this.currentLink = -1;
		}

		// Token: 0x060007FA RID: 2042 RVA: 0x00021A2C File Offset: 0x0001FC2C
		public void OnPointerClick(PointerEventData eventData)
		{
			if (this.currentLink == -1)
			{
				return;
			}
			TMP_LinkInfo tmp_LinkInfo = this.text.textInfo.linkInfo[this.currentLink];
			Application.OpenURL(tmp_LinkInfo.GetLinkID());
		}

		// Token: 0x060007FB RID: 2043 RVA: 0x00021A6C File Offset: 0x0001FC6C
		private void Process(PointerEventData eventData)
		{
			int num = TMP_TextUtilities.FindIntersectingLink(this.text, eventData.position, eventData.pressEventCamera);
			if (num == this.currentLink)
			{
				return;
			}
			this.ResetLinkColour();
			if (num != -1)
			{
				this.SetLinkColour(num, this.hoverColour);
			}
			this.currentLink = num;
		}

		// Token: 0x060007FC RID: 2044 RVA: 0x00021AC3 File Offset: 0x0001FCC3
		private void ResetLinkColour()
		{
			if (this.currentLink != -1)
			{
				this.SetLinkColour(this.currentLink, this.normalColour);
			}
		}

		// Token: 0x060007FD RID: 2045 RVA: 0x00021AE8 File Offset: 0x0001FCE8
		private void SetLinkColour(int linkIndex, Color32 color)
		{
			TMP_LinkInfo tmp_LinkInfo = this.text.textInfo.linkInfo[linkIndex];
			for (int i = 0; i < tmp_LinkInfo.linkTextLength; i++)
			{
				int num = tmp_LinkInfo.linkTextfirstCharacterIndex + i;
				TMP_CharacterInfo tmp_CharacterInfo = this.text.textInfo.characterInfo[num];
				if (tmp_CharacterInfo.isVisible)
				{
					int materialReferenceIndex = tmp_CharacterInfo.materialReferenceIndex;
					int vertexIndex = tmp_CharacterInfo.vertexIndex;
					Color32[] colors = this.text.textInfo.meshInfo[materialReferenceIndex].colors32;
					colors[vertexIndex] = color;
					colors[vertexIndex + 1] = color;
					colors[vertexIndex + 2] = color;
					colors[vertexIndex + 3] = color;
				}
			}
			this.text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
		}

		// Token: 0x040004D1 RID: 1233
		[SerializeField]
		private TextMeshProUGUI text;

		// Token: 0x040004D2 RID: 1234
		[SerializeField]
		private Color hoverColour = Color.cyan;

		// Token: 0x040004D3 RID: 1235
		[SerializeField]
		private Color normalColour = Color.white;

		// Token: 0x040004D4 RID: 1236
		private int currentLink = -1;
	}
}
