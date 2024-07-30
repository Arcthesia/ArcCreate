using System.Collections.Generic;
using UnityEngine;

namespace ArcCreate.Storage.Data
{
    public class CharacterStorage : StorageUnit<CharacterStorage>
    {
        public override string Type => "Character";

        public Dictionary<string,string> Name { get; set; }

        public string IconPath { get; set; }

        public string ImagePath { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public float Scale { get; set; }

        public override bool ValidateSelf(out string reason)
        {
            if (!base.ValidateSelf(out reason))
            {
                return false;
            }

            if (Name.Count == 0
            || !Name.TryGetValue("default", out string defaultName)
            || string.IsNullOrEmpty(defaultName))
            {
                reason = "No valid default character name provided";
                return false;
            }

            if (string.IsNullOrEmpty(ImagePath))
            {
                reason = "Image path is not defined";
                return false;
            }

            if (string.IsNullOrEmpty(IconPath))
            {
                reason = "Icon path is not defined";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public string GetDisplayName()
        {
            if (!Name.TryGetValue(I18n.CurrentLocale, out string name))
            {
                return Name["default"];
            }

            return name;
        }

        public override void Insert()
        {
            base.Insert();

            // Hacky solution for now. Will be deleted after proper selection menu is added
            PlayerPrefs.SetString("Selection.LastCharacter", Identifier);
        }

        public struct CharacterImportInformation
        {
            public Dictionary<string,string> Name { get; set; }

            public string IconPath { get; set; }

            public string ImagePath { get; set; }

            public float X { get; set; }

            public float Y { get; set; }

            public float Scale { get; set; }
        }
    }
}