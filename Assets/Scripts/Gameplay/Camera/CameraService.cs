using System.Collections.Generic;
using ArcCreate.Gameplay.Data;
using ArcCreate.Utility.Extension;
using UnityEngine;

namespace ArcCreate.Gameplay.GameplayCamera
{
    public class CameraService : MonoBehaviour, ICameraService, ICameraControl
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Transform skyInputLabel;
        private float currentTilt;
        private float currentArcPos;
        private bool isReset;
        private List<CameraEvent> events = new List<CameraEvent>();

        public Camera GameplayCamera => gameplayCamera;

        public List<CameraEvent> Events => events;

        public bool IsEditorCamera { get; set; }

        public Vector3 EditorCameraPosition { get; set; }

        public Vector3 EditorCameraRotation { get; set; }

        private bool Is16By9
            => 1.77777779f - (1f * gameplayCamera.pixelWidth / gameplayCamera.pixelHeight) < 0.1f;

        private Vector3 ResetPosition
            => new Vector3(0f, Values.CameraY, Is16By9 ? Values.CameraZ : Values.CameraZTablet);

        private Vector3 ResetRotation
            => new Vector3(Is16By9 ? Values.CameraRotX : Values.CameraRotXTablet, 180f, 0f);

        public void Load(List<CameraEvent> cameras)
        {
            events = cameras;
            RebuildList();
        }

        public void Clear()
        {
            events.Clear();
        }

        public void Add(IEnumerable<CameraEvent> events)
        {
            this.events.AddRange(events);
            RebuildList();
        }

        public void Change(IEnumerable<CameraEvent> events)
        {
            RebuildList();
        }

        public void Remove(IEnumerable<CameraEvent> events)
        {
            foreach (var cam in events)
            {
                this.events.Remove(cam);
            }

            RebuildList();
        }

        public IEnumerable<CameraEvent> FindByTiming(int timing)
        {
            int i = events.BisectLeft(timing, n => n.Timing);
            while (i >= 0 && i < events.Count && events[i].Timing == timing)
            {
                yield return events[i];
                i++;
            }
        }

        public IEnumerable<CameraEvent> FindWithinRange(int from, int to)
        {
            for (int i = 0; i < events.Count; i++)
            {
                CameraEvent cam = events[i];
                if (cam.Timing >= from && cam.Timing + cam.Duration <= to)
                {
                    yield return cam;
                }
            }
        }

        public void AddTiltToCamera(float arcWorldX)
        {
            currentArcPos += arcWorldX;
        }

        public void UpdateCamera(int currentTiming)
        {
            if (IsEditorCamera)
            {
                GameplayCamera.transform.localPosition = EditorCameraPosition;
                GameplayCamera.transform.localRotation = Quaternion.Euler(EditorCameraRotation);
                return;
            }

            skyInputLabel.localPosition = new Vector3(
                Is16By9 ? Values.SkyInputLabelX : Values.SkyInputLabelXTablet,
                skyInputLabel.localPosition.y,
                skyInputLabel.localPosition.z);

            Vector3 prevPosition = gameplayCamera.transform.localPosition;
            Vector3 position = ResetPosition;
            Vector3 rotation = ResetRotation;
            gameplayCamera.fieldOfView = Is16By9 ? 50 : 65;

            for (int i = 0; i < events.Count; i++)
            {
                CameraEvent cam = events[i];
                if (cam.Timing > currentTiming)
                {
                    break;
                }

                isReset = cam.IsReset;
                if (isReset)
                {
                    isReset = true;
                    position = ResetPosition;
                    rotation = ResetRotation;
                }

                float percent = cam.PercentAt(currentTiming);
                position += new Vector3(-cam.Move.x, cam.Move.y, cam.Move.z) * percent / 100f;
                rotation += new Vector3(-cam.Rotate.y, -cam.Rotate.x, cam.Rotate.z) * percent;
            }

            if (position.IsNaN() || rotation.IsNaN())
            {
                gameplayCamera.transform.localPosition = prevPosition;
            }
            else
            {
                gameplayCamera.transform.localPosition = position;
                gameplayCamera.transform.localRotation = Quaternion.Euler(0f, 0f, rotation.z) * Quaternion.Euler(rotation.x, rotation.y, 0f);
            }

            UpdateCameraTilt();
        }

        private void UpdateCameraTilt()
        {
            if (!isReset)
            {
                currentTilt = 0;
                return;
            }

            float pos = -Mathf.Clamp(currentArcPos / Values.LaneWidth, -1, 1) * Values.CameraArcPosScalar;
            float delta = pos - currentTilt;
            if (Mathf.Abs(delta) >= 0.001f)
            {
                float speed = Values.CameraTiltSpeed;
                currentTilt += speed * delta * Time.deltaTime;
            }
            else
            {
                currentTilt = pos;
            }

            gameplayCamera.transform.LookAt(
                gameplayCamera.transform.localPosition - ResetPosition + new Vector3(0, -5.5f, -20),
                new Vector3(currentTilt, 1 - currentTilt, 0));
            currentArcPos = 0;
        }

        private void RebuildList()
        {
            events.Sort((a, b) => a.CompareTo(b));
        }

        private void Awake()
        {
            currentArcPos = 0;
            isReset = true;
        }
    }
}