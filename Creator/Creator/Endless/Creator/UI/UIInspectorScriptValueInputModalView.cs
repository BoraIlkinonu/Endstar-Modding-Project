using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020001CF RID: 463
	public class UIInspectorScriptValueInputModalView : UIScriptModalView
	{
		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x060006EB RID: 1771 RVA: 0x000230DA File Offset: 0x000212DA
		// (set) Token: 0x060006EC RID: 1772 RVA: 0x000230E2 File Offset: 0x000212E2
		public IUIPresentable Presentable { get; private set; }

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x060006ED RID: 1773 RVA: 0x000230EB File Offset: 0x000212EB
		// (set) Token: 0x060006EE RID: 1774 RVA: 0x000230F3 File Offset: 0x000212F3
		public Type InspectorScriptValueType { get; private set; }

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x060006EF RID: 1775 RVA: 0x000230FC File Offset: 0x000212FC
		// (set) Token: 0x060006F0 RID: 1776 RVA: 0x00023104 File Offset: 0x00021304
		public bool IsCollection { get; private set; }

		// Token: 0x060006F1 RID: 1777 RVA: 0x00023110 File Offset: 0x00021310
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			this.InspectorScriptValueType = (Type)modalData[0];
			this.IsCollection = (bool)modalData[1];
			this.Clear();
			Type type = this.InspectorScriptValueType;
			if (this.IsCollection)
			{
				type = type.MakeArrayType();
			}
			if (base.VerboseLogging)
			{
				DebugUtility.Log("typeToSpawn: " + type.Name, this);
			}
			object obj = MonoBehaviourSingleton<UIDynamicTypeFactory>.Instance.Create(type);
			Dictionary<Type, Enum> dictionary = new Dictionary<Type, Enum>(this.typeStyleOverrideDictionary);
			Enum @enum;
			if (this.typeStyleOverrideDictionary.TryGetValue(type, out @enum))
			{
				this.Presentable = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnObjectModelWithStyle(obj, @enum, this.defaultValueContainer, dictionary);
			}
			else
			{
				this.Presentable = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnObjectModelWithDefaultStyle(obj, this.defaultValueContainer, dictionary);
			}
			foreach (InterfaceReference<IUIChildLayoutable> interfaceReference in this.defaultValueContainerLayoutables)
			{
				interfaceReference.Interface.CollectChildLayoutItems();
				interfaceReference.Interface.RequestLayout();
			}
			UIIEnumerablePresenter uiienumerablePresenter = this.Presentable as UIIEnumerablePresenter;
			if (uiienumerablePresenter != null)
			{
				uiienumerablePresenter.SetElementType(this.IsCollection ? type.GetElementType() : type);
				UIBaseIEnumerableView uibaseIEnumerableView = uiienumerablePresenter.View.Interface as UIBaseIEnumerableView;
				uibaseIEnumerableView.SetDisplayEnumLabels(false);
				uibaseIEnumerableView.LayoutElement.FlexibleHeightLayoutDimension.ExplicitValue = 1f;
				uiienumerablePresenter.IEnumerableView.SetCanAddAndRemoveItems(true);
				uiienumerablePresenter.SetModel((IEnumerable)obj, true);
				uibaseIEnumerableView.SetInteractable(true);
			}
			UIEnumDropdownView uienumDropdownView = this.Presentable.Viewable as UIEnumDropdownView;
			if (uienumDropdownView != null)
			{
				uienumDropdownView.SetLabel(string.Empty);
			}
			LayoutRebuilder.MarkLayoutForRebuild(this.defaultValueContainer);
		}

		// Token: 0x060006F2 RID: 1778 RVA: 0x0001FDD0 File Offset: 0x0001DFD0
		public override void OnBack()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
		}

		// Token: 0x060006F3 RID: 1779 RVA: 0x000232B0 File Offset: 0x000214B0
		private void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.nameInputField.Clear(true);
			this.descriptionInputField.Clear(true);
			this.groupNameInputField.Clear(true);
			this.hideToggle.SetIsOn(false, true, true);
			if (this.Presentable != null)
			{
				this.Presentable.ReturnToPool();
				this.Presentable = null;
			}
		}

		// Token: 0x04000634 RID: 1588
		[Header("UIInspectorScriptValueInputModalView")]
		[SerializeField]
		private UIInputField nameInputField;

		// Token: 0x04000635 RID: 1589
		[SerializeField]
		private UIInputField descriptionInputField;

		// Token: 0x04000636 RID: 1590
		[SerializeField]
		private UIInputField groupNameInputField;

		// Token: 0x04000637 RID: 1591
		[SerializeField]
		private UIToggle hideToggle;

		// Token: 0x04000638 RID: 1592
		[SerializeField]
		private RectTransform defaultValueContainer;

		// Token: 0x04000639 RID: 1593
		[SerializeField]
		private InterfaceReference<IUIChildLayoutable>[] defaultValueContainerLayoutables = Array.Empty<InterfaceReference<IUIChildLayoutable>>();

		// Token: 0x0400063A RID: 1594
		private readonly Dictionary<Type, Enum> typeStyleOverrideDictionary = new Dictionary<Type, Enum>
		{
			{
				typeof(InstanceReference),
				UIInstanceReferenceView.Styles.NoneOrContext
			},
			{
				typeof(NpcInstanceReference),
				UINpcInstanceReferenceView.Styles.NoneOrContext
			},
			{
				typeof(LevelDestination),
				UILevelDestinationView.Styles.None
			},
			{
				typeof(CellReference),
				UICellReferenceView.Styles.ReadOnly
			},
			{
				typeof(Color),
				UIBaseColorView.Styles.Detail
			}
		};
	}
}
