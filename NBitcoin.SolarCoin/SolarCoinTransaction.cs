using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NBitcoin.Crypto;

namespace NBitcoin.SolarCoin
{
    public class SolarCoinTransaction : Transaction
    {
        #region SolarTransaction

        private const int LEGACY_VERSION_1 = 1;
        private const int LEGACY_VERSION_2 = 2; // V3 - Includes nTime
        private const int LEGACY_VERSION_3 = 3; // V4 - Includes nTime in tx hash
        private const int CURRENT_VERSION = 4;

        #endregion

        private const uint OVERWINTER_BRANCH_ID = 0x5ba81b19;
        private const uint OVERWINTER_VERSION = 3;
        private const uint OVERWINTER_VERSION_GROUP_ID = 0x03C48270;
        private const uint SAPLING_BRANCH_ID = 0x76b809bb;
        private const uint SAPLING_VERSION = 4;
        private const uint SAPLING_VERSION_GROUP_ID = 0x892f2085;
        private const uint GROTH_PROOF_SIZE = 192;
        private const uint ZC_SAPLING_ENCCIPHERTEXT_SIZE = 580;
        private const uint ZC_SAPLING_OUTCIPHERTEXT_SIZE = 80;
        private const byte G1_PREFIX_MASK = 0x02;
        private const byte G2_PREFIX_MASK = 0x0a;
        private const uint NOT_AN_INPUT = uint.MaxValue;

        private class SolarCoinSpendDescription : IBitcoinSerializable
        {
            public uint256 cv;
            public uint256 anchor;
            public uint256 nullifier;
            public uint256 rk;
            public byte[] zkproof = new byte[GROTH_PROOF_SIZE];
            public byte[] spendAuthSig = new byte[64];

            public void ReadWrite(BitcoinStream stream)
            {
                stream.ReadWrite(ref cv);
                stream.ReadWrite(ref anchor);
                stream.ReadWrite(ref nullifier);
                stream.ReadWrite(ref rk);
                stream.ReadWrite(ref zkproof);
                stream.ReadWrite(ref spendAuthSig);
            }
        }

        private class SolarCoinOutputDescription : IBitcoinSerializable
        {
            public uint256 cv;
            public uint256 cm;
            public uint256 ephemeralKey;
            public byte[] encCiphertext = new byte[ZC_SAPLING_ENCCIPHERTEXT_SIZE];
            public byte[] outCiphertext = new byte[ZC_SAPLING_OUTCIPHERTEXT_SIZE];
            public byte[] zkproof = new byte[GROTH_PROOF_SIZE];

            public void ReadWrite(BitcoinStream stream)
            {
                stream.ReadWrite(ref cv);
                stream.ReadWrite(ref cm);
                stream.ReadWrite(ref ephemeralKey);
                stream.ReadWrite(ref encCiphertext);
                stream.ReadWrite(ref outCiphertext);
                stream.ReadWrite(ref zkproof);
            }
        }

        private class JSDescription : IBitcoinSerializable
        {
            public long vpub_old;
            public long vpub_new;
            public uint256 anchor;
            public uint256[] nullifiers = { uint256.Zero, uint256.Zero };
            public uint256[] commitments = { uint256.Zero, uint256.Zero };
            public uint256 ephemeralKey;
            public byte[][] ciphertexts = { new byte[601], new byte[601] };
            public uint256 randomSeed = uint256.Zero;
            public uint256[] macs = { uint256.Zero, uint256.Zero };
            public byte[] grothProof = new byte[GROTH_PROOF_SIZE];
            public PHGRProof phgrProof = new PHGRProof();

            public void ReadWrite(BitcoinStream stream)
            {
                stream.ReadWrite(ref vpub_old);
                stream.ReadWrite(ref vpub_new);
                stream.ReadWrite(ref anchor);

                stream.ReadWrite(ref nullifiers[0]);
                stream.ReadWrite(ref nullifiers[1]);
                stream.ReadWrite(ref commitments[0]);
                stream.ReadWrite(ref commitments[1]);
                stream.ReadWrite(ref ephemeralKey);
                stream.ReadWrite(ref randomSeed);
                stream.ReadWrite(ref macs[0]);
                stream.ReadWrite(ref macs[1]);

                if (((SolarCoinStream)stream).Version >= SAPLING_VERSION)
                {
                    stream.ReadWrite(ref grothProof);
                }
                else
                {
                    stream.ReadWrite(ref phgrProof);
                }

                stream.ReadWrite(ref ciphertexts[0]);
                stream.ReadWrite(ref ciphertexts[1]);
            }
        }

        private class PHGRProof : IBitcoinSerializable
        {
            public CompressedG1 g_A;
            public CompressedG1 g_A_prime;
            public CompressedG2 g_B;
            public CompressedG1 g_B_prime;
            public CompressedG1 g_C;
            public CompressedG1 g_C_prime;
            public CompressedG1 g_K;
            public CompressedG1 g_H;

            public void ReadWrite(BitcoinStream stream)
            {
                stream.ReadWrite(ref g_A);
                stream.ReadWrite(ref g_A_prime);
                stream.ReadWrite(ref g_B);
                stream.ReadWrite(ref g_B_prime);
                stream.ReadWrite(ref g_C);
                stream.ReadWrite(ref g_C_prime);
                stream.ReadWrite(ref g_K);
                stream.ReadWrite(ref g_H);
            }
        }

        private class CompressedG1 : IBitcoinSerializable
        {
            public bool y_lsb;
            public uint256 x;

            public void ReadWrite(BitcoinStream stream)
            {
                byte leadingByte = G1_PREFIX_MASK;

                if (y_lsb)
                {
                    leadingByte |= 1;
                }

                stream.ReadWrite(ref leadingByte);

                if ((leadingByte & (~1)) != G1_PREFIX_MASK)
                {
                    throw new InvalidOperationException("lead byte of G1 point not recognized");
                }

                y_lsb = (leadingByte & 1) == 1;

                stream.ReadWrite(ref x);
            }
        }

        private class CompressedG2 : IBitcoinSerializable
        {
            public bool y_gt;
            public uint512 x;

            public void ReadWrite(BitcoinStream stream)
            {
                byte leadingByte = G2_PREFIX_MASK;

                if (y_gt)
                {
                    leadingByte |= 1;
                }

                stream.ReadWrite(ref leadingByte);

                if ((leadingByte & (~1)) != G2_PREFIX_MASK)
                {
                    throw new InvalidOperationException("lead byte of G2 point not recognized");
                }

                y_gt = (leadingByte & 1) == 1;

                BitcoinStreamExtensions.ReadWrite(stream, ref x);
            }
        }

        private static readonly char[] SolarCoin_PREVOUTS_HASH_PERSONALIZATION = { 'Z', 'c', 'a', 's', 'h', 'P', 'r', 'e', 'v', 'o', 'u', 't', 'H', 'a', 's', 'h' };
        private static readonly char[] SolarCoin_SEQUENCE_HASH_PERSONALIZATION = { 'Z', 'c', 'a', 's', 'h', 'S', 'e', 'q', 'u', 'e', 'n', 'c', 'H', 'a', 's', 'h' };
        private static readonly char[] SolarCoin_OUTPUTS_HASH_PERSONALIZATION = { 'Z', 'c', 'a', 's', 'h', 'O', 'u', 't', 'p', 'u', 't', 's', 'H', 'a', 's', 'h' };
        private static readonly char[] SolarCoin_JOINSPLITS_HASH_PERSONALIZATION = { 'Z', 'c', 'a', 's', 'h', 'J', 'S', 'p', 'l', 'i', 't', 's', 'H', 'a', 's', 'h' };
        private static readonly char[] SolarCoin_SHIELDED_SPENDS_HASH_PERSONALIZATION = { 'Z', 'c', 'a', 's', 'h', 'S', 'S', 'p', 'e', 'n', 'd', 's', 'H', 'a', 's', 'h' };
        private static readonly char[] SolarCoin_SHIELDED_OUTPUTS_HASH_PERSONALIZATION = { 'Z', 'c', 'a', 's', 'h', 'S', 'O', 'u', 't', 'p', 'u', 't', 'H', 'a', 's', 'h' };

        private bool fOverwintered = false;
        private uint nVersionGroupId = 0;
        private uint? nBranchId = 0;
        private uint nExpiryHeight = 0;
        private long valueBalance = 0;
        private List<SolarCoinSpendDescription> vShieldedSpend = new List<SolarCoinSpendDescription>();
        private List<SolarCoinOutputDescription> vShieldedOutput = new List<SolarCoinOutputDescription>();
        private List<JSDescription> vjoinsplit = new List<JSDescription>();
        private uint256 joinSplitPubKey;

        public SolarCoinTransaction()
        {
            Version = SAPLING_VERSION;
            nVersionGroupId = SAPLING_VERSION_GROUP_ID;
        }

        public SolarCoinTransaction(string hex, uint? branchId = null) : base(hex)
        {
            nBranchId = branchId;
        }

        public uint NTime { get; protected set; }

        public string TransactionComment { get; protected set; }

        public override ConsensusFactory GetConsensusFactory()
        {
            return SolarCoinConsensusFactory.Instance;
        }

        public override uint256 GetSignatureHash(Script scriptCode, int nIn, SigHash nHashType, Money amount,
            HashVersion sigversion, PrecomputedTransactionData precomputedTransactionData)
        {
            if (sigversion == HashVersion.Witness)
            {
                if (amount == null)
                    throw new ArgumentException("The amount of the output being signed must be provided", "amount");
                uint256 hashPrevouts = uint256.Zero;
                uint256 hashSequence = uint256.Zero;
                uint256 hashOutputs = uint256.Zero;

                if ((nHashType & SigHash.AnyoneCanPay) == 0)
                {
                    hashPrevouts = precomputedTransactionData == null ?
                                   GetHashPrevouts() : precomputedTransactionData.HashPrevouts;
                }

                if ((nHashType & SigHash.AnyoneCanPay) == 0 && ((uint)nHashType & 0x1f) != (uint)SigHash.Single && ((uint)nHashType & 0x1f) != (uint)SigHash.None)
                {
                    hashSequence = precomputedTransactionData == null ?
                                   GetHashSequence() : precomputedTransactionData.HashSequence;
                }

                if (((uint)nHashType & 0x1f) != (uint)SigHash.Single && ((uint)nHashType & 0x1f) != (uint)SigHash.None)
                {
                    hashOutputs = precomputedTransactionData == null ?
                                    GetHashOutputs() : precomputedTransactionData.HashOutputs;
                }
                else if (((uint)nHashType & 0x1f) == (uint)SigHash.Single && nIn < this.Outputs.Count)
                {
                    BitcoinStream ss = CreateHashWriter(sigversion);
                    ss.ReadWrite(this.Outputs[nIn]);
                    hashOutputs = GetHash(ss);
                }

                BitcoinStream sss = CreateHashWriter(sigversion);
                // Version
                sss.ReadWrite(this.Version);
                // Input prevouts/nSequence (none/all, depending on flags)
                sss.ReadWrite(hashPrevouts);
                sss.ReadWrite(hashSequence);
                // The input being signed (replacing the scriptSig with scriptCode + amount)
                // The prevout may already be contained in hashPrevout, and the nSequence
                // may already be contain in hashSequence.
                sss.ReadWrite(Inputs[nIn].PrevOut);
                sss.ReadWrite(scriptCode);
                sss.ReadWrite(amount.Satoshi);
                sss.ReadWrite((uint)Inputs[nIn].Sequence);
                // Outputs (none/one/all, depending on flags)
                sss.ReadWrite(hashOutputs);
                // Locktime
                sss.ReadWriteStruct(LockTime);
                // Sighash type
                sss.ReadWrite((uint)nHashType);

                return GetHash(sss);
            }

            if (nIn >= Inputs.Count)
            {
                return uint256.One;
            }

            var hashType = nHashType & (SigHash)31;

            // Check for invalid use of SIGHASH_SINGLE
            if (hashType == SigHash.Single)
            {
                if (nIn >= Outputs.Count)
                {
                    return uint256.One;
                }
            }

            var scriptCopy = new Script(scriptCode.ToBytes());
            //scriptCopy = scriptCopy.(OpcodeType.OP_CODESEPARATOR);

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

        }

        public void ReadWriteOld(BitcoinStream stream)
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
            //if (nVersion > LEGACY_VERSION_3)
            solarStream.ReadWriteVersionEncoded(ref nTime);

            stream.ReadWrite<TxInList, TxIn>(ref vin);
            var vinTransactionSetter = vin.GetType().GetProperty(nameof(vin.Transaction), BindingFlags.Instance);
            vinTransactionSetter?.SetValue(vin, this);
            stream.ReadWrite<TxOutList, TxOut>(ref vout);
            var voutTransactionSetter = vout.GetType().GetProperty(nameof(vin.Transaction), BindingFlags.Instance);
            voutTransactionSetter?.SetValue(vout, this);
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

        protected override HashStreamBase CreateHashStream()
        {
            return new HashStream();
        }

        protected override HashStreamBase CreateSignatureHashStream()
        {
            return new HashStream();
        }

        private uint256 GetHashOutputs()
        {
            using (var ss = new BLAKE2bWriter(SolarCoin_OUTPUTS_HASH_PERSONALIZATION))
            {
                foreach (var txout in Outputs)
                {
                    ss.ReadWrite(txout);
                }

                return ss.GetHash();
            }
        }

        private uint256 GetHashSequence()
        {
            using (var ss = new BLAKE2bWriter(SolarCoin_SEQUENCE_HASH_PERSONALIZATION))
            {
                foreach (var input in Inputs)
                {
                    ss.ReadWrite((uint)input.Sequence);
                }

                return ss.GetHash();
            }
        }

        private uint256 GetHashPrevouts()
        {
            using (var ss = new BLAKE2bWriter(SolarCoin_PREVOUTS_HASH_PERSONALIZATION))
            {
                foreach (var input in Inputs)
                {
                    ss.ReadWrite(input.PrevOut);
                }

                return ss.GetHash();
            }
        }

        private uint256 GetJoinSplitsHash()
        {
            using (var ss = new BLAKE2bWriter(SolarCoin_JOINSPLITS_HASH_PERSONALIZATION))
            {
                // provide version info to joinSplit serializer
                ss.Version = Version;

                foreach (var js in vjoinsplit)
                {
                    ss.ReadWrite(js);
                }

                ss.ReadWrite(joinSplitPubKey);

                return ss.GetHash();
            }
        }

        private uint256 GetShieldedSpendsHash()
        {
            using (var ss = new BLAKE2bWriter(SolarCoin_SHIELDED_SPENDS_HASH_PERSONALIZATION))
            {
                foreach (var spend in vShieldedSpend)
                {
                    ss.ReadWrite(spend.cv);
                    ss.ReadWrite(spend.anchor);
                    ss.ReadWrite(spend.nullifier);
                    ss.ReadWrite(spend.rk);
                    ss.ReadWrite(ref spend.zkproof);
                }

                return ss.GetHash();
            }
        }

        private uint256 GetShieldedOutputsHash()
        {
            using (var ss = new BLAKE2bWriter(SolarCoin_SHIELDED_OUTPUTS_HASH_PERSONALIZATION))
            {
                foreach (var sout in vShieldedOutput)
                {
                    ss.ReadWrite(sout);
                }

                return ss.GetHash();
            }
        }
    }
}
