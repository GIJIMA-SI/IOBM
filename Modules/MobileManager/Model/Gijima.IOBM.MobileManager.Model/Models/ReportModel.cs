using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class ReportModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ReportModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new report entity in the database
        /// </summary>
        /// <param name="report">The report entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateReport(Report report)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.Reports.Any(p => p.enReportType == report.enReportType &&
                                             p.ReportName.ToUpper() == report.ReportName))
                    {
                        db.Reports.Add(report);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("ReportModel",
                                                                        string.Format("A report with the name {0} already exist.", report.ReportName),
                                                                        "CreateReport",
                                                                        ApplicationMessage.MessageTypes.Information));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ReportModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateReport",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Read all or active only cities from the database
        /// </summary>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Cityes</returns>
        public ObservableCollection<Report> ReadReports(ReportType enReportType, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<Report> reports = null;
                short reportTypeID = enReportType.Value();

                using (var db = MobileManagerEntities.GetContext())
                {
                    reports = ((DbQuery<Report>)(from report in db.Reports
                                                 where report.pkReportID == 0 ||
                                                       report.enReportType == reportTypeID
                                                 select report)).ToList();

                    if (excludeDefault)
                        reports = reports.Where(p => p.pkReportID> 0);

                    return new ObservableCollection<Report>(reports);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ReportModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadReports",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Read the company due data for the requested report
        /// </summary>
        /// <param name="CompanyName"></param>
        /// <param name="Period"></param>
        /// <returns></returns>
        public List<sp_Company_Due_Result> ReadyCompanyDueData(string CompanyName, DateTime Period)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    return db.sp_Company_Due(CompanyName, Period).ToList();
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
