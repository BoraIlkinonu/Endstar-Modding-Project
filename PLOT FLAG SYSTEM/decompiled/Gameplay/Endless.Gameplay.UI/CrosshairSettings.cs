using System;
using UnityEngine;

namespace Endless.Gameplay.UI;

[Serializable]
public struct CrosshairSettings
{
	public float maxSpread;

	public float resetSpeed;

	public float weaponStrength;

	public float weaponAccuracy;

	public float movementPenalty;

	public AnimationCurve recoilSettleCurve;
}
