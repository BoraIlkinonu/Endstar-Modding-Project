using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay;

[CreateAssetMenu(menuName = "ScriptableObject/ComboAttack", fileName = "NewComboAttack")]
public class ComboAttack : ScriptableObject
{
	public float Cost;

	public List<ComboStep> ComboSteps;
}
