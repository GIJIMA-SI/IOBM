using Gijima.IOBM.Common.Infrastructure;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class SuburbModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public SuburbModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new suburb entity in the database
        /// </summary>
        /// <param name="suburb">The suburb entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateSuburb(Suburb suburb)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.Suburbs.Any(p => p.SuburbName.ToUpper() == suburb.SuburbName))
                    {
                        db.Suburbs.Add(suburb);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The {0} suburb already exist.", suburb.SuburbName));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }

        /// <summary>
        /// Read all or active only suburbs from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Suburbes</returns>
        public ObservableCollection<Suburb> ReadSuburbes(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<Suburb> suburbes = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    suburbes = ((DbQuery<Suburb>)(from suburb in db.Suburbs
                                                  where activeOnly ? suburb.IsActive : true &&
                                                        excludeDefault ? suburb.pkSuburbID > 0 : true
                                                  select suburb)).Include("City")
                                                                 .Include("City.Province").OrderBy(p => p.SuburbName).ToList();

                    return new ObservableCollection<Suburb>(suburbes);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Update an existing suburb entity in the database
        /// </summary>
        /// <param name="suburb">The suburb entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateSuburb(Suburb suburb)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    Suburb existingSuburb = db.Suburbs.Where(p => p.SuburbName == suburb.SuburbName).FirstOrDefault();

                    // Check to see if the suburb description already exist for another entity 
                    if (existingSuburb != null && existingSuburb.pkSuburbID != suburb.pkSuburbID)
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The {0} suburb already exist.", suburb.SuburbName));
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingSuburb != null)
                            db.Entry(existingSuburb).State = System.Data.Entity.EntityState.Detached;

                        db.Suburbs.Attach(suburb);
                        db.Entry(suburb).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }
    }
}
