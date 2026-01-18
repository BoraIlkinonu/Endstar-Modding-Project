using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI.Anchors
{
	// Token: 0x020002A7 RID: 679
	public class UIAnchorPositioner : UIGameObject
	{
		// Token: 0x17000335 RID: 821
		// (get) Token: 0x060010C0 RID: 4288 RVA: 0x00047514 File Offset: 0x00045714
		// (set) Token: 0x060010C1 RID: 4289 RVA: 0x0004751C File Offset: 0x0004571C
		public Vector3 Offset
		{
			get
			{
				return this.offset;
			}
			set
			{
				this.offset = value;
			}
		}

		// Token: 0x060010C2 RID: 4290 RVA: 0x00047528 File Offset: 0x00045728
		public bool UpdatePosition(Transform target)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdatePosition", new object[] { target.DebugSafeName(true) });
			}
			if (!this.targetCamera)
			{
				this.targetCamera = Camera.main;
			}
			if (!target)
			{
				return false;
			}
			Vector3 vector = target.position + this.offset;
			vector = this.targetCamera.WorldToScreenPoint(vector);
			bool flag = UIAnchorPositioner.IsScreenPositionVisible(vector, 0f, 0f);
			if (this.canvas.enabled != flag)
			{
				this.canvas.enabled = flag;
			}
			base.RectTransform.position = vector;
			return true;
		}

		// Token: 0x060010C3 RID: 4291 RVA: 0x000475D4 File Offset: 0x000457D4
		public static bool IsScreenPositionVisible(Vector3 screenPosition, float horizontalPadding = 0f, float verticalPadding = 0f)
		{
			return screenPosition.z > 0f && screenPosition.x > -horizontalPadding && screenPosition.x < (float)Screen.width + horizontalPadding && screenPosition.y > -verticalPadding && screenPosition.y < (float)Screen.height + verticalPadding;
		}

		// Token: 0x060010C4 RID: 4292 RVA: 0x00047624 File Offset: 0x00045824
		public Vector3 GetScreenPosition(Transform target)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetScreenPosition", new object[] { target.DebugSafeName(true) });
			}
			if (!this.targetCamera)
			{
				this.targetCamera = Camera.main;
			}
			if (!target)
			{
				return Vector3.zero;
			}
			Vector3 vector = target.position + this.offset;
			return this.targetCamera.WorldToScreenPoint(vector);
		}

		// Token: 0x060010C5 RID: 4293 RVA: 0x00047698 File Offset: 0x00045898
		public void SetScreenPosition(Vector3 screenPosition)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetScreenPosition", new object[] { screenPosition });
			}
			base.RectTransform.position = screenPosition;
		}

		// Token: 0x04000A8E RID: 2702
		[SerializeField]
		private Vector3 offset = Vector3.zero;

		// Token: 0x04000A8F RID: 2703
		[SerializeField]
		private Canvas canvas;

		// Token: 0x04000A90 RID: 2704
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000A91 RID: 2705
		private Camera targetCamera;
	}
}
