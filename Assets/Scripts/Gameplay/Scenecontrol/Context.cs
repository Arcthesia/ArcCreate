using System.Collections.Generic;
using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyDoc("Class for accessing context value channels")]
    [EmmySingleton]
    public class Context : ISerializableUnit, ISceneController
    {
        private ValueChannel laneFrom = new ConstantChannel(1);
        private ValueChannel laneTo = new ConstantChannel(4);

        public static DropRateChannel DropRate => new DropRateChannel();

        public static GlobalOffsetChannel GlobalOffset => new GlobalOffsetChannel();

        public static CurrentScoreChannel CurrentScore => new CurrentScoreChannel();

        public static CurrentComboChannel CurrentCombo => new CurrentComboChannel();

        public static CurrentTimingChannel CurrentTiming => new CurrentTimingChannel();

        public static ScreenWidthChannel ScreenWidth => new ScreenWidthChannel();

        public static ScreenHeightChannel ScreenHeight => new ScreenHeightChannel();

        public static ProductChannel ScreenAspectRatio => ScreenWidth / ScreenHeight;

        public static ScreenIs16By9Channel Is16By9 => new ScreenIs16By9Channel();

        public ValueChannel LaneFrom
        {
            get => laneFrom;
            set
            {
                laneFrom = value;
                Services.Scenecontrol.AddReferencedController(this);
            }
        }

        public ValueChannel LaneTo
        {
            get => laneTo;
            set
            {
                laneTo = value;
                Services.Scenecontrol.AddReferencedController(this);
            }
        }

        [MoonSharpHidden]
        public string SerializedType { get; set; } = "context";

        public static ProductChannel BeatLength(int timingGroup = 0)
            => 60000 / Bpm(timingGroup);

        public static BPMChannel Bpm(int timingGroup = 0)
            => new BPMChannel(timingGroup);

        public static DivisorChannel Divisor(int timingGroup = 0)
            => new DivisorChannel(timingGroup);

        public static FloorPositionChannel FloorPosition(int timingGroup = 0)
            => new FloorPositionChannel(timingGroup);

        [MoonSharpHidden]
        public List<object> SerializeProperties(ScenecontrolSerialization serialization)
        {
            return new List<object>
            {
                serialization.AddUnitAndGetId(LaneFrom),
                serialization.AddUnitAndGetId(LaneTo),
            };
        }

        [MoonSharpHidden]
        public void DeserializeProperties(List<object> properties, ScenecontrolDeserialization deserialization)
        {
            int offset = 0;
            laneFrom = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
            laneTo = deserialization.GetUnitFromId<ValueChannel>(properties[offset++]);
        }

        [MoonSharpHidden]
        public void UpdateController(int timing)
        {
            Values.LaneFrom = laneFrom.ValueAt(timing);
            Values.LaneTo = laneTo.ValueAt(timing);
            float x = Values.LaneFrom;
            float y = Values.LaneTo;
        }

        [MoonSharpHidden]
        public void CleanController()
        {
            laneFrom = new ConstantChannel(1);
            laneTo = new ConstantChannel(4);
            Values.LaneFrom = 1;
            Values.LaneTo = 4;
        }
    }
}