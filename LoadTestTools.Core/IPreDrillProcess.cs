using RestSharp;

namespace LoadTestTools.Core
{
    public interface IPreDrillProcess
    {
        void Execute(DrillOptions drillOptions, RestRequest request);
    }
}