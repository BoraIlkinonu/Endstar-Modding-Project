using System;
using System.Linq;
using Endless.Props.Assets;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Props.Loaders
{
	// Token: 0x02000036 RID: 54
	public class PropLoader
	{
		// Token: 0x060000D9 RID: 217 RVA: 0x00003194 File Offset: 0x00001394
		public static Prop[] LoadArrayFromJson(string json)
		{
			Prop[] array = JsonConvert.DeserializeObject<Prop[]>(json);
			Prop[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				PropLoader.UpgradeProp(array2[i]);
			}
			return array;
		}

		// Token: 0x060000DA RID: 218 RVA: 0x000031C1 File Offset: 0x000013C1
		public static Prop LoadFromJson(string json)
		{
			Prop prop = JsonConvert.DeserializeObject<Prop>(json);
			PropLoader.UpgradeProp(prop);
			return prop;
		}

		// Token: 0x060000DB RID: 219 RVA: 0x000031D0 File Offset: 0x000013D0
		public static void UpgradeProp(Prop prop)
		{
			prop.InternalVersion = (string.IsNullOrEmpty(prop.InternalVersion) ? "0.0.0" : prop.InternalVersion);
			if (SemanticVersion.Parse(prop.InternalVersion) == new SemanticVersion(0, 0, 0))
			{
				Vector3Int boundingSize = prop.GetBoundingSize();
				prop.ApplyXOffset = boundingSize.x % 2 == 0;
				prop.ApplyZOffset = boundingSize.z % 2 == 0;
				Vector3Int zero = Vector3Int.zero;
				if (boundingSize.x > 1)
				{
					zero.x = -Mathf.FloorToInt((float)boundingSize.x / 2f);
				}
				if (boundingSize.z > 1)
				{
					zero.z = -Mathf.FloorToInt((float)boundingSize.z / 2f);
				}
				PropLocationOffset[] array = prop.PropLocationOffsets.ToArray<PropLocationOffset>();
				PropLocationOffset[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].Offset += zero;
				}
				prop.SetPropLocationOffsets(array);
			}
			prop.InternalVersion = Prop.INTERNAL_VERSION.ToString();
		}
	}
}
