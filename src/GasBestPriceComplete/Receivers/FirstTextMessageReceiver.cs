using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;
using Take.Blip.Client.Session;
using Lime.Messaging.Contents;
using GasBestPrice.Model;

namespace GasBestPrice.Receivers
{
    public class FirstTextMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        private readonly IStateManager _stateManager;

        public FirstTextMessageReceiver(ISender sender, IStateManager stateManager)
        {
            _sender = sender;
            _stateManager = stateManager;
        }
        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            //Update user state
            await _stateManager.SetStateAsync(message.From.ToIdentity(), "1.0.0", cancellationToken);

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
        }
    }
}
