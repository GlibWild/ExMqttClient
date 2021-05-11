using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExMqttClient
{
    public abstract class AbstractMqttBase : IMqttBase
    {
        public virtual event MqttDelegate.DisconnectedDelegate OnDisconnected;
        public virtual event MqttDelegate.ConnectedDelegate OnConnected;
        public virtual event MqttDelegate.ReceivedMessageDelegate OnReceivedMessage;

        public abstract void ClientSubscribeTopic(string topic);

        public abstract void Init(string Host, int Port, string UserName, string UserPassword, string[] TopicName, bool IsShowInput,string ClientId);

        public abstract void PublishTopic(string topic, string payload);

        public abstract Task StartAsync(CancellationToken cancellationToken);

        public abstract Task StopAsync(Task task, CancellationTokenSource source);
        protected abstract Task ClientStart();
        protected abstract Task ClientStop();
    }
}
