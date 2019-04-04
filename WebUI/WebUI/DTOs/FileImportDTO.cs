using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WebUI.DTOs
{
    public enum UploadTypesDTO
    {
        Employees = 0,
        Roles = 1
    }

    public enum FileTypesDTO
    {
        CSV = 0
    }

    public class FileImportDTO
    {
        [Required]
        [Display(Name = "Data type")]
        public UploadTypesDTO DataType { get; set; }

        [Required]
        [Display(Name = "File type")]
        public FileTypesDTO FileType { get; set; }

        [Required]
        [Display(Name = "File to upload")]
        public byte[] FileUpload { get; set; }

        public Guid BusinessId { get; set; }
        
        public Guid BusinessLocationId { get; set; }
    }
}
