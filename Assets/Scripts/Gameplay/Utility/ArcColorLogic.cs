using System.Collections.Generic;
using UnityEngine;

namespace ArcCreate.Gameplay
{
    /// <summary>
    /// Handler for a single arc color.
    /// </summary>
    public class ArcColorLogic
    {
        public const int UnassignedFingerId = int.MinValue;

        // In an attempt to contain all the logic into a single class,
        // expect the innerworkings of this to be very messy...
        private static readonly List<ArcColorLogic> Instances = new List<ArcColorLogic>();
        private static int frameTiming;
        private readonly int color;
        private int assignedFingerId = UnassignedFingerId;
        private float minDistanceThisFrame = float.MaxValue;
        private bool assignedFingerMissedThisFrame = false;
        private bool wrongFingerHitThisFrame = false;
        private bool existsArcWithinRangeThisFrame = false;
        private bool isAssigningThisFrame = false;
        private bool assignedFingerExistsThisFrame = false;
        private int lockUntil = int.MinValue;
        private int graceUntil = int.MinValue;
        private float currentRedArcValue = 0;
        private float redArcDuration = 1;

        private ArcColorLogic(int color)
        {
            this.color = color;
        }

        public static int MaxColor => Instances.Count - 1;

        /// <summary>
        /// Gets the color id of this instance.
        /// </summary>
        public int Color => color;

        /// <summary>
        /// Gets the currently assigned finger id to this color.
        /// </summary>
        public int AssignedFingerId
        {
            get => assignedFingerId;
            private set
            {
                assignedFingerId = value;
                IsFingerAssigned = value != UnassignedFingerId;
            }
        }

        /// <summary>
        /// Gets the red arc value between 0 and 1.
        /// </summary>
        public float RedArcValue => currentRedArcValue;

        private bool IsFingerAssigned { get; set; }

        private bool IsInputLocked
            => frameTiming <= lockUntil;

        private bool IsGraceActive
            => frameTiming <= graceUntil;

        /// <summary>
        /// Get the instance for a color.
        /// </summary>
        /// <param name="color">The arc color.</param>
        /// <returns>The instance for the color.</returns>
        public static ArcColorLogic Get(int color)
        {
            if (color < 0)
            {
                throw new System.Exception();
            }

            while (color >= Instances.Count)
            {
                Instances.Add(new ArcColorLogic(Instances.Count));
            }

            return Instances[color];
        }

        /// <summary>
        /// Remove all saved color instances.
        /// </summary>
        public static void ResetAll()
        {
            for (int i = 0; i < Instances.Count; i++)
            {
                Services.Skin.ApplyRedArcValue(i, 0);
            }

            Instances.Clear();
        }

        public static void ApplyRedValue()
        {
            for (int i = 0; i < Instances.Count; i++)
            {
                ArcColorLogic color = Instances[i];
                Services.Skin.ApplyRedArcValue(i, color.RedArcValue);
            }
        }

        /// <summary>
        /// Notify all colors that a new frame has started at the timing.
        /// It's important that this method is called BEFORE any other method each frame.
        /// </summary>
        /// <param name="timing">The timing of the frame.</param>
        public static void NewFrame(int timing)
        {
            int lastFrameTiming = frameTiming;
            frameTiming = timing;
            for (int i = 0; i < Instances.Count; i++)
            {
                ArcColorLogic color = Instances[i];
                color.ResetIntraframeState();
                color.UpdateRedArcValue(frameTiming - lastFrameTiming);
            }
        }

        /// <summary>
        /// Notify that two arcs of different colors collided and a grace period should start.
        /// </summary>
        public static void StartGracePeriodForAllColors()
        {
            for (int i = 0; i < Instances.Count; i++)
            {
                ArcColorLogic color = Instances[i];
                color.StartGracePeriod();
            }
        }

        /// <summary>
        /// Notify that two arcs of different colors collided and a grace period should start.
        /// </summary>
        public void StartGracePeriod()
        {
            graceUntil = frameTiming + Values.ArcGraceDuration;
            UnlockInput();
            ResetAssignedFinger();
        }

        /// <summary>
        /// Notify that a finger is no longer touching the screen.
        /// </summary>
        /// <param name="fingerId">The finger id.</param>
        /// <param name="arcJudgeInterval">The judgement interval of an arc of this color.</param>
        public void FingerLifted(int fingerId, float arcJudgeInterval)
        {
            if (fingerId == AssignedFingerId)
            {
                assignedFingerExistsThisFrame = true;
                ResetAssignedFinger();

                if (existsArcWithinRangeThisFrame)
                {
                    LockInput(arcJudgeInterval);
                }
            }
        }

        /// <summary>
        /// Notify that a finger has hit an arc of this color.
        /// </summary>
        /// <param name="fingerId">The finger id.</param>
        /// <param name="distance">The distance between the finger and an arc of this color.</param>
        /// <param name="arcJudgeInterval">The judgement interval of an arc of this color.</param>
        public void FingerHit(int fingerId, float distance, float arcJudgeInterval)
        {
            if (fingerId == assignedFingerId)
            {
                assignedFingerExistsThisFrame = true;
            }

            if (IsFingerAssigned)
            {
                if (!IsGraceActive && assignedFingerMissedThisFrame)
                {
                    FlashRedArc(arcJudgeInterval);
                }
                else
                {
                    if (fingerId == AssignedFingerId)
                    {
                        ResetRedArcValue();
                    }
                    else if (!IsGraceActive)
                    {
                        wrongFingerHitThisFrame = true;
                    }
                }
            }

            if (IsInputLocked)
            {
                FlashRedArc(arcJudgeInterval);
            }

            if (!IsFingerAssigned || isAssigningThisFrame)
            {
                if (!IsGraceActive && IsFingerAssignedToAnotherColor(fingerId))
                {
                    ConstantRedArc();
                }
                else if (!IsInputLocked && !IsGraceActive)
                {
                    if (distance < minDistanceThisFrame)
                    {
                        minDistanceThisFrame = distance;
                        AssignedFingerId = fingerId;
                        ResetRedArcValue();
                    }

                    isAssigningThisFrame = true;
                }
            }

            Services.Judgement.Debug.ShowFingerHit(Color, fingerId);
        }

        /// <summary>
        /// Notify that a finger has not hit an arc of this color.
        /// </summary>
        /// <param name="fingerId">The finger id.</param>
        /// <param name="arcJudgeInterval">The judgement interval of an arc of this color.</param>
        public void FingerMiss(int fingerId, float arcJudgeInterval)
        {
            if (fingerId == AssignedFingerId)
            {
                assignedFingerExistsThisFrame = true;
                assignedFingerMissedThisFrame = true;
                if (wrongFingerHitThisFrame)
                {
                    FlashRedArc(arcJudgeInterval);
                }
            }

            Services.Judgement.Debug.ShowFingerMiss(Color, fingerId);
        }

        /// <summary>
        /// Notify whether or not there are arcs within judgement range.
        /// It's important that this method is called BEFORE any finger state notifying methods.
        /// </summary>
        /// <param name="exists">Whether or not there are arc within judgement range.</param>
        public void ExistsArcWithinRange(bool exists)
        {
            if (!exists)
            {
                UnlockInput();
                ResetRedArcValue();
            }

            Services.Judgement.Debug.ShowExistsArc(Color, exists);
            existsArcWithinRangeThisFrame = exists;
        }

        /// <summary>
        /// Check whether or not to accept input from a finger id.
        /// </summary>
        /// <param name="fingerId">The finger id.</param>
        /// <returns>Whether or not to accept input from the finger id.</returns>
        public bool ShouldAcceptInput(int fingerId)
        {
            if (IsInputLocked)
            {
                return false;
            }

            if (IsGraceActive)
            {
                return true;
            }

            // Just to be safe
            if (fingerId == AssignedFingerId)
            {
                ResetRedArcValue();
                return true;
            }

            return false;
        }

        private void ResetIntraframeState()
        {
            minDistanceThisFrame = float.MaxValue;
            wrongFingerHitThisFrame = false;
            assignedFingerMissedThisFrame = false;
            existsArcWithinRangeThisFrame = false;
            isAssigningThisFrame = false;

            if (!assignedFingerExistsThisFrame)
            {
                ResetAssignedFinger();
            }

            assignedFingerExistsThisFrame = false;

            float lockVal = lockUntil == int.MinValue ? 0 : (float)(lockUntil - frameTiming) / Values.ArcLockDuration;
            float graceVal = graceUntil == int.MinValue ? 0 : (float)(graceUntil - frameTiming) / Values.ArcGraceDuration;
            Services.Judgement.Debug.ShowInputLock(Color, Mathf.Clamp(lockVal, 0, 1));
            Services.Judgement.Debug.ShowGrace(Mathf.Clamp(graceVal, 0, 1));
            Services.Judgement.Debug.ShowAssignedFinger(Color, AssignedFingerId);
        }

        private void ResetAssignedFinger()
        {
            AssignedFingerId = UnassignedFingerId;
        }

        private void LockInput(float arcJudgeInterval)
        {
            int lockDuration = ArcFormula.CalculateArcLockDuration(arcJudgeInterval);
            lockUntil = frameTiming + lockDuration;
        }

        private void UnlockInput()
        {
            lockUntil = int.MinValue;
        }

        private bool IsFingerAssignedToAnotherColor(int fingerId)
        {
            for (int i = 0; i < Instances.Count; i++)
            {
                ArcColorLogic color = Instances[i];
                if (color != this && color.AssignedFingerId == fingerId)
                {
                    return true;
                }
            }

            return false;
        }

        private void FlashRedArc(float arcJudgeInterval)
        {
            if (currentRedArcValue > 0)
            {
                return;
            }

            currentRedArcValue = 1;
            redArcDuration = ArcFormula.CalculateArcLockDuration(arcJudgeInterval);
        }

        private void ConstantRedArc()
        {
            currentRedArcValue = 1;
        }

        private void ResetRedArcValue()
        {
            currentRedArcValue = 0;
        }

        private void UpdateRedArcValue(float deltaTime)
        {
            float deltaValue = deltaTime / redArcDuration;
            currentRedArcValue -= deltaValue;
            if (currentRedArcValue < 0)
            {
                ResetRedArcValue();
            }
        }
    }
}