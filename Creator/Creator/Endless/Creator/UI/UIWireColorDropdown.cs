using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002F5 RID: 757
	public class UIWireColorDropdown : UIBaseDropdown<UIWireColorDropdownValue>
	{
		// Token: 0x1400002C RID: 44
		// (add) Token: 0x06000D18 RID: 3352 RVA: 0x0003ED88 File Offset: 0x0003CF88
		// (remove) Token: 0x06000D19 RID: 3353 RVA: 0x0003EDC0 File Offset: 0x0003CFC0
		public event Action<WireColor> OnColorChanged;

		// Token: 0x170001B8 RID: 440
		// (get) Token: 0x06000D1A RID: 3354 RVA: 0x0003EDF5 File Offset: 0x0003CFF5
		// (set) Token: 0x06000D1B RID: 3355 RVA: 0x0003EDFD File Offset: 0x0003CFFD
		public bool Initialized { get; private set; }

		// Token: 0x06000D1C RID: 3356 RVA: 0x0003EE06 File Offset: 0x0003D006
		protected override void Start()
		{
			base.Start();
			if (!this.Initialized)
			{
				this.Initialize();
			}
		}

		// Token: 0x06000D1D RID: 3357 RVA: 0x0003EE1C File Offset: 0x0003D01C
		public void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			if (this.Initialized)
			{
				return;
			}
			this.Initialized = true;
			List<UIWireColorDropdownValue> list = new List<UIWireColorDropdownValue>();
			foreach (KeyValuePair<WireColor, UIWireColorDictionaryEntryValue> keyValuePair in this.wireColorDictionary)
			{
				UIWireColorDropdownValue uiwireColorDropdownValue = new UIWireColorDropdownValue(keyValuePair.Value, keyValuePair.Key);
				list.Add(uiwireColorDropdownValue);
			}
			UIWireColorDropdownValue[] array = new UIWireColorDropdownValue[] { list[0] };
			base.SetOptionsAndValue(list, array, false);
			base.OnValueChanged.AddListener(new UnityAction(this.InvokeOnColorChanged));
		}

		// Token: 0x06000D1E RID: 3358 RVA: 0x0003EEE4 File Offset: 0x0003D0E4
		protected override void View()
		{
			base.View();
			if (!this.Initialized)
			{
				this.Initialize();
			}
			base.SetValueIconColor(base.Value[0].WireColorDictionaryEntryValue.Color);
		}

		// Token: 0x06000D1F RID: 3359 RVA: 0x0003EF18 File Offset: 0x0003D118
		protected override string GetLabelFromOption(int optionIndex)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetLabelFromOption", new object[] { optionIndex });
			}
			if (!this.Initialized)
			{
				this.Initialize();
			}
			base.ValidateIndex(optionIndex, base.Count);
			return base.Options[optionIndex].WireColorName;
		}

		// Token: 0x06000D20 RID: 3360 RVA: 0x0003EF74 File Offset: 0x0003D174
		protected override Sprite GetIconFromOption(int optionIndex)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetIconFromOption", new object[] { optionIndex });
			}
			base.ValidateIndex(optionIndex, base.Count);
			return base.Options[optionIndex].WireColorDictionaryEntryValue.Sprite;
		}

		// Token: 0x06000D21 RID: 3361 RVA: 0x0003EFC6 File Offset: 0x0003D1C6
		private void InvokeOnColorChanged()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeOnColorChanged", Array.Empty<object>());
			}
			Action<WireColor> onColorChanged = this.OnColorChanged;
			if (onColorChanged == null)
			{
				return;
			}
			onColorChanged(base.Value[0].WireColor);
		}

		// Token: 0x06000D22 RID: 3362 RVA: 0x0003F001 File Offset: 0x0003D201
		public void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			if (!this.Initialized)
			{
				this.Initialize();
			}
		}

		// Token: 0x04000B4B RID: 2891
		[Header("UIWireColorDropdown")]
		[SerializeField]
		private WireColorDictionary wireColorDictionary;
	}
}
