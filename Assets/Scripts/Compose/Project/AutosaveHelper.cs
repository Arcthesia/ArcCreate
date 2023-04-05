using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ArcCreate.Compose.Project
{
    public class AutosaveHelper : IDisposable
    {
        private readonly ProjectService projectService;
        private readonly int interval;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public AutosaveHelper(ProjectService projectService, int interval)
        {
            this.projectService = projectService;
            this.interval = interval;
            StartSession().Forget();
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
        }

        private async UniTask StartSession()
        {
            CancellationToken ct = cts.Token;
            while (true)
            {
                bool isCancelled = await UniTask.Delay(interval * 1000, cancellationToken: ct).SuppressCancellationThrow();
                if (isCancelled)
                {
                    return;
                }

                projectService.SaveProject();
            }
        }
    }
}