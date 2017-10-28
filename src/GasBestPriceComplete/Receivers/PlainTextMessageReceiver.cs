using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using System.Diagnostics;
using Take.Blip.Client;
using Take.Blip.Client.Session;

namespace GasBestPrice
{
    /// <summary>
    /// Defines a class for handling messages. 
    /// This type must be registered in the application.json file in the 'messageReceivers' section.
    /// </summary>
    public class PlainTextMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        private readonly Settings _settings;
        private readonly IStateManager _stateManager;

        public PlainTextMessageReceiver(ISender sender, Settings settings, IStateManager stateManager)
        {
            _sender = sender;
            _settings = settings;
            _stateManager = stateManager;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var currentState = await _stateManager.GetStateAsync(message.From.ToIdentity(), cancellationToken);

            if (currentState == null)
            {

            }

            Trace.TraceInformation($"From: {message.From} \tContent: {message.Content}");
            await _sender.SendMessageAsync("Pong!", message.From, cancellationToken);
        }
    }
}
