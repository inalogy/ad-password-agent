using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidPointUpdatingService.Models
{
    public enum MidPointErrorEnum
    {
        OK = 0,
        ParametersError = 101,
        ErrorDecodingResultFromXml = 201,
        NoActionResult = 400
    }
}
