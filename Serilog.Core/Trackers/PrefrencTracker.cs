using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Serilog.Core.Trackers
{
    public class PrefrencTracker
    {
        private readonly Stopwatch _stopwatch;
        private readonly LogDetail _logDetail;

        public PrefrencTracker(string name, string userId, string location, string product, string layer)
        {
            _stopwatch = Stopwatch.StartNew();
            _logDetail = new LogDetail
            {
                Message = name,
                UserId = userId,
                Product = product,
                Layer = layer,
                Location = location,
                Hostname = Environment.MachineName
            };

            var beginTime = DateTime.Now;

            _logDetail.AdditionalInfo = new Dictionary<string, object>
            {
                {"Started", beginTime.ToString(CultureInfo.InvariantCulture)}
            };
        }

        public PrefrencTracker
        (
            string name,
            string userId,
            string location,
            string product,
            string layer,
            Dictionary<string, object> perfParams
        ) : this(name, userId, location, product, layer)
        {
            foreach (var param in perfParams)
            {
                _logDetail.AdditionalInfo.Add($"input-{param.Key}", param.Value);
            }
        }
        
        public void Stop()
        {
            _stopwatch.Stop();
            _logDetail.ElapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
            Logger.WritePerf(_logDetail);
        }
    }
}