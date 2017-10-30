using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using System.Diagnostics;
using Take.Blip.Client;
using Take.Blip.Client.Session;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;
using Lime.Messaging.Contents;
using System.Collections.Generic;
using GasBestPrice.Model;

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
        private readonly IArtificialIntelligenceExtension _artificialIntelligenceExtension;

        public PlainTextMessageReceiver(ISender sender, 
            Settings settings, 
            IStateManager stateManager,
            IArtificialIntelligenceExtension artificialIntelligenceExtension)
        {
            _sender = sender;
            _settings = settings;
            _stateManager = stateManager;
            _artificialIntelligenceExtension = artificialIntelligenceExtension;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var currentState = await _stateManager.GetStateAsync(message.From.ToIdentity(), cancellationToken);

            var receivedText = (message.Content as PlainText).Text;

            var result = await _artificialIntelligenceExtension.AnalyzeAsync(new AnalysisRequest { Text = receivedText });

            var bestIntention = result.Intentions[0];

            var actionsQuickReply = new Select
            {
                Text = "Oii 😄, em que posso te ajudar ? 👇",
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

            var notHandledText = new PlainText { Text = "Não entendi 😶 ainda estou aprendendo" };

            if (bestIntention.Score < 0.3)
            {
                //Not handled
                await _sender.SendMessageAsync(notHandledText, message.From, cancellationToken);

                await Task.Delay(2000);

                actionsQuickReply.Text = "Mas eu já sei falar sobre isso:";
                await _sender.SendMessageAsync(actionsQuickReply, message.From, cancellationToken);
                return;
            }

            switch (bestIntention.Name)
            {
                case "Salutation":

                    await _sender.SendMessageAsync(actionsQuickReply, message.From, cancellationToken);
                    break;
                case "Help":

                    actionsQuickReply.Text = "Eu sou o bot Gasosa Barata, que te ajuda a encontrar o combustível mais em conta perto de você! O que deseja ? 👇";
                    await _sender.SendMessageAsync(actionsQuickReply, message.From, cancellationToken);

                    break;
                case "SearchFor":

                    actionsQuickReply.Text = "Quais postos deseja consultar ? 👇";
                    await _sender.SendMessageAsync(actionsQuickReply, message.From, cancellationToken);
                    break;
                default:
                    //Not handled
                    await _sender.SendMessageAsync(notHandledText, message.From, cancellationToken);
                    break;
            }
        }
    }
}
