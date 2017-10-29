using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;
using Lime.Messaging.Contents;
using Take.Blip.Client.Session;
using GasBestPrice.Services;
using Take.Blip.Client.Extensions.Bucket;
using GasBestPrice.Model;

namespace GasBestPrice.Receivers
{
    public class FixPriceMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        private readonly IStateManager _stateManager;
        private readonly IBucketExtension _bucketExtension;
        private readonly IGasStationService _gasStationService;

        public FixPriceMessageReceiver(ISender sender, IStateManager stateManage, IBucketExtension bucketExtension)
        {
            _sender = sender;
            _stateManager = stateManage;
            _bucketExtension = bucketExtension;
            _gasStationService = new GasStationService(bucketExtension);
        }
        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var receivedText = (message.Content as PlainText).Text;
            var currentState = await _stateManager.GetStateAsync(message.From, cancellationToken);

            var myContextKey = $"{message.From.ToIdentity()}:context";

            var contextDocument = await _bucketExtension.GetAsync<JsonDocument>(myContextKey);

            var gasStationId = contextDocument["lastSelectedGasStationId"] as string;

            switch (currentState)
            {
                case "3.1.1":
                    //Extract gasoline price
                    var gasolinePrice = float.Parse(receivedText);

                    await _gasStationService.UpdateGasolinePriceAsync(gasStationId, gasolinePrice);

                    //Change user state
                    await _stateManager.SetStateAsync(message.From, "3.1.1A", cancellationToken);

                    //Send Alcohol message
                    var alcoholText = new PlainText { Text = "E o etanol?" };
                    await _sender.SendMessageAsync(alcoholText, message.From, cancellationToken);
                    break;
                case "3.1.1A":

                    //Extract alcohol price
                    var alcoholPrice = float.Parse(receivedText);

                    await _gasStationService.UpdateAlcoholPriceAsync(gasStationId, alcoholPrice);

                    var actionsQuickReply = new Select
                    {
                        Text = "O que você quer fazer?",
                        Scope = SelectScope.Immediate,
                        Options = new SelectOption[]
                        {
                            new SelectOption
                            {
                                Text = "📍 Postos próximos",
                                Value = new Trigger { StateId = "3.1.0" }
                            },
                            new SelectOption
                            {
                                Text = "⭐ Meus favoritos",
                                Value = new Trigger { StateId = "3.2.0" }
                            }
                        }
                    };

                    await _sender.SendMessageAsync(actionsQuickReply, message.From, cancellationToken);
                    break;
            }            
        }
    }
}
