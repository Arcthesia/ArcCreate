using System;
using UnityEngine;

namespace ArcCreate.Gameplay.Hitsound
{
    public interface IHitsoundPlayer : IDisposable
    {
        void LoadTap(AudioClip clip);

        void LoadArc(AudioClip clip);

        void PlayTap();

        void PlayArc();

        void SetVolume(float v);
    }
}