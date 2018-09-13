using System;
using System.IO;
namespace NBitcoin.SolarCoin
{
    public class SolarCoinStream : BitcoinStream
    {
        public SolarCoinStream(Stream inner, bool serializing) : base(inner, serializing)
        {
        }

        public uint Version { get; set; }
    }
}
