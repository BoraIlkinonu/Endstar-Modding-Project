using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x0200011B RID: 283
	public class UIStepper : UIGameObject
	{
		// Token: 0x17000127 RID: 295
		// (get) Token: 0x060006EE RID: 1774 RVA: 0x0001D710 File Offset: 0x0001B910
		public string[] Values
		{
			get
			{
				return this.values;
			}
		}

		// Token: 0x17000128 RID: 296
		// (get) Token: 0x060006EF RID: 1775 RVA: 0x0001D718 File Offset: 0x0001B918
		// (set) Token: 0x060006F0 RID: 1776 RVA: 0x0001D720 File Offset: 0x0001B920
		public int ValueIndex { get; private set; }

		// Token: 0x17000129 RID: 297
		// (get) Token: 0x060006F1 RID: 1777 RVA: 0x0001D729 File Offset: 0x0001B929
		public string Value
		{
			get
			{
				return this.values[this.ValueIndex];
			}
		}

		// Token: 0x060006F2 RID: 1778 RVA: 0x0001D738 File Offset: 0x0001B938
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.UpdateVisuals();
		}

		// Token: 0x060006F3 RID: 1779 RVA: 0x0001D758 File Offset: 0x0001B958
		public void StepUp()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "StepUp", Array.Empty<object>());
			}
			this.SetValue(this.ValueIndex + 1, false);
		}

		// Token: 0x060006F4 RID: 1780 RVA: 0x0001D781 File Offset: 0x0001B981
		public void StepDown()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "StepDown", Array.Empty<object>());
			}
			this.SetValue(this.ValueIndex - 1, false);
		}

		// Token: 0x060006F5 RID: 1781 RVA: 0x0001D7AC File Offset: 0x0001B9AC
		public void SetValues(List<string> newValues, bool suppressOnChange)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetValues", "newValues", newValues.Count, "suppressOnChange", suppressOnChange }), this);
				DebugUtility.DebugEnumerable<string>("newValues", newValues, this);
			}
			string[] array = this.ToStringArray(newValues);
			this.SetValues(array, suppressOnChange);
		}

		// Token: 0x060006F6 RID: 1782 RVA: 0x0001D824 File Offset: 0x0001BA24
		public void SetValues(string[] newValues, bool suppressOnChange)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetValues", "newValues", newValues.Length, "suppressOnChange", suppressOnChange }), this);
				DebugUtility.DebugEnumerable<string>("newValues", newValues, this);
			}
			if (this.values == newValues)
			{
				return;
			}
			this.values = newValues;
			this.SetValue(this.ValueIndex, suppressOnChange);
		}

		// Token: 0x060006F7 RID: 1783 RVA: 0x0001D8A8 File Offset: 0x0001BAA8
		public void SetValue(string newValue, bool suppressOnChange)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetValue", "newValue", newValue, "suppressOnChange", suppressOnChange }), this);
			}
			int num = Array.IndexOf<string>(this.values, newValue);
			if (num == -1)
			{
				DebugUtility.LogException(new IndexOutOfRangeException(), this);
				return;
			}
			this.SetValue(num, suppressOnChange);
		}

		// Token: 0x060006F8 RID: 1784 RVA: 0x0001D920 File Offset: 0x0001BB20
		public void SetValue(int newValueIndex, bool suppressOnChange)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetValue", "newValueIndex", newValueIndex, "suppressOnChange", suppressOnChange }), this);
			}
			newValueIndex = Mathf.Clamp(newValueIndex, 0, this.values.Length - 1);
			this.ValueIndex = newValueIndex;
			this.UpdateVisuals();
			if (!suppressOnChange)
			{
				this.ChangeUnityEvent.Invoke();
			}
		}

		// Token: 0x060006F9 RID: 1785 RVA: 0x0001D9A4 File Offset: 0x0001BBA4
		private void UpdateVisuals()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateVisuals", Array.Empty<object>());
			}
			string text = this.Value;
			if (this.insertSpaceBeforeAllCapitalCharactersInValueText)
			{
				text = StringUtility.InsertSpaceBeforeAllCapitalCharacters(text, true);
			}
			this.valueText.text = text;
			this.stepUpButton.interactable = this.ValueIndex < this.values.Length - 1;
			this.stepDownButton.interactable = this.ValueIndex > 0;
		}

		// Token: 0x060006FA RID: 1786 RVA: 0x0001DA20 File Offset: 0x0001BC20
		private string[] ToStringArray(List<string> list)
		{
			string[] array = new string[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				array[i] = list[i];
			}
			return array;
		}

		// Token: 0x04000406 RID: 1030
		public UnityEvent ChangeUnityEvent = new UnityEvent();

		// Token: 0x04000407 RID: 1031
		[SerializeField]
		private string[] values = new string[] { "A", "B", "C" };

		// Token: 0x04000408 RID: 1032
		[SerializeField]
		private TextMeshProUGUI valueText;

		// Token: 0x04000409 RID: 1033
		[SerializeField]
		private UIButton stepUpButton;

		// Token: 0x0400040A RID: 1034
		[SerializeField]
		private UIButton stepDownButton;

		// Token: 0x0400040B RID: 1035
		[SerializeField]
		[Tooltip("Except for the first character")]
		private bool insertSpaceBeforeAllCapitalCharactersInValueText = true;

		// Token: 0x0400040C RID: 1036
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
