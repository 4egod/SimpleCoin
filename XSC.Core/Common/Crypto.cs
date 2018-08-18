using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace XSC
{
    using Core;

    public static class Crypto
    {
        public static string GetRandomHash()
        {
            byte[] buf = new byte[32];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(buf);
            return buf.ToHex();
        }

        public static string CalculateHash(object value)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(value.ToBinary()).ToHex();
            }
        }

        public static void CalculateHash(Block block)
        {
            while (true)
            {
                block.CalculateHash();
                if (block.CheckHash())
                {
                    block.Timestamp = DateTime.Now;
                    return;
                }
                    
                Thread.Sleep(1); // Simulate Hardwork and don't overload system
            }
        }

        public static async Task CalculateHashAsync(Block block, ulong difficulty)
        {
            await CalculateHashAsync(block, difficulty, CancellationToken.None);
        }

        public static async Task CalculateHashAsync(Block block, ulong difficulty, CancellationToken token)
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    block.CalculateHash();
                    if (block.CheckHash()) return;
                    //block.Nonce = Crypto.GetRandomHash();
                    Thread.Sleep(1); // Simulate Hardwork
                }
            }, token);
        }

        public static bool VerifyData(object value, string hash)
        {
            string actual;

            using (SHA256 sha = SHA256.Create())
            {
                actual = sha.ComputeHash(value.ToBinary()).ToHex();
            }

            return actual == hash;
        }

        public static string SignHash(string hash, byte[] privateKey)
        {
            using (ECDsaCng dsa = new ECDsaCng(CngKey.Import(privateKey, CngKeyBlobFormat.EccPrivateBlob)))
            {
                return dsa.SignHash(hash.FromHex()).ToHex();
            }
        }

        public static bool VerifyHash(string hash, string publicKey, string signature)
        {
            using (ECDsaCng dsa = new ECDsaCng(CngKey.Import(publicKey.FromHex(), CngKeyBlobFormat.EccPublicBlob)))
            {
                return dsa.VerifyHash(hash.FromHex(), signature.FromHex());
            }
        }

    }
}
