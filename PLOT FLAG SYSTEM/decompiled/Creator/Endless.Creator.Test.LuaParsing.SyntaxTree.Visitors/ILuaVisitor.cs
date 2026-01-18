using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

public interface ILuaVisitor
{
	void VisitBinaryExpression(BinaryExpression binaryExpression);

	void VisitCallExpression(CallExpression callExpression);

	void VisitGetExpression(GetExpression getExpression);

	void VisitSetExpression(SetExpression setExpression);

	void VisitGroupingExpression(GroupingExpression groupingExpression);

	void VisitLiteralExpression(LiteralExpression literalExpression);

	void VisitLogicalExpression(LogicalExpression logicalExpression);

	void VisitUnaryExpression(UnaryExpression unaryExpression);

	void VisitConcatenateExpression(ConcatenateExpression unaryExpression);

	void VisitVariableExpression(VariableExpression variableExpression);

	void VisitAssignmentExpression(AssignmentExpression assignmentExpression);

	void VisitBlockStatement(BlockStatement blockStatement);

	void VisitExpressionStatement(ExpressionStatement expressionStatement);

	void VisitIfStatement(IfStatement ifStatement);

	void VisitPrintStatement(PrintStatement printStatement);

	void VisitFunctionStatement(FunctionStatement functionStatement);

	void VisitReturnStatement(ReturnStatement returnStatement);

	void VisitVariableStatement(VariableStatement variableStatement);

	void VisitWhileStatement(WhileStatement whileStatement);

	void VisitForStatement(ForStatement whileStatement);

	void VisitTableStatement(TableStatement tableStatement);

	void VisitMetaTableIndexStatement(MetaTableIndexStatement metaTableIndexStatement);
}
