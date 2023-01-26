using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ArcCreate.Compose
{
    public static class EasterEggs
    {
        private static readonly HashSet<Graphic> AffectedByColor = new HashSet<Graphic>();

        private static readonly Color Red = new Color(0.44f, 0.09f, 0.09f, 1);
        private static readonly Color Orange = new Color(0.44f, 0.28f, 0.09f, 1);
        private static readonly Color Yellow = new Color(0.43f, 0.44f, 0.09f, 1);
        private static readonly Color Green = new Color(0.11f, 0.44f, 0.09f, 1);
        private static readonly Color Blue = new Color(0.09f, 0.15f, 0.44f, 1);
        private static readonly Color Purple = new Color(0.31f, 0.09f, 0.44f, 1);

        private static readonly Dictionary<string, string> Strings = new Dictionary<string, string>()
        {
            { "triggeron", "The chaos ensues..." },
            { "triggeroff", "The chaos subsides..." },
            { "nullapoptt", "null apo is not a 10.6" },
            { "unsunscribe", "can i have chart file pls i just want to practice 🥰" },
            { "overshart", "amazing 5⭐ overchart xoxo" },
            { "xixi", "god not another one" },
            { "btstan", "I question your tastes in music..." },
            { "sdvx", "Let me guess. Sound Voltex?" },
            { "pentachrome", "Hello!" },
            { "vineboom", "🗿" },
            { "inthatlight", "I find deliverance..." },
            { "taronuke", "💕" },
        };

        public static void AddAffectedByColor(Graphic graphic)
        {
            AffectedByColor.Add(graphic);
        }

        public static void RemoveAffectedByColor(Graphic graphic)
        {
            AffectedByColor.Remove(graphic);
        }

        public static bool TryTrigger(string text)
        {
            if (text == "random")
            {
                ShowRandom();
                return true;
            }
            else if (text == "triggeron")
            {
                Settings.EnableEasterEggs.Value = true;
            }
            else if (text == "triggeroff")
            {
                Settings.EnableEasterEggs.Value = false;
            }

            if (Strings.ContainsKey(text))
            {
                Display(text);
                return true;
            }

            return false;
        }

        public static void ShowRandom()
        {
            var keyList = Strings.Keys.ToList();
            int index = Random.Range(0, keyList.Count);
            string key = keyList[index];

            Display(key);
        }

        private static void Display(string key)
        {
            Services.Popups.Notify(Popups.Severity.Info, Strings[key]);

            if (key == "pentachrome")
            {
                foreach (var obj in AffectedByColor)
                {
                    obj.color = Red;
                    obj.DOColor(Orange, 0.2f).OnComplete(() =>
                        obj.DOColor(Yellow, 0.2f).OnComplete(() =>
                        obj.DOColor(Green, 0.2f).OnComplete(() =>
                        obj.DOColor(Blue, 0.2f).OnComplete(() =>
                        obj.DOColor(Purple, 0.2f)))));
                }
            }

            if (key == "vineboom")
            {
                Services.Popups.PlayVineBoom();
            }
        }
    }
}