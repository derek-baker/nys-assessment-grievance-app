using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Library.Services.Crypto
{
    public static class HashService
    {
        public static readonly RNGCryptoServiceProvider cryptoProvider = new RNGCryptoServiceProvider();
        
        public static string HashData(string data, byte[] salt)
        {
            string hashedPassword = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: data,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8
                )
            );
            return hashedPassword;
        }

        public static byte[] GenerateSalt(int saltBytesMaxLengh = 32)
        {
            var salt = new byte[saltBytesMaxLengh];
            cryptoProvider.GetNonZeroBytes(salt);
            return salt;
        }
    }
}
