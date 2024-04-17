using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

[Preserve]
public static class SplashWorkaround
{
    // WHY IS THIS A THING
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void BeforeSplashScreen()
    {
        if (Application.HasProLicense())
        {
            return;
        }

        Task.Run(StopSplashScreen);
    }

    private static void StopSplashScreen()
    {
        SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
    }
}