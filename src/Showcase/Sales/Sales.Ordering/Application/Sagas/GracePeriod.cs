using System.ComponentModel.DataAnnotations.Schema;
using Sales.Messages.Events;
using Stock.Messages.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sales.Ordering.Application.Sagas
{
    public class GracePeriod : Saga<GracePeriod.GracePeriodState>
        , IAmStartedByMessages<OrderPlaced>
        , IAmStartedByMessages<OrderStockConfirmed>
        , IAmStartedByMessages<OrderStockRejected>
        , IHandleTimeouts<GracePeriodExpired>
    {
        private readonly ILogger _logger;

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<GracePeriodState> mapper)
        {
            mapper.MapSaga(state => state.OrderId)
                .ToMessage<OrderPlaced>(msg => msg.OrderId)
                .ToMessage<OrderStockConfirmed>(msg => msg.OrderId)
                .ToMessage<OrderStockRejected>(msg => msg.OrderId);
        }

        public GracePeriod(ILogger<GracePeriod> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Received {MessageType} for OrderId: {OrderId}", nameof(OrderPlaced),
                message.OrderId);

            await RequestTimeout<GracePeriodExpired>(context, TimeSpan.FromMinutes(1));
        }

        public async Task Handle(OrderStockConfirmed message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Received {MessageType} for OrderId: {OrderId}", nameof(OrderStockConfirmed), message.OrderId);

            Data.StockConfirmed = true;
            await ContinueOrderProcess(context);
        }

        public Task Handle(OrderStockRejected message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Received {MessageType} for OrderId: {OrderId}", nameof(OrderStockRejected), message.OrderId);
            
            MarkAsComplete();
            return Task.CompletedTask;
        }

        public async Task Timeout(GracePeriodExpired state, IMessageHandlerContext context)
        {
            Data.GracePeriodIsOver = true;
            await ContinueOrderProcess(context);
        }

        private async Task ContinueOrderProcess(IMessageHandlerContext context)
        {
            if (Data.GracePeriodIsOver && Data.StockConfirmed)
            {
                await context.Publish(new OrderAccepted
                {
                    OrderId = Data.OrderId
                });
                MarkAsComplete();

                _logger.LogInformation("Order '{OrderId}' accepted.", Data.OrderId);
            }
        }

        public class GracePeriodState : ContainSagaData
        {
            public string OrderId { get; set; }
            public bool GracePeriodIsOver { get; set; }
            public bool StockConfirmed { get; set; }
        }
    }
}
