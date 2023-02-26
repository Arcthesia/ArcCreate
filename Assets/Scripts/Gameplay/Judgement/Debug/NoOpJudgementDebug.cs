using System.Collections.Generic;
using ArcCreate.Gameplay.Judgement.Input;

namespace ArcCreate.Gameplay.Judgement
{
    public class NoOpJudgementDebug : IJudgementDebug
    {
        public void SetTouchState(List<TouchInput> currentInputs)
        {
        }

        public void ShowAssignedFinger(int color, int assignedFingerId)
        {
        }

        public void ShowExistsArc(int color, bool exists)
        {
        }

        public void ShowFingerHit(int color, int fingerId)
        {
        }

        public void ShowFingerMiss(int color, int fingerId)
        {
        }

        public void ShowGrace(float v)
        {
        }

        public void ShowInputLock(int color, float v)
        {
        }
    }
}