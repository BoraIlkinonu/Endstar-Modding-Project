using UnityEngine;

namespace Endless.Gameplay;

public class PlayerStunComponent : MonoBehaviour
{
	private uint incomingStunThisFrame;

	public void ProcessStun(uint frame, ref NetState currentState)
	{
		if (currentState.StunFrame != 0)
		{
			currentState.StunFrame = (uint)Mathf.Max(currentState.StunFrame - 1, 0f);
		}
	}

	public void ApplyStun(uint frame, uint stunLengthFrames)
	{
		incomingStunThisFrame = (uint)Mathf.Max(stunLengthFrames, incomingStunThisFrame);
	}

	public void EndFrame(ref NetState currentState)
	{
		currentState.StunFrame = (uint)Mathf.Max(currentState.StunFrame, incomingStunThisFrame);
		incomingStunThisFrame = 0u;
	}
}
