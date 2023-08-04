using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ArcCreate.SceneTransition
{
    public class TransitionSequence
    {
        private readonly List<TransitionStep> onShow = new List<TransitionStep>();
        private readonly List<TransitionStep> onHide = new List<TransitionStep>();
        private readonly List<UniTask> waitTask = new List<UniTask>();
        private short mode = 0;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public int WaitDurationMs { get; set; }

        public int ShowDurationMs { get; private set; } = 0;

        public int HideDurationMs { get; private set; } = 0;

        public int FullSequenceMs => ShowDurationMs + WaitDurationMs + HideDurationMs;

        public float FullSequenceSeconds => FullSequenceMs / 1000f;

        public float WaitDurationSeconds => WaitDurationMs / 1000f;

        public float ShowDurationSeconds => ShowDurationMs / 1000f;

        public float HidedDurationSeconds => HideDurationMs / 1000f;

        public async UniTask Show()
        {
            KillAllAnimations();
            waitTask.Clear();
            for (int i = 0; i < onShow.Count; i++)
            {
                TransitionStep step = onShow[i];
                waitTask.Add(step.Show(cts.Token));
            }

            await UniTask.WhenAll(waitTask);
        }

        public async UniTask Hide()
        {
            KillAllAnimations();
            waitTask.Clear();
            for (int i = 0; i < onHide.Count; i++)
            {
                TransitionStep step = onHide[i];
                waitTask.Add(step.Hide(cts.Token));
            }

            await UniTask.WhenAll(waitTask);
        }

        public void DisableGameObject()
        {
            for (int i = 0; i < onShow.Count; i++)
            {
                TransitionStep step = onShow[i];
                step.Transition.DisableGameObject();
            }

            for (int i = 0; i < onHide.Count; i++)
            {
                TransitionStep step = onHide[i];
                step.Transition.DisableGameObject();
            }
        }

        public void EnableGameObject()
        {
            for (int i = 0; i < onShow.Count; i++)
            {
                TransitionStep step = onShow[i];
                step.Transition.DisableGameObject();
            }

            for (int i = 0; i < onHide.Count; i++)
            {
                TransitionStep step = onHide[i];
                step.Transition.EnableGameObject();
            }
        }

        public TransitionSequence OnShow()
        {
            mode = -1;
            return this;
        }

        public TransitionSequence OnHide()
        {
            mode = 1;
            return this;
        }

        public TransitionSequence OnBoth()
        {
            mode = 0;
            return this;
        }

        public TransitionSequence SetWaitDuration(int ms)
        {
            WaitDurationMs = ms;
            return this;
        }

        public TransitionSequence AddTransition(ITransition transition, int delay = 0)
        {
            if (mode <= 0)
            {
                onShow.Add(new TransitionStep(transition, delay, false));
                ShowDurationMs = Mathf.Max(ShowDurationMs, delay + transition.DurationMs);
            }

            if (mode >= 0)
            {
                onHide.Add(new TransitionStep(transition, delay, false));
                HideDurationMs = Mathf.Max(HideDurationMs, delay + transition.DurationMs);
            }

            return this;
        }

        public TransitionSequence AddTransitionReversed(ITransition transition, int delay = 0)
        {
            if (mode <= 0)
            {
                onShow.Add(new TransitionStep(transition, delay, true));
                ShowDurationMs = Mathf.Max(ShowDurationMs, delay + transition.DurationMs);
            }

            if (mode >= 0)
            {
                onHide.Add(new TransitionStep(transition, delay, true));
                HideDurationMs = Mathf.Max(HideDurationMs, delay + transition.DurationMs);
            }

            return this;
        }

        private void KillAllAnimations()
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }

        private struct TransitionStep
        {
            public ITransition Transition;
            public bool IsReversed;
            public int Delay;

            public TransitionStep(ITransition transiton, int delay, bool isReversed)
            {
                Transition = transiton;
                IsReversed = isReversed;
                Delay = delay;
            }

            public async UniTask Show(CancellationToken ct)
            {
                if (Delay > 0)
                {
                    bool cancelled = await UniTask.Delay(Delay, cancellationToken: ct).SuppressCancellationThrow();
                    if (cancelled)
                    {
                        return;
                    }
                }

                if (!IsReversed)
                {
                    await Forward();
                }
                else
                {
                    await Backward(ct);
                }
            }

            public async UniTask Hide(CancellationToken ct)
            {
                if (Delay > 0)
                {
                    bool cancelled = await UniTask.Delay(Delay, cancellationToken: ct).SuppressCancellationThrow();
                    if (cancelled)
                    {
                        return;
                    }
                }

                if (IsReversed)
                {
                    await Forward();
                }
                else
                {
                    await Backward(ct);
                }
            }

            private async UniTask Forward()
            {
                Transition.EnableGameObject();
                await Transition.StartTransition();
            }

            private async UniTask Backward(CancellationToken ct)
            {
                await Transition.EndTransition();
                if (ct.IsCancellationRequested)
                {
                    return;
                }

                Transition.DisableGameObject();
            }
        }
    }
}