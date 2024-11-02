namespace SmartExchange.Extensions
{
    public static class MathExtensions
    {
        public static decimal Round(decimal amount, decimal stepSize)
        {
            if (stepSize == 0)
            {
                return amount;
            }
            if (amount < stepSize)
            {
                return amount;
            }

            int decimalPlaces = BitConverter.GetBytes(decimal.GetBits(stepSize)[3])[2];
            return Math.Round(Math.Floor(amount / stepSize) * stepSize, decimalPlaces);
        }
    }
}
