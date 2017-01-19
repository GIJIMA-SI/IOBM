namespace Gijima.IOBM.Infrastructure.Structs
{
    public class ProgressBarInfo
    {
        //public enum ProgressType
        //{
        //    Initialise,
        //    Progress,
        //    Error,
        //    Finalise
        //}
        //public ProgressType BillingProgressType { get; set; }
        public int StepValue { get; set; }
        public int CurrentValue { get; set; }
        public int MaxValue { get; set; }
        //public string ProgressMessage { get; set; }
        //public string ErrorMessage { get; set; }
    }
}
