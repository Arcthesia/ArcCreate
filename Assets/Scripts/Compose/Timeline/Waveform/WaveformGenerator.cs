using UnityEngine;

namespace ArcCreate.Compose.Timeline
{
    public static class WaveformGenerator
    {
        private const int AverageSampleCount = 32;

        /// <summary>
        /// Encode the audio clip's waveform into a texture which should be displayed with the Waveform shader.
        /// </summary>
        /// <param name="clip">The audio clip to encode.</param>
        /// <returns>The encoded texture.</returns>
        public static Texture2D EncodeTexture(AudioClip clip)
        {
            int sampleCount = clip.samples * clip.channels;
            int maxSize = SystemInfo.maxTextureSize;

            Texture2D texture = null;
            int width = 0;
            int height = 0;
            while (maxSize > 1024)
            {
                int samplePixelCount = Mathf.CeilToInt(sampleCount / AverageSampleCount / 4f);
                width = Mathf.Min(samplePixelCount, maxSize);
                height = (samplePixelCount + width - 1) / width;

                try
                {
                    texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true)
                    {
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp,
                    };

                    break;
                }
                catch
                {
                    maxSize /= 2;
                }
            }

            if (texture == null)
            {
                throw new System.Exception("Could not create waveform texture, no suitable texture size found");
            }

            float[] samples = new float[sampleCount];
            clip.GetData(samples, 0);

            int sample = 0;

            // Encode sample values into each pixel
            // Each pixel is a RGBA value, which can hold 4 sample.
            // Additionally max-smoothing over AverageSampleCount is performed,
            //   which means one pixel encodes (4 * AverageSampleCount) audio sample.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float s1 = 0;
                    for (int i = sample; i < sample + AverageSampleCount; i++)
                    {
                        s1 = Mathf.Max(s1, i < samples.Length ? samples[i] : 0);
                    }

                    sample += AverageSampleCount;

                    float s2 = 0;
                    for (int i = sample; i < sample + AverageSampleCount; i++)
                    {
                        s2 = Mathf.Max(s2, i < samples.Length ? samples[i] : 0);
                    }

                    sample += AverageSampleCount;

                    float s3 = 0;
                    for (int i = sample; i < sample + AverageSampleCount; i++)
                    {
                        s3 = Mathf.Max(s3, i < samples.Length ? samples[i] : 0);
                    }

                    sample += AverageSampleCount;

                    float s4 = 0;
                    for (int i = sample; i < sample + AverageSampleCount; i++)
                    {
                        s4 = Mathf.Max(s4, i < samples.Length ? samples[i] : 0);
                    }

                    sample += AverageSampleCount;

                    s1 = (s1 + 1) * 0.5f;
                    s2 = (s2 + 1) * 0.5f;
                    s3 = (s3 + 1) * 0.5f;
                    s4 = (s4 + 1) * 0.5f;

                    texture.SetPixel(x, y, new Color(s1, s2, s3, s4));
                }
            }

            texture.Apply(false, true);
            return texture;
        }

        public static int SecondToSample(float seconds, AudioClip clip)
        {
            float samplePerSecond = clip.samples / clip.length;
            return (int)(samplePerSecond * clip.channels * seconds);
        }
    }
}