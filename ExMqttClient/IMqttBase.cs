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
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="Host">ip</param>
        /// <param name="Port">端口</param>
        /// <param name="UserName">验证用户名</param>
        /// <param name="UserPassword">验证用户密码</param>
        /// <param name="TopicName">主题</param>
        /// <param name="IsShowInput">是否显示Debug信息</param>
        /// <param name="ClientId">客户端名称，默认Guid生成</param>
        void Init(string Host, int Port, string UserName, string UserPassword, string[] TopicName, bool IsShowInput = true, string ClientId = null);
        /// <summary>
        /// 开启
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StartAsync(CancellationToken cancellationToken);
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="task"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        Task StopAsync(Task task, CancellationTokenSource source);
        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic"></param>
        void ClientSubscribeTopic(string topic);
        /// <summary>
        /// 发布主题消息
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        void PublishTopic(string topic, string payload);
    }
}
