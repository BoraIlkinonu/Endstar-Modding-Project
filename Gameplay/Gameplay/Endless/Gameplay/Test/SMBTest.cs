using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay.Test
{
	// Token: 0x02000421 RID: 1057
	public class SMBTest : StateMachineBehaviour
	{
		// Token: 0x06001A44 RID: 6724 RVA: 0x00078BCC File Offset: 0x00076DCC
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			int layerIndex2 = animator.GetLayerIndex("Turns");
			if (layerIndex == layerIndex2)
			{
				string text;
				if (stateInfo.shortNameHash == SMBTest.turn90LHash)
				{
					text = "Turn Left";
				}
				else if (stateInfo.shortNameHash == SMBTest.turn90RHash)
				{
					text = "Turn Right";
				}
				else if (stateInfo.shortNameHash == SMBTest.turn180Hash)
				{
					text = "Turn 180";
				}
				else
				{
					text = stateInfo.shortNameHash.ToString();
				}
				Debug.Log(string.Format("Enter Turn state {0} at time {1}.", text, Time.time));
			}
		}

		// Token: 0x06001A45 RID: 6725 RVA: 0x00078C58 File Offset: 0x00076E58
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			int layerIndex2 = animator.GetLayerIndex("Turns");
			if (layerIndex == layerIndex2)
			{
				string text;
				if (stateInfo.shortNameHash == SMBTest.turn90LHash)
				{
					text = "Turn Left";
				}
				else if (stateInfo.shortNameHash == SMBTest.turn90RHash)
				{
					text = "Turn Right";
				}
				else if (stateInfo.shortNameHash == SMBTest.turn180Hash)
				{
					text = "Turn 180";
				}
				else
				{
					text = stateInfo.shortNameHash.ToString();
				}
				Debug.Log(string.Format("Exited Turn state {0} at time {1}.", text, Time.time));
				if (animator.IsInTransition(layerIndex))
				{
					Debug.Log(string.Format("Transition is {0}", animator.GetAnimatorTransitionInfo(layerIndex).normalizedTime));
				}
				if (this.onTurnCompleted != null)
				{
					Debug.Log("has callback");
				}
				Action action = this.onTurnCompleted;
				if (action == null)
				{
					return;
				}
				action();
			}
		}

		// Token: 0x040014F9 RID: 5369
		public string stateName;

		// Token: 0x040014FA RID: 5370
		public Action onTurnCompleted;

		// Token: 0x040014FB RID: 5371
		private static int turn90LHash = Animator.StringToHash("Turn Unarmed 90 L");

		// Token: 0x040014FC RID: 5372
		private static int turn90RHash = Animator.StringToHash("Turn Unarmed 90 R");

		// Token: 0x040014FD RID: 5373
		private static int turn180Hash = Animator.StringToHash("Turn Unarmed 180 R");

		// Token: 0x040014FE RID: 5374
		private Dictionary<int, string> stateNames;
	}
}
