using System;
using UnityEngine;

namespace ArcCreate.Gameplay.Hitsound
{
    public interface IHitsoundPlayer : IDisposable
    {
        float Volume { get; set; }

        void LoadTap(AudioClip clip);

        void LoadArc(AudioClip clip);

        void PlayTap();

        void PlayArc();
    }
}