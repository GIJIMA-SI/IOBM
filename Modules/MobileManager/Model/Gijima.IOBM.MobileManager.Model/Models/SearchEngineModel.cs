using Gijima.IOBM.Infrastructure.Events;
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
    public class SearchEngineModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public SearchEngineModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Search all or active only clients for the specified search criteria
        /// </summary>
        /// <param name="searchCriteria">The specified search criteria.</param>
        /// <param name="searchEntity">The specific client property to search on.</param>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of Client Entities</returns>
        public ObservableCollection<Client> SearchForClient(string searchCriteria, SearchEntity searchEntity, bool activeOnly = true)
        {
            try
            {
                IEnumerable<Client> clients = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    switch (searchEntity)
                    {
                        case SearchEntity.ClientID:
                            int clientID = Convert.ToInt32(searchCriteria);
                            clients = ((DbQuery<Client>)(from client in db.Clients where client.pkClientID == clientID select client)).ToList();
                            break;
                        case SearchEntity.PrimaryCellNumber:
                            clients = ((DbQuery<Client>)(from client in db.Clients where client.PrimaryCellNumber == searchCriteria select client)).ToList();
                            break;
                        case SearchEntity.EmployeeNumber:
                            clients = ((DbQuery<Client>)(from client in db.Clients where client.EmployeeNumber == searchCriteria select client)).ToList();
                            break;
                        case SearchEntity.IDNumber:
                            clients = ((DbQuery<Client>)(from client in db.Clients where client.IDNumber == searchCriteria select client)).ToList();
                            break;
                        case SearchEntity.Email:
                            clients = ((DbQuery<Client>)(from client in db.Clients where client.Email.Contains(searchCriteria) select client)).ToList();
                            break;
                        default:
                            clients = ((DbQuery<Client>)(from client in db.Clients
                                                         join contract in db.Contracts
                                                         on client.fkContractID equals contract.pkContractID
                                                         from device in db.Devices.Where(p => p.fkContractID == client.fkContractID)
                                                                                  .DefaultIfEmpty()
                                                         join deviceIMENumber in db.DeviceIMENumbers
                                                         on device.pkDeviceID equals deviceIMENumber.fkDeviceID
                                                         from simCard in db.SimCards.Where(p => p.fkContractID == client.fkContractID)
                                                                                    .DefaultIfEmpty()
                                                         where client.ClientName.Contains(searchCriteria) ||
                                                               client.CostCode.Contains(searchCriteria) ||
                                                               contract.AccountNumber.Contains(searchCriteria) ||
                                                               deviceIMENumber.IMENumber.Contains(searchCriteria) ||
                                                               simCard.PUKNumber.Contains(searchCriteria)
                                                         select client)).Distinct().ToList();
                            break;
                    }

                    // If active only and client is in-active return null
                    if (clients != null && activeOnly)
                        clients = clients.Where(p => p.IsActive);

                    return new ObservableCollection<Client>(clients);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Search all or active only clients for the specified search criteria
        /// </summary>
        /// <param name="searchCriteria">The specified search criteria.</param>
        /// <param name="searchEntity">The specific client property to search on.</param>
        /// <param name="companyGroup">The company group the client is linked to.</param>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of Client Entities</returns>
        public ObservableCollection<Client> SearchForClientByCompanyGroup(string searchCriteria, SearchEntity searchEntity, CompanyGroup companyGroup, bool activeOnly = true)
        {
            try
            {
                IEnumerable<Client> clients = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    int[] companiesInGroup = db.Companies.Where(p => p.fkCompanyGroupID == companyGroup.pkCompanyGroupID)
                                                         .Select(p => p.pkCompanyID).ToArray();

                    switch (searchEntity)
                    {
                        case SearchEntity.EmployeeNumber:
                            clients = ((DbQuery<Client>)(from client in db.Clients
                                                         where client.EmployeeNumber == searchCriteria &&
                                                               companiesInGroup.Contains(client.fkCompanyID) 
                                                         select client)).ToList();
                            break;
                        default:
                            clients = ((DbQuery<Client>)(from client in db.Clients
                                                         join device in db.Devices
                                                         on client.fkContractID equals device.fkContractID
                                                         join simCard in db.SimCards
                                                         on client.fkContractID equals simCard.fkContractID
                                                         join contract in db.Contracts
                                                         on client.fkContractID equals contract.pkContractID
                                                         join deviceIMENumber in db.DeviceIMENumbers
                                                         on device.pkDeviceID equals deviceIMENumber.fkDeviceID
                                                         where client.ClientName.Contains(searchCriteria) ||
                                                               client.CostCode.Contains(searchCriteria) ||
                                                               contract.AccountNumber.Contains(searchCriteria) ||
                                                               deviceIMENumber.IMENumber.Contains(searchCriteria) ||
                                                               simCard.PUKNumber.Contains(searchCriteria)
                                                         select client)).Distinct().ToList();
                            break;
                    }

                    // If active only and client is in-active return null
                    if (clients != null && activeOnly)
                        clients = clients.Where(p => p.IsActive);

                    return new ObservableCollection<Client>(clients);
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
