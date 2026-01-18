using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000242 RID: 578
	[CreateAssetMenu(menuName = "ScriptableObject/AiWeapons", fileName = "AiWeapons")]
	public class WeaponList : ScriptableObject
	{
		// Token: 0x06000BF0 RID: 3056 RVA: 0x00040E94 File Offset: 0x0003F094
		public GameObject GetWeaponObject(NpcEnum.Weapon weapon, int variant = 0)
		{
			foreach (WeaponData weaponData in this.weaponPairs)
			{
				if (weaponData.Weapon == weapon && weaponData.Variants.Count > 0)
				{
					GameObject gameObject = ((variant == 0 || variant >= weaponData.Variants.Count) ? weaponData.Variants[0] : weaponData.Variants[variant]);
					if (gameObject)
					{
						return gameObject;
					}
					break;
				}
			}
			Debug.LogException(new Exception("No GameObject found for the requested weapon"));
			return this.errorObject;
		}

		// Token: 0x06000BF1 RID: 3057 RVA: 0x00040F48 File Offset: 0x0003F148
		public string GetWeaponAttachBone(NpcEnum.Weapon weapon)
		{
			foreach (WeaponData weaponData in this.weaponPairs)
			{
				if (weaponData.Weapon == weapon && weaponData.Variants.Count > 0)
				{
					return weaponData.BoneName.Value;
				}
			}
			Debug.LogException(new Exception("No Attach Bone found for the requested weapon"));
			return "";
		}

		// Token: 0x04000B18 RID: 2840
		[SerializeField]
		private List<WeaponData> weaponPairs;

		// Token: 0x04000B19 RID: 2841
		[SerializeField]
		private GameObject errorObject;
	}
}
