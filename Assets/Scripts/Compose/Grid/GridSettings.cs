using System;
using UnityEngine;

namespace ArcCreate.Compose.Grid
{
    public class GridSettings
    {
        private static readonly GridSettings[] SettingSlots = new GridSettings[10];

        public GridSettings(int slot)
        {
            Slot = slot;
            UseDefaultSettings = new BoolSetting($"CustomGrid.{slot}.UseDefault", true);
            FromLane = new IntSetting($"CustomGrid.{slot}.FromLane", 1);
            ToLane = new IntSetting($"CustomGrid.{slot}.ToLane", 4);
            ScaleGridToSkyInput = new BoolSetting($"CustomGrid.{slot}.ScaleGrid", true);
            Script = new StringSetting($"CustomGrid.{slot}.Script", null);
        }

        public int Slot { get; private set; }

        public BoolSetting UseDefaultSettings { get; private set; }

        public IntSetting FromLane { get; private set; }

        public IntSetting ToLane { get; private set; }

        public BoolSetting ScaleGridToSkyInput { get; private set; }

        public StringSetting Script { get; private set; }

        public static void Initialize()
        {
            for (int i = 0; i < SettingSlots.Length; i++)
            {
                SettingSlots[i] = new GridSettings(i);
            }
        }

        public static GridSettings GetSlot(int slot) => SettingSlots[Mathf.Clamp(slot, 0, 9)];

        public IVerticalGridSettings GetVerticalSettings(string fallbackScript)
        {
            if (UseDefaultSettings.Value)
            {
                return new DefaultVerticalGridSettings();
            }

            try
            {
                return new LuaVerticalGridSettings(ScriptWithFallback(fallbackScript));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return new DefaultVerticalGridSettings();
            }
        }

        public string ScriptWithFallback(string fallbackScript)
        {
            return string.IsNullOrEmpty(Script.Value) ? fallbackScript : Script.Value;
        }
    }
}