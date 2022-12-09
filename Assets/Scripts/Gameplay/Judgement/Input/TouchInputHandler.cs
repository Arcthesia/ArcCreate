using System.Collections.Generic;
using ArcCreate.Utility;
using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ArcCreate.Gameplay.Judgement.Input
{
    public class TouchInputHandler : IInputHandler
    {
        private readonly List<TouchInput> currentInputs = new List<TouchInput>(10);

        public void PollInput()
        {
            var touches = Touch.activeTouches;
            currentInputs.Clear();
            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];

                TouchInput input = new TouchInput(touch, GetCameraRay(touch));
                currentInputs.Add(input);

                Services.InputFeedback.LaneFeedback(input.Lane);
                Services.InputFeedback.FloatlineFeedback(input.VerticalY);
            }
        }

        public void HandleLaneTapRequests(int currentTiming, UnorderedList<LaneTapJudgementRequest> requests)
        {
            for (int inpIndex = 0; inpIndex < currentInputs.Count; inpIndex++)
            {
                TouchInput input = currentInputs[inpIndex];
                if (!input.IsTap)
                {
                    continue;
                }

                int minTimingDifference = int.MaxValue;
                float minPositionDifference = float.MaxValue;
                bool applicableRequestExists = false;
                LaneTapJudgementRequest applicableRequest = default;
                int applicableRequestIndex = 0;

                for (int i = requests.Count - 1; i >= 0; i--)
                {
                    LaneTapJudgementRequest req = requests[i];
                    int timingDifference = req.AutoAtTiming - currentTiming;
                    if (timingDifference > minTimingDifference)
                    {
                        continue;
                    }

                    Vector3 worldPosition = new Vector3(ArcFormula.LaneToWorldX(req.Lane), 0, 0);
                    Vector3 screenPosition = Services.Camera.GameplayCamera.WorldToScreenPoint(worldPosition);
                    Vector3 deltaToNote = screenPosition - new Vector3(input.ScreenX, input.ScreenY);
                    float distanceToNote = deltaToNote.sqrMagnitude;

                    if (distanceToNote <= minPositionDifference
                     && LaneCollide(input, screenPosition, req.Lane))
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
                    applicableRequest.Receiver.ProcessLaneTapJudgement(currentTiming - applicableRequest.AutoAtTiming);
                    requests.RemoveAt(applicableRequestIndex);
                }
            }
        }

        public void HandleLaneHoldRequests(int currentTiming, UnorderedList<LaneHoldJudgementRequest> requests)
        {
            for (int inpIndex = 0; inpIndex < currentInputs.Count; inpIndex++)
            {
                TouchInput input = currentInputs[inpIndex];

                for (int i = requests.Count - 1; i >= 0; i--)
                {
                    LaneHoldJudgementRequest req = requests[i];

                    Vector3 worldPosition = new Vector3(ArcFormula.LaneToWorldX(req.Lane), 0, 0);
                    Vector3 screenPosition = Services.Camera.GameplayCamera.WorldToScreenPoint(worldPosition);

                    if (LaneCollide(input, screenPosition, req.Lane))
                    {
                        req.Receiver.ProcessLaneHoldJudgement(currentTiming - req.AutoAtTiming);
                        requests.RemoveAt(i);
                    }
                }
            }
        }

        public void HandleArcRequests(int currentTiming, UnorderedList<ArcJudgementRequest> requests)
        {
        }

        private bool LaneCollide(TouchInput input, Vector3 screenPosition, int lane)
        {
            if (input.Lane == lane)
            {
                return true;
            }

            if (Mathf.Abs(input.ScreenX - screenPosition.x) <= Values.LaneScreenHitbox
             && Mathf.Abs(input.ScreenY - screenPosition.y) <= Values.LaneScreenHitbox)
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