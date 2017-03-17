using Gijima.IOBM.MobileManager.Common.Structs;
using Prism.Events;

namespace Gijima.IOBM.MobileManager.Common.Events
{
    public class NavigationEvent : PubSubEvent<int> {}
    public class ActionCompletedEvent : PubSubEvent<ActionCompleted> {}
    public class ReadInvoicesEvent : PubSubEvent<int> {}
    public class ReadDevicesEvent : PubSubEvent<int> {}
    public class SaveDeviceEvent : PubSubEvent<int> {}
    public class ReadSimCardsEvent : PubSubEvent<int> {}
    public class SaveSimCardEvent : PubSubEvent<int> {}
    public class LinkDeviceSimCardEvent : PubSubEvent<int> {}
    public class LinkSimCardDeviceEvent : PubSubEvent<int> {}
    public class SearchResultEvent : PubSubEvent<int> {}
    public class SetActivityLogProcessEvent : PubSubEvent<object> {}
    public class ShowInvoiceReportEvent : PubSubEvent<object> {}
    public class MobileManagerSecurityEvent : PubSubEvent<object> {}
    public class BillingProcessEvent : PubSubEvent<BillingExecutionState> {}
    public class BillingProcessHistoryEvent : PubSubEvent<bool> {}
    public class BillingCurrentHistoryEvent : PubSubEvent<object> {}
    public class BillingProcessCompletedEvent : PubSubEvent<BillingExecutionState> {}
    public class BillingPeriodEvent : PubSubEvent<object> { }
    public class DataValiationResultEvent : PubSubEvent<object> {}
    public class DataValiationHeaderEvent : PubSubEvent<bool> { }
    public class DataImportUpdateResultEvent : PubSubEvent<object> {}
}
