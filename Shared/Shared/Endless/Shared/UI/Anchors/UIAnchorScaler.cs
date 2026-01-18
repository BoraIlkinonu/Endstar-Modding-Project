using System;
using UnityEngine;

namespace Endless.Shared.UI.Anchors
{
	// Token: 0x020002A8 RID: 680
	public class UIAnchorScaler : MonoBehaviour
	{
		// Token: 0x17000336 RID: 822
		// (get) Token: 0x060010C7 RID: 4295 RVA: 0x000476DB File Offset: 0x000458DB
		// (set) Token: 0x060010C8 RID: 4296 RVA: 0x000476E3 File Offset: 0x000458E3
		public float SmallestScale
		{
			get
			{
				return this.smallestScale;
			}
			set
			{
				this.smallestScale = value;
			}
		}

		// Token: 0x17000337 RID: 823
		// (get) Token: 0x060010C9 RID: 4297 RVA: 0x000476EC File Offset: 0x000458EC
		// (set) Token: 0x060010CA RID: 4298 RVA: 0x000476F4 File Offset: 0x000458F4
		public float MinDistance
		{
			get
			{
				return this.minDistance;
			}
			set
			{
				this.minDistance = value;
			}
		}

		// Token: 0x17000338 RID: 824
		// (get) Token: 0x060010CB RID: 4299 RVA: 0x000476FD File Offset: 0x000458FD
		// (set) Token: 0x060010CC RID: 4300 RVA: 0x00047705 File Offset: 0x00045905
		public float MaxDistance
		{
			get
			{
				return this.maxDistance;
			}
			set
			{
				this.maxDistance = value;
			}
		}

		// Token: 0x17000339 RID: 825
		// (get) Token: 0x060010CD RID: 4301 RVA: 0x0004770E File Offset: 0x0004590E
		// (set) Token: 0x060010CE RID: 4302 RVA: 0x00047716 File Offset: 0x00045916
		public bool FadeAwayPastMaxDistance
		{
			get
			{
				return this.fadeAwayPastMaxDistance;
			}
			set
			{
				this.fadeAwayPastMaxDistance = value;
			}
		}

		// Token: 0x060010CF RID: 4303 RVA: 0x00047720 File Offset: 0x00045920
		public void UpdateScale(Transform target, Vector3 screenPosition)
		{
			if (!this.targetCamera)
			{
				this.targetCamera = Camera.main;
			}
			if (!target)
			{
				return;
			}
			if (screenPosition.z < 0f)
			{
				return;
			}
			float num = Vector3.Distance(this.targetCamera.transform.position, target.position);
			if (num > this.minDistance)
			{
				float num2 = (num - this.minDistance) / (this.maxDistance - this.minDistance);
				if (this.fadeAwayPastMaxDistance && this.displayAndHideHandler)
				{
					if (num2 >= 1f && this.displayAndHideHandler.IsDisplaying)
					{
						this.displayAndHideHandler.Hide();
					}
					else if (num2 < 1f && !this.displayAndHideHandler.IsDisplaying)
					{
						this.displayAndHideHandler.Display();
					}
				}
				num2 = Mathf.Clamp(num2, 0f, 1f);
				base.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * this.smallestScale, num2);
				return;
			}
			if (base.transform.localScale != Vector3.one)
			{
				base.transform.localScale = Vector3.one;
			}
		}

		// Token: 0x04000A92 RID: 2706
		[SerializeField]
		private float smallestScale = 0.25f;

		// Token: 0x04000A93 RID: 2707
		[SerializeField]
		private float minDistance = 10f;

		// Token: 0x04000A94 RID: 2708
		[SerializeField]
		private float maxDistance = 50f;

		// Token: 0x04000A95 RID: 2709
		[SerializeField]
		private bool fadeAwayPastMaxDistance;

		// Token: 0x04000A96 RID: 2710
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x04000A97 RID: 2711
		private Camera targetCamera;
	}
}
