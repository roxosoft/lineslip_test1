using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Demo1.LimitRequests
{
    public class Throttler
    {
        // Use this constant as average rate to disable throttling
        public const long NoLimit = -1;
        // Number of consumed tokens
        private long _consumedTokens;
        // timestamp of last refill time
        private long _lastRefillTime;
        // ticks per period
        private long _periodTicks;

        private double _averageRate;

        public long BurstSize
        {
            get;
            set;
        }

        public long AverageRate
        {
            get { return (long)_averageRate; }
            set { _averageRate = value; }
        }

        public TimeSpan Period
        {
            get
            {
                return new TimeSpan(_periodTicks);
            }
            set
            {
                _periodTicks = value.Ticks;
            }
        }
        public Throttler()
        {
            BurstSize = 1;
            AverageRate = NoLimit;
            Period = TimeSpan.FromSeconds(1);
        }

        /// <summary>
        /// Create a Throttler
        /// ex: To throttle to 1024 byte per seconds with burst of 200 byte use
        /// new Throttler(1024,TimeSpan.FromSeconds(1), 200);
        /// </summary>
        /// <param name="averageRate">The number of tokens to add to the bucket every interval. </param>
        /// <param name="period">Timespan of on interval.</param>
        /// <param name="burstSize"></param>
        public Throttler(long averageRate, TimeSpan period, long burstSize = 1)
        {
            BurstSize = burstSize;
            AverageRate = averageRate;
            Period = period;
        }

        public long GetTokensLeft(long amount)
        {
            if (BurstSize <= 0 || _averageRate <= 0)
            { // Instead of throwing exception, we just let all the traffic go
                return Int64.MaxValue;
            }
            RefillToken();
            return ConsumeTokenLeft(amount);
        }

        public bool TryThrottledWait(long amount)
        {
            if (BurstSize <= 0 || _averageRate <= 0)
            { // Instead of throwing exception, we just let all the traffic go
                return true;
            }
            RefillToken();
            return ConsumeToken(amount);
        }

        private bool ConsumeToken(long amount)
        {
            while (true)
            {
                long currentLevel = System.Threading.Volatile.Read(ref _consumedTokens);
                if (currentLevel + amount > BurstSize)
                {
                    return false; // not enough space for amount token
                }

                if (Interlocked.CompareExchange(ref _consumedTokens, currentLevel + amount, currentLevel) == currentLevel)
                {
                    return true;
                }
            }
        }

        private long ConsumeTokenLeft(long amount)
        {
            while (true)
            {
                long currentLevel = System.Threading.Volatile.Read(ref _consumedTokens);
                if (currentLevel + amount > BurstSize)
                {
                    return currentLevel; // not enough space for amount token
                }

                if (Interlocked.CompareExchange(ref _consumedTokens, currentLevel + amount, currentLevel) == currentLevel)
                {
                    return currentLevel;
                }
            }
        }

        public void ThrottledWait(long amount)
        {
            while (true)
            {
                if (TryThrottledWait(amount))
                {
                    break;
                }

                long refillTime = System.Threading.Volatile.Read(ref _lastRefillTime);
                long nextRefillTime = (long)(refillTime + (_periodTicks / _averageRate));
                long currentTimeTicks = DateTime.UtcNow.Ticks;
                long sleepTicks = Math.Max(nextRefillTime - currentTimeTicks, 0);
                TimeSpan ts = new TimeSpan(sleepTicks);
                Thread.Sleep(ts);
            }
        }

        /// <summary>
        /// Compute elapsed time using DateTime.UtcNow.Ticks and refil token using _periodTicks and _averageRate
        /// </summary>
        private void RefillToken()
        {
            long currentTimeTicks = DateTime.UtcNow.Ticks;
            // Last refill time in  ticks unit
            long refillTime = System.Threading.Volatile.Read(ref _lastRefillTime);
            // Time delta in ticks unit
            long TicksDelta = currentTimeTicks - refillTime;
            long newTokens = (long)(TicksDelta * _averageRate / _periodTicks);
            if (newTokens > 0)
            {
                long newRefillTime = refillTime == 0
                    ? currentTimeTicks
                    : refillTime + (long)(newTokens * _periodTicks / _averageRate);

                if (Interlocked.CompareExchange(ref _lastRefillTime, newRefillTime, refillTime) == refillTime)
                {
                    // Loop until we succeed in refilling "newTokens" tokens
                    while (true)
                    {
                        long currentLevel = System.Threading.Volatile.Read(ref _consumedTokens);
                        long adjustedLevel = (long)Math.Min(currentLevel, BurstSize); // In case burstSize decreased
                        long newLevel = (long)Math.Max(0, adjustedLevel - newTokens);
                        if (Interlocked.CompareExchange(ref _consumedTokens, newLevel, currentLevel) == currentLevel)
                        {
                            return;
                        }
                    }
                }
            }
        }
    }
}
