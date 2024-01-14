namespace ArcCreate.Compose.Navigation
{
    public static class KeybindPriorities
    {
        public const int Mirror = 9000;
        public const int Clipboard = 8000;
        public const int Delete = 7000;
        public const int Selection = 6000;

        public const int NoteCreation = 5000;

        public const int Playback = 4000;
        public const int Grid = 3000;
        public const int Measurer = 2000;
        public const int Dragging = 1000;
        public const int FreeCamera = 0;

        public const int SubConfirm = 999999;
        public const int SubCancel = 999998;
    }
}