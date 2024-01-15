using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace UnityAIPlugin.API
{
    public static class Utils
    {
        // WrapOpenAIRequest方法用于创建一个UnityWebRequest对象，用于发送OpenAI请求
        // 参数messages是一个包含Message对象的列表，表示要发送的消息
        // 参数modelName是一个字符串，表示模型名称
        // 参数temp是一个浮点数，表示温度
        // 参数apiKey是一个字符串，表示API密钥
        // 参数apiURL是一个字符串，表示API URL
        // 参数isStream是一个布尔值，表示是否使用流式请求
        public static UnityWebRequest WrapOpenAIRequest(List<Message> messages, string modelName, float temp, string apiKey, string apiURL, bool isStream)
        {
            // 将messages列表中的每个Message对象序列化为JSON字符串，并拼接到msgStr中
            string msgStr = "";
            foreach (Message msg in messages)
            {
                msgStr += JsonConvert.SerializeObject(msg) + ",";
            }

            // 如果msgStr不为空，则去掉末尾的逗号
            if (!string.IsNullOrEmpty(msgStr))
            {
                msgStr = msgStr.TrimEnd(',');
            }

            // 构建请求体的JSON字符串
            string requestBody = $@"
                        {{
                            ""model"": ""{modelName}"",
                            ""messages"": [{msgStr}],
                            ""temperature"": {temp},
                            ""stream"": {isStream.ToString().ToLower()}
                        }}";

            // 将请求体转换为字节数组
            byte[] jsonRequestBodyBytes = System.Text.Encoding.UTF8.GetBytes(requestBody);

            // 创建一个POST请求，并设置请求体、授权头和内容类型头
            UnityWebRequest request = UnityWebRequest.Post(apiURL, "");
            request.uploadHandler = new UploadHandlerRaw(jsonRequestBodyBytes);
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            request.SetRequestHeader("Content-Type", "application/json");

            // 返回请求对象
            return request;
        }
    }
}