using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Thinktecture.IdentityModel;
using WebUI.Common;
using WebUI.DTOs;
using WebUI.Models;

namespace WebUI.Controllers.API
{
    public class EmployeeActionAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();


        [ActionName("GetEmployeeRoles")]
        public IEnumerable<RoleDTO> GetEmployeeRoles(Guid Id)
        {
            var employee = db.Employees.Find(Id);

            if (employee == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            return MapperFacade.MapperConfiguration.Map<IEnumerable<Role>, IEnumerable<RoleDTO>>(employee.Roles).ToList();
        }

        [ActionName("GetEmployeesInRole")]
        public IEnumerable<EmployeeSummaryDTO> GetEmployeesInRole(Guid Id)
        {
            var role = db.Roles.Find(Id);
            if (role == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            // var employees = db.Employees.Where(e => e.Roles.);

            return MapperFacade.MapperConfiguration.Map<IEnumerable<Employee>, IEnumerable<EmployeeSummaryDTO>>(role.Employee.AsEnumerable());
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}