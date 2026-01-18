using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000143 RID: 323
	public class NpcOutline : NpcComponent, IStartSubscriber
	{
		// Token: 0x0600078D RID: 1933 RVA: 0x0002377C File Offset: 0x0002197C
		private void UpdateAllRenderers()
		{
			foreach (Renderer renderer in this.characterRenderers)
			{
				this.UpdateRenderer(renderer);
			}
		}

		// Token: 0x0600078E RID: 1934 RVA: 0x000237D0 File Offset: 0x000219D0
		private void HandleHeathChanged(int _, int newHealth)
		{
			this.UpdateAllRenderers();
		}

		// Token: 0x0600078F RID: 1935 RVA: 0x000237D8 File Offset: 0x000219D8
		public void EndlessStart()
		{
			this.materialPropertyBlock = new MaterialPropertyBlock();
			base.NpcEntity.Components.DynamicAttributes.OnDamageModeChanged += this.UpdateAllRenderers;
			base.NpcEntity.Components.Health.OnHealthChanged.AddListener(new UnityAction<int, int>(this.HandleHeathChanged));
			this.UpdateAllRenderers();
		}

		// Token: 0x06000790 RID: 1936 RVA: 0x00023840 File Offset: 0x00021A40
		private void UpdateRenderer(Renderer characterRenderer)
		{
			characterRenderer.GetPropertyBlock(this.materialPropertyBlock);
			if (base.NpcEntity.Team == Team.Friendly)
			{
				this.materialPropertyBlock.SetFloat(NpcOutline.outlineThickness, 0f);
			}
			else if (base.NpcEntity.DamageMode == DamageMode.IgnoreDamage)
			{
				this.materialPropertyBlock.SetColor(NpcOutline.outlineColor, this.immortalColor);
				this.materialPropertyBlock.SetFloat(NpcOutline.outlineThickness, 5f * this.outlineThicknessPerHp);
			}
			else
			{
				this.materialPropertyBlock.SetColor(NpcOutline.outlineColor, this.enemyColor);
				this.materialPropertyBlock.SetFloat(NpcOutline.outlineThickness, this.useConstantOutline ? (5f * this.outlineThicknessPerHp) : ((float)base.NpcEntity.Components.Health.CurrentHealth * this.outlineThicknessPerHp));
			}
			characterRenderer.SetPropertyBlock(this.materialPropertyBlock);
		}

		// Token: 0x04000603 RID: 1539
		[SerializeField]
		private List<Renderer> characterRenderers;

		// Token: 0x04000604 RID: 1540
		[SerializeField]
		private Color immortalColor;

		// Token: 0x04000605 RID: 1541
		[SerializeField]
		private Color enemyColor;

		// Token: 0x04000606 RID: 1542
		[SerializeField]
		private float outlineThicknessPerHp;

		// Token: 0x04000607 RID: 1543
		[SerializeField]
		private bool useConstantOutline;

		// Token: 0x04000608 RID: 1544
		private MaterialPropertyBlock materialPropertyBlock;

		// Token: 0x04000609 RID: 1545
		private static readonly int outlineColor = Shader.PropertyToID("_Outline_Color");

		// Token: 0x0400060A RID: 1546
		private static readonly int outlineThickness = Shader.PropertyToID("_Outline_Thickness");
	}
}
