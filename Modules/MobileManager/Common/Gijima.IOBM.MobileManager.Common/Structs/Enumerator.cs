using System;
using System.ComponentModel;

namespace Gijima.IOBM.MobileManager.Common.Structs
{
    /// <summary>
    /// The <see cref="ProcessMenuOption"/> enumeration lists of
    /// application main menu options.
    /// </summary>
    public enum ProcessMenuOption
    {
        [Description("Dashboard")]
        Dashboard,
        [Description("Administration")]
        Administration,
        [Description("Accounts")]
        Accounts,
        [Description("Billing")]
        Billing,
        [Description("Activity Log")]
        ActivityLog,
        [Description("Advanced Search")]
        AdvancedSearch,
        [Description("Reports")]
        Reports,
        [Description("System Tools")]
        SystemTools,
        [Description("Configuration")]
        Configuration,
    }

    /// <summary>
    /// The <see cref="ToolsMenuOption"/> enumeration lists of
    /// application system tools menu options.
    /// </summary>
    public enum ToolsMenuOption
    {
        [Description("Data Update")]
        DataUpdate,
        [Description("Data Import")]
        DataImport,
        [Description("Data Validation")]
        DataValidation,
    }

    /// <summary>
    /// The <see cref="ProcessMenuOption"/> enumeration lists of
    /// application configuration options.
    /// </summary>
    public enum ConfigMenuOption
    {
        [Description("Data Validation")]
        DataValidation,
        [Description("Security")]
        Security,
        [Description("System")]
        System,
        [Description("Reference Data")]
        ReferenceData,
    }

    /// <summary>
    /// The <see cref="SecurityRole"/> enumeration lists of
    /// application security roles.
    /// </summary>
    public enum SecurityRole
    {
        Administrator = 1,
        DataManager = 2,
        SystemUser = 3,
        AccountsUser = 4,
        BillingUser = 5,
        ReadOnly = 6
    }

    /// <summary>
    /// The <see cref="ActionCompleted"/> enumeration lists of
    /// actions that completed.
    /// </summary>
    public enum ActionCompleted
    {
        ReadContractDevices,
        ReadContractSimCards,
        SaveContractDevices,
        SaveContractSimCards
    }

    /// <summary>
    /// The <see cref="Statuses"/> enumeration lists of
    /// option statuses.
    /// </summary>
    public enum Statuses
    {
        CANCELLED = 156,
        UPGRADED = 157,
        LOAN = 158,
        STOLEN = 159,
        TRANSFERED = 160,
        REPLACED = 161,
        ISSUED = 162,
        EQUIPMENT = 164,
        SUSPENDED = 165,
        ACTIVE = 167,
        BER = 168,
        REALLOCATED = 169,
        PREPAID = 171,
        REPAIRED = 1181,
        XLOAN = 1182,
        INACTIVE = 1211,
        AVAILABLE = 1171
    }

    /// <summary>
    /// The <see cref="StatusLink"/> enumeration lists of
    /// option status entites can be linked to.
    /// </summary>
    public enum StatusLink
    {
        All = 0,
        Contract = 1,
        Device = 2,
        Sim = 3,
        ContractDevice = 4,
        ContractSim = 5,
        DeviceSim = 6
    }

    /// <summary>
    /// The <see cref="SearchEntity"/> enumeration lists of entities
    /// the user can search on.
    /// </summary>
    public enum SearchEntity
    {
        ClientID,
        EmployeeNumber,
        PrimaryCellNumber,
        IDNumber,
        Email,
        CompanyName,
        PackageName,
        CellNumber,
        AccountNumber,
        IMENumber,
        Other
    }

    /// <summary>
    /// The <see cref="BillingExecutionState"/> enumeration a list of 
    /// billing processes.
    /// </summary>
    public enum BillingExecutionState
    {
        StartBillingProcess = 1,
        InternalDataValidation = 2,
        ExternalDataImport = 3,
        ExternalDataRuleValidation = 4,
        ExternalDataValidation = 5,
        BillingDataExport = 6,
        BillingResultAudit = 7,
        CloseBillingProcess = 8
    }

    /// <summary>
    /// The <see cref="DataBaseEntity"/> enumeration a list of 
    /// data update column options.
    /// </summary>
    public enum DataBaseEntity
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("Clients")]
        Client = 1,
        [Description("Client Billing")]
        ClientBilling = 2,
        [Description("Client Locations")]
        ClientLocation = 3,
        [Description("Companies")]
        Company = 4,
        [Description("Company Billing Levels")]
        CompanyBillingLevel = 5,
        [Description("Contracts")]
        Contract = 6,
        [Description("Devices")]
        Device = 7,
        [Description("Device Makes")]
        DeviceMake = 8,
        [Description("Device Models")]
        DeviceModel = 9,
        [Description("Packages")]
        Package = 10,
        [Description("Client Package Setup")]
        PackageSetup = 11,
        [Description("Provinces")]
        Province = 12,
        [Description("Service Providers")]
        ServiceProvider = 13,
        [Description("Sim Cards")]
        SimCard = 14,
        [Description("Suburbs")]
        Suburb = 15,
        [Description("Cities")]
        City = 16,
    }
    
    #region Types

    /// <summary>
    /// The <see cref="PackageType"/> enumeration lists of package types.
    /// </summary>
    public enum PackageType
    {
        NONE = 0,
        VOICE = 1,
        DATA = 2
    }

    /// <summary>
    /// The <see cref="BillingLevelType"/> enumeration lists of billing level types.
    /// </summary>
    public enum BillingLevelType
    {
        VOICE = 1,
        DATA = 2
    }

    /// <summary>
    /// The <see cref="CostType"/> enumeration lists of package cost types.
    /// </summary>
    public enum CostType
    {
        NONE = 0,
        STANDARD = 1,
        AMORTIZED = 2
    }

    /// <summary>
    /// The <see cref="ReportType"/> enumeration lists of report types.
    /// </summary>
    public enum ReportType
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("Accounts")]
        Accounts = 1,
        [Description("Usage")]
        Usage = 2,
    }

    /// <summary>
    /// The <see cref="SearchCategory"/> enumeration lists of advanced search types.
    /// </summary>
    public enum SearchCategory
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("Client")]
        Client = 1,
        [Description("Client Address")]
        ClientAddress = 2,
        [Description("Contract")]
        Contract = 3,
        [Description("Billing")]
        Billing = 4,
        [Description("Device")]
        Device = 5,
        [Description("Sim Card")]
        SimCard = 6
    }

    #endregion

    #region Data Activity

    /// <summary>
    /// The <see cref="ActivityProcess"/> enumeration a list of administartion
    /// options that activity logs can be filtered on.
    /// </summary>
    public enum ActivityProcess
    {
        Administration = 1,
        Maintenance,
        Configuartion
    }

    /// <summary>
    /// The <see cref="AdminActivityFilter"/> enumeration a list of administartion
    /// options that activity logs can be filtered on.
    /// </summary>
    public enum AdminActivityFilter
    {
        None,
        Client,
        Contract,
        ClientBilling,
        Device,
        SimCard,
        PackageSetup,
        Manual
    }

    /// <summary>
    /// The <see cref="MaintActivityFilter"/> enumeration a list of maintenance
    /// options that activity logs can be filtered on.
    /// </summary>
    public enum MaintActivityFilter
    {
        None,
        City,
        ClientLocation,
        Company,
        DeviceMake,
        DeviceModel,
        Package,
        Province,
        ServiceProvider,
        Status,
        Suburb,
        CompanyBillingLevel
    }

    /// <summary>
    /// The <see cref="ConfigActivityFilter"/> enumeration a list of configuration
    /// options that activity logs can be filtered on.
    /// </summary>
    public enum ConfigActivityFilter
    {
        None,
    }

    #endregion

    #region Data Validation

    /// <summary>
    /// The <see cref="DataValidationProcess"/> enumeration a list of 
    /// data validation processes on.
    /// </summary>
    public enum DataValidationProcess
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("System Data")]
        System = 1,
        [Description("System Billing Data")]
        SystemBilling = 2,
        [Description("External Billing Data")]
        ExternalBilling = 3
    }

    /// <summary>
    /// The <see cref="DataValidationGroupName"/> enumeration a list of 
    /// data entities to validate on.
    /// </summary>
    public enum DataValidationGroupName
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("Client Data")]
        Client = 1,
        [Description("Company Data")]
        Company = 2,
        [Description("Company Client Data")]
        CompanyClient = 3,
        [Description("Package Data")]
        Package = 4,
        [Description("Package Client Data")]
        PackageClient = 5,
        [Description("Device Data")]
        Device = 6,
        [Description("Simcard Data")]
        SimCard = 7,
        [Description("Status Client Data")]
        StatusClient = 8,
        [Description("External Data")]
        ExternalData = 9,
    }

    /// <summary>
    /// The <see cref="DataValidationPropertyName"/> enumeration a list of 
    /// data properties to validate on.
    /// </summary>
    public enum DataValidationPropertyName
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("Cell Number")]
        PrimaryCellNumber = 1,
        [Description("Client Employee Number")]
        EmployeeNumber = 2,
        [Description("Client Name")]
        ClientName = 3,
        [Description("Client Land Line")]
        LandLine = 4,
        [Description("Client ID Number")]
        IDNumber = 5,
        [Description("Client EMail Address")]
        Email = 6,
        [Description("Cost Code")]
        CostCode = 7,
        [Description("WBS Number")]
        WBSNumber = 8,
        [Description("Admin Fee")]
        AdminFee = 9,
        [Description("IP Address")]
        IPAddress = 10,
        [Description("Client Street Address")]
        AddressLine1 = 11,
        [Description("Client Suburb")]
        fkSuburbID = 12,
        [Description("Is Client Billable")]
        IsBillable = 13,
        [Description("Has Split Billing")]
        IsSplitBilling = 14,
        [Description("Client Voice Allowance")]
        VoiceAllowance = 15,
        [Description("Client SP Limit")]
        SPLimit = 16,
        [Description("Company Name")]
        CompanyName = 17,
        [Description("Company Billing Levels")]
        CompanyBillingLevel = 18,
        [Description("Contract Account Number")]
        AccountNumber = 19,
        [Description("Contract Start Date")]
        ContractStartDate = 20,
        [Description("Contract End Date")]
        ContractEndDate = 21,
        [Description("Contract Upgrade Date")]
        ContractUpgradeDate = 22,
        [Description("Contract Payment Cancel Date")]
        PaymentCancelDate = 23,
        [Description("Device IME Number")]
        IMENumber = 24,
        [Description("Device Serial Number")]
        SerialNumber = 25,
        [Description("Package Name")]
        PackageName = 26,
        [Description("Package Cost")]
        Cost = 27,
        [Description("Package MB Data")]
        MBData = 28,
        [Description("Package Talk Time")]
        TalkTimeMinutes = 29,
        [Description("Package SMS Limit")]
        SMSNumber = 30,
        [Description("Package Rand Value")]
        RandValue = 31,
        [Description("Package SPUL Value")]
        SPULValue = 32,
        [Description("Card Number")]
        CardNumber = 33,
        [Description("PUK Number")]
        PUKNumber = 34
    }

    #endregion

    #region Data Update

    /// <summary>
    /// The <see cref="DataUpdateColumn"/> enumeration a list of 
    /// data update column options.
    /// </summary>
    public enum DataUpdateColumn
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("Employee Number")]
        EmployeeNumber = 1,
        [Description("Cost Code")]
        CostCode = 2,
        [Description("Client Name")]
        ClientName = 3,
        [Description("Client Land Line")]
        LandLine = 4,
        [Description("Client ID Number")]
        IDNumber = 5,
        [Description("Client EMail")]
        Email = 6,
        [Description("WBS Number")]
        WBSNumber = 7,
        [Description("IPAddress")]
        IPAddress = 8,
        [Description("Client Billable")]
        IsBillable = 9,
        [Description("Client Voice Allowance")]
        VoiceAllowance = 10,
        [Description("Supplier Limit")]
        SPLimit = 11,
        [Description("Stop Billing Period")]
        StopBilling = 12,
        [Description("Admin Fee")]
        AdminFee = 13,
        [Description("Company Billing Level")]
        CompanyBillingLevel = 14,
        [Description("Contract Payment Cancel Date")]
        PaymentCancelDate = 15,
        [Description("Package Name")]
        PackageName = 16,
        [Description("Package Cost")]
        Cost = 17,
        [Description("Package MB Data")]
        MBData = 18,
        [Description("Package Talk Time")]
        TalkTimeMinutes = 19,
        [Description("Package Rand Value")]
        RandValue = 20,
        [Description("Package SPUL Value")]
        SPULValue = 21,
        [Description("Sim Number")]
        CardNumber = 22,
        [Description("PUK Number")]
        PUKNumber = 23,
        [Description("Pin Number")]
        PinNumber = 24,
        [Description("Status")]
        fkStatusID = 25
    }

    /// <summary>
    /// The <see cref="DataUpdateColumn"/> enumeration a list of 
    /// data update column options.
    /// </summary>
    public enum DataImportColumn
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("Device Make")]
        fkDeviceMakeID = 1,
        [Description("Device Model")]
        fkDeviceModelID = 2,
        [Description("Status")]
        fkStatusID = 3,
        [Description("Serial Number")]
        SerialNumber = 4,
        [Description("Received Date")]
        ReceiveDate = 5,
        [Description("Insurance Cost")]
        InsuranceCost = 6,
        [Description("Insurance Value")]
        InsuranceValue = 7,
        [Description("IME Number")]
        IMENumber = 8,
        [Description("Cell Number")]
        CellNumber = 9,
        [Description("Card Number")]
        CardNumber = 10,
        [Description("Pin Number")]
        PinNumber = 11,
        [Description("PUK Number")]
        PUKNumber = 12,
        [Description("Company Billing Level")]
        fkCompanyBillingLevelID = 13,
        [Description("Is Billable")]
        IsBillable = 14,
        [Description("Is Split Billing")]
        IsSplitBilling = 15,
        [Description("Split Billing Exception")]
        SplitBillingException = 16,
        [Description("WDP Allowance")]
        WDPAllowance = 17,
        [Description("Voice Allowance")]
        VoiceAllowance = 18,
        [Description("SP Limit")]
        SPLimit = 19,
        [Description("Allowance Limit")]
        AllowanceLimit = 20,
        [Description("International Dailing")]
        InternationalDailing = 21,
        [Description("Permanent Int Dailing")]
        PermanentIntDailing = 22,
        [Description("International Roaming")]
        InternationalRoaming = 23,
        [Description("Permanent Int Roaming")]
        PermanentIntRoaming = 24,
        [Description("Country Visiting")]
        CountryVisiting = 25,
        [Description("Roaming From Date")]
        RoamingFromDate = 26,
        [Description("Roaming To Date")]
        RoamingToDate = 27,
        [Description("Stop Billing From Date")]
        StopBillingFromDate = 28,
        [Description("Stop Billing To Date")]
        StopBillingToDate = 29
    }

    /// <summary>
    /// The <see cref="DataUpdateEntity"/> enumeration a list of 
    /// data import entities options.
    /// </summary>
    public enum DataUpdateEntity
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("Client Billing Data")]
        ClientBilling = 1,
        [Description("Client Contract Data")]
        Contract = 2,
        [Description("Client Detail Data")]
        Client = 3,
        [Description("Sim Data")]
        SimCard = 4,
        [Description("Package Data")]
        Package = 5,
        [Description("Device Data")]
        Device = 6

    }

    /// <summary>
    /// The <see cref="DataImportEntity"/> enumeration a list of 
    /// data import entities options.
    /// </summary>
    public enum DataImportEntity
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("Device Data")]
        Device = 1,
        [Description("Sim Data")]
        SimCard = 2,
        [Description("Client & Contract Data")]
        Client = 3,
        [Description("Billing Data")]
        ClientBilling = 4,
        [Description("Package Data")]
        Package = 5
    }

    /// <summary>
    /// Refrence Data Screen options
    /// </summary>
    public enum ReferenceDataOption
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("Billing Level")]
        ViewBillingLevel = 1,
        [Description("City")]
        ViewCity = 2,
        [Description("Client Site")]
        ViewClientSite = 3,
        [Description("Company")]
        ViewCompany = 4,
        [Description("Company Billing Level")]
        ViewCompanyBillingLevel = 5,
        [Description("Company Group")]
        ViewCompanyGroup = 6,
        [Description("Contract Service")]
        ViewContractService = 7,
        [Description("Device Make")]
        ViewDeviceMake = 8,
        [Description("Device Model")]
        ViewDeviceModel = 9,
        [Description("Package")]
        ViewPackage = 10,
        [Description("Province")]
        ViewProvince = 11,
        [Description("Service Provider")]
        ViewServiceProvider = 12,
        [Description("Status")]
        ViewStatus = 13,
        [Description("Suburb")]
        ViewSuburb = 14
    }

    #endregion

    /// <summary>
    /// The below extension class can be used for all Enum types defined herein.
    /// Please not that you'll have to define each extension method per Enum type
    /// to handle the correct return type.
    /// </summary>
    public static class EnumExtensions
    {
        public static short Value(this ActivityProcess type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this StatusLink type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this PackageType type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this CostType type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this SearchCategory type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this SecurityRole type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DataValidationGroupName type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this BillingExecutionState type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DataUpdateColumn type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DataUpdateEntity type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DataImportEntity type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DataValidationPropertyName type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DataValidationProcess type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DataBaseEntity type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this ReportType type)
        {
            return Convert.ToInt16(type);
        }
        public static int Value(this Statuses type)
        {
            return Convert.ToInt32(type);
        }
        public static short Value(this BillingLevelType type)
        {
            return Convert.ToInt16(type);
        }
    }
}
