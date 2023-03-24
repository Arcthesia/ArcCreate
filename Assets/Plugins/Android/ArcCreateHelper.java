package com.Arcthesia.ArcCreate;

import android.os.Environment;
import java.io.OutputStream;
import java.io.FileOutputStream;
import android.database.Cursor;
import java.io.InputStream;
import android.net.Uri;
import android.content.ContentResolver;
import android.content.Intent;
import android.content.DialogInterface;
import android.content.Context;
import android.app.AlertDialog;
import android.os.Build;
import java.io.File;
import java.io.FileNotFoundException;
import android.util.Log;
import com.unity3d.player.UnityPlayer;
import android.os.Bundle;
import com.unity3d.player.UnityPlayerActivity;

public class ArcCreateHelper
{
    public String processInputStream(final InputStream in, final String path) {
        try {
            final File file = new File(path);
            final OutputStream out = new FileOutputStream(file);
            final byte[] buffer = new byte[1024];
            int size;
            while ((size = in.read(buffer)) != -1) {
                out.write(buffer, 0, size);
            }
            out.close();
            
            return "";
        }
        catch (Exception e) {
            Log.e("MainActivity", "InputStreamToFile exception: " + e.getMessage());
            
            return e.getMessage();
        }
    }
}
