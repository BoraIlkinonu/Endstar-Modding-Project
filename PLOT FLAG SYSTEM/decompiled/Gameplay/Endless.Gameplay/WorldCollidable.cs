using System;
using Unity.Netcode;

namespace Endless.Gameplay;

public class WorldCollidable : NetworkBehaviour
{
	private IPhysicsTaker physicsTaker;

	private WorldObject worldObject;

	public Func<bool> isSimulatedCheckOverride { get; set; }

	public bool IsSimulated
	{
		get
		{
			if (isSimulatedCheckOverride != null)
			{
				return isSimulatedCheckOverride();
			}
			if (!base.IsServer)
			{
				return base.IsOwner;
			}
			return true;
		}
	}

	public IPhysicsTaker PhysicsTaker
	{
		get
		{
			if (physicsTaker == null)
			{
				physicsTaker = GetComponent<IPhysicsTaker>();
			}
			return physicsTaker;
		}
	}

	public WorldObject WorldObject
	{
		get
		{
			if (worldObject == null)
			{
				worldObject = GetComponent<WorldObject>();
				if (worldObject == null)
				{
					worldObject = GetComponentInParent<WorldObject>();
				}
			}
			return worldObject;
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "WorldCollidable";
	}
}
