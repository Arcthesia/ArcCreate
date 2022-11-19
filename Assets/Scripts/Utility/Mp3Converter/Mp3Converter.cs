using NAudio.Wave;

namespace Utility.Mp3Converter
{
    public static class Mp3Converter
    {
        public static void Mp3ToWav(string mp3FileName, string waveFileName)
        {
            using (var reader = new Mp3FileReader(mp3FileName))
            {
                using (var writer = new WaveFileWriter(waveFileName, reader.WaveFormat))
                {
                    reader.CopyTo(writer);
                }
            }
        }
    }
}
