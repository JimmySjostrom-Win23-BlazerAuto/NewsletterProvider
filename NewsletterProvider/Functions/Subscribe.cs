using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NewsletterProvider.Functions
{
    public class Subscribe
    {
        private readonly ILogger<Subscribe> _logger;
        private readonly DataContext _context;

        public Subscribe(ILogger<Subscribe> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Function("Subscribe")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(body))
            {
                var subscribeEntity = JsonConvert.DeserializeObject<SubscribeEntity>(body);
                if (subscribeEntity != null)
                {
                    var existingSubscriber = await _context.Subscribers.FirstOrDefaultAsync(s => s.Email == subscribeEntity.Email);
                    if (existingSubscriber != null)
                    {
                        // Update only subscription options
                        existingSubscriber.DailyNewsletter = subscribeEntity.DailyNewsletter;
                        existingSubscriber.AdvertisingUpdates = subscribeEntity.AdvertisingUpdates;
                        existingSubscriber.WeekinReview = subscribeEntity.WeekinReview;
                        existingSubscriber.EventUpdates = subscribeEntity.EventUpdates;
                        existingSubscriber.StartupsWeekly = subscribeEntity.StartupsWeekly;
                        existingSubscriber.Podcasts = subscribeEntity.Podcasts;

                        // Save changes to the database
                        await _context.SaveChangesAsync();

                        return new OkObjectResult(new
                        {
                            Status = 200,
                            Message = "Subscription options were updated."
                        });
                    }

                    // If subscriber doesn't exist, add them to the database
                    _context.Subscribers.Add(subscribeEntity);
                    await _context.SaveChangesAsync();
                    return new OkObjectResult(new
                    {
                        Status = 200,
                        Message = "Subscriber was added to the newsletter."
                    });
                }
            }

            return new BadRequestObjectResult(new
            {
                Status = 400,
                Message = "Unable to subscribe right now."
            });
        }
    }
}
