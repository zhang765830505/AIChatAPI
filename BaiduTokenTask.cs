

using Newtonsoft.Json;
using RestSharp;

namespace AIChatAPI
{
    /// <summary>
    /// 小程序接口访问 access_token 的存储与更新
    /// </summary>
    public class BaiduTokenTask
    {
        public static string access_token = string.Empty;
        private static System.Timers.Timer timer;
        private static string appid = "";
        private static string secret = "";
        public static void Init()
        {
            timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 5000;
            timer.AutoReset = true;
            timer.Start();
            Timer_Elapsed(null, null);

        }

        private static void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {

                    var url = "https://aip.baidubce.com/oauth/2.0/token";
                    var values = new Dictionary<string, string>
                    {
                        { "grant_type", "client_credentials" },
                        { "client_id", "自己id" },
                        { "client_secret", "自己的secret" }
                    };

                    using (HttpClient client = new HttpClient())
                    {
                        var content = new FormUrlEncodedContent(values);
                        HttpResponseMessage response = await client.PostAsync(url, content);
                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();
                            BaiduTokenResponse tokenResponse = JsonConvert.DeserializeObject<BaiduTokenResponse>(responseContent);

                            access_token = tokenResponse.access_token;
                            timer.Interval = (tokenResponse.expires_in - 1500) * 1000;
                        }
                        else
                        {
                            Console.WriteLine($"Error: {response.StatusCode}");
                        }
                    }

                    //  AccessTokenData data = await $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appid}&secret={secret}".GetAsAsync<AccessTokenData>();
                    //if (response.IsSuccessStatusCode)
                    //{
                    //    access_token = response.Content.;

                    //    timer.Interval = (data.expires_in - 1500) * 1000;//提前进行刷新
                    //}
                }
                catch (Exception ex)
                {


                }

            });

        }
    }
    public class AccessTokenData
    {
        /// <summary>
        /// 接口全局唯一token
        /// </summary>
        public string? access_token { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int expires_in { get; set; }
    }
}
