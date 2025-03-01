using System.Security.Cryptography;

namespace ChatApi.server.Services
{
    public class FileHasher
    {
        public static string ComputeHash(byte[] data, HashAlgorithmName hashAlgorithmName)
        {
            using var hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName.Name);
            var Hash = hashAlgorithm.ComputeHash(data);
            return BitConverter.ToString(Hash).Replace("-", "");
        }

        public static string ComputeHash(Stream stream, HashAlgorithmName hashAlgorithmName)
        {
            if (stream == null || !stream.CanRead)
                throw new ArgumentException("Stream is null or cannot be read");

            using var hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName.Name);
            var Hash = hashAlgorithm.ComputeHash(stream);
            return BitConverter.ToString(Hash).Replace("-", "");
        }

        public static async Task<string> ComputeHashAsync(Stream stream, HashAlgorithmName hashAlgorithmName)
        {
            if (stream == null || !stream.CanRead)
                throw new ArgumentException("Stream is null or cannot be read");

            using var hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName.Name);
            var Hash = await hashAlgorithm.ComputeHashAsync(stream);
            return BitConverter.ToString(Hash).Replace("-", "");
        }
    }
}
