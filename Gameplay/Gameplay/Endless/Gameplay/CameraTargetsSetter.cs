using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200007F RID: 127
	public class CameraTargetsSetter : MonoBehaviour
	{
		// Token: 0x0600024A RID: 586 RVA: 0x0000CF21 File Offset: 0x0000B121
		private void Start()
		{
			if (this.useInitialInfoOnStart)
			{
				this.SetCameraTargets(this.initialTargetInfo);
			}
		}

		// Token: 0x0600024B RID: 587 RVA: 0x0000CF3C File Offset: 0x0000B13C
		public void SetCameraTargets(CameraTarget cameraTargets)
		{
			this.cinemachineCamera.Target = cameraTargets;
		}

		// Token: 0x0600024C RID: 588 RVA: 0x0000CF4C File Offset: 0x0000B14C
		public void SetCameraTargets(Transform followTarget, Transform lookAtTarget)
		{
			this.SetCameraTargets(new CameraTarget
			{
				TrackingTarget = followTarget,
				LookAtTarget = lookAtTarget,
				CustomLookAtTarget = true
			});
		}

		// Token: 0x0400023C RID: 572
		[SerializeField]
		private CinemachineCamera cinemachineCamera;

		// Token: 0x0400023D RID: 573
		[SerializeField]
		private bool useInitialInfoOnStart = true;

		// Token: 0x0400023E RID: 574
		[SerializeField]
		private CameraTargetsSetter.TargetInfo initialTargetInfo;

		// Token: 0x02000080 RID: 128
		[Serializable]
		public struct TargetInfo
		{
			// Token: 0x0600024E RID: 590 RVA: 0x0000CF90 File Offset: 0x0000B190
			public static implicit operator CameraTarget(CameraTargetsSetter.TargetInfo targetInfo)
			{
				return new CameraTarget
				{
					TrackingTarget = targetInfo.TrackingTarget,
					LookAtTarget = targetInfo.LookAtTarget,
					CustomLookAtTarget = targetInfo.CustomLookAtTarget
				};
			}

			// Token: 0x0400023F RID: 575
			public Transform TrackingTarget;

			// Token: 0x04000240 RID: 576
			public Transform LookAtTarget;

			// Token: 0x04000241 RID: 577
			public bool CustomLookAtTarget;
		}
	}
}
