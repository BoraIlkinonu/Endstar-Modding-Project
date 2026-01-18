using Endless.Creator.LevelEditing;
using Endless.Gameplay;
using Endless.Shared;
using UnityEngine;

namespace Endless.Creator;

public class CreatorPlayerReferenceManager : PlayerReferenceManager
{
	[SerializeField]
	private PlayerGhostController playerGhostController;

	[SerializeField]
	private CreatorGunController creatorGunController;

	public override PlayerGhostController PlayerGhostController => playerGhostController;

	public CreatorGunController CreatorGunController => creatorGunController;

	protected override void Track()
	{
		base.Track();
		Debug.Log("Telling World Boundary Marker to track us");
		MonoBehaviourSingleton<WorldBoundaryMarker>.Instance.Track(base.transform);
	}

	protected override void Untrack()
	{
		base.Untrack();
		Debug.Log("Telling World Boundary Marker to untrack us");
		MonoBehaviourSingleton<WorldBoundaryMarker>.Instance.Untrack(base.transform);
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
		return "CreatorPlayerReferenceManager";
	}
}
