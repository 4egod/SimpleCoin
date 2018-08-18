using System;
using System.IO;
using System.Security.Cryptography;

namespace XSC.Core
{
    using Newtonsoft.Json;

    public class Wallet
    {
        public const string Extension = ".wallet";

        public const int AddressLength = 22;

        [JsonConstructor]
        private Wallet() { }

        [JsonConverter(typeof(HexJsonConverter))]
        [JsonProperty(Required = Required.Always)]
        public byte[] PrivateKey { get; private set; }

        [JsonConverter(typeof(HexJsonConverter))]
        [JsonProperty(Required = Required.Always)]
        public byte[] PublicKey { get; private set; }

        [JsonProperty(Required = Required.Always)]
        public string Address { get; private set; }
        
        public void Export()
        {
            File.WriteAllText(Config.WalletsDirectory + Address + Extension, this.ToJson());
        }

        public static Wallet Generate()
        {
            Wallet res = new Wallet();
            res.InternalGenerate();
            return res;
        }

        public static Wallet Import(byte[] privateKey)
        {
            Wallet res = new Wallet();
            res.InternalImport(privateKey);
            return res;
        }

        public static Wallet Import(string address)
        {
            string path = Config.WalletsDirectory + address + Extension;

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            if (address.Length != AddressLength)
            {
                throw new ArgumentException(address);
            }

            string json = File.ReadAllText(path);
            return Converter.FromJson<Wallet>(json);
        }

        public static string GetAddress(byte[] publicKey)
        {
            var md5 = MD5.Create();
            string res = md5.ComputeHash(publicKey).ToBase58();
            res = res.PadRight(22, 'X'); 
            return res;
        }

        public static string GetAddress(string publicKey)
        {
            return GetAddress(publicKey.FromHex());
        }

        private void InternalGenerate()
        {
            CngKey key = CngKey.Create(CngAlgorithm.ECDsaP256, null, new CngKeyCreationParameters() { ExportPolicy = CngExportPolicies.AllowPlaintextExport });
            PrivateKey = key.Export(CngKeyBlobFormat.EccPrivateBlob);
            PublicKey = key.Export(CngKeyBlobFormat.EccPublicBlob);
            Address = GetAddress(PublicKey);
        }

        private void InternalImport(byte[] privateKey)
        {
            CngKey key = CngKey.Import(privateKey, CngKeyBlobFormat.EccPrivateBlob);
            PrivateKey = privateKey;
            PublicKey = key.Export(CngKeyBlobFormat.EccPublicBlob);
            Address = GetAddress(PublicKey);
        }
    }
}
