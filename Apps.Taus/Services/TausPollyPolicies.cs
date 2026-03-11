using Polly;
using Polly.Retry;
using RestSharp;
using System.Globalization;
using System.Net;

namespace Apps.Taus.Services;

public static class TausPollyPolicies
{
    public static ResiliencePipeline<RestResponse> GetTooManyRequestsRetryPolicy(int retryCount = 10)
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

                var retryAfterValue = response?.Headers?
                    .FirstOrDefault(h => h.Name?.Equals("Retry-After", StringComparison.OrdinalIgnoreCase) == true)
                    ?.Value?.ToString();

                if (!string.IsNullOrWhiteSpace(retryAfterValue))
                {
                    if (int.TryParse(retryAfterValue, out var headerSeconds) && headerSeconds >= 0)
                    {
                        return new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(
                            Math.Min(headerSeconds, maxDelaySeconds)));
                    }

                    if (DateTimeOffset.TryParse(retryAfterValue, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var retryDate))
                    {
                        var delay = retryDate - DateTimeOffset.UtcNow;
                        if (delay > TimeSpan.Zero)
                        {
                            return new ValueTask<TimeSpan?>(delay > TimeSpan.FromSeconds(maxDelaySeconds)
                                ? TimeSpan.FromSeconds(maxDelaySeconds)
                                : delay);
                        }
                    }
                }

                var expSeconds = Math.Min(maxDelaySeconds, baseDelaySeconds * Math.Pow(2, args.AttemptNumber));
                var delaySeconds = Random.Shared.NextDouble() * expSeconds;

                return new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(delaySeconds));
            },

            OnRetry = args =>
            {
                var statusCode = args.Outcome.Result?.StatusCode;
                return default;
            }
        };

        return new ResiliencePipelineBuilder<RestResponse>()
            .AddRetry(retryOptions)
            .Build();
    }
}