using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RGS.Backend.Services;
using RGS.Backend.Shared.Models;

namespace RGS.Backend;

internal class SetPostingNotes
{
    private readonly IUserDataRepository _userDataRepository;

    public SetPostingNotes(IUserDataRepository userDataRepository)
    {
        _userDataRepository = userDataRepository;
    }

    [Function("SetPostingNotes")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        // TODO: CSRF protection
        // Currently using cookie-based authentication built into Azure Functions, and thus vulnerable to CSRF.
        // Fixes include changing to token-based authentication in headers or implementing anti-CSRF tokens.

        var payload = await req.ReadFromJsonAsync<Notes>();

        if (payload is null)
        {
            return new BadRequestResult();
        }

        var result = await _userDataRepository.SetPostingNotesAsync(payload);

        return result.ToActionResult();
    }
}