using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NBitcoin.Crypto;
using NBitcoin.SolarCoin.Extensions;
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
        private const uint _nBestHeight = 2444966;
        private const int LAST_POW_BLOCK = 835213;
        private const int FORK_HEIGHT_1 = 1177000;
        #endregion

        public SolarCoinTransaction()
        {
            InitTransaction();
        }

        public SolarCoinTransaction(string hex) : base(hex)
        {
            InitTransaction();
        }

        public uint NTime { get; set; }

        public uint NType { get; protected set; }

        public string TransactionComment { get; protected set; }

        public override ConsensusFactory GetConsensusFactory()
        {
            return SolarCoinConsensusFactory.Instance;
        }

        public new SolarCoinTransaction Clone()
        {
            var instance = (SolarCoinTransaction)GetConsensusFactory().CreateTransaction();
            instance.FromBytes(this.ToBytes());
            return instance;
        }

        public override uint256 GetSignatureHash(Script scriptCode, int nIn, SigHash nHashType, Money amount,
            HashVersion sigversion, PrecomputedTransactionData precomputedTransactionData)
        {
            if (nIn >= Inputs.Count)
            {
                //Utils.log("ERROR: SignatureHash() : nIn=" + nIn + " out of range\n");
                return uint256.One;
            }

            var hashType = nHashType & (SigHash)31;

            // Check for invalid use of SIGHASH_SINGLE
            if (hashType == SigHash.Single)
            {
                if (nIn >= Outputs.Count)
                {
                    //Utils.log("ERROR: SignatureHash() : nOut=" + nIn + " out of range\n");
                    return uint256.One;
                }
            }

            var scriptCopy = new Script(scriptCode.ToBytes());
            scriptCopy = scriptCopy.FindAndDelete(OpcodeType.OP_CODESEPARATOR);

            var txCopy = GetConsensusFactory().CreateTransaction();
            txCopy.FromBytes(this.ToBytes());
            //Set all TxIn script to empty string
            foreach (var txin in txCopy.Inputs)
            {
                txin.ScriptSig = new Script();
            }

            //Copy subscript into the txin script you are checking
            txCopy.Inputs[nIn].ScriptSig = scriptCopy;

            if (hashType == SigHash.None)
            {
                //The output of txCopy is set to a vector of zero size.
                txCopy.Outputs.Clear();

                //All other inputs aside from the current input in txCopy have their nSequence index set to zero
                foreach (var input in txCopy.Inputs.Where((x, i) => i != nIn))
                    input.Sequence = 0;
            }
            else if (hashType == SigHash.Single)
            {
                //The output of txCopy is resized to the size of the current input index+1.
                txCopy.Outputs.RemoveRange(nIn + 1, txCopy.Outputs.Count - (nIn + 1));
                //All other txCopy outputs aside from the output that is the same as the current input index are set to a blank script and a value of (long) -1.
                for (var i = 0; i < txCopy.Outputs.Count; i++)
                {
                    if (i == nIn)
                        continue;
                    txCopy.Outputs[i] = new TxOut();
                }

                //All other txCopy inputs aside from the current input are set to have an nSequence index of zero.
                foreach (var input in txCopy.Inputs.Where((x, i) => i != nIn))
                    input.Sequence = 0;
            }


            if ((nHashType & SigHash.AnyoneCanPay) != 0)
            {
                //The txCopy input vector is resized to a length of one.
                var script = txCopy.Inputs[nIn];
                txCopy.Inputs.Clear();
                txCopy.Inputs.Add(script);
                //The subScript (lead in by its length as a var-integer encoded!) is set as the first and only member of this vector.
                txCopy.Inputs[0].ScriptSig = scriptCopy;
            }


            //Serialize TxCopy, append 4 byte hashtypecode
            var stream = CreateHashWriter(sigversion);
            txCopy.ReadWrite(stream);
            stream.ReadWrite((uint)nHashType);
            return GetHash(stream);
        }

        private BitcoinStream CreateHashWriter(HashVersion version)
        {
            var hs = CreateSignatureHashStream();
            BitcoinStream stream = new BitcoinStream(hs, true);
            stream.Type = SerializationType.Hash;
            stream.TransactionOptions =
                version == HashVersion.Original ? TransactionOptions.None : TransactionOptions.Witness;
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
            stream.ReadWrite<TxOutList, TxOut>(ref vout);
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

        public new uint256 GetHash()
        {
            var previousType = this.NType;
            this.NType = (uint)PrimaryActions.SER_GETHASH;
            uint256 h = null;

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

        //TODO: Figure out the best strategy to select Adjusted time
        protected uint GetAdjustedTime()
        {
            var expirationForTx = DateTime.UtcNow + TimeSpan.FromDays(2);
            var expirationInUnixTime = expirationForTx.ToUnixTimestamp();

            return (uint)expirationInUnixTime;
        }

        protected void InitTransaction()
        {
            if (_nBestHeight >= LAST_POW_BLOCK)
            {
                if (Version == 0 || Version == 1)
                {
                    if (_nBestHeight >= FORK_HEIGHT_1)
                        Version = CURRENT_VERSION;
                    else
                        Version = LEGACY_VERSION_3;
                }

                if (NTime == 0)
                    NTime = GetAdjustedTime();
            }
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
            get { throw new NotImplementedException(); }
        }

        public override bool CanSeek
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanWrite
        {
            get { throw new NotImplementedException(); }
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
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