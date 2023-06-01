using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Gameplay.Scenecontrol
{
    [MoonSharpUserData]
    [EmmyAlias("NoteData")]
    [EmmyDoc("Class for getting data about notes")]
    [EmmySingleton]
    public class NoteChannelBuilder
    {
        [EmmyDoc("Channel which returns the timing of a given node.")]
        public static NoteTimingChannel Timing()
            => new NoteTimingChannel();

        [EmmyDoc("Channel which returns the floor-position of a given note.")]
        public static NoteFloorPositionChannel FloorPos()
            => new NoteFloorPositionChannel();

        [EmmyDoc("Channel which returns the x-position of a given note at its start time.")]
        public static NoteXPositionChannel X()
            => new NoteXPositionChannel();

        [EmmyDoc("Channel which returns the y-position of a given note at its start time.")]
        public static NoteYPositionChannel Y()
            => new NoteYPositionChannel();

        [EmmyDoc("Channel which returns the z-position of a given note at its start time.")]
        public static NoteZPositionChannel Z()
            => new NoteZPositionChannel();

        [EmmyDoc("Channel which returns the signed time until a given note is started.")]
        public static SumChannel Delta()
            => Timing() - ValueChannelBuilder.Timing();

        [EmmyDoc("Channel which returns the signed time until a given note is started.")]
        public static NoteIDChannel ID()
            => new NoteIDChannel();

        [EmmyDoc("Channel which returns if a note is an arc")]
        public static NoteIsArcChannel IsArc()
            => new NoteIsArcChannel();

        [EmmyDoc("Channel which returns the type of a note")]
        public static NoteTypeChannel Type()
            => new NoteTypeChannel();
    }
}