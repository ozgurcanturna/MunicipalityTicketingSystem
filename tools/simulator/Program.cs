using System.Diagnostics;
using System.Net;

var baseUrl = Environment.GetEnvironmentVariable("SIMULATOR_BASE_URL") ?? "http://localhost:5197";
var tenantId = Environment.GetEnvironmentVariable("SIMULATOR_TENANT_ID") ?? "ankara";
var durationSeconds = ParseInt("SIMULATOR_DURATION_SECONDS", 30);
var concurrency = ParseInt("SIMULATOR_CONCURRENCY", 8);
var delayMs = ParseInt("SIMULATOR_DELAY_MS", 100);

Console.WriteLine($"Simulator started. BaseUrl={baseUrl}, Tenant={tenantId}, Concurrency={concurrency}, Duration={durationSeconds}s");

using var httpClient = new HttpClient
{
	BaseAddress = new Uri(baseUrl),
	Timeout = TimeSpan.FromSeconds(5)
};

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(durationSeconds));
var cancellationToken = cts.Token;

var successCount = 0L;
var failureCount = 0L;
var elapsed = Stopwatch.StartNew();

var tasks = Enumerable.Range(0, concurrency)
	.Select(workerIndex => Task.Run(async () =>
	{
		var busCode = $"BUS-{workerIndex + 1:D3}";

		while (!cancellationToken.IsCancellationRequested)
		{
			var ok = await ExecuteScenarioAsync(httpClient, tenantId, busCode, cancellationToken);

			if (ok)
			{
				Interlocked.Increment(ref successCount);
			}
			else
			{
				Interlocked.Increment(ref failureCount);
			}

			try
			{
				await Task.Delay(delayMs, cancellationToken);
			}
			catch (OperationCanceledException)
			{
				break;
			}
		}
	}, cancellationToken))
	.ToArray();

await Task.WhenAll(tasks);
elapsed.Stop();

var total = successCount + failureCount;
var rps = elapsed.Elapsed.TotalSeconds <= 0 ? 0 : total / elapsed.Elapsed.TotalSeconds;

Console.WriteLine("Simulation finished.");
Console.WriteLine($"Total Requests : {total}");
Console.WriteLine($"Success        : {successCount}");
Console.WriteLine($"Failure        : {failureCount}");
Console.WriteLine($"Approx RPS     : {rps:F2}");

static async Task<bool> ExecuteScenarioAsync(HttpClient httpClient, string tenantId, string busCode, CancellationToken cancellationToken)
{
	try
	{
		using var healthRequest = new HttpRequestMessage(HttpMethod.Get, "/health");
		var healthResponse = await httpClient.SendAsync(healthRequest, cancellationToken);

		if (!healthResponse.IsSuccessStatusCode)
		{
			return false;
		}

		using var telemetryRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/telemetry/journeys/active/{busCode}");
		telemetryRequest.Headers.Add("X-Tenant-Id", tenantId);

		var telemetryResponse = await httpClient.SendAsync(telemetryRequest, cancellationToken);

		return telemetryResponse.IsSuccessStatusCode || telemetryResponse.StatusCode == HttpStatusCode.NotFound;
	}
	catch
	{
		return false;
	}
}

static int ParseInt(string key, int fallback)
{
	var value = Environment.GetEnvironmentVariable(key);
	return int.TryParse(value, out var parsed) ? parsed : fallback;
}
