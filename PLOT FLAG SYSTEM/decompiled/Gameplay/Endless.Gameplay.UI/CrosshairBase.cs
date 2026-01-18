using UnityEngine;

namespace Endless.Gameplay.UI;

public abstract class CrosshairBase : MonoBehaviour
{
	public virtual void Init(CrosshairSettings settings)
	{
	}

	public abstract void OnShow();

	public abstract void OnHide();

	public virtual void SetHidden()
	{
	}

	public virtual void ApplySpread(float normalRecoilAmount, float shotStrengthMultiplier, float maxRecoilMultiplier, float recoilSettleMultiplier, float recoilSettleDelay)
	{
	}

	public virtual void OnMoved(float moveSpeedPercent = 1f)
	{
	}
}
