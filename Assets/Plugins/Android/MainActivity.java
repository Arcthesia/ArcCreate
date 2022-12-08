package com.Arcthesia.ArcCreate;
import com.unity3d.player.UnityPlayerActivity;
import android.os.Bundle;
import android.os.Build;
import android.view.Display;
import android.view.Window;
import android.view.Surface;
import android.view.WindowManager;
public class MainActivity extends UnityPlayerActivity {
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            Window w = getWindow();
            WindowManager.LayoutParams p = w.getAttributes();
            Display.Mode[] modes = getDisplay().getSupportedModes();
            //find display mode with max hz
            int maxMode = 0;
            float maxHZ = 60f;
            for(Display.Mode m:modes) {
                if (maxHZ < m.getRefreshRate()) {
                    maxHZ = m.getRefreshRate();
                    maxMode = m.getModeId();
                }
            }
            p.preferredDisplayModeId = maxMode;
            w.setAttributes(p);
        }
    }
}