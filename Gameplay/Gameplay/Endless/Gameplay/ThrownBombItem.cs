using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002B9 RID: 697
	public class ThrownBombItem : StackableItem, IScriptInjector
	{
		// Token: 0x17000316 RID: 790
		// (get) Token: 0x06000FD2 RID: 4050 RVA: 0x00051A56 File Offset: 0x0004FC56
		protected override Item.VisualsInfo GroundVisualsInfo
		{
			get
			{
				return this.tempVisualsInfoGround;
			}
		}

		// Token: 0x17000317 RID: 791
		// (get) Token: 0x06000FD3 RID: 4051 RVA: 0x00051A5E File Offset: 0x0004FC5E
		protected override Item.VisualsInfo EquippedVisualsInfo
		{
			get
			{
				return this.tempVisualsInfoEqupped;
			}
		}

		// Token: 0x17000318 RID: 792
		// (get) Token: 0x06000FD4 RID: 4052 RVA: 0x00051A66 File Offset: 0x0004FC66
		// (set) Token: 0x06000FD5 RID: 4053 RVA: 0x00051A6E File Offset: 0x0004FC6E
		public int DamageAtCenter { get; internal set; } = 4;

		// Token: 0x17000319 RID: 793
		// (get) Token: 0x06000FD6 RID: 4054 RVA: 0x00051A77 File Offset: 0x0004FC77
		// (set) Token: 0x06000FD7 RID: 4055 RVA: 0x00051A7F File Offset: 0x0004FC7F
		public int DamageAtEdge { get; internal set; } = 2;

		// Token: 0x1700031A RID: 794
		// (get) Token: 0x06000FD8 RID: 4056 RVA: 0x00051A88 File Offset: 0x0004FC88
		// (set) Token: 0x06000FD9 RID: 4057 RVA: 0x00051A90 File Offset: 0x0004FC90
		public float CenterRadius { get; internal set; } = 2f;

		// Token: 0x1700031B RID: 795
		// (get) Token: 0x06000FDA RID: 4058 RVA: 0x00051A99 File Offset: 0x0004FC99
		// (set) Token: 0x06000FDB RID: 4059 RVA: 0x00051AA1 File Offset: 0x0004FCA1
		public float TotalBlastRadius { get; internal set; } = 4f;

		// Token: 0x1700031C RID: 796
		// (get) Token: 0x06000FDC RID: 4060 RVA: 0x00051AAA File Offset: 0x0004FCAA
		// (set) Token: 0x06000FDD RID: 4061 RVA: 0x00051AB2 File Offset: 0x0004FCB2
		public float CenterBlastForce { get; internal set; } = 12f;

		// Token: 0x1700031D RID: 797
		// (get) Token: 0x06000FDE RID: 4062 RVA: 0x00051ABB File Offset: 0x0004FCBB
		// (set) Token: 0x06000FDF RID: 4063 RVA: 0x00051AC3 File Offset: 0x0004FCC3
		public float EdgeBlastForce { get; internal set; } = 3f;

		// Token: 0x06000FE0 RID: 4064 RVA: 0x00051ACC File Offset: 0x0004FCCC
		protected override object SaveData()
		{
			return new ValueTuple<int, int, float, float, float, float>(this.DamageAtCenter, this.DamageAtEdge, this.CenterRadius, this.TotalBlastRadius, this.CenterBlastForce, this.EdgeBlastForce);
		}

		// Token: 0x06000FE1 RID: 4065 RVA: 0x00051AFC File Offset: 0x0004FCFC
		protected override void LoadData(object data)
		{
			ValueTuple<int, int, float, float, float, float> valueTuple = (ValueTuple<int, int, float, float, float, float>)data;
			this.DamageAtCenter = valueTuple.Item1;
			this.DamageAtEdge = valueTuple.Item2;
			this.CenterRadius = valueTuple.Item3;
			this.TotalBlastRadius = valueTuple.Item4;
			this.CenterBlastForce = valueTuple.Item5;
			this.EdgeBlastForce = valueTuple.Item6;
		}

		// Token: 0x1700031E RID: 798
		// (get) Token: 0x06000FE2 RID: 4066 RVA: 0x00051B58 File Offset: 0x0004FD58
		public object LuaObject
		{
			get
			{
				if (this.luaInterface == null)
				{
					this.luaInterface = new ThrownBomb(this);
				}
				return this.luaInterface;
			}
		}

		// Token: 0x1700031F RID: 799
		// (get) Token: 0x06000FE3 RID: 4067 RVA: 0x00051B74 File Offset: 0x0004FD74
		Type IScriptInjector.LuaObjectType
		{
			get
			{
				return typeof(ThrownBomb);
			}
		}

		// Token: 0x06000FE4 RID: 4068 RVA: 0x0004EE71 File Offset: 0x0004D071
		void IScriptInjector.ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x06000FE6 RID: 4070 RVA: 0x00051BD0 File Offset: 0x0004FDD0
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000FE7 RID: 4071 RVA: 0x0004EEA2 File Offset: 0x0004D0A2
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000FE8 RID: 4072 RVA: 0x00051BE6 File Offset: 0x0004FDE6
		protected internal override string __getTypeName()
		{
			return "ThrownBombItem";
		}

		// Token: 0x04000DB7 RID: 3511
		[SerializeField]
		private Item.VisualsInfo tempVisualsInfoGround;

		// Token: 0x04000DB8 RID: 3512
		[SerializeField]
		private Item.VisualsInfo tempVisualsInfoEqupped;

		// Token: 0x04000DBF RID: 3519
		private ThrownBomb luaInterface;
	}
}
