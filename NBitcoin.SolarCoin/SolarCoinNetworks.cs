using System;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;

namespace NBitcoin.SolarCoin
{
    public class SolarCoinNetworks : NetworkSetBase
    {
        private SolarCoinNetworks()
        {
            EnsureRegistered();
        }

        public static SolarCoinNetworks Instance { get; } = new SolarCoinNetworks();

        public override string CryptoCode => "SLR";

        protected override NetworkBuilder CreateMainnet()
        {
            NetworkBuilder builder = new NetworkBuilder();
            builder.SetConsensus(new Consensus()
                {
                    SubsidyHalvingInterval = 210000,
                    MajorityEnforceBlockUpgrade = 750,
                    MajorityRejectBlockOutdated = 950,
                    MajorityWindow = 1000,
                    BIP34Hash = new uint256("0x000000000000024b89b42a942fe0d9fea3bb44ab7bd1b19115dd6a759c0808b8"),
                    PowLimit = new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                    PowTargetTimespan = TimeSpan.FromSeconds(16 * 60),
                    PowTargetSpacing = TimeSpan.FromSeconds(1 * 60),
                    PowAllowMinDifficultyBlocks = false,
                    PowNoRetargeting = false,
                    RuleChangeActivationThreshold = 48,
                    MinerConfirmationWindow = 64,
                    CoinbaseMaturity = 100,
                    LitecoinWorkCalculation = true,
                    ConsensusFactory = SolarCoinConsensusFactory.Instance
                })
                .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 18 })
                .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 5 })
                .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 146 })
                .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x08, 0xC5, 0xD1 })
                .SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x22, 0xBE, 0xD7 })
                //.SetNetworkStringParser(new Litecoin())
                .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("slr"))
                .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("slr"))
                .SetMagic(0x04f104fd)
                .SetPort(18188)
                .SetRPCPort(18188)
                .SetName("solar-main")
                .AddAlias("slr-mainnet")
                .AddAlias("solar-mainnet")
                .AddAlias("solarcoin-main")
                .AddDNSSeeds(new[]
                {
                    new DNSSeedData("dnsseed.solarcoin.org", "seed.solarcoin.org"),
                    new DNSSeedData("download.solarcoin.org", "seed.solarcoin.org")
                })
                .AddSeeds(new NetworkAddress[0])
                .SetGenesis("edcf32dbfd327fe7f546d3a175d91b05e955ec1224e087961acc9a2aa8f592ee");
            return builder;
        }

        protected override NetworkBuilder CreateRegtest()
        {
            return new NetworkBuilder()
                .SetConsensus(new Consensus()
                {
                    SubsidyHalvingInterval = 150,
                    MajorityEnforceBlockUpgrade = 750,
                    MajorityRejectBlockOutdated = 950,
                    MajorityWindow = 1000,
                    PowLimit = new Target(new uint256("0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f0f")),
                    PowTargetTimespan = TimeSpan.FromSeconds(3.5 * 24 * 60 * 60),
                    PowTargetSpacing = TimeSpan.FromSeconds(2.5 * 60),
                    PowAllowMinDifficultyBlocks = true,
                    PowNoRetargeting = false,
                    RuleChangeActivationThreshold = 1512,
                    MinerConfirmationWindow = 2016,
                    CoinbaseMaturity = 100,
                    LitecoinWorkCalculation = true,
                    SupportSegwit = false,
                    ConsensusFactory = SolarCoinConsensusFactory.Instance
                })
                .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 0x1D, 0x25 })
                .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 0x1C, 0xBA })
                .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 0xEF })
                .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x35, 0x87, 0xCF })
                .SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x35, 0x83, 0x94 })
                .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("reg"))
                .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("reg"))
                .SetMagic(0xf1c8d2fd)
                .SetPort(18233)
                .SetRPCPort(18232)
                .SetName("SolarCoin-reg")
                .AddAlias("SolarCoin-regtest")
                .AddDNSSeeds(new[]
                {
                    new DNSSeedData("z.cash", "dnsseed.testnet.z.cash")
                })
                .AddSeeds(new NetworkAddress[0])
                .SetGenesis("01936b7db1eb4ac39f151b8704642d0a8bda13ec547d54cd5e43ba142fc6d8877cab07b3");
        }

        protected override NetworkBuilder CreateTestnet()
        {
            return new NetworkBuilder()
                .SetConsensus(new Consensus()
                {
                    SubsidyHalvingInterval = 840000,
                    MajorityEnforceBlockUpgrade = 51,
                    MajorityRejectBlockOutdated = 75,
                    MajorityWindow = 400,
                    PowLimit = new Target(new uint256("07ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                    PowTargetTimespan = TimeSpan.FromSeconds(3.5 * 24 * 60 * 60),
                    PowTargetSpacing = TimeSpan.FromSeconds(2.5 * 60),
                    PowAllowMinDifficultyBlocks = true,
                    PowNoRetargeting = false,
                    RuleChangeActivationThreshold = 1512,
                    MinerConfirmationWindow = 2016,
                    CoinbaseMaturity = 100,
                    LitecoinWorkCalculation = true,
                    SupportSegwit = false,
                    ConsensusFactory = SolarCoinConsensusFactory.Instance
                })
                .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 0x1D, 0x25 })
                .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 0x1C, 0xBA })
                .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 0xEF })
                .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x35, 0x87, 0xCF })
                .SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x35, 0x83, 0x94 })
                .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("taz"))
                .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("taz"))
                .SetMagic(0xf1c8d2fd)
                .SetPort(18233)
                .SetRPCPort(18232)
                .SetName("SolarCoin-test")
                .AddAlias("SolarCoin-testnet")
                .AddDNSSeeds(new[]
                {
                    new DNSSeedData("z.cash", "dnsseed.testnet.z.cash")
                })
                .AddSeeds(new NetworkAddress[0])
                .SetGenesis("00a6a51259c3f6732481e2d035197218b7a69504461d04335503cd69759b2d02bd2b53a9653f42cb33c608511c953673fa9da76170958115fe92157ad3bb5720d927f18e09459bf5c6072973e143e20f9bdf0584058c96b7c2234c7565f100d5eea083ba5d3dbaff9f0681799a113e7beff4a611d2b49590563109962baa149b628aae869af791f2f70bb041bd7ebfa658570917f6654a142b05e7ec0289a4f46470be7be5f693b90173eaaa6e84907170f32602204f1f4e1c04b1830116ffd0c54f0b1caa9a5698357bd8aa1f5ac8fc93b405265d824ba0e49f69dab5446653927298e6b7bdc61ee86ff31c07bde86331b4e500d42e4e50417e285502684b7966184505b885b42819a88469d1e9cf55072d7f3510f85580db689302eab377e4e11b14a91fdd0df7627efc048934f0aff8e7eb77eb17b3a95de13678004f2512293891d8baf8dde0ef69be520a58bbd6038ce899c9594cf3e30b8c3d9c7ecc832d4c19a6212747b50724e6f70f6451f78fd27b58ce43ca33b1641304a916186cfbe7dbca224f55d08530ba851e4df22baf7ab7078e9cbea46c0798b35a750f54103b0cdd08c81a6505c4932f6bfbd492a9fced31d54e98b6370d4c96600552fcf5b37780ed18c8787d03200963600db297a8f05dfa551321d17b9917edadcda51e274830749d133ad226f8bb6b94f13b4f77e67b35b71f52112ce9ba5da706ad9573584a2570a4ff25d29ab9761a06bdcf2c33638bf9baf2054825037881c14adf3816ba0cbd0fca689aad3ce16f2fe362c98f48134a9221765d939f0b49677d1c2447e56b46859f1810e2cf23e82a53e0d44f34dae932581b3b7f49eaec59af872cf9de757a964f7b33d143a36c270189508fcafe19398e4d2966948164d40556b05b7ff532f66f5d1edc41334ef742f78221dfe0c7ae2275bb3f24c89ae35f00afeea4e6ed187b866b209dc6e83b660593fce7c40e143beb07ac86c56f39e895385924667efe3a3f031938753c7764a2dbeb0a643fd359c46e614873fd0424e435fa7fac083b9a41a9d6bf7e284eee537ea7c50dd239f359941a43dc982745184bf3ee31a8dc850316aa9c6b66d6985acee814373be3458550659e1a06287c3b3b76a185c5cb93e38c1eebcf34ff072894b6430aed8d34122dafd925c46a515cca79b0269c92b301890ca6b0dc8b679cdac0f23318c105de73d7a46d16d2dad988d49c22e9963c117960bdc70ef0db6b091cf09445a516176b7f6d58ec29539166cc8a38bbff387acefffab2ea5faad0e8bb70625716ef0edf61940733c25993ea3de9f0be23d36e7cb8da10505f9dc426cd0e6e5b173ab4fff8c37e1f1fb56d1ea372013d075e0934c6919393cfc21395eea20718fad03542a4162a9ded66c814ad8320b2d7c2da3ecaf206da34c502db2096d1c46699a91dd1c432f019ad434e2c1ce507f91104f66f491fed37b225b8e0b2888c37276cfa0468fc13b8d593fd9a2675f0f5b20b8a15f8fa7558176a530d6865738ddb25d3426dab905221681cf9da0e0200eea5b2eba3ad3a5237d2a391f9074bf1779a2005cee43eec2b058511532635e0fea61664f531ac2b356f40db5c5d275a4cf5c82d468976455af4e3362cc8f71aa95e71d394aff3ead6f7101279f95bcd8a0fedce1d21cb3c9f6dd3b182fce0db5d6712981b651f29178a24119968b14783cafa713bc5f2a65205a42e4ce9dc7ba462bdb1f3e4553afc15f5f39998fdb53e7e231e3e520a46943734a007c2daa1eda9f495791657eefcac5c32833936e568d06187857ed04d7b97167ae207c5c5ae54e528c36016a984235e9c5b2f0718d7b3aa93c7822ccc772580b6599671b3c02ece8a21399abd33cfd3028790133167d0a97e7de53dc8ff");
        }
    }
}
