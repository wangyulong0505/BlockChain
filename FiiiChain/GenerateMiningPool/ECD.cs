using NSec.Cryptography;
using SimpleBase;
using System;

namespace GenerateMiningPool
{
    public class ECD : IDisposable
    {
        public byte[] PrivateKey { get; set; }
        public byte[] PublicKey { get; set; }

        public static SignatureAlgorithm Algorithm = SignatureAlgorithm.Ed25519;

        public static ECD GenerateNewKeyPair()
        {
            var dsa = new ECD();

            var raw = RandomGenerator.Default.GenerateBytes(Algorithm.PrivateKeySize);
            using (var key = Key.Import(Algorithm, raw, KeyBlobFormat.RawPrivateKey, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport }))
            {
                dsa.PrivateKey = key.Export(KeyBlobFormat.PkixPrivateKey);
                dsa.PublicKey = key.Export(KeyBlobFormat.PkixPublicKey);

                return dsa;
            }
        }

        public static ECD ImportPrivateKey(byte[] privateKey)
        {
            var dsa = new ECD();
            var a = SignatureAlgorithm.Ed25519;
            using (var key = Key.Import(Algorithm, privateKey, KeyBlobFormat.PkixPrivateKey, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport }))
            {
                dsa.PrivateKey = privateKey;
                dsa.PublicKey = key.Export(KeyBlobFormat.PkixPublicKey);

                return dsa;
            }
        }

        public static ECD ImportPublicKey(byte[] publicKey)
        {
            var dsa = new ECD();
            dsa.PublicKey = publicKey;

            return dsa;
        }

        public byte[] SingnData(byte[] data)
        {
            using (var key = Key.Import(Algorithm, this.PrivateKey, KeyBlobFormat.PkixPrivateKey, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport }))
            {
                return Algorithm.Sign(key, data);
            }
        }

        public bool VerifyData(byte[] data, byte[] signature)
        {
            var pubK = NSec.Cryptography.PublicKey.Import(Algorithm, this.PublicKey, KeyBlobFormat.PkixPublicKey);
            return Algorithm.Verify(pubK, data, signature);
        }

        public void Dispose()
        {
        }
    }
}