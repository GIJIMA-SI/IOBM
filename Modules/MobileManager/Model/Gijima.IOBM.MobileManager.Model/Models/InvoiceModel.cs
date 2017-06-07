using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class InvoiceModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private AuditLogModel _activityLogger = null;
        private DataActivityHelper _dataActivityHelper = null;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public InvoiceModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _activityLogger = new AuditLogModel(_eventAggregator);
            _dataActivityHelper = new DataActivityHelper(_eventAggregator);
        }

        /// <summary>
        /// Create a new invoice entity in the database
        /// </summary>
        /// <param name="invoice">The invoice entity to add.</param>
        /// <param name="invoiceItems">The invoice items for the specified invoice.</param>
        /// <param name="servicePrefix">The selected service to create a invoice for.</param>
        /// <returns>True if successfull</returns>
        public bool CreateInvoice(Invoice invoice, IEnumerable<InvoiceDetail> invoiceItems, string servicePrefix)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    //if (!db.Invoices.Any(p => p.fkClientID == invoice.fkClientID &&
                    //                          p.fkServiceID == invoice.fkServiceID &&
                    //                          p.InvoiceDate == invoice.InvoiceDate &&
                    //                          p.PrivateDue == invoice.PrivateDue &&
                    //                          p.CompanyDue == invoice.CompanyDue))
                    if (true)
                    {
                        // Create the invoice entity before any invoice items can be created
                        invoice.InvoiceNumber = GenerateInvoiceNumber(servicePrefix);
                        invoice.InvoiceTotal = invoice.PrivateDue + invoice.CompanyDue;
                        invoice.IsPeriodClosed = false;
                        db.Invoices.Add(invoice);
                        db.SaveChanges();

                        // Create the invoice detail
                        CreateInvoiceItems(invoice.pkInvoiceID, invoiceItems, db);

                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("InvoiceModel",
                                                 "The invoice already exist.",
                                                 "CreateInvoice",
                                                 ApplicationMessage.MessageTypes.Information));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("InvoiceModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateInvoice",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Create a new invoice item entity in the database
        /// </summary>
        /// <param name="invoiceID">The invoice ID linked to the invoice items.</param>
        /// <param name="invoiceItems">The list of invoice item entity to add.</param>
        /// <param name="db">The current data context.</param>
        /// <returns>True if successfull</returns>
        private bool CreateInvoiceItems(int invoiceID, IEnumerable<InvoiceDetail> invoiceItems, MobileManagerEntities db)
        {
            try
            {
                foreach (InvoiceDetail item in invoiceItems)
                {
                    //invoiceItem = new InvoiceDetail();
                    item.Invoice = null;
                    item.ServiceProvider = null;
                    item.fkInvoiceID = invoiceID;
                    db.InvoiceDetails.Add(item);
                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("InvoiceModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateInvoiceItems",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Read all invoices from the database
        /// </summary>
        /// <param name="clientID">The client ID to read accounts for.</param>
        /// <param name="accPeriodFilter">The selected account period to filter on..</param>
        /// <param name="serviceFilter">The selected serive to filter on.</param>
        /// <returns>Collection of Invoices</returns>
        public ObservableCollection<Invoice> ReadInvoicesForClient(int clientID, string accPeriodFilter = "None", int serviceFilter = 0, string activeFilter = "Active")
        {
            try
            {
                IEnumerable<Invoice> invoices = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    invoices = ((DbQuery<Invoice>)(from invoice in db.Invoices
                                                   where invoice.fkClientID == clientID
                                                   select invoice)).Include("Client")
                                                                   .Include("Client.Company")
                                                                   .Include("Service1")
                                                                   .OrderByDescending(p => p.InvoicePeriod)
                                                                   .ThenByDescending(p => p.InvoiceNumber).ToList();

                    switch (activeFilter)
                    {
                        case "All":
                            break;
                        case "Active": invoices = invoices.Where(x => x.IsActive).ToList();
                            break;
                        case "Deleted": invoices = invoices.Where(x => !x.IsActive).ToList();
                            break;
                        default: invoices = invoices.Where(x => x.IsActive).ToList();
                            break;
                    }

                    if (accPeriodFilter != "None")
                        invoices = invoices.Where(p => p.InvoicePeriod == accPeriodFilter);

                    if (serviceFilter > 0)
                        invoices = invoices.Where(p => p.fkServiceID == serviceFilter);
                    
                    return new ObservableCollection<Invoice>(invoices);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("InvoiceModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadInvoicesForClient",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Read all invoice item for the specified invoice from the database
        /// </summary>
        /// <param name="invoiceID">The invoice ID linked to the invoice items.</param>
        /// <returns>Collection of Invoice Items</returns>
        public ObservableCollection<InvoiceDetail> ReadInvoiceItems(int invoiceID)
        {
            try
            {
                IEnumerable<InvoiceDetail> invoiceItems = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    invoiceItems = ((DbQuery<InvoiceDetail>)(from items in db.InvoiceDetails
                                                             where items.fkInvoiceID == invoiceID
                                                             select items)).Include("ServiceProvider").ToList();

                    return new ObservableCollection<InvoiceDetail>(invoiceItems);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("InvoiceModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadInvoiceItems",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Read all or active only services from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Packages</returns>
        public ObservableCollection<Service> ReadServices(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<Service> services = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    services = ((DbQuery<Service>)(from service in db.Services
                                                   where activeOnly ? service.IsActive : true &&
                                                         excludeDefault ? service.pkServiceID > 0 : true
                                                   select service)).OrderBy(p => p.ServiceDescription).ToList();

                    return new ObservableCollection<Service>(services);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("InvoiceModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadServices",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Read all billing periods from the database
        /// </summary>
        /// <returns>Collection of billing periods</returns>
        public List<string> ReadBillingPeriods()
        {
            try
            {
                List<string> billingPeriods = new List<string>();

                using (var db = MobileManagerEntities.GetContext())
                {
                    var query = from c in db.Invoices
                                group c by c.InvoicePeriod into g
                                select new { g.Key };

                    foreach (var period in query.OrderByDescending(p => p.Key))
                    {
                        billingPeriods.Add(period.Key);
                    }

                    return billingPeriods;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("InvoiceModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadBillingPeriods",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Read all the invoice numbers for the specified period from the database
        /// </summary>
        /// <param name="billingPeriod">The billing period to load invoic numbers for.</param>
        /// <returns>Collection of Invoice Numbers</returns>
        public List<string> ReadInvoiceNumbers(string billingPeriod)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    return db.Invoices.Where(p => p.InvoicePeriod == billingPeriod)
                                      .OrderBy(p => p.InvoiceNumber)
                                      .Select(p => p.InvoiceNumber).ToList();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("InvoiceModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadInvoiceNumbers",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Read the invoice data for the selected invoice
        /// </summary>
        /// <param name="invoiceID">The primary key for the selected invoice.</param>
        public List<sp_report_Invoice_Result> ReadInvoiceData(int invoiceID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    return db.sp_report_Invoice(invoiceID).ToList();
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Read the invoice data for the selected invoice
        /// </summary>
        /// <param name="invoiceID">The primary key for the selected invoice.</param>
        public Invoice ReadInvoice(string InvoiceNumber)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    return db.Invoices.Where(x => x.InvoiceNumber == InvoiceNumber).FirstOrDefault();
                }
            }
            catch
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                     .Publish(new ApplicationMessage(this.GetType().Name,
                                              string.Format("Error! {0}",
                                              "Error retrieving report!"),
                                              MethodBase.GetCurrentMethod().Name,
                                              ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Update an existing invoice entity in the database
        /// </summary>
        /// <param name="invoice">The invoice entity to update.</param>
        /// <param name="invoiceItems">The invoice items for the specified invoice.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateInvoice(Invoice invoice, IEnumerable<InvoiceDetail> invoiceItems)
        {
            try
            {
                Invoice oldInvoice;
                using (var logDB = MobileManagerEntities.GetContext())
                {
                    oldInvoice = logDB.Invoices.Where(x => x.pkInvoiceID == invoice.pkInvoiceID).FirstOrDefault();
                }

                using (var db = MobileManagerEntities.GetContext())
                {
                    Invoice existingInvoice = db.Invoices.Where(p => p.pkInvoiceID == invoice.pkInvoiceID).FirstOrDefault();

                    if (existingInvoice != null)
                    {
                        existingInvoice.InvoicePeriod = invoice.InvoicePeriod;
                        existingInvoice.InvoiceDate = invoice.InvoiceDate;
                        existingInvoice.PrivateDue = invoice.PrivateDue;
                        existingInvoice.CompanyDue = invoice.CompanyDue;
                        existingInvoice.InvoiceTotal = invoice.PrivateDue + invoice.CompanyDue;
                        existingInvoice.ModifiedBy = invoice.ModifiedBy;
                        existingInvoice.ModifiedDate = invoice.ModifiedDate;
                        existingInvoice.DeleteComment = invoice.DeleteComment;
                        existingInvoice.IsActive = invoice.IsActive;

                        _activityLogger.CreateDataChangeAudits<SimCard>(_dataActivityHelper.GetDataChangeActivities<Invoice>(oldInvoice, invoice, invoice.pkInvoiceID, db));

                        db.SaveChanges();

                        // Update the invoice detail
                        UpdateInvoiceItems(invoice.pkInvoiceID, invoiceItems, db);

                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("InvoiceModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateInvoice",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Update an existing invoice item entity in the database
        /// </summary>
        /// <param name="invoiceID">The invoice ID linked to the invoice items.</param>
        /// <param name="invoiceItems">The list of invoice item entity to update.</param>
        /// <param name="db">The current data context.</param>
        /// <returns>True if successfull</returns>
        private void UpdateInvoiceItems(int invoiceID, IEnumerable<InvoiceDetail> invoiceItems, MobileManagerEntities db)
        {
            try
            {
                IEnumerable<InvoiceDetail> existingItems = db.InvoiceDetails.Where(p => p.fkInvoiceID == invoiceID).ToList();

                foreach (InvoiceDetail item in existingItems)
                {
                    InvoiceDetail itemToUpdate = invoiceItems.Where(p => p.pkInvoiceItemID == item.pkInvoiceItemID).FirstOrDefault();

                    if (itemToUpdate != null)
                    {
                        item.fkServiceProviderID = itemToUpdate.fkServiceProviderID;
                        item.ItemDescription = itemToUpdate.ItemDescription;
                        item.ReferenceNumber = itemToUpdate.ReferenceNumber;
                        item.ItemAmount = itemToUpdate.ItemAmount;
                        item.IsPrivate = itemToUpdate.IsPrivate;
                        item.ModifiedBy = itemToUpdate.ModifiedBy;
                        item.ModifiedDate = itemToUpdate.ModifiedDate;
                    }
                    else
                    {
                        db.InvoiceDetails.Remove(item);
                    }
                }
                
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("InvoiceModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateInvoiceItems",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Generate the next invoice number for the selected service
        /// </summary>
        /// <param name="servicePreFix">The selected service prefix.</param>
        /// <returns>Invoice number</returns>
        private string GenerateInvoiceNumber(string servicePreFix)
        {
            try
            {
                string invoiceNo = string.Empty;

                using (var db = MobileManagerEntities.GetContext())
                {
                    var latestInvoice = ((DbQuery<string>)(from invoice in db.Invoices
                                                           where invoice.InvoiceNumber.Substring(0, 3) == servicePreFix
                                                           select invoice.InvoiceNumber.Substring(3))).Max();

                    return string.Format("{0}{1}", servicePreFix, (Convert.ToInt32(latestInvoice) + 1).ToString().PadLeft(9, '0'));
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("InvoiceModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "GenerateInvoiceNumber",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }
    }
}
