using Apps.Taus.Models.Estimate;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.Taus.Services;

public static class MetricSelectionHelper
{
    public static MetricRequest? Resolve(string? uidAndVersion)
    {
        var split = uidAndVersion.Split('-');
        if (split.Length != 2)
        {
            throw new PluginMisconfigurationException("Malformed metric Uid-version pair");
        }
        var uid = split[0];
        var version = split[1];

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
