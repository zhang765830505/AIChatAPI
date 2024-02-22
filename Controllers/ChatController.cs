using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Data;

namespace AIChatAPI.Controllers
{
    [ApiController]
    [Route("aichat/api/[controller]")]
    public class ChatController : ControllerBase
    {


        private readonly ILogger<ChatController> _logger;
        private IMemoryCache _cache;
        private string url = $"https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/completions_pro?access_token={BaiduTokenTask.access_token}";


        public ChatController(ILogger<ChatController> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }
        static readonly HttpClient client = new HttpClient();
        [HttpGet(Name = "Chat")]
        public async Task<IActionResult> NewChat()
        {
            try
            {
                //  var accessToken = await GetAccessToken();


                var payload = new Payload
                {
                    messages = new List<Message>
                    {
                        new Message{ role = "user", content = "����һ���г̹滮ר��������ҹ滮�г̻���·��һ�������ʱ��Ҫ����{day:'',distance:'',illustrate:'',location:'',locationDesc:''}������json��ʽ����,��������ͨ�Ի���day�ǵڼ����뷵�ش����֣�distance��·�̣�illustrate��˵��,location�Ǿ�������,locationDesc�Ǿ�������.����������˾ͷ���ok������ĸ�����Ҳ�Ҫ˵����Ļ���Ҳ����ʾ��,�����ڲ���صĻش�ʱ�Ҳ�ϣ����˵�������json��ʽ��صĶ�����" },
                    }
                };

                var jsonString = JsonConvert.SerializeObject(payload);

                var httpContent = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, httpContent);

                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseBody);
                if (string.IsNullOrEmpty( apiResponse?.Result)) {
                    await Console.Out.WriteLineAsync(responseBody);
                    return Content("�������");
                }
                payload.messages.Add(new Message { role = "assistant", content = apiResponse.Result });
                string key = Guid.NewGuid().ToString();
                _cache.Set(key, payload);
                return Content(key);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine($"Message :{e.Message} ");
                return Content("�������");

            }
        }

        [HttpPost("ContinueChat")]
        public async Task<IActionResult> ContinueChat([FromBody] UserMsg userMsg)
        {

            try
            {


                if (_cache.TryGetValue(userMsg.key, out Payload messages))
                {

                    messages.messages.Add(new Message
                    {
                        role = "user",
                        content = userMsg.msg
                    });

                    var jsonString = JsonConvert.SerializeObject(messages);

                    var httpContent = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, httpContent);

                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseBody);
                    if (string.IsNullOrEmpty(apiResponse?.Result))
                    {
                        await Console.Out.WriteLineAsync(responseBody);
                        return Ok(new ReturnRes
                        {
                            IsJson = false,
                            Result = "�������"
                        });
                    }
                    messages.messages.Add(new Message { role = "assistant", content = apiResponse.Result });

                    _cache.Set(userMsg.key, messages);

                    int firstBracket = apiResponse.Result.IndexOf('[');
                    int lastBracket = apiResponse.Result.LastIndexOf(']');
                    if (firstBracket >= 0 && lastBracket >= 0 && lastBracket > firstBracket)
                    {
                        string jsonSubString = apiResponse.Result.Substring(firstBracket, lastBracket - firstBracket + 1);
                        //List<Itinerary> itineraries = JsonConvert.DeserializeObject<List<Itinerary>>(jsonSubString);
                        // do something with itineraries

                        return Ok(new ReturnRes
                        {
                            IsJson = true,
                            Result = jsonSubString
                        });
                    }
                    else
                    {
                        return Ok(new ReturnRes
                        {
                            IsJson = false,
                            Result = apiResponse.Result
                        });
                    }




                }

                else
                {
                    return Ok(new ReturnRes
                    {
                        IsJson = false,
                        Result = "�������"
                    });
                }
            }
            catch (Exception ex)
            {

                return Ok(new ReturnRes
                {
                    IsJson = false,
                    Result = "�������"
                });
            }
        }
    }


    public class UserMsg
    {
        public string key { get; set; }

        public string msg { get; set; }
    }

    public class ReturnRes
    {
        public string Result { get; set; }

        public bool IsJson { get; set; } = false;
    }
    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class Payload
    {
        public List<Message> messages { get; set; }
    }


    public class ApiResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long Created { get; set; }
        public string Result { get; set; }
        public bool IsTruncated { get; set; }
        public bool NeedClearHistory { get; set; }
        public string FinishReason { get; set; }
        public Usage Usage { get; set; }
    }

    public class Usage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }

    public class Itinerary
    {
        public string Day { get; set; }
        public string Distance { get; set; }
        public string Illustrate { get; set; }
    }

}