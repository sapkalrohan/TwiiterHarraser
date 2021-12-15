using System;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Parameters.V2;
using Tweetinvi.Models;
using System.Collections.Generic;
using Tweetinvi.Parameters;
namespace TwitterBot
{
    class Program
    {
        static DateTime showDate = new DateTime(2021, 12, 4, 14, 0, 0);
        static TwitterClient userClient = new TwitterClient("consumerkey", "consumersecret", "accesstoken", "accesssecret");
        static async Task Main(string[] args)
        {

            // request the user's information from Twitter API
            var user = await userClient.Users.GetAuthenticatedUserAsync();
            Console.WriteLine("Hello " + user);

            //publish a tweet
            // var tweet = await userClient.Tweets.PublishTweetAsync("Hello tweetinvi world!");
            // Console.WriteLine("You published the tweet : " + tweet);
            //--------------------v2

            // // await userClient.StreamsV2.AddRulesToFilteredStreamAsync(new FilteredStreamRuleConfig("from:bookmyshow OR from:bookmyshow_sup"));
            // // var fs = userClient.StreamsV2.CreateFilteredStream();

            // // fs.TweetReceived += (sender, args) =>
            // // {
            // //     System.Console.WriteLine($"Tweet receive : {args.Tweet.Text}");
            // // };

            // // await fs.StartAsync();
            //--------------------v2

            var stream = userClient.Streams.CreateFilteredStream();
            var toFollow = new List<long>() { 10650592, 69219061 };//,bookmyshow,bookmyshow_sup
            toFollow.ForEach(id =>
            {
                stream.AddFollow(id);
            });

            // stream.AddTrack("from:bookmyshow OR from:bookmyshow_sup");
            // stream.AddTrack("from:Anthony08687141");

            stream.StreamStarted += (sender, args) =>
            {
                Console.WriteLine("Stream STARTED");
            };
            stream.MatchingTweetReceived += async (sender, eventReceived) =>
            {
                if (toFollow.Contains(eventReceived.Tweet.CreatedBy.Id))
                {
                    Console.WriteLine("MATCH");
                    Console.WriteLine(eventReceived.Tweet);
                    await ReplyWithReminderTweet(eventReceived.Tweet);
                }
            };

            stream.StreamStopped += async (sender, args) =>
            {
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(args.Exception));
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(args.DisconnectMessage));
                await stream.StartMatchingAllConditionsAsync();
            };
            await stream.StartMatchingAllConditionsAsync();

            Console.WriteLine("END");
        }

        static async Task<ITweet> ReplyWithReminderTweet(ITweet tweet)
        {
            TimeSpan span = (DateTime.Now - showDate);
            var replyArg = new PublishTweetParameters(
string.Format(@"@{1} It has been exactly {0} since you have not refunded my money for cancelled Kunal Kamras's show on 4th dec in bangalore
Booking ID ATHJ00KBSZXCFZ
@bookmyshow_sup @bookmyshow @fafsters @BmsStream", String.Format("{0}days,{1}hrs,{2}mins,{3}s", span.Days, span.Hours, span.Minutes, span.Seconds), tweet.CreatedBy));
            replyArg.InReplyToTweet = tweet;
            var reply = await userClient.Tweets.PublishTweetAsync(replyArg);
            return reply;
        }
    }
}
