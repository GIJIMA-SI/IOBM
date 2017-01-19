using Gijima.IOBM.Common.Infrastructure;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class SimmCardModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public SimmCardModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new simm card entity in the database
        /// </summary>
        /// <param name="simmCard">The simm card entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateSimmCard(SimmCard simmCard)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.SimmCards.Any(p => p.PUKNumber == simmCard.PUKNumber))
                    {
                        db.SimmCards.Add(simmCard);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The {0}, {1} simm card already exist.", simmCard.CellNumber, simmCard.PUKNumber));
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
        /// Read all or active only simm cards from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of SimmCard</returns>
        public ObservableCollection<SimmCard> ReadSimmCard(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<SimmCard> simmCards = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    simmCards = ((DbQuery<SimmCard>)(from simmCard in db.SimmCards
                                                     where activeOnly ? simmCard.IsActive : true &&
                                                           excludeDefault ? simmCard.pkSimmCardID > 0 : true
                                                     select simmCard)).ToList();

                    return new ObservableCollection<SimmCard>(simmCards);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Read all or active only simm cards linked to the specified contract from the database
        /// </summary>
        /// <param name="contractID">The contract primary key linked to the simmCards.</param>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of SimmCard</returns>
        public ObservableCollection<SimmCard> ReadSimmCardsForContract(int contractID, bool activeOnly = false)
        {
            try
            {
                IEnumerable<SimmCard> simmCards = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    simmCards = ((DbQuery<SimmCard>)(from simmCard in db.SimmCards
                                                     where simmCard.fkContractID == contractID
                                                     select simmCard)).Include("Devices")
                                                                      .Include("Devices.DeviceMake")
                                                                      .Include("Devices.DeviceModel")
                                                                      .Include("Status").ToList();

                    if (activeOnly)
                        simmCards = simmCards.Where(p => p.IsActive);

                    return new ObservableCollection<SimmCard>(simmCards);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Update an existing simm card entity in the database
        /// </summary>
        /// <param name="simmCard">The simm card entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateSimmCard(SimmCard simmCard)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    SimmCard existingSimmCard = db.SimmCards.Where(p => p.pkSimmCardID == simmCard.pkSimmCardID).FirstOrDefault();

                    // Check to see if the simm card already exist for another entity 
                    if (existingSimmCard != null && existingSimmCard.pkSimmCardID != simmCard.pkSimmCardID && 
                        (existingSimmCard.PUKNumber == simmCard.PUKNumber || existingSimmCard.CardNumber == simmCard.CardNumber ||
                         existingSimmCard.CellNumber == simmCard.CellNumber))
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The {0}, {1} simm card already exist.", simmCard.CellNumber, simmCard.PUKNumber));
                        return false;
                    }
                    else
                    {
                        if (existingSimmCard != null)
                        {
                            existingSimmCard.fkStatusID = simmCard.fkStatusID;
                            existingSimmCard.CellNumber = simmCard.CellNumber;
                            existingSimmCard.CardNumber = simmCard.CardNumber;
                            existingSimmCard.ReceiveDate = simmCard.ReceiveDate;
                            existingSimmCard.PinNumber = simmCard.PinNumber;
                            existingSimmCard.PUKNumber = simmCard.PUKNumber;
                            existingSimmCard.ReceiveDate = simmCard.ReceiveDate;
                            existingSimmCard.ModifiedBy = simmCard.ModifiedBy;
                            existingSimmCard.ModifiedDate = simmCard.ModifiedDate;
                            existingSimmCard.IsActive = simmCard.IsActive;
                            db.SaveChanges();
                            return true;
                        }

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
    }
}
