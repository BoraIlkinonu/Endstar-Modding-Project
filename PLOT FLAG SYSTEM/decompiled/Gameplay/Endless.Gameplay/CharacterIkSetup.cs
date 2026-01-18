using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Endless.Gameplay;

public class CharacterIkSetup : MonoBehaviour
{
	[SerializeField]
	private GameObject characterPrefab;

	[SerializeField]
	private Transform spawnTransform;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private string headBoneName;

	[SerializeField]
	private Rig rig;

	[SerializeField]
	private RigBuilder rigBuilder;

	[SerializeField]
	private MultiAimConstraint multiAimConstraint;

	[SerializeField]
	private bool setupBoneRenderer;

	[SerializeField]
	private string boneRendererNameSubstring = "Rig.";

	[SerializeField]
	private BoneRenderer boneRenderer;

	private IEnumerator Start()
	{
		GameObject gameObject = Object.Instantiate(characterPrefab, spawnTransform);
		Transform childByName = GetChildByName(headBoneName, gameObject.transform);
		rig.transform.parent = gameObject.transform;
		multiAimConstraint.data.constrainedObject = childByName;
		yield return null;
		animator.Rebind();
		yield return null;
		rigBuilder.enabled = true;
	}

	private Transform GetChildByName(string name, Transform root)
	{
		for (int i = 0; i < root.childCount; i++)
		{
			Transform child = root.GetChild(i);
			if (child.name == name)
			{
				return child;
			}
			Transform childByName = GetChildByName(name, child);
			if (childByName != null)
			{
				return childByName;
			}
		}
		return null;
	}

	private List<Transform> GetAllChildrenWithPartialName(string name, Transform root)
	{
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < root.childCount; i++)
		{
			Transform child = root.GetChild(i);
			if (child.name.Contains(name))
			{
				list.Add(child);
			}
			List<Transform> allChildrenWithPartialName = GetAllChildrenWithPartialName(name, child);
			if (allChildrenWithPartialName.Count > 0)
			{
				list.AddRange(allChildrenWithPartialName);
			}
		}
		return list;
	}
}
