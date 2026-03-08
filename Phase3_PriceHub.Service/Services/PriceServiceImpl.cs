using Grpc.Core;
using Pricehub.V1;

namespace PriceHub.Service.Services
{
    public class PriceServiceImpl : PriceService.PriceServiceBase
    {
        /// <summary>
        /// Server-streaming RPC: Returns prices matching the filter criteria.
        /// Maps to SOAP GetPrice operation.
        /// TODO: Replace demo data with repository calls.
        /// </summary>
        public override async Task StreamPrices(PriceFilter request, IServerStreamWriter<Price> responseStream, ServerCallContext context)
        {
            // Demo data - replace with ListPriceRepository.QueryAsync() in Phase 4
            var demo = new List<Price>
            {
                new Price 
                { 
                    Sku = request.Sku ?? "SKU-1000", 
                    Currency = request.Currency ?? "USD", 
                    Price_ = "129.99", 
                    EffectiveDate = request.EffectiveDate ?? "2025-01-01" 
                },
                new Price 
                { 
                    Sku = request.Sku ?? "SKU-1000", 
                    Currency = request.Currency ?? "USD", 
                    Price_ = "119.99", 
                    EffectiveDate = "2024-12-01" 
                }
            };

            foreach (var p in demo)
            {
                if (context.CancellationToken.IsCancellationRequested)
                    break;
                    
                await responseStream.WriteAsync(p);
                await Task.Delay(10, context.CancellationToken); // simulate streaming
            }
        }

        /// <summary>
        /// Unary RPC: Triggers a price calculation job.
        /// Maps to SOAP SubmitPriceJob operation.
        /// TODO: Replace with PriceJobRepository.CreateAsync() in Phase 4.
        /// </summary>
        public override Task<TriggerJobResponse> TriggerPriceJob(TriggerJobRequest request, ServerCallContext context)
        {
            // Stub: accept a job and return a generated id
            var jobId = $"JOB-{Guid.NewGuid():N}";
            return Task.FromResult(new TriggerJobResponse { AcceptedJobId = jobId });
        }
    }
}
