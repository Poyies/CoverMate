using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CoverMate.Model
{

    [AutoValidateAntiforgeryToken]
    public class AssignRequest
    {
        public long RequestId { get; set; }
        public long SubstituteId { get; set; }
    }
}
