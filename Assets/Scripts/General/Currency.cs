namespace General
{
    public class Currency
    {
        public int Shillings;
        public int Jellies;

        public Currency(int _shillings, int _jellies)
        {
            this.Shillings = _shillings;
            this.Jellies = _jellies;
        }
        // make all currency related functions here.
        public void UpdateCurrency()
        {
            
        }
    }
}

public enum CurrencyType
{
    SH,
    JL
}