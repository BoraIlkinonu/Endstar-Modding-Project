using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using TMPro;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200026F RID: 623
	public class UIRuntimePlatformTextHandler : UIGameObject, IValidatable
	{
		// Token: 0x06000FAB RID: 4011 RVA: 0x000435A8 File Offset: 0x000417A8
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (this.runtimePlatformsToIgnore.Contains(Application.platform))
			{
				return;
			}
			UIRuntimePlatformTextHandler.KeyValuePair keyValuePair2 = this.keyValuePairs.FirstOrDefault((UIRuntimePlatformTextHandler.KeyValuePair keyValuePair) => keyValuePair.Key.Contains(Application.platform));
			this.textToApplyTo.text = keyValuePair2.Value;
		}

		// Token: 0x06000FAC RID: 4012 RVA: 0x00043620 File Offset: 0x00041820
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			List<RuntimePlatform> list = new List<RuntimePlatform>();
			HashSet<RuntimePlatform> hashSet = new HashSet<RuntimePlatform>();
			foreach (UIRuntimePlatformTextHandler.KeyValuePair keyValuePair in this.keyValuePairs)
			{
				keyValuePair.Validate(this);
				list.AddRange(keyValuePair.Key);
				foreach (RuntimePlatform runtimePlatform in keyValuePair.Key)
				{
					hashSet.Add(runtimePlatform);
				}
			}
			DebugUtility.DebugHasDuplicates<RuntimePlatform>(list, "keyValuePairs", this);
			if (!this.validateSupportedRuntimePlatforms)
			{
				return;
			}
			foreach (RuntimePlatform runtimePlatform2 in this.supportedRuntimePlatforms.Value)
			{
				if (!hashSet.Contains(runtimePlatform2))
				{
					DebugUtility.LogError(string.Format("Missing support for a {0} of {1}!", "supportedRuntimePlatform", runtimePlatform2), this);
				}
			}
		}

		// Token: 0x040009FF RID: 2559
		[SerializeField]
		private UIRuntimePlatformTextHandler.KeyValuePair[] keyValuePairs = Array.Empty<UIRuntimePlatformTextHandler.KeyValuePair>();

		// Token: 0x04000A00 RID: 2560
		[SerializeField]
		private TextMeshProUGUI textToApplyTo;

		// Token: 0x04000A01 RID: 2561
		[SerializeField]
		private RuntimePlatformScriptableObjectArray supportedRuntimePlatforms;

		// Token: 0x04000A02 RID: 2562
		[SerializeField]
		private SerializableHashSet<RuntimePlatform> runtimePlatformsToIgnore = new SerializableHashSet<RuntimePlatform>();

		// Token: 0x04000A03 RID: 2563
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000A04 RID: 2564
		[SerializeField]
		private bool validateSupportedRuntimePlatforms;

		// Token: 0x04000A05 RID: 2565
		private readonly Dictionary<RuntimePlatform, string> dictionary = new Dictionary<RuntimePlatform, string>();

		// Token: 0x02000270 RID: 624
		[Serializable]
		private struct KeyValuePair
		{
			// Token: 0x170002F3 RID: 755
			// (get) Token: 0x06000FAE RID: 4014 RVA: 0x00043737 File Offset: 0x00041937
			// (set) Token: 0x06000FAF RID: 4015 RVA: 0x0004373F File Offset: 0x0004193F
			public RuntimePlatform[] Key { readonly get; private set; }

			// Token: 0x170002F4 RID: 756
			// (get) Token: 0x06000FB0 RID: 4016 RVA: 0x00043748 File Offset: 0x00041948
			// (set) Token: 0x06000FB1 RID: 4017 RVA: 0x00043750 File Offset: 0x00041950
			public string Value { readonly get; private set; }

			// Token: 0x06000FB2 RID: 4018 RVA: 0x00043759 File Offset: 0x00041959
			public void Validate(object context)
			{
				DebugUtility.DebugHasDuplicates<RuntimePlatform>(this.Key, "Key", context);
			}
		}
	}
}
