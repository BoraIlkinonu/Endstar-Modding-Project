using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay;

public class InlineTextSpanFinder : MonoBehaviour
{
	[SerializeField]
	private TMP_Text textComponent;

	[SerializeField]
	private float sizeScale = 1f;

	[SerializeField]
	private Vector2 pixelNudge = Vector2.zero;

	[SerializeField]
	private string[] allowedSchemes = Array.Empty<string>();

	private readonly List<InlineSpan> spans = new List<InlineSpan>();

	private bool dirtyFromPreRender;

	public RectTransform TargetRectTransform { get; private set; }

	public event Action<IReadOnlyList<InlineSpan>, TMP_Text> SpansReady;

	private void Awake()
	{
		if (textComponent != null)
		{
			textComponent.OnPreRenderText += OnPreRenderCollect;
			TargetRectTransform = textComponent.rectTransform;
		}
	}

	public void RegisterTextComponent(TMP_Text newTextComponent)
	{
		textComponent = newTextComponent;
		textComponent.OnPreRenderText += OnPreRenderCollect;
		TargetRectTransform = textComponent.rectTransform;
	}

	private void OnPreRenderCollect(TMP_TextInfo textInfo)
	{
		spans.Clear();
		for (int i = 0; i < textInfo.linkCount; i++)
		{
			TMP_LinkInfo tMP_LinkInfo = textInfo.linkInfo[i];
			string linkID = tMP_LinkInfo.GetLinkID();
			if (string.IsNullOrEmpty(linkID))
			{
				continue;
			}
			int num = linkID.IndexOf(':');
			if (num <= 0)
			{
				continue;
			}
			string text = linkID.Substring(0, num);
			string key = linkID.Substring(num + 1);
			if (allowedSchemes.Length != 0 && Array.IndexOf(allowedSchemes, text) < 0)
			{
				continue;
			}
			int linkTextfirstCharacterIndex = tMP_LinkInfo.linkTextfirstCharacterIndex;
			int num2 = ((tMP_LinkInfo.linkTextLength > 0) ? (linkTextfirstCharacterIndex + tMP_LinkInfo.linkTextLength - 1) : Mathf.Clamp(linkTextfirstCharacterIndex, 0, textInfo.characterCount - 1));
			if (textInfo.characterCount == 0 || linkTextfirstCharacterIndex < 0 || linkTextfirstCharacterIndex >= textInfo.characterCount)
			{
				continue;
			}
			Vector3 vector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, 0f);
			Vector3 vector2 = new Vector3(float.NegativeInfinity, float.NegativeInfinity, 0f);
			for (int j = linkTextfirstCharacterIndex; j <= num2 && j < textInfo.characterCount; j++)
			{
				TMP_CharacterInfo tMP_CharacterInfo = textInfo.characterInfo[j];
				if (tMP_CharacterInfo.isVisible)
				{
					if (tMP_CharacterInfo.bottomLeft.x < vector.x)
					{
						vector.x = tMP_CharacterInfo.bottomLeft.x;
					}
					if (tMP_CharacterInfo.bottomLeft.y < vector.y)
					{
						vector.y = tMP_CharacterInfo.bottomLeft.y;
					}
					if (tMP_CharacterInfo.topRight.x > vector2.x)
					{
						vector2.x = tMP_CharacterInfo.topRight.x;
					}
					if (tMP_CharacterInfo.topRight.y > vector2.y)
					{
						vector2.y = tMP_CharacterInfo.topRight.y;
					}
				}
			}
			if (!float.IsFinite(vector.x))
			{
				TMP_CharacterInfo tMP_CharacterInfo2 = textInfo.characterInfo[Mathf.Clamp(linkTextfirstCharacterIndex, 0, textInfo.characterCount - 1)];
				float num3 = textComponent.fontSize * 0.9f * sizeScale;
				Vector3 vector3 = new Vector3(tMP_CharacterInfo2.origin, tMP_CharacterInfo2.baseLine, 0f);
				vector = vector3 + new Vector3(0f, (0f - num3) * 0.25f, 0f);
				vector2 = vector3 + new Vector3(num3, num3 * 0.75f, 0f);
			}
			Vector2 vector4 = (vector + vector2) * 0.5f;
			float num4 = vector2.y - vector.y;
			if (!float.IsFinite(num4) || num4 <= 0.0001f)
			{
				num4 = (textComponent.font.faceInfo.capLine - textComponent.font.faceInfo.baseline) * (textComponent.fontSize / (float)textComponent.font.faceInfo.pointSize);
			}
			float sideLength = num4 * sizeScale;
			spans.Add(new InlineSpan
			{
				scheme = text,
				key = key,
				linkIndex = i,
				firstCharIndex = linkTextfirstCharacterIndex,
				lastCharIndex = num2,
				center = vector4 + pixelNudge,
				sideLength = sideLength
			});
		}
		dirtyFromPreRender = true;
	}

	private void LateUpdate()
	{
		if (dirtyFromPreRender)
		{
			dirtyFromPreRender = false;
			this.SpansReady?.Invoke(spans, textComponent);
		}
	}
}
