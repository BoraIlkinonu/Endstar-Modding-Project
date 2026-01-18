using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Endless.Gameplay.Fsm;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.VisualManagement;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay;

public class NpcVisuals
{
	private readonly EndlessVisuals endlessVisuals;

	private readonly Components components;

	private readonly NpcEntity npcEntity;

	private GameObject currentVisuals;

	private GameObject currentWeapon;

	private GameObject currentEquipment;

	private CharacterCosmeticsDefinition currentCosmetics;

	private List<RendererManager> mainRendererManagers = new List<RendererManager>();

	private List<RendererManager> weaponRendererManagers = new List<RendererManager>();

	private List<RendererManager> equipmentRendererManagers = new List<RendererManager>();

	private CancellationTokenSource modelLoadSource = new CancellationTokenSource();

	public MaterialModifier MaterialModifier { get; private set; }

	public NpcVisuals(EndlessVisuals endlessVisuals, Components components, NpcEntity npcEntity)
	{
		this.endlessVisuals = endlessVisuals;
		this.components = components;
		this.npcEntity = npcEntity;
	}

	public void UpdateVisuals(CharacterCosmeticsDefinition cosmeticsDefinition)
	{
		if (!(cosmeticsDefinition == currentCosmetics))
		{
			modelLoadSource.Cancel();
			modelLoadSource = new CancellationTokenSource();
			SpawnVisuals(modelLoadSource.Token, cosmeticsDefinition);
		}
	}

	private async void SpawnVisuals(CancellationToken cancellationToken, CharacterCosmeticsDefinition cosmeticsDefinition)
	{
		GameObject result = await cosmeticsDefinition.Instantiate(components.VisualRoot).Task;
		if (cancellationToken.IsCancellationRequested)
		{
			UnityEngine.Object.Destroy(result);
			return;
		}
		if (!components.ClassDataList.TryGetClassData(npcEntity.NpcClass.NpcClass, out var classData))
		{
			throw new Exception("No associated class found for npc");
		}
		CleanupOldVisuals();
		await Task.Yield();
		SetupNewVisuals(result);
		SpawnDefaultItems(classData);
		currentCosmetics = cosmeticsDefinition;
		components.Animator.Rebind();
		components.MaterialModifier?.SetZombieCracks((classData.Class == NpcClass.Zombie) ? 1f : 0f);
		components.Animator.SetInteger(NpcAnimator.EquippedItem, classData.EquippedItemParameter);
		if (components.DynamicAttributes != null)
		{
			components.Animator.SetBool(NpcAnimator.Walking, npcEntity.MovementMode == MovementMode.Walk);
		}
		FsmState currentState = npcEntity.CurrentState;
		if (currentState != null && currentState.State == NpcEnum.FsmState.Spawning)
		{
			components.NpcAnimator.PlaySpawnAnimation();
		}
	}

	public void RespawnCosmetics()
	{
		if (!components.ClassDataList.TryGetClassData(npcEntity.NpcClass.NpcClass, out var classData))
		{
			throw new Exception("No associated class found for npc");
		}
		CleanupOldWeapon();
		CleanupOldEquipment();
		SpawnDefaultItems(classData);
		components.MaterialModifier?.SetZombieCracks((classData.Class == NpcClass.Zombie) ? 1f : 0f);
		components.Animator.SetInteger(NpcAnimator.EquippedItem, classData.EquippedItemParameter);
	}

	private void SpawnDefaultItems(ClassData classData)
	{
		if ((bool)classData.DefaultWeapon)
		{
			Transform transform = FindBone(classData.WeaponBone, components.VisualRoot);
			if ((bool)transform)
			{
				GameObject newWeapon = UnityEngine.Object.Instantiate(classData.DefaultWeapon, transform);
				SetupNewWeapon(newWeapon);
			}
		}
		if ((bool)classData.DefaultEquipment)
		{
			Transform transform = FindBone(classData.EquipmentBone, components.VisualRoot);
			if ((bool)transform)
			{
				GameObject newEquipment = UnityEngine.Object.Instantiate(classData.DefaultEquipment, transform);
				SetupNewEquipment(newEquipment);
			}
		}
	}

	private void CleanupOldVisuals()
	{
		if ((bool)currentVisuals)
		{
			endlessVisuals.UnmanageRenderers(mainRendererManagers);
			mainRendererManagers.Clear();
			CleanupOldWeapon();
			CleanupOldEquipment();
			currentVisuals.transform.parent = null;
			UnityEngine.Object.Destroy(currentVisuals);
		}
	}

	private void CleanupOldWeapon()
	{
		if ((bool)currentWeapon)
		{
			endlessVisuals.UnmanageRenderers(weaponRendererManagers);
			weaponRendererManagers.Clear();
			UnityEngine.Object.Destroy(currentWeapon);
		}
	}

	private void CleanupOldEquipment()
	{
		if ((bool)currentEquipment)
		{
			endlessVisuals.UnmanageRenderers(equipmentRendererManagers);
			equipmentRendererManagers.Clear();
			UnityEngine.Object.Destroy(currentEquipment);
		}
	}

	private void SetupNewVisuals(GameObject newVisuals)
	{
		currentVisuals = newVisuals;
		Renderer[] componentsInChildren = currentVisuals.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			Material[] materials = array[i].materials;
			foreach (Material material in materials)
			{
				string name = ((material.shader.name == "Shader Graphs/Endless_Shader") ? "Shader Graphs/Endless_Shader_Character_NoFade" : material.shader.name);
				material.shader = Shader.Find(name);
			}
		}
		mainRendererManagers = endlessVisuals.ManageRenderers(componentsInChildren);
		currentVisuals.transform.localRotation = quaternion.identity;
		MaterialModifier = currentVisuals.AddComponent<MaterialModifier>();
	}

	private void SetupNewWeapon(GameObject newWeapon)
	{
		currentWeapon = newWeapon;
		weaponRendererManagers = endlessVisuals.ManageRenderers(currentWeapon.GetComponentsInChildren<Renderer>());
		components.EmitterPoint = currentWeapon.GetComponentInChildren<EmitterPoint>()?.transform;
		AIProjectileShooterReferencePointer component = currentWeapon.GetComponent<AIProjectileShooterReferencePointer>();
		if (component != null)
		{
			components.ProjectileShooter.AISetShooterReferences(component);
		}
	}

	private void SetupNewEquipment(GameObject newEquipment)
	{
		currentEquipment = newEquipment;
		equipmentRendererManagers = endlessVisuals.ManageRenderers(currentEquipment.GetComponentsInChildren<Renderer>());
	}

	private static Transform FindBone(string boneName, Transform currentTransform)
	{
		if (currentTransform.gameObject.name == boneName)
		{
			return currentTransform;
		}
		int childCount = currentTransform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform transform = FindBone(boneName, currentTransform.GetChild(i));
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}
}
