using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CoverMate.Model
{

    public class FollowupSubmitRequest
    {
        public int RequestId { get; set; }
        public string Notes { get; set; }
    }
}
