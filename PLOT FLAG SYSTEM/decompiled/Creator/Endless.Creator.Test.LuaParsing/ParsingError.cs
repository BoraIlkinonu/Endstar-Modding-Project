namespace Endless.Creator.Test.LuaParsing;

public class ParsingError
{
	public string Message { get; }

	public int Line { get; }

	public int CharacterIndex { get; }

	public bool GlobalError { get; set; }

	public ParsingError(string message, int line, int characterIndex)
	{
		Message = message;
		Line = line;
		CharacterIndex = characterIndex;
	}

	public override string ToString()
	{
		if (!GlobalError)
		{
			return $"Line: {Line}, Character Index: {CharacterIndex}, Message: {Message}";
		}
		return "Global Error, Message: " + Message;
	}
}
