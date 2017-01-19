using Prism.Events;

namespace Gijima.IOBM.MobileManager.Common
{
    public class ActionCompletedEvent : PubSubEvent<ActionCompleted> { }
    public class ReadDevicesEvent : PubSubEvent<int> {}
    public class ReadSimmCardsEvent : PubSubEvent<int> { }
    public class LinkDeviceSimmCardEvent : PubSubEvent<int> { }
    public class LinkSimmCardDeviceEvent : PubSubEvent<int> { }
    public class SearchResultEvent : PubSubEvent<int> { }
}
