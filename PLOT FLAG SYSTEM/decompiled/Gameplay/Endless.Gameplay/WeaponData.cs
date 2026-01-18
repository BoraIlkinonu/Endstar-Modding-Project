using System;
using System.Collections.Generic;
using Endless.Shared.SoVariables;
using UnityEngine;

namespace Endless.Gameplay;

[Serializable]
public struct WeaponData
{
	public NpcEnum.Weapon Weapon;

	public StringReference BoneName;

	public List<GameObject> Variants;
}
