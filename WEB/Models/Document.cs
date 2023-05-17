using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB.Models
{
    public class Document
    {
        [Key, Required]
        public Guid DocumentId { get; set; }

        [Required]
        public Guid ItemId { get; set; }

        [Required(AllowEmptyStrings = true), MaxLength(250)]
        public string FileName { get; set; }

        public byte[] FileContents { get; set; }

        public string Notes { get; set; }

        [Required]
        public DateTime UploadedOn { get; set; }

        public Guid? UploadedById { get; set; }

        [Required]
        public int Size { get; set; }

        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }

        [ForeignKey("UploadedById")]
        public virtual User UploadedBy { get; set; }

        public Document()
        {
            DocumentId = Guid.NewGuid();
        }

        public override string ToString()
        {
            return FileName;
        }
    }
}
