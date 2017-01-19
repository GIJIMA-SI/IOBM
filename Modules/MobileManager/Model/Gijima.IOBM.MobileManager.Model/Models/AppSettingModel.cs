using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class AppSettingModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public AppSettingModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Read the app settings from the database
        /// </summary>
        /// <returns>Collection of AppSettings</returns>
        public AppSetting ReadAppSettings()
        {
            try
            {
                AppSetting appSetting = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    appSetting = ((DbQuery<AppSetting>)(from setting in db.AppSettings
                                                        select setting)).FirstOrDefault();

                    return appSetting;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }
    }
}
