using UnityEngine;

namespace Endless.Gameplay;

public static class NpcMovementValues
{
	public static float MaxVerticalVelocity => 5.7f;

	public static float MaxHorizontalVelocity => 3.3f;

	public static float Gravity => 9.81f;

	public static LayerMask JumpSweepMask => LayerMask.GetMask("Default");

	public static float NpcHeight => 1.6f;

	public static float NpcRadius => 0.18f;

	public static float JumpCostScalar => 2f;

	public static float DropCostScalar => 2.5f;
}
