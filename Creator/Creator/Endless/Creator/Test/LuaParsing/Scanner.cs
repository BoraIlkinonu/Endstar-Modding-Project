using System;
using System.Collections.Generic;

namespace Endless.Creator.Test.LuaParsing
{
	// Token: 0x0200031D RID: 797
	public class Scanner
	{
		// Token: 0x06000E8F RID: 3727 RVA: 0x000453DC File Offset: 0x000435DC
		public Scanner(string source, Action<int, string> errorCallback)
		{
			this.source = source;
			this.errorCallback = errorCallback;
			this.keywordMap.Add("and", TokenType.And);
			this.keywordMap.Add("break", TokenType.Break);
			this.keywordMap.Add("do", TokenType.Do);
			this.keywordMap.Add("if", TokenType.If);
			this.keywordMap.Add("else", TokenType.Else);
			this.keywordMap.Add("elseif", TokenType.Elseif);
			this.keywordMap.Add("end", TokenType.End);
			this.keywordMap.Add("true", TokenType.True);
			this.keywordMap.Add("false", TokenType.False);
			this.keywordMap.Add("for", TokenType.For);
			this.keywordMap.Add("function", TokenType.Function);
			this.keywordMap.Add("in", TokenType.In);
			this.keywordMap.Add("local", TokenType.Local);
			this.keywordMap.Add("nil", TokenType.Nil);
			this.keywordMap.Add("not", TokenType.Not);
			this.keywordMap.Add("or", TokenType.Or);
			this.keywordMap.Add("repeat", TokenType.Repeat);
			this.keywordMap.Add("return", TokenType.Return);
			this.keywordMap.Add("then", TokenType.Then);
			this.keywordMap.Add("until", TokenType.Until);
			this.keywordMap.Add("while", TokenType.While);
			this.keywordMap.Add("print", TokenType.Print);
		}

		// Token: 0x06000E90 RID: 3728 RVA: 0x000455A8 File Offset: 0x000437A8
		public List<Token> ScanSourceForTokens()
		{
			while (!this.IsAtEnd())
			{
				this.start = this.current;
				this.ScanToken();
			}
			this.tokens.Add(new Token(TokenType.EndOfFile, "", null, this.current, this.current, this.line));
			return this.tokens;
		}

		// Token: 0x06000E91 RID: 3729 RVA: 0x00045601 File Offset: 0x00043801
		private bool IsAtEnd()
		{
			return this.current >= this.source.Length;
		}

		// Token: 0x06000E92 RID: 3730 RVA: 0x0004561C File Offset: 0x0004381C
		private void ScanToken()
		{
			char c = this.Advance();
			if (c <= '>')
			{
				switch (c)
				{
				case '\t':
				case '\r':
					return;
				case '\n':
					this.AddToken(TokenType.EndOfLine);
					this.line++;
					return;
				case '\v':
				case '\f':
					break;
				default:
					switch (c)
					{
					case ' ':
						return;
					case '!':
						this.AddToken(TokenType.Bang);
						return;
					case '"':
						this.CollectString();
						return;
					case '#':
						this.AddToken(TokenType.Hash);
						return;
					case '%':
						this.AddToken(TokenType.Modulo);
						return;
					case '(':
						this.AddToken(TokenType.LeftParenthesis);
						return;
					case ')':
						this.AddToken(TokenType.RightParenthesis);
						return;
					case '*':
						this.AddToken(TokenType.Star);
						return;
					case '+':
						this.AddToken(TokenType.Plus);
						return;
					case ',':
						this.AddToken(TokenType.Comma);
						return;
					case '-':
						if (this.Match('-'))
						{
							while (this.Peek() != '\n')
							{
								if (this.IsAtEnd())
								{
									return;
								}
								this.Advance();
							}
							return;
						}
						this.AddToken(TokenType.Minus);
						return;
					case '.':
						if (this.Match('.'))
						{
							this.AddToken(TokenType.Concatenate);
							return;
						}
						if (this.IsDigit(this.PeekNext()))
						{
							this.CollectNumber();
							return;
						}
						this.AddToken(TokenType.Dot);
						return;
					case '/':
						this.AddToken(TokenType.Slash);
						return;
					case ':':
						this.AddToken(TokenType.Colon);
						return;
					case ';':
						this.AddToken(TokenType.Semicolon);
						return;
					case '<':
						this.AddToken(this.Match('=') ? TokenType.LessEqual : TokenType.Less);
						return;
					case '=':
						this.AddToken(this.Match('=') ? TokenType.EqualEqual : TokenType.Equal);
						return;
					case '>':
						this.AddToken(this.Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
						return;
					}
					break;
				}
			}
			else
			{
				switch (c)
				{
				case '[':
					this.AddToken(TokenType.LeftBracket);
					return;
				case '\\':
					break;
				case ']':
					this.AddToken(TokenType.RightBracket);
					return;
				case '^':
					this.AddToken(TokenType.Carrot);
					return;
				default:
					switch (c)
					{
					case '{':
						this.AddToken(TokenType.LeftBrace);
						return;
					case '}':
						this.AddToken(TokenType.RightBrace);
						return;
					case '~':
						this.AddToken(this.Match('=') ? TokenType.BangEqual : TokenType.Tilde);
						return;
					}
					break;
				}
			}
			if (this.IsDigit(c))
			{
				this.CollectNumber();
				return;
			}
			if (this.IsAlpha(c))
			{
				this.CollectIdentifier();
				return;
			}
			this.errorCallback(this.line, "Unexpected Character.");
		}

		// Token: 0x06000E93 RID: 3731 RVA: 0x000458B0 File Offset: 0x00043AB0
		private char Advance()
		{
			string text = this.source;
			int num = this.current;
			this.current = num + 1;
			return text[num];
		}

		// Token: 0x06000E94 RID: 3732 RVA: 0x000458D9 File Offset: 0x00043AD9
		private bool Match(char expected)
		{
			if (this.IsAtEnd())
			{
				return false;
			}
			if (this.source[this.current] != expected)
			{
				return false;
			}
			this.current++;
			return true;
		}

		// Token: 0x06000E95 RID: 3733 RVA: 0x0004590A File Offset: 0x00043B0A
		private char Peek()
		{
			if (!this.IsAtEnd())
			{
				return this.source[this.current];
			}
			return '\0';
		}

		// Token: 0x06000E96 RID: 3734 RVA: 0x00045927 File Offset: 0x00043B27
		private char PeekNext()
		{
			if (this.current + 1 < this.source.Length)
			{
				return this.source[this.current + 1];
			}
			return '\0';
		}

		// Token: 0x06000E97 RID: 3735 RVA: 0x00045953 File Offset: 0x00043B53
		private bool IsAlphaNumeric(char c)
		{
			return this.IsDigit(c) || this.IsAlpha(c);
		}

		// Token: 0x06000E98 RID: 3736 RVA: 0x00045967 File Offset: 0x00043B67
		private bool IsDigit(char c)
		{
			return c >= '0' && c <= '9';
		}

		// Token: 0x06000E99 RID: 3737 RVA: 0x00045978 File Offset: 0x00043B78
		private bool IsAlpha(char c)
		{
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
		}

		// Token: 0x06000E9A RID: 3738 RVA: 0x00045998 File Offset: 0x00043B98
		private void CollectString()
		{
			while (this.Peek() != '"' && !this.IsAtEnd())
			{
				if (this.Peek() == '\n')
				{
					this.line++;
				}
				this.Advance();
			}
			if (this.IsAtEnd())
			{
				this.errorCallback(this.line, "Unterminated string detected.");
				return;
			}
			this.Advance();
			string text = this.source.Substring(this.start + 1, this.current - this.start - 1);
			this.AddToken(TokenType.String, text);
		}

		// Token: 0x06000E9B RID: 3739 RVA: 0x00045A2C File Offset: 0x00043C2C
		private void CollectNumber()
		{
			while (this.IsDigit(this.Peek()))
			{
				this.Advance();
			}
			if (this.Peek() == '.' && this.IsDigit(this.PeekNext()))
			{
				this.Advance();
				while (this.IsDigit(this.Peek()))
				{
					this.Advance();
				}
			}
			string text = this.source.Substring(this.start, this.current - this.start);
			double num;
			if (double.TryParse(text, out num))
			{
				this.AddToken(TokenType.Number, num);
				return;
			}
			int num2;
			if (int.TryParse(text, out num2))
			{
				this.AddToken(TokenType.Number, num2);
				return;
			}
			float num3;
			if (float.TryParse(text, out num3))
			{
				this.AddToken(TokenType.Number, num3);
				return;
			}
			this.errorCallback(this.line, "Unable to parse value (" + text + ") into number.");
		}

		// Token: 0x06000E9C RID: 3740 RVA: 0x00045B14 File Offset: 0x00043D14
		private void CollectIdentifier()
		{
			while (this.IsAlphaNumeric(this.Peek()))
			{
				this.Advance();
			}
			string text = this.source.Substring(this.start, this.current - this.start);
			TokenType tokenType;
			if (!this.keywordMap.TryGetValue(text, out tokenType))
			{
				tokenType = TokenType.Identifier;
			}
			this.AddToken(tokenType);
		}

		// Token: 0x06000E9D RID: 3741 RVA: 0x00045B71 File Offset: 0x00043D71
		private void AddToken(TokenType tokenType)
		{
			this.AddToken(tokenType, null);
		}

		// Token: 0x06000E9E RID: 3742 RVA: 0x00045B7C File Offset: 0x00043D7C
		private void AddToken(TokenType type, object literal)
		{
			string text = this.source.Substring(this.start, this.current - this.start);
			this.tokens.Add(new Token(type, text, literal, this.start, this.current - this.start, this.line));
		}

		// Token: 0x04000C31 RID: 3121
		private string source;

		// Token: 0x04000C32 RID: 3122
		private List<Token> tokens = new List<Token>();

		// Token: 0x04000C33 RID: 3123
		private int start;

		// Token: 0x04000C34 RID: 3124
		private int current;

		// Token: 0x04000C35 RID: 3125
		private int line = 1;

		// Token: 0x04000C36 RID: 3126
		private Dictionary<string, TokenType> keywordMap = new Dictionary<string, TokenType>();

		// Token: 0x04000C37 RID: 3127
		private Action<int, string> errorCallback;
	}
}
