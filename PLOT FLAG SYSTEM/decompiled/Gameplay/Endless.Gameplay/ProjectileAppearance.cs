using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class ProjectileAppearance : MonoBehaviour, IPoolableT
{
	public struct ProjectileState : IFrameInfo
	{
		public Vector3 position;

		public Vector3 eulerAngles;

		public bool visible;

		public uint NetFrame { get; set; }

		public void Clear()
		{
		}

		public void Initialize()
		{
		}
	}

	private const uint CATCH_UP_FRAME_COUNT = 6u;

	[SerializeField]
	private GameObject visualsObject;

	[SerializeField]
	private ParticleSystem particles;

	private uint serverSpawnFrame;

	private uint localSpawnFrame;

	private Vector3 spawnPosition;

	private InterpolationRingBuffer<ProjectileState> stateRingBuffer = new InterpolationRingBuffer<ProjectileState>(15);

	private bool visualsActive = true;

	private uint lastStateFrame;

	private bool destroyed;

	public MonoBehaviour Prefab { get; set; }

	public uint LifetimeEndFrame { get; set; } = uint.MaxValue;

	public int OwnerInstanceID { get; set; }

	private void Awake()
	{
		DisableVisuals();
	}

	private void OnEnable()
	{
		ProjectileManager.AddProjectileAppearance(this);
	}

	private void OnDisable()
	{
		ProjectileManager.RemoveProjectileAppearance(this);
	}

	public void SetState(uint frame, Vector3 position, Vector3 eulerAngles, bool visible, uint? catchUpFrames = null)
	{
		uint num = (catchUpFrames.HasValue ? (frame + catchUpFrames.Value) : ((localSpawnFrame > frame) ? localSpawnFrame : frame));
		if (!catchUpFrames.HasValue)
		{
			catchUpFrames = 6u;
		}
		position = Vector3.Lerp(spawnPosition, position, (float)(num - localSpawnFrame) / (float)catchUpFrames.Value);
		ref ProjectileState atPosition = ref stateRingBuffer.GetAtPosition(frame);
		atPosition.position = position;
		atPosition.eulerAngles = eulerAngles;
		atPosition.visible = visible;
		atPosition.NetFrame = ((frame < localSpawnFrame) ? frame : num);
		stateRingBuffer.FrameUpdated(frame);
		lastStateFrame = ((NetClock.CurrentFrame > num) ? NetClock.CurrentFrame : num);
	}

	public void SetupAutoHit(Vector3 startPosition, Projectile.HitScanResult autohitData, uint autoHitFrames)
	{
		Vector3 eulerAngles = Quaternion.LookRotation(autohitData.WorldPosition - startPosition, Vector3.up).eulerAngles;
		uint num = NetClock.CurrentSimulationFrame - 1;
		RegisterSpawnInfo(num, num, startPosition, eulerAngles);
		SetState(num + 1, autohitData.WorldPosition, eulerAngles, visible: true, autoHitFrames);
		SetState(num + 2, autohitData.WorldPosition, eulerAngles, visible: false, autoHitFrames);
		LifetimeEndFrame = num + autoHitFrames;
	}

	public void RegisterSpawnInfo(uint serverSpawnFrame, uint localSpawnFrame, Vector3 spawnPosition, Vector3 spawnAngle)
	{
		this.serverSpawnFrame = serverSpawnFrame;
		this.localSpawnFrame = localSpawnFrame;
		this.spawnPosition = spawnPosition;
		SetState(localSpawnFrame - 1, spawnPosition, spawnAngle, visible: false);
		SetState(localSpawnFrame, spawnPosition, spawnAngle, visible: true);
		stateRingBuffer.InitPastAndNext(localSpawnFrame - 1);
	}

	private void Update()
	{
		if (stateRingBuffer.PastInterpolationState.NetFrame > LifetimeEndFrame || NetworkManager.Singleton == null || (NetClock.CurrentFrame > lastStateFrame && NetClock.CurrentFrame - lastStateFrame > 3))
		{
			DestroySelf();
			return;
		}
		stateRingBuffer.ActiveInterpolationTime = (NetworkManager.Singleton.IsServer ? NetClock.ServerAppearanceTime : NetClock.ClientExtrapolatedAppearanceTime);
		stateRingBuffer.ActiveInterpolatedState.position = Vector3.Lerp(stateRingBuffer.PastInterpolationState.position, stateRingBuffer.NextInterpolationState.position, stateRingBuffer.ActiveStateLerpTime);
		stateRingBuffer.ActiveInterpolatedState.eulerAngles = Vector3.Lerp(stateRingBuffer.PastInterpolationState.eulerAngles, stateRingBuffer.NextInterpolationState.eulerAngles, stateRingBuffer.ActiveStateLerpTime);
		stateRingBuffer.ActiveInterpolatedState.visible = stateRingBuffer.NextInterpolationState.visible;
		base.transform.position = stateRingBuffer.ActiveInterpolatedState.position;
		base.transform.eulerAngles = stateRingBuffer.ActiveInterpolatedState.eulerAngles;
		if (stateRingBuffer.ActiveInterpolatedState.visible)
		{
			EnableVisuals();
		}
		else
		{
			DisableVisuals();
		}
	}

	private void EnableVisuals()
	{
		if (!visualsActive)
		{
			visualsActive = true;
			visualsObject.SetActive(value: true);
		}
		if (particles != null)
		{
			particles.Play();
		}
	}

	private void DisableVisuals()
	{
		if (visualsActive)
		{
			visualsActive = false;
			visualsObject.SetActive(value: false);
		}
	}

	public void DestroySelf()
	{
		if (base.gameObject != null && !destroyed)
		{
			destroyed = true;
			if (ProjectileManager.UsePooling)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(this);
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	public void OnSpawn()
	{
		base.transform.SetParent(null, worldPositionStays: true);
		LifetimeEndFrame = uint.MaxValue;
		lastStateFrame = 0u;
		destroyed = false;
		stateRingBuffer.Clear();
	}

	public void OnDespawn()
	{
		DisableVisuals();
		if (particles != null)
		{
			particles.Stop();
		}
	}

	public void Play()
	{
		if (particles != null)
		{
			particles.Play();
		}
	}
}
