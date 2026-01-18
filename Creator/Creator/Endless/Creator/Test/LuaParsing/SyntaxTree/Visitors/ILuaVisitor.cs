using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors
{
	// Token: 0x02000327 RID: 807
	public interface ILuaVisitor
	{
		// Token: 0x06000ED4 RID: 3796
		void VisitBinaryExpression(BinaryExpression binaryExpression);

		// Token: 0x06000ED5 RID: 3797
		void VisitCallExpression(CallExpression callExpression);

		// Token: 0x06000ED6 RID: 3798
		void VisitGetExpression(GetExpression getExpression);

		// Token: 0x06000ED7 RID: 3799
		void VisitSetExpression(SetExpression setExpression);

		// Token: 0x06000ED8 RID: 3800
		void VisitGroupingExpression(GroupingExpression groupingExpression);

		// Token: 0x06000ED9 RID: 3801
		void VisitLiteralExpression(LiteralExpression literalExpression);

		// Token: 0x06000EDA RID: 3802
		void VisitLogicalExpression(LogicalExpression logicalExpression);

		// Token: 0x06000EDB RID: 3803
		void VisitUnaryExpression(UnaryExpression unaryExpression);

		// Token: 0x06000EDC RID: 3804
		void VisitConcatenateExpression(ConcatenateExpression unaryExpression);

		// Token: 0x06000EDD RID: 3805
		void VisitVariableExpression(VariableExpression variableExpression);

		// Token: 0x06000EDE RID: 3806
		void VisitAssignmentExpression(AssignmentExpression assignmentExpression);

		// Token: 0x06000EDF RID: 3807
		void VisitBlockStatement(BlockStatement blockStatement);

		// Token: 0x06000EE0 RID: 3808
		void VisitExpressionStatement(ExpressionStatement expressionStatement);

		// Token: 0x06000EE1 RID: 3809
		void VisitIfStatement(IfStatement ifStatement);

		// Token: 0x06000EE2 RID: 3810
		void VisitPrintStatement(PrintStatement printStatement);

		// Token: 0x06000EE3 RID: 3811
		void VisitFunctionStatement(FunctionStatement functionStatement);

		// Token: 0x06000EE4 RID: 3812
		void VisitReturnStatement(ReturnStatement returnStatement);

		// Token: 0x06000EE5 RID: 3813
		void VisitVariableStatement(VariableStatement variableStatement);

		// Token: 0x06000EE6 RID: 3814
		void VisitWhileStatement(WhileStatement whileStatement);

		// Token: 0x06000EE7 RID: 3815
		void VisitForStatement(ForStatement whileStatement);

		// Token: 0x06000EE8 RID: 3816
		void VisitTableStatement(TableStatement tableStatement);

		// Token: 0x06000EE9 RID: 3817
		void VisitMetaTableIndexStatement(MetaTableIndexStatement metaTableIndexStatement);
	}
}
