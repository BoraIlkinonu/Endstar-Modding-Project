using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay;

[CreateAssetMenu(menuName = "ScriptableObject/ClassDataList", fileName = "ClassDataList")]
public class ClassDataList : ScriptableObject
{
	[SerializeField]
	private List<ClassData> classDataList;

	private readonly Dictionary<NpcClass, ClassData> classDataByClass = new Dictionary<NpcClass, ClassData>();

	private void OnValidate()
	{
		InitializeDictionary();
	}

	public bool TryGetClassData(NpcClass aiClass, out ClassData classData)
	{
		if (classDataList.Count != classDataByClass.Count)
		{
			InitializeDictionary();
		}
		if (classDataByClass.TryGetValue(aiClass, out classData))
		{
			return true;
		}
		Debug.LogWarning($"No ClassData was found associated with {aiClass}");
		return false;
	}

	private void InitializeDictionary()
	{
		classDataByClass.Clear();
		foreach (ClassData classData in classDataList)
		{
			if (!classDataByClass.TryAdd(classData.Class, classData))
			{
				Debug.LogWarning($"Multiple ClassData entries with {classData.Class} class, ignoring all after the first");
			}
		}
	}
}
