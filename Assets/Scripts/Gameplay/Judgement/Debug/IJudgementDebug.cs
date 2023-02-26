using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement.Input;

namespace ArcCreate.Gameplay.Judgement
{
    public interface IJudgementDebug
    {
        void SetTouchState(List<TouchInput> currentInputs);

        void ShowAssignedFinger(int color, int assignedFingerId);

        void ShowExistsArc(int color, bool exists);

        void ShowFingerHit(int color, int fingerId);

        void ShowFingerMiss(int color, int fingerId);

        void ShowGrace(float v);

        void ShowInputLock(int color, float v);
    }
}