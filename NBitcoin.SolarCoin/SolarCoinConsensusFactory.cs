namespace NBitcoin.SolarCoin
{
    public class SolarCoinConsensusFactory : ConsensusFactory
    {
        private SolarCoinConsensusFactory()
        {
        }

        public static SolarCoinConsensusFactory Instance { get; } = new SolarCoinConsensusFactory();

        public override Transaction CreateTransaction()
        {
            return new SolarCoinTransaction();
        }
    }
}
