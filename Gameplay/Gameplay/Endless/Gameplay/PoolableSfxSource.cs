using System;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020000C4 RID: 196
	public class PoolableSfxSource : MonoBehaviour, IPoolableT
	{
		// Token: 0x1700009A RID: 154
		// (get) Token: 0x060003A8 RID: 936 RVA: 0x000141F9 File Offset: 0x000123F9
		// (set) Token: 0x060003A9 RID: 937 RVA: 0x00014201 File Offset: 0x00012401
		public AudioSource AudioSource { get; private set; }

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x060003AA RID: 938 RVA: 0x0001420A File Offset: 0x0001240A
		// (set) Token: 0x060003AB RID: 939 RVA: 0x00014212 File Offset: 0x00012412
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x060003AC RID: 940 RVA: 0x0001421B File Offset: 0x0001241B
		private void LateUpdate()
		{
			if (this.followTarget)
			{
				base.transform.position = this.followTarget.position;
				return;
			}
			if (this.audioId != -1)
			{
				this.OnSelfDisabled.Invoke(this.audioId);
			}
		}

		// Token: 0x060003AD RID: 941 RVA: 0x0001425B File Offset: 0x0001245B
		public void OnSpawn()
		{
			this.isActive = true;
		}

		// Token: 0x060003AE RID: 942 RVA: 0x00014264 File Offset: 0x00012464
		public void OnDespawn()
		{
			this.isActive = false;
			this.audioId = -1;
			this.followTarget = null;
			this.OnSelfDisabled.RemoveAllListeners();
		}

		// Token: 0x060003AF RID: 943 RVA: 0x00014286 File Offset: 0x00012486
		public void SetAudioIdAndFollowTarget(int newAudioId, Transform transformToAttachTo)
		{
			this.audioId = newAudioId;
			this.followTarget = transformToAttachTo;
		}

		// Token: 0x0400036C RID: 876
		private int audioId = -1;

		// Token: 0x0400036D RID: 877
		private bool isActive;

		// Token: 0x0400036E RID: 878
		private Transform followTarget;

		// Token: 0x0400036F RID: 879
		[NonSerialized]
		public readonly UnityEvent<int> OnSelfDisabled = new UnityEvent<int>();
	}
}
