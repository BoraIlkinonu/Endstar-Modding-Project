namespace Endless.Gameplay;

public interface IPerceptionJob
{
	bool IsComplete { get; }

	void RespondToRequests();
}
