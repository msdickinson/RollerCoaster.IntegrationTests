using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RollerCoaster.IntegrationTests.Runner.Services.SuccessMessages
{
    public class SuccessMessagesService : ISuccessMessagesService
    {
        public string SuccessMessages
        {
            get
            {
                return _successMessages.Value;
            }
            set
            {
                _successMessages.Value = value;
            }
        }
        internal AsyncLocal<string> _successMessages { get; set; }

        public SuccessMessagesService()
        {
            _successMessages = new AsyncLocal<string>();
        }
    }
}
