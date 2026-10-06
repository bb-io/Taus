using Apps.Taus.Models.Estimate;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Taus.Services;

public static class MetricSelectionHelper
{
    public static MetricRequest? Resolve(string? uid, string? version)
    {
        uid = string.IsNullOrWhiteSpace(uid) ? null : uid.Trim();
        version = string.IsNullOrWhiteSpace(version) ? null : version.Trim();

        if (uid is null)
        {
            if (version is not null)
                throw new PluginMisconfigurationException("Metric UID must be provided when Metric version is specified.");

            return null;
        }

        return new MetricRequest { Uid = uid, Version = version };
    }
}
