using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions
{
	// Token: 0x02000335 RID: 821
	public class AssignmentExpression : Expression
	{
		// Token: 0x1700023E RID: 574
		// (get) Token: 0x06000F35 RID: 3893 RVA: 0x000472AB File Offset: 0x000454AB
		public Token Name { get; }

		// Token: 0x1700023F RID: 575
		// (get) Token: 0x06000F36 RID: 3894 RVA: 0x000472B3 File Offset: 0x000454B3
		public Expression Value { get; }

		// Token: 0x06000F37 RID: 3895 RVA: 0x000472BB File Offset: 0x000454BB
		public AssignmentExpression(Token name, Expression value)
		{
			this.Name = name;
			this.Value = value;
		}

		// Token: 0x06000F38 RID: 3896 RVA: 0x000472D1 File Offset: 0x000454D1
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitAssignmentExpression(this);
		}
	}
}
