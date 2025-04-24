using System.ComponentModel.DataAnnotations;

namespace CoverMate.Model
{


    //[AutoValidateAntiforgeryToken]
    public class SetSubstituteModel
    {
        [Required(ErrorMessage = "request_id is required.")]
        public string request_id { get; set; }


        [Required(ErrorMessage = "substitute_id is required.")]
        public string substitute_id { get; set; }


        [Required(ErrorMessage = "assignee_id is required.")]
        public string assignee_id { get; set; }


    }
}
