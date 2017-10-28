using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;

namespace GasBestPrice
{
    public class TriggerMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public TriggerMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        public Task ReceiveAsync(Message envelope, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
