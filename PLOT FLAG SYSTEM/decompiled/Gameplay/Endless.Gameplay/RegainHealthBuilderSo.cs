using UnityEngine;

namespace Endless.Gameplay;

[CreateAssetMenu(menuName = "ScriptableObject/GoalBuilders/RegainHealthBuilder", fileName = "RegainHealthBuilder")]
public class RegainHealthBuilderSo : GoalBuilderSo
{
	[SerializeField]
	private AnimationCurve priorityCurve;

	protected override float Priority(NpcEntity entity)
	{
		return basePriority * priorityCurve.Evaluate((float)entity.Health / (float)entity.Components.Health.MaxHealth);
	}
}
