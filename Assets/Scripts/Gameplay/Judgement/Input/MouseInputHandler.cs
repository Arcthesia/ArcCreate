using UnityEngine;

namespace ArcCreate.Gameplay.Judgement.Input
{
    public class MouseInputHandler : TouchInputHandler
    {
        public override void PollInput()
        {
            Vector2 mousePosition = UnityEngine.Input.mousePosition;
            CurrentInputs.Clear();

            TouchInput input;
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                input = new TouchInput(0, mousePosition, true, TouchPhase.Began, GetCameraRay(mousePosition));
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                input = new TouchInput(0, mousePosition, false, TouchPhase.Ended, GetCameraRay(mousePosition));
            }
            else if (UnityEngine.Input.GetMouseButton(0))
            {
                input = new TouchInput(0, mousePosition, false, TouchPhase.Moved, GetCameraRay(mousePosition));
                Services.InputFeedback.LaneFeedback(Mathf.RoundToInt(input.Lane));
                Services.InputFeedback.FloatlineFeedback(input.VerticalPos.y);
            }
            else
            {
                return;
            }

            CurrentInputs.Add(input);
            Services.Judgement.Debug.SetTouchState(CurrentInputs);
        }
    }
}