using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Endless.Gameplay;

public class CameraTargetsSetter : MonoBehaviour
{
	[Serializable]
	public struct TargetInfo
	{
		public Transform TrackingTarget;

		public Transform LookAtTarget;

		public bool CustomLookAtTarget;

		public static implicit operator CameraTarget(TargetInfo targetInfo)
		{
			return new CameraTarget
			{
				TrackingTarget = targetInfo.TrackingTarget,
				LookAtTarget = targetInfo.LookAtTarget,
				CustomLookAtTarget = targetInfo.CustomLookAtTarget
			};
		}
	}

	[SerializeField]
	private CinemachineCamera cinemachineCamera;

	[SerializeField]
	private bool useInitialInfoOnStart = true;

	[SerializeField]
	private TargetInfo initialTargetInfo;

	private void Start()
	{
		if (useInitialInfoOnStart)
		{
			SetCameraTargets(initialTargetInfo);
		}
	}

	public void SetCameraTargets(CameraTarget cameraTargets)
	{
		cinemachineCamera.Target = cameraTargets;
	}

	public void SetCameraTargets(Transform followTarget, Transform lookAtTarget)
	{
		SetCameraTargets(new CameraTarget
		{
			TrackingTarget = followTarget,
			LookAtTarget = lookAtTarget,
			CustomLookAtTarget = true
		});
	}
}
