using System;
using Endless.Creator.DynamicPropCreation;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000148 RID: 328
	public class UIPropCreationDataListCellController : UIBaseListCellController<PropCreationData>
	{
		// Token: 0x06000509 RID: 1289 RVA: 0x0001C05C File Offset: 0x0001A25C
		protected override void Start()
		{
			base.Start();
			this.selectButton.onClick.AddListener(new UnityAction(this.SelectPropCreation));
		}

		// Token: 0x0600050A RID: 1290 RVA: 0x0001C080 File Offset: 0x0001A280
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x0600050B RID: 1291 RVA: 0x0001C0A0 File Offset: 0x0001A2A0
		private void SelectPropCreation()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SelectPropCreation", Array.Empty<object>());
			}
			if (base.Model.IsSubMenu)
			{
				PropCreationMenuData propCreationMenuData = (PropCreationMenuData)base.Model;
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.propCreationMenuModalSource, UIModalManagerStackActions.MaintainStack, new object[] { propCreationMenuData });
				return;
			}
			PropCreationPromptData propCreationPromptData = base.Model as PropCreationPromptData;
			if (propCreationPromptData != null)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.propCreationPromptDataModalSource, UIModalManagerStackActions.MaintainStack, new object[] { propCreationPromptData });
				return;
			}
			AbstractPropCreationScreenData abstractPropCreationScreenData = base.Model as AbstractPropCreationScreenData;
			if (abstractPropCreationScreenData != null)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.abstractPropCreationModalSource, UIModalManagerStackActions.MaintainStack, new object[] { abstractPropCreationScreenData });
				return;
			}
			GenericPropCreationScreenData genericPropCreationScreenData = base.Model as GenericPropCreationScreenData;
			if (genericPropCreationScreenData != null)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.genericPropCreationModalSource, UIModalManagerStackActions.MaintainStack, new object[] { genericPropCreationScreenData });
				return;
			}
			Debug.LogError(base.Model.GetType());
		}

		// Token: 0x0400049E RID: 1182
		[Header("UIPropCreationDataListCellController")]
		[SerializeField]
		private UIButton selectButton;

		// Token: 0x0400049F RID: 1183
		[SerializeField]
		private UIPropCreationPromptDataModalView propCreationPromptDataModalSource;

		// Token: 0x040004A0 RID: 1184
		[SerializeField]
		private UIPropCreationMenuModalView propCreationMenuModalSource;

		// Token: 0x040004A1 RID: 1185
		[SerializeField]
		private UIAbstractPropCreationModalView abstractPropCreationModalSource;

		// Token: 0x040004A2 RID: 1186
		[SerializeField]
		private UIGenericPropCreationModalView genericPropCreationModalSource;
	}
}
