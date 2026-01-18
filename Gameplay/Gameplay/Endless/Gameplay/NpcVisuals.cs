using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.VisualManagement;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000144 RID: 324
	public class NpcVisuals
	{
		// Token: 0x17000169 RID: 361
		// (get) Token: 0x06000793 RID: 1939 RVA: 0x00023947 File Offset: 0x00021B47
		// (set) Token: 0x06000794 RID: 1940 RVA: 0x0002394F File Offset: 0x00021B4F
		public MaterialModifier MaterialModifier { get; private set; }

		// Token: 0x06000795 RID: 1941 RVA: 0x00023958 File Offset: 0x00021B58
		public NpcVisuals(EndlessVisuals endlessVisuals, Components components, NpcEntity npcEntity)
		{
			this.endlessVisuals = endlessVisuals;
			this.components = components;
			this.npcEntity = npcEntity;
		}

		// Token: 0x06000796 RID: 1942 RVA: 0x000239AC File Offset: 0x00021BAC
		public void UpdateVisuals(CharacterCosmeticsDefinition cosmeticsDefinition)
		{
			if (cosmeticsDefinition == this.currentCosmetics)
			{
				return;
			}
			this.modelLoadSource.Cancel();
			this.modelLoadSource = new CancellationTokenSource();
			this.SpawnVisuals(this.modelLoadSource.Token, cosmeticsDefinition);
		}

		// Token: 0x06000797 RID: 1943 RVA: 0x000239E8 File Offset: 0x00021BE8
		private void SpawnVisuals(CancellationToken cancellationToken, CharacterCosmeticsDefinition cosmeticsDefinition)
		{
			NpcVisuals.<SpawnVisuals>d__17 <SpawnVisuals>d__;
			<SpawnVisuals>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<SpawnVisuals>d__.<>4__this = this;
			<SpawnVisuals>d__.cancellationToken = cancellationToken;
			<SpawnVisuals>d__.cosmeticsDefinition = cosmeticsDefinition;
			<SpawnVisuals>d__.<>1__state = -1;
			<SpawnVisuals>d__.<>t__builder.Start<NpcVisuals.<SpawnVisuals>d__17>(ref <SpawnVisuals>d__);
		}

		// Token: 0x06000798 RID: 1944 RVA: 0x00023A30 File Offset: 0x00021C30
		public void RespawnCosmetics()
		{
			ClassData classData;
			if (!this.components.ClassDataList.TryGetClassData(this.npcEntity.NpcClass.NpcClass, out classData))
			{
				throw new Exception("No associated class found for npc");
			}
			this.CleanupOldWeapon();
			this.CleanupOldEquipment();
			this.SpawnDefaultItems(classData);
			MaterialModifier materialModifier = this.components.MaterialModifier;
			if (materialModifier != null)
			{
				materialModifier.SetZombieCracks((classData.Class == NpcClass.Zombie) ? 1f : 0f);
			}
			this.components.Animator.SetInteger(NpcAnimator.EquippedItem, classData.EquippedItemParameter);
		}

		// Token: 0x06000799 RID: 1945 RVA: 0x00023AC8 File Offset: 0x00021CC8
		private void SpawnDefaultItems(ClassData classData)
		{
			if (classData.DefaultWeapon)
			{
				Transform transform = NpcVisuals.FindBone(classData.WeaponBone, this.components.VisualRoot);
				if (transform)
				{
					GameObject gameObject = global::UnityEngine.Object.Instantiate<GameObject>(classData.DefaultWeapon, transform);
					this.SetupNewWeapon(gameObject);
				}
			}
			if (classData.DefaultEquipment)
			{
				Transform transform = NpcVisuals.FindBone(classData.EquipmentBone, this.components.VisualRoot);
				if (transform)
				{
					GameObject gameObject2 = global::UnityEngine.Object.Instantiate<GameObject>(classData.DefaultEquipment, transform);
					this.SetupNewEquipment(gameObject2);
				}
			}
		}

		// Token: 0x0600079A RID: 1946 RVA: 0x00023B58 File Offset: 0x00021D58
		private void CleanupOldVisuals()
		{
			if (!this.currentVisuals)
			{
				return;
			}
			this.endlessVisuals.UnmanageRenderers(this.mainRendererManagers);
			this.mainRendererManagers.Clear();
			this.CleanupOldWeapon();
			this.CleanupOldEquipment();
			this.currentVisuals.transform.parent = null;
			global::UnityEngine.Object.Destroy(this.currentVisuals);
		}

		// Token: 0x0600079B RID: 1947 RVA: 0x00023BB7 File Offset: 0x00021DB7
		private void CleanupOldWeapon()
		{
			if (!this.currentWeapon)
			{
				return;
			}
			this.endlessVisuals.UnmanageRenderers(this.weaponRendererManagers);
			this.weaponRendererManagers.Clear();
			global::UnityEngine.Object.Destroy(this.currentWeapon);
		}

		// Token: 0x0600079C RID: 1948 RVA: 0x00023BEE File Offset: 0x00021DEE
		private void CleanupOldEquipment()
		{
			if (!this.currentEquipment)
			{
				return;
			}
			this.endlessVisuals.UnmanageRenderers(this.equipmentRendererManagers);
			this.equipmentRendererManagers.Clear();
			global::UnityEngine.Object.Destroy(this.currentEquipment);
		}

		// Token: 0x0600079D RID: 1949 RVA: 0x00023C28 File Offset: 0x00021E28
		private void SetupNewVisuals(GameObject newVisuals)
		{
			this.currentVisuals = newVisuals;
			Renderer[] componentsInChildren = this.currentVisuals.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Material material in array[i].materials)
				{
					string text = ((material.shader.name == "Shader Graphs/Endless_Shader") ? "Shader Graphs/Endless_Shader_Character_NoFade" : material.shader.name);
					material.shader = Shader.Find(text);
				}
			}
			this.mainRendererManagers = this.endlessVisuals.ManageRenderers(componentsInChildren);
			this.currentVisuals.transform.localRotation = quaternion.identity;
			this.MaterialModifier = this.currentVisuals.AddComponent<MaterialModifier>();
		}

		// Token: 0x0600079E RID: 1950 RVA: 0x00023CF4 File Offset: 0x00021EF4
		private void SetupNewWeapon(GameObject newWeapon)
		{
			this.currentWeapon = newWeapon;
			this.weaponRendererManagers = this.endlessVisuals.ManageRenderers(this.currentWeapon.GetComponentsInChildren<Renderer>());
			Components components = this.components;
			EmitterPoint componentInChildren = this.currentWeapon.GetComponentInChildren<EmitterPoint>();
			components.EmitterPoint = ((componentInChildren != null) ? componentInChildren.transform : null);
			AIProjectileShooterReferencePointer component = this.currentWeapon.GetComponent<AIProjectileShooterReferencePointer>();
			if (component != null)
			{
				this.components.ProjectileShooter.AISetShooterReferences(component);
			}
		}

		// Token: 0x0600079F RID: 1951 RVA: 0x00023D6C File Offset: 0x00021F6C
		private void SetupNewEquipment(GameObject newEquipment)
		{
			this.currentEquipment = newEquipment;
			this.equipmentRendererManagers = this.endlessVisuals.ManageRenderers(this.currentEquipment.GetComponentsInChildren<Renderer>());
		}

		// Token: 0x060007A0 RID: 1952 RVA: 0x00023D94 File Offset: 0x00021F94
		private static Transform FindBone(string boneName, Transform currentTransform)
		{
			if (currentTransform.gameObject.name == boneName)
			{
				return currentTransform;
			}
			int childCount = currentTransform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform transform = NpcVisuals.FindBone(boneName, currentTransform.GetChild(i));
				if (transform != null)
				{
					return transform;
				}
			}
			return null;
		}

		// Token: 0x0400060B RID: 1547
		private readonly EndlessVisuals endlessVisuals;

		// Token: 0x0400060C RID: 1548
		private readonly Components components;

		// Token: 0x0400060D RID: 1549
		private readonly NpcEntity npcEntity;

		// Token: 0x0400060E RID: 1550
		private GameObject currentVisuals;

		// Token: 0x0400060F RID: 1551
		private GameObject currentWeapon;

		// Token: 0x04000610 RID: 1552
		private GameObject currentEquipment;

		// Token: 0x04000611 RID: 1553
		private CharacterCosmeticsDefinition currentCosmetics;

		// Token: 0x04000612 RID: 1554
		private List<RendererManager> mainRendererManagers = new List<RendererManager>();

		// Token: 0x04000613 RID: 1555
		private List<RendererManager> weaponRendererManagers = new List<RendererManager>();

		// Token: 0x04000614 RID: 1556
		private List<RendererManager> equipmentRendererManagers = new List<RendererManager>();

		// Token: 0x04000616 RID: 1558
		private CancellationTokenSource modelLoadSource = new CancellationTokenSource();
	}
}
