namespace SftpScheduler.BLL.Models
{
    public class QuartzTriggerEntity
    {
        public string TriggerName { get; set; } = String.Empty;

        /// <summary>
        /// Next fire time exactly as Quartz stores it in QRTZ_TRIGGERS.NEXT_FIRE_TIME: UTC .NET ticks
        /// (DateTime.Ticks). Null when the trigger will not fire again.
        /// </summary>
        public long? NextFireTimeTicks { get; set; }
    }
}
