namespace Endless.Creator.Test.LuaParsing;

public class Token
{
	public int StartIndex { get; }

	public int Length { get; }

	public TokenType Type { get; }

	public string Lexeme { get; }

	public object Literal { get; }

	public int Line { get; }

	public Token(TokenType type, string lexeme, object literal, int startIndex, int endIndex, int line)
	{
		Type = type;
		Lexeme = lexeme;
		Literal = literal;
		StartIndex = startIndex;
		Length = endIndex;
		Line = line;
	}

	public override string ToString()
	{
		return "Lexeme: <b>" + Lexeme + "</b>, " + string.Format("{0}: {1}, ", "Type", Type) + string.Format("{0}: {1}, ", "StartIndex", StartIndex) + $"EndIndex: {StartIndex + Length}, " + string.Format("{0}: {1}, ", "Length", Length) + string.Format("{0}: {1}, ", "Line", Line) + string.Format("{0}: {1}", "Literal", Literal);
	}
}
