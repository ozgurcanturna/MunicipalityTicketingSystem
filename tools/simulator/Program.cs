using System.Net.Http.Json;

namespace MunicipalityTicketing.Simulator;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Municipality Ticketing Simulator Started");
        Console.WriteLine("Press Ctrl+C to stop...\n");

        var httpClient = new HttpClient();
        var baseUrl = "http://localhost:5000";

        try
        {
            // Test Identity API
            Console.WriteLine("Testing Identity API...");
            var identityResponse = await httpClient.GetAsync($"{baseUrl}/api/identity/users");
            if (identityResponse.IsSuccessStatusCode)
            {
                var users = await identityResponse.Content.ReadFromJsonAsync<List<string>>();
                Console.WriteLine($"✓ Identity API OK - Users: {string.Join(", ", users ?? new List<string>())}");
            }
            else
            {
                Console.WriteLine($"✗ Identity API Failed - Status: {identityResponse.StatusCode}");
            }

            // Test Wallet API
            Console.WriteLine("\nTesting Wallet API...");
            var walletResponse = await httpClient.GetAsync($"{baseUrl}/api/wallet/wallets");
            if (walletResponse.IsSuccessStatusCode)
            {
                var wallets = await walletResponse.Content.ReadFromJsonAsync<List<string>>();
                Console.WriteLine($"✓ Wallet API OK - Wallets: {string.Join(", ", wallets ?? new List<string>())}");
            }
            else
            {
                Console.WriteLine($"✗ Wallet API Failed - Status: {walletResponse.StatusCode}");
            }

            // Test Telemetry API
            Console.WriteLine("\nTesting Telemetry API...");
            var telemetryResponse = await httpClient.GetAsync($"{baseUrl}/api/telemetry/trips");
            if (telemetryResponse.IsSuccessStatusCode)
            {
                var trips = await telemetryResponse.Content.ReadFromJsonAsync<List<string>>();
                Console.WriteLine($"✓ Telemetry API OK - Trips: {string.Join(", ", trips ?? new List<string>())}");
            }
            else
            {
                Console.WriteLine($"✗ Telemetry API Failed - Status: {telemetryResponse.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during testing: {ex.Message}");
        }

        Console.WriteLine("\nSimulation completed.");
    }
}
