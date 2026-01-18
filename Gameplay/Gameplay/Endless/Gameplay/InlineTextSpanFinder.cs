using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200036B RID: 875
	public class InlineTextSpanFinder : MonoBehaviour
	{
		// Token: 0x170004BB RID: 1211
		// (get) Token: 0x06001677 RID: 5751 RVA: 0x000698FC File Offset: 0x00067AFC
		// (set) Token: 0x06001678 RID: 5752 RVA: 0x00069904 File Offset: 0x00067B04
		public RectTransform TargetRectTransform { get; private set; }

		// Token: 0x1400002C RID: 44
		// (add) Token: 0x06001679 RID: 5753 RVA: 0x00069910 File Offset: 0x00067B10
		// (remove) Token: 0x0600167A RID: 5754 RVA: 0x00069948 File Offset: 0x00067B48
		public event Action<IReadOnlyList<InlineSpan>, TMP_Text> SpansReady;

		// Token: 0x0600167B RID: 5755 RVA: 0x0006997D File Offset: 0x00067B7D
		private void Awake()
		{
			if (this.textComponent != null)
			{
				this.textComponent.OnPreRenderText += this.OnPreRenderCollect;
				this.TargetRectTransform = this.textComponent.rectTransform;
			}
		}

		// Token: 0x0600167C RID: 5756 RVA: 0x000699B5 File Offset: 0x00067BB5
		public void RegisterTextComponent(TMP_Text newTextComponent)
		{
			this.textComponent = newTextComponent;
			this.textComponent.OnPreRenderText += this.OnPreRenderCollect;
			this.TargetRectTransform = this.textComponent.rectTransform;
		}

		// Token: 0x0600167D RID: 5757 RVA: 0x000699E8 File Offset: 0x00067BE8
		private void OnPreRenderCollect(TMP_TextInfo textInfo)
		{
			this.spans.Clear();
			for (int i = 0; i < textInfo.linkCount; i++)
			{
				TMP_LinkInfo tmp_LinkInfo = textInfo.linkInfo[i];
				string linkID = tmp_LinkInfo.GetLinkID();
				if (!string.IsNullOrEmpty(linkID))
				{
					int num = linkID.IndexOf(':');
					if (num > 0)
					{
						string text = linkID.Substring(0, num);
						string text2 = linkID.Substring(num + 1);
						if (this.allowedSchemes.Length == 0 || Array.IndexOf<string>(this.allowedSchemes, text) >= 0)
						{
							int linkTextfirstCharacterIndex = tmp_LinkInfo.linkTextfirstCharacterIndex;
							int num2 = ((tmp_LinkInfo.linkTextLength > 0) ? (linkTextfirstCharacterIndex + tmp_LinkInfo.linkTextLength - 1) : Mathf.Clamp(linkTextfirstCharacterIndex, 0, textInfo.characterCount - 1));
							if (textInfo.characterCount != 0 && linkTextfirstCharacterIndex >= 0 && linkTextfirstCharacterIndex < textInfo.characterCount)
							{
								Vector3 vector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, 0f);
								Vector3 vector2 = new Vector3(float.NegativeInfinity, float.NegativeInfinity, 0f);
								int num3 = linkTextfirstCharacterIndex;
								while (num3 <= num2 && num3 < textInfo.characterCount)
								{
									TMP_CharacterInfo tmp_CharacterInfo = textInfo.characterInfo[num3];
									if (tmp_CharacterInfo.isVisible)
									{
										if (tmp_CharacterInfo.bottomLeft.x < vector.x)
										{
											vector.x = tmp_CharacterInfo.bottomLeft.x;
										}
										if (tmp_CharacterInfo.bottomLeft.y < vector.y)
										{
											vector.y = tmp_CharacterInfo.bottomLeft.y;
										}
										if (tmp_CharacterInfo.topRight.x > vector2.x)
										{
											vector2.x = tmp_CharacterInfo.topRight.x;
										}
										if (tmp_CharacterInfo.topRight.y > vector2.y)
										{
											vector2.y = tmp_CharacterInfo.topRight.y;
										}
									}
									num3++;
								}
								if (!float.IsFinite(vector.x))
								{
									TMP_CharacterInfo tmp_CharacterInfo2 = textInfo.characterInfo[Mathf.Clamp(linkTextfirstCharacterIndex, 0, textInfo.characterCount - 1)];
									float num4 = this.textComponent.fontSize * 0.9f * this.sizeScale;
									Vector3 vector3 = new Vector3(tmp_CharacterInfo2.origin, tmp_CharacterInfo2.baseLine, 0f);
									vector = vector3 + new Vector3(0f, -num4 * 0.25f, 0f);
									vector2 = vector3 + new Vector3(num4, num4 * 0.75f, 0f);
								}
								Vector2 vector4 = (vector + vector2) * 0.5f;
								float num5 = vector2.y - vector.y;
								if (!float.IsFinite(num5) || num5 <= 0.0001f)
								{
									num5 = (this.textComponent.font.faceInfo.capLine - this.textComponent.font.faceInfo.baseline) * (this.textComponent.fontSize / (float)this.textComponent.font.faceInfo.pointSize);
								}
								float num6 = num5 * this.sizeScale;
								this.spans.Add(new InlineSpan
								{
									scheme = text,
									key = text2,
									linkIndex = i,
									firstCharIndex = linkTextfirstCharacterIndex,
									lastCharIndex = num2,
									center = vector4 + this.pixelNudge,
									sideLength = num6
								});
							}
						}
					}
				}
			}
			this.dirtyFromPreRender = true;
		}

		// Token: 0x0600167E RID: 5758 RVA: 0x00069D75 File Offset: 0x00067F75
		private void LateUpdate()
		{
			if (!this.dirtyFromPreRender)
			{
				return;
			}
			this.dirtyFromPreRender = false;
			Action<IReadOnlyList<InlineSpan>, TMP_Text> spansReady = this.SpansReady;
			if (spansReady == null)
			{
				return;
			}
			spansReady(this.spans, this.textComponent);
		}

		// Token: 0x04001224 RID: 4644
		[SerializeField]
		private TMP_Text textComponent;

		// Token: 0x04001225 RID: 4645
		[SerializeField]
		private float sizeScale = 1f;

		// Token: 0x04001226 RID: 4646
		[SerializeField]
		private Vector2 pixelNudge = Vector2.zero;

		// Token: 0x04001228 RID: 4648
		[SerializeField]
		private string[] allowedSchemes = Array.Empty<string>();

		// Token: 0x04001229 RID: 4649
		private readonly List<InlineSpan> spans = new List<InlineSpan>();

		// Token: 0x0400122A RID: 4650
		private bool dirtyFromPreRender;
	}
}
