namespace Gijima.IOBM.Infrastructure.Structs
{
    public class ApplicationInfo
    {
        public enum InfoSource
        {
            UserInfo,
            ConnectionInfo,
            ApplicationVersion
        }
        public InfoSource ApplicationInfoSource { get; set; }
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string PublisedApplicationVersion { get; set; }
    }
}
