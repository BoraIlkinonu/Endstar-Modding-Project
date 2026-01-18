using System;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x02000085 RID: 133
	public class RendererBoundsVisualizer : MonoBehaviour
	{
		// Token: 0x060003D9 RID: 985 RVA: 0x00010EFC File Offset: 0x0000F0FC
		private void OnValidate()
		{
			if (this.targetRenderer == null)
			{
				this.targetRenderer = base.GetComponent<Renderer>();
			}
		}

		// Token: 0x060003DA RID: 986 RVA: 0x00010EFC File Offset: 0x0000F0FC
		private void Reset()
		{
			if (this.targetRenderer == null)
			{
				this.targetRenderer = base.GetComponent<Renderer>();
			}
		}

		// Token: 0x060003DB RID: 987 RVA: 0x00010F18 File Offset: 0x0000F118
		private void OnDrawGizmosSelected()
		{
			if (this.targetRenderer != null)
			{
				this.DrawBounds(this.targetRenderer.bounds, 0f);
			}
		}

		// Token: 0x060003DC RID: 988 RVA: 0x00010F40 File Offset: 0x0000F140
		private void DrawBounds(Bounds b, float delay = 0f)
		{
			Vector3 vector = new Vector3(b.min.x, b.min.y, b.min.z);
			Vector3 vector2 = new Vector3(b.max.x, b.min.y, b.min.z);
			Vector3 vector3 = new Vector3(b.max.x, b.min.y, b.max.z);
			Vector3 vector4 = new Vector3(b.min.x, b.min.y, b.max.z);
			Debug.DrawLine(vector, vector2, Color.blue, delay);
			Debug.DrawLine(vector2, vector3, Color.red, delay);
			Debug.DrawLine(vector3, vector4, Color.yellow, delay);
			Debug.DrawLine(vector4, vector, Color.magenta, delay);
			Vector3 vector5 = new Vector3(b.min.x, b.max.y, b.min.z);
			Vector3 vector6 = new Vector3(b.max.x, b.max.y, b.min.z);
			Vector3 vector7 = new Vector3(b.max.x, b.max.y, b.max.z);
			Vector3 vector8 = new Vector3(b.min.x, b.max.y, b.max.z);
			Debug.DrawLine(vector5, vector6, Color.blue, delay);
			Debug.DrawLine(vector6, vector7, Color.red, delay);
			Debug.DrawLine(vector7, vector8, Color.yellow, delay);
			Debug.DrawLine(vector8, vector5, Color.magenta, delay);
			Debug.DrawLine(vector, vector5, Color.white, delay);
			Debug.DrawLine(vector2, vector6, Color.gray, delay);
			Debug.DrawLine(vector3, vector7, Color.green, delay);
			Debug.DrawLine(vector4, vector8, Color.cyan, delay);
		}

		// Token: 0x040001DB RID: 475
		[SerializeField]
		private Renderer targetRenderer;
	}
}
