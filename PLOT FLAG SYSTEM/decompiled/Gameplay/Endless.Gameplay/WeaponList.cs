using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay;

[CreateAssetMenu(menuName = "ScriptableObject/AiWeapons", fileName = "AiWeapons")]
public class WeaponList : ScriptableObject
{
	[SerializeField]
	private List<WeaponData> weaponPairs;

	[SerializeField]
	private GameObject errorObject;

	public GameObject GetWeaponObject(NpcEnum.Weapon weapon, int variant = 0)
	{
		foreach (WeaponData weaponPair in weaponPairs)
		{
			if (weaponPair.Weapon == weapon && weaponPair.Variants.Count > 0)
			{
				GameObject gameObject = ((variant == 0 || variant >= weaponPair.Variants.Count) ? weaponPair.Variants[0] : weaponPair.Variants[variant]);
				if ((bool)gameObject)
				{
					return gameObject;
				}
				break;
			}
		}
		Debug.LogException(new Exception("No GameObject found for the requested weapon"));
		return errorObject;
	}

	public string GetWeaponAttachBone(NpcEnum.Weapon weapon)
	{
		foreach (WeaponData weaponPair in weaponPairs)
		{
			if (weaponPair.Weapon == weapon && weaponPair.Variants.Count > 0)
			{
				return weaponPair.BoneName.Value;
			}
		}
		Debug.LogException(new Exception("No Attach Bone found for the requested weapon"));
		return "";
	}
}
