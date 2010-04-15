﻿using System;
using Quartz;
using TweetSharp.Fluent;
using Zmanim.Examples.QuartzScheduling.Properties;

namespace Zmanim.Examples.QuartzScheduling
{
    public class TweetZmanimJob : IJob
    {
        public void Execute(JobExecutionContext context)
        {
            var simpleTrigger = ((SimpleTrigger)context.Trigger);
            simpleTrigger.StartTimeUtc = SchedulerHelper.GetZman(DateTime.Now.AddDays(1)).AddMinutes(-30);

            DateTime zmanSunset = SchedulerHelper.GetZman(DateTime.Now).ToLocalTime();

            var twitter = FluentTwitter.CreateRequest()
                .AuthenticateAs(Settings.Default.TWITTER_USERNAME, Settings.Default.TWITTER_PASSWORD)
                .Statuses().Update(
                        string.Format("The Shekiat Hachama today for Brooklyn, NY will be at: {0}. Please remember to daven Mincha. #Mincha #Jewish #Torah #Daven", zmanSunset.ToShortTimeString())
                    )
                .AsJson().Request();
        }
    }
}