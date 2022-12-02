using ArcCreate.Gameplay;
using NUnit.Framework;

namespace Tests.Unit
{
    public class ArcColorLogicTest
    {
        private ArcColorLogic colorBlue;
        private ArcColorLogic colorRed;

        [SetUp]
        public void SetUp()
        {
            ArcColorLogic.ResetAll();
            colorBlue = ArcColorLogic.Get(0);
            colorRed = ArcColorLogic.Get(1);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void ByDefaultDoesNotAcceptAnyFinger(int finger)
        {
            Assert.That(colorBlue.ShouldAcceptInput(finger), Is.False);
        }

        // Hitting an arc will assign the finger to that arc's color.
        // Multiple fingers hitting the same arc at the same time before any were assigned to its color
        //   means it will pick the finger closest to the arc's hitbox to assign to the color.
        [Test]
        public void HittingArcFirstTimeAssignsFingerToArcColor()
        {
            ArcColorLogic.NewFrame(0);
            colorBlue.FingerHit(0, 0);

            Assert.That(colorBlue.AssignedFingerId, Is.Zero);
        }

        [Test]
        public void MissingArcDoesNotChangeAssignedFinger()
        {
            ArcColorLogic.NewFrame(0);

            colorBlue.FingerHit(0, 0);
            colorBlue.FingerMiss(0);
            colorBlue.FingerMiss(1);

            Assert.That(colorBlue.AssignedFingerId, Is.Zero);
        }

        [Test]
        public void HittingArcFirstTimeWithMultipleFingerAssignsClosestFinger()
        {
            ArcColorLogic.NewFrame(0);

            colorBlue.FingerHit(0, 1f);
            colorBlue.FingerHit(1, 0.5f);
            colorBlue.FingerHit(2, 2f);
            colorBlue.FingerHit(3, 0);
            colorBlue.FingerMiss(4);

            Assert.That(colorBlue.AssignedFingerId, Is.EqualTo(3));
        }

        // When a finger is assigned to an arc color, any arc of that color won't accept input from any other finger.
        // If the finger is different to the arc color's assigned finger,
        // hitting an arc of that color will cause it to flash red at full value then ramp down repeatedly,
        // if and only if said arc weren't being hit by the assigned finger already.
        // If a finger is assigned to an arc color already, it also won't be accepted by other arc colors.
        // Trying to do so will cause it to flash red constantly.
        [Test]
        public void ArcColorAcceptsAssignedFingerOnly()
        {
            ArcColorLogic.NewFrame(0);

            colorBlue.FingerHit(0, 0);

            Assert.That(colorBlue.ShouldAcceptInput(0), Is.True);
            Assert.That(colorBlue.ShouldAcceptInput(1), Is.False);
        }

        [Test]
        public void ArcColorFlashesRedIfOnlyUnassignedFingerCollides()
        {
            ArcColorLogic.NewFrame(0);
            colorBlue.FingerHit(0, 0);

            ArcColorLogic.NewFrame(1000);
            colorBlue.FingerHit(1, 0);
            colorBlue.FingerMiss(0);

            Assert.That(colorBlue.RedArcValue, Is.EqualTo(1));
            ArcColorLogic.NewFrame(1000 + (Values.ArcRedFlashCycle / 2));
            Assert.That(colorBlue.RedArcValue, Is.LessThan(1));
            ArcColorLogic.NewFrame(1000 + Values.ArcRedFlashCycle + 1);
            Assert.That(colorBlue.RedArcValue, Is.GreaterThan(0));
        }

        [Test]
        public void ArcColorDoesNotFlashRedIfAssignedFingerStillCollides()
        {
            ArcColorLogic.NewFrame(0);
            colorBlue.FingerHit(0, 0);

            ArcColorLogic.NewFrame(1000);
            colorBlue.FingerHit(1, 0);
            colorBlue.FingerHit(0, 0);

            Assert.That(colorBlue.RedArcValue, Is.Zero);
        }

        [Test]
        public void UnassignedArcColorStaysRedIfFingerAlreadyAssignedToAnotherColorHits()
        {
            ArcColorLogic.NewFrame(0);
            colorRed.FingerHit(0, 0);

            colorBlue.FingerHit(0, 0);

            Assert.That(colorBlue.RedArcValue, Is.EqualTo(1));
            ArcColorLogic.NewFrame(1000 + (Values.ArcRedFlashCycle / 2));
            Assert.That(colorBlue.RedArcValue, Is.EqualTo(1));
            ArcColorLogic.NewFrame(1000 + Values.ArcRedFlashCycle);
            Assert.That(colorBlue.RedArcValue, Is.EqualTo(1));
        }

        [Test]
        public void UnassignedArcColorStopBeingRedAfterdAfterAFreeFingerHits()
        {
            ArcColorLogic.NewFrame(0);
            colorRed.FingerHit(0, 0);
            colorBlue.FingerHit(0, 10);
            Assert.That(colorBlue.RedArcValue, Is.EqualTo(1));

            ArcColorLogic.NewFrame(1000);
            colorBlue.FingerHit(1, 0);

            Assert.That(colorBlue.RedArcValue, Is.EqualTo(0));
            Assert.That(colorBlue.AssignedFingerId, Is.EqualTo(1));
        }

        // If the finger assigned to an arc's color has been lifted up,
        // the arc color will lock itself for ~500ms, during which time it will not accept any input whatsoever.
        // Any finger hitting an arc of this color will cause it to flash red then ramp down repeatedly.
        // If a chain of arcs end, then the lock state of the arc's color will be removed
        [Test]
        public void LiftingFingerLocksArcColorForSetDuration()
        {
            ArcColorLogic.NewFrame(0);
            colorBlue.FingerHit(0, 0);

            ArcColorLogic.NewFrame(1000);
            colorBlue.ExistsArcWithinRange(true);
            colorBlue.FingerLifted(0);

            Assert.That(colorBlue.ShouldAcceptInput(1), Is.False);
            Assert.That(colorBlue.ShouldAcceptInput(0), Is.False);
            ArcColorLogic.NewFrame(1000 + (Values.ArcLockDuration / 2));
            colorBlue.ExistsArcWithinRange(true);
            Assert.That(colorBlue.ShouldAcceptInput(0), Is.False);
        }

        [TestCase(0)]
        [TestCase(1)]
        public void LockedArcFlashedRedOnAnyFingerHit(int hittingFinger)
        {
            ArcColorLogic.NewFrame(0);
            colorBlue.FingerHit(0, 0);

            ArcColorLogic.NewFrame(1000);
            colorBlue.ExistsArcWithinRange(true);
            colorBlue.FingerLifted(0);
            colorBlue.FingerHit(hittingFinger, 0);

            Assert.That(colorBlue.RedArcValue, Is.EqualTo(1));
            ArcColorLogic.NewFrame(1000 + (Values.ArcLockDuration / 2));
            colorBlue.ExistsArcWithinRange(true);
            Assert.That(colorBlue.RedArcValue, Is.LessThan(1));
        }

        [Test]
        public void LockedArcReassignToClosestHittingFingerAfterLockDurationEnds()
        {
            ArcColorLogic.NewFrame(0);
            colorBlue.FingerHit(0, 0);
            ArcColorLogic.NewFrame(1000);
            colorBlue.ExistsArcWithinRange(true);
            colorBlue.FingerLifted(0);

            ArcColorLogic.NewFrame(1000 + Values.ArcLockDuration + 1);
            colorBlue.ExistsArcWithinRange(true);
            colorBlue.FingerHit(0, 50);
            colorBlue.FingerHit(1, 0);
            colorBlue.FingerMiss(2);

            Assert.That(colorBlue.ShouldAcceptInput(0), Is.False);
            Assert.That(colorBlue.ShouldAcceptInput(1), Is.True);
            Assert.That(colorBlue.ShouldAcceptInput(2), Is.False);
        }

        [Test]
        public void OnNoArcInRangeUnlockArcColor()
        {
            ArcColorLogic.NewFrame(0);
            colorBlue.FingerHit(0, 0);
            ArcColorLogic.NewFrame(1000);
            colorBlue.ExistsArcWithinRange(true);
            colorBlue.FingerLifted(0);

            ArcColorLogic.NewFrame(1000 + (Values.ArcLockDuration / 2));
            colorBlue.ExistsArcWithinRange(false);
            colorBlue.FingerHit(1, 0);

            Assert.That(colorBlue.ShouldAcceptInput(0), Is.False);
            Assert.That(colorBlue.ShouldAcceptInput(1), Is.True);
        }

        [Test]
        public void LiftingWhenNoArcInRangeDoesNotLockArc()
        {
            ArcColorLogic.NewFrame(0);
            colorBlue.ExistsArcWithinRange(true);
            colorBlue.FingerHit(0, 0);

            ArcColorLogic.NewFrame(1000);
            colorBlue.ExistsArcWithinRange(false);
            colorBlue.FingerLifted(0);

            ArcColorLogic.NewFrame(1000 + (Values.ArcLockDuration / 2));
            colorBlue.ExistsArcWithinRange(true);
            colorBlue.FingerHit(1, 0);

            Assert.That(colorBlue.ShouldAcceptInput(0), Is.False);
            Assert.That(colorBlue.ShouldAcceptInput(1), Is.True);
        }

        // If two arcs of any two different colors collide with each other, a grace period will enable globally for all arc colors.
        // During a grace period, arcs accept input from any finger.
        // After the grace period is over, arcs will reassign itself to the closest finger colliding with it.
        // However, arc that were locked before the period remains locked.
        [Test]
        public void DuringGracePeriodAcceptsInputFromAllFingers()
        {
            ArcColorLogic.NewFrame(0);
            colorBlue.StartGracePeriod();

            Assert.That(colorBlue.ShouldAcceptInput(0), Is.True);
            Assert.That(colorBlue.ShouldAcceptInput(1), Is.True);
            Assert.That(colorBlue.ShouldAcceptInput(2), Is.True);
        }

        [Test]
        public void LockedArcDoesNotAcceptInputDuringGracePeriod()
        {
            ArcColorLogic.NewFrame(0);
            colorBlue.FingerHit(0, 0);
            ArcColorLogic.NewFrame(1000);
            colorBlue.ExistsArcWithinRange(true);
            colorBlue.FingerLifted(0);

            colorBlue.StartGracePeriod();

            Assert.That(colorBlue.ShouldAcceptInput(0), Is.False);
        }

        [Test]
        public void AfterGracePeriodReassignToClosestHittingFinger()
        {
            ArcColorLogic.NewFrame(0);
            colorBlue.StartGracePeriod();

            ArcColorLogic.NewFrame(0 + Values.ArcGraceDuration + 1);
            colorBlue.FingerHit(0, 50);
            colorBlue.FingerHit(1, 0);
            colorBlue.FingerMiss(2);

            Assert.That(colorBlue.ShouldAcceptInput(0), Is.False);
            Assert.That(colorBlue.ShouldAcceptInput(1), Is.True);
            Assert.That(colorBlue.ShouldAcceptInput(2), Is.False);
        }
    }
}