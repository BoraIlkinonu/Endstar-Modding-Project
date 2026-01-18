using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions
{
	// Token: 0x02000340 RID: 832
	public class VariableExpression : Expression
	{
		// Token: 0x17000256 RID: 598
		// (get) Token: 0x06000F63 RID: 3939 RVA: 0x000474BD File Offset: 0x000456BD
		public bool Negated { get; }

		// Token: 0x17000257 RID: 599
		// (get) Token: 0x06000F64 RID: 3940 RVA: 0x000474C5 File Offset: 0x000456C5
		public Token Name { get; }

		// Token: 0x06000F65 RID: 3941 RVA: 0x000474CD File Offset: 0x000456CD
		public VariableExpression(Token name, bool negated)
		{
			this.Name = name;
			this.Negated = negated;
		}

		// Token: 0x06000F66 RID: 3942 RVA: 0x000474E3 File Offset: 0x000456E3
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitVariableExpression(this);
		}
	}
}
