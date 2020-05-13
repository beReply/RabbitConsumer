using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastProductSlowConsumeWeb.Datas;
using FastProductSlowConsumeWeb.Models;
using RabbitMQ.Client.Events;

namespace FastProductSlowConsumeWeb.Services.Rabbit
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly IConnection _connection;
        private readonly DataContext _dataContext;

        public RabbitMqService(DataContext dataContext)
        {
            _dataContext = dataContext;
            var factory = new ConnectionFactory
            {
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/",
                HostName = "localhost"
            };
            _connection = factory.CreateConnection();
        }


        /// <summary>
        /// 创建IModel
        /// </summary>
        /// <returns></returns>
        public IModel CreateIModel()
        {
            return _connection.CreateModel();
        }

        /// <summary>
        /// 删除IModel
        /// </summary>
        /// <param name="channel">IModel</param>
        /// <returns></returns>
        public bool CloseIModel(IModel channel)
        {
            channel.Close();
            return channel.IsClosed;
        }


        /// <summary>
        /// product message
        /// </summary>
        /// <param name="channel">IModel</param>
        /// <param name="propType">属性类型(ex:"text/plain")</param>
        /// <param name="queue">队列</param>
        /// <param name="exchange">交换名</param>
        /// <param name="routeKey">路由键</param>
        /// <param name="message">将要发布的消息</param>
        public void BasicProductMessage(IModel channel, string propType, string queue, string exchange, string routeKey,
            string message)
        {
            channel.ExchangeDeclare(exchange, ExchangeType.Direct);
            channel.QueueDeclare(queue, false, false, false, null);
            channel.QueueBind(queue, exchange, routeKey, null);  //将队列和交换机通过键进行绑定

            var messageBodyBytes = Encoding.UTF8.GetBytes(message);
            var props = channel.CreateBasicProperties();
            props.ContentType = propType;
            props.DeliveryMode = 2;

            //
            channel.BasicPublish(exchange, routeKey, props, messageBodyBytes);
        }

        /// <summary>
        /// consume message
        /// </summary>
        /// <param name="channel">IModel</param>
        /// <param name="queue">队列名</param>
        public void ConsumeMessage(IModel channel, string queue)
        {
            channel.QueueDeclare(queue, false, false, false, null);
            var consumer = new DatabaseConsumer(channel, _dataContext);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                ea.EventDataContext.LogDatas.Add(new LogData { Id = message, Log = message });
                var res = _dataContext.SaveChanges();

                Console.WriteLine(message);
                channel.BasicAck(ea.DeliveryTag, false);
            };
            string consumerTag = channel.BasicConsume(queue, false, consumer);
            channel.BasicCancel(consumerTag); //通过这种方式可以关闭该消费者
        }



        #region 未使用

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel">IModel</param>
        /// <param name="queue">队列名</param>
        /// <param name="exchange">交换机名</param>
        /// <param name="routeKey">路由建</param>
        public void CreateBindTheExchangeQueue(IModel channel, string queue, string exchange, string routeKey)
        {
            channel.ExchangeDeclare(exchange, ExchangeType.Direct);
            channel.QueueDeclare(queue, false, false, false, null);
            channel.QueueBind(queue, exchange, routeKey, null);
        }

        /// <summary>
        /// 使用IModel创建ExchangeDeclare
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="exchangeName"></param>
        public void CreateExchange(IModel channel, string exchangeName)
        {
            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
        }


        /// <summary>
        /// 使用IModel创建QueueDeclare
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="queueName"></param>
        public void CreateQueue(IModel channel, string queueName)
        {
            channel.QueueDeclare(queueName, false, false, false, null);
        }

        /// <summary>
        /// 使用IModel关联ExchangeDeclare和QueueDeclare
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="queueName"></param>
        /// <param name="exchangeName"></param>
        /// <param name="routeKey"></param>
        public void BindExchangeWithQueue(IModel channel, string queueName, string exchangeName, string routeKey)
        {
            channel.QueueBind(queueName, exchangeName, routeKey, null);
        }

        #endregion
    }
}
