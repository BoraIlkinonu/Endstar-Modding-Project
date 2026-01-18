namespace Endless.Gameplay;

public interface IPersistantStateSubscriber
{
	bool ShouldSaveAndLoad { get; }

	object GetSaveState();

	void LoadState(object loadedState);
}
