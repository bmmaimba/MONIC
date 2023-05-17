using System;
using System.ComponentModel.DataAnnotations;

namespace WEB.Models
{
    public class OptionDTO
    {
        [Required]
        public Guid OptionId { get; set; }

        [Required]
        public Guid FieldId { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false), MaxLength(2000)]
        public string Name { get; set; }

        [Required]
        public int SortOrder { get; set; }

        public FieldDTO Field { get; set; }

        public virtual List<OptionValueDTO> OptionValues { get; set; } = new List<OptionValueDTO>();

    }

    public static partial class ModelFactory
    {
        public static OptionDTO Create(Option option, bool includeParents = true, bool includeChildren = false)
        {
            if (option == null) return null;

            var optionDTO = new OptionDTO();

            optionDTO.OptionId = option.OptionId;
            optionDTO.FieldId = option.FieldId;
            optionDTO.Name = option.Name;
            optionDTO.SortOrder = option.SortOrder;

            if (includeParents)
            {
                optionDTO.Field = Create(option.Field);
            }

            if (includeChildren)
            {
                foreach (var optionValue in option.OptionValues)
                    optionDTO.OptionValues.Add(Create(optionValue));
            }

            return optionDTO;
        }

        public static void Hydrate(Option option, OptionDTO optionDTO)
        {
            option.FieldId = optionDTO.FieldId;
            option.Name = optionDTO.Name;
            option.SortOrder = optionDTO.SortOrder;
        }
    }
}
