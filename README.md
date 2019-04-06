# Rooster
Project Rooster is a rostering application which utilises the latest cloud based platform from Microsoft Windows Azure to provide an efficient solution for small/medium sized businesses to manage all their staff rostering needs.

## Application Overview
The Project Rooster application is intended to enhance the efficiency of small to medium businesses and their day to day rostering and staffing needs. Many of these businesses are still using manual processes in their routine rostering tasks; this might involve looking up staff availabilities and building up the rosters based on these availabilities and staff skills. Project Rooster aims to automate and simplify this process by allowing businesses to register staffing details and provide capabilities to enter staff availability, regular shift patterns, push notices of shift availability to suitable staff, link multiple branches of same organisation to share staff pools.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. 

### Database

Rooster has been designed to leverage a SQL Server Azure database, however the Open Source codebase 
has been written to leverage an inbuilt SQL Server Express instance in order to enable rapid startup and debugging of the application direct from a prebuilt Rooster.mdf file containing test data.

To set up your own databse on a SQL Server instance simply leverage the follow these steps

```
1) Create a DB instance on your preferred DBMS.
2) Run the SQL script "Models\DataModel.edmx.sql" against your database to create the required schema
3) For pre-populated test data run "Models\DataModelSEED.sql" OR to populate only the required reference data use "Models\1-InitBusinessTypes.sql""
```
