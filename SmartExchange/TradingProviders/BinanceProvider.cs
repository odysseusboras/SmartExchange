using SmartExchange.Model.Configuration;
using SmartExchange.TradingProviders.Model;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SmartExchange.TradingProviders
{
    internal class BinanceProvider : ITradingProvider
    {
        private readonly AppSettings _settings;
        private readonly HttpClient _httpClient;

        public BinanceProvider(AppSettings settings)
        {
            _settings = settings;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(settings.TradingProvider?.BaseUrl ?? throw new Exception("Binance URL is not defined"))
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<AccountInfo> GetAccountInfo(List<ConfigPair>? configPairs = null)
        {
            long timestamp = GetTimestamp();

            string query = $"timestamp={timestamp}";

            string signature = GenerateSignature(query, _settings.TradingProvider?.Secret ?? "");

            query += $"&signature={signature}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", _settings.TradingProvider?.ApiKey);

            HttpResponseMessage response = await _httpClient.GetAsync($"/api/v3/account?{query}");

            if (!response.IsSuccessStatusCode)
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error {response.StatusCode}. Response: {errorResponse}");
            }

            _ = response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            Dictionary<string, object> accountInfoDict = JsonSerializer.Deserialize<Dictionary<string, object>>(content) ?? throw new Exception("Account info response is empty");

            JsonElement.ArrayEnumerator balances = ((JsonElement)accountInfoDict["balances"]).EnumerateArray();

            List<Asset> res = balances
                    .Select(x =>
                    {
                        _ = decimal.TryParse(x.GetProperty("free").GetString(), out decimal amount);
                        return new Asset()
                        {
                            Name = x.GetProperty("asset").GetString() ?? throw new Exception("Asset is empty"),
                            Quantity = amount
                        };
                    })
                     .ToList();

            res = res
                .Where(x => configPairs is null || (configPairs.Exists(c => c.Name.Contains(x.Name)) && x.Quantity > 0))
                .ToList();

            return new AccountInfo
            {
                Assets = res
            };
        }

        public async Task<IEnumerable<PairInfo>> GetAllPairs(List<ConfigPair>? configPairs = null)
        {
            HttpResponseMessage response = await _httpClient.GetAsync("/api/v3/exchangeInfo");
            _ = response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(content);
            JsonElement symbolsElement = doc.RootElement.GetProperty("symbols");

            List<PairInfo> pairs = [];
            foreach (JsonElement symbolElement in symbolsElement.EnumerateArray())
            {
                string pairName = symbolElement.GetProperty("symbol").GetString() ?? throw new Exception("Symbol is empty");
                string baseAsset = symbolElement.GetProperty("baseAsset").GetString() ?? throw new Exception("Asset is empty");
                if (configPairs is not null && !configPairs.Any(c => string.Equals(c.Name, pairName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                JsonElement filters = symbolElement.GetProperty("filters");
                decimal minQuantity = 0;
                decimal maxQuantity = 0;
                decimal stepSize = 0;

                foreach (JsonElement filterElement in filters.EnumerateArray())
                {
                    string filterType = filterElement.GetProperty("filterType").GetString() ?? throw new Exception("LOT SIZE is empty");
                    if (filterType == "LOT_SIZE")
                    {
                        _ = decimal.TryParse(filterElement.GetProperty("minQty").GetString(), out minQuantity);
                        _ = decimal.TryParse(filterElement.GetProperty("maxQty").GetString(), out maxQuantity);
                        _ = decimal.TryParse(filterElement.GetProperty("stepSize").GetString(), out stepSize);
                        break;
                    }
                }

                pairs.Add(new PairInfo
                {
                    PairName = pairName,
                    FromCoinName = baseAsset,
                    ToCoinName = symbolElement.GetProperty("quoteAsset").GetString() ?? throw new Exception("Quote asset is empty"),
                    MinQuantity = minQuantity,
                    MaxQuantity = maxQuantity,
                    StepSize = stepSize
                });
            }

            return pairs;
        }

        public async Task<decimal> GetPairPriceAsync(string pairName)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/v3/ticker/price?symbol={pairName}");

            _ = response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            Dictionary<string, object>? priceInfoDict = JsonSerializer.Deserialize<Dictionary<string, object>>(content);

            if (priceInfoDict != null && priceInfoDict.TryGetValue("price", out object? priceValue) && priceValue is JsonElement priceElement)
            {
                _ = decimal.TryParse(priceElement.GetString(), out decimal res);
                return res;
            }

            throw new Exception("Price information not found in response.");
        }


        public async Task SellCoin(string pairName, decimal amount)
        {
            List<KeyValuePair<string, string>> parameters =
            [
                new KeyValuePair<string, string>("symbol", pairName),
                new KeyValuePair<string, string>("side", "SELL"),
                new KeyValuePair<string, string>("type", "MARKET"),
                new KeyValuePair<string, string>("quantity", amount.ToString()),
                new KeyValuePair<string, string>("timestamp", GetTimestamp().ToString())
            ];

            string queryString = string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"));
            string signature = GenerateSignature(queryString, _settings.TradingProvider?.Secret ?? "");
            parameters.Add(new KeyValuePair<string, string>("signature", signature));
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", _settings.TradingProvider?.ApiKey);
            HttpResponseMessage response = await _httpClient.PostAsync("/api/v3/order", new FormUrlEncodedContent(parameters));

            if (!response.IsSuccessStatusCode)
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error placing order: {response.StatusCode}. Response: {errorResponse}");
            }

            _ = response.EnsureSuccessStatusCode();
        }


        public async Task BuyCoin(string pairName, decimal amount)
        {
            List<KeyValuePair<string, string>> parameters =
            [
                new KeyValuePair<string, string>("symbol", pairName),
                new KeyValuePair<string, string>("side", "BUY"),
                new KeyValuePair<string, string>("type", "MARKET"),
                new KeyValuePair<string, string>("quantity", amount.ToString()),
                new KeyValuePair<string, string>("timestamp", GetTimestamp().ToString())
            ];

            string queryString = string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"));
            string signature = GenerateSignature(queryString, _settings.TradingProvider?.Secret ?? "");
            parameters.Add(new KeyValuePair<string, string>("signature", signature));
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", _settings.TradingProvider?.ApiKey);
            HttpResponseMessage response = await _httpClient.PostAsync("/api/v3/order", new FormUrlEncodedContent(parameters));

            if (!response.IsSuccessStatusCode)
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error placing order: {response.StatusCode}. Response: {errorResponse}");
            }

            _ = response.EnsureSuccessStatusCode();
        }


        private string GenerateSignature(string query, string secret)
        {
            using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(secret));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(query));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private long GetTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
