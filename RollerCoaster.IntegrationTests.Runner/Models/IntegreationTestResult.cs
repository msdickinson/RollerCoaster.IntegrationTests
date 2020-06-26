using System;
using System.Collections.Generic;
using System.Text;

namespace RollerCoaster.IntegrationTests.Runner.Models
{
    class IntegreationTestResult
    {
        public string TestName { get; set; }
        public bool Pass { get; set; }
        public string CorrelationId { get; set; }
        public Exception Exception { get; set; }
        public List<string> SuccessLog { get; set; }
    }
}
