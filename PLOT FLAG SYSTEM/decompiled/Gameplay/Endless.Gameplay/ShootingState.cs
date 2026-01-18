namespace Endless.Gameplay;

public struct ShootingState : IFrameInfo
{
	private uint netFrame;

	public uint finishedFrame;

	public uint lastShotFrame;

	public bool cameraLocked;

	public bool commitToShot;

	public bool shotReleased;

	public bool waitingForShot;

	public CameraController.CameraType aimState;

	public uint NetFrame
	{
		get
		{
			return netFrame;
		}
		set
		{
			if (value > netFrame)
			{
				netFrame = value;
			}
		}
	}

	public void Clear()
	{
		netFrame = 0u;
		finishedFrame = 0u;
		lastShotFrame = 0u;
		cameraLocked = false;
		commitToShot = false;
		shotReleased = true;
		waitingForShot = false;
		aimState = CameraController.CameraType.Normal;
	}

	public void Initialize()
	{
	}
}
