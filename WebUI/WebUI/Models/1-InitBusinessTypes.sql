
-- --------------------------------------------------
-- Populate the static data for business types
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO

-- --------------------------------------------------
-- START: Populate Industry Types
-- --------------------------------------------------
SET IDENTITY_INSERT [dbo].[IndustryTypes] ON

INSERT INTO [dbo].[IndustryTypes] ([ID],[Name]) VALUES (1,'Hospitality');
INSERT INTO [dbo].[IndustryTypes] ([ID],[Name]) VALUES (2,'Retail');

SET IDENTITY_INSERT [dbo].[IndustryTypes] OFF

-- --------------------------------------------------
-- END: Populate Industry Types
-- --------------------------------------------------

-- --------------------------------------------------
-- START: Populate Business Types
-- --------------------------------------------------
SET IDENTITY_INSERT [dbo].[BusinessTypes] ON

DECLARE @HospitalityId int
SELECT @HospitalityId = ID from [dbo].[IndustryTypes] where Name = 'Hospitality'

DECLARE @RetailId int
SELECT @RetailId = ID from [dbo].[IndustryTypes] where Name = 'Retail'

INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (1,@HospitalityId, 'Café');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (2,@HospitalityId, 'Bar');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (3,@HospitalityId, 'Restaurant');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (4,@HospitalityId, 'Hotel');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (5,@HospitalityId, 'Fast-food');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (6,@HospitalityId, 'Supermarket');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (7,@HospitalityId, 'Fine dining');

INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (20,@RetailId, 'Convenience store');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (21,@RetailId, 'Department Store');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (22,@RetailId, 'Womens fashion');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (23,@RetailId, 'Mens fashion');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (24,@RetailId, 'Womens fashion - Designer');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (25,@RetailId, 'Mens fashion- Designer');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (26,@RetailId, 'Shoes');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (27,@RetailId, 'Childrens fashion');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (28,@RetailId, 'Toy store');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (29,@RetailId, 'Electronics');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (30,@RetailId, 'Flowers and gifts');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (31,@RetailId, 'Home and outdoors');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (32,@RetailId, 'Bags and luggage');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (33,@RetailId, 'Hardware');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (34,@RetailId, 'Beauty and skincare');
INSERT INTO [dbo].[BusinessTypes] ([Id],[IndustryType_ID],[Detail]) VALUES (35,@RetailId, 'Pharmacy and Health');


SET IDENTITY_INSERT [dbo].[BusinessTypes] OFF

-- --------------------------------------------------
-- END: Populate Business Types
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------