using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay;

public class NpcOutline : NpcComponent, IStartSubscriber
{
	[SerializeField]
	private List<Renderer> characterRenderers;

	[SerializeField]
	private Color immortalColor;

	[SerializeField]
	private Color enemyColor;

	[SerializeField]
	private float outlineThicknessPerHp;

	[SerializeField]
	private bool useConstantOutline;

	private MaterialPropertyBlock materialPropertyBlock;

	private static readonly int outlineColor = Shader.PropertyToID("_Outline_Color");

	private static readonly int outlineThickness = Shader.PropertyToID("_Outline_Thickness");

	private void UpdateAllRenderers()
	{
		foreach (Renderer characterRenderer in characterRenderers)
		{
			UpdateRenderer(characterRenderer);
		}
	}

	private void HandleHeathChanged(int _, int newHealth)
	{
		UpdateAllRenderers();
	}

	public void EndlessStart()
	{
		materialPropertyBlock = new MaterialPropertyBlock();
		base.NpcEntity.Components.DynamicAttributes.OnDamageModeChanged += UpdateAllRenderers;
		base.NpcEntity.Components.Health.OnHealthChanged.AddListener(HandleHeathChanged);
		UpdateAllRenderers();
	}

	private void UpdateRenderer(Renderer characterRenderer)
	{
		characterRenderer.GetPropertyBlock(materialPropertyBlock);
		if (base.NpcEntity.Team == Team.Friendly)
		{
			materialPropertyBlock.SetFloat(outlineThickness, 0f);
		}
		else if (base.NpcEntity.DamageMode == DamageMode.IgnoreDamage)
		{
			materialPropertyBlock.SetColor(outlineColor, immortalColor);
			materialPropertyBlock.SetFloat(outlineThickness, 5f * outlineThicknessPerHp);
		}
		else
		{
			materialPropertyBlock.SetColor(outlineColor, enemyColor);
			materialPropertyBlock.SetFloat(outlineThickness, useConstantOutline ? (5f * outlineThicknessPerHp) : ((float)base.NpcEntity.Components.Health.CurrentHealth * outlineThicknessPerHp));
		}
		characterRenderer.SetPropertyBlock(materialPropertyBlock);
	}
}
