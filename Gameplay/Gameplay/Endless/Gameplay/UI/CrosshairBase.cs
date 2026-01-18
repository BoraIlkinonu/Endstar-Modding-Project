using System;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000391 RID: 913
	public abstract class CrosshairBase : MonoBehaviour
	{
		// Token: 0x06001735 RID: 5941 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public virtual void Init(CrosshairSettings settings)
		{
		}

		// Token: 0x06001736 RID: 5942
		public abstract void OnShow();

		// Token: 0x06001737 RID: 5943
		public abstract void OnHide();

		// Token: 0x06001738 RID: 5944 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public virtual void SetHidden()
		{
		}

		// Token: 0x06001739 RID: 5945 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public virtual void ApplySpread(float normalRecoilAmount, float shotStrengthMultiplier, float maxRecoilMultiplier, float recoilSettleMultiplier, float recoilSettleDelay)
		{
		}

		// Token: 0x0600173A RID: 5946 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public virtual void OnMoved(float moveSpeedPercent = 1f)
		{
		}
	}
}
