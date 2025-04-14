using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CoverMate.Model
{

    [AutoValidateAntiforgeryToken]
    public class NewRequest
    {
        [Required(ErrorMessage = "ScheduleId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ScheduleId must be a positive number.")]
        public int ScheduleId { get; set; }

        [Required(ErrorMessage = "Subplanlink is required.")]
        [StringLength(1000, ErrorMessage = "Subplanlink cannot exceed 1000 characters.")]
        public string Subplanlink { get; set; }

        [Required(ErrorMessage = "Reason is required.")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string Reason { get; set; }


        [Required(ErrorMessage = "Notes is required.")]
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters.")]
        public string Notes { get; set; }

        // RequestDate is required and must be today or a future date.
        [Required(ErrorMessage = "RequestDate is required.")]
        [DataType(DataType.DateTime)]
        [NotPast(ErrorMessage = "Request date must be today or a future date.")]
        public DateTime? RequestDate { get; set; }

    }


    public class NotPastAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // If value is null, let [Required] handle that validation.
            if (value is DateTime dateValue)
            {
                // Compare only the date part.
                if (dateValue.Date < DateTime.Today)
                {
                    return new ValidationResult(ErrorMessage ?? "Request date cannot be in the past.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
