using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200023A RID: 570
	[CreateAssetMenu(menuName = "ScriptableObject/ClassDataList", fileName = "ClassDataList")]
	public class ClassDataList : ScriptableObject
	{
		// Token: 0x06000BC7 RID: 3015 RVA: 0x00040A8C File Offset: 0x0003EC8C
		private void OnValidate()
		{
			this.InitializeDictionary();
		}

		// Token: 0x06000BC8 RID: 3016 RVA: 0x00040A94 File Offset: 0x0003EC94
		public bool TryGetClassData(NpcClass aiClass, out ClassData classData)
		{
			if (this.classDataList.Count != this.classDataByClass.Count)
			{
				this.InitializeDictionary();
			}
			if (this.classDataByClass.TryGetValue(aiClass, out classData))
			{
				return true;
			}
			Debug.LogWarning(string.Format("No ClassData was found associated with {0}", aiClass));
			return false;
		}

		// Token: 0x06000BC9 RID: 3017 RVA: 0x00040AE8 File Offset: 0x0003ECE8
		private void InitializeDictionary()
		{
			this.classDataByClass.Clear();
			foreach (ClassData classData in this.classDataList)
			{
				if (!this.classDataByClass.TryAdd(classData.Class, classData))
				{
					Debug.LogWarning(string.Format("Multiple ClassData entries with {0} class, ignoring all after the first", classData.Class));
				}
			}
		}

		// Token: 0x04000AFD RID: 2813
		[SerializeField]
		private List<ClassData> classDataList;

		// Token: 0x04000AFE RID: 2814
		private readonly Dictionary<NpcClass, ClassData> classDataByClass = new Dictionary<NpcClass, ClassData>();
	}
}
