using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay;

[CreateAssetMenu(menuName = "ScriptableObject/AiMeleeAttacks", fileName = "AiMeleeAttacks")]
public class MeleeAttacks : ScriptableObject
{
	[SerializeField]
	public List<GameObject> MeleeAttackPrefabs;

	public MeleeAttackData GetMeleeAttackDataForIndex(int index)
	{
		return MeleeAttackPrefabs[index].GetComponent<MeleeAttackData>();
	}
}
