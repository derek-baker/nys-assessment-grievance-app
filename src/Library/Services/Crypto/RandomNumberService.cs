using System;
using System.Security.Cryptography;

namespace Library.Services.Crypto
{
    public static class RandomNumberService
    {
        public static int GenerateRandomNumber()
        {
            using var cryptoProvider = new RNGCryptoServiceProvider();
            var randomNumber = new byte[4]; // 4 for int32
            cryptoProvider.GetBytes(randomNumber);
            int value = BitConverter.ToInt32(randomNumber, 0);
            return (value < 0) ? value * -1 : value;
        }
    }
}
