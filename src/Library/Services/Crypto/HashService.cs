using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Library.Services.Crypto
{
    public static class HashService
    {
        public static readonly RNGCryptoServiceProvider _cryptoProvider = new RNGCryptoServiceProvider();

        public static int GenerateSecurityCode(string value, string salt)
        {
            var hash = HashData<byte[]>(value, Encoding.Unicode.GetBytes(salt));
            return Math.Abs(BitConverter.ToInt32(hash));
        }
        
        /// <typeparam name="T">string | byte[]</typeparam>
        public static T HashData<T>(string data, byte[] salt)
        {
            var hash = 
                KeyDerivation.Pbkdf2(
                    password: data,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8
                );

            return (typeof(T) == typeof(string))
                ? (T)Convert.ChangeType(Convert.ToBase64String(hash), typeof(T))
                : (T)Convert.ChangeType(hash, typeof(T));
        }

        public static byte[] GenerateSalt(int saltBytesMaxLength = 32)
        {
            var salt = new byte[saltBytesMaxLength];
            _cryptoProvider.GetNonZeroBytes(salt);
            return salt;
        }

        public static string ConvertSaltToString(byte[] salt) 
            => Convert.ToBase64String(salt);        
    }
}
