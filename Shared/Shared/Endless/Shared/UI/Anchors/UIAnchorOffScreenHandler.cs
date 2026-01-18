using System;
using UnityEngine;

namespace Endless.Shared.UI.Anchors
{
	// Token: 0x020002A6 RID: 678
	public class UIAnchorOffScreenHandler : UIGameObject
	{
		// Token: 0x060010BB RID: 4283 RVA: 0x0004721C File Offset: 0x0004541C
		public static void GetScreenCenterAndBounds(out Vector3 screenCenter, out Vector3 screenBounds)
		{
			screenCenter = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f);
			screenBounds = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f);
		}

		// Token: 0x060010BC RID: 4284 RVA: 0x0004727C File Offset: 0x0004547C
		public void GetScreenCenterAndBoundsWithPadding(out Vector3 screenCenter, out Vector3 screenBounds)
		{
			screenCenter = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f);
			screenBounds = new Vector3((float)Screen.width * 0.5f - this.edgePaddingPixels, (float)Screen.height * 0.5f - this.edgePaddingPixels, 0f);
		}

		// Token: 0x060010BD RID: 4285 RVA: 0x000472E8 File Offset: 0x000454E8
		public Vector3 ProcessOffScreenPosition(Vector3 screenPosition)
		{
			Vector3 vector;
			Vector3 vector2;
			this.GetScreenCenterAndBoundsWithPadding(out vector, out vector2);
			return this.ProcessOffScreenPosition(screenPosition, vector, vector2);
		}

		// Token: 0x060010BE RID: 4286 RVA: 0x00047308 File Offset: 0x00045508
		public Vector3 ProcessOffScreenPosition(Vector3 screenPosition, Vector3 screenCenter, Vector3 screenBounds)
		{
			if (screenPosition.z > 0f && screenPosition.x > 0f && screenPosition.x < (float)Screen.width && screenPosition.y > 0f && screenPosition.y < (float)Screen.height)
			{
				if (this.enableRotation)
				{
					float num = Mathf.MoveTowardsAngle(base.RectTransform.localEulerAngles.z, 0f, this.rotationSpeed * Time.deltaTime);
					base.RectTransform.localEulerAngles = new Vector3(0f, 0f, num);
				}
				else
				{
					base.RectTransform.localEulerAngles = Vector3.zero;
				}
				return screenPosition;
			}
			Vector3 vector = screenPosition - screenCenter;
			if (vector.z < 0f)
			{
				vector *= -1f;
			}
			float num2 = Mathf.Atan2(vector.y, vector.x);
			float num3 = Mathf.Tan(num2);
			vector = ((vector.x > 0f) ? new Vector3(screenBounds.x, screenBounds.x * num3, 0f) : new Vector3(-screenBounds.x, -screenBounds.x * num3, 0f));
			if (vector.y > screenBounds.y)
			{
				vector = new Vector3(screenBounds.y / num3, screenBounds.y, 0f);
			}
			else if (vector.y < -screenBounds.y)
			{
				vector = new Vector3(-screenBounds.y / num3, -screenBounds.y, 0f);
			}
			vector += screenCenter;
			if (this.enableRotation)
			{
				float num4 = num2 * 57.29578f + 90f;
				float num5 = Mathf.MoveTowardsAngle(base.RectTransform.localEulerAngles.z, num4, this.rotationSpeed * Time.deltaTime);
				base.RectTransform.localEulerAngles = new Vector3(0f, 0f, num5);
			}
			return vector;
		}

		// Token: 0x04000A8B RID: 2699
		[SerializeField]
		private bool enableRotation = true;

		// Token: 0x04000A8C RID: 2700
		[Tooltip("Rotation speed in degrees per second")]
		[SerializeField]
		private float rotationSpeed = 360f;

		// Token: 0x04000A8D RID: 2701
		[Tooltip("Padding from screen edges in pixels")]
		[SerializeField]
		private float edgePaddingPixels = 50f;
	}
}
