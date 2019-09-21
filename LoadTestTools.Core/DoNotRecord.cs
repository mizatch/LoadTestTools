namespace LoadTestTools.Core
{
    public class DoNotRecord: IRecorder
    {
        public void RecordDrill(DrillOptions drillOptions, DrillStats drillStats) {}
        public void RecordHammer(HammerOptions hammerOptions, HammerStats hammerStats) {}
    }
}
