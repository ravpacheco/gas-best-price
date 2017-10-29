using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;
using Take.Blip.Client.Session;
using Lime.Messaging.Contents;
using GasBestPrice.Model;
using Take.Blip.Client.Extensions.Bucket;

namespace GasBestPrice.Receivers
{
    public class FirstTextMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        private readonly IStateManager _stateManager;
        private readonly IBucketExtension _bucketExtension;

        public FirstTextMessageReceiver(ISender sender, IStateManager stateManager, IBucketExtension bucketExtension)
        {
            _sender = sender;
            _stateManager = stateManager;
            _bucketExtension = bucketExtension;
        }
        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            //Update user state
            await _stateManager.SetStateAsync(message.From.ToIdentity(), "1.0.0", cancellationToken);

            //Create a user context
            var myContextKey = $"{message.From.ToIdentity()}:context";
            var contextDocument = new JsonDocument();
            await _bucketExtension.SetAsync(myContextKey, contextDocument);

            // Salutation texts
            PlainText textDocument = new PlainText { Text = "Oi Paula!Sou o Gasosa Barata, o bot 🤖 que te ajuda a encontrar o combustível mais em conta perto de você!" };
            await _sender.SendMessageAsync(textDocument, message.From, cancellationToken);
            await Task.Delay(2000);

            textDocument.Text = "Separei algumas dicas rápidas, posso te mostrar ? 🙃";
            await _sender.SendMessageAsync(textDocument, message.From, cancellationToken);
            await Task.Delay(2000);

            // Start Options
            Select quickReply = new Select
            {
                Text = "Escolha abaixo: ⬇️",
                Scope = SelectScope.Immediate,
                Options = new SelectOption[]
                {
                    new SelectOption
                    {
                        Text = "👍 Claro, vamos lá!",
                        Value = new Trigger
                        {
                            StateId = "2.0.0"
                        }
                    },
                    new SelectOption
                    {
                        Text = "Fica pra próxima",
                        Value = new Trigger
                        {
                            StateId = "3.0.0"
                        }
                    }
                }
            };

            await _sender.SendMessageAsync(quickReply, message.From, cancellationToken);
        }
    }
}
