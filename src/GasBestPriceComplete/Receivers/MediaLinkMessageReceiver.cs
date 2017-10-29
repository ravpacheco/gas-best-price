using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;
using Lime.Messaging.Contents;
using GasBestPrice.Model;

namespace GasBestPrice
{
    public class MediaLinkMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public MediaLinkMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken = default(CancellationToken))
        {
            var chatStateMessage = new Message
            {
                Content = new ChatState { State = ChatStateEvent.Composing },
                To = message.From
            };
            await _sender.SendMessageAsync(chatStateMessage, cancellationToken);

            var content = message.Content as MediaLink;
            var contentType = content.Type;
            PlainText result = null;

            switch (contentType.Type)
            {
                case "audio":
                    result = new PlainText { Text = "Não consigo ouvir você, por enquanto eu ainda sou surdo 🙉 e entendo apenas texto" };
                    break;
                case "image":
                case "video":
                    result = new PlainText { Text = "Vish… eu ainda não sei conversar por imagem, entendo apenas texto 🙈" };
                    break;
                case "document":
                case "application":
                    result = new PlainText { Text = "Oi, não curto documentos, por enquanto eu entendo apenas texto 😉" };
                    break;
            }

            await _sender.SendMessageAsync(result, message.From, cancellationToken);
            await Task.Delay(6000);

            var quickReply = new Select
            {
                Text = "Escolha abaixo:",
                Scope = SelectScope.Immediate,
                Options = new SelectOption[]
                        {
                            new SelectOption
                            {
                                Text = "📍 Postos próximos",
                                Value = new Trigger { StateId = "M3" }
                            },
                            new SelectOption
                            {
                                Text = "⭐ Meus favoritos",
                                Value = new Trigger { StateId = "M1" }
                            }
                        }
            };

            await _sender.SendMessageAsync(quickReply, message.From, cancellationToken);
        }
    }
}
