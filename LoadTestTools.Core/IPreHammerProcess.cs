using RestSharp;

namespace LoadTestTools.Core
{
    public interface IPreHammerProcess
    {
        void Execute(HammerOptions hammerOptions, RestRequest request);
    }
}