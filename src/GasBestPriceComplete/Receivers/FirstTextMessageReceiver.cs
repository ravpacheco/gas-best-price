using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;
using Take.Blip.Client.Session;
using Lime.Messaging.Contents;
using GasBestPrice.Model;
using Take.Blip.Client.Extensions.Bucket;
using System.Collections.Generic;
using Take.Blip.Client.Extensions.Directory;

namespace GasBestPrice.Receivers
{
    public class FirstTextMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        private readonly IStateManager _stateManager;
        private readonly IBucketExtension _bucketExtension;
        private readonly IDirectoryExtension _directoryExtension;

        public FirstTextMessageReceiver(ISender sender, IStateManager stateManager, IBucketExtension bucketExtension, IDirectoryExtension directoryExtension)
        {
            _sender = sender;
            _stateManager = stateManager;
            _bucketExtension = bucketExtension;
            _directoryExtension = directoryExtension;
        }
        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            //Get user details
            var userDetails = await _directoryExtension.GetDirectoryAccountAsync(message.From, cancellationToken);

            //Update user state
            await _stateManager.SetStateAsync(message.From.ToIdentity(), "1.0.0", cancellationToken);

            //Create a user context
            var myContextKey = $"{message.From.ToIdentity()}:context";
            var contextDocument = new JsonDocument();
            await _bucketExtension.SetAsync(myContextKey, contextDocument);

            // Salutation texts
            PlainText textDocument = new PlainText { Text = "Oi ${contact.name}!Sou o Gasosa Barata, o bot 🤖 que te ajuda a encontrar o combustível mais em conta perto de você!" };

            var salutationMessage = new Message
            {
                To = message.From,
                Content = textDocument,
                Id = EnvelopeId.NewId(),
                Metadata = new Dictionary<string, string>
                {
                    { "#message.replaceVariables", "true" }
                }
            };

            await _sender.SendMessageAsync(salutationMessage, cancellationToken);
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
