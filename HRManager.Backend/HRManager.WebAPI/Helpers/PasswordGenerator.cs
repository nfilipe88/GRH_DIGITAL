using System.Security.Cryptography;

namespace HRManager.WebAPI.Helpers
{
    public static class PasswordGenerator
    {
        // Caracteres permitidos (Letras maiúsculas, minúsculas, números e especiais)
        private static readonly char[] Punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();

        public static string Generate(int length = 12)
        {
            if (length < 1 || length > 128)
            {
                throw new ArgumentException("Comprimento da password inválido.");
            }

            using (var rng = RandomNumberGenerator.Create())
            {
                var byteBuffer = new byte[length];

                rng.GetBytes(byteBuffer);

                var count = 0;
                var characterBuffer = new char[length];

                // Garante pelo menos um de cada tipo para passar nas regras do Identity
                characterBuffer[count++] = GetRandomChar("ABCDEFGHJKLMNOPQRSTUVWXYZ", rng);
                characterBuffer[count++] = GetRandomChar("abcdefghijkmnopqrstuvwxyz", rng);
                characterBuffer[count++] = GetRandomChar("0123456789", rng);
                characterBuffer[count++] = GetRandomChar("!@#$%^&*", rng);

                // Preenche o resto
                var allChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*";
                for (int i = count; i < length; i++)
                {
                    characterBuffer[i] = GetRandomChar(allChars, rng);
                }

                return new string(Shuffle(characterBuffer, rng));
            }
        }

        private static char GetRandomChar(string validChars, RandomNumberGenerator rng)
        {
            var randomByte = new byte[1];
            rng.GetBytes(randomByte);
            return validChars[randomByte[0] % validChars.Length];
        }

        private static char[] Shuffle(char[] array, RandomNumberGenerator rng)
        {
            int n = array.Length;
            while (n > 1)
            {
                var box = new byte[1];
                do rng.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                (array[n], array[k]) = (array[k], array[n]);
            }
            return array;
        }
    }
}
