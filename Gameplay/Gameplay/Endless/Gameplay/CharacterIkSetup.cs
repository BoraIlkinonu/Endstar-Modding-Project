using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Endless.Gameplay
{
	// Token: 0x02000087 RID: 135
	public class CharacterIkSetup : MonoBehaviour
	{
		// Token: 0x0600026F RID: 623 RVA: 0x0000D887 File Offset: 0x0000BA87
		private IEnumerator Start()
		{
			GameObject gameObject = global::UnityEngine.Object.Instantiate<GameObject>(this.characterPrefab, this.spawnTransform);
			Transform childByName = this.GetChildByName(this.headBoneName, gameObject.transform);
			this.rig.transform.parent = gameObject.transform;
			this.multiAimConstraint.data.constrainedObject = childByName;
			yield return null;
			this.animator.Rebind();
			yield return null;
			this.rigBuilder.enabled = true;
			yield break;
		}

		// Token: 0x06000270 RID: 624 RVA: 0x0000D898 File Offset: 0x0000BA98
		private Transform GetChildByName(string name, Transform root)
		{
			for (int i = 0; i < root.childCount; i++)
			{
				Transform child = root.GetChild(i);
				if (child.name == name)
				{
					return child;
				}
				Transform childByName = this.GetChildByName(name, child);
				if (childByName != null)
				{
					return childByName;
				}
			}
			return null;
		}

		// Token: 0x06000271 RID: 625 RVA: 0x0000D8E4 File Offset: 0x0000BAE4
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
				List<Transform> allChildrenWithPartialName = this.GetAllChildrenWithPartialName(name, child);
				if (allChildrenWithPartialName.Count > 0)
				{
					list.AddRange(allChildrenWithPartialName);
				}
			}
			return list;
		}

		// Token: 0x0400025A RID: 602
		[SerializeField]
		private GameObject characterPrefab;

		// Token: 0x0400025B RID: 603
		[SerializeField]
		private Transform spawnTransform;

		// Token: 0x0400025C RID: 604
		[SerializeField]
		private Animator animator;

		// Token: 0x0400025D RID: 605
		[SerializeField]
		private string headBoneName;

		// Token: 0x0400025E RID: 606
		[SerializeField]
		private Rig rig;

		// Token: 0x0400025F RID: 607
		[SerializeField]
		private RigBuilder rigBuilder;

		// Token: 0x04000260 RID: 608
		[SerializeField]
		private MultiAimConstraint multiAimConstraint;

		// Token: 0x04000261 RID: 609
		[SerializeField]
		private bool setupBoneRenderer;

		// Token: 0x04000262 RID: 610
		[SerializeField]
		private string boneRendererNameSubstring = "Rig.";

		// Token: 0x04000263 RID: 611
		[SerializeField]
		private BoneRenderer boneRenderer;
	}
}
