using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x02000032 RID: 50
	[ExecuteInEditMode]
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class UITextWobbleEffect : UIGameObject
	{
		// Token: 0x1700003B RID: 59
		// (get) Token: 0x0600014B RID: 331 RVA: 0x00008895 File Offset: 0x00006A95
		private TextMeshProUGUI TextComponent
		{
			get
			{
				if (!this.textComponent)
				{
					base.TryGetComponent<TextMeshProUGUI>(out this.textComponent);
				}
				return this.textComponent;
			}
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x0600014C RID: 332 RVA: 0x00007447 File Offset: 0x00005647
		private float TimeValue
		{
			get
			{
				return Time.time;
			}
		}

		// Token: 0x0600014D RID: 333 RVA: 0x000088B8 File Offset: 0x00006AB8
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			if (this.textWobbleEffect == TextWobbleEffects.Word)
			{
				this.wordIndexes = new List<int> { 0 };
				this.wordLengths.Clear();
				string text = this.TextComponent.text;
				for (int i = text.IndexOf(' '); i > -1; i = text.IndexOf(' ', i + 1))
				{
					this.wordLengths.Add(i - this.wordIndexes[this.wordIndexes.Count - 1]);
					this.wordIndexes.Add(i + 1);
				}
				this.wordLengths.Add(text.Length - this.wordIndexes[this.wordIndexes.Count - 1]);
			}
		}

		// Token: 0x0600014E RID: 334 RVA: 0x0000898C File Offset: 0x00006B8C
		private void Update()
		{
			this.TextComponent.ForceMeshUpdate(false, false);
			this.mesh = this.TextComponent.mesh;
			this.vertices = this.mesh.vertices;
			switch (this.textWobbleEffect)
			{
			case TextWobbleEffects.Vertex:
			{
				for (int i = 0; i < this.vertices.Length; i++)
				{
					Vector3 vector = this.Wobble(this.TimeValue + (float)i);
					this.vertices[i] = this.vertices[i] + vector;
				}
				break;
			}
			case TextWobbleEffects.Character:
			{
				for (int j = 0; j < this.TextComponent.textInfo.characterCount; j++)
				{
					int vertexIndex = this.TextComponent.textInfo.characterInfo[j].vertexIndex;
					Vector3 vector2 = this.Wobble(this.TimeValue + (float)j);
					this.vertices[vertexIndex] += vector2;
					this.vertices[vertexIndex + 1] += vector2;
					this.vertices[vertexIndex + 2] += vector2;
					this.vertices[vertexIndex + 3] += vector2;
				}
				break;
			}
			case TextWobbleEffects.Word:
			{
				Color[] colors = this.mesh.colors;
				for (int k = 0; k < this.wordIndexes.Count; k++)
				{
					int num = this.wordIndexes[k];
					Vector3 vector3 = this.Wobble(this.TimeValue + (float)k);
					for (int l = 0; l < this.wordLengths[k]; l++)
					{
						int vertexIndex2 = this.TextComponent.textInfo.characterInfo[num + l].vertexIndex;
						colors[vertexIndex2] = this.gradient.Evaluate(Mathf.Repeat(this.TimeValue + this.vertices[vertexIndex2].x * 0.001f, 1f));
						colors[vertexIndex2 + 1] = this.gradient.Evaluate(Mathf.Repeat(this.TimeValue + this.vertices[vertexIndex2 + 1].x * 0.001f, 1f));
						colors[vertexIndex2 + 2] = this.gradient.Evaluate(Mathf.Repeat(this.TimeValue + this.vertices[vertexIndex2 + 2].x * 0.001f, 1f));
						colors[vertexIndex2 + 3] = this.gradient.Evaluate(Mathf.Repeat(this.TimeValue + this.vertices[vertexIndex2 + 3].x * 0.001f, 1f));
						this.vertices[vertexIndex2] += vector3;
						this.vertices[vertexIndex2 + 1] += vector3;
						this.vertices[vertexIndex2 + 2] += vector3;
						this.vertices[vertexIndex2 + 3] += vector3;
					}
				}
				this.mesh.colors = colors;
				break;
			}
			default:
				DebugUtility.LogWarning(this, "Update", string.Format("{0} has no support for a {1} value of {2}", "UITextWobbleEffect", "textWobbleEffect", this.textWobbleEffect), Array.Empty<object>());
				break;
			}
			this.mesh.vertices = this.vertices;
			this.TextComponent.canvasRenderer.SetMesh(this.mesh);
		}

		// Token: 0x0600014F RID: 335 RVA: 0x00008D89 File Offset: 0x00006F89
		private Vector2 Wobble(float time)
		{
			return new Vector2(Mathf.Sin(time * this.wobbleX), Mathf.Cos(time * this.wobbleY));
		}

		// Token: 0x040000B2 RID: 178
		[SerializeField]
		private TextWobbleEffects textWobbleEffect;

		// Token: 0x040000B3 RID: 179
		[SerializeField]
		private float wobbleX = 5f;

		// Token: 0x040000B4 RID: 180
		[SerializeField]
		private float wobbleY = 5f;

		// Token: 0x040000B5 RID: 181
		[SerializeField]
		private Gradient gradient = new Gradient();

		// Token: 0x040000B6 RID: 182
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040000B7 RID: 183
		private Mesh mesh;

		// Token: 0x040000B8 RID: 184
		private Vector3[] vertices = new Vector3[0];

		// Token: 0x040000B9 RID: 185
		private List<int> wordIndexes = new List<int>();

		// Token: 0x040000BA RID: 186
		private readonly List<int> wordLengths = new List<int>();

		// Token: 0x040000BB RID: 187
		private TextMeshProUGUI textComponent;
	}
}
