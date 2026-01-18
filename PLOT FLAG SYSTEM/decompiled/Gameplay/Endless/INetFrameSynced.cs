using UnityEngine;

namespace Endless;

internal interface INetFrameSynced
{
	void RollbackListener(uint frame);

	void SimulateFrameListener(uint frame);

	Vector3 GetPositionAtFrame(uint frame);
}
