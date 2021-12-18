using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MQTTnet;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace MQTTServer
{
    public partial class Form1 : Form
    {
        public MqttServer mqttServer = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            var username = this.txtUserName.Text;
            var password = this.txtPassword.Text;

            StartServer(username, password);
        }

        public async void StartServer(string username, string password)
        {
            try
            {
                if (mqttServer == null)
                {
                    var optionsBuilder = new MqttServerOptionsBuilder()
                    .WithDefaultEndpoint().WithDefaultEndpointPort(1883).WithConnectionValidator(c =>
                    {
                        var currentUser = username;
                        var currentPwd = password;

                        if (currentUser == null || currentPwd == null)
                        {
                            c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                            return;
                        }

                        if (c.Username != currentUser || c.Password != currentPwd)
                        {
                            c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        }

                        c.ReasonCode = MqttConnectReasonCode.Success;
                    })
                    .WithSubscriptionInterceptor(c =>
                    {
                        c.AcceptSubscription = true;
                    })
                    .WithApplicationMessageInterceptor(c =>
                    {
                        c.AcceptPublish = true;
                    });

                    mqttServer = new MqttFactory().CreateMqttServer() as MqttServer;
                    mqttServer.StartedHandler = new MqttServerStartedHandlerDelegate(new Action<EventArgs>(e =>
                    {
                        if (mqttServer.IsStarted)
                        { }

                        this.txtMessage.BeginInvoke(new Action(() =>
                        {
                            this.txtMessage.Text += "MQTT服务启动完成！" + Environment.NewLine;
                        }));

                        LogManager.WriteLogEx(LOGLEVEL.INFO, "MQTT服务启动完成！");
                    }));
                    //mqttServer.StartedHandler = new MqttServerStartedHandlerDelegate(OnMqttServerStarted);

                    mqttServer.StoppedHandler = new MqttServerStoppedHandlerDelegate(new Action<EventArgs>(e =>
                    {
                        if (!mqttServer.IsStarted)
                        { }

                        this.txtMessage.BeginInvoke(new Action(() =>
                        {
                            this.txtMessage.Text += "MQTT服务停止完成！" + Environment.NewLine;
                        }));

                        LogManager.WriteLogEx(LOGLEVEL.INFO, "MQTT服务停止完成！");
                    }));

                    mqttServer.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(new Action<MqttServerClientConnectedEventArgs>(e =>
                    {
                        LogManager.WriteLogEx(LOGLEVEL.INFO, $"客户端[{e.ClientId}]已连接");
                        this.txtMessage.BeginInvoke(new Action(() =>
                        {
                            this.txtMessage.Text += $"客户端[{e.ClientId}]已连接" + Environment.NewLine;
                        }));
                    }));

                    mqttServer.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(new Action<MqttServerClientDisconnectedEventArgs>(e =>
                    {
                        LogManager.WriteLogEx(LOGLEVEL.INFO, $"客户端[{e.ClientId}]已断开连接！");
                        this.txtMessage.BeginInvoke(new Action(() =>
                        {
                            this.txtMessage.Text += $"客户端[{e.ClientId}]已断开连接！" + Environment.NewLine;
                        }));
                    }));

                    mqttServer.ClientSubscribedTopicHandler = new MqttServerClientSubscribedTopicHandlerDelegate(new Action<MqttServerClientSubscribedTopicEventArgs>(e =>
                    {
                        LogManager.WriteLogEx(LOGLEVEL.INFO, $"客户端[{e.ClientId}]已成功订阅主题[{e.TopicFilter}]！");
                        this.txtMessage.BeginInvoke(new Action(() =>
                        {
                            this.txtMessage.Text += $"客户端[{e.ClientId}]已成功订阅主题[{e.TopicFilter}]！" + Environment.NewLine;
                        }));
                    }));

                    mqttServer.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(new Action<MqttServerClientUnsubscribedTopicEventArgs>(e =>
                    {
                        LogManager.WriteLogEx(LOGLEVEL.INFO, $"客户端[{e.ClientId}]已成功取消订阅主题[{e.TopicFilter}]！");
                        this.txtMessage.BeginInvoke(new Action(() =>
                        {
                            this.txtMessage.Text += $"客户端[{e.ClientId}]已成功取消订阅主题[{e.TopicFilter}]！" + Environment.NewLine;
                        }));
                    }));

                    mqttServer.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(new Action<MqttApplicationMessageReceivedEventArgs>(e =>
                    {
                        LogManager.WriteLogEx(LOGLEVEL.INFO, $"客户端[{e.ClientId}]>> 主题：{e.ApplicationMessage.Topic} 负荷：{Encoding.UTF8.GetString(e.ApplicationMessage.Payload)} Qos：{e.ApplicationMessage.QualityOfServiceLevel} 保留：{e.ApplicationMessage.Retain}");
                        this.txtMessage.BeginInvoke(new Action(() =>
                        {
                            this.txtMessage.Text += $"客户端[{e.ClientId}]>> 主题：{e.ApplicationMessage.Topic} 负荷：{Encoding.UTF8.GetString(e.ApplicationMessage.Payload)} Qos：{e.ApplicationMessage.QualityOfServiceLevel} 保留：{e.ApplicationMessage.Retain}" + Environment.NewLine;
                        }));
                    }));

                    await mqttServer.StartAsync(optionsBuilder.Build());
                    this.txtMessage.BeginInvoke(new Action(() =>
                    {
                        this.txtMessage.Text += "MQTT Server is Started." + Environment.NewLine;
                    }));

                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLogEx(LOGLEVEL.ERROR, "MQTT Server 异常：" + ex.Message);
                this.txtMessage.BeginInvoke(new Action(() =>
                {
                    this.txtMessage.Text += "MQTT Server 异常：" + ex.Message + Environment.NewLine;
                }));
            }
        }

        public Task OnMqttServerStarted(EventArgs e)
        {
            return Task.Run(() =>
            {
                this.txtMessage.BeginInvoke(new Action(() =>
                {
                    this.txtMessage.Text += "MQTT服务启动完成！" + Environment.NewLine;
                }));
            });
        }

        private void btnStopServer_Click(object sender, EventArgs e)
        {

        }

        public async void StopServer()
        {
            try
            {
                if (mqttServer == null)
                {
                    return;
                }

                await mqttServer.StopAsync();
                mqttServer = null;

                this.txtMessage.BeginInvoke(new Action(() =>
                {
                    this.txtMessage.Text += "MQTT服务已停止" + Environment.NewLine;
                }));
            }
            catch (Exception ex)
            {
                this.txtMessage.BeginInvoke(new Action(() =>
                {
                    this.txtMessage.Text += "MQTT服务停止异常：" + ex.Message + Environment.NewLine;
                }));
            }
        }

        /// <summary>
        /// 服务端 发布消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPublish_Click(object sender, EventArgs e)
        {
            var topic = this.txtTopic.Text;
            var payload = this.txtPayload.Text;

            ServerPublishTopic(topic, payload);
        }

        public async void ServerPublishTopic(string topic, string payload)
        {
            var message = new MqttApplicationMessage()
            {
                Topic = topic,
                Payload = Encoding.UTF8.GetBytes(payload)
            };

            await mqttServer.PublishAsync(message);

            this.txtMessage.BeginInvoke(new Action(() =>
            {
                this.txtMessage.Text += "MQTT服务发布消息成功：" + topic + "=>" + payload + Environment.NewLine;
            }));
        }
    }
}
