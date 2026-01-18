using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;
using UnityEngine;

namespace Endless.Creator.Test.LuaParsing
{
	// Token: 0x0200031C RID: 796
	public class Parser
	{
		// Token: 0x17000213 RID: 531
		// (get) Token: 0x06000E6A RID: 3690 RVA: 0x000442D8 File Offset: 0x000424D8
		public List<ParsingError> ErrorMessages { get; } = new List<ParsingError>();

		// Token: 0x06000E6B RID: 3691 RVA: 0x000442E0 File Offset: 0x000424E0
		public Parser(List<Token> tokens)
		{
			this.tokens = tokens;
			this.environmentStack.Push(new Environment());
		}

		// Token: 0x06000E6C RID: 3692 RVA: 0x00044344 File Offset: 0x00042544
		public List<Statement> Parse()
		{
			this.statements = new List<Statement>();
			int num = 0;
			while (!this.IsAtEnd() && num++ < 1000)
			{
				int num2 = 0;
				while (this.Match(new TokenType[] { TokenType.EndOfLine }) && ++num2 < 1000)
				{
				}
				this.statements.Add(this.Declaration());
			}
			return this.statements;
		}

		// Token: 0x06000E6D RID: 3693 RVA: 0x000443AC File Offset: 0x000425AC
		private Statement Declaration()
		{
			if (this.Check(TokenType.Local))
			{
				this.Advance();
			}
			Statement statement = null;
			Token token = this.Peek(0);
			Token token2 = this.Peek(1);
			if (token.Type == TokenType.Function)
			{
				this.Consume(TokenType.Function, "Expect 'function' before function declaration.");
				statement = this.Function("function");
			}
			if (token.Type == TokenType.Identifier && token2.Type == TokenType.Equal)
			{
				statement = this.VariableStatement();
			}
			if (token.Type == TokenType.Identifier && token2.Type == TokenType.Dot && this.Peek(2).Lexeme == "__index")
			{
				statement = this.MetaTableIndexStatement();
			}
			if (statement == null)
			{
				statement = this.Statement();
			}
			ExpressionStatement expressionStatement = statement as ExpressionStatement;
			if (expressionStatement != null)
			{
				VariableExpression variableExpression = expressionStatement.Expression as VariableExpression;
				if (variableExpression != null && variableExpression.Name.Type == TokenType.Identifier)
				{
					this.ErrorMessages.Add(new ParsingError("Identifier Expression statement is invalid.", variableExpression.Name.Line, variableExpression.Name.StartIndex));
					this.Synchronize();
					return statement;
				}
				if (expressionStatement.Expression == null)
				{
					string text = "Unexpected expression statement.";
					if (token.Type == TokenType.End)
					{
						text = "Unexpected 'end', did you declare an if, function, or while correctly?";
					}
					else if (token.Type == TokenType.Do)
					{
						text = "Unexpected 'do', did you declare a while statement correctly?";
					}
					else if (token.Type == TokenType.Then)
					{
						text = "Unexpected 'then', did you declare an if correctly?";
					}
					this.Advance();
					this.ErrorMessages.Add(new ParsingError(text, token.Line, token.StartIndex));
					this.Synchronize();
					return statement;
				}
			}
			int num = 0;
			while (this.Check(TokenType.EndOfLine) && ++num < 1000)
			{
				this.Advance();
			}
			return statement;
		}

		// Token: 0x06000E6E RID: 3694 RVA: 0x00044554 File Offset: 0x00042754
		private Statement Function(string kind)
		{
			Token token = this.Consume(TokenType.Identifier, "Expect " + kind + " name.");
			Token token2 = null;
			if (this.Match(new TokenType[] { TokenType.Colon }))
			{
				token2 = this.Consume(TokenType.Identifier, "Expect identifier after semi colon.");
			}
			this.Consume(TokenType.LeftParenthesis, "Expect '(' after " + kind + " name.");
			List<Token> list = new List<Token>();
			if (!this.Check(TokenType.RightParenthesis))
			{
				int num = 0;
				do
				{
					if (list.Count >= 255)
					{
						Debug.LogError("Attempted to parse argument with more than 255 arguments...");
					}
					list.Add(this.Consume(TokenType.Identifier, "Expect parameter name"));
				}
				while (this.Match(new TokenType[] { TokenType.Comma }) && num++ < 1000);
			}
			this.Consume(TokenType.RightParenthesis, "Expect ')' after parameters.");
			return new FunctionStatement(token, token2, list, this.Block());
		}

		// Token: 0x06000E6F RID: 3695 RVA: 0x0004462A File Offset: 0x0004282A
		private Statement TableStatement()
		{
			Token token = this.Consume(TokenType.Identifier, "Expect table name");
			this.Consume(TokenType.Equal, "Expect '=' after table name identifier");
			this.Consume(TokenType.LeftBrace, "Expect '{' after table name identifier");
			return new TableStatement(token, this.Block());
		}

		// Token: 0x06000E70 RID: 3696 RVA: 0x00044660 File Offset: 0x00042860
		private Statement VariableStatement()
		{
			Token token = this.Consume(TokenType.Identifier, "Expected variable name");
			Expression expression = null;
			List<Statement> list = null;
			if (this.Match(new TokenType[] { TokenType.Equal }))
			{
				if (!this.Match(new TokenType[] { TokenType.LeftBrace }))
				{
					expression = this.Expression();
				}
				else if (!this.Match(new TokenType[] { TokenType.RightBrace }))
				{
					list = this.Block();
				}
			}
			if (this.Peek(0).Type == TokenType.Semicolon)
			{
				this.Advance();
			}
			return new VariableStatement(token, expression, list);
		}

		// Token: 0x06000E71 RID: 3697 RVA: 0x000446E8 File Offset: 0x000428E8
		private Statement MetaTableIndexStatement()
		{
			Token token = this.Consume(TokenType.Identifier, "Expect table name to assign __index to");
			this.Consume(TokenType.Dot, "Expect '.' following table name");
			this.Consume(TokenType.Identifier, "Expect identifier '__index' following '.'");
			this.Consume(TokenType.Equal, "Expect '=' following '__index'");
			Token token2 = this.Consume(TokenType.Identifier, "Expect target identifier to assign to '__index'");
			return new MetaTableIndexStatement(token, token2);
		}

		// Token: 0x06000E72 RID: 3698 RVA: 0x00044740 File Offset: 0x00042940
		private Statement Statement()
		{
			if (this.Match(new TokenType[] { TokenType.If }) || this.Match(new TokenType[] { TokenType.Elseif }))
			{
				return this.IfStatement();
			}
			if (this.Match(new TokenType[] { TokenType.Return }))
			{
				return this.ReturnStatement();
			}
			if (this.Match(new TokenType[] { TokenType.While }))
			{
				return this.WhileStatement();
			}
			if (this.Match(new TokenType[] { TokenType.For }))
			{
				return this.ForStatement();
			}
			return this.ExpressionStatement();
		}

		// Token: 0x06000E73 RID: 3699 RVA: 0x000447D0 File Offset: 0x000429D0
		private Statement IfStatement()
		{
			Expression expression = this.Expression();
			if (expression == null)
			{
				if (((this.tokens.Count > this.current) ? this.tokens[this.current] : this.tokens.LastOrDefault<Token>()) != null)
				{
					this.ErrorMessages.Add(new ParsingError("Expect condition after 'if'.", this.tokens[this.current].Line, this.tokens[this.current].StartIndex));
				}
				this.Synchronize();
			}
			this.Consume(TokenType.Then, "Expected \"then\" after if condition");
			this.environmentStack.Push(new Environment(this.environmentStack.Peek()));
			Statement statement = new BlockStatement(this.Block());
			this.environmentStack.Pop();
			Statement statement2 = null;
			if (this.Match(new TokenType[] { TokenType.Elseif }))
			{
				statement2 = this.IfStatement();
			}
			else if (this.Peek(0).Type == TokenType.Else)
			{
				this.Consume(TokenType.Else, "Expected to find 'else'");
				this.environmentStack.Push(new Environment(this.environmentStack.Peek()));
				statement2 = new BlockStatement(this.Block());
				this.environmentStack.Pop();
			}
			return new IfStatement(expression, statement, statement2);
		}

		// Token: 0x06000E74 RID: 3700 RVA: 0x0004491C File Offset: 0x00042B1C
		private Statement WhileStatement()
		{
			Expression expression = this.Expression();
			if (expression == null)
			{
				if (((this.tokens.Count > this.current) ? this.tokens[this.current] : this.tokens.LastOrDefault<Token>()) != null)
				{
					this.ErrorMessages.Add(new ParsingError("Expect condition after 'while'.", this.tokens[this.current].Line, this.tokens[this.current].StartIndex));
				}
				this.Synchronize();
			}
			this.Consume(TokenType.Do, "Expected \"do\" after while condition");
			this.environmentStack.Push(new Environment(this.environmentStack.Peek()));
			Statement statement = new BlockStatement(this.Block());
			this.environmentStack.Pop();
			return new WhileStatement(expression, statement);
		}

		// Token: 0x06000E75 RID: 3701 RVA: 0x000449F4 File Offset: 0x00042BF4
		private Statement ForStatement()
		{
			Expression expression = this.Expression();
			this.Consume(TokenType.Comma, "Expect ',' after index assignment in 'for' statement.");
			Expression expression2 = this.Expression();
			if (expression == null)
			{
				if (((this.tokens.Count > this.current) ? this.tokens[this.current] : this.tokens.LastOrDefault<Token>()) != null)
				{
					this.ErrorMessages.Add(new ParsingError("Expect initial assignment in 'for' statement.", this.tokens[this.current].Line, this.tokens[this.current].StartIndex));
				}
				this.Synchronize();
			}
			if (expression2 == null)
			{
				if (((this.tokens.Count > this.current) ? this.tokens[this.current] : this.tokens.LastOrDefault<Token>()) != null)
				{
					this.ErrorMessages.Add(new ParsingError("Expect value statement limit in 'for' statement.", this.tokens[this.current].Line, this.tokens[this.current].StartIndex));
				}
				this.Synchronize();
			}
			Expression expression3 = null;
			if (this.Peek(0).Type == TokenType.In)
			{
				this.Consume(TokenType.In, "Expected 'in' after index and value declaration in 'for' statement.");
				expression3 = this.Expression();
			}
			this.Consume(TokenType.Do, "Expected 'do' after step expression in 'for' statement.");
			if (expression == null || expression2 == null)
			{
				this.Synchronize();
			}
			this.environmentStack.Push(new Environment(this.environmentStack.Peek()));
			Statement statement = new BlockStatement(this.Block());
			this.environmentStack.Pop();
			return new ForStatement(expression, expression2, expression3, statement);
		}

		// Token: 0x06000E76 RID: 3702 RVA: 0x00044B94 File Offset: 0x00042D94
		private List<Statement> Block()
		{
			List<Statement> list = new List<Statement>();
			Environment environment = new Environment(this.environmentStack.Peek());
			this.environmentStack.Push(environment);
			int num = 0;
			while (!this.Check(TokenType.End) && !this.Check(TokenType.Elseif) && !this.Check(TokenType.Else) && !this.Check(TokenType.RightBrace) && !this.IsAtEnd() && num++ < 1500)
			{
				while (this.Peek(0).Type == TokenType.Semicolon || this.Peek(0).Type == TokenType.Comma || this.Peek(0).Type == TokenType.EndOfLine)
				{
					this.Advance();
				}
				if (this.Peek(0).Type == TokenType.End)
				{
					break;
				}
				list.Add(this.Declaration());
				while (this.Peek(0).Type == TokenType.Semicolon || this.Peek(0).Type == TokenType.Comma || this.Peek(0).Type == TokenType.EndOfLine)
				{
					this.Advance();
				}
			}
			if (this.Check(TokenType.End) || (!this.Check(TokenType.End) && this.IsAtEnd()))
			{
				this.Consume(TokenType.End, "Expected to find 'end'.");
			}
			if (this.Check(TokenType.RightBrace))
			{
				this.Consume(TokenType.RightBrace, "Expected to find '}'.");
			}
			this.environmentStack.Pop();
			return list;
		}

		// Token: 0x06000E77 RID: 3703 RVA: 0x00044CE8 File Offset: 0x00042EE8
		private Statement PrintStatement()
		{
			if (this.Peek(0).Type == TokenType.EndOfLine)
			{
				this.ErrorMessages.Add(new ParsingError("Expect expression after 'print'.", this.Peek(0).Line, this.Peek(0).StartIndex));
				this.Synchronize();
				return new PrintStatement(null);
			}
			return new PrintStatement(this.Expression());
		}

		// Token: 0x06000E78 RID: 3704 RVA: 0x00044D4C File Offset: 0x00042F4C
		private Statement ReturnStatement()
		{
			Token token = this.Previous();
			Expression expression = this.Expression();
			return new ReturnStatement(token, expression);
		}

		// Token: 0x06000E79 RID: 3705 RVA: 0x00044D6C File Offset: 0x00042F6C
		private Statement ExpressionStatement()
		{
			return new ExpressionStatement(this.Expression());
		}

		// Token: 0x06000E7A RID: 3706 RVA: 0x00044D7C File Offset: 0x00042F7C
		private Expression Expression()
		{
			int num = 0;
			Expression expression = this.Assignment();
			while (this.Match(new TokenType[] { TokenType.EndOfLine }) && ++num < 100)
			{
			}
			return expression;
		}

		// Token: 0x06000E7B RID: 3707 RVA: 0x00044DB0 File Offset: 0x00042FB0
		private Expression Assignment()
		{
			Expression expression = this.Or();
			if (this.Match(new TokenType[] { TokenType.Equal }))
			{
				Expression expression2 = this.Assignment();
				VariableExpression variableExpression = expression as VariableExpression;
				if (variableExpression != null)
				{
					return new AssignmentExpression(variableExpression.Name, expression2);
				}
				GetExpression getExpression = expression as GetExpression;
				if (getExpression != null)
				{
					return new SetExpression(getExpression.Obj, getExpression.Name, expression2);
				}
				if (((this.tokens.Count > this.current) ? this.tokens[this.current] : this.tokens.LastOrDefault<Token>()) != null)
				{
					this.ErrorMessages.Add(new ParsingError("Invalid assignment target", this.tokens[this.current].Line, this.tokens[this.current].StartIndex));
				}
			}
			return expression;
		}

		// Token: 0x06000E7C RID: 3708 RVA: 0x00044E8C File Offset: 0x0004308C
		private Expression Or()
		{
			Expression expression = this.And();
			while (this.Match(new TokenType[] { TokenType.Or }))
			{
				Token token = this.Previous();
				Expression expression2 = this.And();
				expression = new LogicalExpression(expression, token, expression2);
			}
			return expression;
		}

		// Token: 0x06000E7D RID: 3709 RVA: 0x00044ED0 File Offset: 0x000430D0
		private Expression And()
		{
			Expression expression = this.Equality();
			while (this.Match(new TokenType[] { TokenType.And }))
			{
				Token token = this.Previous();
				Expression expression2 = this.Equality();
				expression = new LogicalExpression(expression, token, expression2);
			}
			return expression;
		}

		// Token: 0x06000E7E RID: 3710 RVA: 0x00044F14 File Offset: 0x00043114
		private Expression Equality()
		{
			Expression expression = this.Comparison();
			while (this.Match(new TokenType[]
			{
				TokenType.BangEqual,
				TokenType.EqualEqual
			}))
			{
				Token token = this.Previous();
				Expression expression2 = this.Comparison();
				expression = new BinaryExpression(expression, token, expression2);
			}
			return expression;
		}

		// Token: 0x06000E7F RID: 3711 RVA: 0x00044F5C File Offset: 0x0004315C
		private Expression Comparison()
		{
			Expression expression = this.Term();
			while (this.Match(new TokenType[]
			{
				TokenType.Greater,
				TokenType.GreaterEqual,
				TokenType.Less,
				TokenType.LessEqual
			}))
			{
				Token token = this.Previous();
				Expression expression2 = this.Term();
				expression = new BinaryExpression(expression, token, expression2);
			}
			return expression;
		}

		// Token: 0x06000E80 RID: 3712 RVA: 0x00044FA4 File Offset: 0x000431A4
		private Expression Term()
		{
			Expression expression = this.Factor();
			while (this.Match(new TokenType[]
			{
				TokenType.Minus,
				TokenType.Plus
			}))
			{
				Token token = this.Previous();
				Expression expression2 = this.Factor();
				expression = new BinaryExpression(expression, token, expression2);
			}
			return expression;
		}

		// Token: 0x06000E81 RID: 3713 RVA: 0x00044FE8 File Offset: 0x000431E8
		private Expression Factor()
		{
			Expression expression = this.Unary();
			while (this.Match(new TokenType[]
			{
				TokenType.Slash,
				TokenType.Star
			}))
			{
				Token token = this.Previous();
				Expression expression2 = this.Unary();
				expression = new BinaryExpression(expression, token, expression2);
			}
			return expression;
		}

		// Token: 0x06000E82 RID: 3714 RVA: 0x00045030 File Offset: 0x00043230
		private Expression Unary()
		{
			if (this.Match(new TokenType[]
			{
				TokenType.Bang,
				TokenType.Minus
			}))
			{
				Token token = this.Previous();
				Expression expression = this.Unary();
				return new UnaryExpression(token, expression);
			}
			return this.Concatenate();
		}

		// Token: 0x06000E83 RID: 3715 RVA: 0x00045070 File Offset: 0x00043270
		private Expression Concatenate()
		{
			Expression expression = this.Call();
			if (this.Match(new TokenType[] { TokenType.Concatenate }))
			{
				Token token = this.Previous();
				Expression expression2 = this.Expression();
				return new ConcatenateExpression(expression, token, expression2);
			}
			return expression;
		}

		// Token: 0x06000E84 RID: 3716 RVA: 0x000450B0 File Offset: 0x000432B0
		private Expression Call()
		{
			Expression expression = this.Primary();
			for (;;)
			{
				if (this.Match(new TokenType[1]))
				{
					expression = this.FinishCall(expression);
				}
				else if (this.Match(new TokenType[] { TokenType.LeftBracket }))
				{
					expression = this.Expression();
					this.Consume(TokenType.RightBracket, "Expect ']' after index expression");
				}
				else
				{
					if (!this.Match(new TokenType[]
					{
						TokenType.Colon,
						TokenType.Dot
					}))
					{
						break;
					}
					Token token = this.Consume(TokenType.Identifier, "Expect method name after '.'.");
					expression = new GetExpression(expression, token);
				}
			}
			return expression;
		}

		// Token: 0x06000E85 RID: 3717 RVA: 0x00045134 File Offset: 0x00043334
		private Expression FinishCall(Expression callee)
		{
			List<Expression> list = new List<Expression>();
			if (!this.Check(TokenType.RightParenthesis))
			{
				do
				{
					list.Add(this.Expression());
				}
				while (this.Match(new TokenType[] { TokenType.Comma }));
			}
			Token token = this.Consume(TokenType.RightParenthesis, "Expected ')' after arguments.");
			return new CallExpression(callee, token, list);
		}

		// Token: 0x06000E86 RID: 3718 RVA: 0x00045184 File Offset: 0x00043384
		private Expression Primary()
		{
			bool flag = this.Match(new TokenType[] { TokenType.Not });
			if (this.Match(new TokenType[] { TokenType.False }))
			{
				return new LiteralExpression(false, flag);
			}
			if (this.Match(new TokenType[] { TokenType.True }))
			{
				return new LiteralExpression(true, flag);
			}
			if (this.Match(new TokenType[] { TokenType.Nil }))
			{
				return new LiteralExpression(null, flag);
			}
			if (this.Match(new TokenType[]
			{
				TokenType.Number,
				TokenType.String
			}))
			{
				return new LiteralExpression(this.Previous().Literal, flag);
			}
			if (this.Match(new TokenType[] { TokenType.Hash }))
			{
				return new GetExpression(null, this.Advance());
			}
			if (this.Match(new TokenType[]
			{
				TokenType.Identifier,
				TokenType.Print
			}))
			{
				return new VariableExpression(this.Previous(), flag);
			}
			if (this.Match(new TokenType[1]))
			{
				Expression expression = this.Expression();
				this.Consume(TokenType.RightParenthesis, "Expect ')' after expression.");
				return new GroupingExpression(expression);
			}
			return null;
		}

		// Token: 0x06000E87 RID: 3719 RVA: 0x00045298 File Offset: 0x00043498
		private bool Match(params TokenType[] tokenTypes)
		{
			foreach (TokenType tokenType in tokenTypes)
			{
				if (this.Check(tokenType))
				{
					this.Advance();
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000E88 RID: 3720 RVA: 0x000452CC File Offset: 0x000434CC
		private bool Check(TokenType type)
		{
			return !this.IsAtEnd() && this.Peek(0).Type == type;
		}

		// Token: 0x06000E89 RID: 3721 RVA: 0x000452E7 File Offset: 0x000434E7
		private Token Advance()
		{
			if (!this.IsAtEnd())
			{
				this.current++;
			}
			return this.Previous();
		}

		// Token: 0x06000E8A RID: 3722 RVA: 0x00045305 File Offset: 0x00043505
		private bool IsAtEnd()
		{
			return this.Peek(0).Type == TokenType.EndOfFile;
		}

		// Token: 0x06000E8B RID: 3723 RVA: 0x00045317 File Offset: 0x00043517
		private Token Peek(int lookAhead = 0)
		{
			if (this.current + lookAhead >= this.tokens.Count)
			{
				return null;
			}
			return this.tokens[this.current + lookAhead];
		}

		// Token: 0x06000E8C RID: 3724 RVA: 0x00045343 File Offset: 0x00043543
		private Token Previous()
		{
			return this.tokens[this.current - 1];
		}

		// Token: 0x06000E8D RID: 3725 RVA: 0x00045358 File Offset: 0x00043558
		private Token Consume(TokenType type, string message)
		{
			int line = this.Peek(0).Line;
			int startIndex = this.Peek(0).StartIndex;
			if (this.Check(type))
			{
				return this.Advance();
			}
			this.ErrorMessages.Add(new ParsingError(message, line, startIndex));
			this.Synchronize();
			return null;
		}

		// Token: 0x06000E8E RID: 3726 RVA: 0x000453A9 File Offset: 0x000435A9
		private void Synchronize()
		{
			if (this.Previous().Type == TokenType.EndOfLine)
			{
				return;
			}
			while (!this.Check(TokenType.EndOfLine) && !this.IsAtEnd())
			{
				this.Advance();
			}
			this.Advance();
		}

		// Token: 0x04000C2B RID: 3115
		private TokenType[] BLOCK_TERMINATORS = new TokenType[]
		{
			TokenType.End,
			TokenType.Elseif,
			TokenType.Else
		};

		// Token: 0x04000C2C RID: 3116
		private List<Token> tokens;

		// Token: 0x04000C2D RID: 3117
		private int current;

		// Token: 0x04000C2E RID: 3118
		private Stack<Environment> environmentStack = new Stack<Environment>();

		// Token: 0x04000C2F RID: 3119
		private List<Statement> statements = new List<Statement>();
	}
}
