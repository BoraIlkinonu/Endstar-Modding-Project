using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.LuaInterfaces;

public class Camera
{
	private CutsceneCamera cutsceneCamera;

	internal Camera(CutsceneCamera cutsceneCamera)
	{
		this.cutsceneCamera = cutsceneCamera;
	}

	public void EnterShot(Context context, bool global)
	{
		if (global)
		{
			NetworkBehaviourSingleton<CutsceneManager>.Instance.EnterGlobalCutscene(cutsceneCamera);
		}
		else if (context != null)
		{
			if (context.IsPlayer())
			{
				NetworkBehaviourSingleton<CutsceneManager>.Instance.EnterPrivateCutscene(context, cutsceneCamera);
			}
			else if (NetworkBehaviourSingleton<CutsceneManager>.Instance.CheckIfManagedCutsceneExists(context))
			{
				NetworkBehaviourSingleton<CutsceneManager>.Instance.EnterLocalCutscene(null, cutsceneCamera, context);
			}
		}
	}

	public void ExitShot(Context context, bool global)
	{
		if (global)
		{
			NetworkBehaviourSingleton<CutsceneManager>.Instance.ExitGlobalCutscene(cutsceneCamera);
		}
		else if (context != null)
		{
			if (context.IsPlayer())
			{
				NetworkBehaviourSingleton<CutsceneManager>.Instance.ExitPrivateCutscene(context.WorldObject.NetworkObject.OwnerClientId, cutsceneCamera);
			}
			else if (NetworkBehaviourSingleton<CutsceneManager>.Instance.CheckIfManagedCutsceneExists(context))
			{
				NetworkBehaviourSingleton<CutsceneManager>.Instance.ExitLocalCutscene(context);
			}
		}
	}

	public void EnterShotManaged(Context context, Context manager)
	{
		NetworkBehaviourSingleton<CutsceneManager>.Instance.EnterLocalCutscene(context, cutsceneCamera, manager);
	}

	public void ExitShotManaged(Context manager)
	{
		NetworkBehaviourSingleton<CutsceneManager>.Instance.ExitLocalCutscene(manager);
	}

	public void SetFollowTarget(Context context, Context followTarget)
	{
		cutsceneCamera.SetFollowTarget(followTarget);
	}

	public void SetFollowOffset(Context context, UnityEngine.Vector3 offset)
	{
		cutsceneCamera.SetFollowOffset(offset);
	}

	public void SetLookAtTarget(Context context, Context lookAtTarget)
	{
		cutsceneCamera.SetLookAtTarget(lookAtTarget);
	}

	public void SetLookOffset(Context context, UnityEngine.Vector3 offset)
	{
		cutsceneCamera.SetLookOffset(offset);
	}

	public void SetCameraPitch(Context context, float value)
	{
		cutsceneCamera.SetPitch(value);
	}

	public void SetCameraYaw(Context context, float value)
	{
		cutsceneCamera.SetYaw(value);
	}

	public void SetFieldOfView(Context context, float value)
	{
		cutsceneCamera.SetFieldOfView(value);
	}

	public void SetFollowDampening(Context context, float value)
	{
		cutsceneCamera.SetFollowDampening(value);
	}

	public void SetLookDampening(Context context, float value)
	{
		cutsceneCamera.SetLookDampening(value);
	}

	public void SetDuration(Context context, float value)
	{
		cutsceneCamera.SetDuration(value);
	}

	public void SetDuration(Context context, float primaryDurationValue, float secondaryDurationValue)
	{
		cutsceneCamera.SetDuration(primaryDurationValue, secondaryDurationValue);
	}

	public void SetMoveToPosition(Context context, UnityEngine.Vector3 moveToPosition)
	{
		cutsceneCamera.SetMoveToPosition(moveToPosition);
	}

	public void SetMoveToRotation(Context context, float pitch, float yaw)
	{
		cutsceneCamera.SetMoveToRotation(pitch, yaw);
	}

	public void SetTransitionIn(Context context, int transitionType, float transitionDuration)
	{
		cutsceneCamera.SetTransitionIn((CameraTransition)transitionType, transitionDuration);
	}

	public void SetTransitionOut(Context context, int transitionType, float transitionDuration)
	{
		cutsceneCamera.SetTransitionOut((CameraTransition)transitionType, transitionDuration);
	}
}
