using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using WebUI.DTOs;
using WebUI.Models;


namespace WebUI
{
    public static class AutoMapperConfig
    {
        public static void Configure()
        {
            var config = new MapperConfiguration(cfg => {
                //cfg.CreateMap<Source, Destination>
                cfg.CreateMap<RegisterModelDTO, UserProfile>()
                 .ForMember(dest => dest.Address, opt => opt.ResolveUsing(src =>
                 {
                     return new Address()
                     {
                         Line1 = src.Address.Line1,
                         Line2 = src.Address.Line2,
                         Suburb = src.Address.Suburb,
                         Postcode = src.Address.Postcode,
                         State = src.Address.State,
                         PlaceLatitude = src.Address.Lat,
                         PlaceLongitude = src.Address.Long,
                         PlaceId = src.Address.PlaceId
                     };
                 }))
                    .ForMember(src => src.Membership, opt => opt.Ignore())
                    .ForMember(src => src.security_Roles, opt => opt.Ignore())
                    .ForMember(src => src.Employees, opt => opt.Ignore())
                    .ForMember(src => src.EmployerRequests, opt => opt.Ignore())
                    .ForMember(src => src.RecurringCalendarEvents, opt => opt.Ignore())
                    .ForMember(src => src.EmployeeRequests, opt => opt.Ignore())
                .ReverseMap()
                    .ForSourceMember(src => src.Address, opt => opt.Ignore())
                    .ForMember(src => src.Password, opt => opt.Ignore())
                    .ForMember(src => src.ConfirmPassword, opt => opt.Ignore());

            cfg.CreateMap<Role, RoleDTO>()
                .ForMember(dest => dest.BusinessId, opt => opt.ResolveUsing(src => src.Business.Id))
                .ReverseMap()
                    .ForSourceMember(src => src.BusinessId, opt => opt.Ignore())
                    .ForMember(dest => dest.Business, opt => opt.Ignore())
                    .ForMember(dest => dest.Employee, opt => opt.Ignore());

            cfg.CreateMap<InternalLocation, InternalLocationDTO>()
                    .ForMember(dest => dest.BusinessId, opt => opt.ResolveUsing(src => src.BusinessLocation.Business.Id))
                     .ForMember(dest => dest.BusinessLocationId, opt => opt.ResolveUsing(src => src.BusinessLocation.Id))
                .ReverseMap()
                   .ForMember(dest => dest.BusinessLocation, opt => opt.Ignore());

            cfg.CreateMap<Employee, EmployeeDTO>()
                .ForMember(dest => dest.BusinessId, opt => opt.ResolveUsing(src => src.BusinessLocation.Business.Id))
                .ForMember(dest => dest.BusinessLocationId, opt => opt.ResolveUsing(src => src.BusinessLocation.Id))
                .ForMember(dest => dest.FullName, opt => opt.Ignore())
                .ReverseMap()
                    .ForSourceMember(src => src.BusinessId, opt => opt.Ignore())
                    .ForMember(dest => dest.BusinessLocation, opt => opt.Ignore());
            cfg.CreateMap<BusinessLocation, BusinessLocationDTO>()
                 .ForMember(dest => dest.Address, opt => opt.ResolveUsing(src =>
                 {
                     return new AddressModelDTO()
                     {
                         Line1 = src.Address.Line1,
                         Line2 = src.Address.Line2,
                         Suburb = src.Address.Suburb,
                         Postcode = src.Address.Postcode,
                         State = src.Address.State,
                         Lat = src.Address.PlaceLatitude,
                         Long = src.Address.PlaceLongitude,
                         PlaceId = src.Address.PlaceId
                     };
                 }))
               .ReverseMap()
                .ForMember(dest => dest.Address, opt => opt.ResolveUsing(src =>
                   {
                       return new Address()
                       {
                           Line1 = src.Address.Line1,
                           Line2 = src.Address.Line2,
                           Suburb = src.Address.Suburb,
                           Postcode = src.Address.Postcode,
                           State = src.Address.State,
                           PlaceLatitude = src.Address.Lat,
                           PlaceLongitude = src.Address.Long,
                           PlaceId = src.Address.PlaceId
                       };
                   }))
                   .ForMember(dest => dest.InternalLocations, opt => opt.Ignore())
                   .ForMember(dest => dest.ShiftTemplates, opt => opt.Ignore())
                   .ForMember(dest => dest.ShiftBlocks, opt => opt.Ignore())
                   .ForMember(dest => dest.BusinessPreferences, opt => opt.Ignore());

            cfg.CreateMap<Address, AddressModelDTO>()
               .ReverseMap();

            cfg.CreateMap<Business, BusinessDTO>();
            //.ReverseMap()
            //    .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src =>
            //    {
            //        return new BusinessType()
            //        {
            //            Id = src.TypeId,
            //            Industry = src.TypeIndustry,
            //            Detail = src.TypeDetail
            //        };
            //    }))
            //    .ForMember(dest => dest.Roles, opt => opt.Ignore());

            cfg.CreateMap<BusinessPreferencesDTO, BusinessPreferences>()
               .ForMember(dest => dest.BusinessLocation, opt => opt.Ignore())
               .ReverseMap()
               .ForMember(dest => dest.BusinessId, opt => opt.ResolveUsing(src => src.BusinessLocation.Business.Id));

            cfg.CreateMap<BusinessTypeDTO, BusinessType>()
                .ReverseMap()
                  .ForMember(dest => dest.Industry, opt => opt.ResolveUsing(src => src.IndustryType.Name));

            cfg.CreateMap<ShiftTemplateDTO, ShiftTemplate>()
                 .ForMember(dest => dest.BusinessLocation, opt => opt.Ignore())
                 .ForMember(dest => dest.Role, opt => opt.Ignore())
                 .ForMember(dest => dest.InternalLocation, opt => opt.Ignore())
                 .ForMember(dest => dest.Employee, opt => opt.Ignore())
               .ReverseMap()
                .ForMember(dest => dest.BusinessId, opt => opt.ResolveUsing(src => src.BusinessLocation.Business.Id))
                .ForMember(dest => dest.BusinessName, opt => opt.ResolveUsing(src => src.BusinessLocation.Business.Name));

            cfg.CreateMap<RosterDTO, Roster>()
                 .ForMember(dest => dest.BusinessLocation, opt => opt.Ignore())
                 .ForMember(dest => dest.WeekStartDate, opt => opt.ResolveUsing(src => src.WeekStartDate.Date))
              .ReverseMap()
                 .ForMember(dest => dest.BusinessId, opt => opt.ResolveUsing(src => src.BusinessLocation.Business.Id));

            cfg.CreateMap<RosterCreateDTO, Roster>()
                .ForMember(dest => dest.WeekStartDate, opt => opt.ResolveUsing(src => src.WeekStartDate.Date))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Shifts, opt => opt.Ignore())
                .ForMember(dest => dest.BusinessLocation, opt => opt.Ignore());

            cfg.CreateMap<ShiftDTO, Shift>()
                 .ForMember(dest => dest.StartTime, opt => opt.ResolveUsing(src => src.StartDateTime))
                 .ForMember(dest => dest.FinishTime, opt => opt.ResolveUsing(src => src.FinishDateTime))
                .ForMember(dest => dest.Roster, opt => opt.Ignore())
                .ForMember(dest => dest.Employee, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.InternalLocation, opt => opt.Ignore())
                .ForMember(dest => dest.ShiftChangeRequests, opt => opt.Ignore())
                .ReverseMap()
                    .ForMember(dest => dest.StartDay, opt => opt.Ignore())
                    .ForMember(dest => dest.StartTime, opt => opt.Ignore())
                    .ForMember(dest => dest.FinishDay, opt => opt.Ignore())
                    .ForMember(dest => dest.FinishTime, opt => opt.Ignore())
                    .ForMember(dest => dest.StartDateTime, opt => opt.ResolveUsing(src => src.StartTime))
                    .ForMember(dest => dest.FinishDateTime, opt => opt.ResolveUsing(src => src.FinishTime))
                    .ForMember(dest => dest.BusinessId, opt => opt.ResolveUsing(src => src.Roster.BusinessLocation.Business.Id))
                    .ForMember(dest => dest.BusinessName, opt => opt.ResolveUsing(src => src.Roster.BusinessLocation.Business.Name))
                    .ForMember(dest => dest.BusinessLocationId, opt => opt.ResolveUsing(src => src.Roster.BusinessLocation.Id))
                    .ForMember(dest => dest.BusinessLocationName, opt => opt.ResolveUsing(src => src.Roster.BusinessLocation.Name));

            cfg.CreateMap<Employee, EmployeeSummaryDTO>();

            cfg.CreateMap<ShiftChangeRequestDTO, ShiftChangeRequest>()
                 .ForMember(dest => dest.ActionedDate, opt => opt.Ignore())
                 .ForMember(dest => dest.ActionedComment, opt => opt.Ignore())
                 .ForMember(dest => dest.Shift, opt => opt.Ignore())
                 .ForMember(dest => dest.ActionedBy, opt => opt.Ignore())
              .ReverseMap()
                .ForMember(dest => dest.ShiftTemplateId, opt => opt.ResolveUsing(src => src.Shift.ShiftTemplate != null ? src.Shift.ShiftTemplate.Id : Guid.Empty))
                .ForMember(dest => dest.StartDateTime, opt => opt.ResolveUsing(src => src.Shift.StartTime))
                .ForMember(dest => dest.FinishDateTime, opt => opt.ResolveUsing(src => src.Shift.FinishTime));

            cfg.CreateMap<ScheduleDTO, Schedule>()
                .ForMember(dest => dest.UserProfile, opt => opt.Ignore())
             .ReverseMap()
              .ForMember(dest => dest.Frequency, opt => opt.Ignore())
              .ForMember(dest => dest.FrequencyChoice, opt => opt.ResolveUsing(src => src.Frequency));

            cfg.CreateMap<EmployeeRequestDTO, EmployeeRequest>()
                 .ForMember(dest => dest.ActionedDate, opt => opt.Ignore())
                 .ForMember(dest => dest.BusinessLocation, opt => opt.Ignore())
                 .ForMember(dest => dest.Employee, opt => opt.Ignore())
                 .ForMember(dest => dest.ActionedBy, opt => opt.Ignore())
             .ReverseMap();

            cfg.CreateMap<ShiftBlockDTO, ShiftBlock>()
                 .ForMember(dest => dest.Role, opt => opt.Ignore())
                 .ForMember(dest => dest.BusinessLocation, opt => opt.Ignore())
            .ReverseMap();

            cfg.CreateMap<UserPreferencesDTO, UserPreferences>()
           .ReverseMap();
            //.ForMember(dest => dest.UserId, opt => opt.ResolveUsing(src => src.UserProfile.Id))
            // .ForMember(dest => dest.Email, opt => opt.ResolveUsing(src => src.UserProfile.Email));

            cfg.CreateMap<BusinessDetailsDTO, Business>()
                .ForMember(dest => dest.BusinessLocations, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.Ignore());
            //{
            //    return new BusinessType()
            //    {
            //        Id = src.TypeId,
            //        Industry = src.TypeIndustry,
            //        Detail = src.TypeDetail
            //    };
            //}));

            cfg.CreateMap<BusinessDTO, BusinessDetailsDTO>();

            cfg.CreateMap<UserSkillEndorsements, UserSkillEndorsementDTO>();
            cfg.CreateMap<RecurringShiftChangeRequestDTO, RecurringShiftChangeRequest>()
        .ReverseMap()
            .ForMember(dest => dest.StartTime, opt => opt.ResolveUsing(src => src.ShiftTemplate.StartTime))
            .ForMember(dest => dest.FinishTime, opt => opt.ResolveUsing(src => src.ShiftTemplate.FinishTime));

            cfg.CreateMap<PaymentDetailsDTO, PaymentDetails>()
                .ReverseMap();


            cfg.CreateMap<TimeCardDTO, TimeCard>()
                .ReverseMap()
                 .ForMember(dest => dest.TimesheetId, opt => opt.ResolveUsing(src => src.Roster.Timesheet.Id));

            cfg.CreateMap<TimesheetDTO, Timesheet>()
               .ReverseMap()
                .ForMember(dest => dest.WeekStartDate, opt => opt.ResolveUsing(src => src.Roster.WeekStartDate))
                .ForMember(dest => dest.WeekEndDate, opt => opt.ResolveUsing(src => src.Roster.WeekStartDate.AddDays(6)));

            cfg.CreateMap<TimesheetWeekDTO, Timesheet>()
             .ReverseMap()
              .ForMember(dest => dest.WeekStartDate, opt => opt.ResolveUsing(src => src.Roster.WeekStartDate))
              .ForMember(dest => dest.BusinessId, opt => opt.ResolveUsing(src => src.Roster.BusinessLocation.Business.Id))
              .ForMember(dest => dest.BusinessLocationId, opt => opt.ResolveUsing(src => src.Roster.BusinessLocation.Id))
              .ForMember(dest => dest.BusinessLocationName, opt => opt.ResolveUsing(src => src.Roster.BusinessLocation.Name));

            cfg.CreateMap<TimesheetEntryDTO, TimesheetEntry>()
              .ReverseMap()
               .ForMember(dest => dest.StartDay, opt => opt.Ignore())
                    .ForMember(dest => dest.StartTime, opt => opt.Ignore())
                    .ForMember(dest => dest.FinishDay, opt => opt.Ignore())
                    .ForMember(dest => dest.TimesheetId, opt => opt.ResolveUsing(src => src.TimeCard.Roster.Timesheet.Id))
                    .ForMember(dest => dest.FinishTime, opt => opt.Ignore());

            cfg.CreateMap<WorkHistory, WorkHistoryDTO>()
                .ForMember(dest => dest.UserProfile, opt => opt.Ignore());

            cfg.CreateMap<OtherQualification, OtherQualificationDTO>()
                .ForMember(dest => dest.UserProfile, opt => opt.Ignore());

            cfg.CreateMap<UserQualification, UserQualificationDTO>();

            cfg.CreateMap<ExternalShiftBroadcast, ExternalBroadcastDTO>()
                .ForMember(dest => dest.BusinessId, opt => opt.ResolveUsing(src => src.Shifts.First().Roster.BusinessLocation.Business.Id))
                .ForMember(dest => dest.BusinessName, opt => opt.ResolveUsing(src => src.Shifts.First().Roster.BusinessLocation.Business.Name))
                .ForMember(dest => dest.BusinessLocationId, opt => opt.ResolveUsing(src => src.Shifts.First().Roster.BusinessLocation.Id))
                .ForMember(dest => dest.BusinessLocationName, opt => opt.ResolveUsing(src => src.Shifts.First().Roster.BusinessLocation.Name))
                 .ForMember(dest => dest.BusinessLocationAddress, opt => opt.ResolveUsing(src =>
                 {
                     return new AddressModelDTO()
                     {
                         Line1 = src.Shifts.First().Roster.BusinessLocation.Address.Line1,
                         Line2 = src.Shifts.First().Roster.BusinessLocation.Address.Line2,
                         Suburb = src.Shifts.First().Roster.BusinessLocation.Address.Suburb,
                         Postcode = src.Shifts.First().Roster.BusinessLocation.Address.Postcode,
                         State = src.Shifts.First().Roster.BusinessLocation.Address.State,
                         Lat = src.Shifts.First().Roster.BusinessLocation.Address.PlaceLatitude,
                         Long = src.Shifts.First().Roster.BusinessLocation.Address.PlaceLongitude,
                         PlaceId = src.Shifts.First().Roster.BusinessLocation.Address.PlaceId
                     };
                 }));

            });
            MapperFacade.MapperConfiguration = config.CreateMapper();

        }
    }
}