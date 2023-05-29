using InstrinsicValueCalculator.Lib;

namespace InstrinsicValueCalculator
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            var stockApi = new StockApi();

            while (true)
            {
                try
                {
                    Console.Write("Enter a stock ticker symbol or press 'q' to exit: ");
                    var tickerSymbol = Console.ReadLine();

                    if (tickerSymbol == "q")
                    {
                        break;
                    }

                    var stockData = await stockApi.GetIntrinsicValueAndCurrentPrice(tickerSymbol);

                    if (stockData == null)
                    {
                        throw new Exception($"Failed to retrieve stock with ticket symbol {tickerSymbol}");
                    }

                    var isGoodBuy = stockData.CurrentPrice < stockData.IntrinsicValue;
                    Console.WriteLine($"Intrinsic value of {tickerSymbol}: {stockData.IntrinsicValue:C} with a current price of {stockData.CurrentPrice:C}.");
                    Console.ForegroundColor = isGoodBuy ? ConsoleColor.Green : ConsoleColor.DarkRed;
                    Console.WriteLine(tickerSymbol + " is a " + (stockData.CurrentPrice < stockData.IntrinsicValue ? "good" : "bad") + " buy");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There was an error: {ex.Message} ({ex.InnerException?.Message})");
                }
            }
        }
    }

}