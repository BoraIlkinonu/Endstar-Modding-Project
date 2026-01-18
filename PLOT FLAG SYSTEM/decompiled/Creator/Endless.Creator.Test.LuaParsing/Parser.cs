using System.Collections.Generic;
using System.Linq;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;
using UnityEngine;

namespace Endless.Creator.Test.LuaParsing;

public class Parser
{
	private TokenType[] BLOCK_TERMINATORS = new TokenType[3]
	{
		TokenType.End,
		TokenType.Elseif,
		TokenType.Else
	};

	private List<Token> tokens;

	private int current;

	private Stack<Environment> environmentStack = new Stack<Environment>();

	private List<Statement> statements = new List<Statement>();

	public List<ParsingError> ErrorMessages { get; } = new List<ParsingError>();

	public Parser(List<Token> tokens)
	{
		this.tokens = tokens;
		environmentStack.Push(new Environment());
	}

	public List<Statement> Parse()
	{
		statements = new List<Statement>();
		int num = 0;
		while (!IsAtEnd() && num++ < 1000)
		{
			int num2 = 0;
			while (Match(TokenType.EndOfLine) && ++num2 < 1000)
			{
			}
			statements.Add(Declaration());
		}
		return statements;
	}

	private Statement Declaration()
	{
		if (Check(TokenType.Local))
		{
			Advance();
		}
		Statement statement = null;
		Token token = Peek();
		Token token2 = Peek(1);
		if (token.Type == TokenType.Function)
		{
			Consume(TokenType.Function, "Expect 'function' before function declaration.");
			statement = Function("function");
		}
		if (token.Type == TokenType.Identifier && token2.Type == TokenType.Equal)
		{
			statement = VariableStatement();
		}
		if (token.Type == TokenType.Identifier && token2.Type == TokenType.Dot && Peek(2).Lexeme == "__index")
		{
			statement = MetaTableIndexStatement();
		}
		if (statement == null)
		{
			statement = Statement();
		}
		if (statement is ExpressionStatement expressionStatement)
		{
			if (expressionStatement.Expression is VariableExpression variableExpression && variableExpression.Name.Type == TokenType.Identifier)
			{
				ErrorMessages.Add(new ParsingError("Identifier Expression statement is invalid.", variableExpression.Name.Line, variableExpression.Name.StartIndex));
				Synchronize();
				return statement;
			}
			if (expressionStatement.Expression == null)
			{
				string message = "Unexpected expression statement.";
				if (token.Type == TokenType.End)
				{
					message = "Unexpected 'end', did you declare an if, function, or while correctly?";
				}
				else if (token.Type == TokenType.Do)
				{
					message = "Unexpected 'do', did you declare a while statement correctly?";
				}
				else if (token.Type == TokenType.Then)
				{
					message = "Unexpected 'then', did you declare an if correctly?";
				}
				Advance();
				ErrorMessages.Add(new ParsingError(message, token.Line, token.StartIndex));
				Synchronize();
				return statement;
			}
		}
		int num = 0;
		while (Check(TokenType.EndOfLine) && ++num < 1000)
		{
			Advance();
		}
		return statement;
	}

	private Statement Function(string kind)
	{
		Token name = Consume(TokenType.Identifier, "Expect " + kind + " name.");
		Token subname = null;
		if (Match(TokenType.Colon))
		{
			subname = Consume(TokenType.Identifier, "Expect identifier after semi colon.");
		}
		Consume(TokenType.LeftParenthesis, "Expect '(' after " + kind + " name.");
		List<Token> list = new List<Token>();
		if (!Check(TokenType.RightParenthesis))
		{
			int num = 0;
			do
			{
				if (list.Count >= 255)
				{
					Debug.LogError("Attempted to parse argument with more than 255 arguments...");
				}
				list.Add(Consume(TokenType.Identifier, "Expect parameter name"));
			}
			while (Match(TokenType.Comma) && num++ < 1000);
		}
		Consume(TokenType.RightParenthesis, "Expect ')' after parameters.");
		return new FunctionStatement(name, subname, list, Block());
	}

	private Statement TableStatement()
	{
		Token name = Consume(TokenType.Identifier, "Expect table name");
		Consume(TokenType.Equal, "Expect '=' after table name identifier");
		Consume(TokenType.LeftBrace, "Expect '{' after table name identifier");
		return new TableStatement(name, Block());
	}

	private Statement VariableStatement()
	{
		Token name = Consume(TokenType.Identifier, "Expected variable name");
		Expression initializer = null;
		List<Statement> tableStatements = null;
		if (Match(TokenType.Equal))
		{
			if (!Match(TokenType.LeftBrace))
			{
				initializer = Expression();
			}
			else if (!Match(TokenType.RightBrace))
			{
				tableStatements = Block();
			}
		}
		if (Peek().Type == TokenType.Semicolon)
		{
			Advance();
		}
		return new VariableStatement(name, initializer, tableStatements);
	}

	private Statement MetaTableIndexStatement()
	{
		Token name = Consume(TokenType.Identifier, "Expect table name to assign __index to");
		Consume(TokenType.Dot, "Expect '.' following table name");
		Consume(TokenType.Identifier, "Expect identifier '__index' following '.'");
		Consume(TokenType.Equal, "Expect '=' following '__index'");
		Token target = Consume(TokenType.Identifier, "Expect target identifier to assign to '__index'");
		return new MetaTableIndexStatement(name, target);
	}

	private Statement Statement()
	{
		if (Match(TokenType.If) || Match(TokenType.Elseif))
		{
			return IfStatement();
		}
		if (Match(TokenType.Return))
		{
			return ReturnStatement();
		}
		if (Match(TokenType.While))
		{
			return WhileStatement();
		}
		if (Match(TokenType.For))
		{
			return ForStatement();
		}
		return ExpressionStatement();
	}

	private Statement IfStatement()
	{
		Expression expression = Expression();
		if (expression == null)
		{
			if (((tokens.Count > current) ? tokens[current] : tokens.LastOrDefault()) != null)
			{
				ErrorMessages.Add(new ParsingError("Expect condition after 'if'.", tokens[current].Line, tokens[current].StartIndex));
			}
			Synchronize();
		}
		Consume(TokenType.Then, "Expected \"then\" after if condition");
		environmentStack.Push(new Environment(environmentStack.Peek()));
		Statement thenBranch = new BlockStatement(Block());
		environmentStack.Pop();
		Statement elseBranch = null;
		if (Match(TokenType.Elseif))
		{
			elseBranch = IfStatement();
		}
		else if (Peek().Type == TokenType.Else)
		{
			Consume(TokenType.Else, "Expected to find 'else'");
			environmentStack.Push(new Environment(environmentStack.Peek()));
			elseBranch = new BlockStatement(Block());
			environmentStack.Pop();
		}
		return new IfStatement(expression, thenBranch, elseBranch);
	}

	private Statement WhileStatement()
	{
		Expression expression = Expression();
		if (expression == null)
		{
			if (((tokens.Count > current) ? tokens[current] : tokens.LastOrDefault()) != null)
			{
				ErrorMessages.Add(new ParsingError("Expect condition after 'while'.", tokens[current].Line, tokens[current].StartIndex));
			}
			Synchronize();
		}
		Consume(TokenType.Do, "Expected \"do\" after while condition");
		environmentStack.Push(new Environment(environmentStack.Peek()));
		Statement body = new BlockStatement(Block());
		environmentStack.Pop();
		return new WhileStatement(expression, body);
	}

	private Statement ForStatement()
	{
		Expression expression = Expression();
		Consume(TokenType.Comma, "Expect ',' after index assignment in 'for' statement.");
		Expression expression2 = Expression();
		if (expression == null)
		{
			if (((tokens.Count > current) ? tokens[current] : tokens.LastOrDefault()) != null)
			{
				ErrorMessages.Add(new ParsingError("Expect initial assignment in 'for' statement.", tokens[current].Line, tokens[current].StartIndex));
			}
			Synchronize();
		}
		if (expression2 == null)
		{
			if (((tokens.Count > current) ? tokens[current] : tokens.LastOrDefault()) != null)
			{
				ErrorMessages.Add(new ParsingError("Expect value statement limit in 'for' statement.", tokens[current].Line, tokens[current].StartIndex));
			}
			Synchronize();
		}
		Expression step = null;
		if (Peek().Type == TokenType.In)
		{
			Consume(TokenType.In, "Expected 'in' after index and value declaration in 'for' statement.");
			step = Expression();
		}
		Consume(TokenType.Do, "Expected 'do' after step expression in 'for' statement.");
		if (expression == null || expression2 == null)
		{
			Synchronize();
		}
		environmentStack.Push(new Environment(environmentStack.Peek()));
		Statement body = new BlockStatement(Block());
		environmentStack.Pop();
		return new ForStatement(expression, expression2, step, body);
	}

	private List<Statement> Block()
	{
		List<Statement> list = new List<Statement>();
		Environment item = new Environment(environmentStack.Peek());
		environmentStack.Push(item);
		int num = 0;
		while (!Check(TokenType.End) && !Check(TokenType.Elseif) && !Check(TokenType.Else) && !Check(TokenType.RightBrace) && !IsAtEnd() && num++ < 1500)
		{
			while (Peek().Type == TokenType.Semicolon || Peek().Type == TokenType.Comma || Peek().Type == TokenType.EndOfLine)
			{
				Advance();
			}
			if (Peek().Type == TokenType.End)
			{
				break;
			}
			list.Add(Declaration());
			while (Peek().Type == TokenType.Semicolon || Peek().Type == TokenType.Comma || Peek().Type == TokenType.EndOfLine)
			{
				Advance();
			}
		}
		if (Check(TokenType.End) || (!Check(TokenType.End) && IsAtEnd()))
		{
			Consume(TokenType.End, "Expected to find 'end'.");
		}
		if (Check(TokenType.RightBrace))
		{
			Consume(TokenType.RightBrace, "Expected to find '}'.");
		}
		environmentStack.Pop();
		return list;
	}

	private Statement PrintStatement()
	{
		if (Peek().Type == TokenType.EndOfLine)
		{
			ErrorMessages.Add(new ParsingError("Expect expression after 'print'.", Peek().Line, Peek().StartIndex));
			Synchronize();
			return new PrintStatement(null);
		}
		return new PrintStatement(Expression());
	}

	private Statement ReturnStatement()
	{
		Token keyword = Previous();
		Expression value = Expression();
		return new ReturnStatement(keyword, value);
	}

	private Statement ExpressionStatement()
	{
		return new ExpressionStatement(Expression());
	}

	private Expression Expression()
	{
		int num = 0;
		Expression result = Assignment();
		while (Match(TokenType.EndOfLine) && ++num < 100)
		{
		}
		return result;
	}

	private Expression Assignment()
	{
		Expression expression = Or();
		if (Match(TokenType.Equal))
		{
			Expression value = Assignment();
			if (expression is VariableExpression variableExpression)
			{
				return new AssignmentExpression(variableExpression.Name, value);
			}
			if (expression is GetExpression getExpression)
			{
				return new SetExpression(getExpression.Obj, getExpression.Name, value);
			}
			if (((tokens.Count > current) ? tokens[current] : tokens.LastOrDefault()) != null)
			{
				ErrorMessages.Add(new ParsingError("Invalid assignment target", tokens[current].Line, tokens[current].StartIndex));
			}
		}
		return expression;
	}

	private Expression Or()
	{
		Expression expression = And();
		while (Match(TokenType.Or))
		{
			Token op = Previous();
			Expression right = And();
			expression = new LogicalExpression(expression, op, right);
		}
		return expression;
	}

	private Expression And()
	{
		Expression expression = Equality();
		while (Match(TokenType.And))
		{
			Token op = Previous();
			Expression right = Equality();
			expression = new LogicalExpression(expression, op, right);
		}
		return expression;
	}

	private Expression Equality()
	{
		Expression expression = Comparison();
		while (Match(TokenType.BangEqual, TokenType.EqualEqual))
		{
			Token op = Previous();
			Expression right = Comparison();
			expression = new BinaryExpression(expression, op, right);
		}
		return expression;
	}

	private Expression Comparison()
	{
		Expression expression = Term();
		while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
		{
			Token op = Previous();
			Expression right = Term();
			expression = new BinaryExpression(expression, op, right);
		}
		return expression;
	}

	private Expression Term()
	{
		Expression expression = Factor();
		while (Match(TokenType.Minus, TokenType.Plus))
		{
			Token op = Previous();
			Expression right = Factor();
			expression = new BinaryExpression(expression, op, right);
		}
		return expression;
	}

	private Expression Factor()
	{
		Expression expression = Unary();
		while (Match(TokenType.Slash, TokenType.Star))
		{
			Token op = Previous();
			Expression right = Unary();
			expression = new BinaryExpression(expression, op, right);
		}
		return expression;
	}

	private Expression Unary()
	{
		if (Match(TokenType.Bang, TokenType.Minus))
		{
			Token op = Previous();
			Expression right = Unary();
			return new UnaryExpression(op, right);
		}
		return Concatenate();
	}

	private Expression Concatenate()
	{
		Expression expression = Call();
		if (Match(TokenType.Concatenate))
		{
			Token op = Previous();
			Expression right = Expression();
			return new ConcatenateExpression(expression, op, right);
		}
		return expression;
	}

	private Expression Call()
	{
		Expression expression = Primary();
		while (true)
		{
			if (Match(default(TokenType)))
			{
				expression = FinishCall(expression);
				continue;
			}
			if (Match(TokenType.LeftBracket))
			{
				expression = Expression();
				Consume(TokenType.RightBracket, "Expect ']' after index expression");
				continue;
			}
			if (!Match(TokenType.Colon, TokenType.Dot))
			{
				break;
			}
			Token name = Consume(TokenType.Identifier, "Expect method name after '.'.");
			expression = new GetExpression(expression, name);
		}
		return expression;
	}

	private Expression FinishCall(Expression callee)
	{
		List<Expression> list = new List<Expression>();
		if (!Check(TokenType.RightParenthesis))
		{
			do
			{
				list.Add(Expression());
			}
			while (Match(TokenType.Comma));
		}
		Token paren = Consume(TokenType.RightParenthesis, "Expected ')' after arguments.");
		return new CallExpression(callee, paren, list);
	}

	private Expression Primary()
	{
		bool negated = Match(TokenType.Not);
		if (Match(TokenType.False))
		{
			return new LiteralExpression(false, negated);
		}
		if (Match(TokenType.True))
		{
			return new LiteralExpression(true, negated);
		}
		if (Match(TokenType.Nil))
		{
			return new LiteralExpression(null, negated);
		}
		if (Match(TokenType.Number, TokenType.String))
		{
			return new LiteralExpression(Previous().Literal, negated);
		}
		if (Match(TokenType.Hash))
		{
			return new GetExpression(null, Advance());
		}
		if (Match(TokenType.Identifier, TokenType.Print))
		{
			return new VariableExpression(Previous(), negated);
		}
		if (Match(default(TokenType)))
		{
			Expression expression = Expression();
			Consume(TokenType.RightParenthesis, "Expect ')' after expression.");
			return new GroupingExpression(expression);
		}
		return null;
	}

	private bool Match(params TokenType[] tokenTypes)
	{
		foreach (TokenType type in tokenTypes)
		{
			if (Check(type))
			{
				Advance();
				return true;
			}
		}
		return false;
	}

	private bool Check(TokenType type)
	{
		if (IsAtEnd())
		{
			return false;
		}
		return Peek().Type == type;
	}

	private Token Advance()
	{
		if (!IsAtEnd())
		{
			current++;
		}
		return Previous();
	}

	private bool IsAtEnd()
	{
		return Peek().Type == TokenType.EndOfFile;
	}

	private Token Peek(int lookAhead = 0)
	{
		if (current + lookAhead >= tokens.Count)
		{
			return null;
		}
		return tokens[current + lookAhead];
	}

	private Token Previous()
	{
		return tokens[current - 1];
	}

	private Token Consume(TokenType type, string message)
	{
		int line = Peek().Line;
		int startIndex = Peek().StartIndex;
		if (Check(type))
		{
			return Advance();
		}
		ErrorMessages.Add(new ParsingError(message, line, startIndex));
		Synchronize();
		return null;
	}

	private void Synchronize()
	{
		if (Previous().Type != TokenType.EndOfLine)
		{
			while (!Check(TokenType.EndOfLine) && !IsAtEnd())
			{
				Advance();
			}
			Advance();
		}
	}
}
