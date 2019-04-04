using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public class RoleDTO
    {
       // [Required]
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        [Required]
        public string Name { get; set; }
        
        public bool Enabled { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is RoleDTO)
            {
                var objRoleDTO = obj as RoleDTO;
                return (objRoleDTO.Id == this.Id &&
                        objRoleDTO.BusinessId == this.BusinessId &&
                        objRoleDTO.Name == this.Name);

            }
            else
                return base.Equals(obj);
        }
    }
}