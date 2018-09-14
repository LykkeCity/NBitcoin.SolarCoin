using System;
using System.IO;
using System.Text;
using NBitcoin.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using SCrypt = Org.BouncyCastle.Crypto.Generators.SCrypt;

namespace NBitcoin.SolarCoin
{
    public class BLAKE2bWriter : SolarCoinStream, IDisposable
    {
        public BLAKE2bWriter() : base(new MemoryStream(), true)
        {
            TransactionOptions = TransactionOptions.None;
            Type = SerializationType.Hash;
        }

        public uint256 GetHash()
        {
            var blake2b = new Sha256Digest();
            var hash1 = new byte[blake2b.GetDigestSize()];
            var hash2 = new byte[blake2b.GetDigestSize()];

            var buffer = ((MemoryStream) Inner).ToArrayEfficient();
            var h = NBitcoin.Crypto.SCrypt.ComputeDerivedKey(buffer, buffer, 1024, 1, 1, null, 32);
            return new uint256(h);
            //blake2b.BlockUpdate(((MemoryStream)Inner).ToArrayEfficient(), 0, (int)Inner.Length);
            //blake2b.DoFinal(hash1, 0);
            //blake2b.BlockUpdate(hash1, 0, (int)hash1.Length);
            //blake2b.DoFinal(hash2, 0);

            return new uint256(hash2);
        }

        public void Dispose()
        {
            Inner.Dispose();
        }
    }
}

/*
        / invalidates the object
        uint256 GetHash()
        {
        uint256 hash1;
        SHA256_Final((unsigned char *) & hash1, &ctx);
        uint256 hash2;
        SHA256((unsigned char *) & hash1, sizeof(hash1), (unsigned char*)&hash2);
        return hash2;
        }
 *
 */