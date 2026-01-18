using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay;

[CreateAssetMenu(menuName = "ScriptableObject/AiEquipment", fileName = "AiEquipment")]
public class EquipmentList : ScriptableObject
{
	[SerializeField]
	private List<EquipmentData> equipmentPairs;

	[SerializeField]
	private GameObject errorObject;

	public GameObject GetEquipmentObject(NpcEnum.Equipment equipment)
	{
		foreach (EquipmentData equipmentPair in equipmentPairs)
		{
			if (equipmentPair.Equipment == equipment && (bool)equipmentPair.GameObject)
			{
				return equipmentPair.GameObject;
			}
		}
		Debug.LogException(new Exception("No GameObject found for the requested equipment"));
		return errorObject;
	}

	public string GetEquipmentAttachBone(NpcEnum.Equipment equipment)
	{
		foreach (EquipmentData equipmentPair in equipmentPairs)
		{
			if (equipmentPair.Equipment == equipment && (bool)equipmentPair.GameObject)
			{
				return equipmentPair.BoneName.Value;
			}
		}
		Debug.LogException(new Exception("No Attach Bone found for the requested weapon"));
		return "";
	}
}
