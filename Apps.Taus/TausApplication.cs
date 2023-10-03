using Blackbird.Applications.Sdk.Common;

namespace Apps.Taus;

public class TausApplication : IApplication
{
    public string Name
    {
        get => "TAUS";
        set { }
    }

    public T GetInstance<T>()
    {
        throw new NotImplementedException();
    }
}