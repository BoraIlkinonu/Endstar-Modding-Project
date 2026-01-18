using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200023E RID: 574
	[CreateAssetMenu(menuName = "ScriptableObject/AiEquipment", fileName = "AiEquipment")]
	public class EquipmentList : ScriptableObject
	{
		// Token: 0x06000BCF RID: 3023 RVA: 0x00040C80 File Offset: 0x0003EE80
		public GameObject GetEquipmentObject(NpcEnum.Equipment equipment)
		{
			foreach (EquipmentData equipmentData in this.equipmentPairs)
			{
				if (equipmentData.Equipment == equipment && equipmentData.GameObject)
				{
					return equipmentData.GameObject;
				}
			}
			Debug.LogException(new Exception("No GameObject found for the requested equipment"));
			return this.errorObject;
		}

		// Token: 0x06000BD0 RID: 3024 RVA: 0x00040D04 File Offset: 0x0003EF04
		public string GetEquipmentAttachBone(NpcEnum.Equipment equipment)
		{
			foreach (EquipmentData equipmentData in this.equipmentPairs)
			{
				if (equipmentData.Equipment == equipment && equipmentData.GameObject)
				{
					return equipmentData.BoneName.Value;
				}
			}
			Debug.LogException(new Exception("No Attach Bone found for the requested weapon"));
			return "";
		}

		// Token: 0x04000B05 RID: 2821
		[SerializeField]
		private List<EquipmentData> equipmentPairs;

		// Token: 0x04000B06 RID: 2822
		[SerializeField]
		private GameObject errorObject;
	}
}
