using System.Collections.Generic;
using ArcCreate.Gameplay.Scenecontrol;
using NUnit.Framework;

namespace Tests.Unit
{
    public class TriggerTest
    {
        private const int Duration = 1000;
        private readonly ManualTrigger[] triggers = new ManualTrigger[1];
        private ManualTrigger trigger;
        private ManualChannel manualChannel;

        [SetUp]
        public void SetUp()
        {
            trigger = new ManualTrigger
            {
                TriggerDispatch = new TriggerValueDispatch
                {
                    Value = new ConstantChannel(1),
                    Easing = Easing.Linear,
                    Duration = new ConstantChannel(Duration),
                    EasingString = "s",
                },
            };

            triggers[0] = trigger;
            manualChannel = new ManualChannel();
        }

        [Test]
        public void AccumulatingTriggerChannel()
        {
            TriggerChannel channel = new AccumulatingTriggerChannel(triggers);

            trigger.Activate();
            channel.ValueAt(0);

            trigger.Activate();
            Assert.That(channel.ValueAt(Duration / 2), Is.EqualTo(0.5f));
            Assert.That(channel.ValueAt(Duration + (Duration / 2)), Is.EqualTo(1.5f));
            Assert.That(channel.ValueAt(Duration * 2), Is.EqualTo(1.5f));
        }

        [Test]
        public void LoopingTriggerChannel()
        {
            TriggerChannel channel = new LoopingTriggerChannel(triggers);

            trigger.Activate();
            channel.ValueAt(0);

            trigger.Activate();
            channel.ValueAt(Duration / 2);

            Assert.That(channel.ValueAt(Duration + (Duration / 2)), Is.EqualTo(1));
        }

        [Test]
        public void SettingTriggerChannel()
        {
            TriggerChannel channel = new SettingTriggerChannel(triggers);

            trigger.Activate();
            channel.ValueAt(0);

            trigger.Activate();
            Assert.That(channel.ValueAt(Duration / 2), Is.EqualTo(1));
            Assert.That(channel.ValueAt(Duration), Is.EqualTo(1));
        }

        [Test]
        public void StackingTriggerChannel()
        {
            TriggerChannel channel = new StackingTriggerChannel(triggers);

            trigger.Activate();
            channel.ValueAt(0);

            trigger.Activate();
            channel.ValueAt(Duration / 2);

            Assert.That(channel.ValueAt(Duration * 2), Is.EqualTo(2));
        }

        [Test]
        public void ObserveTrigger_GoAbove()
        {
            SubstituteObserveTrigger trigger = new SubstituteObserveTrigger(manualChannel)
                .GoAbove(new ConstantChannel(5)) as SubstituteObserveTrigger;

            trigger.Poll(0);
            Assert.That(trigger.Dispatched, Is.False);

            manualChannel.Value = 5;
            trigger.Poll(0);
            Assert.That(trigger.Dispatched, Is.True);

            manualChannel.Value = 1;
            trigger.Poll(0);
            Assert.That(trigger.Dispatched, Is.False);
        }

        [Test]
        public void ObserveTrigger_GoBelow()
        {
            SubstituteObserveTrigger trigger = new SubstituteObserveTrigger(manualChannel)
                .GoBelow(new ConstantChannel(5)) as SubstituteObserveTrigger;

            trigger.Poll(0);
            Assert.That(trigger.Dispatched, Is.False);

            manualChannel.Value = 10;
            trigger.Poll(0);
            Assert.That(trigger.Dispatched, Is.False);

            manualChannel.Value = 5;
            trigger.Poll(0);
            Assert.That(trigger.Dispatched, Is.True);
        }

        private class ManualTrigger : Trigger
        {
            private bool activated = false;

            public void Activate()
            {
                activated = true;
            }

            public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
            {
            }

            public override void Poll(int timing)
            {
                if (activated)
                {
                    Dispatch(timing);
                }

                activated = false;
            }

            public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
            {
                return new List<object>();
            }
        }

        private class ManualChannel : ValueChannel
        {
            public float Value { get; set; }

            public override void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
            {
            }

            public override List<object> SerializeProperties(ScenecontrolSerialization serialization)
            {
                return new List<object>();
            }

            public override float ValueAt(int timing) => Value;
        }

        private class SubstituteObserveTrigger : ObserveTrigger
        {
            private bool dispatched = false;

            public SubstituteObserveTrigger(ValueChannel target)
                : base(target)
            {
            }

            public bool Dispatched
            {
                get
                {
                    bool res = dispatched;
                    dispatched = false;
                    return res;
                }
                set => dispatched = value;
            }

            protected override void Dispatch(int timing)
            {
                Dispatched = true;
            }
        }
    }
}