using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using UnityEngine;

namespace Endless.Gameplay.Scripting;

public class InteractionNode : AttributeModifierNode, IInteractionBehavior, INpcAttributeModifier
{
	private readonly List<NpcEntity> currentEntities = new List<NpcEntity>();

	private float interactionDuration;

	private bool isHeldInteraction;

	[HideInInspector]
	public EndlessEvent OnInteractionCompleted = new EndlessEvent();

	[HideInInspector]
	public EndlessEvent OnInteractionCanceled = new EndlessEvent();

	[HideInInspector]
	public EndlessEvent OnInteractionFinished = new EndlessEvent();

	public override string InstructionName => "Generated Interaction";

	public override Type LuaObjectType => typeof(Interaction);

	public override NpcEnum.AttributeRank AttributeRank => NpcEnum.AttributeRank.Interaction;

	public override object LuaObject => luaObject ?? (luaObject = new Interaction(this));

	public float InteractionDuration
	{
		get
		{
			if (!IsHeldInteraction)
			{
				return 0f;
			}
			return interactionDuration;
		}
		internal set
		{
			foreach (NpcEntity currentEntity in currentEntities)
			{
				currentEntity.Components.Interactable.InteractionDuration = value;
			}
			interactionDuration = value;
		}
	}

	public bool IsHeldInteraction
	{
		get
		{
			return isHeldInteraction;
		}
		internal set
		{
			foreach (NpcEntity currentEntity in currentEntities)
			{
				currentEntity.Components.Interactable.IsHeldInteraction = value;
			}
			isHeldInteraction = value;
		}
	}

	public InteractionAnimation InteractionAnimation { get; set; }

	public override void RescindInstruction(Context context)
	{
		NpcEntity npcEntity = NpcReference.GetNpcEntity(context);
		if ((bool)npcEntity)
		{
			RescindInstruction(npcEntity);
		}
	}

	public override InstructionNode GetNode()
	{
		return this;
	}

	public override void GiveInstruction(Context context)
	{
		NpcEntity npcEntity = NpcReference.GetNpcEntity(context);
		if ((bool)npcEntity)
		{
			GiveInstruction(npcEntity);
		}
	}

	protected void GiveInstruction(NpcEntity npcEntity)
	{
		npcEntity.SetInteractionBehavior(this);
		npcEntity.Components.Interactable.IsHeldInteraction = isHeldInteraction;
		npcEntity.Components.Interactable.InteractionDuration = interactionDuration;
		scriptComponent.TryExecuteFunction("ConfigureNpc", out var _, npcEntity.Context);
		npcEntity.Components.Interactable.SetAllInteractablesEnabled(interactable: true);
		currentEntities.Add(npcEntity);
	}

	protected void RescindInstruction(NpcEntity npcEntity)
	{
		if (npcEntity.InteractionBehavior == this)
		{
			npcEntity.ClearInteractionBehavior();
			npcEntity.Components.Interactable.SetAllInteractablesEnabled(interactable: false);
			currentEntities.Remove(npcEntity);
		}
	}

	public void InteractionComplete(Context interactor)
	{
		OnInteractionCompleted?.Invoke(interactor);
		OnInteractionFinished?.Invoke(interactor);
	}

	public void InteractionCanceled(Context interactor)
	{
		OnInteractionCanceled?.Invoke(interactor);
		OnInteractionFinished?.Invoke(interactor);
	}

	public bool AttemptInteractServerLogic(Context interactor, Context npc, int colliderIndex)
	{
		bool result = true;
		if (scriptComponent.TryExecuteFunction("AttemptInteraction", out var returnValues, interactor, npc, colliderIndex))
		{
			int num;
			if (returnValues.Length != 0)
			{
				object obj = returnValues[0];
				num = ((obj is bool && (bool)obj) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			result = (byte)num != 0;
		}
		return result;
	}

	public void OnInteracted(Context interactor, Context npc, int colliderIndex)
	{
		scriptComponent.TryExecuteFunction("OnInteracted", out var _, interactor, npc, colliderIndex);
		npc.WorldObject.GetUserComponent<NpcEntity>().Components.Parameters.InteractionStartedTrigger = true;
	}

	public void InteractionStopped(Context interactor, Context npc)
	{
		scriptComponent.TryExecuteFunction("OnInteractionStopped", out var _, interactor, npc);
		NpcEntity userComponent = npc.WorldObject.GetUserComponent<NpcEntity>();
		if (userComponent.Components.Interactable.ActiveInteractors.Count == 0)
		{
			userComponent.Components.Parameters.InteractionFinishedTrigger = true;
		}
	}
}
