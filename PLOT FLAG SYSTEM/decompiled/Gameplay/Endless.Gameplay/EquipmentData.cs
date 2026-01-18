using System;
using Endless.Shared.SoVariables;
using UnityEngine;

namespace Endless.Gameplay;

[Serializable]
public struct EquipmentData
{
	public NpcEnum.Equipment Equipment;

	public GameObject GameObject;

	public StringReference BoneName;
}
