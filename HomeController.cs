using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FastProductSlowConsumeWeb.Datas;
using FastProductSlowConsumeWeb.Models;
using FastProductSlowConsumeWeb.Services.Rabbit;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FastProductSlowConsumeWeb.Controllers.Home
{
    public class HomeController : Controller
    {
        private readonly IModel _channel;
        private readonly IRabbitMqService _reRabbitMqService;
        private readonly DataContext _dataContext;


        public HomeController(IRabbitMqService rabbitMqService, DataContext dataContext)
        {
            _reRabbitMqService = rabbitMqService;
            _dataContext = dataContext;
            _channel = rabbitMqService.CreateIModel();
        }

        [HttpGet("/ProductText")]
        public IActionResult ProductText()
        {
            for (int i = 0; i < 100000; i++)
            {
                var exchange = "Janitor"; //交换机名
                var routeKey = "TempeCheck"; //路由名
                var propType = "text/plain"; //消息类型
                var queue = "Queue"; //管道名
                _reRabbitMqService.BasicProductMessage(_channel, propType, queue, exchange, routeKey, $"消息{i}");
                Console.WriteLine($"消息{i}");
            }


            return Json("true");
        }

        /// <summary>
        /// 生产示例
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpGet("/BasicProductMessage")]
        public IActionResult BasicProductMessage(string message)
        {
            var exchange = "Janitor"; //交换机名
            var routeKey = "TempeCheck"; //路由名
            var propType = "text/plain"; //消息类型
            var queue = "Queue"; //管道名
            _reRabbitMqService.BasicProductMessage(_channel, propType, queue, exchange, routeKey, message);

            return Json("HasBeenDown");
        }

        //消费
        [HttpGet("/ConsumeMessage")]
        public IActionResult ConsumeMessage()
        {
            var queue = "Queue";

            _reRabbitMqService.ConsumeMessage(_channel, queue);

            return Json("HasBeenDown");
        }


        [HttpDelete("/Close")]
        public IActionResult CloseIModel()
        {
            var res = _reRabbitMqService.CloseIModel(_channel);
            return Json(res.ToString());
        }

        private bool Show(string log)
        {
            Console.WriteLine(log);
            return true;
        }

        [HttpGet("/SaveToEfCore")]
        public bool SaveToEfCore(string log)
        {

            _dataContext.LogDatas.Add(new LogData { Id = log, Log = log });
            var res = _dataContext.SaveChanges();

            return res > 0;
        }

        [HttpGet("/Text")]
        public IActionResult Text()
        {
            //var lll = new string[3] { "djfgkds", "sjdfgjsdjfjds", "sdhfjdsfds" };
            //var ss = new LogData
            //{
            //    Id = "sjefjjsdf",
            //    Log = "dkfksdkf",
            //    Ss = lll
            //};
            //_dataContext.LogDatas.Add(ss);
            var res = _dataContext.SaveChanges();

            return Json("true");
        }

        [HttpGet("/Text2")]
        public IActionResult Text2()
        {
            var res = _dataContext.LogDatas.Find("sjefjjsdf");
            return Json(res.Ss);
        }
    }
}
