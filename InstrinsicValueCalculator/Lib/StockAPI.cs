using InstrinsicValueCalculator.Models;
using Newtonsoft.Json.Linq;

namespace InstrinsicValueCalculator.Lib;

public class StockApi
{
    private const string ApiKey = "";
    private const string ApiEndpoint = "https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol=";

    public async Task<decimal?> GetIntrinsicValue(string tickerSymbol)
    {
        var client = new HttpClient();
        var response = await client.GetAsync($"{ApiEndpoint}{tickerSymbol}&apikey={ApiKey}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        var data = JObject.Parse(json)["Global Quote"];

        if (data == null)
        {
            return null;
        }

        var currentPrice = decimal.Parse(data["05. price"].ToString());

        // Retrieve EPS and P/E ratio
        var companyOverviewResponse = await client.GetAsync($"https://www.alphavantage.co/query?function=OVERVIEW&symbol={tickerSymbol}&apikey={ApiKey}");
        var companyOverviewJson = await companyOverviewResponse.Content.ReadAsStringAsync();
        var companyOverviewData = JObject.Parse(companyOverviewJson);

        var eps = decimal.Parse(companyOverviewData["EPS"].ToString());
        var peRatio = decimal.Parse(companyOverviewData["PERatio"].ToString());

        // Calculate and return intrinsic value
        var intrinsicValue = eps * (1m + 0.1m) * peRatio;
        return intrinsicValue;
    }

    public async Task<IntrinsicValueAndCurrentPrice?> GetIntrinsicValueAndCurrentPrice(string? tickerSymbol)
    {
        if (tickerSymbol == null)
        {
            Console.WriteLine($"{tickerSymbol} is not a valid ticker symbol");
            return null;
        }

        try
        {
            var client = new HttpClient();
            var response = await client.GetAsync($"{ApiEndpoint}{tickerSymbol}&apikey={ApiKey}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(json)["Global Quote"];

            if (data == null)
            {
                return null;
            }

            if (data is string)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(data);
                Console.ForegroundColor = ConsoleColor.White;
                return null;
            }

            var currentPrice = decimal.Parse(data["05. price"]?.ToString() ?? "0.0");

            // Retrieve EPS and P/E ratio
            var companyOverviewResponse =
                await client.GetAsync(
                    $"https://www.alphavantage.co/query?function=OVERVIEW&symbol={tickerSymbol}&apikey={ApiKey}");
            var companyOverviewJson = await companyOverviewResponse.Content.ReadAsStringAsync();
            var companyOverviewData = JObject.Parse(companyOverviewJson);

            if (string.IsNullOrEmpty(companyOverviewData["EPS"]?.ToString()) ||
                 companyOverviewData["EPS"]?.ToString().ToLower() == "none")
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"EPS was not found for {tickerSymbol}. However it's current price is {currentPrice}");
                Console.ForegroundColor = ConsoleColor.White;
                return null;
            }

            if (string.IsNullOrEmpty(companyOverviewData["PERatio"]?.ToString()) ||
                companyOverviewData["PERatio"]?.ToString().ToLower() == "none")
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"P/E Ratio was not found for {tickerSymbol}. However it's current price is {currentPrice}");
                Console.ForegroundColor = ConsoleColor.White;
                return null;
            }

            var eps = decimal.Parse(companyOverviewData["EPS"]?.ToString() ?? "0.0");
            var peRatio = decimal.Parse(companyOverviewData["PERatio"]?.ToString() ?? "0.0");

            // Calculate and return intrinsic value
            var intrinsicValue = eps * (1m + 0.1m) * peRatio;

            return new IntrinsicValueAndCurrentPrice()
            {
                CurrentPrice = currentPrice,
                IntrinsicValue = intrinsicValue
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"There was an error calculating the Intrinsic value for ticker symbol {tickerSymbol}: {ex.Message} ({ex.InnerException?.Message})");
            return null;
        }
    }
}