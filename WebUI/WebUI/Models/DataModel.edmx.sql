
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 08/08/2016 13:45:19
-- Generated from EDMX file: D:\Offical project\batdog\WebUI\WebUI\Models\DataModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [Rooster];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_EmployeeRole_Employee]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EmployeeRole] DROP CONSTRAINT [FK_EmployeeRole_Employee];
GO
IF OBJECT_ID(N'[dbo].[FK_EmployeeRole_Role]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EmployeeRole] DROP CONSTRAINT [FK_EmployeeRole_Role];
GO
IF OBJECT_ID(N'[dbo].[FK_MembershipUserProfile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[security_Membership] DROP CONSTRAINT [FK_MembershipUserProfile];
GO
IF OBJECT_ID(N'[dbo].[FK_security_UsersInRole_UserProfile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[security_UsersInRoles] DROP CONSTRAINT [FK_security_UsersInRole_UserProfile];
GO
IF OBJECT_ID(N'[dbo].[FK_security_UsersInRole_security_Role]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[security_UsersInRoles] DROP CONSTRAINT [FK_security_UsersInRole_security_Role];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfileEmployee]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Employees] DROP CONSTRAINT [FK_UserProfileEmployee];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfileEmployerRequest]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EmployerRequests] DROP CONSTRAINT [FK_UserProfileEmployerRequest];
GO
IF OBJECT_ID(N'[dbo].[FK_EmployeeEmployerRequest]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EmployerRequests] DROP CONSTRAINT [FK_EmployeeEmployerRequest];
GO
IF OBJECT_ID(N'[dbo].[FK_ManagerEmployerRequest]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EmployerRequests] DROP CONSTRAINT [FK_ManagerEmployerRequest];
GO
IF OBJECT_ID(N'[dbo].[FK_BusinessLocationInternalLocation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[InternalLocations] DROP CONSTRAINT [FK_BusinessLocationInternalLocation];
GO
IF OBJECT_ID(N'[dbo].[FK_BusinessLocationShiftTemplate]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ShiftTemplates] DROP CONSTRAINT [FK_BusinessLocationShiftTemplate];
GO
IF OBJECT_ID(N'[dbo].[FK_ShiftTemplateRole]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ShiftTemplates] DROP CONSTRAINT [FK_ShiftTemplateRole];
GO
IF OBJECT_ID(N'[dbo].[FK_ShiftTemplateInternalLocation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ShiftTemplates] DROP CONSTRAINT [FK_ShiftTemplateInternalLocation];
GO
IF OBJECT_ID(N'[dbo].[FK_ShiftTemplateEmployee]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ShiftTemplates] DROP CONSTRAINT [FK_ShiftTemplateEmployee];
GO
IF OBJECT_ID(N'[dbo].[FK_RosterShift]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Shifts] DROP CONSTRAINT [FK_RosterShift];
GO
IF OBJECT_ID(N'[dbo].[FK_EmployeeShift]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Shifts] DROP CONSTRAINT [FK_EmployeeShift];
GO
IF OBJECT_ID(N'[dbo].[FK_RoleShift]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Shifts] DROP CONSTRAINT [FK_RoleShift];
GO
IF OBJECT_ID(N'[dbo].[FK_InternalLocationShift]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Shifts] DROP CONSTRAINT [FK_InternalLocationShift];
GO
IF OBJECT_ID(N'[dbo].[FK_BusinessBusinessPreferences]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BusinessPreferences] DROP CONSTRAINT [FK_BusinessBusinessPreferences];
GO
IF OBJECT_ID(N'[dbo].[FK_ShiftShiftChangeRequest]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ShiftChangeRequests] DROP CONSTRAINT [FK_ShiftShiftChangeRequest];
GO
IF OBJECT_ID(N'[dbo].[FK_ShiftChangeRequestEmployee]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ShiftChangeRequests] DROP CONSTRAINT [FK_ShiftChangeRequestEmployee];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfileRecurringCalendarEvent]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Schedules] DROP CONSTRAINT [FK_UserProfileRecurringCalendarEvent];
GO
IF OBJECT_ID(N'[dbo].[FK_EmployeeEmployeeRequest]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EmployeeRequests] DROP CONSTRAINT [FK_EmployeeEmployeeRequest];
GO
IF OBJECT_ID(N'[dbo].[FK_EmployeeRequestUserProfile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EmployeeRequests] DROP CONSTRAINT [FK_EmployeeRequestUserProfile];
GO
IF OBJECT_ID(N'[dbo].[FK_ShiftBlockRole]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ShiftBlocks] DROP CONSTRAINT [FK_ShiftBlockRole];
GO
IF OBJECT_ID(N'[dbo].[FK_BusinessShiftBlock]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ShiftBlocks] DROP CONSTRAINT [FK_BusinessShiftBlock];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfileUserPreferences]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserPreferences] DROP CONSTRAINT [FK_UserProfileUserPreferences];
GO
IF OBJECT_ID(N'[dbo].[FK_BusinessBusinessLocation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BusinessLocations] DROP CONSTRAINT [FK_BusinessBusinessLocation];
GO
IF OBJECT_ID(N'[dbo].[FK_BusinessTypeBusiness]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Businesses] DROP CONSTRAINT [FK_BusinessTypeBusiness];
GO
IF OBJECT_ID(N'[dbo].[FK_BusinessRole]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Roles] DROP CONSTRAINT [FK_BusinessRole];
GO
IF OBJECT_ID(N'[dbo].[FK_BusinessLocationEmployerRequest]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EmployerRequests] DROP CONSTRAINT [FK_BusinessLocationEmployerRequest];
GO
IF OBJECT_ID(N'[dbo].[FK_BusinessLocationEmployee]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Employees] DROP CONSTRAINT [FK_BusinessLocationEmployee];
GO
IF OBJECT_ID(N'[dbo].[FK_BusinessLocationEmployeeRequest]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[EmployeeRequests] DROP CONSTRAINT [FK_BusinessLocationEmployeeRequest];
GO
IF OBJECT_ID(N'[dbo].[FK_ManagerBusinessLocation_Manager]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ManagerBusinessLocations] DROP CONSTRAINT [FK_ManagerBusinessLocation_Manager];
GO
IF OBJECT_ID(N'[dbo].[FK_ManagerBusinessLocation_BusinessLocation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ManagerBusinessLocations] DROP CONSTRAINT [FK_ManagerBusinessLocation_BusinessLocation];
GO
IF OBJECT_ID(N'[dbo].[FK_BusinessLocationRoster]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Rosters] DROP CONSTRAINT [FK_BusinessLocationRoster];
GO
IF OBJECT_ID(N'[dbo].[FK_ShiftTemplateRecurringShiftChangeRequest]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RecurringShiftChangeRequests] DROP CONSTRAINT [FK_ShiftTemplateRecurringShiftChangeRequest];
GO
IF OBJECT_ID(N'[dbo].[FK_EmployeeRecurringShiftChangeRequest]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[RecurringShiftChangeRequests] DROP CONSTRAINT [FK_EmployeeRecurringShiftChangeRequest];
GO
IF OBJECT_ID(N'[dbo].[FK_ShiftChangeRequestEmployee1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ShiftChangeRequests] DROP CONSTRAINT [FK_ShiftChangeRequestEmployee1];
GO
IF OBJECT_ID(N'[dbo].[FK_ShiftShiftTemplate]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Shifts] DROP CONSTRAINT [FK_ShiftShiftTemplate];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfilewebpages_OAuthMembership]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[security_OAuthMembership] DROP CONSTRAINT [FK_UserProfilewebpages_OAuthMembership];
GO
IF OBJECT_ID(N'[dbo].[FK_EmployeePaymentDetails]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PaymentDetails] DROP CONSTRAINT [FK_EmployeePaymentDetails];
GO
IF OBJECT_ID(N'[dbo].[FK_BusinessLocationPaymentDetails]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PaymentDetails] DROP CONSTRAINT [FK_BusinessLocationPaymentDetails];
GO
IF OBJECT_ID(N'[dbo].[FK_EmployeeTimeCard]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TimeCards] DROP CONSTRAINT [FK_EmployeeTimeCard];
GO
IF OBJECT_ID(N'[dbo].[FK_BusinessLocationTimeCard]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TimeCards] DROP CONSTRAINT [FK_BusinessLocationTimeCard];
GO
IF OBJECT_ID(N'[dbo].[FK_EmployeeTimeCard1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TimeCards] DROP CONSTRAINT [FK_EmployeeTimeCard1];
GO
IF OBJECT_ID(N'[dbo].[FK_ShiftTimeCard]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TimeCards] DROP CONSTRAINT [FK_ShiftTimeCard];
GO
IF OBJECT_ID(N'[dbo].[FK_TimesheetUserProfile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Timesheets] DROP CONSTRAINT [FK_TimesheetUserProfile];
GO
IF OBJECT_ID(N'[dbo].[FK_TimesheetRoster]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Timesheets] DROP CONSTRAINT [FK_TimesheetRoster];
GO
IF OBJECT_ID(N'[dbo].[FK_TimesheetEntryUserProfile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TimesheetEntries] DROP CONSTRAINT [FK_TimesheetEntryUserProfile];
GO
IF OBJECT_ID(N'[dbo].[FK_TimesheetEntryTimeCard]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TimesheetEntries] DROP CONSTRAINT [FK_TimesheetEntryTimeCard];
GO
IF OBJECT_ID(N'[dbo].[FK_RosterTimeCard]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TimeCards] DROP CONSTRAINT [FK_RosterTimeCard];
GO
IF OBJECT_ID(N'[dbo].[FK_IndustryTypesUserProfile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserProfiles] DROP CONSTRAINT [FK_IndustryTypesUserProfile];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfileWorkHistory]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[WorkHistories] DROP CONSTRAINT [FK_UserProfileWorkHistory];
GO
IF OBJECT_ID(N'[dbo].[FK_IndustryTypestblQualificationslookup]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Qualificationslookups] DROP CONSTRAINT [FK_IndustryTypestblQualificationslookup];
GO
IF OBJECT_ID(N'[dbo].[FK_tblQualificationslookuptblUserQualification]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserQualifications] DROP CONSTRAINT [FK_tblQualificationslookuptblUserQualification];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfiletblUserQualification]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserQualifications] DROP CONSTRAINT [FK_UserProfiletblUserQualification];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfileOtherQualification]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[OtherQualifications] DROP CONSTRAINT [FK_UserProfileOtherQualification];
GO
IF OBJECT_ID(N'[dbo].[FK_IndustryTypesIndustrialSkills]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[IndustrySkills] DROP CONSTRAINT [FK_IndustryTypesIndustrialSkills];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfileUserSkills]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserSkills] DROP CONSTRAINT [FK_UserProfileUserSkills];
GO
IF OBJECT_ID(N'[dbo].[FK_IndustrySkillsUserSkills]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserSkills] DROP CONSTRAINT [FK_IndustrySkillsUserSkills];
GO
IF OBJECT_ID(N'[dbo].[FK_QualificationslookupShiftQualification]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ExternalShiftQualifications] DROP CONSTRAINT [FK_QualificationslookupShiftQualification];
GO
IF OBJECT_ID(N'[dbo].[FK_ExternalShiftBroadcastShift]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Shifts] DROP CONSTRAINT [FK_ExternalShiftBroadcastShift];
GO
IF OBJECT_ID(N'[dbo].[FK_ExternalShiftBroadcastShiftQualification]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ExternalShiftQualifications] DROP CONSTRAINT [FK_ExternalShiftBroadcastShiftQualification];
GO
IF OBJECT_ID(N'[dbo].[FK_ExternalShiftBroadcastShiftSkills]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ExternalShiftSkills] DROP CONSTRAINT [FK_ExternalShiftBroadcastShiftSkills];
GO
IF OBJECT_ID(N'[dbo].[FK_IndustrySkillsShiftSkills]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ExternalShiftSkills] DROP CONSTRAINT [FK_IndustrySkillsShiftSkills];
GO
IF OBJECT_ID(N'[dbo].[FK_ExternalShiftRequestEmployee1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ExternalShiftRequests] DROP CONSTRAINT [FK_ExternalShiftRequestEmployee1];
GO
IF OBJECT_ID(N'[dbo].[FK_ExternalShiftRequestExternalShiftBroadcast]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ExternalShiftRequests] DROP CONSTRAINT [FK_ExternalShiftRequestExternalShiftBroadcast];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfileExternalShiftRequest]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ExternalShiftRequests] DROP CONSTRAINT [FK_UserProfileExternalShiftRequest];
GO
IF OBJECT_ID(N'[dbo].[FK_IndustryTypesBusinessType]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BusinessTypes] DROP CONSTRAINT [FK_IndustryTypesBusinessType];
GO
IF OBJECT_ID(N'[dbo].[FK_UserRecommendationsUserProfile_UserRecommendedBy]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserRecommendations] DROP CONSTRAINT [FK_UserRecommendationsUserProfile_UserRecommendedBy];
GO
IF OBJECT_ID(N'[dbo].[FK_UserRecommendationsUserProfile_UserRecommended]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserRecommendations] DROP CONSTRAINT [FK_UserRecommendationsUserProfile_UserRecommended];
GO
IF OBJECT_ID(N'[dbo].[FK_UserSkillEndorsementsUserSkills]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserSkillEndorsements] DROP CONSTRAINT [FK_UserSkillEndorsementsUserSkills];
GO
IF OBJECT_ID(N'[dbo].[FK_UserSkillEndorsementsUserProfile_UserRecommendedBY]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserSkillEndorsements] DROP CONSTRAINT [FK_UserSkillEndorsementsUserProfile_UserRecommendedBY];
GO
IF OBJECT_ID(N'[dbo].[FK_UserSkillEndorsementsUserProfile_UserBeingRecommended]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserSkillEndorsements] DROP CONSTRAINT [FK_UserSkillEndorsementsUserProfile_UserBeingRecommended];
GO
IF OBJECT_ID(N'[dbo].[FK_MessagesUserProfile_MessagesFrom]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Messages] DROP CONSTRAINT [FK_MessagesUserProfile_MessagesFrom];
GO
IF OBJECT_ID(N'[dbo].[FK_MessagesUserProfile_MessagesTo]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Messages] DROP CONSTRAINT [FK_MessagesUserProfile_MessagesTo];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[BusinessLocations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BusinessLocations];
GO
IF OBJECT_ID(N'[dbo].[BusinessTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BusinessTypes];
GO
IF OBJECT_ID(N'[dbo].[Employees]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Employees];
GO
IF OBJECT_ID(N'[dbo].[Roles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Roles];
GO
IF OBJECT_ID(N'[dbo].[UserProfiles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserProfiles];
GO
IF OBJECT_ID(N'[dbo].[security_Membership]', 'U') IS NOT NULL
    DROP TABLE [dbo].[security_Membership];
GO
IF OBJECT_ID(N'[dbo].[security_Role]', 'U') IS NOT NULL
    DROP TABLE [dbo].[security_Role];
GO
IF OBJECT_ID(N'[dbo].[EmployerRequests]', 'U') IS NOT NULL
    DROP TABLE [dbo].[EmployerRequests];
GO
IF OBJECT_ID(N'[dbo].[InternalLocations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[InternalLocations];
GO
IF OBJECT_ID(N'[dbo].[ShiftTemplates]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ShiftTemplates];
GO
IF OBJECT_ID(N'[dbo].[Rosters]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Rosters];
GO
IF OBJECT_ID(N'[dbo].[Shifts]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Shifts];
GO
IF OBJECT_ID(N'[dbo].[BusinessPreferences]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BusinessPreferences];
GO
IF OBJECT_ID(N'[dbo].[ShiftChangeRequests]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ShiftChangeRequests];
GO
IF OBJECT_ID(N'[dbo].[Schedules]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Schedules];
GO
IF OBJECT_ID(N'[dbo].[EmployeeRequests]', 'U') IS NOT NULL
    DROP TABLE [dbo].[EmployeeRequests];
GO
IF OBJECT_ID(N'[dbo].[ShiftBlocks]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ShiftBlocks];
GO
IF OBJECT_ID(N'[dbo].[UserPreferences]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserPreferences];
GO
IF OBJECT_ID(N'[dbo].[Businesses]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Businesses];
GO
IF OBJECT_ID(N'[dbo].[RecurringShiftChangeRequests]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RecurringShiftChangeRequests];
GO
IF OBJECT_ID(N'[dbo].[security_OAuthMembership]', 'U') IS NOT NULL
    DROP TABLE [dbo].[security_OAuthMembership];
GO
IF OBJECT_ID(N'[dbo].[PaymentDetails]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PaymentDetails];
GO
IF OBJECT_ID(N'[dbo].[TimeCards]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TimeCards];
GO
IF OBJECT_ID(N'[dbo].[Timesheets]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Timesheets];
GO
IF OBJECT_ID(N'[dbo].[TimesheetEntries]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TimesheetEntries];
GO
IF OBJECT_ID(N'[dbo].[IndustryTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[IndustryTypes];
GO
IF OBJECT_ID(N'[dbo].[WorkHistories]', 'U') IS NOT NULL
    DROP TABLE [dbo].[WorkHistories];
GO
IF OBJECT_ID(N'[dbo].[Qualificationslookups]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Qualificationslookups];
GO
IF OBJECT_ID(N'[dbo].[UserQualifications]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserQualifications];
GO
IF OBJECT_ID(N'[dbo].[OtherQualifications]', 'U') IS NOT NULL
    DROP TABLE [dbo].[OtherQualifications];
GO
IF OBJECT_ID(N'[dbo].[IndustrySkills]', 'U') IS NOT NULL
    DROP TABLE [dbo].[IndustrySkills];
GO
IF OBJECT_ID(N'[dbo].[UserSkills]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserSkills];
GO
IF OBJECT_ID(N'[dbo].[ExternalShiftBroadcasts]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ExternalShiftBroadcasts];
GO
IF OBJECT_ID(N'[dbo].[ExternalShiftQualifications]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ExternalShiftQualifications];
GO
IF OBJECT_ID(N'[dbo].[ExternalShiftSkills]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ExternalShiftSkills];
GO
IF OBJECT_ID(N'[dbo].[ExternalShiftRequests]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ExternalShiftRequests];
GO
IF OBJECT_ID(N'[dbo].[UserRecommendations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserRecommendations];
GO
IF OBJECT_ID(N'[dbo].[UserSkillEndorsements]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserSkillEndorsements];
GO
IF OBJECT_ID(N'[dbo].[Messages]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Messages];
GO
IF OBJECT_ID(N'[dbo].[EmployeeRole]', 'U') IS NOT NULL
    DROP TABLE [dbo].[EmployeeRole];
GO
IF OBJECT_ID(N'[dbo].[security_UsersInRoles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[security_UsersInRoles];
GO
IF OBJECT_ID(N'[dbo].[ManagerBusinessLocations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ManagerBusinessLocations];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'BusinessLocations'
CREATE TABLE [dbo].[BusinessLocations] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Phone] nvarchar(max)  NOT NULL,
    [Address_Line1] nvarchar(max)  NOT NULL,
    [Address_Suburb] nvarchar(max)  NOT NULL,
    [Address_State] nvarchar(max)  NOT NULL,
    [Address_Postcode] nvarchar(max)  NOT NULL,
    [Address_Line2] nvarchar(max)  NULL,
    [Address_PlaceId] nvarchar(max)  NULL,
    [Address_PlaceLatitude] float  NULL,
    [Address_PlaceLongitude] float  NULL,
    [Enabled] bit  NOT NULL,
    [Business_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'BusinessTypes'
CREATE TABLE [dbo].[BusinessTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Detail] nvarchar(max)  NOT NULL,
    [IndustryType_ID] int  NOT NULL
);
GO

-- Creating table 'Employees'
CREATE TABLE [dbo].[Employees] (
    [Id] uniqueidentifier  NOT NULL,
    [FirstName] nvarchar(20)  NOT NULL,
    [LastName] nvarchar(20)  NULL,
    [MobilePhone] nvarchar(max)  NULL,
    [DateOfBirth] datetime  NULL,
    [Email] nvarchar(max)  NULL,
    [Type] int  NOT NULL,
    [IsAdmin] bit  NOT NULL,
    [IsActive] bit  NOT NULL,
    [QRCode] varbinary(max)  NULL,
    [PayRate] decimal(5,2)  NULL,
    [UserProfile_Id] uniqueidentifier  NULL,
    [BusinessLocation_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'Roles'
CREATE TABLE [dbo].[Roles] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Enabled] bit  NOT NULL,
    [Business_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'UserProfiles'
CREATE TABLE [dbo].[UserProfiles] (
    [Id] uniqueidentifier  NOT NULL,
    [FirstName] nvarchar(20)  NOT NULL,
    [LastName] nvarchar(20)  NOT NULL,
    [Email] nvarchar(max)  NOT NULL,
    [Address_Line1] nvarchar(max)  NOT NULL,
    [Address_Suburb] nvarchar(max)  NOT NULL,
    [Address_State] nvarchar(max)  NOT NULL,
    [Address_Postcode] nvarchar(max)  NOT NULL,
    [Address_Line2] nvarchar(max)  NULL,
    [Address_PlaceId] nvarchar(max)  NULL,
    [Address_PlaceLatitude] float  NULL,
    [Address_PlaceLongitude] float  NULL,
    [MobilePhone] nvarchar(max)  NOT NULL,
    [HasViewedWizard] bit  NOT NULL,
    [QRCode] varbinary(max)  NULL,
    [CurrentAddress_Line1] nvarchar(max)  NOT NULL,
    [CurrentAddress_Suburb] nvarchar(max)  NOT NULL,
    [CurrentAddress_State] nvarchar(max)  NOT NULL,
    [CurrentAddress_Postcode] nvarchar(max)  NOT NULL,
    [CurrentAddress_Line2] nvarchar(max)  NULL,
    [CurrentAddress_PlaceId] nvarchar(max)  NULL,
    [CurrentAddress_PlaceLatitude] float  NULL,
    [CurrentAddress_PlaceLongitude] float  NULL,
    [Distance] int  NULL,
    [IndustryTypesID] int  NULL,
    [AboutMe] nvarchar(400)  NULL,
    [DateofBirth] datetime  NULL,
    [IsRegisteredExternal] bit  NULL
);
GO

-- Creating table 'security_Membership'
CREATE TABLE [dbo].[security_Membership] (
    [Id] uniqueidentifier  NOT NULL,
    [CreateDate] datetime  NULL,
    [ConfirmationToken] nvarchar(128)  NULL,
    [IsConfirmed] bit  NULL,
    [LastPasswordFailureDate] datetime  NULL,
    [PasswordFailuresSinceLastSuccess] smallint  NOT NULL,
    [Password] nvarchar(128)  NOT NULL,
    [PasswordChangedDate] datetime  NULL,
    [PasswordVerificationToken] nvarchar(128)  NULL,
    [PasswordVerificationTokenExpirationDate] datetime  NULL,
    [IsLockedOut] bit  NOT NULL,
    [LastLockoutDate] datetime  NULL,
    [LastLoginDate] datetime  NULL
);
GO

-- Creating table 'security_Role'
CREATE TABLE [dbo].[security_Role] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(256)  NOT NULL
);
GO

-- Creating table 'EmployerRequests'
CREATE TABLE [dbo].[EmployerRequests] (
    [Id] uniqueidentifier  NOT NULL,
    [Status] int  NOT NULL,
    [CreatedDate] datetime  NOT NULL,
    [ActionedDate] datetime  NULL,
    [UserProfile_Id] uniqueidentifier  NOT NULL,
    [Employee_Id] uniqueidentifier  NULL,
    [ActionedBy_Id] uniqueidentifier  NULL,
    [BusinessLocation_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'InternalLocations'
CREATE TABLE [dbo].[InternalLocations] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(40)  NOT NULL,
    [Enabled] bit  NOT NULL,
    [BusinessLocation_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'ShiftTemplates'
CREATE TABLE [dbo].[ShiftTemplates] (
    [Id] uniqueidentifier  NOT NULL,
    [StartTime] time  NOT NULL,
    [FinishTime] time  NOT NULL,
    [FinishNextDay] bit  NOT NULL,
    [Monday] bit  NOT NULL,
    [Tuesday] bit  NOT NULL,
    [Wednesday] bit  NOT NULL,
    [Thursday] bit  NOT NULL,
    [Friday] bit  NOT NULL,
    [Saturday] bit  NOT NULL,
    [Sunday] bit  NOT NULL,
    [Frequency] int  NOT NULL,
    [WeekStarting] datetime  NULL,
    [Enabled] bit  NOT NULL,
    [BusinessLocation_Id] uniqueidentifier  NOT NULL,
    [Role_Id] uniqueidentifier  NULL,
    [InternalLocation_Id] uniqueidentifier  NULL,
    [Employee_Id] uniqueidentifier  NULL
);
GO

-- Creating table 'Rosters'
CREATE TABLE [dbo].[Rosters] (
    [Id] uniqueidentifier  NOT NULL,
    [WeekStartDate] datetime  NOT NULL,
    [BusinessLocation_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'Shifts'
CREATE TABLE [dbo].[Shifts] (
    [Id] uniqueidentifier  NOT NULL,
    [StartTime] datetime  NOT NULL,
    [FinishTime] datetime  NOT NULL,
    [IsPublished] bit  NOT NULL,
    [ExternalShiftBroadcastId] uniqueidentifier  NULL,
    [Roster_Id] uniqueidentifier  NOT NULL,
    [Employee_Id] uniqueidentifier  NULL,
    [Role_Id] uniqueidentifier  NULL,
    [InternalLocation_Id] uniqueidentifier  NULL,
    [ShiftTemplate_Id] uniqueidentifier  NULL
);
GO

-- Creating table 'BusinessPreferences'
CREATE TABLE [dbo].[BusinessPreferences] (
    [Id] uniqueidentifier  NOT NULL,
    [CancelShiftAllowed] bit  NOT NULL,
    [CancelShiftTimeframe] smallint  NULL
);
GO

-- Creating table 'ShiftChangeRequests'
CREATE TABLE [dbo].[ShiftChangeRequests] (
    [Id] uniqueidentifier  NOT NULL,
    [Type] int  NOT NULL,
    [Reason] nvarchar(400)  NOT NULL,
    [Status] int  NOT NULL,
    [CreatedDate] datetime  NOT NULL,
    [ActionedDate] datetime  NULL,
    [ActionedComment] nvarchar(400)  NULL,
    [Shift_Id] uniqueidentifier  NOT NULL,
    [ActionedBy_Id] uniqueidentifier  NULL,
    [CreatedBy_Id] uniqueidentifier  NULL
);
GO

-- Creating table 'Schedules'
CREATE TABLE [dbo].[Schedules] (
    [Id] smallint IDENTITY(1,1) NOT NULL,
    [Title] nvarchar(max)  NOT NULL,
    [Location] nvarchar(50)  NULL,
    [Description] nvarchar(1000)  NULL,
    [StartDate] datetime  NOT NULL,
    [StartTime] time  NOT NULL,
    [EndDate] datetime  NULL,
    [EndTime] time  NOT NULL,
    [Frequency] smallint  NOT NULL,
    [ScheduleRecurrence] int  NOT NULL,
    [NumberOfOccurrences] smallint  NULL,
    [MonthlyInterval] smallint  NOT NULL,
    [DaysOfWeek] smallint  NOT NULL,
    [WeeklyInterval] smallint  NOT NULL,
    [UserProfile_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'EmployeeRequests'
CREATE TABLE [dbo].[EmployeeRequests] (
    [Id] uniqueidentifier  NOT NULL,
    [Status] int  NOT NULL,
    [CreatedDate] datetime  NOT NULL,
    [ActionedDate] datetime  NULL,
    [Employee_Id] uniqueidentifier  NOT NULL,
    [ActionedBy_Id] uniqueidentifier  NULL,
    [BusinessLocation_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'ShiftBlocks'
CREATE TABLE [dbo].[ShiftBlocks] (
    [Id] uniqueidentifier  NOT NULL,
    [StartTime] time  NOT NULL,
    [FinishTime] time  NOT NULL,
    [FinishNextDay] bit  NOT NULL,
    [Role_Id] uniqueidentifier  NOT NULL,
    [BusinessLocation_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'UserPreferences'
CREATE TABLE [dbo].[UserPreferences] (
    [Id] uniqueidentifier  NOT NULL,
    [InternalAvailableShifts] bit  NOT NULL,
    [ExternalShiftInfo] bit  NOT NULL,
    [ExternalAvailableShifts] bit  NOT NULL,
    [DistanceTravel] int  NOT NULL,
    [ShiftReminderLength] int  NOT NULL,
    [NotifyByApp] bit  NOT NULL,
    [NotifyBySMS] bit  NOT NULL,
    [NotifyByEmail] bit  NOT NULL,
    [TimeFormat24Hr] bit  NOT NULL,
    [MonthCalView] bit  NOT NULL,
    [ImageData] varbinary(max)  NOT NULL,
    [ImageType] nvarchar(max)  NULL
);
GO

-- Creating table 'Businesses'
CREATE TABLE [dbo].[Businesses] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(40)  NOT NULL,
    [HasMultiBusLocations] bit  NOT NULL,
    [HasMultiInternalLocations] bit  NOT NULL,
    [Type_Id] int  NOT NULL
);
GO

-- Creating table 'RecurringShiftChangeRequests'
CREATE TABLE [dbo].[RecurringShiftChangeRequests] (
    [Id] uniqueidentifier  NOT NULL,
    [Type] int  NOT NULL,
    [Reason] nvarchar(max)  NOT NULL,
    [Status] int  NOT NULL,
    [OccurenceDate] datetime  NOT NULL,
    [CreatedDate] datetime  NOT NULL,
    [ActionedDate] datetime  NULL,
    [ActionedComment] nvarchar(400)  NULL,
    [ShiftTemplate_Id] uniqueidentifier  NOT NULL,
    [ActionedBy_Id] uniqueidentifier  NULL
);
GO

-- Creating table 'security_OAuthMembership'
CREATE TABLE [dbo].[security_OAuthMembership] (
    [Provider] nvarchar(30)  NOT NULL,
    [ProviderUserId] nvarchar(100)  NOT NULL,
    [UserProfile_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'PaymentDetails'
CREATE TABLE [dbo].[PaymentDetails] (
    [Id] uniqueidentifier  NOT NULL,
    [TokenCustomerID] bigint  NOT NULL,
    [CreatedDate] datetime  NOT NULL,
    [Employee_Id] uniqueidentifier  NOT NULL,
    [BusinessLocation_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'TimeCards'
CREATE TABLE [dbo].[TimeCards] (
    [Id] uniqueidentifier  NOT NULL,
    [ClockIn] datetime  NOT NULL,
    [ClockInComment] nvarchar(max)  NULL,
    [ClockOut] datetime  NULL,
    [ClockOutComment] nvarchar(max)  NULL,
    [LastUpdatedDate] datetime  NOT NULL,
    [Employee_Id] uniqueidentifier  NOT NULL,
    [BusinessLocation_Id] uniqueidentifier  NOT NULL,
    [LastUpdatedBy_Id] uniqueidentifier  NOT NULL,
    [Shift_Id] uniqueidentifier  NULL,
    [Roster_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'Timesheets'
CREATE TABLE [dbo].[Timesheets] (
    [Id] uniqueidentifier  NOT NULL,
    [Approved] bit  NOT NULL,
    [ApprovedDateTime] datetime  NULL,
    [ApprovedBy_Id] uniqueidentifier  NULL,
    [Roster_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'TimesheetEntries'
CREATE TABLE [dbo].[TimesheetEntries] (
    [Id] uniqueidentifier  NOT NULL,
    [StartDateTime] datetime  NOT NULL,
    [FinishDateTime] datetime  NULL,
    [Approved] bit  NOT NULL,
    [ApprovedDateTime] datetime  NULL,
    [ApprovedBy_Id] uniqueidentifier  NULL,
    [TimeCard_Id] uniqueidentifier  NOT NULL,
    [TimeCard_ClockIn] datetime  NOT NULL
);
GO

-- Creating table 'IndustryTypes'
CREATE TABLE [dbo].[IndustryTypes] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'WorkHistories'
CREATE TABLE [dbo].[WorkHistories] (
    [workId] uniqueidentifier  NOT NULL,
    [workCompanyName] nvarchar(100)  NOT NULL,
    [workStartDate] datetime  NOT NULL,
    [workEndDate] datetime  NOT NULL,
    [UserProfileId] uniqueidentifier  NOT NULL,
    [UserRole] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Qualificationslookups'
CREATE TABLE [dbo].[Qualificationslookups] (
    [QualificationId] uniqueidentifier  NOT NULL,
    [QualificationName] nvarchar(200)  NOT NULL,
    [IndustryTypesID] int  NOT NULL
);
GO

-- Creating table 'UserQualifications'
CREATE TABLE [dbo].[UserQualifications] (
    [UserQualificationId] uniqueidentifier  NOT NULL,
    [QualificationslookupQualificationId] uniqueidentifier  NOT NULL,
    [UserProfileId] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'OtherQualifications'
CREATE TABLE [dbo].[OtherQualifications] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(300)  NOT NULL,
    [UserProfileId] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'IndustrySkills'
CREATE TABLE [dbo].[IndustrySkills] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(200)  NOT NULL,
    [IndustryTypesID] int  NOT NULL
);
GO

-- Creating table 'UserSkills'
CREATE TABLE [dbo].[UserSkills] (
    [UserSkillId] uniqueidentifier  NOT NULL,
    [UserProfileId] uniqueidentifier  NOT NULL,
    [IndustrySkillsId] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'ExternalShiftBroadcasts'
CREATE TABLE [dbo].[ExternalShiftBroadcasts] (
    [Id] uniqueidentifier  NOT NULL,
    [Description] nvarchar(max)  NOT NULL,
    [Wage] decimal(18,0)  NOT NULL,
    [Status] int  NOT NULL
);
GO

-- Creating table 'ExternalShiftQualifications'
CREATE TABLE [dbo].[ExternalShiftQualifications] (
    [Id] uniqueidentifier  NOT NULL,
    [QualificationslookupQualificationId] uniqueidentifier  NOT NULL,
    [ExternalShiftBroadcastId] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'ExternalShiftSkills'
CREATE TABLE [dbo].[ExternalShiftSkills] (
    [Id] uniqueidentifier  NOT NULL,
    [ExternalShiftBroadcastId] uniqueidentifier  NOT NULL,
    [IndustrySkillsId] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'ExternalShiftRequests'
CREATE TABLE [dbo].[ExternalShiftRequests] (
    [Id] uniqueidentifier  NOT NULL,
    [Type] int  NULL,
    [Reason] nvarchar(max)  NULL,
    [Status] int  NULL,
    [CreatedDate] datetime  NULL,
    [ActionedDate] datetime  NULL,
    [ActionedComment] nvarchar(max)  NULL,
    [ExternalShiftMessage] nvarchar(max)  NULL,
    [ActionedBy_Id] uniqueidentifier  NULL,
    [ExternalShiftBroadcast_Id] uniqueidentifier  NULL,
    [CreatedBy_Id] uniqueidentifier  NULL
);
GO

-- Creating table 'UserRecommendations'
CREATE TABLE [dbo].[UserRecommendations] (
    [Id] uniqueidentifier  NOT NULL,
    [UserProfileUserRecommendedBy_Id] uniqueidentifier  NOT NULL,
    [UserProfileUserRecommended_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'UserSkillEndorsements'
CREATE TABLE [dbo].[UserSkillEndorsements] (
    [Id] uniqueidentifier  NOT NULL,
    [UserSkill_UserSkillId] uniqueidentifier  NOT NULL,
    [UserProfileUserRecommendedBY_Id] uniqueidentifier  NOT NULL,
    [UserProfileUserBeingRecommended_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'Messages'
CREATE TABLE [dbo].[Messages] (
    [Id] uniqueidentifier  NOT NULL,
    [Message] nvarchar(max)  NULL,
    [DateSent] datetime  NULL,
    [UserProfilesMessageFrom_Id] uniqueidentifier  NOT NULL,
    [UserProfileMessageTo_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'EmployeeRole'
CREATE TABLE [dbo].[EmployeeRole] (
    [Employee_Id] uniqueidentifier  NOT NULL,
    [Roles_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'security_UsersInRoles'
CREATE TABLE [dbo].[security_UsersInRoles] (
    [UserProfile_Id] uniqueidentifier  NOT NULL,
    [security_Roles_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'ManagerBusinessLocations'
CREATE TABLE [dbo].[ManagerBusinessLocations] (
    [Managers_Id] uniqueidentifier  NOT NULL,
    [ManagerBusinessLocations_Id] uniqueidentifier  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'BusinessLocations'
ALTER TABLE [dbo].[BusinessLocations]
ADD CONSTRAINT [PK_BusinessLocations]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'BusinessTypes'
ALTER TABLE [dbo].[BusinessTypes]
ADD CONSTRAINT [PK_BusinessTypes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Employees'
ALTER TABLE [dbo].[Employees]
ADD CONSTRAINT [PK_Employees]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Roles'
ALTER TABLE [dbo].[Roles]
ADD CONSTRAINT [PK_Roles]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UserProfiles'
ALTER TABLE [dbo].[UserProfiles]
ADD CONSTRAINT [PK_UserProfiles]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'security_Membership'
ALTER TABLE [dbo].[security_Membership]
ADD CONSTRAINT [PK_security_Membership]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'security_Role'
ALTER TABLE [dbo].[security_Role]
ADD CONSTRAINT [PK_security_Role]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'EmployerRequests'
ALTER TABLE [dbo].[EmployerRequests]
ADD CONSTRAINT [PK_EmployerRequests]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'InternalLocations'
ALTER TABLE [dbo].[InternalLocations]
ADD CONSTRAINT [PK_InternalLocations]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ShiftTemplates'
ALTER TABLE [dbo].[ShiftTemplates]
ADD CONSTRAINT [PK_ShiftTemplates]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Rosters'
ALTER TABLE [dbo].[Rosters]
ADD CONSTRAINT [PK_Rosters]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Shifts'
ALTER TABLE [dbo].[Shifts]
ADD CONSTRAINT [PK_Shifts]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'BusinessPreferences'
ALTER TABLE [dbo].[BusinessPreferences]
ADD CONSTRAINT [PK_BusinessPreferences]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ShiftChangeRequests'
ALTER TABLE [dbo].[ShiftChangeRequests]
ADD CONSTRAINT [PK_ShiftChangeRequests]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Schedules'
ALTER TABLE [dbo].[Schedules]
ADD CONSTRAINT [PK_Schedules]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'EmployeeRequests'
ALTER TABLE [dbo].[EmployeeRequests]
ADD CONSTRAINT [PK_EmployeeRequests]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ShiftBlocks'
ALTER TABLE [dbo].[ShiftBlocks]
ADD CONSTRAINT [PK_ShiftBlocks]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UserPreferences'
ALTER TABLE [dbo].[UserPreferences]
ADD CONSTRAINT [PK_UserPreferences]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Businesses'
ALTER TABLE [dbo].[Businesses]
ADD CONSTRAINT [PK_Businesses]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'RecurringShiftChangeRequests'
ALTER TABLE [dbo].[RecurringShiftChangeRequests]
ADD CONSTRAINT [PK_RecurringShiftChangeRequests]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Provider], [ProviderUserId] in table 'security_OAuthMembership'
ALTER TABLE [dbo].[security_OAuthMembership]
ADD CONSTRAINT [PK_security_OAuthMembership]
    PRIMARY KEY CLUSTERED ([Provider], [ProviderUserId] ASC);
GO

-- Creating primary key on [Id] in table 'PaymentDetails'
ALTER TABLE [dbo].[PaymentDetails]
ADD CONSTRAINT [PK_PaymentDetails]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id], [ClockIn] in table 'TimeCards'
ALTER TABLE [dbo].[TimeCards]
ADD CONSTRAINT [PK_TimeCards]
    PRIMARY KEY CLUSTERED ([Id], [ClockIn] ASC);
GO

-- Creating primary key on [Id] in table 'Timesheets'
ALTER TABLE [dbo].[Timesheets]
ADD CONSTRAINT [PK_Timesheets]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'TimesheetEntries'
ALTER TABLE [dbo].[TimesheetEntries]
ADD CONSTRAINT [PK_TimesheetEntries]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [ID] in table 'IndustryTypes'
ALTER TABLE [dbo].[IndustryTypes]
ADD CONSTRAINT [PK_IndustryTypes]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [workId] in table 'WorkHistories'
ALTER TABLE [dbo].[WorkHistories]
ADD CONSTRAINT [PK_WorkHistories]
    PRIMARY KEY CLUSTERED ([workId] ASC);
GO

-- Creating primary key on [QualificationId] in table 'Qualificationslookups'
ALTER TABLE [dbo].[Qualificationslookups]
ADD CONSTRAINT [PK_Qualificationslookups]
    PRIMARY KEY CLUSTERED ([QualificationId] ASC);
GO

-- Creating primary key on [UserQualificationId] in table 'UserQualifications'
ALTER TABLE [dbo].[UserQualifications]
ADD CONSTRAINT [PK_UserQualifications]
    PRIMARY KEY CLUSTERED ([UserQualificationId] ASC);
GO

-- Creating primary key on [Id] in table 'OtherQualifications'
ALTER TABLE [dbo].[OtherQualifications]
ADD CONSTRAINT [PK_OtherQualifications]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'IndustrySkills'
ALTER TABLE [dbo].[IndustrySkills]
ADD CONSTRAINT [PK_IndustrySkills]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [UserSkillId] in table 'UserSkills'
ALTER TABLE [dbo].[UserSkills]
ADD CONSTRAINT [PK_UserSkills]
    PRIMARY KEY CLUSTERED ([UserSkillId] ASC);
GO

-- Creating primary key on [Id] in table 'ExternalShiftBroadcasts'
ALTER TABLE [dbo].[ExternalShiftBroadcasts]
ADD CONSTRAINT [PK_ExternalShiftBroadcasts]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ExternalShiftQualifications'
ALTER TABLE [dbo].[ExternalShiftQualifications]
ADD CONSTRAINT [PK_ExternalShiftQualifications]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ExternalShiftSkills'
ALTER TABLE [dbo].[ExternalShiftSkills]
ADD CONSTRAINT [PK_ExternalShiftSkills]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ExternalShiftRequests'
ALTER TABLE [dbo].[ExternalShiftRequests]
ADD CONSTRAINT [PK_ExternalShiftRequests]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UserRecommendations'
ALTER TABLE [dbo].[UserRecommendations]
ADD CONSTRAINT [PK_UserRecommendations]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UserSkillEndorsements'
ALTER TABLE [dbo].[UserSkillEndorsements]
ADD CONSTRAINT [PK_UserSkillEndorsements]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Messages'
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [PK_Messages]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Employee_Id], [Roles_Id] in table 'EmployeeRole'
ALTER TABLE [dbo].[EmployeeRole]
ADD CONSTRAINT [PK_EmployeeRole]
    PRIMARY KEY CLUSTERED ([Employee_Id], [Roles_Id] ASC);
GO

-- Creating primary key on [UserProfile_Id], [security_Roles_Id] in table 'security_UsersInRoles'
ALTER TABLE [dbo].[security_UsersInRoles]
ADD CONSTRAINT [PK_security_UsersInRoles]
    PRIMARY KEY CLUSTERED ([UserProfile_Id], [security_Roles_Id] ASC);
GO

-- Creating primary key on [Managers_Id], [ManagerBusinessLocations_Id] in table 'ManagerBusinessLocations'
ALTER TABLE [dbo].[ManagerBusinessLocations]
ADD CONSTRAINT [PK_ManagerBusinessLocations]
    PRIMARY KEY CLUSTERED ([Managers_Id], [ManagerBusinessLocations_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Employee_Id] in table 'EmployeeRole'
ALTER TABLE [dbo].[EmployeeRole]
ADD CONSTRAINT [FK_EmployeeRole_Employee]
    FOREIGN KEY ([Employee_Id])
    REFERENCES [dbo].[Employees]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Roles_Id] in table 'EmployeeRole'
ALTER TABLE [dbo].[EmployeeRole]
ADD CONSTRAINT [FK_EmployeeRole_Role]
    FOREIGN KEY ([Roles_Id])
    REFERENCES [dbo].[Roles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EmployeeRole_Role'
CREATE INDEX [IX_FK_EmployeeRole_Role]
ON [dbo].[EmployeeRole]
    ([Roles_Id]);
GO

-- Creating foreign key on [Id] in table 'security_Membership'
ALTER TABLE [dbo].[security_Membership]
ADD CONSTRAINT [FK_MembershipUserProfile]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [UserProfile_Id] in table 'security_UsersInRoles'
ALTER TABLE [dbo].[security_UsersInRoles]
ADD CONSTRAINT [FK_security_UsersInRole_UserProfile]
    FOREIGN KEY ([UserProfile_Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [security_Roles_Id] in table 'security_UsersInRoles'
ALTER TABLE [dbo].[security_UsersInRoles]
ADD CONSTRAINT [FK_security_UsersInRole_security_Role]
    FOREIGN KEY ([security_Roles_Id])
    REFERENCES [dbo].[security_Role]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_security_UsersInRole_security_Role'
CREATE INDEX [IX_FK_security_UsersInRole_security_Role]
ON [dbo].[security_UsersInRoles]
    ([security_Roles_Id]);
GO

-- Creating foreign key on [UserProfile_Id] in table 'Employees'
ALTER TABLE [dbo].[Employees]
ADD CONSTRAINT [FK_UserProfileEmployee]
    FOREIGN KEY ([UserProfile_Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfileEmployee'
CREATE INDEX [IX_FK_UserProfileEmployee]
ON [dbo].[Employees]
    ([UserProfile_Id]);
GO

-- Creating foreign key on [UserProfile_Id] in table 'EmployerRequests'
ALTER TABLE [dbo].[EmployerRequests]
ADD CONSTRAINT [FK_UserProfileEmployerRequest]
    FOREIGN KEY ([UserProfile_Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfileEmployerRequest'
CREATE INDEX [IX_FK_UserProfileEmployerRequest]
ON [dbo].[EmployerRequests]
    ([UserProfile_Id]);
GO

-- Creating foreign key on [Employee_Id] in table 'EmployerRequests'
ALTER TABLE [dbo].[EmployerRequests]
ADD CONSTRAINT [FK_EmployeeEmployerRequest]
    FOREIGN KEY ([Employee_Id])
    REFERENCES [dbo].[Employees]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EmployeeEmployerRequest'
CREATE INDEX [IX_FK_EmployeeEmployerRequest]
ON [dbo].[EmployerRequests]
    ([Employee_Id]);
GO

-- Creating foreign key on [ActionedBy_Id] in table 'EmployerRequests'
ALTER TABLE [dbo].[EmployerRequests]
ADD CONSTRAINT [FK_ManagerEmployerRequest]
    FOREIGN KEY ([ActionedBy_Id])
    REFERENCES [dbo].[Employees]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ManagerEmployerRequest'
CREATE INDEX [IX_FK_ManagerEmployerRequest]
ON [dbo].[EmployerRequests]
    ([ActionedBy_Id]);
GO

-- Creating foreign key on [BusinessLocation_Id] in table 'InternalLocations'
ALTER TABLE [dbo].[InternalLocations]
ADD CONSTRAINT [FK_BusinessLocationInternalLocation]
    FOREIGN KEY ([BusinessLocation_Id])
    REFERENCES [dbo].[BusinessLocations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BusinessLocationInternalLocation'
CREATE INDEX [IX_FK_BusinessLocationInternalLocation]
ON [dbo].[InternalLocations]
    ([BusinessLocation_Id]);
GO

-- Creating foreign key on [BusinessLocation_Id] in table 'ShiftTemplates'
ALTER TABLE [dbo].[ShiftTemplates]
ADD CONSTRAINT [FK_BusinessLocationShiftTemplate]
    FOREIGN KEY ([BusinessLocation_Id])
    REFERENCES [dbo].[BusinessLocations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BusinessLocationShiftTemplate'
CREATE INDEX [IX_FK_BusinessLocationShiftTemplate]
ON [dbo].[ShiftTemplates]
    ([BusinessLocation_Id]);
GO

-- Creating foreign key on [Role_Id] in table 'ShiftTemplates'
ALTER TABLE [dbo].[ShiftTemplates]
ADD CONSTRAINT [FK_ShiftTemplateRole]
    FOREIGN KEY ([Role_Id])
    REFERENCES [dbo].[Roles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ShiftTemplateRole'
CREATE INDEX [IX_FK_ShiftTemplateRole]
ON [dbo].[ShiftTemplates]
    ([Role_Id]);
GO

-- Creating foreign key on [InternalLocation_Id] in table 'ShiftTemplates'
ALTER TABLE [dbo].[ShiftTemplates]
ADD CONSTRAINT [FK_ShiftTemplateInternalLocation]
    FOREIGN KEY ([InternalLocation_Id])
    REFERENCES [dbo].[InternalLocations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ShiftTemplateInternalLocation'
CREATE INDEX [IX_FK_ShiftTemplateInternalLocation]
ON [dbo].[ShiftTemplates]
    ([InternalLocation_Id]);
GO

-- Creating foreign key on [Employee_Id] in table 'ShiftTemplates'
ALTER TABLE [dbo].[ShiftTemplates]
ADD CONSTRAINT [FK_ShiftTemplateEmployee]
    FOREIGN KEY ([Employee_Id])
    REFERENCES [dbo].[Employees]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ShiftTemplateEmployee'
CREATE INDEX [IX_FK_ShiftTemplateEmployee]
ON [dbo].[ShiftTemplates]
    ([Employee_Id]);
GO

-- Creating foreign key on [Roster_Id] in table 'Shifts'
ALTER TABLE [dbo].[Shifts]
ADD CONSTRAINT [FK_RosterShift]
    FOREIGN KEY ([Roster_Id])
    REFERENCES [dbo].[Rosters]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_RosterShift'
CREATE INDEX [IX_FK_RosterShift]
ON [dbo].[Shifts]
    ([Roster_Id]);
GO

-- Creating foreign key on [Employee_Id] in table 'Shifts'
ALTER TABLE [dbo].[Shifts]
ADD CONSTRAINT [FK_EmployeeShift]
    FOREIGN KEY ([Employee_Id])
    REFERENCES [dbo].[Employees]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EmployeeShift'
CREATE INDEX [IX_FK_EmployeeShift]
ON [dbo].[Shifts]
    ([Employee_Id]);
GO

-- Creating foreign key on [Role_Id] in table 'Shifts'
ALTER TABLE [dbo].[Shifts]
ADD CONSTRAINT [FK_RoleShift]
    FOREIGN KEY ([Role_Id])
    REFERENCES [dbo].[Roles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_RoleShift'
CREATE INDEX [IX_FK_RoleShift]
ON [dbo].[Shifts]
    ([Role_Id]);
GO

-- Creating foreign key on [InternalLocation_Id] in table 'Shifts'
ALTER TABLE [dbo].[Shifts]
ADD CONSTRAINT [FK_InternalLocationShift]
    FOREIGN KEY ([InternalLocation_Id])
    REFERENCES [dbo].[InternalLocations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_InternalLocationShift'
CREATE INDEX [IX_FK_InternalLocationShift]
ON [dbo].[Shifts]
    ([InternalLocation_Id]);
GO

-- Creating foreign key on [Id] in table 'BusinessPreferences'
ALTER TABLE [dbo].[BusinessPreferences]
ADD CONSTRAINT [FK_BusinessBusinessPreferences]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[BusinessLocations]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Shift_Id] in table 'ShiftChangeRequests'
ALTER TABLE [dbo].[ShiftChangeRequests]
ADD CONSTRAINT [FK_ShiftShiftChangeRequest]
    FOREIGN KEY ([Shift_Id])
    REFERENCES [dbo].[Shifts]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ShiftShiftChangeRequest'
CREATE INDEX [IX_FK_ShiftShiftChangeRequest]
ON [dbo].[ShiftChangeRequests]
    ([Shift_Id]);
GO

-- Creating foreign key on [ActionedBy_Id] in table 'ShiftChangeRequests'
ALTER TABLE [dbo].[ShiftChangeRequests]
ADD CONSTRAINT [FK_ShiftChangeRequestEmployee]
    FOREIGN KEY ([ActionedBy_Id])
    REFERENCES [dbo].[Employees]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ShiftChangeRequestEmployee'
CREATE INDEX [IX_FK_ShiftChangeRequestEmployee]
ON [dbo].[ShiftChangeRequests]
    ([ActionedBy_Id]);
GO

-- Creating foreign key on [UserProfile_Id] in table 'Schedules'
ALTER TABLE [dbo].[Schedules]
ADD CONSTRAINT [FK_UserProfileRecurringCalendarEvent]
    FOREIGN KEY ([UserProfile_Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfileRecurringCalendarEvent'
CREATE INDEX [IX_FK_UserProfileRecurringCalendarEvent]
ON [dbo].[Schedules]
    ([UserProfile_Id]);
GO

-- Creating foreign key on [Employee_Id] in table 'EmployeeRequests'
ALTER TABLE [dbo].[EmployeeRequests]
ADD CONSTRAINT [FK_EmployeeEmployeeRequest]
    FOREIGN KEY ([Employee_Id])
    REFERENCES [dbo].[Employees]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EmployeeEmployeeRequest'
CREATE INDEX [IX_FK_EmployeeEmployeeRequest]
ON [dbo].[EmployeeRequests]
    ([Employee_Id]);
GO

-- Creating foreign key on [ActionedBy_Id] in table 'EmployeeRequests'
ALTER TABLE [dbo].[EmployeeRequests]
ADD CONSTRAINT [FK_EmployeeRequestUserProfile]
    FOREIGN KEY ([ActionedBy_Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EmployeeRequestUserProfile'
CREATE INDEX [IX_FK_EmployeeRequestUserProfile]
ON [dbo].[EmployeeRequests]
    ([ActionedBy_Id]);
GO

-- Creating foreign key on [Role_Id] in table 'ShiftBlocks'
ALTER TABLE [dbo].[ShiftBlocks]
ADD CONSTRAINT [FK_ShiftBlockRole]
    FOREIGN KEY ([Role_Id])
    REFERENCES [dbo].[Roles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ShiftBlockRole'
CREATE INDEX [IX_FK_ShiftBlockRole]
ON [dbo].[ShiftBlocks]
    ([Role_Id]);
GO

-- Creating foreign key on [BusinessLocation_Id] in table 'ShiftBlocks'
ALTER TABLE [dbo].[ShiftBlocks]
ADD CONSTRAINT [FK_BusinessShiftBlock]
    FOREIGN KEY ([BusinessLocation_Id])
    REFERENCES [dbo].[BusinessLocations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BusinessShiftBlock'
CREATE INDEX [IX_FK_BusinessShiftBlock]
ON [dbo].[ShiftBlocks]
    ([BusinessLocation_Id]);
GO

-- Creating foreign key on [Id] in table 'UserPreferences'
ALTER TABLE [dbo].[UserPreferences]
ADD CONSTRAINT [FK_UserProfileUserPreferences]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Business_Id] in table 'BusinessLocations'
ALTER TABLE [dbo].[BusinessLocations]
ADD CONSTRAINT [FK_BusinessBusinessLocation]
    FOREIGN KEY ([Business_Id])
    REFERENCES [dbo].[Businesses]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BusinessBusinessLocation'
CREATE INDEX [IX_FK_BusinessBusinessLocation]
ON [dbo].[BusinessLocations]
    ([Business_Id]);
GO

-- Creating foreign key on [Type_Id] in table 'Businesses'
ALTER TABLE [dbo].[Businesses]
ADD CONSTRAINT [FK_BusinessTypeBusiness]
    FOREIGN KEY ([Type_Id])
    REFERENCES [dbo].[BusinessTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BusinessTypeBusiness'
CREATE INDEX [IX_FK_BusinessTypeBusiness]
ON [dbo].[Businesses]
    ([Type_Id]);
GO

-- Creating foreign key on [Business_Id] in table 'Roles'
ALTER TABLE [dbo].[Roles]
ADD CONSTRAINT [FK_BusinessRole]
    FOREIGN KEY ([Business_Id])
    REFERENCES [dbo].[Businesses]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BusinessRole'
CREATE INDEX [IX_FK_BusinessRole]
ON [dbo].[Roles]
    ([Business_Id]);
GO

-- Creating foreign key on [BusinessLocation_Id] in table 'EmployerRequests'
ALTER TABLE [dbo].[EmployerRequests]
ADD CONSTRAINT [FK_BusinessLocationEmployerRequest]
    FOREIGN KEY ([BusinessLocation_Id])
    REFERENCES [dbo].[BusinessLocations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BusinessLocationEmployerRequest'
CREATE INDEX [IX_FK_BusinessLocationEmployerRequest]
ON [dbo].[EmployerRequests]
    ([BusinessLocation_Id]);
GO

-- Creating foreign key on [BusinessLocation_Id] in table 'Employees'
ALTER TABLE [dbo].[Employees]
ADD CONSTRAINT [FK_BusinessLocationEmployee]
    FOREIGN KEY ([BusinessLocation_Id])
    REFERENCES [dbo].[BusinessLocations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BusinessLocationEmployee'
CREATE INDEX [IX_FK_BusinessLocationEmployee]
ON [dbo].[Employees]
    ([BusinessLocation_Id]);
GO

-- Creating foreign key on [BusinessLocation_Id] in table 'EmployeeRequests'
ALTER TABLE [dbo].[EmployeeRequests]
ADD CONSTRAINT [FK_BusinessLocationEmployeeRequest]
    FOREIGN KEY ([BusinessLocation_Id])
    REFERENCES [dbo].[BusinessLocations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BusinessLocationEmployeeRequest'
CREATE INDEX [IX_FK_BusinessLocationEmployeeRequest]
ON [dbo].[EmployeeRequests]
    ([BusinessLocation_Id]);
GO

-- Creating foreign key on [Managers_Id] in table 'ManagerBusinessLocations'
ALTER TABLE [dbo].[ManagerBusinessLocations]
ADD CONSTRAINT [FK_ManagerBusinessLocation_Manager]
    FOREIGN KEY ([Managers_Id])
    REFERENCES [dbo].[Employees]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ManagerBusinessLocations_Id] in table 'ManagerBusinessLocations'
ALTER TABLE [dbo].[ManagerBusinessLocations]
ADD CONSTRAINT [FK_ManagerBusinessLocation_BusinessLocation]
    FOREIGN KEY ([ManagerBusinessLocations_Id])
    REFERENCES [dbo].[BusinessLocations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ManagerBusinessLocation_BusinessLocation'
CREATE INDEX [IX_FK_ManagerBusinessLocation_BusinessLocation]
ON [dbo].[ManagerBusinessLocations]
    ([ManagerBusinessLocations_Id]);
GO

-- Creating foreign key on [BusinessLocation_Id] in table 'Rosters'
ALTER TABLE [dbo].[Rosters]
ADD CONSTRAINT [FK_BusinessLocationRoster]
    FOREIGN KEY ([BusinessLocation_Id])
    REFERENCES [dbo].[BusinessLocations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BusinessLocationRoster'
CREATE INDEX [IX_FK_BusinessLocationRoster]
ON [dbo].[Rosters]
    ([BusinessLocation_Id]);
GO

-- Creating foreign key on [ShiftTemplate_Id] in table 'RecurringShiftChangeRequests'
ALTER TABLE [dbo].[RecurringShiftChangeRequests]
ADD CONSTRAINT [FK_ShiftTemplateRecurringShiftChangeRequest]
    FOREIGN KEY ([ShiftTemplate_Id])
    REFERENCES [dbo].[ShiftTemplates]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ShiftTemplateRecurringShiftChangeRequest'
CREATE INDEX [IX_FK_ShiftTemplateRecurringShiftChangeRequest]
ON [dbo].[RecurringShiftChangeRequests]
    ([ShiftTemplate_Id]);
GO

-- Creating foreign key on [ActionedBy_Id] in table 'RecurringShiftChangeRequests'
ALTER TABLE [dbo].[RecurringShiftChangeRequests]
ADD CONSTRAINT [FK_EmployeeRecurringShiftChangeRequest]
    FOREIGN KEY ([ActionedBy_Id])
    REFERENCES [dbo].[Employees]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EmployeeRecurringShiftChangeRequest'
CREATE INDEX [IX_FK_EmployeeRecurringShiftChangeRequest]
ON [dbo].[RecurringShiftChangeRequests]
    ([ActionedBy_Id]);
GO

-- Creating foreign key on [CreatedBy_Id] in table 'ShiftChangeRequests'
ALTER TABLE [dbo].[ShiftChangeRequests]
ADD CONSTRAINT [FK_ShiftChangeRequestEmployee1]
    FOREIGN KEY ([CreatedBy_Id])
    REFERENCES [dbo].[Employees]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ShiftChangeRequestEmployee1'
CREATE INDEX [IX_FK_ShiftChangeRequestEmployee1]
ON [dbo].[ShiftChangeRequests]
    ([CreatedBy_Id]);
GO

-- Creating foreign key on [ShiftTemplate_Id] in table 'Shifts'
ALTER TABLE [dbo].[Shifts]
ADD CONSTRAINT [FK_ShiftShiftTemplate]
    FOREIGN KEY ([ShiftTemplate_Id])
    REFERENCES [dbo].[ShiftTemplates]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ShiftShiftTemplate'
CREATE INDEX [IX_FK_ShiftShiftTemplate]
ON [dbo].[Shifts]
    ([ShiftTemplate_Id]);
GO

-- Creating foreign key on [UserProfile_Id] in table 'security_OAuthMembership'
ALTER TABLE [dbo].[security_OAuthMembership]
ADD CONSTRAINT [FK_UserProfilewebpages_OAuthMembership]
    FOREIGN KEY ([UserProfile_Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfilewebpages_OAuthMembership'
CREATE INDEX [IX_FK_UserProfilewebpages_OAuthMembership]
ON [dbo].[security_OAuthMembership]
    ([UserProfile_Id]);
GO

-- Creating foreign key on [Employee_Id] in table 'PaymentDetails'
ALTER TABLE [dbo].[PaymentDetails]
ADD CONSTRAINT [FK_EmployeePaymentDetails]
    FOREIGN KEY ([Employee_Id])
    REFERENCES [dbo].[Employees]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EmployeePaymentDetails'
CREATE INDEX [IX_FK_EmployeePaymentDetails]
ON [dbo].[PaymentDetails]
    ([Employee_Id]);
GO

-- Creating foreign key on [BusinessLocation_Id] in table 'PaymentDetails'
ALTER TABLE [dbo].[PaymentDetails]
ADD CONSTRAINT [FK_BusinessLocationPaymentDetails]
    FOREIGN KEY ([BusinessLocation_Id])
    REFERENCES [dbo].[BusinessLocations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BusinessLocationPaymentDetails'
CREATE INDEX [IX_FK_BusinessLocationPaymentDetails]
ON [dbo].[PaymentDetails]
    ([BusinessLocation_Id]);
GO

-- Creating foreign key on [Employee_Id] in table 'TimeCards'
ALTER TABLE [dbo].[TimeCards]
ADD CONSTRAINT [FK_EmployeeTimeCard]
    FOREIGN KEY ([Employee_Id])
    REFERENCES [dbo].[Employees]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EmployeeTimeCard'
CREATE INDEX [IX_FK_EmployeeTimeCard]
ON [dbo].[TimeCards]
    ([Employee_Id]);
GO

-- Creating foreign key on [BusinessLocation_Id] in table 'TimeCards'
ALTER TABLE [dbo].[TimeCards]
ADD CONSTRAINT [FK_BusinessLocationTimeCard]
    FOREIGN KEY ([BusinessLocation_Id])
    REFERENCES [dbo].[BusinessLocations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BusinessLocationTimeCard'
CREATE INDEX [IX_FK_BusinessLocationTimeCard]
ON [dbo].[TimeCards]
    ([BusinessLocation_Id]);
GO

-- Creating foreign key on [LastUpdatedBy_Id] in table 'TimeCards'
ALTER TABLE [dbo].[TimeCards]
ADD CONSTRAINT [FK_EmployeeTimeCard1]
    FOREIGN KEY ([LastUpdatedBy_Id])
    REFERENCES [dbo].[Employees]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EmployeeTimeCard1'
CREATE INDEX [IX_FK_EmployeeTimeCard1]
ON [dbo].[TimeCards]
    ([LastUpdatedBy_Id]);
GO

-- Creating foreign key on [Shift_Id] in table 'TimeCards'
ALTER TABLE [dbo].[TimeCards]
ADD CONSTRAINT [FK_ShiftTimeCard]
    FOREIGN KEY ([Shift_Id])
    REFERENCES [dbo].[Shifts]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ShiftTimeCard'
CREATE INDEX [IX_FK_ShiftTimeCard]
ON [dbo].[TimeCards]
    ([Shift_Id]);
GO

-- Creating foreign key on [ApprovedBy_Id] in table 'Timesheets'
ALTER TABLE [dbo].[Timesheets]
ADD CONSTRAINT [FK_TimesheetUserProfile]
    FOREIGN KEY ([ApprovedBy_Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TimesheetUserProfile'
CREATE INDEX [IX_FK_TimesheetUserProfile]
ON [dbo].[Timesheets]
    ([ApprovedBy_Id]);
GO

-- Creating foreign key on [Roster_Id] in table 'Timesheets'
ALTER TABLE [dbo].[Timesheets]
ADD CONSTRAINT [FK_TimesheetRoster]
    FOREIGN KEY ([Roster_Id])
    REFERENCES [dbo].[Rosters]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TimesheetRoster'
CREATE INDEX [IX_FK_TimesheetRoster]
ON [dbo].[Timesheets]
    ([Roster_Id]);
GO

-- Creating foreign key on [ApprovedBy_Id] in table 'TimesheetEntries'
ALTER TABLE [dbo].[TimesheetEntries]
ADD CONSTRAINT [FK_TimesheetEntryUserProfile]
    FOREIGN KEY ([ApprovedBy_Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TimesheetEntryUserProfile'
CREATE INDEX [IX_FK_TimesheetEntryUserProfile]
ON [dbo].[TimesheetEntries]
    ([ApprovedBy_Id]);
GO

-- Creating foreign key on [TimeCard_Id], [TimeCard_ClockIn] in table 'TimesheetEntries'
ALTER TABLE [dbo].[TimesheetEntries]
ADD CONSTRAINT [FK_TimesheetEntryTimeCard]
    FOREIGN KEY ([TimeCard_Id], [TimeCard_ClockIn])
    REFERENCES [dbo].[TimeCards]
        ([Id], [ClockIn])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TimesheetEntryTimeCard'
CREATE INDEX [IX_FK_TimesheetEntryTimeCard]
ON [dbo].[TimesheetEntries]
    ([TimeCard_Id], [TimeCard_ClockIn]);
GO

-- Creating foreign key on [Roster_Id] in table 'TimeCards'
ALTER TABLE [dbo].[TimeCards]
ADD CONSTRAINT [FK_RosterTimeCard]
    FOREIGN KEY ([Roster_Id])
    REFERENCES [dbo].[Rosters]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_RosterTimeCard'
CREATE INDEX [IX_FK_RosterTimeCard]
ON [dbo].[TimeCards]
    ([Roster_Id]);
GO

-- Creating foreign key on [IndustryTypesID] in table 'UserProfiles'
ALTER TABLE [dbo].[UserProfiles]
ADD CONSTRAINT [FK_IndustryTypesUserProfile]
    FOREIGN KEY ([IndustryTypesID])
    REFERENCES [dbo].[IndustryTypes]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_IndustryTypesUserProfile'
CREATE INDEX [IX_FK_IndustryTypesUserProfile]
ON [dbo].[UserProfiles]
    ([IndustryTypesID]);
GO

-- Creating foreign key on [UserProfileId] in table 'WorkHistories'
ALTER TABLE [dbo].[WorkHistories]
ADD CONSTRAINT [FK_UserProfileWorkHistory]
    FOREIGN KEY ([UserProfileId])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfileWorkHistory'
CREATE INDEX [IX_FK_UserProfileWorkHistory]
ON [dbo].[WorkHistories]
    ([UserProfileId]);
GO

-- Creating foreign key on [IndustryTypesID] in table 'Qualificationslookups'
ALTER TABLE [dbo].[Qualificationslookups]
ADD CONSTRAINT [FK_IndustryTypestblQualificationslookup]
    FOREIGN KEY ([IndustryTypesID])
    REFERENCES [dbo].[IndustryTypes]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_IndustryTypestblQualificationslookup'
CREATE INDEX [IX_FK_IndustryTypestblQualificationslookup]
ON [dbo].[Qualificationslookups]
    ([IndustryTypesID]);
GO

-- Creating foreign key on [QualificationslookupQualificationId] in table 'UserQualifications'
ALTER TABLE [dbo].[UserQualifications]
ADD CONSTRAINT [FK_tblQualificationslookuptblUserQualification]
    FOREIGN KEY ([QualificationslookupQualificationId])
    REFERENCES [dbo].[Qualificationslookups]
        ([QualificationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tblQualificationslookuptblUserQualification'
CREATE INDEX [IX_FK_tblQualificationslookuptblUserQualification]
ON [dbo].[UserQualifications]
    ([QualificationslookupQualificationId]);
GO

-- Creating foreign key on [UserProfileId] in table 'UserQualifications'
ALTER TABLE [dbo].[UserQualifications]
ADD CONSTRAINT [FK_UserProfiletblUserQualification]
    FOREIGN KEY ([UserProfileId])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfiletblUserQualification'
CREATE INDEX [IX_FK_UserProfiletblUserQualification]
ON [dbo].[UserQualifications]
    ([UserProfileId]);
GO

-- Creating foreign key on [UserProfileId] in table 'OtherQualifications'
ALTER TABLE [dbo].[OtherQualifications]
ADD CONSTRAINT [FK_UserProfileOtherQualification]
    FOREIGN KEY ([UserProfileId])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfileOtherQualification'
CREATE INDEX [IX_FK_UserProfileOtherQualification]
ON [dbo].[OtherQualifications]
    ([UserProfileId]);
GO

-- Creating foreign key on [IndustryTypesID] in table 'IndustrySkills'
ALTER TABLE [dbo].[IndustrySkills]
ADD CONSTRAINT [FK_IndustryTypesIndustrialSkills]
    FOREIGN KEY ([IndustryTypesID])
    REFERENCES [dbo].[IndustryTypes]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_IndustryTypesIndustrialSkills'
CREATE INDEX [IX_FK_IndustryTypesIndustrialSkills]
ON [dbo].[IndustrySkills]
    ([IndustryTypesID]);
GO

-- Creating foreign key on [UserProfileId] in table 'UserSkills'
ALTER TABLE [dbo].[UserSkills]
ADD CONSTRAINT [FK_UserProfileUserSkills]
    FOREIGN KEY ([UserProfileId])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfileUserSkills'
CREATE INDEX [IX_FK_UserProfileUserSkills]
ON [dbo].[UserSkills]
    ([UserProfileId]);
GO

-- Creating foreign key on [IndustrySkillsId] in table 'UserSkills'
ALTER TABLE [dbo].[UserSkills]
ADD CONSTRAINT [FK_IndustrySkillsUserSkills]
    FOREIGN KEY ([IndustrySkillsId])
    REFERENCES [dbo].[IndustrySkills]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_IndustrySkillsUserSkills'
CREATE INDEX [IX_FK_IndustrySkillsUserSkills]
ON [dbo].[UserSkills]
    ([IndustrySkillsId]);
GO

-- Creating foreign key on [QualificationslookupQualificationId] in table 'ExternalShiftQualifications'
ALTER TABLE [dbo].[ExternalShiftQualifications]
ADD CONSTRAINT [FK_QualificationslookupShiftQualification]
    FOREIGN KEY ([QualificationslookupQualificationId])
    REFERENCES [dbo].[Qualificationslookups]
        ([QualificationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_QualificationslookupShiftQualification'
CREATE INDEX [IX_FK_QualificationslookupShiftQualification]
ON [dbo].[ExternalShiftQualifications]
    ([QualificationslookupQualificationId]);
GO

-- Creating foreign key on [ExternalShiftBroadcastId] in table 'Shifts'
ALTER TABLE [dbo].[Shifts]
ADD CONSTRAINT [FK_ExternalShiftBroadcastShift]
    FOREIGN KEY ([ExternalShiftBroadcastId])
    REFERENCES [dbo].[ExternalShiftBroadcasts]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ExternalShiftBroadcastShift'
CREATE INDEX [IX_FK_ExternalShiftBroadcastShift]
ON [dbo].[Shifts]
    ([ExternalShiftBroadcastId]);
GO

-- Creating foreign key on [ExternalShiftBroadcastId] in table 'ExternalShiftQualifications'
ALTER TABLE [dbo].[ExternalShiftQualifications]
ADD CONSTRAINT [FK_ExternalShiftBroadcastShiftQualification]
    FOREIGN KEY ([ExternalShiftBroadcastId])
    REFERENCES [dbo].[ExternalShiftBroadcasts]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ExternalShiftBroadcastShiftQualification'
CREATE INDEX [IX_FK_ExternalShiftBroadcastShiftQualification]
ON [dbo].[ExternalShiftQualifications]
    ([ExternalShiftBroadcastId]);
GO

-- Creating foreign key on [ExternalShiftBroadcastId] in table 'ExternalShiftSkills'
ALTER TABLE [dbo].[ExternalShiftSkills]
ADD CONSTRAINT [FK_ExternalShiftBroadcastShiftSkills]
    FOREIGN KEY ([ExternalShiftBroadcastId])
    REFERENCES [dbo].[ExternalShiftBroadcasts]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ExternalShiftBroadcastShiftSkills'
CREATE INDEX [IX_FK_ExternalShiftBroadcastShiftSkills]
ON [dbo].[ExternalShiftSkills]
    ([ExternalShiftBroadcastId]);
GO

-- Creating foreign key on [IndustrySkillsId] in table 'ExternalShiftSkills'
ALTER TABLE [dbo].[ExternalShiftSkills]
ADD CONSTRAINT [FK_IndustrySkillsShiftSkills]
    FOREIGN KEY ([IndustrySkillsId])
    REFERENCES [dbo].[IndustrySkills]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_IndustrySkillsShiftSkills'
CREATE INDEX [IX_FK_IndustrySkillsShiftSkills]
ON [dbo].[ExternalShiftSkills]
    ([IndustrySkillsId]);
GO

-- Creating foreign key on [ActionedBy_Id] in table 'ExternalShiftRequests'
ALTER TABLE [dbo].[ExternalShiftRequests]
ADD CONSTRAINT [FK_ExternalShiftRequestEmployee1]
    FOREIGN KEY ([ActionedBy_Id])
    REFERENCES [dbo].[Employees]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ExternalShiftRequestEmployee1'
CREATE INDEX [IX_FK_ExternalShiftRequestEmployee1]
ON [dbo].[ExternalShiftRequests]
    ([ActionedBy_Id]);
GO

-- Creating foreign key on [ExternalShiftBroadcast_Id] in table 'ExternalShiftRequests'
ALTER TABLE [dbo].[ExternalShiftRequests]
ADD CONSTRAINT [FK_ExternalShiftRequestExternalShiftBroadcast]
    FOREIGN KEY ([ExternalShiftBroadcast_Id])
    REFERENCES [dbo].[ExternalShiftBroadcasts]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ExternalShiftRequestExternalShiftBroadcast'
CREATE INDEX [IX_FK_ExternalShiftRequestExternalShiftBroadcast]
ON [dbo].[ExternalShiftRequests]
    ([ExternalShiftBroadcast_Id]);
GO

-- Creating foreign key on [CreatedBy_Id] in table 'ExternalShiftRequests'
ALTER TABLE [dbo].[ExternalShiftRequests]
ADD CONSTRAINT [FK_UserProfileExternalShiftRequest]
    FOREIGN KEY ([CreatedBy_Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfileExternalShiftRequest'
CREATE INDEX [IX_FK_UserProfileExternalShiftRequest]
ON [dbo].[ExternalShiftRequests]
    ([CreatedBy_Id]);
GO

-- Creating foreign key on [IndustryType_ID] in table 'BusinessTypes'
ALTER TABLE [dbo].[BusinessTypes]
ADD CONSTRAINT [FK_IndustryTypesBusinessType]
    FOREIGN KEY ([IndustryType_ID])
    REFERENCES [dbo].[IndustryTypes]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_IndustryTypesBusinessType'
CREATE INDEX [IX_FK_IndustryTypesBusinessType]
ON [dbo].[BusinessTypes]
    ([IndustryType_ID]);
GO

-- Creating foreign key on [UserProfileUserRecommendedBy_Id] in table 'UserRecommendations'
ALTER TABLE [dbo].[UserRecommendations]
ADD CONSTRAINT [FK_UserRecommendationsUserProfile_UserRecommendedBy]
    FOREIGN KEY ([UserProfileUserRecommendedBy_Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserRecommendationsUserProfile_UserRecommendedBy'
CREATE INDEX [IX_FK_UserRecommendationsUserProfile_UserRecommendedBy]
ON [dbo].[UserRecommendations]
    ([UserProfileUserRecommendedBy_Id]);
GO

-- Creating foreign key on [UserProfileUserRecommended_Id] in table 'UserRecommendations'
ALTER TABLE [dbo].[UserRecommendations]
ADD CONSTRAINT [FK_UserRecommendationsUserProfile_UserRecommended]
    FOREIGN KEY ([UserProfileUserRecommended_Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserRecommendationsUserProfile_UserRecommended'
CREATE INDEX [IX_FK_UserRecommendationsUserProfile_UserRecommended]
ON [dbo].[UserRecommendations]
    ([UserProfileUserRecommended_Id]);
GO

-- Creating foreign key on [UserSkill_UserSkillId] in table 'UserSkillEndorsements'
ALTER TABLE [dbo].[UserSkillEndorsements]
ADD CONSTRAINT [FK_UserSkillEndorsementsUserSkills]
    FOREIGN KEY ([UserSkill_UserSkillId])
    REFERENCES [dbo].[UserSkills]
        ([UserSkillId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserSkillEndorsementsUserSkills'
CREATE INDEX [IX_FK_UserSkillEndorsementsUserSkills]
ON [dbo].[UserSkillEndorsements]
    ([UserSkill_UserSkillId]);
GO

-- Creating foreign key on [UserProfileUserRecommendedBY_Id] in table 'UserSkillEndorsements'
ALTER TABLE [dbo].[UserSkillEndorsements]
ADD CONSTRAINT [FK_UserSkillEndorsementsUserProfile_UserRecommendedBY]
    FOREIGN KEY ([UserProfileUserRecommendedBY_Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserSkillEndorsementsUserProfile_UserRecommendedBY'
CREATE INDEX [IX_FK_UserSkillEndorsementsUserProfile_UserRecommendedBY]
ON [dbo].[UserSkillEndorsements]
    ([UserProfileUserRecommendedBY_Id]);
GO

-- Creating foreign key on [UserProfileUserBeingRecommended_Id] in table 'UserSkillEndorsements'
ALTER TABLE [dbo].[UserSkillEndorsements]
ADD CONSTRAINT [FK_UserSkillEndorsementsUserProfile_UserBeingRecommended]
    FOREIGN KEY ([UserProfileUserBeingRecommended_Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserSkillEndorsementsUserProfile_UserBeingRecommended'
CREATE INDEX [IX_FK_UserSkillEndorsementsUserProfile_UserBeingRecommended]
ON [dbo].[UserSkillEndorsements]
    ([UserProfileUserBeingRecommended_Id]);
GO

-- Creating foreign key on [UserProfilesMessageFrom_Id] in table 'Messages'
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [FK_MessagesUserProfile_MessagesFrom]
    FOREIGN KEY ([UserProfilesMessageFrom_Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_MessagesUserProfile_MessagesFrom'
CREATE INDEX [IX_FK_MessagesUserProfile_MessagesFrom]
ON [dbo].[Messages]
    ([UserProfilesMessageFrom_Id]);
GO

-- Creating foreign key on [UserProfileMessageTo_Id] in table 'Messages'
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [FK_MessagesUserProfile_MessagesTo]
    FOREIGN KEY ([UserProfileMessageTo_Id])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_MessagesUserProfile_MessagesTo'
CREATE INDEX [IX_FK_MessagesUserProfile_MessagesTo]
ON [dbo].[Messages]
    ([UserProfileMessageTo_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------