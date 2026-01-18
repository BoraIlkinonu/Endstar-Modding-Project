using System;
using System.Collections;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Shared.Mobile
{
	// Token: 0x020000FC RID: 252
	public class MobileShakeUnityEvent : MonoBehaviour
	{
		// Token: 0x170000FD RID: 253
		// (get) Token: 0x060005FF RID: 1535 RVA: 0x000191AA File Offset: 0x000173AA
		public UnityEvent OnShake { get; } = new UnityEvent();

		// Token: 0x06000600 RID: 1536 RVA: 0x000191B4 File Offset: 0x000173B4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (!MobileUtility.IsMobile)
			{
				return;
			}
			if (Application.isEditor)
			{
				return;
			}
			if (Accelerometer.current != null)
			{
				InputSystem.EnableDevice(Accelerometer.current);
			}
			if (Accelerometer.current == null)
			{
				return;
			}
			this.lowPassFilterFactor = this.accelerometerUpdateInterval / this.lowPassKernelWidthInSeconds;
			this.shakeDetectionThreshold *= this.shakeDetectionThreshold;
			this.lowPassValue = Accelerometer.current.acceleration.ReadValue();
		}

		// Token: 0x06000601 RID: 1537 RVA: 0x00019240 File Offset: 0x00017440
		private void Update()
		{
			Vector3 vector = Accelerometer.current.acceleration.ReadValue();
			this.lowPassValue = Vector3.Lerp(this.lowPassValue, vector, this.lowPassFilterFactor);
			if ((vector - this.lowPassValue).sqrMagnitude < this.shakeDetectionThreshold)
			{
				return;
			}
			if (this.block)
			{
				return;
			}
			base.StartCoroutine(this.WaitSecondAndUnblock());
			this.OnShake.Invoke();
		}

		// Token: 0x06000602 RID: 1538 RVA: 0x000192B3 File Offset: 0x000174B3
		private IEnumerator WaitSecondAndUnblock()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "WaitSecondAndUnblock", Array.Empty<object>());
			}
			this.block = true;
			yield return new WaitForSeconds(1f);
			this.block = false;
			yield break;
		}

		// Token: 0x04000349 RID: 841
		[SerializeField]
		private float shakeDetectionThreshold = 2f;

		// Token: 0x0400034A RID: 842
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400034B RID: 843
		private readonly float accelerometerUpdateInterval = 0.016666668f;

		// Token: 0x0400034C RID: 844
		private readonly float lowPassKernelWidthInSeconds = 1f;

		// Token: 0x0400034D RID: 845
		private float lowPassFilterFactor;

		// Token: 0x0400034E RID: 846
		private Vector3 lowPassValue;

		// Token: 0x0400034F RID: 847
		private bool block;
	}
}
