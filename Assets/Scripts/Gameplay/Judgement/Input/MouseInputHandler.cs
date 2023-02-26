using UnityEngine;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace ArcCreate.Gameplay.Judgement.Input
{
    public class MouseInputHandler : TouchInputHandler
    {
        public override void PollInput()
        {
            Mouse mouse = Mouse.current;
            Vector2 mousePosition = mouse.position.ReadValue();
            CurrentInputs.Clear();

            TouchInput input;
            bool isPressed = mouse.leftButton.isPressed;
            if (mouse.leftButton.wasPressedThisFrame)
            {
                input = new TouchInput(0, mousePosition, true, TouchPhase.Began, GetCameraRay(mousePosition));
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
            {
                input = new TouchInput(0, mousePosition, false, TouchPhase.Ended, GetCameraRay(mousePosition));
            }
            else if (mouse.leftButton.isPressed)
            {
                input = new TouchInput(0, mousePosition, false, TouchPhase.Moved, GetCameraRay(mousePosition));
            }
            else
            {
                return;
            }

            CurrentInputs.Add(input);
            if (mouse.leftButton.isPressed)
            {
                Services.InputFeedback.LaneFeedback(input.Lane);
                Services.InputFeedback.FloatlineFeedback(input.VerticalPos.y);
            }

            Services.Judgement.Debug.SetTouchState(CurrentInputs);
        }
    }
}