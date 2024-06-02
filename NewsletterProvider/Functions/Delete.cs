using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NewsletterProvider.Functions;

public class Delete(ILogger<Delete> logger, DataContext context)
{
    private readonly ILogger<Delete> _logger = logger;
    private readonly DataContext _context = context;

    [Function("Delete")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        if (!string.IsNullOrEmpty(body))
        {
            var email = body.Trim(); 

            var subscriber = await _context.Subscribers.FirstOrDefaultAsync(s => s.Email == email);
            if (subscriber != null)
            {
                _context.Subscribers.Remove(subscriber);
                await _context.SaveChangesAsync();

                return new OkObjectResult(new
                {
                    Status = 200,
                    Message = "Subscriber was successfully deleted."
                });
            }
            else
            {
                return new NotFoundObjectResult(new
                {
                    Status = 404,
                    Message = "Subscriber not found."
                });
            }
        }

        return new BadRequestObjectResult(new
        {
            Status = 400,
            Message = "Invalid request body."
        });
    }
}