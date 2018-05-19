using System;
using Betting.Backend.Factories;
using Betting.Backend.Managers.Interface;
using FluentScheduler;
using NodaTime;

namespace Betting.Backend.Managers.Impl
{
    public class JobScheduleManager :Registry, IJobScheduleManager
    {
        private readonly IPricingServiceFactory _pricingServiceFactory;

        public JobScheduleManager(IPricingServiceFactory pricingServiceFactory)
        {
            _pricingServiceFactory = pricingServiceFactory;
        }

        public void Start()
        {
            var currentUtc = DateTime.UtcNow;

            var eastern = Instant.FromDateTimeUtc(currentUtc)
                .InZone(DateTimeZoneProviders.Tzdb["America/New_York"])
                .ToDateTimeUnspecified();
            
            var currentTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(currentUtc, TimeZoneInfo.Local.Id);
            var hoursUntillMidnight = 24 - eastern.Hour;
            var timeWhenToUpdate = currentTime.AddHours(hoursUntillMidnight);
            var hoursWhenToUpdate = timeWhenToUpdate.Hour + 2;
            Schedule(() => UpdatePricing()).ToRunEvery(1).Days().At(hoursWhenToUpdate, 0);

            Console.WriteLine($"Server time is {currentTime}");
            Console.WriteLine($"Schedule job {nameof(UpdatePricing)} to run everyday @{hoursWhenToUpdate}:00 server time!");
        }


        private async void UpdatePricing()
        {
//            var pricingService = _pricingServiceFactory.GetPricingService(PricingServices.CsgoFast);
//            await pricingService.UpdatePricingForPubgAsync();
//            await pricingService.UpdatePricingForCsgoAsync();
        }
    }
}