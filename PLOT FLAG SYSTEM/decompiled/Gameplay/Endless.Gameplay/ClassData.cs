using System;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay;

[Serializable]
public struct ClassData
{
	public NpcClass Class;

	public GameObject DefaultWeapon;

	public string WeaponBone;

	public GameObject DefaultEquipment;

	public string EquipmentBone;

	public GoalsList Goals;

	public GameObject AttackComponent;

	public int EquippedItemParameter;
}
