using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using ArcCreate.Storage.Data;
using ArcCreate.Utility.Base62;
using UltraLiteDB;

namespace ArcCreate.Storage
{
    public class FileStorage
    {
        public static string StoragePath => FileStatics.FileStoragePath;

        public static UltraLiteCollection<FileReference> Collection => Database.Current.GetCollection<FileReference>();

        public static void ImportFile(string filePath, string virtualPath)
        {
            if (Collection.FindById(virtualPath) != null)
            {
                throw new Exception("Duplicate virtual path");
            }

            // Read file content
            using (FileStream stream = File.OpenRead(filePath))
            {
                // Calculate hash
                byte[] hashBytes = ComputeHash(stream);
                string hash = hashBytes.ToBase62();

                string ext = Path.GetExtension(filePath);
                string correctHashPath = hash + ext;

                // Resolve collision
                bool shouldStoreFile = true;

                string path = correctHashPath;

                // If file with same hash already exists
                while (File.Exists(Path.Combine(StoragePath, path)))
                {
                    using (FileStream sameHashStream = File.OpenRead(Path.Combine(StoragePath, hash + ext)))
                    {
                        // Check if file contents are the same
                        if (stream.Length == sameHashStream.Length)
                        {
                            // Reset position for re-reading
                            stream.Position = 0;
                            int b;
                            while ((b = stream.ReadByte()) == sameHashStream.ReadByte() && b != -1)
                            {
                            }

                            // EOF. Files are the same
                            if (b == -1)
                            {
                                shouldStoreFile = false;
                                break;
                            }
                        }
                    }

                    // Files are different. This is a collision
                    IncrementOneBit(ref hashBytes);
                    path = hashBytes.ToBase62() + ext;
                }

                // Hash should now be unique. Copy the file (unless a file with the same content already exists).
                string fullPath = Path.Combine(StoragePath, path);
                if (shouldStoreFile)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    }

                    File.Copy(filePath, fullPath);
                }

                // Store file content into DB
                Collection.Insert(new FileReference
                {
                    VirtualPath = virtualPath,
                    RealPath = path,
                    CorrectHashPath = correctHashPath,
                });
            }

            File.Delete(filePath);
        }

        public static string GetFilePath(string virtualPath)
        {
            return Path.Combine(StoragePath, Collection.FindById(virtualPath)?.RealPath);
        }

        public static void DeleteReference(string referenceId)
        {
            FileReference reference = Collection.FindById(referenceId);

            // Find other references pointing to the same file
            IEnumerable<FileReference> sameRealPath = Collection.Find(Query.EQ("RealPath", reference.RealPath));

            // Only delete the physical file if there are no references left
            if (sameRealPath.Count() == 1)
            {
                // Find references in the same hash group
                IEnumerable<FileReference> hashGroup = Collection.Find(Query.EQ("CorrectHashPath", reference.CorrectHashPath));

                if (hashGroup.Any())
                {
                    // Get max hash
                    string maxHash = null;
                    foreach (FileReference refr in hashGroup)
                    {
                        if (StringComparer.OrdinalIgnoreCase.Compare(refr.RealPath, maxHash) > 0)
                        {
                            maxHash = refr.RealPath;
                        }
                    }

                    // Replace deleted file with max hash file
                    if (maxHash != reference.RealPath)
                    {
                        File.Copy(maxHash, reference.RealPath, true);

                        // Update references that uses maxHash to the new path
                        IEnumerable<FileReference> useMaxHashFiles = Collection.Find(Query.EQ("RealPath", maxHash));
                        foreach (var refr in useMaxHashFiles)
                        {
                            refr.RealPath = reference.RealPath;
                        }

                        Collection.UpsertBulk(useMaxHashFiles);
                    }

                    File.Delete(maxHash);
                }
            }

            Collection.Delete(reference.VirtualPath);
        }

        public static void Clear()
        {
            if (!Directory.Exists(StoragePath))
            {
                return;
            }

            DirectoryInfo dir = new DirectoryInfo(StoragePath);
            dir.Delete(true);
            dir.Create();
        }

        private static void IncrementOneBit(ref byte[] bytes)
        {
            byte carry = 1;
            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                if (bytes[i] < byte.MaxValue)
                {
                    bytes[i] += carry;
                    return;
                }
                else
                {
                    bytes[i] = 0;
                    carry = 1;
                }
            }

            return;
        }

        private static byte[] ComputeHash(FileStream stream)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(stream);
            }
        }
    }
}