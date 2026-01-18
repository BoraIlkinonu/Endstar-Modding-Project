using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay.Fsm;

public class InteractionState : FsmState
{
	private InteractorBase currentInteractor;

	public override NpcEnum.FsmState State => NpcEnum.FsmState.Interaction;

	public InteractionState(NpcEntity entity)
		: base(entity)
	{
	}

	public override void Enter()
	{
		base.Enter();
		List<InteractorBase> activeInteractors = base.Entity.Components.Interactable.ActiveInteractors;
		currentInteractor = activeInteractors[activeInteractors.Count - 1];
	}

	protected override void Update()
	{
		base.Update();
		Vector3 vector = currentInteractor.transform.position - base.Entity.Position;
		vector = new Vector3(vector.x, 0f, vector.z);
		Quaternion to = Quaternion.LookRotation(vector);
		base.Entity.transform.rotation = Quaternion.RotateTowards(base.Entity.transform.rotation, to, base.Entity.Settings.RotationSpeed * Time.deltaTime);
	}
}
