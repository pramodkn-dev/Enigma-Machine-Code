using Grpc.Core;
using PriceHub.Service.Protos;
using PriceHub.Service.Store;

namespace PriceHub.Service.Services {
  public class PriceServiceImpl : PriceService.PriceServiceBase {
    private readonly ListPriceRepository _prices = new(new OracleDb());
    private readonly PriceJobRepository _jobs = new(new OracleDb());
    public override async Task StreamPrices(PriceQuery request, IServerStreamWriter<PriceRow> stream, ServerCallContext ctx) {
      foreach (var r in _prices.Query(request.Sku, request.Market, null)) {
        await stream.WriteAsync(new PriceRow { Sku=r.SKU, Currency=r.CURRENCY, Price=r.PRICE.ToString("0.####"), EffectiveDate=r.EFFECTIVE_DATE.ToString("yyyy-MM-dd") });
      }
    }
    public override Task<PriceJobOut> TriggerPriceJob(PriceJobIn request, ServerCallContext ctx) {
      var id=_jobs.Create(request.Market);
      return Task.FromResult(new PriceJobOut { JobId=id, Status="QUEUED" });
    }
  }
}
