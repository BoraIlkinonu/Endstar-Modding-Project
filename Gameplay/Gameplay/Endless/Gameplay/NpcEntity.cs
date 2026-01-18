using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Data;
using Endless.Gameplay.Fsm;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Matchmaking;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x0200032C RID: 812
	public class NpcEntity : EndlessNetworkBehaviour, IAttributeSourceController, IAwakeSubscriber, IGameEndSubscriber, IPersistantStateSubscriber, IBaseType, IComponentBase, IScriptInjector
	{
		// Token: 0x170003BC RID: 956
		// (get) Token: 0x060012DB RID: 4827 RVA: 0x0005C78C File Offset: 0x0005A98C
		public static NavMeshQueryFilter NavFilter
		{
			get
			{
				return new NavMeshQueryFilter
				{
					agentTypeID = 0,
					areaMask = -1
				};
			}
		}

		// Token: 0x170003BD RID: 957
		// (get) Token: 0x060012DC RID: 4828 RVA: 0x0005C7B2 File Offset: 0x0005A9B2
		// (set) Token: 0x060012DD RID: 4829 RVA: 0x0005C7BA File Offset: 0x0005A9BA
		public NpcSettings Settings { get; private set; }

		// Token: 0x170003BE RID: 958
		// (get) Token: 0x060012DE RID: 4830 RVA: 0x0005C7C3 File Offset: 0x0005A9C3
		// (set) Token: 0x060012DF RID: 4831 RVA: 0x0005C7CB File Offset: 0x0005A9CB
		public IdleBehavior IdleBehavior { get; private set; }

		// Token: 0x170003BF RID: 959
		// (get) Token: 0x060012E0 RID: 4832 RVA: 0x0005C7D4 File Offset: 0x0005A9D4
		// (set) Token: 0x060012E1 RID: 4833 RVA: 0x0005C7DC File Offset: 0x0005A9DC
		public PathfindingRange PathfindingRange { get; private set; }

		// Token: 0x170003C0 RID: 960
		// (get) Token: 0x060012E2 RID: 4834 RVA: 0x0005C7E5 File Offset: 0x0005A9E5
		// (set) Token: 0x060012E3 RID: 4835 RVA: 0x0005C7ED File Offset: 0x0005A9ED
		public int StartingHealth { get; private set; }

		// Token: 0x170003C1 RID: 961
		// (get) Token: 0x060012E4 RID: 4836 RVA: 0x0005C7F6 File Offset: 0x0005A9F6
		// (set) Token: 0x060012E5 RID: 4837 RVA: 0x0005C7FE File Offset: 0x0005A9FE
		public AnimationClipSet Fidgets { get; private set; }

		// Token: 0x170003C2 RID: 962
		// (get) Token: 0x060012E6 RID: 4838 RVA: 0x0005C807 File Offset: 0x0005AA07
		// (set) Token: 0x060012E7 RID: 4839 RVA: 0x0005C80F File Offset: 0x0005AA0F
		public AnimationClipSet CombatFidgets { get; private set; }

		// Token: 0x170003C3 RID: 963
		// (get) Token: 0x060012E8 RID: 4840 RVA: 0x0005C818 File Offset: 0x0005AA18
		// (set) Token: 0x060012E9 RID: 4841 RVA: 0x0005C820 File Offset: 0x0005AA20
		public AnimationClipSet TauntClipSet { get; private set; }

		// Token: 0x170003C4 RID: 964
		// (get) Token: 0x060012EA RID: 4842 RVA: 0x0005C829 File Offset: 0x0005AA29
		// (set) Token: 0x060012EB RID: 4843 RVA: 0x0005C831 File Offset: 0x0005AA31
		public NpcSpawnAnimation SpawnAnimation { get; private set; }

		// Token: 0x170003C5 RID: 965
		// (get) Token: 0x060012EC RID: 4844 RVA: 0x0005C83A File Offset: 0x0005AA3A
		// (set) Token: 0x060012ED RID: 4845 RVA: 0x0005C844 File Offset: 0x0005AA44
		public CharacterVisualsReference CharacterVisuals
		{
			get
			{
				return this.characterVisuals;
			}
			set
			{
				if (this.characterVisuals.Id == value.Id)
				{
					return;
				}
				this.characterVisuals = value;
				CharacterVisualsReference characterVisualsReference = this.characterVisuals;
				CharacterCosmeticsDefinition characterCosmeticsDefinition = ((characterVisualsReference != null) ? characterVisualsReference.GetDefinition() : null);
				if (this.characterVisuals != null && characterCosmeticsDefinition && !characterCosmeticsDefinition.IsMissingAsset)
				{
					this.haveSpawnedVisuals = true;
					this.Components.NpcVisuals.UpdateVisuals(characterCosmeticsDefinition);
				}
			}
		}

		// Token: 0x170003C6 RID: 966
		// (get) Token: 0x060012EE RID: 4846 RVA: 0x0005C8B4 File Offset: 0x0005AAB4
		public bool IsInteractable
		{
			get
			{
				NpcEnum.FsmState state = this.CurrentState.State;
				return state == NpcEnum.FsmState.Neutral || state == NpcEnum.FsmState.Interaction;
			}
		}

		// Token: 0x170003C7 RID: 967
		// (get) Token: 0x060012EF RID: 4847 RVA: 0x0005C8DF File Offset: 0x0005AADF
		// (set) Token: 0x060012F0 RID: 4848 RVA: 0x0005C8E7 File Offset: 0x0005AAE7
		public FsmState CurrentState
		{
			get
			{
				return this.currentState;
			}
			set
			{
				if (this.currentState != value)
				{
					this.currentState = value;
					this.CurrentStateName = value.StateName;
				}
			}
		}

		// Token: 0x170003C8 RID: 968
		// (get) Token: 0x060012F1 RID: 4849 RVA: 0x0005C905 File Offset: 0x0005AB05
		// (set) Token: 0x060012F2 RID: 4850 RVA: 0x0005C910 File Offset: 0x0005AB10
		public NpcClassCustomizationData NpcClass
		{
			get
			{
				return this.npcClass;
			}
			set
			{
				if (!MatchSession.Instance.MatchData.IsEditSession || (MatchSession.Instance.MatchData.IsEditSession && !NetworkManager.Singleton.IsHost))
				{
					if (!EndlessCloudService.CanHaveRiflemen() && value.NpcClass == Endless.Gameplay.LuaEnums.NpcClass.Rifleman)
					{
						ErrorHandler.HandleError(ErrorCodes.NpcEntity_NpcClasSet_ContentRestricted_Rifleman, new Exception("Content Restricted: Account is not allowed to have riflemen."), true, true);
					}
					else if (!EndlessCloudService.CanHaveGrunt() && value.NpcClass == Endless.Gameplay.LuaEnums.NpcClass.Grunt)
					{
						ErrorHandler.HandleError(ErrorCodes.NpcEntity_NpcClasSet_ContentRestricted_Grunt, new Exception("Content Restricted: Account is not allowed to have grunts."), true, true);
					}
					else if (!EndlessCloudService.CanHaveZombies() && value.NpcClass == Endless.Gameplay.LuaEnums.NpcClass.Zombie)
					{
						ErrorHandler.HandleError(ErrorCodes.NpcEntity_NpcClasSet_ContentRestricted_Zombie, new Exception("Content Restricted: Account is not allowed to have zombies."), true, true);
					}
				}
				if (this.npcClass == value)
				{
					return;
				}
				this.npcClass = value;
				this.Components.NpcVisuals.RespawnCosmetics();
			}
		}

		// Token: 0x170003C9 RID: 969
		// (get) Token: 0x060012F3 RID: 4851 RVA: 0x0005C9E5 File Offset: 0x0005ABE5
		// (set) Token: 0x060012F4 RID: 4852 RVA: 0x0005C9ED File Offset: 0x0005ABED
		public uint IdleCompleteFrame { get; set; }

		// Token: 0x170003CA RID: 970
		// (get) Token: 0x060012F5 RID: 4853 RVA: 0x0005C9F6 File Offset: 0x0005ABF6
		// (set) Token: 0x060012F6 RID: 4854 RVA: 0x0005CA00 File Offset: 0x0005AC00
		public NpcGroup Group
		{
			get
			{
				return this.group;
			}
			set
			{
				if (value == this.group)
				{
					return;
				}
				NpcGroup npcGroup = this.group;
				this.group = value;
				MonoBehaviourSingleton<NpcManager>.Instance.UpdateNpcGroup(this, npcGroup);
			}
		}

		// Token: 0x14000025 RID: 37
		// (add) Token: 0x060012F7 RID: 4855 RVA: 0x0005CA34 File Offset: 0x0005AC34
		// (remove) Token: 0x060012F8 RID: 4856 RVA: 0x0005CA6C File Offset: 0x0005AC6C
		public event Action OnBaseAttributeChanged;

		// Token: 0x14000026 RID: 38
		// (add) Token: 0x060012F9 RID: 4857 RVA: 0x0005CAA4 File Offset: 0x0005ACA4
		// (remove) Token: 0x060012FA RID: 4858 RVA: 0x0005CADC File Offset: 0x0005ACDC
		public event Action OnCombatStateChanged;

		// Token: 0x170003CB RID: 971
		// (get) Token: 0x060012FB RID: 4859 RVA: 0x0005CB11 File Offset: 0x0005AD11
		public NpcBlackboard NpcBlackboard { get; } = new NpcBlackboard();

		// Token: 0x170003CC RID: 972
		// (get) Token: 0x060012FC RID: 4860 RVA: 0x0001965C File Offset: 0x0001785C
		public bool CanDoubleJump
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003CD RID: 973
		// (get) Token: 0x060012FD RID: 4861 RVA: 0x0005CB19 File Offset: 0x0005AD19
		// (set) Token: 0x060012FE RID: 4862 RVA: 0x0005CB21 File Offset: 0x0005AD21
		public bool IsRangedAttacker { get; set; }

		// Token: 0x170003CE RID: 974
		// (get) Token: 0x060012FF RID: 4863 RVA: 0x0005CB2A File Offset: 0x0005AD2A
		public Team Team
		{
			get
			{
				return this.WorldObject.GetUserComponent<TeamComponent>().Team;
			}
		}

		// Token: 0x170003CF RID: 975
		// (get) Token: 0x06001300 RID: 4864 RVA: 0x0005CB3C File Offset: 0x0005AD3C
		public bool IsDowned
		{
			get
			{
				return this.CurrentState.State == NpcEnum.FsmState.Downed;
			}
		}

		// Token: 0x170003D0 RID: 976
		// (get) Token: 0x06001301 RID: 4865 RVA: 0x0005CB4C File Offset: 0x0005AD4C
		public float AnimationSpeed
		{
			get
			{
				return this.NpcBlackboard.GetValueOrDefault<float>(NpcBlackboard.Key.OverrideSpeed, this.Components.Agent.speed);
			}
		}

		// Token: 0x170003D1 RID: 977
		// (get) Token: 0x06001302 RID: 4866 RVA: 0x0005CB6B File Offset: 0x0005AD6B
		public int Health
		{
			get
			{
				return this.Components.Health.CurrentHealth;
			}
		}

		// Token: 0x170003D2 RID: 978
		// (get) Token: 0x06001303 RID: 4867 RVA: 0x0005CB7D File Offset: 0x0005AD7D
		// (set) Token: 0x06001304 RID: 4868 RVA: 0x0005CB85 File Offset: 0x0005AD85
		public NpcEnum.CombatState CombatState
		{
			get
			{
				return this.combatState;
			}
			set
			{
				this.combatState = value;
				Action onCombatStateChanged = this.OnCombatStateChanged;
				if (onCombatStateChanged == null)
				{
					return;
				}
				onCombatStateChanged();
			}
		}

		// Token: 0x170003D3 RID: 979
		// (get) Token: 0x06001305 RID: 4869 RVA: 0x0005CB9E File Offset: 0x0005AD9E
		public HittableComponent Target
		{
			get
			{
				return this.Components.TargeterComponent.Target;
			}
		}

		// Token: 0x170003D4 RID: 980
		// (get) Token: 0x06001306 RID: 4870 RVA: 0x0005CBB0 File Offset: 0x0005ADB0
		// (set) Token: 0x06001307 RID: 4871 RVA: 0x0005CBB8 File Offset: 0x0005ADB8
		public HittableComponent FollowTarget { get; set; }

		// Token: 0x170003D5 RID: 981
		// (get) Token: 0x06001308 RID: 4872 RVA: 0x0005CBC1 File Offset: 0x0005ADC1
		// (set) Token: 0x06001309 RID: 4873 RVA: 0x0005CBC9 File Offset: 0x0005ADC9
		public bool HasAttackToken { get; set; }

		// Token: 0x170003D6 RID: 982
		// (get) Token: 0x0600130A RID: 4874 RVA: 0x0005CBD2 File Offset: 0x0005ADD2
		// (set) Token: 0x0600130B RID: 4875 RVA: 0x0005CBDA File Offset: 0x0005ADDA
		public AttackRequest CurrentRequest { get; set; }

		// Token: 0x170003D7 RID: 983
		// (get) Token: 0x0600130C RID: 4876 RVA: 0x00017DAC File Offset: 0x00015FAC
		// (set) Token: 0x0600130D RID: 4877 RVA: 0x0005CBE3 File Offset: 0x0005ADE3
		public global::UnityEngine.Vector3 Position
		{
			get
			{
				return base.transform.position;
			}
			set
			{
				base.transform.position = value;
			}
		}

		// Token: 0x170003D8 RID: 984
		// (get) Token: 0x0600130E RID: 4878 RVA: 0x0005CBF1 File Offset: 0x0005ADF1
		// (set) Token: 0x0600130F RID: 4879 RVA: 0x0005CC03 File Offset: 0x0005AE03
		public float Rotation
		{
			get
			{
				return base.transform.rotation.y;
			}
			set
			{
				base.transform.rotation = Quaternion.Euler(0f, value, 0f);
			}
		}

		// Token: 0x170003D9 RID: 985
		// (get) Token: 0x06001310 RID: 4880 RVA: 0x0005CC20 File Offset: 0x0005AE20
		public global::UnityEngine.Vector3 FootPosition
		{
			get
			{
				NavMeshHit navMeshHit;
				if (NavMesh.SamplePosition(this.Position + global::UnityEngine.Vector3.down * 0.5f, out navMeshHit, 0.25f, -1))
				{
					return navMeshHit.position;
				}
				return this.Position + global::UnityEngine.Vector3.down * 0.5f;
			}
		}

		// Token: 0x170003DA RID: 986
		// (get) Token: 0x06001311 RID: 4881 RVA: 0x0005CC78 File Offset: 0x0005AE78
		// (set) Token: 0x06001312 RID: 4882 RVA: 0x0005CC80 File Offset: 0x0005AE80
		internal CombatMode BaseCombatMode
		{
			get
			{
				return this.baseCombatMode;
			}
			set
			{
				if (this.baseCombatMode == value)
				{
					return;
				}
				this.baseCombatMode = value;
				Action onBaseAttributeChanged = this.OnBaseAttributeChanged;
				if (onBaseAttributeChanged == null)
				{
					return;
				}
				onBaseAttributeChanged();
			}
		}

		// Token: 0x170003DB RID: 987
		// (get) Token: 0x06001313 RID: 4883 RVA: 0x0005CCA3 File Offset: 0x0005AEA3
		// (set) Token: 0x06001314 RID: 4884 RVA: 0x0005CCAC File Offset: 0x0005AEAC
		internal DamageMode BaseDamageMode
		{
			get
			{
				return this.baseDamageMode;
			}
			set
			{
				DamageMode damageMode = value;
				if (damageMode == DamageMode.UseDefault)
				{
					damageMode = DamageMode.TakeDamage;
				}
				if (this.baseDamageMode == damageMode)
				{
					return;
				}
				this.baseDamageMode = damageMode;
				Action onBaseAttributeChanged = this.OnBaseAttributeChanged;
				if (onBaseAttributeChanged == null)
				{
					return;
				}
				onBaseAttributeChanged();
			}
		}

		// Token: 0x170003DC RID: 988
		// (get) Token: 0x06001315 RID: 4885 RVA: 0x0005CCE2 File Offset: 0x0005AEE2
		// (set) Token: 0x06001316 RID: 4886 RVA: 0x0005CCEA File Offset: 0x0005AEEA
		internal PhysicsMode BasePhysicsMode
		{
			get
			{
				return this.basePhysicsMode;
			}
			set
			{
				if (this.basePhysicsMode == value)
				{
					return;
				}
				this.basePhysicsMode = value;
				Action onBaseAttributeChanged = this.OnBaseAttributeChanged;
				if (onBaseAttributeChanged == null)
				{
					return;
				}
				onBaseAttributeChanged();
			}
		}

		// Token: 0x170003DD RID: 989
		// (get) Token: 0x06001317 RID: 4887 RVA: 0x0005CD0D File Offset: 0x0005AF0D
		// (set) Token: 0x06001318 RID: 4888 RVA: 0x0005CD15 File Offset: 0x0005AF15
		internal NpcEnum.FallMode BaseFallMode
		{
			get
			{
				return this.baseFallMode;
			}
			set
			{
				if (this.baseFallMode == value)
				{
					return;
				}
				this.baseFallMode = value;
				Action onBaseAttributeChanged = this.OnBaseAttributeChanged;
				if (onBaseAttributeChanged == null)
				{
					return;
				}
				onBaseAttributeChanged();
			}
		}

		// Token: 0x170003DE RID: 990
		// (get) Token: 0x06001319 RID: 4889 RVA: 0x0005CD38 File Offset: 0x0005AF38
		// (set) Token: 0x0600131A RID: 4890 RVA: 0x0005CD40 File Offset: 0x0005AF40
		internal MovementMode BaseMovementMode
		{
			get
			{
				return this.baseMovementMode;
			}
			set
			{
				if (this.baseMovementMode == value)
				{
					return;
				}
				this.baseMovementMode = value;
				Action onBaseAttributeChanged = this.OnBaseAttributeChanged;
				if (onBaseAttributeChanged == null)
				{
					return;
				}
				onBaseAttributeChanged();
			}
		}

		// Token: 0x0600131B RID: 4891 RVA: 0x0005CD63 File Offset: 0x0005AF63
		private void Awake()
		{
			this.WorldObject.EndlessVisuals.FadeOnStart = false;
			this.Components.InitializeVisuals();
		}

		// Token: 0x0600131C RID: 4892 RVA: 0x0005CD81 File Offset: 0x0005AF81
		protected override void Start()
		{
			base.Start();
			if (!this.haveSpawnedVisuals)
			{
				this.haveSpawnedVisuals = true;
				this.Components.NpcVisuals.UpdateVisuals(MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultCharacterCosmetics.Cosmetics[7]);
			}
		}

		// Token: 0x170003DF RID: 991
		// (get) Token: 0x0600131D RID: 4893 RVA: 0x0005CDBD File Offset: 0x0005AFBD
		public CombatMode CombatMode
		{
			get
			{
				return this.Components.DynamicAttributes.CombatMode;
			}
		}

		// Token: 0x170003E0 RID: 992
		// (get) Token: 0x0600131E RID: 4894 RVA: 0x0005CDCF File Offset: 0x0005AFCF
		public DamageMode DamageMode
		{
			get
			{
				return this.Components.DynamicAttributes.DamageMode;
			}
		}

		// Token: 0x170003E1 RID: 993
		// (get) Token: 0x0600131F RID: 4895 RVA: 0x0005CDE1 File Offset: 0x0005AFE1
		public PhysicsMode PhysicsMode
		{
			get
			{
				return this.Components.DynamicAttributes.PhysicsMode;
			}
		}

		// Token: 0x170003E2 RID: 994
		// (get) Token: 0x06001320 RID: 4896 RVA: 0x0005CDF3 File Offset: 0x0005AFF3
		public NpcEnum.FallMode FallMode
		{
			get
			{
				return this.Components.DynamicAttributes.FallMode;
			}
		}

		// Token: 0x170003E3 RID: 995
		// (get) Token: 0x06001321 RID: 4897 RVA: 0x0005CE05 File Offset: 0x0005B005
		public MovementMode MovementMode
		{
			get
			{
				return this.Components.DynamicAttributes.MovementMode;
			}
		}

		// Token: 0x170003E4 RID: 996
		// (get) Token: 0x06001322 RID: 4898 RVA: 0x0005CE17 File Offset: 0x0005B017
		public float CloseDistance
		{
			get
			{
				return this.NpcBlackboard.GetValueOrDefault<float>(NpcBlackboard.Key.CloseDistance, 1f);
			}
		}

		// Token: 0x170003E5 RID: 997
		// (get) Token: 0x06001323 RID: 4899 RVA: 0x0005CE2A File Offset: 0x0005B02A
		public float OptimalAttackDistance
		{
			get
			{
				return this.NpcBlackboard.GetValueOrDefault<float>(NpcBlackboard.Key.OptimalAttackDistance, 1.25f);
			}
		}

		// Token: 0x170003E6 RID: 998
		// (get) Token: 0x06001324 RID: 4900 RVA: 0x0005CE3D File Offset: 0x0005B03D
		public float MeleeDistance
		{
			get
			{
				return this.NpcBlackboard.GetValueOrDefault<float>(NpcBlackboard.Key.MeleeDistance, 1.5f);
			}
		}

		// Token: 0x170003E7 RID: 999
		// (get) Token: 0x06001325 RID: 4901 RVA: 0x0005CE50 File Offset: 0x0005B050
		public float NearDistance
		{
			get
			{
				return this.NpcBlackboard.GetValueOrDefault<float>(NpcBlackboard.Key.NearDistance, 2f);
			}
		}

		// Token: 0x170003E8 RID: 1000
		// (get) Token: 0x06001326 RID: 4902 RVA: 0x0005CE63 File Offset: 0x0005B063
		public float AroundDistance
		{
			get
			{
				return this.NpcBlackboard.GetValueOrDefault<float>(NpcBlackboard.Key.AroundDistance, 4f);
			}
		}

		// Token: 0x170003E9 RID: 1001
		// (get) Token: 0x06001327 RID: 4903 RVA: 0x0005CE77 File Offset: 0x0005B077
		public float MaxRangedAttackDistance
		{
			get
			{
				return this.NpcBlackboard.GetValueOrDefault<float>(NpcBlackboard.Key.MaxRangedAttackDistance, 60f);
			}
		}

		// Token: 0x170003EA RID: 1002
		// (get) Token: 0x06001328 RID: 4904 RVA: 0x0005CE8C File Offset: 0x0005B08C
		public bool CanFidget
		{
			get
			{
				bool flag;
				return !this.NpcBlackboard.TryGet<bool>(NpcBlackboard.Key.CanFidget, out flag) || flag;
			}
		}

		// Token: 0x170003EB RID: 1003
		// (get) Token: 0x06001329 RID: 4905 RVA: 0x0005CEAC File Offset: 0x0005B0AC
		public float DestinationTolerance
		{
			get
			{
				float num;
				if (this.NpcBlackboard.TryGet<float>(NpcBlackboard.Key.DestinationTolerance, out num))
				{
					return num;
				}
				return 1f;
			}
		}

		// Token: 0x170003EC RID: 1004
		// (get) Token: 0x0600132A RID: 4906 RVA: 0x0005CED1 File Offset: 0x0005B0D1
		public bool IsConfigured
		{
			get
			{
				return this.configuration != null;
			}
		}

		// Token: 0x170003ED RID: 1005
		// (get) Token: 0x0600132B RID: 4907 RVA: 0x0005CEDF File Offset: 0x0005B0DF
		// (set) Token: 0x0600132C RID: 4908 RVA: 0x0005CEE7 File Offset: 0x0005B0E7
		public bool LostTarget { get; set; }

		// Token: 0x0600132D RID: 4909 RVA: 0x0005CEF0 File Offset: 0x0005B0F0
		internal void Teleport(TeleportType teleportType, global::UnityEngine.Vector3 position, float rotation)
		{
			this.NpcBlackboard.Set<int>(NpcBlackboard.Key.TeleportType, (int)teleportType);
			this.NpcBlackboard.Set<global::UnityEngine.Vector3>(NpcBlackboard.Key.TeleportPosition, position);
			this.NpcBlackboard.Set<float>(NpcBlackboard.Key.TeleportRotation, rotation);
			this.Components.Parameters.TeleportTrigger = true;
		}

		// Token: 0x0600132E RID: 4910 RVA: 0x0005CF30 File Offset: 0x0005B130
		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
		{
			if (serializer.IsWriter && this.configuration != null)
			{
				FastBufferWriter fastBufferWriter = serializer.GetFastBufferWriter();
				NetworkableNpcConfig networkableNpcConfig = new NetworkableNpcConfig(this.configuration);
				fastBufferWriter.WriteValueSafe<NetworkableNpcConfig>(in networkableNpcConfig, default(FastBufferWriter.ForNetworkSerializable));
			}
			if (serializer.IsReader)
			{
				NetworkableNpcConfig networkableNpcConfig2;
				serializer.GetFastBufferReader().ReadValueSafe<NetworkableNpcConfig>(out networkableNpcConfig2, default(FastBufferWriter.ForNetworkSerializable));
				if (networkableNpcConfig2 != null)
				{
					this.configuration = new NpcConfiguration(networkableNpcConfig2);
					this.UpdateNpcSettings(this.configuration);
				}
			}
			base.OnSynchronize<T>(ref serializer);
		}

		// Token: 0x0600132F RID: 4911 RVA: 0x0005CFB6 File Offset: 0x0005B1B6
		public override void OnDestroy()
		{
			base.OnDestroy();
			if (MonoBehaviourSingleton<NpcManager>.Instance)
			{
				MonoBehaviourSingleton<NpcManager>.Instance.RemoveNpc(this);
			}
			if (MonoBehaviourSingleton<CombatManager>.Instance)
			{
				MonoBehaviourSingleton<CombatManager>.Instance.UnregisterCombatNpc(this);
			}
		}

		// Token: 0x06001330 RID: 4912 RVA: 0x0005CFEC File Offset: 0x0005B1EC
		public void StopInteraction(Context context)
		{
			IInteractionBehavior interactionBehavior = this.InteractionBehavior;
			if (interactionBehavior == null)
			{
				return;
			}
			interactionBehavior.InteractionStopped(this.Context, context);
		}

		// Token: 0x06001331 RID: 4913 RVA: 0x0005D008 File Offset: 0x0005B208
		public void Despawn()
		{
			Action onNpcDespawned = this.OnNpcDespawned;
			if (onNpcDespawned != null)
			{
				onNpcDespawned();
			}
			MonoBehaviourSingleton<NpcManager>.Instance.RemoveNpc(this);
			MonoBehaviourSingleton<TargetingManager>.Instance.RemoveTargetable(this.Components.HittableComponent);
			if (base.IsServer)
			{
				this.RaiseOnDiedEvent();
				global::UnityEngine.Object.Destroy(this.WorldObject.gameObject);
			}
		}

		// Token: 0x06001332 RID: 4914 RVA: 0x0005D064 File Offset: 0x0005B264
		public void Hit(Knockback knockback)
		{
			if (base.IsServer && this.PhysicsMode == PhysicsMode.TakePhysics)
			{
				this.Components.PhysicsTaker.TakePhysicsForce(knockback.Force, PlayerController.AngleToForwardMotion(knockback.Angle).normalized, knockback.StartFrame, knockback.Source, false, false, false);
			}
		}

		// Token: 0x06001333 RID: 4915 RVA: 0x0005D0BC File Offset: 0x0005B2BC
		public void RaiseOnModifiedObjectHealthEvent(HealthChangeResult result, Context hitContext)
		{
			NpcEntity.<>c__DisplayClass198_0 CS$<>8__locals1 = new NpcEntity.<>c__DisplayClass198_0();
			CS$<>8__locals1.<>4__this = this;
			if (!base.NetworkManager.IsServer)
			{
				return;
			}
			if (result == HealthChangeResult.HealthZeroed)
			{
				EndlessEvent<Context> onEntityHealthZeroed = this.OnEntityHealthZeroed;
				if (onEntityHealthZeroed != null)
				{
					onEntityHealthZeroed.Invoke(this.Context, hitContext);
				}
				if (this.Target != null && hitContext.WorldObject == this.Target.WorldObject)
				{
					EndlessEvent<Context> onTargetHealthZeroed = this.OnTargetHealthZeroed;
					if (onTargetHealthZeroed != null)
					{
						onTargetHealthZeroed.Invoke(this.Context, this.Target.WorldObject.Context);
					}
				}
				ZombieNpcCustomizationData zombieNpcCustomizationData = this.npcClass as ZombieNpcCustomizationData;
				if (zombieNpcCustomizationData != null && zombieNpcCustomizationData.ZombifyTarget && this.Team != Team.None && hitContext.IsNpc())
				{
					CS$<>8__locals1.entity = hitContext.WorldObject.GetUserComponent<NpcEntity>();
					if (CS$<>8__locals1.entity.npcClass.NpcClass != Endless.Gameplay.LuaEnums.NpcClass.Zombie)
					{
						NpcEntity entity = CS$<>8__locals1.entity;
						entity.OnNpcDespawned = (Action)Delegate.Combine(entity.OnNpcDespawned, new Action(CS$<>8__locals1.<RaiseOnModifiedObjectHealthEvent>g__HandleOnNpcDespawned|0));
					}
				}
			}
			HittableComponent hittableComponent;
			if (result == HealthChangeResult.NoChange && hitContext.WorldObject.TryGetUserComponent<HittableComponent>(out hittableComponent))
			{
				float num;
				if (this.scoreReductionDictionary.TryGetValue(hittableComponent, out num))
				{
					if (num < 0.5f)
					{
						Dictionary<HittableComponent, float> dictionary = this.scoreReductionDictionary;
						HittableComponent hittableComponent2 = hittableComponent;
						dictionary[hittableComponent2] += 0.1f;
					}
				}
				else
				{
					this.scoreReductionDictionary[hittableComponent] = 0.1f;
					base.StartCoroutine(this.DiminishScoreReductionRoutine(hittableComponent, 3f, 0.1f));
				}
			}
			object[] array;
			this.scriptComponent.TryExecuteFunction("OnModifiedObjectHealth", out array, new object[]
			{
				hitContext,
				(int)result
			});
		}

		// Token: 0x06001334 RID: 4916 RVA: 0x0005D261 File Offset: 0x0005B461
		private IEnumerator DiminishScoreReductionRoutine(HittableComponent hittableComponent, float reductionDelay, float reductionAmount)
		{
			if (!this.scoreReductionDictionary.ContainsKey(hittableComponent))
			{
				yield break;
			}
			for (;;)
			{
				yield return new WaitForSeconds(reductionDelay);
				if (!hittableComponent)
				{
					break;
				}
				Dictionary<HittableComponent, float> dictionary = this.scoreReductionDictionary;
				dictionary[hittableComponent] -= reductionAmount;
				if (this.scoreReductionDictionary[hittableComponent] <= 0f)
				{
					goto Block_3;
				}
			}
			yield break;
			Block_3:
			this.scoreReductionDictionary.Remove(hittableComponent);
			yield break;
		}

		// Token: 0x06001335 RID: 4917 RVA: 0x0005D288 File Offset: 0x0005B488
		private void RaiseOnDiedEvent()
		{
			if (base.NetworkManager.IsServer)
			{
				object[] array;
				this.scriptComponent.TryExecuteFunction("OnNpcDied", out array, Array.Empty<object>());
			}
		}

		// Token: 0x06001336 RID: 4918 RVA: 0x0005D2BA File Offset: 0x0005B4BA
		public void BindInstantiatedWorldObject()
		{
			this.WorldObject = base.GetComponent<WorldObject>();
		}

		// Token: 0x14000027 RID: 39
		// (add) Token: 0x06001337 RID: 4919 RVA: 0x0005D2C8 File Offset: 0x0005B4C8
		// (remove) Token: 0x06001338 RID: 4920 RVA: 0x0005D300 File Offset: 0x0005B500
		public event Action OnAttributeSourceChanged;

		// Token: 0x170003EE RID: 1006
		// (get) Token: 0x06001339 RID: 4921 RVA: 0x0005D335 File Offset: 0x0005B535
		// (set) Token: 0x0600133A RID: 4922 RVA: 0x0005D33D File Offset: 0x0005B53D
		public IInteractionBehavior InteractionBehavior
		{
			get
			{
				return this.interactionBehavior;
			}
			private set
			{
				if (this.interactionBehavior == value)
				{
					return;
				}
				this.interactionBehavior = value;
				Action onAttributeSourceChanged = this.OnAttributeSourceChanged;
				if (onAttributeSourceChanged == null)
				{
					return;
				}
				onAttributeSourceChanged();
			}
		}

		// Token: 0x0600133B RID: 4923 RVA: 0x0005D360 File Offset: 0x0005B560
		public void SetInteractionBehavior(IInteractionBehavior newInteractionBehavior)
		{
			IInteractionBehavior interactionBehavior = this.InteractionBehavior;
			if (interactionBehavior != null)
			{
				interactionBehavior.RescindInstruction(this.Context);
			}
			this.InteractionBehavior = newInteractionBehavior;
		}

		// Token: 0x0600133C RID: 4924 RVA: 0x0005D380 File Offset: 0x0005B580
		public void ClearInteractionBehavior()
		{
			this.InteractionBehavior = null;
		}

		// Token: 0x0600133D RID: 4925 RVA: 0x0005D389 File Offset: 0x0005B589
		public List<INpcAttributeModifier> GetAttributeModifiers()
		{
			return new List<INpcAttributeModifier> { this.InteractionBehavior };
		}

		// Token: 0x0600133E RID: 4926 RVA: 0x0005D39C File Offset: 0x0005B59C
		public void EndlessAwake()
		{
			if (!base.IsServer)
			{
				this.SetupClientNpc();
				this.Components.InitializeComponents();
			}
			else
			{
				this.Components.InitializeComponents();
				this.Components.DynamicAttributes.OnDamageModeChanged += this.HandleOnDamageModeChanged;
				this.Components.TargeterComponent.TargetScoreModifier = new Func<HittableComponent, float, float>(this.TargetScoreModifier);
				this.Components.Health.OnHealthZeroed_Internal.AddListener(delegate
				{
					MonoBehaviourSingleton<StageManager>.Instance.PropDestroyed(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(this.WorldObject.gameObject));
				});
				this.Components.Health.OnHealthZeroed_Internal.AddListener(delegate
				{
					this.downedFrame = NetClock.CurrentFrame;
				});
				if (this.persistenceData == null)
				{
					this.persistenceData = new NpcEntity.PersistenceData(global::UnityEngine.Vector3.zero, 0f, this.Position, this.WorldObject.transform.rotation, new List<Vector3Int>());
				}
				else
				{
					if (this.Health <= 0)
					{
						global::UnityEngine.Object.Destroy(this);
						return;
					}
					this.Position = this.persistenceData.MostRecentLocation;
					foreach (Vector3Int vector3Int in this.persistenceData.CurrentNodes)
					{
						MonoBehaviourSingleton<NodeMap>.Instance.InstructionNodesByCellPosition[vector3Int].GiveInstruction(this.Context);
					}
					base.transform.rotation = Quaternion.Euler(new global::UnityEngine.Vector3(0f, this.persistenceData.MostRecentRotation, 0f));
				}
				if (!this.isDynamicSpawn)
				{
					this.Components.Health.SetMaxHealth(this.Context, this.StartingHealth);
					this.configuration = new NpcConfiguration(this);
					this.BaseCombatMode = (CombatMode)this.combatMode;
					this.BaseDamageMode = (DamageMode)this.damageMode;
					this.BasePhysicsMode = (PhysicsMode)this.physicsMode;
					this.BaseFallMode = (NpcEnum.FallMode)this.fallMode;
					this.BaseMovementMode = (MovementMode)this.movementMode;
				}
				else
				{
					this.Components.TextBubble.ShouldSaveAndLoad = false;
					this.scriptComponent.ShouldSaveAndLoad = false;
				}
				if (this.SpawnAnimation == NpcSpawnAnimation.None)
				{
					this.FinishedSpawnAnimation = true;
				}
				this.SetClassBlackboardValues();
				MonoBehaviourSingleton<NpcManager>.Instance.RegisterNewNpc(this);
				FsmBuilder.BuildFsm(this);
				this.Components.Animator.SetBool(NpcAnimator.Walking, this.MovementMode == MovementMode.Walk);
				this.Components.DynamicAttributes.OnMovementModeChanged += delegate
				{
					this.Components.Animator.SetBool(NpcAnimator.Walking, this.MovementMode == MovementMode.Walk);
				};
				this.Components.DynamicAttributes.OnCombatModeChanged += this.HandleOnCombatModeChanged;
				this.Components.TargeterComponent.OnTargetChanged += this.HandleOnTargetChanged;
				this.Components.IndividualStateUpdater.OnTickAi += this.HandleOnTickAi;
				if (this.npcClass.NpcClass != Endless.Gameplay.LuaEnums.NpcClass.Blank)
				{
					if (this.combatMode == PropCombatMode.Passive)
					{
						MonoBehaviourSingleton<CombatManager>.Instance.UnregisterCombatNpc(this);
					}
					else
					{
						MonoBehaviourSingleton<CombatManager>.Instance.RegisterCombatNpc(this);
					}
				}
			}
			if (MatchSession.Instance.MatchData.IsEditSession && NetworkManager.Singleton.IsHost)
			{
				return;
			}
			if (!EndlessCloudService.CanHaveRiflemen() && this.NpcClass.NpcClass == Endless.Gameplay.LuaEnums.NpcClass.Rifleman)
			{
				ErrorHandler.HandleError(ErrorCodes.NpcEntity_EndlessAwake_ContentRestricted_Rifleman, new Exception("Content Restricted: Account is not allowed to have riflemen."), true, true);
			}
			if (!EndlessCloudService.CanHaveGrunt() && this.NpcClass.NpcClass == Endless.Gameplay.LuaEnums.NpcClass.Grunt)
			{
				ErrorHandler.HandleError(ErrorCodes.NpcEntity_EndlessAwake_ContentRestricted_Grunt, new Exception("Content Restricted: Account is not allowed to have grunts."), true, true);
			}
			if (!EndlessCloudService.CanHaveZombies() && this.NpcClass.NpcClass == Endless.Gameplay.LuaEnums.NpcClass.Zombie)
			{
				ErrorHandler.HandleError(ErrorCodes.NpcEntity_EndlessAwake_ContentRestricted_Zombie, new Exception("Content Restricted: Account is not allowed to have zombies."), true, true);
			}
		}

		// Token: 0x0600133F RID: 4927 RVA: 0x0005D744 File Offset: 0x0005B944
		private void HandleOnCombatModeChanged()
		{
			if (this.npcClass.NpcClass == Endless.Gameplay.LuaEnums.NpcClass.Blank)
			{
				return;
			}
			if (this.combatMode == PropCombatMode.Passive)
			{
				MonoBehaviourSingleton<CombatManager>.Instance.UnregisterCombatNpc(this);
				return;
			}
			MonoBehaviourSingleton<CombatManager>.Instance.RegisterCombatNpc(this);
		}

		// Token: 0x06001340 RID: 4928 RVA: 0x0005D773 File Offset: 0x0005B973
		private void HandleOnTickAi()
		{
			if (this.Target)
			{
				this.LastKnownTargetLocation = this.Target.NavPosition;
			}
			if (this.Health <= 0 && NetClock.CurrentFrame > this.downedFrame + 100U)
			{
				this.Despawn();
			}
		}

		// Token: 0x06001341 RID: 4929 RVA: 0x0005D7B4 File Offset: 0x0005B9B4
		private void HandleOnTargetChanged(HittableComponent newTarget)
		{
			HealthComponent healthComponent;
			if (!newTarget && this.Components.TargeterComponent.LastTarget.WorldObject.TryGetUserComponent<HealthComponent>(out healthComponent) && healthComponent.CurrentHealth > 0)
			{
				this.LostTarget = true;
			}
		}

		// Token: 0x06001342 RID: 4930 RVA: 0x0005D7F8 File Offset: 0x0005B9F8
		private float TargetScoreModifier(HittableComponent target, float currentScore)
		{
			float num;
			if (!this.scoreReductionDictionary.TryGetValue(target, out num))
			{
				return currentScore;
			}
			return currentScore * (1f - num);
		}

		// Token: 0x06001343 RID: 4931 RVA: 0x0005D820 File Offset: 0x0005BA20
		private void SetupClientNpc()
		{
			if (!this.IsConfigured)
			{
				this.configuration = new NpcConfiguration(this);
				this.BaseCombatMode = (CombatMode)this.combatMode;
				this.BaseDamageMode = (DamageMode)this.damageMode;
				this.BasePhysicsMode = (PhysicsMode)this.physicsMode;
				this.BaseFallMode = (NpcEnum.FallMode)this.fallMode;
				this.BaseMovementMode = (MovementMode)this.movementMode;
			}
		}

		// Token: 0x06001344 RID: 4932 RVA: 0x0005D880 File Offset: 0x0005BA80
		private void SetClassBlackboardValues()
		{
			switch (this.NpcClass.NpcClass)
			{
			case Endless.Gameplay.LuaEnums.NpcClass.Blank:
			case Endless.Gameplay.LuaEnums.NpcClass.Grunt:
			case Endless.Gameplay.LuaEnums.NpcClass.Zombie:
				return;
			case Endless.Gameplay.LuaEnums.NpcClass.Rifleman:
				this.NpcBlackboard.Set<float>(NpcBlackboard.Key.AroundDistance, 80f);
				this.NpcBlackboard.Set<float>(NpcBlackboard.Key.MeleeDistance, 80f);
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x06001345 RID: 4933 RVA: 0x0005D8DC File Offset: 0x0005BADC
		[ClientRpc]
		public void ConfigureSpawnedNpc_ClientRpc(NetworkableNpcConfig config, ClientRpcParams clientRpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1532021543U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = config != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<NetworkableNpcConfig>(in config, default(FastBufferWriter.ForNetworkSerializable));
				}
				base.__endSendClientRpc(ref fastBufferWriter, 1532021543U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.configuration = new NpcConfiguration(config);
			this.UpdateNpcSettings(this.configuration);
			this.isDynamicSpawn = true;
			if (base.IsServer)
			{
				this.Components.Health.SetMaxHealth(null, config.Health);
			}
			this.WorldObject.GetUserComponent<TeamComponent>().Team = (Team)config.Team;
		}

		// Token: 0x06001346 RID: 4934 RVA: 0x0005DA50 File Offset: 0x0005BC50
		private void UpdateNpcSettings(NpcConfiguration config)
		{
			this.CharacterVisuals = config.NpcVisuals;
			if (base.IsServer)
			{
				this.Components.Health.SetMaxHealth(this.Context, this.StartingHealth);
			}
			this.npcClass = this.configuration.NpcClass;
			this.BaseCombatMode = (CombatMode)config.CombatMode;
			this.BaseDamageMode = (DamageMode)config.DamageMode;
			this.BaseMovementMode = (MovementMode)config.MovementMode;
			this.BasePhysicsMode = (PhysicsMode)config.PhysicsMode;
			this.IdleBehavior = (IdleBehavior)config.IdleBehavior;
			this.PathfindingRange = (PathfindingRange)config.PathfindingRange;
			this.SpawnAnimation = (NpcSpawnAnimation)config.SpawnAnimation;
			if (this.SpawnAnimation == NpcSpawnAnimation.None)
			{
				this.FinishedSpawnAnimation = true;
			}
			this.Group = (NpcGroup)config.Group;
		}

		// Token: 0x06001347 RID: 4935 RVA: 0x0005DB0D File Offset: 0x0005BD0D
		private void HandleOnDamageModeChanged()
		{
			this.NetworkedDamageMode.Value = this.DamageMode;
			this.Components.HittableComponent.IsDamageable = this.DamageMode == DamageMode.TakeDamage;
		}

		// Token: 0x06001348 RID: 4936 RVA: 0x0005DB39 File Offset: 0x0005BD39
		public void EndlessGameEnd()
		{
			MonoBehaviourSingleton<CombatManager>.Instance.UnregisterCombatNpc(this);
		}

		// Token: 0x170003EF RID: 1007
		// (get) Token: 0x06001349 RID: 4937 RVA: 0x0005DB46 File Offset: 0x0005BD46
		public bool ShouldSaveAndLoad
		{
			get
			{
				return !this.isDynamicSpawn;
			}
		}

		// Token: 0x0600134A RID: 4938 RVA: 0x0005DB54 File Offset: 0x0005BD54
		public object GetSaveState()
		{
			Transform transform = base.transform;
			List<Vector3Int> list = new List<Vector3Int>();
			IInteractionBehavior interactionBehavior = this.InteractionBehavior;
			Endless.Gameplay.Scripting.InstructionNode instructionNode = ((interactionBehavior != null) ? interactionBehavior.GetNode() : null);
			if (instructionNode != null)
			{
				list.Add(instructionNode.CellPosition);
			}
			IGoapNode currentBehaviorNode = this.Components.GoapController.CurrentBehaviorNode;
			Endless.Gameplay.Scripting.InstructionNode instructionNode2 = ((currentBehaviorNode != null) ? currentBehaviorNode.GetNode() : null);
			if (instructionNode2 != null)
			{
				list.Add(instructionNode2.CellPosition);
			}
			IGoapNode currentCommandNode = this.Components.GoapController.CurrentCommandNode;
			Endless.Gameplay.Scripting.InstructionNode instructionNode3 = ((currentCommandNode != null) ? currentCommandNode.GetNode() : null);
			if (instructionNode3 != null)
			{
				list.Add(instructionNode3.CellPosition);
			}
			return new NpcEntity.PersistenceData(transform.position, transform.rotation.eulerAngles.y, this.persistenceData.InitialLocation, this.persistenceData.InitialRotation, list);
		}

		// Token: 0x0600134B RID: 4939 RVA: 0x0005DC21 File Offset: 0x0005BE21
		public void LoadState(object loadedState)
		{
			if (base.IsServer && loadedState != null)
			{
				this.persistenceData = loadedState as NpcEntity.PersistenceData;
			}
		}

		// Token: 0x170003F0 RID: 1008
		// (get) Token: 0x0600134C RID: 4940 RVA: 0x0005DC3A File Offset: 0x0005BE3A
		// (set) Token: 0x0600134D RID: 4941 RVA: 0x0005DC42 File Offset: 0x0005BE42
		public WorldObject WorldObject { get; private set; }

		// Token: 0x170003F1 RID: 1009
		// (get) Token: 0x0600134E RID: 4942 RVA: 0x0005DC4B File Offset: 0x0005BE4B
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(NpcEntity);
			}
		}

		// Token: 0x170003F2 RID: 1010
		// (get) Token: 0x0600134F RID: 4943 RVA: 0x0003FE71 File Offset: 0x0003E071
		public ReferenceFilter Filter
		{
			get
			{
				return ReferenceFilter.NonStatic | ReferenceFilter.Npc;
			}
		}

		// Token: 0x170003F3 RID: 1011
		// (get) Token: 0x06001350 RID: 4944 RVA: 0x0005DC58 File Offset: 0x0005BE58
		public Context Context
		{
			get
			{
				Context context;
				if ((context = this.context) == null)
				{
					context = (this.context = new Context(this.WorldObject));
				}
				return context;
			}
		}

		// Token: 0x170003F4 RID: 1012
		// (get) Token: 0x06001351 RID: 4945 RVA: 0x0001BD04 File Offset: 0x00019F04
		public NavType NavValue
		{
			get
			{
				return NavType.Intangible;
			}
		}

		// Token: 0x06001352 RID: 4946 RVA: 0x0005DC83 File Offset: 0x0005BE83
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
			worldObject.GetUserComponent<HealthComponent>().SetHealthZeroedBehaviour(HealthZeroedBehavior.Custom);
		}

		// Token: 0x170003F5 RID: 1013
		// (get) Token: 0x06001353 RID: 4947 RVA: 0x0005DC98 File Offset: 0x0005BE98
		public object LuaObject
		{
			get
			{
				Npc npc;
				if ((npc = this.luaInterface) == null)
				{
					npc = (this.luaInterface = new Npc(this));
				}
				return npc;
			}
		}

		// Token: 0x170003F6 RID: 1014
		// (get) Token: 0x06001354 RID: 4948 RVA: 0x0005DCBE File Offset: 0x0005BEBE
		public Type LuaObjectType
		{
			get
			{
				return typeof(Npc);
			}
		}

		// Token: 0x06001355 RID: 4949 RVA: 0x0005DCCA File Offset: 0x0005BECA
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x06001356 RID: 4950 RVA: 0x0005DCD4 File Offset: 0x0005BED4
		[ClientRpc]
		public void DownedClientRpc()
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3594010860U, clientRpcParams, RpcDelivery.Reliable);
				base.__endSendClientRpc(ref fastBufferWriter, 3594010860U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (base.NetworkManager.IsHost)
			{
				return;
			}
			this.Components.Animator.SetTrigger(NpcAnimator.Downed);
			this.Components.Animator.SetBool(NpcAnimator.Dbno, true);
		}

		// Token: 0x06001357 RID: 4951 RVA: 0x0005DDE0 File Offset: 0x0005BFE0
		[ClientRpc]
		public void ExplosionsOnlyClientRpc()
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2749909393U, clientRpcParams, RpcDelivery.Reliable);
				base.__endSendClientRpc(ref fastBufferWriter, 2749909393U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			foreach (Collider collider in this.WorldObject.GetUserComponent<HittableComponent>().HittableColliders)
			{
				collider.gameObject.layer = LayerMask.NameToLayer("ExplosionsOnly");
			}
		}

		// Token: 0x06001358 RID: 4952 RVA: 0x0005DF10 File Offset: 0x0005C110
		[ClientRpc]
		public void EnqueueMeleeAttackClientRpc(uint frame, int duration, int meleeAttackIndex)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(633007624U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, frame);
				BytePacker.WriteValueBitPacked(fastBufferWriter, duration);
				BytePacker.WriteValueBitPacked(fastBufferWriter, meleeAttackIndex);
				base.__endSendClientRpc(ref fastBufferWriter, 633007624U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			((MeleeAttackComponent)this.Components.Attack).EnqueueMeleeAttack(frame, duration, meleeAttackIndex);
		}

		// Token: 0x06001359 RID: 4953 RVA: 0x0005E024 File Offset: 0x0005C224
		[ClientRpc]
		public void StartTeleportClientRpc(TeleportType teleportType)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1056222869U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<TeleportType>(in teleportType, default(FastBufferWriter.ForEnums));
				base.__endSendClientRpc(ref fastBufferWriter, 1056222869U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			RuntimeDatabase.GetTeleportInfo(teleportType).TeleportStart(this.WorldObject.EndlessVisuals, this.Components.Animator, this.Position);
		}

		// Token: 0x0600135A RID: 4954 RVA: 0x0005E13C File Offset: 0x0005C33C
		[ClientRpc]
		public void EndTeleportClientRpc(TeleportType teleportType)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1460646776U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<TeleportType>(in teleportType, default(FastBufferWriter.ForEnums));
				base.__endSendClientRpc(ref fastBufferWriter, 1460646776U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			RuntimeDatabase.GetTeleportInfo(teleportType).TeleportEnd(this.WorldObject.EndlessVisuals, this.Components.Animator, this.Position);
		}

		// Token: 0x0600135F RID: 4959 RVA: 0x0005E31C File Offset: 0x0005C51C
		protected override void __initializeVariables()
		{
			bool flag = this.NetworkedDamageMode == null;
			if (flag)
			{
				throw new Exception("NpcEntity.NetworkedDamageMode cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.NetworkedDamageMode.Initialize(this);
			base.__nameNetworkVariable(this.NetworkedDamageMode, "NetworkedDamageMode");
			this.NetworkVariableFields.Add(this.NetworkedDamageMode);
			flag = this.NetworkedPhysicsMode == null;
			if (flag)
			{
				throw new Exception("NpcEntity.NetworkedPhysicsMode cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.NetworkedPhysicsMode.Initialize(this);
			base.__nameNetworkVariable(this.NetworkedPhysicsMode, "NetworkedPhysicsMode");
			this.NetworkVariableFields.Add(this.NetworkedPhysicsMode);
			base.__initializeVariables();
		}

		// Token: 0x06001360 RID: 4960 RVA: 0x0005E3CC File Offset: 0x0005C5CC
		protected override void __initializeRpcs()
		{
			base.__registerRpc(1532021543U, new NetworkBehaviour.RpcReceiveHandler(NpcEntity.__rpc_handler_1532021543), "ConfigureSpawnedNpc_ClientRpc");
			base.__registerRpc(3594010860U, new NetworkBehaviour.RpcReceiveHandler(NpcEntity.__rpc_handler_3594010860), "DownedClientRpc");
			base.__registerRpc(2749909393U, new NetworkBehaviour.RpcReceiveHandler(NpcEntity.__rpc_handler_2749909393), "ExplosionsOnlyClientRpc");
			base.__registerRpc(633007624U, new NetworkBehaviour.RpcReceiveHandler(NpcEntity.__rpc_handler_633007624), "EnqueueMeleeAttackClientRpc");
			base.__registerRpc(1056222869U, new NetworkBehaviour.RpcReceiveHandler(NpcEntity.__rpc_handler_1056222869), "StartTeleportClientRpc");
			base.__registerRpc(1460646776U, new NetworkBehaviour.RpcReceiveHandler(NpcEntity.__rpc_handler_1460646776), "EndTeleportClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06001361 RID: 4961 RVA: 0x0005E48C File Offset: 0x0005C68C
		private static void __rpc_handler_1532021543(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			NetworkableNpcConfig networkableNpcConfig = null;
			if (flag)
			{
				reader.ReadValueSafe<NetworkableNpcConfig>(out networkableNpcConfig, default(FastBufferWriter.ForNetworkSerializable));
			}
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((NpcEntity)target).ConfigureSpawnedNpc_ClientRpc(networkableNpcConfig, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001362 RID: 4962 RVA: 0x0005E534 File Offset: 0x0005C734
		private static void __rpc_handler_3594010860(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((NpcEntity)target).DownedClientRpc();
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001363 RID: 4963 RVA: 0x0005E588 File Offset: 0x0005C788
		private static void __rpc_handler_2749909393(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((NpcEntity)target).ExplosionsOnlyClientRpc();
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001364 RID: 4964 RVA: 0x0005E5DC File Offset: 0x0005C7DC
		private static void __rpc_handler_633007624(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			uint num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			int num2;
			ByteUnpacker.ReadValueBitPacked(reader, out num2);
			int num3;
			ByteUnpacker.ReadValueBitPacked(reader, out num3);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((NpcEntity)target).EnqueueMeleeAttackClientRpc(num, num2, num3);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001365 RID: 4965 RVA: 0x0005E660 File Offset: 0x0005C860
		private static void __rpc_handler_1056222869(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			TeleportType teleportType;
			reader.ReadValueSafe<TeleportType>(out teleportType, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((NpcEntity)target).StartTeleportClientRpc(teleportType);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001366 RID: 4966 RVA: 0x0005E6D0 File Offset: 0x0005C8D0
		private static void __rpc_handler_1460646776(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			TeleportType teleportType;
			reader.ReadValueSafe<TeleportType>(out teleportType, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((NpcEntity)target).EndTeleportClientRpc(teleportType);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001367 RID: 4967 RVA: 0x0005E740 File Offset: 0x0005C940
		protected internal override string __getTypeName()
		{
			return "NpcEntity";
		}

		// Token: 0x04001003 RID: 4099
		public const float DISTANCE_TOLERANCE = 0.2f;

		// Token: 0x04001004 RID: 4100
		public const uint MAX_DOWN_FRAMES = 100U;

		// Token: 0x04001005 RID: 4101
		public const uint EXTRAPOLATION_FRAMES = 8U;

		// Token: 0x04001006 RID: 4102
		public const float OPTIMAL_ATTACK_DISTANCE = 1.25f;

		// Token: 0x04001007 RID: 4103
		public const float MAX_RANGED_ATTACK_DISTANCE = 60f;

		// Token: 0x04001008 RID: 4104
		private const float CLOSE_DISTANCE = 1f;

		// Token: 0x04001009 RID: 4105
		private const float MELEE_DISTANCE = 1.5f;

		// Token: 0x0400100A RID: 4106
		private const float NEAR_DISTANCE = 2f;

		// Token: 0x0400100B RID: 4107
		private const float AROUND_DISTANCE = 4f;

		// Token: 0x0400100C RID: 4108
		private const float IMMUNE_TARGET_REDUCTION = 0.1f;

		// Token: 0x0400100D RID: 4109
		private const float MAX_IMMUNE_TARGET_REDUCTION = 0.5f;

		// Token: 0x0400100E RID: 4110
		public const uint LANDING_FRAMES = 4U;

		// Token: 0x0400100F RID: 4111
		[SerializeField]
		public Components Components;

		// Token: 0x04001010 RID: 4112
		[SerializeField]
		[SerializeReference]
		private NpcClassCustomizationData npcClass = new GruntNpcCustomizationData();

		// Token: 0x04001012 RID: 4114
		[SerializeField]
		private NpcGroup group;

		// Token: 0x04001013 RID: 4115
		[SerializeField]
		private PropCombatMode combatMode;

		// Token: 0x04001014 RID: 4116
		[SerializeField]
		private PropDamageMode damageMode;

		// Token: 0x04001015 RID: 4117
		[SerializeField]
		private PropPhysicsMode physicsMode;

		// Token: 0x0400101C RID: 4124
		[SerializeField]
		private CharacterVisualsReference characterVisuals;

		// Token: 0x0400101D RID: 4125
		public uint RangedAttackFrames = 12U;

		// Token: 0x0400101E RID: 4126
		public bool FinishedSpawnAnimation;

		// Token: 0x0400101F RID: 4127
		private NpcEnum.CombatState combatState;

		// Token: 0x04001020 RID: 4128
		private NpcEnum.PropFallMode fallMode;

		// Token: 0x04001021 RID: 4129
		private PropMovementMode movementMode = PropMovementMode.Run;

		// Token: 0x04001022 RID: 4130
		public Action OnNpcDespawned;

		// Token: 0x04001023 RID: 4131
		private CombatMode baseCombatMode;

		// Token: 0x04001024 RID: 4132
		private DamageMode baseDamageMode;

		// Token: 0x04001025 RID: 4133
		private PhysicsMode basePhysicsMode;

		// Token: 0x04001026 RID: 4134
		private NpcEnum.FallMode baseFallMode;

		// Token: 0x04001027 RID: 4135
		private MovementMode baseMovementMode;

		// Token: 0x04001028 RID: 4136
		private bool isDynamicSpawn;

		// Token: 0x04001029 RID: 4137
		private NpcConfiguration configuration;

		// Token: 0x0400102B RID: 4139
		private readonly Dictionary<HittableComponent, float> scoreReductionDictionary = new Dictionary<HittableComponent, float>();

		// Token: 0x0400102C RID: 4140
		private FsmState currentState;

		// Token: 0x0400102E RID: 4142
		public EndlessEvent<Context> OnEntityHealthZeroed = new EndlessEvent<Context>();

		// Token: 0x0400102F RID: 4143
		public EndlessEvent<Context> OnTargetHealthZeroed = new EndlessEvent<Context>();

		// Token: 0x04001032 RID: 4146
		public List<GoapAction.ActionKind> Actions;

		// Token: 0x04001038 RID: 4152
		private bool haveSpawnedVisuals;

		// Token: 0x04001039 RID: 4153
		public NetworkVariable<DamageMode> NetworkedDamageMode = new NetworkVariable<DamageMode>(DamageMode.TakeDamage, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x0400103A RID: 4154
		public NetworkVariable<PhysicsMode> NetworkedPhysicsMode = new NetworkVariable<PhysicsMode>(PhysicsMode.TakePhysics, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x0400103C RID: 4156
		public string CurrentStateName;

		// Token: 0x0400103D RID: 4157
		public global::UnityEngine.Vector3 LastKnownTargetLocation;

		// Token: 0x0400103E RID: 4158
		private uint downedFrame;

		// Token: 0x0400103F RID: 4159
		private IInteractionBehavior interactionBehavior;

		// Token: 0x04001041 RID: 4161
		private NpcEntity.PersistenceData persistenceData;

		// Token: 0x04001042 RID: 4162
		private Context context;

		// Token: 0x04001044 RID: 4164
		private Npc luaInterface;

		// Token: 0x04001045 RID: 4165
		[SerializeField]
		[HideInInspector]
		private EndlessScriptComponent scriptComponent;

		// Token: 0x0200032D RID: 813
		private class PersistenceData
		{
			// Token: 0x06001368 RID: 4968 RVA: 0x0005E747 File Offset: 0x0005C947
			public PersistenceData(global::UnityEngine.Vector3 mostRecentLocation, float mostRecentRotation, global::UnityEngine.Vector3 initialLocation, Quaternion initialRotation, List<Vector3Int> currentNodes)
			{
				this.MostRecentLocation = mostRecentLocation;
				this.MostRecentRotation = mostRecentRotation;
				this.InitialLocation = initialLocation;
				this.InitialRotation = initialRotation;
				this.CurrentNodes = currentNodes;
			}

			// Token: 0x04001046 RID: 4166
			public readonly global::UnityEngine.Vector3 InitialLocation;

			// Token: 0x04001047 RID: 4167
			public readonly Quaternion InitialRotation;

			// Token: 0x04001048 RID: 4168
			public readonly global::UnityEngine.Vector3 MostRecentLocation;

			// Token: 0x04001049 RID: 4169
			public readonly float MostRecentRotation;

			// Token: 0x0400104A RID: 4170
			public readonly List<Vector3Int> CurrentNodes;
		}
	}
}
