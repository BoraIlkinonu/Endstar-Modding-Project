using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.SoVariables;
using Endless.Gameplay.UI;
using Endless.Props;
using Endless.Props.ReferenceComponents;
using Endless.Shared.SoVariables;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000D7 RID: 215
	public class Interactable : InteractableBase, IAwakeSubscriber, IScriptInjector, IComponentBase
	{
		// Token: 0x0600043E RID: 1086 RVA: 0x00016B90 File Offset: 0x00014D90
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			NetworkVariable<PropLibraryReference> networkVariable = this.libraryReferenceForPromptIcon;
			networkVariable.OnValueChanged = (NetworkVariable<PropLibraryReference>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<PropLibraryReference>.OnValueChangedDelegate(this.HandleLibraryIconChanged));
			if (this.libraryReferenceForPromptIcon.Value != null)
			{
				this.UpdateIcon();
			}
		}

		// Token: 0x0600043F RID: 1087 RVA: 0x00016BE3 File Offset: 0x00014DE3
		private void HandleLibraryIconChanged(PropLibraryReference previousvalue, PropLibraryReference newvalue)
		{
			this.UpdateIcon();
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x00016BEC File Offset: 0x00014DEC
		private void UpdateIcon()
		{
			UIInteractionPromptVariable uiinteractionPromptVariable = ScriptableObject.CreateInstance<UIInteractionPromptVariable>();
			uiinteractionPromptVariable.Value = base.InteractionPrompt.Value;
			SoVariable<UIInteractionPrompt> soVariable = uiinteractionPromptVariable;
			PropLibraryReference value = this.libraryReferenceForPromptIcon.Value;
			soVariable.Value.supplementalInteractionResultSprite = ((value != null) ? value.GetReference().Icon : null);
			base.InteractionPrompt = uiinteractionPromptVariable;
		}

		// Token: 0x06000441 RID: 1089 RVA: 0x00016C3E File Offset: 0x00014E3E
		public int GetNumberOfInteractables()
		{
			return this.interactableColliders.Count;
		}

		// Token: 0x06000442 RID: 1090 RVA: 0x00016C4C File Offset: 0x00014E4C
		protected override bool AttemptInteract_ServerLogic(InteractorBase interactor, int colliderIndex)
		{
			bool flag = true;
			object[] array;
			if (this.scriptComponent.TryExecuteFunction("AttemptInteraction", out array, new object[] { interactor.ContextObject, colliderIndex }))
			{
				bool flag2;
				if (array.Length != 0)
				{
					object obj = array[0];
					flag2 = obj is bool && (bool)obj;
				}
				else
				{
					flag2 = false;
				}
				flag = flag2;
			}
			return flag;
		}

		// Token: 0x06000443 RID: 1091 RVA: 0x00016CA8 File Offset: 0x00014EA8
		protected override void InteractServerLogic(InteractorBase interactor, int colliderIndex)
		{
			object[] array;
			this.scriptComponent.TryExecuteFunction("OnInteracted", out array, new object[] { interactor.ContextObject, colliderIndex });
		}

		// Token: 0x06000444 RID: 1092 RVA: 0x00016CE0 File Offset: 0x00014EE0
		public override void InteractionStopped(InteractorBase interactor)
		{
			object[] array;
			this.scriptComponent.TryExecuteFunction("OnInteractionStopped", out array, new object[] { interactor.ContextObject });
		}

		// Token: 0x06000445 RID: 1093 RVA: 0x00016D0F File Offset: 0x00014F0F
		public void SetIconFromPropReference(PropLibraryReference reference)
		{
			this.libraryReferenceForPromptIcon.Value = reference;
		}

		// Token: 0x06000446 RID: 1094 RVA: 0x00016D1D File Offset: 0x00014F1D
		public void EndlessAwake()
		{
			base.InteractionAnimation = this.defaultInteractionAnimation;
		}

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x06000447 RID: 1095 RVA: 0x00016D2C File Offset: 0x00014F2C
		public object LuaObject
		{
			get
			{
				Interactable interactable;
				if ((interactable = this.luaInterface) == null)
				{
					interactable = (this.luaInterface = new Interactable(this));
				}
				return interactable;
			}
		}

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x06000448 RID: 1096 RVA: 0x00016D52 File Offset: 0x00014F52
		public Type LuaObjectType
		{
			get
			{
				return typeof(Interactable);
			}
		}

		// Token: 0x06000449 RID: 1097 RVA: 0x00016D5E File Offset: 0x00014F5E
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x0600044A RID: 1098 RVA: 0x00016D67 File Offset: 0x00014F67
		// (set) Token: 0x0600044B RID: 1099 RVA: 0x00016D6F File Offset: 0x00014F6F
		public WorldObject WorldObject { get; private set; }

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x0600044C RID: 1100 RVA: 0x00016D78 File Offset: 0x00014F78
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(SimpleInteractableReferences);
			}
		}

		// Token: 0x0600044D RID: 1101 RVA: 0x00016D84 File Offset: 0x00014F84
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			IReadOnlyList<ColliderInfo> interactableColliderInfos = (referenceBase as SimpleInteractableReferences).InteractableColliderInfos;
			base.SetupColliders(interactableColliderInfos);
		}

		// Token: 0x0600044E RID: 1102 RVA: 0x00016DA4 File Offset: 0x00014FA4
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x06000450 RID: 1104 RVA: 0x00016DC4 File Offset: 0x00014FC4
		protected override void __initializeVariables()
		{
			bool flag = this.libraryReferenceForPromptIcon == null;
			if (flag)
			{
				throw new Exception("Interactable.libraryReferenceForPromptIcon cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.libraryReferenceForPromptIcon.Initialize(this);
			base.__nameNetworkVariable(this.libraryReferenceForPromptIcon, "libraryReferenceForPromptIcon");
			this.NetworkVariableFields.Add(this.libraryReferenceForPromptIcon);
			base.__initializeVariables();
		}

		// Token: 0x06000451 RID: 1105 RVA: 0x00016E27 File Offset: 0x00015027
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000452 RID: 1106 RVA: 0x00016E31 File Offset: 0x00015031
		protected internal override string __getTypeName()
		{
			return "Interactable";
		}

		// Token: 0x040003B4 RID: 948
		private NetworkVariable<PropLibraryReference> libraryReferenceForPromptIcon = new NetworkVariable<PropLibraryReference>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x040003B5 RID: 949
		[SerializeField]
		private InteractionAnimation defaultInteractionAnimation;

		// Token: 0x040003B6 RID: 950
		private Interactable luaInterface;

		// Token: 0x040003B7 RID: 951
		internal EndlessScriptComponent scriptComponent;
	}
}
