using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CoverMate.Model
{

    [AutoValidateAntiforgeryToken]
    public class ApproveRequest
    {
        public int RequestId { get; set; }
    }
}
