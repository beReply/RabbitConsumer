using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace FastProductSlowConsumeWeb.Services.Rabbit
{
    public interface IRabbitMqService
    {
        #region IModel

        public IModel CreateIModel();

        public bool CloseIModel(IModel channel);

        #endregion


        #region 功能相关

        public void BasicProductMessage(IModel channel, string propType, string queue, string exchange, string routeKey,
            string message);

        public void ConsumeMessage(IModel channel, string queue);

        #endregion
    }
}
