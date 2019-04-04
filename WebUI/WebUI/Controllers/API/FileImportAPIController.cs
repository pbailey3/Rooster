using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebUI.DTOs;
using System.Text;
using System.Text.RegularExpressions;
using WebUI.Models;
using AutoMapper;
using WebUI.Common;
using Thinktecture.IdentityModel;

namespace WebUI.Controllers.API
{
    [Authorize]
    public class FileImportAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        // POST: api/FileImportAPI
        public LogFileDTO PostFile(FileImportDTO filedto)
        {
            if (ModelState.IsValid)
            {
                if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", filedto.BusinessLocationId.ToString()))
                {
                    MemoryStream stream = new MemoryStream(filedto.FileUpload);
                   
                    if (filedto.DataType == UploadTypesDTO.Employees)
                    {
                        Tuple<List<EmployeeDTO>, LogFileDTO> result = ValidateCSV(stream, filedto.BusinessLocationId);

                        if (result.Item2.ErrorLines == 0)
                        {
                            foreach (EmployeeDTO employeeDTO in result.Item1)
                            {
                                employeeDTO.BusinessId = filedto.BusinessId;
                                employeeDTO.BusinessLocationId = filedto.BusinessLocationId;

                                using (var employeeAPIController = new EmployeeAPIController())
                                {
                                    employeeAPIController.CreateNewEmployee(employeeDTO, true);
                                }

                                result.Item2.LoadedSuccesfully++;
                            }
                        }
                        return result.Item2;
                    }
                    else if (filedto.DataType == UploadTypesDTO.Roles)
                    {
                        throw new NotImplementedException();
                    }
                    else 
                    {
                        return new LogFileDTO
                        {
                            Status = "Failed",
                            LinesRead = 0
                        };

                    }
                }
                else
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have appropriate permission"));
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
        }

        private Tuple<List<EmployeeDTO>, LogFileDTO> ValidateCSV(MemoryStream stream, Guid businessLocationId)
        {
            LogFileDTO result = new LogFileDTO();
            List<string> mandatoryHeader = new List<string>() { "FirstName", "LastName", "Mobile", "Email", "Type", "Admin", "Roles" };
            List<EmployeeDTO> foundEmployees = new List<EmployeeDTO>();

            using (StreamReader reader = new StreamReader(stream))
            {
                string line = String.Empty;
                bool correct = true;
                List<string> headers = new List<string>();
                Tuple<bool, EmployeeDTO, string> lineEmployee;

                //Read Line 1, the Header Row
                if ((line = reader.ReadLine()) != null)
                {
                    result.LinesRead++;
                    headers = line.Split(',').ToList();
                    foreach (string head in mandatoryHeader)
                    {
                        correct = headers.Contains(head);
                        if (correct == false)
                            break;
                    }
                    if (correct) //If correct headers exist, then continue to read data rows from Line 2 onwards
                    {
                        while ((line = reader.ReadLine()) != null)
                        {
                            result.LinesRead++;
                            lineEmployee = ValidRecord(line, mandatoryHeader.Count, mandatoryHeader, businessLocationId);
                            if (lineEmployee.Item1)
                            {
                                foundEmployees.Add(lineEmployee.Item2);
                                result.ValidLines++;
                            }
                            else
                            {
                                result.ErrorList.Add("Line " + result.LinesRead.ToString() + ": " + lineEmployee.Item3);
                                result.ErrorLines++;
                            }
                        }
                    }
                    else
                    {
                        result.ErrorList.Add("Line " + result.LinesRead.ToString() + ": Mandatory column headers is not valid.");
                        result.ErrorLines++;
                    }
                }
            }

            return new Tuple<List<EmployeeDTO>, LogFileDTO>(foundEmployees, result);
        }

        private Tuple<bool, EmployeeDTO, string> ValidRecord(string line, int numHeaders, List<string> mandatoryHeaders, Guid businessLocationId)
        {
            int i = 0;
            bool valid = true;
            string errorText = "";
            List<string> record = null;
            List<string> roles = new List<string>();
            EmployeeDTO employeeDTO = new EmployeeDTO();
            Regex regexLength = new Regex(@"(^\w{1,20}$)", RegexOptions.Compiled);
            Regex regexWhiteSpace = new Regex(@"^\s$", RegexOptions.Compiled);
            Regex regexAdmin = new Regex(@"\b(true|false)\b", RegexOptions.Compiled);
            Regex regexEmail = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", RegexOptions.Compiled);

            record = SplitLine(line, ',', '"');
            if (record.Count() == numHeaders)
            {
                if (String.IsNullOrWhiteSpace(record[mandatoryHeaders.IndexOf("FirstName")]) == false)
                {
                    if (String.IsNullOrWhiteSpace(record[mandatoryHeaders.IndexOf("Type")]) == false)
                    {
                        foreach (string column in record)
                        {
                            if (mandatoryHeaders[i].Equals("FirstName"))
                            {
                                if (regexLength.IsMatch(column))
                                {
                                    employeeDTO.FirstName = column;
                                }
                                else
                                {
                                    valid = false;
                                    errorText = "Length of \"FirstName\" header is out of range.";
                                    break;
                                }
                            }
                            else if (mandatoryHeaders[i].Equals("Type"))
                            {
                                if (Enum.IsDefined(typeof(EmployeeTypeDTO), column))
                                {
                                    employeeDTO.Type = (EmployeeTypeDTO)Enum.Parse(typeof(EmployeeTypeDTO), column, true);
                                }
                                else
                                {
                                    valid = false;
                                    errorText = "Value '" + column + "' not valid for \"Type\" header.";
                                    break;
                                }
                            }
                            else if (mandatoryHeaders[i].Equals("LastName"))
                            {
                                if (String.IsNullOrWhiteSpace(column) == false)
                                {
                                    if (regexLength.IsMatch(column))
                                    {
                                        employeeDTO.LastName = column;
                                    }
                                    else
                                    {
                                        valid = false;
                                        errorText = "Length of \"LastName\" header is out of range.";
                                        break;
                                    }
                                }
                            }
                            else if (mandatoryHeaders[i].Equals("Email"))
                            {
                                if (String.IsNullOrWhiteSpace(column) == false)
                                {
                                    if (regexEmail.IsMatch(column))
                                    {
                                        bool exist = db.Employees.Where(x => x.Email == column && x.BusinessLocation.Id == businessLocationId).Count() > 0;

                                        if (exist == false)
                                        {
                                            employeeDTO.Email = column;
                                        }
                                        else
                                        {
                                            valid = false;
                                            errorText = "User already exists with email address '" + column + "'";
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        valid = false;
                                        errorText = "Email address '" + column + "' is not valid.";
                                        break;
                                    }
                                }
                            }
                            else if (mandatoryHeaders[i].Equals("Admin"))
                            {
                                if (String.IsNullOrWhiteSpace(column) == false)
                                {
                                    if (regexAdmin.IsMatch(column))
                                    {
                                        employeeDTO.IsAdmin = bool.Parse(column);
                                    }
                                    else
                                    {
                                        valid = false;
                                        errorText = "Value '" + column + "' is not valid for \"Admin\" header.";
                                        break;
                                    }
                                }
                                else
                                {
                                    employeeDTO.IsAdmin = false;
                                }
                            }
                            else if (mandatoryHeaders[i].Equals("Mobile"))
                            {
                                if (String.IsNullOrWhiteSpace(column) == false)
                                {
                                    employeeDTO.MobilePhone = column;
                                }
                            }
                            else if (mandatoryHeaders[i].Equals("Roles"))
                            {
                                if (String.IsNullOrWhiteSpace(column) == false)
                                {
                                    employeeDTO.Roles = new List<RoleDTO>();
                                    foreach (var roleName in column.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList())
                                        employeeDTO.Roles.Add(new RoleDTO { Name = roleName });
                                }
                            }
                            else
                            {
                                valid = false;
                            }
                            i++;
                        }
                    }
                    else
                    {
                        valid = false;
                        errorText = "Mandatory header \"Type\" is missing.";
                    }
                }
                else
                {
                    valid = false;
                    errorText = "Mandatory header \"FirstName\" is missing.";
                }
            }
            else
            {
                valid = false;
                errorText = "Missing columns for this record.";
            }

            if (valid)
            {
                employeeDTO.AddAnother = false;
                employeeDTO.IsActive = true;
            }
            else
            {
                employeeDTO = null;
                roles = null;
            }

            return new Tuple<bool, EmployeeDTO, string>(valid, employeeDTO, errorText);
        }

        private List<string> SplitLine(string line, char delimiter, char quote)
        {
            StringBuilder builder = new StringBuilder();
            List<string> groups = new List<string>();
            bool inQuote = false;

            foreach (char character in line)
            {
                if (character == delimiter)
                {
                    if (inQuote)
                    {
                        builder.Append(character);
                    }
                    else
                    {
                        groups.Add(builder.ToString());
                        builder.Clear();
                    }
                }
                else if (character != delimiter)
                {
                    if (character == quote)
                    {
                        if (inQuote)
                        {
                            inQuote = false;
                            groups.Add(builder.ToString());
                            builder.Clear();
                        }
                        else
                        {
                            inQuote = true;
                        }
                    }
                    else
                    {
                        builder.Append(character);
                    }
                }
            }
            if (builder.Length != 0)
            {
                groups.Add(builder.ToString());
            }
            if (line.Last() == ',')
                groups.Add("");
            return groups;
        }
    }
}
