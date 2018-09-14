using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Xunit;
using NBitcoin.DataEncoders;
using System.Diagnostics;

namespace NBitcoin.SolarCoin.Tests
{
    public class SolarCoinTransactionTests
    {
        //"9b1ba13de83ae2d80fea0df66d55223a86502ba9d1d3a4bf266573958fa0fbf7"
        //f7f394a11dfc4392aefbc3f5d7376eb0d4a1f2c74588b65e2e7cbc0e774678c4
        //9b1ba13de83ae2d80fea0df66d55223a86502ba9d1d3a4bf266573958fa0fbf7
        public const string v1 = "";
        public const string v2 = "020000004823b652010000000000000000000000000000000000000000000000000000000000000000ffffffff0e044823b6520" +
                                 "101062f503253482fffffffff0100008a5d78456301232103e24875f4ed4fb4873be68c36e75b52231fe1465679472ce4430d3e" +
                                 "12a88f66e7ac0000000000";
        public const string v3 = "030000005391be5605fd00efe4dcb686d4cff47cc01ae91ed561c1b740268d0afd3e772825f6778bc4020000004948304502210" +
                                 "0bfa02fbf9f8348b83b6fcc91f286477cfa72e5ac79771e3918557165859b0af602201b7dece9eff261a850f9fe4477d681aab1" +
                                 "132565dca7a7941d652d0e9075d1f101ffffffff59dbe625ce94ae76ccd680f11be3256ca24b6139c1a756af1e0dc07ce639244" +
                                 "2020000004847304402207ed4f59fd3ac0cd7879adf1260393c9c63bf9b44a9b4e29de3768cac462d340b022068ea859586ba0b" +
                                 "48cc79762f12a7df8485861221290b7c17c80ca43ac00f81d401ffffffff5abc46d5f8f0f09a4c10d21d37f8274ca56d3c879d9" +
                                 "cb3ef242306575ee5234001000000484730440220440b80f18d1fed3d25cd7488b455fca997955d993d2b8c3fbe8850ce716a3e" +
                                 "8102207473e7b8695ed5c81128dcab49ad304c97f76e64a7af8e7469799e1f8ef9431801ffffffff5abc46d5f8f0f09a4c10d21" +
                                 "d37f8274ca56d3c879d9cb3ef242306575ee523400200000048473044022050d8217dc1e92695fbfa71aa8716eb69646b70e8ad" +
                                 "00ab73b242d01959cad45f02200e016588f5a7b063ac0c700828396e4f0b5897ae6c02e093df366c15056c6dc201ffffffff5a6" +
                                 "d039251ffd10501eb3997bc57553ffabe632922900d66816bffdd91bb94900100000049483045022100d62e211b7a742acb71d4" +
                                 "990a306f378af47182411a22c00a6d48f7d7b79b9f5102207d3de2eaf3d24c988313f09d400f3e7af753dffbcd517d3c45f3e70" +
                                 "4b857fe2901ffffffff02000000000000000000957a04671a0000004341045d16e07e62b7255f99211bec61d4026adc667ac3fb" +
                                 "e2506d76f7d69a8adff1b8940f9acb6ae35fd021b42ac0092f3fe5168b4007107169657edb5b1bceaa7153ac0000000000";
        public const string v4 = "04000000d9a2965b013b395dad2f3a224058844f68a4defbad3c302e0c46ba5dcef033189c178814bd010000004948304502210" +
                                 "0f0aa84c4834adc6b1471cfbd315b008031cd6c496f6f075828c123504f2e715402201a6d25571ec2947163c3c9e1dfdd4c76dc" +
                                 "686c71250923205d1e0bc2fb91a76301ffffffff03000000000000000000c02fc4c7100000002321031c150f89577671c52b8fd" +
                                 "ee1f8fc075c5dacdf233ea13a2affa4e540672308b1ac0e80d7c7100000002321031c150f89577671c52b8fdee1f8fc075c5dac" +
                                 "df233ea13a2affa4e540672308b1ac0000000000";

        static SolarCoinTransactionTests()
        {
            SolarCoinNetworks.Instance.EnsureRegistered();
        }

        [Theory]
        //[InlineData(v1)]
        [InlineData(v2)]
        [InlineData(v3)]
        [InlineData(v4)]
        public void ShouldDeserializeAndSerializeTransaction(string hex)
        {
            var transaction = new SolarCoinTransaction(hex);
            var rawTransaction = transaction.ToHex();

            Assert.Equal(hex, rawTransaction);
        }

        //[Fact]
        //public void ShouldDeserializeV1()
        //{
        //    var tx = new SolarCoinTransaction(v1);

        //    Assert.Equal("5c6ba844e1ca1c8083cd53e29971bd82f1f9eea1f86c1763a22dd4ca183ae061", tx.GetHash().ToString());
        //    Assert.Equal("5e6e4ea05be96f47715c193444e77cf9f6bd18afa7fca7800db7dc5d69a024ea", tx.Inputs[0].PrevOut.Hash.ToString());
        //    Assert.Equal(288595012L, tx.Outputs[0].Value.Satoshi);
        //}

        [Fact]
        public void ShouldDeserializeV2()
        {
            var tx = new SolarCoinTransaction(v2);
            string transactionHash = tx.GetHashCheat().ToString();
            string inputSourceHash = tx.Inputs[0].PrevOut.Hash.ToString();

            Assert.Equal("f386f9321f2b7ff862dc133b1b384703b2cacb3281638339be18ad5fb7ac17d0", transactionHash);
            //Assert.Equal("c48b77f62528773efd0a8d2640b7c161d51ee91ac07cf4cfd486b6dce4ef00fd", inputSourceHash);
            //Assert.Equal(300000000L, tx.Outputs[0].Value.Satoshi);
        }

        [Fact]
        public void ShouldDeserializeV3()
        {
            var tx = new SolarCoinTransaction(v3);
            string transactionHash = tx.GetHashCheat().ToString();
            string inputSourceHash = tx.Inputs[0].PrevOut.Hash.ToString();

            Assert.Equal("9b1ba13de83ae2d80fea0df66d55223a86502ba9d1d3a4bf266573958fa0fbf7", transactionHash);
            Assert.Equal("c48b77f62528773efd0a8d2640b7c161d51ee91ac07cf4cfd486b6dce4ef00fd", inputSourceHash);
            //Assert.Equal(300000000L, tx.Outputs[0].Value.Satoshi);
        }

        [Fact]
        public void ShouldDeserializeV4()
        {
            var tx = new SolarCoinTransaction(v4);
            string transactionHash = tx.GetHashCheat().ToString();
            string inputSourceHash = tx.Inputs[0].PrevOut.Hash.ToString();

            Assert.Equal("f7f394a11dfc4392aefbc3f5d7376eb0d4a1f2c74588b65e2e7cbc0e774678c4", transactionHash);
            Assert.Equal("bd1488179c1833f0ce5dba460c2e303cadfbdea4684f845840223a2fad5d393b", inputSourceHash);
            //Assert.Contains(tx.Outputs, x => x.Value.Satoshi == 001000000L);
            //Assert.Contains(tx.Outputs, x => x.Value.Satoshi == 269992303L);
        }

        //[Fact]
        //public void ShouldThrow_IfTransactionIsShielded()
        //{
        //    Assert.Throws<NotSupportedException>(() => new SolarCoinTransaction(v2));
        //}

        //[Fact]
        //public void ShouldSignV1()
        //{
        //    var prevTx = Transaction.Parse("01000000018d600e5b601607b9ba7d788830ce442893ba091520d0ac9706b3f5bce0670696010000006a47304402202d83e5f388d44fbf3d93a55d" +
        //                                   "e8eac7744edb04bfc5845e2392d59c51e7d90a28022043a95b0088bb09022ee519e8421eb5c347f5d8ace24dd4242364605754f171a20121033724" +
        //                                   "13cbd5741751044d91b0d225715e783a3fce9f51dc5491f938634783dff1ffffffff015c9ce111000000001976a9144faeeb51bcd0b49f238b323e" +
        //                                   "5f1c6c8bf11ae02a88ac00000000");
        //    var from = "tmGygFvgg1B35XeX3oC4e78VSiAyRGcCgME";
        //    var fromAddress = new BitcoinPubKeyAddress(from);
        //    var fromPrivateKey = "cTD2Ew71UHXkn2XTJLyfu6Rbo1os5zCF9sKZm4oiXshcYo6YPcKY";
        //    var fromKey = Key.Parse(fromPrivateKey);
        //    var to = "tmLaY2Ceabpd9TgMmPv6zfDfVGEwmAWuPKo";
        //    var toAddress = new BitcoinPubKeyAddress(to);

        //    var hex = new TransactionBuilder()
        //        .AddCoins(prevTx.Outputs.AsCoins())
        //        .Send(toAddress, Money.Coins(0.5m))
        //        .SetChange(fromAddress)
        //        .SendFees(Money.Coins(0.00002380m))
        //        .BuildTransaction(false)
        //        .ToHex();

        //    var tx = new SolarCoinTransaction(hex);

        //    tx.Sign(new[] { fromKey }, prevTx.Outputs.AsCoins().ToArray());

        //    Assert.Equal(
        //        "c749dbf380e287fb84190b4d6695b82e7eb4f91a1f79c68dc0cfb539fbdd45c4",
        //        tx.GetHash().ToString());

        //    Assert.Equal(
        //        "01000000016cf8b84870d09e65f5809772d8d56e7ec69801291fe3ae7f8e6e6446e80886fd000000006b483045022100f5a4d723f4dbd4b5c2c3e603ed67f5a8798c7a264198cf7035" +
        //        "eb88d7e92dce4102204237d7e5af9a2fd64545f6bc8fbad0039510dca378795299454dc0253cd46785012103f9e72f0713a4d4a980309a14a2ba563e0b1125ad067818e77553a1eefb" +
        //        "fc5be7ffffffff0290a2e60e000000001976a9144faeeb51bcd0b49f238b323e5f1c6c8bf11ae02a88ac80f0fa02000000001976a914772efac94ff91e33c6b2540e4b539fbbcd9b0e" +
        //        "bb88ac00000000",
        //        tx.ToHex());
        //}

        [Fact]
        public void ShouldSignV3()
        {
            var tx = new SolarCoinTransaction("030000807082c40301ac3e4e9435a8369e049b47906ddaa09601cd7e7cfe2f229e0bd305202a066f8e0100000000ffffffff02809698000000000019" +
                                          "76a91415b6246e9b88867cdc3e14b9a5085813ca6d8b4888acc9180202000000001976a9144faeeb51bcd0b49f238b323e5f1c6c8bf11ae02a88ac00" +
                                          "0000005782030000");

            var privateKeys = new Key[]
            {
                Key.Parse("cTD2Ew71UHXkn2XTJLyfu6Rbo1os5zCF9sKZm4oiXshcYo6YPcKY", SolarCoinNetworks.Instance.Testnet)
            };

            var coins = new Coin[]
            {
                new Coin(
                    new OutPoint(uint256.Parse("8e6f062a2005d30b9e222ffe7c7ecd0196a0da6d90479b049e36a835944e3eac"), 1),
                    new TxOut(Money.Coins(0.43695697m), privateKeys[0].ScriptPubKey))
            };

            tx.Sign(privateKeys, coins);

            var txHex = tx.ToHex();

            Assert.Equal(
                "030000807082c40301ac3e4e9435a8369e049b47906ddaa09601cd7e7cfe2f229e0bd305202a066f8e010000006b483045022100ee41236b2550aa334948e1b750b8f9dd12f30ea15b" +
                "879fe0bf7f9a86989bef230220336f17006636afc4165bef0a5fc1e79a1204f21ec1f1ad71c3c326e4091bc7fa012103f9e72f0713a4d4a980309a14a2ba563e0b1125ad067818e775" +
                "53a1eefbfc5be7ffffffff0280969800000000001976a91415b6246e9b88867cdc3e14b9a5085813ca6d8b4888acc9180202000000001976a9144faeeb51bcd0b49f238b323e5f1c6c" +
                "8bf11ae02a88ac000000005782030000",
                tx.ToHex());
        }

        [Fact]
        public void ShouldSignV4()
        {
            var tx = new SolarCoinTransaction(v4);

            var privateKeys = new Key[]
            {
                Key.Parse("cVWdihupzUy3GyP5bha15Dk1W1ejbETBngMV51xATMJzr4Z6fnRk", SolarCoinNetworks.Instance.Testnet)
            };

            var coins = new Coin[]
            {
                new Coin(
                    new OutPoint(uint256.Parse("e7ab8fb7c05097e88a29a816bdc75c958e58a8deee659c14baf9ffe51da96307"), 1),
                    new TxOut(Money.Coins(2.70996151m), privateKeys[0].ScriptPubKey))
            };

            tx.Sign(privateKeys, coins);

            Assert.Equal(
                "0400008085202f89010763a91de5fff9ba149c65eedea8588e955cc7bd16a8298ae89750c0b78fabe7010000006a473044022018a289ad0bec96d8a6f17aef76df46c38e4932f17433f" +
                "133d3df309f33c712740220311906fa7c6baff0c2388a87e55272429f1de6f311f8d651dd0d9be4772b634d01210250b36ab2839e868e6c12c9cb252c3d7b71b61a7039e3c6a55a53ca" +
                "c6f8a1c17bffffffff0240420f00000000001976a9141fecb553b1cff0364a7308ffd9ec8169495cf47288ac6fc11710000000001976a9147176f5d11e11c59a1248ef0bf0d6dadb2be" +
                "1686188ac00000000c943ff020000000000000000000000",
                tx.ToHex());
        }

        [Theory]
        [InlineData("030000807082c403024201cfb1cd8dbf69b8250c18ef41294ca97993db546c1fe01f7e9c8e36d6a5e29d4e30a703ac6a0098421c69378af1e40f64e125946f62c2fa7b2fecbcb6" +
                    "4b6968912a6381ce3dc166d56a1d62f5a8d7056363635353e8c7203d026af786387ae60100080063656a63ac520023752997f4ff0400075151005353656597b0e4e4c705fc0502" +
                    "0000000000000000000000000000000076495c222f7fba1e31defa3d5a57efc2e1e9b01a035587d5fb1a38e01d94903d3c3e0ad3360c1d3710acd20b183e31d49f25c9a138f49b" +
                    "1a537edcf04be34a9851a7af9db6990ed83dd64af3597c04323ea51b0052ad8084a8b9da948d320dadd64f5431e61ddf658d24ae67c22c8d1309131fc00fe7f235734276d38d47" +
                    "f1e191e00c7a1d48af046827591e9733a97fa6b679f3dc601d008285edcbdae69ce8fc1be4aac00ff2711ebd931de518856878f73476f21a482ec9378365c8f7393c94e2885315" +
                    "eb4671098b79535e790fe53e29fef2b3766697ac32b4f473f468a008e72389fc03880d780cb07fcfaabe3f1a84b27db59a4a153d882d2b2103596555ed9494c6ac893c49723833" +
                    "ec8926c1039586a7afcf4a0d9c731e985d99589c03b838e8aaf745533ed9e8ae3a1cd074a51a20da8aba18d1dbebbc862ded42435e02476930d069896cff30eb414f727b89e001" +
                    "afa2fb8dc3436d75a4a6f26572504b0b2232ecb9f0c02411e52596bc5e90457e745939ffedbd12863ce71a02af117d417adb3d15cc54dcb1fce467500c6b8fb86b12b56da9c382" +
                    "857deecc40a98d5f2903395ee4762dd21afdbb5d47fa9a6dd984d567db2857b927b7fae2db587105415d0242789d38f50b8dbcc129cab3d17d19f3355bcf73cecb8cb8a5da0130" +
                    "7152f13902a270572670dc82d39026c6cb4cd4b0f7f5aa2a4f5a5341ec5dd715406f2fdd2a02733f5f641c8c21862a1bafce2609d9eecfa158cfb5cd79f88008e315dc7d838803" +
                    "6c1782fd2795d18a763624c25fa959cc97489ce75745824b77868c53239cfbdf73caec65604037314faaceb56218c6bd30f8374ac13386793f21a9fb80ad03bc0cda4a44946c00" +
                    "e1b1a1df0e5b87b5bece477a709649e950060591394812951e1fe3895b8cc3d14d2cf6556df6ed4b4ddd3d9a69f53357d7767f4f5ccbdbc596631277f8fecd08cb056b95e3025b" +
                    "9792fff7f244fc716269b926d62e9596fa825c6bf21aff9e68625a192440ea06828123d97884806f15fa08da52754a1095e3ff1abd5ce4fddfccfc3a6128aef784a64610a89d1a" +
                    "7099216d0814d3a2d452431c32d411ac1cce82ad0229407bbc48985675e3f874a4533f1d63a84dfa3e0f460fe2f57e34fbc75423c3737f5b2a0615f5722db041a3ef66fa483afd" +
                    "3c2e19e59444a64add6df1d963f5dd5b5010d3d025f0287c4cf19c75f33d51ddddba5d657b43ee8da645443814cc7329f3e9b4e54c236c29af3923101756d9fa4bd0f7d2ddaacb" +
                    "6b0f86a2658e0a07a05ac5b950051cd24c47a88d13d659ba2a46ca1830816d09cd7646f76f716abec5de07fe9b523410806ea6f288f8736c23357c85f45791e1708029d9824d90" +
                    "704607f387a03e49bf9836574431345a7877efaa8a08e73081ef8d62cb780ab6883a50a0d470190dfba10a857f82842d3825b3d6da0573d316eb160dc0b716c48fbd467f75b780" +
                    "149ae8808f4e68f50c0536acddf6f1aeab016b6bc1ec144b4e553acfd670f77e755fc88e0677e31ba459b44e307768958fe3789d41c2b1ff434cb30e15914f01bc6bc2307b488d" +
                    "2556d7b7380ea4ffd712f6b02fe806b94569cd4059f396bf29b99d0a40e5e1711ca944f72d436a102fca4b97693da0b086fe9d2e7162470d02e0f05d4bec9512bfb3f38327296e" +
                    "faa74328b118c27402c70c3a90b49ad4bbc68e37c0aa7d9b3fe17799d73b841e751713a02943905aae0803fd69442eb7681ec2a05600054e92eed555028f21b6a155268a2dd664" +
                    "0a69301a52a38d4d9f9f957ae35af7167118141ce4c9be0a6a492fe79f1581a155fa3a2b9dafd82e650b386ad3a08cb6b83131ac300b0846354a7eef9c410e4b62c47c5426907d" +
                    "fc6685c5c99b7141ac626ab4761fd3f41e728e1a28f89db89ffdeca364dd2f0f0739f0534556483199c71f189341ac9b78a269164206a0ea1ce73bfb2a942e7370b247c046f8e7" +
                    "5ef8e3f8bd821cf577491864e20e6d08fd2e32b555c92c661f19588b72a89599710a88061253ca285b6304b37da2b5294f5cb354a894322848ccbdc7c2545b7da568afac87ffa0" +
                    "05c312241c2d57f4b45d6419f0d2e2c5af33ae243785b325cdab95404fc7aed70525cddb41872cfcc214b13232edc78609753dbff930eb0dc156612b9cb434bc4b693392deb87c" +
                    "530435312edcedc6a961133338d786c4a3e103f60110a16b1337129704bf4754ff6ba9fbe65951e610620f71cda8fc877625f2c5bb04cbe1228b1e886f4050afd8fe94e97d2e9e" +
                    "85c6bb748c0042d3249abb1342bb0eebf62058bf3de080d94611a3750915b5dc6c0b3899d41222bace760ee9c8818ded599e34c56d7372af1eb86852f2a732104bdb750739de6c" +
                    "2c6e0f9eb7cb17f1942bfc9f4fd6ebb6b4cdd4da2bca26fac4578e9f543405acc7d86ff59158bd0cba3aef6f4a8472d144d99f8b8d1dedaa9077d4f01d4bb27bbe31d88fbefac3" +
                    "dcd4797563a26b1d61fcd9a464ab21ed550fe6fa09695ba0b2f10e00000000000000000000000000000000ea6468cc6e20a66f826e3d14c5006f0563887f5e1289be1b2004caca" +
                    "8d3f34d6e84bf59c1e04619a7c23a996941d889e4622a9b9b1d59d5e319094318cd405ba27b7e2c084762d31453ec4549a4d97729d033460fcf89d6494f2ffd789e98082ea5ce9" +
                    "534b3acd60fe49e37e4f666931677319ed89f85588741b3128901a93bd78e4be0225a9e2692c77c969ed0176bdf9555948cbd5a332d045de6ba6bf4490adfe7444cd467a090754" +
                    "17fcc0062e49f008c51ad4227439c1b4476ccd8e97862dab7be1e8d399c05ef27c6e22ee273e15786e394c8f1be31682a30147963ac8da8d41d804258426a3f70289b8ad19d8de" +
                    "13be4eebe3bd4c8a6f55d6e0c373d456851879f5fbc282db9e134806bff71e11bc33ab75dd6ca067fb73a043b646a70339cab4928386786d2f24141ee120fdc34d6764eafc6688" +
                    "0ee0204f53cc1167ed02b43a52dea3ca7cff8ef35cd8e6d7c111a68ef44bcd0c1513ad47ca61c659cc5d0a5b440f6b9f59aff66879bb6688fd2859362b182f207b3175961f6411" +
                    "a493bffd048e7d0d87d82fe6f990a2b0a25f5aa0111a6e68f37bf6f3ac2d26b84686e569038d99c1383597fad81193c4c1b16e6a90e2d507cdfe6fbdaa86163e9cf5de310003ca" +
                    "7e8da047b090db9f37952fbfee76af61668190bd52ed490e677b515d0143840307219c7c0ee7fc7bfc79f325644e4df4c0d7db08e9f0bd024943c705abff899403a605cfbc7ed7" +
                    "46a7d3f7c37d9e8bdc433b7d79e08a12f738a8f0dbddfef2f26502f3e47d1b0fd11e6a13311fb799c79c641d9da43b33e7ad012e28255398789262275f1175be8462c01491c4d8" +
                    "42406d0ec4282c9526174a09878fe8fdde33a29604e5e5e7b2a025d6650b97dbb52befb59b1d30a57433b0a351474444099daa371046613260cf3354cfcdada663ece824ffd7e4" +
                    "4393886a86165ddddf2b4c41773554c86995269408b11e6737a4c447586f69173446d8e48bf84cbc000a807899973eb93c5e819aad669413f8387933ad1584aa35e43f4ecd1e2d" +
                    "0407c0b1b89920ffdfdb9bea51ac95b557af71b89f903f5d9848f14fcbeb1837570f544d6359eb23faf38a0822da36ce426c4a2fbeffeb0a8a2e297a9d19ba15024590e3329d9f" +
                    "a9261f9938a4032dd34606c9cf9f3dd33e576f05cd1dd6811c6298757d77d9e810abdb226afcaa4346a6560f8932b3181fd355d5d391976183f8d99388839632d6354f666d09d3" +
                    "e5629ea19737388613d38a34fd0f6e50ee5a0cc9677177f50028c141378187bd2819403fc534f80076e9380cb4964d3b6b45819d3b8e9caf54f051852d671bf8c1ffde2d151075" +
                    "6418cb4810936aa57e6965d6fb656a760b7f19adf96c173488552193b147ee58858033dac7cd0eb204c06490bbdedf5f7571acb2ebe76acef3f2a01ee987486dfe6c3f0a5e234c" +
                    "127258f97a28fb5d164a8176be946b8097d0e317287f33bf9c16f9a545409ce29b1f4273725fc0df02a04ebae178b3414fb0a82d50deb09fcf4e6ee9d180ff4f56ff3bc1d3601f" +
                    "c2dc90d814c3256f4967d3a8d64c83fea339c51f5a8e5801fbb97835581b602465dee04b5922c2761b54245bec0c9eef2db97d22b2b3556cc969fbb13d06509765a52b3fac54b9" +
                    "3f421bf08e18d52ddd52cc1c8ca8adfaccab7e5cc2f4573fbbf8239bb0b8aedbf8dad16282da5c9125dba1c059d0df8abf621078f02d6c4bc86d40845ac1d59710c45f07d585eb" +
                    "48b32fc0167ba256e73ca3b9311c62d109497957d8dbe10aa3e866b40c0baa2bc492c19ad1e6372d9622bf163fbffeaeee796a3cd9b6fbbfa4d792f34d7fd6e763cd5859dd2683" +
                    "3d21d9bc5452bd19515dff9f4995b35bc0c1f876e6ad11f2452dc9ae85aec01fc56f8cbfda75a7727b75ebbd6bbffb43b63a3b1b671e40feb0db002974a3c3b1a788567231bf63" +
                    "99ff89236981149d423802d2341a3bedb9ddcbac1fe7b6435e1479c72e7089d029e7fbbaf3cf37e9b9a6b776791e4c5e6fda57e8d5f14c8c35a2d270846b9dbe005cda16af4408" +
                    "f3ab06a916eeeb9c9594b70424a4c1d171295b6763b22f47f80b53ccbb904bd68fd65fbd3fbdea1035e98c21a7dbc91a9b5bc7690f05ec317c97f8764eb48e911d428ec8d861b7" +
                    "08e8298acb62155145155ae95f0a1d1501034753146e22d05f586d7f6b4fe12dad9a17f5db70b1db96b8d9a83edadc966c8a5466b61fc998c31f1070d9a5c9a6d268d304fe6b8f" +
                    "d3b4010348611abdcbd49fe4f85b623c7828c71382e1034ea67bc8ae97404b0c50b2a04f559e49950afcb0ef462a2ae024b0f0224dfd73684b88c7fbe92d02b68f759c4752663c" +
                    "d7b97a14943649305521326bde085630864629291bae25ff8822a14c4b666a9259ad0dc42a8290ac7bc7f53a16f379f758e5de750f04fd7cad47701c8597f97888bea6fa0bf299" +
                    "9956fbfd0ee68ec36e4688809ae231eb8bc4369f5fe1573f57e099d9c09901bf39caac48dc11956a8ae905ead86954547c448ae43d315e669c4242da565938f417bf43ce7b2b30" +
                    "b1cd4018388e1a910f0fc41fb0877a5925e466819d375b0a912d4fe843b76ef6f223f0f7c894f38f7ab780dfd75f669c8c06cffa43eb47565a50e3b1fa45ad61ce9a1c4727b7aa" +
                    "a53562f523e73952", "53", 1, SigHash.Single, 0x00014c3b96048441, "4d266d7abfa1ace97df1e6aae5ae63627395f08b6d7b2eec15201a35faebe1d4")]
        public void ShouldSignTestVectorsInline(string rawTransaction, string script, int vin, SigHash hashType, long money, string expected)
        {
            var trx = new SolarCoinTransaction(rawTransaction);
            var hex = trx.ToHex();

            Assert.Equal(rawTransaction, hex);

            var scriptCode = new Script(Encoders.Hex.DecodeData(script));
            var actual = new uint256(trx.GetSignatureHash(scriptCode, vin, hashType, Money.Satoshis(money), HashVersion.Original, null)
                                        .ToBytes()
                                        .Reverse()
                                        .ToArray()).ToString();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(GetTestVectors))]
        public void ShouldSignTestVectors(string rawTransaction, string script, int vin, uint hashType, uint branchId, string expected)
        {
            try
            {
                var trx = new SolarCoinTransaction(rawTransaction);
                var hex = trx.ToHex();

                Assert.Equal(rawTransaction, hex);

                var scriptCode = new Script(Encoders.Hex.DecodeData(script));
                var actual = trx.GetSignatureHash(scriptCode, vin, (SigHash)hashType, Money.Zero, HashVersion.Original, null)
                                .ToString();

                Assert.Equal(expected, actual);
            }
            catch (NotSupportedException)
            {
                Console.WriteLine($"Unsupported tx {rawTransaction.Substring(0, Math.Min(rawTransaction.Length, 32))}...");
            }
        }

        public static IEnumerable<object[]> GetTestVectors()
        {
            var vectors = JsonConvert.DeserializeObject<object[][]>(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "../../../sighash.json")));
            for (int i = 1; i < vectors.Length; i++)
            {
                var rawTransaction = vectors[i][0].ToString();
                var hashType = (long)vectors[i][3];

                if (hashType > 0) // negative SigHashType is not supported by NBitcoin
                {
                    yield return vectors[i];
                }
                else
                {
                    Console.WriteLine($"Unsupported SigHashType {hashType}, {rawTransaction.Substring(0, Math.Min(rawTransaction.Length, 32))}...");
                }
            }
        }
    }
}
