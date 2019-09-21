namespace LoadTestTools.Core
{
    public interface IRecorder
    {
        void RecordDrill(DrillOptions drillOptions, DrillStats drillStats);
        void RecordHammer(HammerOptions hammerOptions, HammerStats hammerStats);
    }
}
