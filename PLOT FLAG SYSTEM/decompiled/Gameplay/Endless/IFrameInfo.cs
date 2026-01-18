namespace Endless;

public interface IFrameInfo
{
	uint NetFrame { get; set; }

	void Clear();

	void Initialize();
}
