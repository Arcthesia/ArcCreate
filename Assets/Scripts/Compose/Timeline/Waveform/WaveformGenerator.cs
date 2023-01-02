using UnityEngine;

namespace ArcCreate.Compose.Timeline
{
    public static class WaveformGenerator
    {
        public static Texture2D GetWaveformTexture(AudioClip clip, Vector2 size, float fromSecond, float toSecond, Color waveformBG, Color waveformColor)
        {
            float samplePerSecond = clip.samples / clip.length;
            int fromSample = (int)(fromSecond * samplePerSecond);
            int toSample = (int)(toSecond * samplePerSecond);
            int sampleCount = Mathf.Abs((toSample - fromSample) * clip.channels);

            float[] samples = new float[sampleCount];
            if (clip.GetData(samples, fromSample) == false)
            {
                return null;
            }

            // Setup texture
            int width = (int)size.x * 2;
            int height = (int)size.y;
            Texture2D texture = new Texture2D(width, height);

            // Get sampleIntensity
            int resolution = sampleCount / width;

            // Draw color array per pixel in width
            Color32[] colors = new Color32[height];
            float midHeight = height / 2f;
            float sampleComp = 0f;
            for (int i = 0; i < width; i++)
            {
                float sampleChunk = 0;
                for (int j = 0; j < resolution; j++)
                {
                    sampleChunk = Mathf.Max(samples[(i * resolution) + j], sampleChunk);
                }

                for (int h = 0; h < height; h++)
                {
                    // Get value of height relative to totalHeight
                    if (h < midHeight)
                    {
                        sampleComp = Mathf.InverseLerp(midHeight, 0, h);
                    }
                    else
                    {
                        sampleComp = Mathf.InverseLerp(midHeight, height, h);
                    }

                    // Correlate to sample height
                    if (sampleComp <= sampleChunk || (Mathf.Abs(h - midHeight) < 1))
                    {
                        colors[h] = waveformColor;
                    }
                    else
                    {
                        colors[h] = waveformBG;
                    }
                }

                texture.SetPixels32(i, 0, 1, height, colors);
            }

            texture.Apply();
            return texture;
        }
    }
}