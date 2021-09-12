using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;

namespace Library.Services.Crypto
{
    public static class HashService
    {
        /// <summary>
        /// TODO: Generate per-password salt? While this won't prevent collisions during hashing, keeping the salt
        ///       in the app code/DLLs will at least prevent attackers from having it in the event of data exfiltration from the DB.
        /// </summary>
        private static readonly byte[] _salt = new byte[16] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76, 0x41, 0x30, 0x12
        };

        public static string HashData(string data)
        {
            string hashedPassword = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: data,
                    salt: _salt,
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8
                )
            );
            return hashedPassword;
        }
    }
}
