using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MidPointUpdatingService.Operations
{
    public interface IMidPointOperation
    {
        int TTL { get; set; }

        string OperationName { get; }

        void ExecuteOperation(Dictionary<string, object> parameters, HttpClient client, ILog log, CancellationToken token);

    }
}
