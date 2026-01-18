using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.SoVariables;
using Endless.Props;
using Endless.Props.ReferenceComponents;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class Interactable : InteractableBase, IAwakeSubscriber, IScriptInjector, IComponentBase
{
	private NetworkVariable<PropLibraryReference> libraryReferenceForPromptIcon = new NetworkVariable<PropLibraryReference>();

	[SerializeField]
	private InteractionAnimation defaultInteractionAnimation;

	private Endless.Gameplay.LuaInterfaces.Interactable luaInterface;

	internal EndlessScriptComponent scriptComponent;

	public object LuaObject => luaInterface ?? (luaInterface = new Endless.Gameplay.LuaInterfaces.Interactable(this));

	public Type LuaObjectType => typeof(Endless.Gameplay.LuaInterfaces.Interactable);

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public Type ComponentReferenceType => typeof(SimpleInteractableReferences);

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		NetworkVariable<PropLibraryReference> networkVariable = libraryReferenceForPromptIcon;
		networkVariable.OnValueChanged = (NetworkVariable<PropLibraryReference>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<PropLibraryReference>.OnValueChangedDelegate(HandleLibraryIconChanged));
		if (libraryReferenceForPromptIcon.Value != null)
		{
			UpdateIcon();
		}
	}

	private void HandleLibraryIconChanged(PropLibraryReference previousvalue, PropLibraryReference newvalue)
	{
		UpdateIcon();
	}

	private void UpdateIcon()
	{
		UIInteractionPromptVariable uIInteractionPromptVariable = ScriptableObject.CreateInstance<UIInteractionPromptVariable>();
		uIInteractionPromptVariable.Value = base.InteractionPrompt.Value;
		uIInteractionPromptVariable.Value.supplementalInteractionResultSprite = libraryReferenceForPromptIcon.Value?.GetReference().Icon;
		base.InteractionPrompt = uIInteractionPromptVariable;
	}

	public int GetNumberOfInteractables()
	{
		return interactableColliders.Count;
	}

	protected override bool AttemptInteract_ServerLogic(InteractorBase interactor, int colliderIndex)
	{
		bool result = true;
		if (scriptComponent.TryExecuteFunction("AttemptInteraction", out var returnValues, interactor.ContextObject, colliderIndex))
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

	protected override void InteractServerLogic(InteractorBase interactor, int colliderIndex)
	{
		scriptComponent.TryExecuteFunction("OnInteracted", out var _, interactor.ContextObject, colliderIndex);
	}

	public override void InteractionStopped(InteractorBase interactor)
	{
		scriptComponent.TryExecuteFunction("OnInteractionStopped", out var _, interactor.ContextObject);
	}

	public void SetIconFromPropReference(PropLibraryReference reference)
	{
		libraryReferenceForPromptIcon.Value = reference;
	}

	public void EndlessAwake()
	{
		base.InteractionAnimation = defaultInteractionAnimation;
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		IReadOnlyList<ColliderInfo> interactableColliderInfos = (referenceBase as SimpleInteractableReferences).InteractableColliderInfos;
		SetupColliders(interactableColliderInfos);
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	protected override void __initializeVariables()
	{
		if (libraryReferenceForPromptIcon == null)
		{
			throw new Exception("Interactable.libraryReferenceForPromptIcon cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		libraryReferenceForPromptIcon.Initialize(this);
		__nameNetworkVariable(libraryReferenceForPromptIcon, "libraryReferenceForPromptIcon");
		NetworkVariableFields.Add(libraryReferenceForPromptIcon);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "Interactable";
	}
}
