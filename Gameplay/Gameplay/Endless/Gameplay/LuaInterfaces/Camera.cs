using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000445 RID: 1093
	public class Camera
	{
		// Token: 0x06001B5D RID: 7005 RVA: 0x0007C309 File Offset: 0x0007A509
		internal Camera(CutsceneCamera cutsceneCamera)
		{
			this.cutsceneCamera = cutsceneCamera;
		}

		// Token: 0x06001B5E RID: 7006 RVA: 0x0007C318 File Offset: 0x0007A518
		public void EnterShot(Context context, bool global)
		{
			if (global)
			{
				NetworkBehaviourSingleton<CutsceneManager>.Instance.EnterGlobalCutscene(this.cutsceneCamera);
				return;
			}
			if (context != null)
			{
				if (context.IsPlayer())
				{
					NetworkBehaviourSingleton<CutsceneManager>.Instance.EnterPrivateCutscene(context, this.cutsceneCamera);
					return;
				}
				if (NetworkBehaviourSingleton<CutsceneManager>.Instance.CheckIfManagedCutsceneExists(context))
				{
					NetworkBehaviourSingleton<CutsceneManager>.Instance.EnterLocalCutscene(null, this.cutsceneCamera, context);
				}
			}
		}

		// Token: 0x06001B5F RID: 7007 RVA: 0x0007C378 File Offset: 0x0007A578
		public void ExitShot(Context context, bool global)
		{
			if (global)
			{
				NetworkBehaviourSingleton<CutsceneManager>.Instance.ExitGlobalCutscene(this.cutsceneCamera);
				return;
			}
			if (context != null)
			{
				if (context.IsPlayer())
				{
					NetworkBehaviourSingleton<CutsceneManager>.Instance.ExitPrivateCutscene(context.WorldObject.NetworkObject.OwnerClientId, this.cutsceneCamera);
					return;
				}
				if (NetworkBehaviourSingleton<CutsceneManager>.Instance.CheckIfManagedCutsceneExists(context))
				{
					NetworkBehaviourSingleton<CutsceneManager>.Instance.ExitLocalCutscene(context);
				}
			}
		}

		// Token: 0x06001B60 RID: 7008 RVA: 0x0007C3DD File Offset: 0x0007A5DD
		public void EnterShotManaged(Context context, Context manager)
		{
			NetworkBehaviourSingleton<CutsceneManager>.Instance.EnterLocalCutscene(context, this.cutsceneCamera, manager);
		}

		// Token: 0x06001B61 RID: 7009 RVA: 0x0007C3F1 File Offset: 0x0007A5F1
		public void ExitShotManaged(Context manager)
		{
			NetworkBehaviourSingleton<CutsceneManager>.Instance.ExitLocalCutscene(manager);
		}

		// Token: 0x06001B62 RID: 7010 RVA: 0x0007C3FE File Offset: 0x0007A5FE
		public void SetFollowTarget(Context context, Context followTarget)
		{
			this.cutsceneCamera.SetFollowTarget(followTarget);
		}

		// Token: 0x06001B63 RID: 7011 RVA: 0x0007C40C File Offset: 0x0007A60C
		public void SetFollowOffset(Context context, global::UnityEngine.Vector3 offset)
		{
			this.cutsceneCamera.SetFollowOffset(offset);
		}

		// Token: 0x06001B64 RID: 7012 RVA: 0x0007C41A File Offset: 0x0007A61A
		public void SetLookAtTarget(Context context, Context lookAtTarget)
		{
			this.cutsceneCamera.SetLookAtTarget(lookAtTarget);
		}

		// Token: 0x06001B65 RID: 7013 RVA: 0x0007C428 File Offset: 0x0007A628
		public void SetLookOffset(Context context, global::UnityEngine.Vector3 offset)
		{
			this.cutsceneCamera.SetLookOffset(offset);
		}

		// Token: 0x06001B66 RID: 7014 RVA: 0x0007C436 File Offset: 0x0007A636
		public void SetCameraPitch(Context context, float value)
		{
			this.cutsceneCamera.SetPitch(value);
		}

		// Token: 0x06001B67 RID: 7015 RVA: 0x0007C444 File Offset: 0x0007A644
		public void SetCameraYaw(Context context, float value)
		{
			this.cutsceneCamera.SetYaw(value);
		}

		// Token: 0x06001B68 RID: 7016 RVA: 0x0007C452 File Offset: 0x0007A652
		public void SetFieldOfView(Context context, float value)
		{
			this.cutsceneCamera.SetFieldOfView(value);
		}

		// Token: 0x06001B69 RID: 7017 RVA: 0x0007C460 File Offset: 0x0007A660
		public void SetFollowDampening(Context context, float value)
		{
			this.cutsceneCamera.SetFollowDampening(value);
		}

		// Token: 0x06001B6A RID: 7018 RVA: 0x0007C46E File Offset: 0x0007A66E
		public void SetLookDampening(Context context, float value)
		{
			this.cutsceneCamera.SetLookDampening(value);
		}

		// Token: 0x06001B6B RID: 7019 RVA: 0x0007C47C File Offset: 0x0007A67C
		public void SetDuration(Context context, float value)
		{
			this.cutsceneCamera.SetDuration(value, 0f);
		}

		// Token: 0x06001B6C RID: 7020 RVA: 0x0007C48F File Offset: 0x0007A68F
		public void SetDuration(Context context, float primaryDurationValue, float secondaryDurationValue)
		{
			this.cutsceneCamera.SetDuration(primaryDurationValue, secondaryDurationValue);
		}

		// Token: 0x06001B6D RID: 7021 RVA: 0x0007C49E File Offset: 0x0007A69E
		public void SetMoveToPosition(Context context, global::UnityEngine.Vector3 moveToPosition)
		{
			this.cutsceneCamera.SetMoveToPosition(moveToPosition);
		}

		// Token: 0x06001B6E RID: 7022 RVA: 0x0007C4AC File Offset: 0x0007A6AC
		public void SetMoveToRotation(Context context, float pitch, float yaw)
		{
			this.cutsceneCamera.SetMoveToRotation(pitch, yaw);
		}

		// Token: 0x06001B6F RID: 7023 RVA: 0x0007C4BB File Offset: 0x0007A6BB
		public void SetTransitionIn(Context context, int transitionType, float transitionDuration)
		{
			this.cutsceneCamera.SetTransitionIn((CameraTransition)transitionType, transitionDuration);
		}

		// Token: 0x06001B70 RID: 7024 RVA: 0x0007C4CA File Offset: 0x0007A6CA
		public void SetTransitionOut(Context context, int transitionType, float transitionDuration)
		{
			this.cutsceneCamera.SetTransitionOut((CameraTransition)transitionType, transitionDuration);
		}

		// Token: 0x040015AA RID: 5546
		private CutsceneCamera cutsceneCamera;
	}
}
