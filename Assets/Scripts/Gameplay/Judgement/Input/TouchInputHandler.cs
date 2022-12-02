using System.Collections.Generic;
using ArcCreate.Utility;
using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ArcCreate.Gameplay.Judgement.Input
{
    public class TouchInputHandler : IInputHandler
    {
        private readonly Queue<TouchInput> currentInputs = new Queue<TouchInput>();

        public void PollInput()
        {
            var touches = Touch.activeTouches;
            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];

                TouchInput input = new TouchInput(touch, GetCameraRay(touch));
                currentInputs.Enqueue(input);

                Services.InputFeedback.LaneFeedback(input.Lane);
                Services.InputFeedback.FloatlineFeedback(input.VerticalY);
            }
        }

        public void HandleTaps(int currentTiming, UnorderedList<TapJudgementRequest> tapRequests)
        {
            while (currentInputs.Count > 0)
            {
                TouchInput input = currentInputs.Dequeue();
                if (!input.IsTap)
                {
                    continue;
                }

                int minTimingDifference = int.MaxValue;
                float minPositionDifference = float.MaxValue;
                bool applicableRequestExists = false;
                TapJudgementRequest applicableRequest = default;
                int applicableRequestIndex = 0;

                for (int i = tapRequests.Count - 1; i >= 0; i--)
                {
                    TapJudgementRequest req = tapRequests[i];
                    int timingDifference = req.AutoAt - currentTiming;
                    if (timingDifference > minTimingDifference)
                    {
                        continue;
                    }

                    Vector3 worldPosition = new Vector3(ArcFormula.LaneToWorldX(req.Lane), 0, 0);
                    Vector3 screenPosition = Services.Camera.GameplayCamera.WorldToScreenPoint(worldPosition);
                    Vector3 deltaToNote = screenPosition - new Vector3(input.ScreenX, input.ScreenY);
                    float distanceToNote = deltaToNote.sqrMagnitude;

                    if (distanceToNote <= minPositionDifference
                     && TapCollide(input, screenPosition, req.Lane))
                    {
                        minTimingDifference = timingDifference;
                        minPositionDifference = distanceToNote;
                        applicableRequestExists = true;
                        applicableRequest = req;
                        applicableRequestIndex = i;
                    }
                }

                if (applicableRequestExists)
                {
                    JudgementResult result = currentTiming.CalculateJudgeResult(applicableRequest.AutoAt);
                    applicableRequest.Receiver.ProcessJudgement(result);
                    Services.InputFeedback.LaneFeedback(applicableRequest.Receiver.Lane);
                    tapRequests.RemoveAt(applicableRequestIndex);
                }
            }
        }

        private bool TapCollide(TouchInput input, Vector3 screenPosition, int lane)
        {
            if (input.Lane == lane)
            {
                return true;
            }

            if (Mathf.Abs(input.ScreenX - screenPosition.x) <= Values.TapScreenHitbox
             && Mathf.Abs(input.ScreenY - screenPosition.y) <= Values.TapScreenHitbox)
            {
                return true;
            }

            return false;
        }

        private Ray GetCameraRay(Touch touch)
        {
            return Services.Camera.GameplayCamera.ScreenPointToRay(touch.screenPosition);
        }
    }
}