using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;
using GasBestPrice.Model;
using Lime.Messaging.Contents;
using Take.Blip.Client.Session;
using GasBestPrice.Services;
using Take.Blip.Client.Extensions.Bucket;

namespace GasBestPrice
{
    public class TriggerMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        private readonly IStateManager _stateManager;
        private readonly IBucketExtension _bucketExtension;
        private readonly IGasStationService _gasStationService;

        public TriggerMessageReceiver(ISender sender, IStateManager stateManager, IBucketExtension bucketExtension)
        {
            _sender = sender;
            _stateManager = stateManager;
            _bucketExtension = bucketExtension;
            _gasStationService = new GasStationService(bucketExtension);
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var trigger = message.Content as Trigger;

            //Update user state
            await _stateManager.SetStateAsync(message.From.ToIdentity(), trigger.StateId, cancellationToken);

            switch (trigger.StateId)
            {
                //Tips
                case "2.0.0":
                    var textDocument = new PlainText { Text = "Mandou bem, Paula! 😄" };
                    await _sender.SendMessageAsync(textDocument, message.From, cancellationToken);
                    await Task.Delay(2000);

                    textDocument = new PlainText { Text = "Deslize ⬅️➡️ para saber mais:" };
                    await _sender.SendMessageAsync(textDocument, message.From, cancellationToken);
                    await Task.Delay(2000);

                    //Carousel with tips
                    var carouselDocument = new DocumentCollection
                    {
                        ItemType = DocumentSelect.MediaType,
                        Items = new DocumentSelect[3]
                        {
                            new DocumentSelect
                            {
                                Header = new DocumentContainer
                                {
                                    Value = new MediaLink
                                    {
                                        Uri = new Uri("http://www.botsbrasil.com.br/wp-content/uploads/2016/12/take.png"),
                                        Type = new MediaType("image", "png"),
                                        Title = "Eu sou um robô, sempre que possível utilize os botões",
                                        Text = "Consigo te entender melhor assim"
                                    }
                                },
                                Options = new DocumentSelectOption[]{}
                            },
                            new DocumentSelect
                            {
                                Header = new DocumentContainer
                                {
                                    Value = new MediaLink
                                    {
                                        Uri = new Uri("http://www.botsbrasil.com.br/wp-content/uploads/2016/12/take.png"),
                                        Type = new MediaType("image", "png"),
                                        Title = "Se o preço não estiver correto, corrija no mesmo instante",
                                        Text = "A sua ajuda é essencial"
                                    }
                                },
                                Options = new DocumentSelectOption[]{}
                            },
                            new DocumentSelect
                            {
                                Header = new DocumentContainer
                                {
                                    Value = new MediaLink
                                    {
                                        Uri = new Uri("http://www.botsbrasil.com.br/wp-content/uploads/2016/12/take.png"),
                                        Type = new MediaType("image", "png"),
                                        Title = "Favorite seus postos prediletos",
                                        Text = "Assim você não precisa informar o endereço. Crie atalhos!",
                                    }
                                },
                                Options = new DocumentSelectOption[]{}
                            }
                        }
                    };

                    await _sender.SendMessageAsync(carouselDocument, message.From, cancellationToken);
                    await Task.Delay(6000);

                    var quickReply = new Select
                    {
                        Text = "Já estou pronto! Assim que terminar de ler clique no botão abaixo:",
                        Scope = SelectScope.Immediate,
                        Options = new SelectOption[]
                        {
                            new SelectOption
                            {
                                Text = "Começar",
                                Value = new Trigger
                                {
                                    StateId = "3.0.0"
                                }
                            }
                        }
                    };

                    await _sender.SendMessageAsync(quickReply, message.From, cancellationToken);

                    break;
                
                //Options
                case "3.0.0":

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

                //Near GasStations
                case "3.1.0":

                    var locationInput = new Input
                    {
                        Label = new DocumentContainer
                        {
                            Value = "Clique no botão abaixo para me enviar a sua localização ou digite o endereço:"
                        },
                        Validation = new InputValidation
                        {
                            Type = Location.MediaType,
                            Rule = InputValidationRule.Type
                        }
                    };

                    await _sender.SendMessageAsync(locationInput, message.From, cancellationToken);
                    break;

                //Favorites
                case "3.2.0":

                    var waitingTextDocument = new PlainText { Text = "Um instante... ⏳" };
                    await _sender.SendMessageAsync(waitingTextDocument, message.From, cancellationToken);

                    //Get Favorites GasStations
                    var carousel = await _gasStationService.GetFavoritesGasStationsAsync(message.From.ToIdentity());

                    //Carousel with favorites gas stations
                    var readyTextDocument = new PlainText { Text = "Pronto!Os postos favoritos são: ⬅️➡️" };
                    await _sender.SendMessageAsync(readyTextDocument, message.From, cancellationToken);

                    await _sender.SendMessageAsync(carousel, message.From, cancellationToken);

                    break;

                //Update GasStation prices
                case "3.1.1":

                    var currentState = await _stateManager.GetStateAsync(message.From, cancellationToken);

                    var myContextKey = $"{message.From.ToIdentity()}:context";

                    var contextDocument = await _bucketExtension.GetAsync<JsonDocument>(myContextKey);
                    contextDocument["lastSelectedGasStationId"] = trigger.Payload;
                    await _bucketExtension.SetAsync(myContextKey, contextDocument);

                    var helpText = new PlainText { Text = "Ops!Ainda bem que você está aqui para me ajudar 😅" };
                    await _sender.SendMessageAsync(helpText, message.From, cancellationToken);
                    await Task.Delay(2000);

                    helpText.Text = "Quanto está a gasolina ?";
                    await _sender.SendMessageAsync(helpText, message.From, cancellationToken);
                    break;

                //Favorite GasStation
                case "3.1.2":

                    await _gasStationService.FavoriteGasStationAsync(message.From, trigger.Payload);

                    var confirmationText = new PlainText { Text = "🌟 Posto favoritado!" };
                    await _sender.SendMessageAsync(confirmationText, message.From, cancellationToken);
                    await Task.Delay(2000);

                    actionsQuickReply = new Select
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

                //Get the route
                case "3.1.3":

                    var gasStation = await _gasStationService.GetAsync(trigger.Payload);

                    var locationRoute = new Location
                    {
                        Latitude = gasStation.Latitude,
                        Longitude = gasStation.Longitude,
                        Text = gasStation.Address
                    };

                    await _sender.SendMessageAsync(locationRoute, message.From, cancellationToken);
                    break;

                default:
                    break;
            }
        }
    }
}
