using System;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200030D RID: 781
	public class DraggablePhysicsCube : EndlessNetworkBehaviour, IBaseType, IComponentBase
	{
		// Token: 0x06001217 RID: 4631 RVA: 0x0005992E File Offset: 0x00057B2E
		public GameObject GetAppearanceObject()
		{
			return this.references.VisualsBaseTransform.gameObject;
		}

		// Token: 0x1700039A RID: 922
		// (get) Token: 0x06001218 RID: 4632 RVA: 0x00059940 File Offset: 0x00057B40
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

		// Token: 0x1700039B RID: 923
		// (get) Token: 0x06001219 RID: 4633 RVA: 0x0005996B File Offset: 0x00057B6B
		// (set) Token: 0x0600121A RID: 4634 RVA: 0x00059973 File Offset: 0x00057B73
		public WorldObject WorldObject { get; private set; }

		// Token: 0x1700039C RID: 924
		// (get) Token: 0x0600121B RID: 4635 RVA: 0x0005997C File Offset: 0x00057B7C
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(DraggablePhysicsCubeReferences);
			}
		}

		// Token: 0x1700039D RID: 925
		// (get) Token: 0x0600121C RID: 4636 RVA: 0x0001BD04 File Offset: 0x00019F04
		public NavType NavValue
		{
			get
			{
				return NavType.Intangible;
			}
		}

		// Token: 0x0600121D RID: 4637 RVA: 0x00059988 File Offset: 0x00057B88
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.references = (DraggablePhysicsCubeReferences)referenceBase;
		}

		// Token: 0x0600121E RID: 4638 RVA: 0x00059996 File Offset: 0x00057B96
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x06001220 RID: 4640 RVA: 0x000599A0 File Offset: 0x00057BA0
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06001221 RID: 4641 RVA: 0x0001E813 File Offset: 0x0001CA13
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06001222 RID: 4642 RVA: 0x000599B6 File Offset: 0x00057BB6
		protected internal override string __getTypeName()
		{
			return "DraggablePhysicsCube";
		}

		// Token: 0x04000F60 RID: 3936
		[SerializeField]
		public PhysicsCubeController PhysicsCubeController;

		// Token: 0x04000F61 RID: 3937
		private Context context;

		// Token: 0x04000F63 RID: 3939
		[HideInInspector]
		[SerializeField]
		private DraggablePhysicsCubeReferences references;
	}
}
