using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExMqttClient.Implementation
{
    public class DefaultMqttClientBase : AbstractMqttBase
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string[] TopicNames { get; set; }
        public bool IsShowInput { get; set; }
        public string ClientId { get; set; }

        public override event MqttDelegate.DisconnectedDelegate OnDisconnected;
        public override event MqttDelegate.ConnectedDelegate OnConnected;
        public override event MqttDelegate.ReceivedMessageDelegate OnReceivedMessage;

        public override void Init(string Host, int Port, string UserName, string UserPassword, string[] TopicNames, bool IsShowInput, string ClientId)
        {
            this.Host = Host;
            this.Port = Port;
            this.UserName = UserName;
            this.UserPassword = UserPassword;
            this.TopicNames = TopicNames;
            this.IsShowInput = IsShowInput;
            if (!string.IsNullOrEmpty(ClientId))
            {
                this.clientId = ClientId;
            }
        }

        private MqttClient mqttClient = null;
        string clientId = "client-" + Guid.NewGuid().ToString();

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var task = Task.Factory.StartNew(async () =>
            {
                int tryTimes = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (mqttClient == null || !mqttClient.IsConnected)
                    {
                        if (tryTimes > 0)
                            Console.WriteLine($"正在尝试重新连接...失败次数{tryTimes}");
                        await ClientStart();
                        tryTimes++;
                    }
                    else
                    {
                        tryTimes = 0;
                    }
                    Thread.Sleep(1000);
                }
            }, cancellationToken);
            return task;
        }

        public override async Task StopAsync(Task task, CancellationTokenSource source)
        {
            await ClientStop();
            source.Cancel();
            task.Wait(source.Token);
        }


        protected override async Task ClientStart()
        {
            try
            {
                var tcpServer = Host;
                var tcpPort = Port;
                var mqttUser = UserName;
                var mqttPassword = UserPassword;

                var mqttFactory = new MqttFactory();

                var options = new MqttClientOptions
                {
                    ClientId = clientId,
                    ProtocolVersion = MQTTnet.Formatter.MqttProtocolVersion.V311,
                    ChannelOptions = new MqttClientTcpOptions
                    {
                        Server = tcpServer,
                        Port = tcpPort
                    },
                    WillDelayInterval = 10,
                    WillMessage = new MqttApplicationMessage()
                    {
                        Topic = $"LastWill/{clientId}",
                        Payload = Encoding.UTF8.GetBytes("I Lost the connection!"),
                        QualityOfServiceLevel = MqttQualityOfServiceLevel.ExactlyOnce
                    }

                };
                if (options.ChannelOptions == null)
                {
                    throw new InvalidOperationException();
                }

                if (!string.IsNullOrEmpty(mqttUser))
                {
                    options.Credentials = new MqttClientCredentials
                    {
                        Username = mqttUser,
                        Password = Encoding.UTF8.GetBytes(mqttPassword)
                    };
                }

                options.CleanSession = true;
                options.KeepAlivePeriod = TimeSpan.FromSeconds(5);

                mqttClient = mqttFactory.CreateMqttClient() as MqttClient;
                mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnMqttClientConnected);
                mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnMqttClientDisConnected);
                mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(OnSubscriberMessageReceived);
                await mqttClient.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                if (mqttClient != null)
                {
                    await ClientStop();
                }
                mqttClient = null;
                if (IsShowInput)
                    Console.WriteLine($"客户端尝试连接出错.>{ex.Message}");
            }
        }
        int index = 0;

        private void OnSubscriberMessageReceived(MqttApplicationMessageReceivedEventArgs obj)
        {
            index++;
            if (IsShowInput)
                Console.WriteLine($"{obj.ApplicationMessage.Topic}-{Encoding.UTF8.GetString(obj.ApplicationMessage.Payload)}-total received : {index}");
            OnReceivedMessage?.Invoke(obj);
        }

        private void OnMqttClientDisConnected(MqttClientDisconnectedEventArgs obj)
        {
            if (IsShowInput)
                Console.WriteLine($"{obj.ClientWasConnected}:{obj.Reason} DisConnected");
            OnDisconnected?.Invoke(obj);
        }

        private void OnMqttClientConnected(MqttClientConnectedEventArgs obj)
        {
            if (IsShowInput)
                Console.WriteLine($"client connected");

            OnConnected?.Invoke(obj);
            if (TopicNames != null)
                foreach (var TopicName in TopicNames)
                {
                    ClientSubscribeTopic(TopicName);
                }
        }

        public override async void ClientSubscribeTopic(string topic)
        {
            await mqttClient.SubscribeAsync(topic);
            if (IsShowInput)
                Console.WriteLine("客户端[{0}]订阅主题[{1}]成功！", mqttClient.Options.ClientId, topic);
        }

        protected override async Task ClientStop()
        {
            try
            {
                if (mqttClient == null) return;
                await mqttClient.DisconnectAsync();
                mqttClient = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"客户端尝试断开Server出错.>{ex.Message}");
            }
        }

        public async override void PublishTopic(string topic, string payload)
        {
            var message = new MqttApplicationMessage()
            {
                Topic = topic,
                Payload = Encoding.UTF8.GetBytes(payload)
            };
            await mqttClient.PublishAsync(message);
            if (IsShowInput)
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}MQTT已发布主题{topic}");
        }
    }
}
