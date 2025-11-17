using container.Services;
using Microsoft.Azure.Functions.Worker;

namespace RGS.Backend;

public class PendingPostingsCatchupTrigger(PostingProcessor postingProcessor)
{
    private PostingProcessor _postingProcessor = postingProcessor;

    [Function("PendingPostingsCatchupTrigger")]
    public async Task Run([TimerTrigger("0 0 17 * * *")] TimerInfo myTimer)
  {
        await _postingProcessor.CatchUp();
  }
}