using System;
using System.Collections.Generic;

namespace Endless.Creator.Test.LuaParsing;

public class Scanner
{
	private string source;

	private List<Token> tokens = new List<Token>();

	private int start;

	private int current;

	private int line = 1;

	private Dictionary<string, TokenType> keywordMap = new Dictionary<string, TokenType>();

	private Action<int, string> errorCallback;

	public Scanner(string source, Action<int, string> errorCallback)
	{
		this.source = source;
		this.errorCallback = errorCallback;
		keywordMap.Add("and", TokenType.And);
		keywordMap.Add("break", TokenType.Break);
		keywordMap.Add("do", TokenType.Do);
		keywordMap.Add("if", TokenType.If);
		keywordMap.Add("else", TokenType.Else);
		keywordMap.Add("elseif", TokenType.Elseif);
		keywordMap.Add("end", TokenType.End);
		keywordMap.Add("true", TokenType.True);
		keywordMap.Add("false", TokenType.False);
		keywordMap.Add("for", TokenType.For);
		keywordMap.Add("function", TokenType.Function);
		keywordMap.Add("in", TokenType.In);
		keywordMap.Add("local", TokenType.Local);
		keywordMap.Add("nil", TokenType.Nil);
		keywordMap.Add("not", TokenType.Not);
		keywordMap.Add("or", TokenType.Or);
		keywordMap.Add("repeat", TokenType.Repeat);
		keywordMap.Add("return", TokenType.Return);
		keywordMap.Add("then", TokenType.Then);
		keywordMap.Add("until", TokenType.Until);
		keywordMap.Add("while", TokenType.While);
		keywordMap.Add("print", TokenType.Print);
	}

	public List<Token> ScanSourceForTokens()
	{
		while (!IsAtEnd())
		{
			start = current;
			ScanToken();
		}
		tokens.Add(new Token(TokenType.EndOfFile, "", null, current, current, line));
		return tokens;
	}

	private bool IsAtEnd()
	{
		return current >= source.Length;
	}

	private void ScanToken()
	{
		char c = Advance();
		switch (c)
		{
		case '(':
			AddToken(TokenType.LeftParenthesis);
			break;
		case ')':
			AddToken(TokenType.RightParenthesis);
			break;
		case '{':
			AddToken(TokenType.LeftBrace);
			break;
		case '}':
			AddToken(TokenType.RightBrace);
			break;
		case '[':
			AddToken(TokenType.LeftBracket);
			break;
		case ']':
			AddToken(TokenType.RightBracket);
			break;
		case ',':
			AddToken(TokenType.Comma);
			break;
		case '.':
			if (Match('.'))
			{
				AddToken(TokenType.Concatenate);
			}
			else if (IsDigit(PeekNext()))
			{
				CollectNumber();
			}
			else
			{
				AddToken(TokenType.Dot);
			}
			break;
		case '-':
			if (Match('-'))
			{
				while (Peek() != '\n' && !IsAtEnd())
				{
					Advance();
				}
			}
			else
			{
				AddToken(TokenType.Minus);
			}
			break;
		case '+':
			AddToken(TokenType.Plus);
			break;
		case ';':
			AddToken(TokenType.Semicolon);
			break;
		case '*':
			AddToken(TokenType.Star);
			break;
		case '/':
			AddToken(TokenType.Slash);
			break;
		case '%':
			AddToken(TokenType.Modulo);
			break;
		case ':':
			AddToken(TokenType.Colon);
			break;
		case '#':
			AddToken(TokenType.Hash);
			break;
		case '^':
			AddToken(TokenType.Carrot);
			break;
		case '~':
			AddToken(Match('=') ? TokenType.BangEqual : TokenType.Tilde);
			break;
		case '!':
			AddToken(TokenType.Bang);
			break;
		case '=':
			AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
			break;
		case '<':
			AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
			break;
		case '>':
			AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
			break;
		case '\n':
			AddToken(TokenType.EndOfLine);
			line++;
			break;
		case '"':
			CollectString();
			break;
		default:
			if (IsDigit(c))
			{
				CollectNumber();
			}
			else if (IsAlpha(c))
			{
				CollectIdentifier();
			}
			else
			{
				errorCallback(line, "Unexpected Character.");
			}
			break;
		case '\t':
		case '\r':
		case ' ':
			break;
		}
	}

	private char Advance()
	{
		return source[current++];
	}

	private bool Match(char expected)
	{
		if (IsAtEnd())
		{
			return false;
		}
		if (source[current] != expected)
		{
			return false;
		}
		current++;
		return true;
	}

	private char Peek()
	{
		if (!IsAtEnd())
		{
			return source[current];
		}
		return '\0';
	}

	private char PeekNext()
	{
		if (current + 1 < source.Length)
		{
			return source[current + 1];
		}
		return '\0';
	}

	private bool IsAlphaNumeric(char c)
	{
		if (!IsDigit(c))
		{
			return IsAlpha(c);
		}
		return true;
	}

	private bool IsDigit(char c)
	{
		if (c >= '0')
		{
			return c <= '9';
		}
		return false;
	}

	private bool IsAlpha(char c)
	{
		if ((c < 'a' || c > 'z') && (c < 'A' || c > 'Z'))
		{
			return c == '_';
		}
		return true;
	}

	private void CollectString()
	{
		while (Peek() != '"' && !IsAtEnd())
		{
			if (Peek() == '\n')
			{
				line++;
			}
			Advance();
		}
		if (IsAtEnd())
		{
			errorCallback(line, "Unterminated string detected.");
			return;
		}
		Advance();
		string literal = source.Substring(start + 1, current - start - 1);
		AddToken(TokenType.String, literal);
	}

	private void CollectNumber()
	{
		while (IsDigit(Peek()))
		{
			Advance();
		}
		if (Peek() == '.' && IsDigit(PeekNext()))
		{
			Advance();
			while (IsDigit(Peek()))
			{
				Advance();
			}
		}
		string text = source.Substring(start, current - start);
		int result2;
		float result3;
		if (double.TryParse(text, out var result))
		{
			AddToken(TokenType.Number, result);
		}
		else if (int.TryParse(text, out result2))
		{
			AddToken(TokenType.Number, result2);
		}
		else if (float.TryParse(text, out result3))
		{
			AddToken(TokenType.Number, result3);
		}
		else
		{
			errorCallback(line, "Unable to parse value (" + text + ") into number.");
		}
	}

	private void CollectIdentifier()
	{
		while (IsAlphaNumeric(Peek()))
		{
			Advance();
		}
		string key = source.Substring(start, current - start);
		if (!keywordMap.TryGetValue(key, out var value))
		{
			value = TokenType.Identifier;
		}
		AddToken(value);
	}

	private void AddToken(TokenType tokenType)
	{
		AddToken(tokenType, null);
	}

	private void AddToken(TokenType type, object literal)
	{
		string lexeme = source.Substring(start, current - start);
		tokens.Add(new Token(type, lexeme, literal, start, current - start, line));
	}
}
