using System.Net.WebSockets;

namespace FriendlyRemoteAssistantService.BackgroundServices;

public class MyService : BackgroundService
{
    private WebSocket _wsSender = null;
    private WebSocket _wsReceiver = null;

    private int _messageSize = 4096;

    public void SetSender(WebSocket ws)
    {
        _wsSender = ws;
    }

    public void SetReceiver(WebSocket ws)
    {
        _wsReceiver = ws;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Queue<byte[]> messageQueue = new Queue<byte[]>();
        while (true)
        {
            if (_wsSender != null && _wsReceiver != null)
            {
                List<byte> incomingMessage = new List<byte>();

                Task.Run(() =>
                {
                    byte[] sendBuffer = new byte[_messageSize];
                    while (true)
                    {
                        var result = _wsSender.ReceiveAsync(new ArraySegment<byte>(sendBuffer), stoppingToken).GetAwaiter().GetResult();

                        incomingMessage.AddRange(sendBuffer);
                        if (result.EndOfMessage)
                        {
                            messageQueue.Enqueue(incomingMessage.ToArray());
                            incomingMessage.Clear();
                        }
                    }
                });

                await Task.Run(async () =>
                {
                    while (true)
                    {
                        if (messageQueue.Count > 0)
                        {
                            byte[] ba = messageQueue.Dequeue();

                            byte[] sendBuffer = new byte[_messageSize];
                            using (var ms = new MemoryStream(ba, false))
                            {
                                for (int i = 0; ba.Length > _messageSize * i; i++)
                                {
                                    await ms.ReadAsync(sendBuffer, 0, _messageSize);

                                    var lastMessage = ba.Length <= _messageSize * (i + 1);
                                    await _wsReceiver.SendAsync(new ArraySegment<byte>(sendBuffer),
                                        WebSocketMessageType.Binary, lastMessage,
                                        CancellationToken.None);
                                }
                            }
                        }
                        else
                            await Task.Delay(500);
                    }
                });
            }
            else
            {
                Console.WriteLine("Not yet connected");
                await Task.Delay(500);
            }
        }

        return;
    }
}