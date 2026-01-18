using System;
using Endless.Shared;
using Endless.Shared.Tweens;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002D2 RID: 722
	public class UISelfPoolingTween : MonoBehaviour, IPoolableT
	{
		// Token: 0x17000336 RID: 822
		// (get) Token: 0x0600105C RID: 4188 RVA: 0x00052E4C File Offset: 0x0005104C
		// (set) Token: 0x0600105D RID: 4189 RVA: 0x00052E54 File Offset: 0x00051054
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x17000337 RID: 823
		// (get) Token: 0x0600105E RID: 4190 RVA: 0x00017586 File Offset: 0x00015786
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600105F RID: 4191 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void OnSpawn()
		{
		}

		// Token: 0x06001060 RID: 4192 RVA: 0x00052E5D File Offset: 0x0005105D
		public void Tween()
		{
			this.tweenCollection.Tween(new Action(this.HandleFinished));
		}

		// Token: 0x06001061 RID: 4193 RVA: 0x00052E76 File Offset: 0x00051076
		public void OnDespawn()
		{
			this.tweenCollection.SetToStart();
		}

		// Token: 0x06001062 RID: 4194 RVA: 0x00052E83 File Offset: 0x00051083
		private void HandleFinished()
		{
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UISelfPoolingTween>(this);
		}

		// Token: 0x04000E0D RID: 3597
		[SerializeField]
		private TweenCollection tweenCollection;
	}
}
