using System;
using UnityEngine;

namespace Endless.Gameplay;

[Serializable]
public struct ComboStep
{
	public GameObject MeleeAttackPrefab;

	public int MeleeAttackIndex;

	public uint pauseFramesAfterAttack;

	public MeleeAttackData MeleeAttackData => MeleeAttackPrefab.GetComponent<MeleeAttackData>();
}
