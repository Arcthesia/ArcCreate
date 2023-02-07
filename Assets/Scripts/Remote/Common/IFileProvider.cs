using System.IO;

namespace ArcCreate.Remote.Common
{
    public interface IFileProvider
    {
        Stream RespondToFileRequest(string path, out string extension);
    }
}