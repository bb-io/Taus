using Polly;
using Polly.Retry;
using RestSharp;
using System.Net;

namespace Apps.Taus.Services;

public static class TausPollyPolicies
{
    public static ResiliencePipeline<RestResponse> GetTooManyRequestsRetryPolicy(int retryCount = 6)
    {
        const double baseDelaySeconds = 1.0;
        const double maxDelaySeconds = 45.0;

        var retryOptions = new RetryStrategyOptions<RestResponse>
        {
            MaxRetryAttempts = retryCount,

            ShouldHandle = new PredicateBuilder<RestResponse>()
                .HandleResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
                .Handle<HttpRequestException>(ex => ex.StatusCode == HttpStatusCode.TooManyRequests),

            DelayGenerator = args =>
            {
                var response = args.Outcome.Result;

                var retryAfter = response?.Headers?
                    .FirstOrDefault(h => h.Name?.Equals("Retry-After", StringComparison.OrdinalIgnoreCase) == true)
                    ?.Value?.ToString();

                if (!string.IsNullOrWhiteSpace(retryAfter) &&
                    double.TryParse(retryAfter, out var headerSeconds))
                {
                    return new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(headerSeconds));
                }

                var expSeconds = Math.Min(maxDelaySeconds, baseDelaySeconds * Math.Pow(2, args.AttemptNumber));
                var delaySeconds = Random.Shared.NextDouble() * expSeconds;

                return new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(delaySeconds));
            }
        };

        return new ResiliencePipelineBuilder<RestResponse>()
            .AddRetry(retryOptions)
            .Build();
    }
}
