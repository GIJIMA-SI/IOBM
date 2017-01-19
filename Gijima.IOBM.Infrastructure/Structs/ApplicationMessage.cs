namespace Gijima.IOBM.Infrastructure.Structs
{
    public class ApplicationMessage
    {
        public enum MessageTypes
        {
            Information,
            Question,
            ProcessError,
            SystemError
        }

        public string Owner { get; set; }
        public object Message { get; set; }
        public string Header { get; set; }
        public MessageTypes MessageType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="owner">Window sending the message.</param>
        /// <param name="message">The message description.</param>
        /// <param name="header">The message box header.</param>
        /// <param name="messageType">The message type.</param>
        public ApplicationMessage(string owner, string message, string header, MessageTypes messageType)
        {
            Owner = owner;
            Message = message;
            Header = header;
            MessageType = messageType;
        }
    }
}
