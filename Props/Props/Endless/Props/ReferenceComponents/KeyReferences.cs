using System;
using System.Collections.Generic;
using Endless.Validation;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x02000020 RID: 32
	public class KeyReferences : ReferenceBase
	{
		// Token: 0x1700003E RID: 62
		// (get) Token: 0x0600008F RID: 143 RVA: 0x00002C8E File Offset: 0x00000E8E
		// (set) Token: 0x06000090 RID: 144 RVA: 0x00002C96 File Offset: 0x00000E96
		public GameObject KeyVisuals { get; private set; }

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x06000091 RID: 145 RVA: 0x00002C9F File Offset: 0x00000E9F
		// (set) Token: 0x06000092 RID: 146 RVA: 0x00002CA7 File Offset: 0x00000EA7
		public GameObject LockVisuals { get; private set; }

		// Token: 0x06000093 RID: 147 RVA: 0x00002CB0 File Offset: 0x00000EB0
		public override List<Validator> GetValidators()
		{
			return new List<Validator>
			{
				new NotNullObjectValidator(this, this.KeyVisuals, "KeyVisuals cannot be null! This would make the item invisible!"),
				new NotNullObjectValidator(this, this.LockVisuals, "LockVisuals cannot be null! Without a lock lockable objects wont be able to show its visuals!")
			};
		}
	}
}
