using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;
using Lime.Messaging.Contents;
using GasBestPrice.Services;
using Take.Blip.Client.Extensions.Bucket;

namespace GasBestPrice.Receivers
{
    public class LocationMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        private readonly IGasStationService _gasStationService;

        public LocationMessageReceiver(ISender sender, IBucketExtension bucketExtension)
        {
            _sender = sender;
            _gasStationService = new GasStationService(bucketExtension);
        }
        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var receivedLocation = message.Content as Location;

            var textDocument = new PlainText { Text = "Um instante... ⏳ Estou procurando os postos, Paula." };
            await _sender.SendMessageAsync(textDocument, message.From, cancellationToken);

            //Get GasStations
            var carousel = await _gasStationService.GetNearGasStationsAsync(receivedLocation);

            //Carousel with near gas stations
            textDocument.Text = "Pronto!Os postos próximos são esses aqui: ⬅️➡️";
            await _sender.SendMessageAsync(textDocument, message.From, cancellationToken);

            await _sender.SendMessageAsync(carousel, message.From, cancellationToken);
        }
    }
}
