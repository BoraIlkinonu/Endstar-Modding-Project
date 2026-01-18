using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay.Test;

public class SMBTest : StateMachineBehaviour
{
	public string stateName;

	public Action onTurnCompleted;

	private static int turn90LHash = Animator.StringToHash("Turn Unarmed 90 L");

	private static int turn90RHash = Animator.StringToHash("Turn Unarmed 90 R");

	private static int turn180Hash = Animator.StringToHash("Turn Unarmed 180 R");

	private Dictionary<int, string> stateNames;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		int layerIndex2 = animator.GetLayerIndex("Turns");
		if (layerIndex == layerIndex2)
		{
			string arg = ((stateInfo.shortNameHash == turn90LHash) ? "Turn Left" : ((stateInfo.shortNameHash == turn90RHash) ? "Turn Right" : ((stateInfo.shortNameHash != turn180Hash) ? stateInfo.shortNameHash.ToString() : "Turn 180")));
			Debug.Log($"Enter Turn state {arg} at time {Time.time}.");
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		int layerIndex2 = animator.GetLayerIndex("Turns");
		if (layerIndex == layerIndex2)
		{
			string arg = ((stateInfo.shortNameHash == turn90LHash) ? "Turn Left" : ((stateInfo.shortNameHash == turn90RHash) ? "Turn Right" : ((stateInfo.shortNameHash != turn180Hash) ? stateInfo.shortNameHash.ToString() : "Turn 180")));
			Debug.Log($"Exited Turn state {arg} at time {Time.time}.");
			if (animator.IsInTransition(layerIndex))
			{
				Debug.Log($"Transition is {animator.GetAnimatorTransitionInfo(layerIndex).normalizedTime}");
			}
			if (onTurnCompleted != null)
			{
				Debug.Log("has callback");
			}
			onTurnCompleted?.Invoke();
		}
	}
}
