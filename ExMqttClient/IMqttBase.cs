using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExMqttClient
{
    public interface IMqttBase
    {
        event MqttDelegate.DisconnectedDelegate OnDisconnected;
        event MqttDelegate.ConnectedDelegate OnConnected;
        event MqttDelegate.ReceivedMessageDelegate OnReceivedMessage;

        void Init(string Host, int Port, string UserName, string UserPassword, string TopicName, bool IsShowInput = true, string ClientId = null);

        Task StartAsync(CancellationToken cancellationToken);

        Task StopAsync(Task task, CancellationTokenSource source);
    }
}
