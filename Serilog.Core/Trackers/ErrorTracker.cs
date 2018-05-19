using System;
using System.Collections.Generic;
using System.Globalization;

namespace Serilog.Core.Trackers
{
    public class ErrorTracker
    {
        private readonly LogDetail _logDetail;

        public ErrorTracker(string userId, string location, string product, string layer, Exception e, string corelcationId)
        {
            _logDetail = new LogDetail
            {
                UserId = userId,
                Product = product,
                Layer = layer,
                Location = location,
                Hostname = Environment.MachineName,
                Exception = e,
                CorrelationId = corelcationId
            };

            var beginTime = DateTime.Now;

            _logDetail.AdditionalInfo = new Dictionary<string, object>
            {
                {"Received", beginTime.ToString(CultureInfo.InvariantCulture)}
            };
        }

        public ErrorTracker
        (
            string userId,
            string location,
            string product,
            string layer,
            Exception e,
            string corelcationId,
            Dictionary<string, object> perfParams
        ) : this(userId, location, product, layer, e, corelcationId)
        {
            if(perfParams == null) return;
            foreach (var param in perfParams)
            {
                _logDetail.AdditionalInfo.Add($"input-{param.Key}", param.Value);
            }
        }

        public void Stop()
        {
            Logger.WriteError(_logDetail);
        }
    }
}