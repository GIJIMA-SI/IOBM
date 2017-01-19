using Gijima.IOBM.Infrastructure.Helpers;
using Microsoft.Reporting.WinForms;
using System;
using System.Windows.Controls;

namespace Gijima.Controls.WPF
{
    /// <summary>
    /// Interaction logic for ReportViewerUX.xaml
    /// </summary>
    public partial class ReportViewerUX : UserControl
    {
        #region Properties & Attributes

        private double _reportPortraitWidth = 21.00;
        private double _reportLandscapeWidth = 29.70;

        public enum ReportOrientation
        {
            Portrait,
            Landscape
        }

        #endregion

        #region Event Handlers
        #endregion

        #region Methods

        /// <summary>
        /// Constructer
        /// </summary>
        public ReportViewerUX()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Generate the report in the reportviewer
        /// </summary>
        /// <param name="reportName">The selected report's name.</param>
        /// <param name="reportParameters">The selected report's parameters.</param>
        /// <param name="dataSource">The selected report's datasource.</param>
        /// <param name="reportOrientation">The selected report's orientation.</param>
        /// <param name="reportWidth">OUT the selected report's width</param>
        public void GenerateReport(string reportName, ReportParameter[] reportParameters, ReportDataSource dataSource, ReportOrientation reportOrientation, out double reportWidth)
        {
            reportWidth = 0.00;

            try
            {
                //ReportDataSource reportData = null;
                //ReportParameter[] reportParameters = null;

                // Reset the the ReportViewer
                ReportViewer.Refresh();
                ReportViewer.ProcessingMode = ProcessingMode.Local;

                //// Add the report parameters
                //reportParameters = new ReportParameter[2];
                //reportParameters[0] = new ReportParameter("ReportName", parameters[0].ToString());
                //reportParameters[1] = new ReportParameter("ReportHeader", parameters[1].ToString());

                // Set the report datasource
                //reportData = new ReportDataSource(dataSource.Name, dataSource);
                reportWidth = GetReportWith(_reportPortraitWidth);

                if (dataSource != null)
                {
                    System.Security.PermissionSet security = new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted);
                    ReportViewer.LocalReport.SetBasePermissionsForSandboxAppDomain(security);
                    ReportViewer.LocalReport.DataSources.Add(dataSource);
                    ReportViewer.LocalReport.ReportPath = string.Format(@".\Reports\{0}.rdlc", reportName);
                    ReportViewer.LocalReport.SetParameters(reportParameters);
                    ReportViewer.RefreshReport();
                    ReportViewer.Show();
                }
            }
            catch (Exception ex)
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

        #endregion
    }
}
