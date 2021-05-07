using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExMqttClient
{
    public class MqttDelegate
    {
        public delegate void ReceivedMessageDelegate(MqttApplicationMessageReceivedEventArgs obj);
        public delegate void DisconnectedDelegate(MqttClientDisconnectedEventArgs obj);
        public delegate void ConnectedDelegate(MqttClientConnectedEventArgs obj);
    }
}
