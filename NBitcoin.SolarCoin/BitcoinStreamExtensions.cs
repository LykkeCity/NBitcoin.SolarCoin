using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NBitcoin.SolarCoin
{
    public static class BitcoinStreamExtensions
    {
        static uint512.MutableUint512 _mutableUint512 = new uint512.MutableUint512(uint512.Zero);

        public static void ReadWriteVersionEncoded(this BitcoinStream stream, ref uint version)
        {
            if (stream.Serializing)
            {
                stream.ReadWrite(version);
            }
            else
            {
                stream.ReadWrite(ref version);
            }
        }

        public static void ReadWrite(this BitcoinStream stream, ref uint512 value)
        {
            value = value ?? uint512.Zero;
            _mutableUint512.Value = value;
            stream.ReadWrite(ref _mutableUint512);
            value = _mutableUint512.Value;
        }

        public static void ReadWriteArray(this BitcoinStream stream, ref uint512[] value)
        {
            if (stream.Serializing)
            {
                var list = value?.Select(v => v.AsBitcoinSerializable()).ToArray();
                stream.ReadWrite(ref list);
            }
            else
            {
                List<uint512.MutableUint512> list = null;
                stream.ReadWrite(ref list);
                value = list.Select(l => l.Value).ToArray();
            }
        }

        public static void ReadWriteArray(this BitcoinStream stream, ref uint256[] value)
        {
            if (stream.Serializing)
            {
                var list = value?.Select(v => v.AsBitcoinSerializable()).ToArray();
                stream.ReadWrite(ref list);
            }
            else
            {
                List<uint256.MutableUint256> list = null;
                stream.ReadWrite(ref list);
                value = list.Select(l => l.Value).ToArray();
            }
        }
    }
}