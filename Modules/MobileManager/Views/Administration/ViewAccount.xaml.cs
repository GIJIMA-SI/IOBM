using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.MobileManager.Common.Events;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Microsoft.Reporting.WinForms;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Gijima.IOBM.MobileManager.Views
{
    /// <summary>
    /// Interaction logic for ViewAccount.xaml
    /// </summary>
    public partial class ViewAccount : UserControl
    {
        private IEventAggregator _eventAggregator = null;

        public ViewAccount(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            InitializeComponent();

            // Subscribe to this event to show the invoice linked to selectede invoice
            _eventAggregator.GetEvent<ShowInvoiceReportEvent>().Subscribe(ShowInvoiceReport_Event, true);
        }

        private async void ShowInvoiceReport_Event(object sender)
        {
            InvoiceReportEventArgs eventArgs = sender as InvoiceReportEventArgs;
            await ShowInvoiceReportAsync(eventArgs.InvoiceID, eventArgs.ServiceDescription);
        }

        public async Task ShowInvoiceReportAsync(int invoiceID, string serviceDescription)
        {
            try
            {
                ReportViewer.Reset();

                string reportPath = ConfigurationManager.AppSettings["ReportPath"].ToString();

                // Read the invoice data for the selected invoice
                List<sp_report_Invoice_Result> invoiceData = await Task.Run(() => new InvoiceModel(null).ReadInvoiceData(invoiceID));
                ReportDataSource reportData = new ReportDataSource("dsInvoice", invoiceData);

                // Add the report parameters
                ReportParameter[] reportParameters = new ReportParameter[1];
                reportParameters[0] = new ReportParameter("ReportHeader", string.Format("INVOICE - {0}", serviceDescription));

                if (reportData != null)
                {
                    System.Security.PermissionSet security = new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted);
                    ReportViewer.Width = GetReportWith(21.00); 
                    ReportViewer.ProcessingMode = ProcessingMode.Local;
                    ReportViewer.LocalReport.SetBasePermissionsForSandboxAppDomain(security);
                    ReportViewer.LocalReport.DataSources.Add(reportData);
                    ReportViewer.LocalReport.ReportPath = string.Format("{0}{1}", reportPath, "Invoice.rdlc");
                    ReportViewer.LocalReport.SetParameters(reportParameters);
                    ReportViewer.RefreshReport();
                    ReportViewer.Show();
                }
            }
            catch (Exception  ex)
            {

            }
        }

        /// <summary>
        /// Calculate the report width by converting the specified
        /// report page with from centimeters to pixels
        /// </summary>
        /// <param name="reportWidth">The reports page width</param>
        /// <returns>Pixel width as int</returns>
        private int GetReportWith(double reportPageWidth)
        {
            UnitConvertionHelper.Unit pixel = new UnitConvertionHelper.Unit(reportPageWidth - 2, UnitConvertionHelper.UnitTypes.Cm);
            pixel.Type = UnitConvertionHelper.UnitTypes.Px;
            return ((int)pixel.Value) + 2;
        }
    }
}
