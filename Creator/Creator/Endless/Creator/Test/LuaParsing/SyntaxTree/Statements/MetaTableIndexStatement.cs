using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements
{
	// Token: 0x0200032E RID: 814
	public class MetaTableIndexStatement : Statement
	{
		// Token: 0x17000232 RID: 562
		// (get) Token: 0x06000F1B RID: 3867 RVA: 0x00047191 File Offset: 0x00045391
		public Token Name { get; }

		// Token: 0x17000233 RID: 563
		// (get) Token: 0x06000F1C RID: 3868 RVA: 0x00047199 File Offset: 0x00045399
		public Token Target { get; }

		// Token: 0x06000F1D RID: 3869 RVA: 0x000471A1 File Offset: 0x000453A1
		public MetaTableIndexStatement(Token name, Token target)
		{
			this.Name = name;
			this.Target = target;
		}

		// Token: 0x06000F1E RID: 3870 RVA: 0x000471B7 File Offset: 0x000453B7
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitMetaTableIndexStatement(this);
		}
	}
}
