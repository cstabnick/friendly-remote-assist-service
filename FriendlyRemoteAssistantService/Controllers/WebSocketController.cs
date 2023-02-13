using System.Net.WebSockets;
using FriendlyRemoteAssistantService.BackgroundServices;
using Microsoft.AspNetCore.Mvc;

namespace FriendlyRemoteAssistantService.Controllers;

[ApiController]
public class WebSocketController : ControllerBase
{
    private MyService _myService;
    public WebSocketController(MyService myService)
    {
        _myService = myService;
    }
    private static async Task Echo(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            await webSocket.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);

            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
            
            Console.WriteLine(buffer.Length);
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
    
    [HttpGet("/ws")]
    public async Task<IActionResult> Get()
    {
        
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
    
            string test = "this is a test";
            var buffer = new byte[test.Length];
            for (int i = 0; i < test.Length; i++)
            {
                buffer[i] = (byte)test[i];
            }
            
                
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

            while (true)
                await Echo(webSocket);
            // webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);}
        }
    
        return Ok();
    }
    
    [HttpGet("/sender")]
    public async Task<IActionResult> Sender()
    {
        
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
    
            _myService.SetSender(webSocket);
            while (true) {}

            return Ok();
            // webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);}
        }
    
        return Ok();
    }
    
    [HttpGet("/receiver")]
    public async Task<IActionResult> Receiver()
    {
        
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
    
            _myService.SetReceiver(webSocket);
            while (true) {}
            
            return Ok();
            // webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);}
        }
    
        return Ok();
    }
}