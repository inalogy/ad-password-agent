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
        BaseAddressError = 102,
        NotFoundError=103,
        ErrorDecodingResultFromXml = 201,
        NetworkCommunicationError = 301,
        NoActionResult = 400,
        InvalidResult = 408

    }
}
