using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public class InternalLocationDTO
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public Guid BusinessLocationId { get; set; }
        [Required]
        [MaxLength(40)]
        public string Name { get; set; }

        public bool Enabled { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is InternalLocationDTO)
            {
                var objRoleDTO = obj as InternalLocationDTO;
                return (objRoleDTO.Id == this.Id &&
                        objRoleDTO.BusinessId == this.BusinessId &&
                        objRoleDTO.Name == this.Name );

            }
            else
                return base.Equals(obj);
        }
    }
}