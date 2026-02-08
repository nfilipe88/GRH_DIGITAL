using System.Security.Cryptography;
using System.Text;

namespace HRManager.WebAPI.Helpers
{
    public static class GuidUtility
    {
        public static readonly Guid IsoOidNamespace = new Guid("6ba7b812-9dad-11d1-80b4-00c04fd430c8");

        public static Guid Create(Guid namespaceId, string name)
        {
            return Create(namespaceId, name, 5);
        }

        public static Guid Create(Guid namespaceId, string name, int version)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (version != 3 && version != 5)
                throw new ArgumentOutOfRangeException(nameof(version), "version must be either 3 or 5.");

            var nameBytes = Encoding.UTF8.GetBytes(name);
            var namespaceBytes = namespaceId.ToByteArray();

            SwapByteOrder(namespaceBytes);

            var hash = version == 3
                ? ComputeMD5Hash(namespaceBytes, nameBytes)
                : ComputeSHA1Hash(namespaceBytes, nameBytes);

            var newGuid = new byte[16];
            Array.Copy(hash, 0, newGuid, 0, 16);

            newGuid[6] = (byte)((newGuid[6] & 0x0F) | (version << 4));
            newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

            SwapByteOrder(newGuid);
            return new Guid(newGuid);
        }

        private static byte[] ComputeMD5Hash(byte[] namespaceBytes, byte[] nameBytes)
        {
            using var md5 = MD5.Create();
            var combinedBytes = new byte[namespaceBytes.Length + nameBytes.Length];
            Array.Copy(namespaceBytes, 0, combinedBytes, 0, namespaceBytes.Length);
            Array.Copy(nameBytes, 0, combinedBytes, namespaceBytes.Length, nameBytes.Length);

            return md5.ComputeHash(combinedBytes);
        }

        private static byte[] ComputeSHA1Hash(byte[] namespaceBytes, byte[] nameBytes)
        {
            using var sha1 = SHA1.Create();
            var combinedBytes = new byte[namespaceBytes.Length + nameBytes.Length];
            Array.Copy(namespaceBytes, 0, combinedBytes, 0, namespaceBytes.Length);
            Array.Copy(nameBytes, 0, combinedBytes, namespaceBytes.Length, nameBytes.Length);

            var hash = sha1.ComputeHash(combinedBytes);
            var truncatedHash = new byte[16];
            Array.Copy(hash, 0, truncatedHash, 0, 16);

            return truncatedHash;
        }

        private static void SwapByteOrder(byte[] guid)
        {
            SwapBytes(guid, 0, 3);
            SwapBytes(guid, 1, 2);
            SwapBytes(guid, 4, 5);
            SwapBytes(guid, 6, 7);
        }

        private static void SwapBytes(byte[] array, int left, int right)
        {
            (array[left], array[right]) = (array[right], array[left]);
        }
    }
}
