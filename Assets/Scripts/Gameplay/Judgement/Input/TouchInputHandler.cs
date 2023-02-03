using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility;
using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ArcCreate.Gameplay.Judgement.Input
{
    public class TouchInputHandler : IInputHandler
    {
        protected List<TouchInput> CurrentInputs { get; } = new List<TouchInput>(10);

        public virtual void PollInput()
        {
            var touches = Touch.activeTouches;
            CurrentInputs.Clear();
            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];

                TouchInput input = new TouchInput(touch, GetCameraRay(touch.screenPosition));
                CurrentInputs.Add(input);

                Services.InputFeedback.LaneFeedback(input.Lane);
                Services.InputFeedback.FloatlineFeedback(input.VerticalPos.y);
            }
        }

        public void HandleTapRequests(
            int currentTiming,
            UnorderedList<LaneTapJudgementRequest> laneTapRequests,
            UnorderedList<ArcTapJudgementRequest> arcTapRequests)
        {
            for (int inpIndex = 0; inpIndex < CurrentInputs.Count; inpIndex++)
            {
                TouchInput input = CurrentInputs[inpIndex];
                if (!input.IsTap)
                {
                    continue;
                }

                int minTimingDifference = int.MaxValue;
                float minPositionDifference = float.MaxValue;

                bool applicableLaneRequestExists = false;
                LaneTapJudgementRequest applicableLaneRequest = default;
                int applicableLaneRequestIndex = 0;

                for (int i = laneTapRequests.Count - 1; i >= 0; i--)
                {
                    LaneTapJudgementRequest req = laneTapRequests[i];
                    int timingDifference = req.AutoAtTiming - currentTiming;
                    if (timingDifference > minTimingDifference)
                    {
                        continue;
                    }

                    Vector3 worldPosition = new Vector3(ArcFormula.LaneToWorldX(req.Lane), 0, 0);
                    Vector3 screenPosition = Services.Camera.GameplayCamera.WorldToScreenPoint(worldPosition);
                    Vector3 deltaToNote = screenPosition - input.ScreenPos;
                    float distanceToNote = deltaToNote.sqrMagnitude;

                    if (distanceToNote <= minPositionDifference
                     && LaneCollide(input, screenPosition, req.Lane))
                    {
                        minTimingDifference = timingDifference;
                        minPositionDifference = distanceToNote;
                        applicableLaneRequestExists = true;
                        applicableLaneRequest = req;
                        applicableLaneRequestIndex = i;
                    }
                }

                bool applicableArcTapRequestExists = false;
                ArcTapJudgementRequest applicableArcTapRequest = default;
                int applicableArcTapRequestIndex = 0;

                for (int i = arcTapRequests.Count - 1; i >= 0; i--)
                {
                    ArcTapJudgementRequest req = arcTapRequests[i];
                    int timingDifference = req.AutoAtTiming - currentTiming;
                    if (timingDifference > minTimingDifference)
                    {
                        continue;
                    }

                    Vector3 worldPosition = new Vector3(req.X, req.Y, 0);
                    Vector3 screenPosition = Services.Camera.GameplayCamera.WorldToScreenPoint(worldPosition);
                    Vector3 deltaToNote = screenPosition - input.ScreenPos;
                    float distanceToNote = deltaToNote.sqrMagnitude;

                    if (distanceToNote <= minPositionDifference
                     && ArcTapCollide(input.ScreenPos, screenPosition))
                    {
                        minTimingDifference = timingDifference;
                        minPositionDifference = distanceToNote;
                        applicableArcTapRequestExists = true;
                        applicableArcTapRequest = req;
                        applicableArcTapRequestIndex = i;
                    }
                }

                if (applicableArcTapRequestExists)
                {
                    applicableArcTapRequest.Receiver.ProcessArcTapJudgement(currentTiming - applicableArcTapRequest.AutoAtTiming);
                    arcTapRequests.RemoveAt(applicableArcTapRequestIndex);
                }
                else if (applicableLaneRequestExists)
                {
                    applicableLaneRequest.Receiver.ProcessLaneTapJudgement(currentTiming - applicableLaneRequest.AutoAtTiming);
                    laneTapRequests.RemoveAt(applicableLaneRequestIndex);
                }
            }
        }

        public void HandleLaneHoldRequests(int currentTiming, UnorderedList<LaneHoldJudgementRequest> requests)
        {
            for (int inpIndex = 0; inpIndex < CurrentInputs.Count; inpIndex++)
            {
                TouchInput input = CurrentInputs[inpIndex];

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
            ArcColorLogic.NewFrame(currentTiming);

            // Notify if arcs exists
            for (int c = 0; c <= ArcColorLogic.MaxColor; c++)
            {
                ArcColorLogic color = ArcColorLogic.Get(c);

                bool arcOfColorExists = false;
                for (int i = requests.Count - 1; i >= 0; i--)
                {
                    ArcJudgementRequest req = requests[i];
                    if (req.Arc.Color == color.Color)
                    {
                        arcOfColorExists = true;
                        break;
                    }
                }

                color.ExistsArcWithinRange(arcOfColorExists);
            }

            // Detect grace period
            bool graceActive = false;
            for (int i = requests.Count - 1; i >= 0; i--)
            {
                ArcJudgementRequest req1 = requests[i];
                if (currentTiming > req1.Arc.EndTiming)
                {
                    continue;
                }

                for (int j = i - 1; j >= 0; j--)
                {
                    ArcJudgementRequest req2 = requests[j];
                    if (currentTiming > req2.Arc.EndTiming)
                    {
                        continue;
                    }

                    Vector3 worldPosition1 = new Vector3(req1.Arc.WorldXAt(currentTiming), req1.Arc.WorldYAt(currentTiming), 0);
                    Vector3 worldPosition2 = new Vector3(req2.Arc.WorldXAt(currentTiming), req2.Arc.WorldYAt(currentTiming), 0);
                    Vector3 screenPosition1 = Services.Camera.GameplayCamera.WorldToScreenPoint(worldPosition1);
                    Vector3 screenPosition2 = Services.Camera.GameplayCamera.WorldToScreenPoint(worldPosition2);

                    if (ArcHitboxCollide(screenPosition1, screenPosition2))
                    {
                        graceActive = true;
                        break;
                    }
                }

                if (graceActive)
                {
                    ArcColorLogic.StartGracePeriodForAllColors();
                    break;
                }
            }

            // Process finger state change
            for (int inpIndex = 0; inpIndex < CurrentInputs.Count; inpIndex++)
            {
                TouchInput input = CurrentInputs[inpIndex];

                for (int i = requests.Count - 1; i >= 0; i--)
                {
                    ArcJudgementRequest req = requests[i];
                    ArcColorLogic colorLogic = ArcColorLogic.Get(req.Arc.Color);

                    if (input.Phase == UnityEngine.InputSystem.TouchPhase.Ended)
                    {
                        colorLogic.FingerLifted(input.Id, req.Arc.TimeIncrement);
                        continue;
                    }

                    bool collide = ArcCollide(input, req.Arc, currentTiming);
                    if (collide)
                    {
                        Vector3 worldPosition = new Vector3(req.Arc.WorldXAt(currentTiming), req.Arc.WorldYAt(currentTiming), 0);
                        Vector3 screenPosition = Services.Camera.GameplayCamera.WorldToScreenPoint(worldPosition);
                        float distance = (screenPosition - input.ScreenPos).sqrMagnitude;
                        colorLogic.FingerHit(input.Id, distance);
                    }
                    else
                    {
                        colorLogic.FingerMiss(input.Id);
                    }
                }
            }

            // Process requests
            for (int inpIndex = 0; inpIndex < CurrentInputs.Count; inpIndex++)
            {
                TouchInput input = CurrentInputs[inpIndex];

                for (int i = requests.Count - 1; i >= 0; i--)
                {
                    ArcJudgementRequest req = requests[i];
                    ArcColorLogic colorLogic = ArcColorLogic.Get(req.Arc.Color);

                    bool collide = ArcCollide(input, req.Arc, currentTiming);
                    bool acceptInput = colorLogic.ShouldAcceptInput(input.Id);

                    if (collide && acceptInput)
                    {
                        req.Receiver.ProcessArcJudgement(currentTiming - req.AutoAtTiming);
                        requests.RemoveAt(i);
                    }
                }
            }

            ArcColorLogic.ApplyRedValue();
        }

        public void ResetJudge()
        {
            ArcColorLogic.ResetAll();
        }

        protected Ray GetCameraRay(Vector2 screenPosition)
        {
            return Services.Camera.GameplayCamera.ScreenPointToRay(screenPosition);
        }

        private bool ArcCollide(TouchInput touch, Arc arc, int currentTiming)
        {
            Vector3 arcWorldPosition = new Vector3(arc.WorldXAt(currentTiming), arc.WorldYAt(currentTiming));
            float skyInputY = Services.Judgement.SkyInputY;
            if (arcWorldPosition.y <= skyInputY)
            {
                touch.VerticalPos.y = Mathf.Min(touch.VerticalPos.y, skyInputY);
            }

            Vector3 arcScreenPos = Services.Camera.GameplayCamera.WorldToScreenPoint(arcWorldPosition);
            Vector3 touchScreenPos = Services.Camera.GameplayCamera.WorldToScreenPoint(touch.VerticalPos);
            return ArcHitboxCollide(arcScreenPos, touchScreenPos);
        }

        private bool ArcHitboxCollide(Vector3 screenPosition1, Vector3 screenPosition2)
        {
            float dx = Mathf.Abs(screenPosition1.x - screenPosition2.x);
            float dy = Mathf.Abs(screenPosition1.y - screenPosition2.y);

            return dx <= Values.LaneScreenHitbox * Values.ArcHitboxX / Values.LaneWidth
                && dy <= Values.LaneScreenHitbox * Values.ArcHitboxY / Values.LaneWidth;
        }

        private bool ArcTapCollide(Vector3 screenPosition1, Vector3 screenPosition2)
        {
            float dx = Mathf.Abs(screenPosition1.x - screenPosition2.x);
            float dy = Mathf.Abs(screenPosition1.x - screenPosition2.y);

            return dx <= Values.LaneScreenHitbox * Values.ArcTapHitboxX / Values.LaneWidth
                && dy <= Values.LaneScreenHitbox * Values.ArcTapHitboxY / Values.LaneWidth;
        }

        private bool LaneCollide(TouchInput input, Vector3 screenPosition, int lane)
        {
            if (input.Lane == lane)
            {
                return true;
            }

            if (Mathf.Abs(input.ScreenPos.x - screenPosition.x) <= Values.LaneScreenHitbox
             && Mathf.Abs(input.ScreenPos.y - screenPosition.y) <= Values.LaneScreenHitbox)
            {
                return true;
            }

            return false;
        }
    }
}