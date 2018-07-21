using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using TwitchVods.Core.Models;

namespace TwitchVods.Core.Twitch.Helix
{
    internal class HelixTwitchClient : ITwitchClient
    {
        private readonly IDurationParser _durationParser;
        protected const string UrlBase = "https://api.twitch.tv/helix";

        private readonly string _channelName;
        private readonly Settings _settings;
        private readonly IAsyncPolicy _retryRetryPolicy;
        private string _channelUserId;

        public HelixTwitchClient(string channelName, Settings settings, IAsyncPolicy retryPolicy, IDurationParser durationParser)
        {
            _durationParser = durationParser;
            _channelName = channelName ?? throw new ArgumentNullException();

            if (string.IsNullOrWhiteSpace(_channelName))
                throw new ArgumentNullException();

            _settings = settings ?? throw new ArgumentNullException();
            _retryRetryPolicy = retryPolicy ?? throw new ArgumentNullException();

            if (string.IsNullOrEmpty(_channelUserId))
                SetUserId();
        }

        private void SetUserId()
        {
            if (!string.IsNullOrWhiteSpace(_channelUserId))
                return;

            var apiEndpoint = $"{UrlBase}/users?login={_channelName}";

            var request = CreateWebRequest(apiEndpoint);

            var webResponse = request.GetResponse();

            using (var reader = new StreamReader(webResponse.GetResponseStream()))
            {
                dynamic jsonData = JObject.Parse(reader.ReadToEnd());
                _channelUserId = jsonData["data"][0]["id"].ToString();
            }
        }

        private HttpWebRequest CreateWebRequest(string apiEndpoint)
        {
            var request = (HttpWebRequest)WebRequest.Create(apiEndpoint);

            // Need to specify the client ID https://blog.twitch.tv/client-id-required-for-kraken-api-calls-afbb8e95f843#.j496nqkhq  
            request.Headers.Add("Client-ID", _settings.TwitchApiClientId);

            return request;
        }

        public async Task<Channel> GetChannelVideosAsync()
        {
            var channel = Channel.Create(_channelName);
            const int limit = 100;
            var cursor = string.Empty;
            
            do
            {
                cursor = await _retryRetryPolicy.ExecuteAsync(() => PopulateVideos(limit, cursor, channel));

            } while (!string.IsNullOrEmpty(cursor));

            return channel;
        }

        private async Task<string> PopulateVideos(int limit, string cursor, Channel channel)
        {
            var apiEndpoint = GetChannelVideosEndpoint(limit, cursor);

            var request = CreateWebRequest(apiEndpoint);

            var webResponse = await request.GetResponseAsync();

            using (var reader = new StreamReader(webResponse.GetResponseStream()))
            {
                var readerOutput = await reader.ReadToEndAsync();
                var response = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<HelixVideoResponse>(readerOutput));

                if (response.data.Count == 0)
                    return string.Empty;
                
                foreach (var data in response.data)
                {
                    var video = Video.Create(data.id, data.title, -1, data.created_at, "GAME!!!", _durationParser.FromDuration(data.duration), data.url, data.view_count);

                    channel.AddVideo(video);
                }

                Console.WriteLine("{0} Retrieved: {1}", _channelName, channel.TotalVideoCount);

                return response.pagination.cursor;
            }
        }

        private string GetChannelVideosEndpoint(int limit, string cursor)
        {
            return $"{UrlBase}/videos?user_id={_channelUserId}&type=archive&first={limit}&after={cursor}";
        }
    }
}
