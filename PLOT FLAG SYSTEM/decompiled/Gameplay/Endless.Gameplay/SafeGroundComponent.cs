using System;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class SafeGroundComponent : NetworkBehaviour
{
	private NetworkVariable<Vector3> position = new NetworkVariable<Vector3>();

	public Vector3 LastSafePosition => position.Value;

	public void RegisterSafeGround(Vector3 pos)
	{
		if (base.IsServer)
		{
			position.Value = pos;
		}
	}

	protected override void __initializeVariables()
	{
		if (position == null)
		{
			throw new Exception("SafeGroundComponent.position cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		position.Initialize(this);
		__nameNetworkVariable(position, "position");
		NetworkVariableFields.Add(position);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "SafeGroundComponent";
	}
}
