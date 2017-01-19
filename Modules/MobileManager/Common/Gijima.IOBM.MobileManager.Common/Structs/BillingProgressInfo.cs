namespace Gijima.IOBM.MobileManager.Common.Structs
{
    public class BillingProgressInfo
    {
        public enum InfoType
        {
            Result,
            Error,
            EntityCount
        }
        public InfoType BillingInfoType { get; set; }
        public bool Result { get; set; }
        public string Message { get; set; }
        public int Count { get; set; }
    }
}
