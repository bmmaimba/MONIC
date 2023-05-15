using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB.Models
{
    public class Settings
    {
        [Key, Required]
        public Guid Id { get; set; }

        [MaxLength(50)]
        public string TestSetting { get; set; }

        public Settings()
        {
            Id = Guid.NewGuid();
        }

        public override string ToString()
        {
            return Convert.ToString(Id);
        }
    }
}
