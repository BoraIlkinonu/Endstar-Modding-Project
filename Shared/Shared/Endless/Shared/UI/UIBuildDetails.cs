using System;
using TMPro;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200010D RID: 269
	public class UIBuildDetails : MonoBehaviour
	{
		// Token: 0x06000670 RID: 1648 RVA: 0x0001B9AC File Offset: 0x00019BAC
		private void Start()
		{
			string text = BuildUtilities.Manifest.CommitId + "\n" + BuildUtilities.Manifest.Version;
			this.buildDetailsText.SetText(text, true);
		}

		// Token: 0x040003B7 RID: 951
		[SerializeField]
		private TextMeshProUGUI buildDetailsText;
	}
}
