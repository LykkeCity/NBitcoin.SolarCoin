using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NBitcoin.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Utilities;

namespace NBitcoin.SolarCoin
{
    public enum PrimaryActions : uint
    {
        // primary actions
        SER_NETWORK = (1 << 0),
        SER_DISK = (1 << 1),
        SER_GETHASH = (1 << 2),

        // modifiers
        SER_SKIPSIG = (1 << 16),
        SER_BLOCKHEADERONLY = (1 << 17),
        SER_LEGACYPROTOCOL = (1 << 18),
    }

    public class SolarCoinTransaction : Transaction
    {
        #region SolarTransaction

        private const int LEGACY_VERSION_1 = 1;
        private const int LEGACY_VERSION_2 = 2; // V3 - Includes nTime
        private const int LEGACY_VERSION_3 = 3; // V4 - Includes nTime in tx hash
        private const int CURRENT_VERSION = 4;

        #endregion

        public SolarCoinTransaction()
        {
            Version = CURRENT_VERSION;
        }

        public SolarCoinTransaction(string hex) : base(hex)
        {
        }

        public uint NTime { get; protected set; }

        public uint NType { get; protected set; }

        public string TransactionComment { get; protected set; }

        public override ConsensusFactory GetConsensusFactory()
        {
            return SolarCoinConsensusFactory.Instance;
        }

        public override uint256 GetSignatureHash(Script scriptCode, int nIn, SigHash nHashType, Money amount,
            HashVersion sigversion, PrecomputedTransactionData precomputedTransactionData)
        {
            //TODO: Find right algo
            return uint256.Zero;
        }

        private BitcoinStream CreateHashWriter(HashVersion version)
        {
            var hs = CreateSignatureHashStream();
            BitcoinStream stream = new BitcoinStream(hs, true);
            stream.Type = SerializationType.Hash;
            stream.TransactionOptions = version == HashVersion.Original ? TransactionOptions.None : TransactionOptions.Witness;
            return stream;
        }

        private static uint256 GetHash(BitcoinStream stream)
        {
            var preimage = ((HashStreamBase)stream.Inner).GetHash();
            stream.Inner.Dispose();
            return preimage;
        }

        /*
        IMPLEMENT_SERIALIZE
        (
            READWRITE(this->nVersion);
            nVersion = this->nVersion;
            // if (!(nType & (SER_GETHASH|SER_LEGACYPROTOCOL))) {
            if (!(nType & (SER_GETHASH|SER_LEGACYPROTOCOL)) || this->nVersion > LEGACY_VERSION_3) {
                READWRITE(nTime);
            } else if (nType & SER_DISK) {
                READWRITE(nTime);
            }
            READWRITE(vin);
            READWRITE(vout);
            READWRITE(nLockTime);
            if(this->nVersion > LEGACY_VERSION_1) {
            READWRITE(strTxComment);
            }
        )
         */

        public override void ReadWrite(BitcoinStream stream)
        {
            // we can't use "ref" keyword with properties,
            // so copy base class properties to new variables for value types,
            // and get references to reference types
            var nVersion = Version;
            var nTime = NTime;
            var nLockTime = LockTime;
            var vin = Inputs;
            var vout = Outputs;
            string txComment = TransactionComment;
            byte[] byteArr = null;
            var solarStream = new SolarCoinStream(stream.Inner, stream.Serializing);

            solarStream.ReadWriteVersionEncoded(ref nVersion);
            if ((NType & (uint)(PrimaryActions.SER_GETHASH | PrimaryActions.SER_LEGACYPROTOCOL)) == 0 || 
                nVersion > LEGACY_VERSION_3)
            {
                solarStream.ReadWriteVersionEncoded(ref nTime);
            }
            else if ((NType & (uint)PrimaryActions.SER_DISK) != 0)
            {
                solarStream.ReadWriteVersionEncoded(ref nTime);
            }

            stream.ReadWrite<TxInList, TxIn>(ref vin);
            Type vinType = vin.GetType();
            //var vinTransactionSetter = vinType.GetProperty("Transaction", BindingFlags.Instance);
            //vinTransactionSetter?.SetValue(vin, this);
            ////vin.Transaction.
            stream.ReadWrite<TxOutList, TxOut>(ref vout);
            //var voutTransactionSetter = vout.GetType().GetProperty("Transaction", BindingFlags.Instance);
            //voutTransactionSetter?.SetValue(vout, this);
            solarStream.ReadWriteStruct(ref nLockTime);

            if (nVersion > LEGACY_VERSION_1)
                solarStream.ReadWriteAsVarString(ref byteArr);

            if (!solarStream.Serializing)
            {
                Version = nVersion;
                LockTime = nLockTime;
                NTime = nTime;
            }
        }

        public uint256 GetHashCheat()
        {
            var previousType = this.NType;
            this.NType = (uint)PrimaryActions.SER_GETHASH;
            uint256 h = null;

            //using (var cheat = new BLAKE2bWriter())
            //{
            //    this.ReadWrite(cheat);

            //    h = cheat.GetHash();
            //}

            using (var hs = new HashStreamCheat())
            {
                this.ReadWrite(new BitcoinStream(hs, true)
                {
                    TransactionOptions = TransactionOptions.None,
                    ConsensusFactory = GetConsensusFactory(),
                });
                h = hs.GetHash();
            }

            this.NType = previousType;

            return h;
        }

        protected override HashStreamBase CreateHashStream()
        {
            return new HashStream();
        }

        protected override HashStreamBase CreateSignatureHashStream()
        {
            return new HashStream();
        }
    }

    /// <summary>
    /// Double SHA256 hash stream
    /// </summary>
    public class HashStreamCheat : HashStreamBase
    {
        public HashStreamCheat()
        {

        }

        public override bool CanRead
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanSeek
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanWrite
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

#if HAS_SPAN
		public override void Write(ReadOnlySpan<byte> buffer)
		{
			int copied = 0;
			int toCopy = 0;
			var innerSpan = new Span<byte>(_Buffer, _Pos, _Buffer.Length - _Pos);
			while(!buffer.IsEmpty)
			{
				toCopy = Math.Min(innerSpan.Length, buffer.Length);
				buffer.Slice(0, toCopy).CopyTo(innerSpan.Slice(0, toCopy));
				buffer = buffer.Slice(toCopy);
				innerSpan = innerSpan.Slice(toCopy);
				copied += toCopy;
				_Pos += toCopy;
				if(ProcessBlockIfNeeded())
					innerSpan = _Buffer.AsSpan();
			}
		}
#endif
        public override void Write(byte[] buffer, int offset, int count)
        {
            int copied = 0;
            int toCopy = 0;
            while (copied != count)
            {
                toCopy = Math.Min(_Buffer.Length - _Pos, count - copied);
                Buffer.BlockCopy(buffer, offset + copied, _Buffer, _Pos, toCopy);
                copied += toCopy;
                _Pos += toCopy;
                ProcessBlockIfNeeded();
            }
        }

        byte[] _Buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(32 * 10);
        int _Pos;

        public override void WriteByte(byte value)
        {
            _Buffer[_Pos++] = value;
            ProcessBlockIfNeeded();
        }

        private bool ProcessBlockIfNeeded()
        {
            if (_Pos == _Buffer.Length)
            {
                ProcessBlock();
                return true;
            }
            return false;
        }


        //Sha256Digest sha = new Sha256Digest();
        //private void ProcessBlock()
        //{
        //    sha.BlockUpdate(_Buffer, 0, _Pos);
        //    _Pos = 0;
        //}

        //public override uint256 GetHash()
        //{
        //    ProcessBlock();
        //    sha.DoFinal(_Buffer, 0);
        //    _Pos = 32;
        //    ProcessBlock();
        //    sha.DoFinal(_Buffer, 0);
        //    return new uint256(_Buffer.Take(32).ToArray());
        //}

        SHA256Managed sha = new SHA256Managed();
        private void ProcessBlock()
        {
            sha.TransformBlock(_Buffer, 0, _Pos, null, -1);
            _Pos = 0;
        }

        static readonly byte[] Empty = new byte[0];
        public override uint256 GetHash()
        {
            ProcessBlock();
            sha.TransformFinalBlock(Empty, 0, 0);
            var hash1 = sha.Hash;
            Buffer.BlockCopy(sha.Hash, 0, _Buffer, 0, 32);
            sha.Initialize();
            sha.TransformFinalBlock(_Buffer, 0, 32);
            var hash2 = sha.Hash;
            return new uint256(hash2);
        }

        protected override void Dispose(bool disposing)
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(_Buffer);
            if (disposing)
                sha.Dispose();
            base.Dispose(disposing);
        }
    }
}


