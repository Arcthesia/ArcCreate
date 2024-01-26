using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ArcCreate.Gameplay.Audio
{
    public class HoldPauseHandler : IPauseButtonHandler
    {
        private readonly PauseButton parent;
        private readonly float minDuration;
        private bool released;
        private CancellationTokenSource cts;

        public HoldPauseHandler(PauseButton parent, float minDuration)
        {
            this.parent = parent;
            this.minDuration = minDuration;
            cts = new CancellationTokenSource();
        }

        public void OnClick()
        {
            HoldTask(cts.Token).Forget();
            released = false;
        }

        public void OnRelease()
        {
            released = true;
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }

        private async UniTask HoldTask(CancellationToken token)
        {
            await UniTask.Delay((int)(minDuration * 1000), cancellationToken: token);
            if (!released)
            {
                parent.Activate();
            }
        }
    }
}