using System;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay;

public class ResizableVolume : MonoBehaviour, IComponentBase
{
	[SerializeField]
	private Transform scaleTransform;

	[SerializeField]
	private Transform offsetTransform;

	[SerializeField]
	private Renderer volumeVisuals;

	private UnityEngine.Vector3 offset;

	private int forward;

	private int backward;

	private int left;

	private int right;

	private int up;

	private int down;

	[SerializeField]
	[HideInInspector]
	private EndlessProp endlessProp;

	public UnityEngine.Vector3 Offset
	{
		get
		{
			return offset;
		}
		set
		{
			value.x = Mathf.Clamp(value.x, -1f, 1f);
			value.y = Mathf.Clamp(value.y, -1f, 1f);
			value.z = Mathf.Clamp(value.z, -1f, 1f);
			offset = value;
			RepositionVolume();
		}
	}

	public int Forward
	{
		get
		{
			return forward;
		}
		set
		{
			value = Mathf.Max(value, 0);
			forward = value;
			RecalculateZAxis();
		}
	}

	public int Backward
	{
		get
		{
			return backward;
		}
		set
		{
			value = Mathf.Max(value, 0);
			backward = value;
			RecalculateZAxis();
		}
	}

	public int Left
	{
		get
		{
			return left;
		}
		set
		{
			value = Mathf.Max(value, 0);
			left = value;
			RecalculateXAxis();
		}
	}

	public int Right
	{
		get
		{
			return right;
		}
		set
		{
			value = Mathf.Max(value, 0);
			right = value;
			RecalculateXAxis();
		}
	}

	public int Up
	{
		get
		{
			return up;
		}
		set
		{
			value = Mathf.Max(value, 0);
			up = value;
			RecalculateYAxis();
		}
	}

	public int Down
	{
		get
		{
			return down;
		}
		set
		{
			value = Mathf.Max(value, 0);
			down = value;
			RecalculateYAxis();
		}
	}

	public WorldObject WorldObject { get; private set; }

	public Type ComponentReferenceType => typeof(ResizableVolumeReferences);

	private void RecalculateYAxis()
	{
		int num = up + down + 1;
		float y = (float)(up - down) / 2f;
		scaleTransform.localScale = new UnityEngine.Vector3(scaleTransform.localScale.x, num, scaleTransform.localScale.z);
		scaleTransform.localPosition = new UnityEngine.Vector3(scaleTransform.localPosition.x, y, scaleTransform.localPosition.z);
	}

	private void RecalculateXAxis()
	{
		int num = right + left + 1;
		float x = (float)(right - left) / 2f;
		scaleTransform.localScale = new UnityEngine.Vector3(num, scaleTransform.localScale.y, scaleTransform.localScale.z);
		scaleTransform.localPosition = new UnityEngine.Vector3(x, scaleTransform.localPosition.y, scaleTransform.localPosition.z);
	}

	private void RecalculateZAxis()
	{
		int num = forward + backward + 1;
		float z = (float)(forward - backward) / 2f;
		scaleTransform.localScale = new UnityEngine.Vector3(scaleTransform.localScale.x, scaleTransform.localScale.y, num);
		scaleTransform.localPosition = new UnityEngine.Vector3(scaleTransform.localPosition.x, scaleTransform.localPosition.y, z);
	}

	private void RepositionVolume()
	{
		offsetTransform.localPosition = offset;
	}

	protected virtual void Awake()
	{
		endlessProp.OnInspectionStateChanged.AddListener(HandleInspectionStateChanged);
	}

	private void HandleInspectionStateChanged(bool isInspected)
	{
		volumeVisuals.enabled = isInspected;
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		this.endlessProp = endlessProp;
		BoxCollider[] collidersToScale = (referenceBase as ResizableVolumeReferences).CollidersToScale;
		for (int i = 0; i < collidersToScale.Length; i++)
		{
			collidersToScale[i].transform.SetParent(scaleTransform);
		}
	}
}
