using Xunit;

namespace NBitcoin.SolarCoin.Tests
{
    public class SolarCoinTests
    {
        private Network _solarNetwork = SolarCoinNetworks.Instance.Mainnet;

        static SolarCoinTests()
        {
            SolarCoinNetworks.Instance.EnsureRegistered();
        }

        [Fact]
        public void ShouldParsePrivateKey()
        {
            var address = "8PJcN4qdvzUfRny6L44kPzXRk4dAoJczJE";

            var key = Key.Parse("NcRQ4Rzmo9JeSC35muc3UjEWZwc1sGcxsscKqkc1xqKV58FPUZY3", _solarNetwork);

            Assert.Equal(_solarNetwork.Name, BitcoinAddress.Create(address).Network.Name);
            Assert.Equal(address, key.PubKey.GetAddress(_solarNetwork).ToString());
        }

        [Fact]
        public void ShouldGenerateAndParsePrivateKey()
        {
            var key = new Key();
            var secret = key.GetBitcoinSecret(_solarNetwork).PrivateKey.ToString(_solarNetwork);
            var parsedKey = Key.Parse(secret, _solarNetwork);
            var publickKey = parsedKey.PubKey.ToString();
            var parsedAddress = parsedKey.PubKey.GetAddress(_solarNetwork).ToString();

            Assert.Equal(_solarNetwork.Name, BitcoinAddress.Create(parsedAddress).Network.Name);
            Assert.Equal(parsedKey.PubKey.GetAddress(_solarNetwork).ToString(),
                key.PubKey.GetAddress(_solarNetwork).ToString());
        }
    }
}
