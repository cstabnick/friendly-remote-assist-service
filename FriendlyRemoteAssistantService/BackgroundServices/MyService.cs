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

    private Queue<byte[]> _messageQueue = new Queue<byte[]>();

    private void ReceiveFromSender()
    {
        byte[] sendBuffer = new byte[_messageSize];
        List<byte> incomingMessage = new List<byte>();

        while (true)
        {
            var result = _wsSender.ReceiveAsync(new ArraySegment<byte>(sendBuffer), CancellationToken.None).GetAwaiter()
                .GetResult();

            incomingMessage.AddRange(sendBuffer);
            if (result.EndOfMessage && incomingMessage.Count > 0)
            {
                _messageQueue.Enqueue(incomingMessage.ToArray());
                incomingMessage.Clear();
            }
        }
    }

    private void SendToReceiver()
    {
        while (true)
        {
            if (_messageQueue.Count > 0)
            {
                byte[] ba = _messageQueue.Dequeue();

                byte[] sendBuffer = new byte[_messageSize];
                using (var ms = new MemoryStream(ba, false))
                {
                    for (int i = 0; ba.Length > _messageSize * i; i++)
                    {
                        ms.ReadAsync(sendBuffer, 0, _messageSize).GetAwaiter().GetResult();

                        var lastMessage = ba.Length <= _messageSize * (i + 1);
                        _wsReceiver.SendAsync(new ArraySegment<byte>(sendBuffer),
                            WebSocketMessageType.Binary, lastMessage,
                            CancellationToken.None).GetAwaiter().GetResult();
                    }
                }
            }
            else
                Task.Delay(500).GetAwaiter().GetResult();
        }
    }

    private Thread _tReceive;
    private Thread _tSend;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            if (_wsSender != null && _wsReceiver != null)
            {
                if (_tReceive != null && _tReceive.IsAlive)
                    _tReceive.Interrupt();
                if (_tSend != null && _tSend.IsAlive)
                    _tSend.Interrupt();
                
                _tReceive = new Thread(ReceiveFromSender);
                _tSend = new Thread(SendToReceiver);

                _tReceive.Start();
                _tSend.Start();

                while (_tReceive.IsAlive && _tSend.IsAlive)
                {
                    await Task.Delay(1000);
                    Console.WriteLine("Running!");
                }
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