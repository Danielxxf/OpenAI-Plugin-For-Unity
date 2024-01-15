using UnityAIPlugin.API;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace UnityAIPlugin.Editor
{
    public class UnityAIEditorClient
    {
        private string prompt;
        private OpenAIClient openaiClient;

        private static UnityAIEditorClient instance;

        public static UnityAIEditorClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UnityAIEditorClient();
                }
                return instance;
            }
        }

        private UnityAIEditorClient()
        {
            prompt = "你是一个经验丰富、技艺高超的全栈开发高级工程师，你的回答除了代码（包括代码内注释）以外不能有其他文字说明。";
            openaiClient = new OpenAIClient(prompt, "gpt-3.5-turbo", 0.5f);
        }

        public async Task<Response> Process(string content, string model, string option)
        {
            openaiClient.model = model;
            Response response = await openaiClient.SendMessageTask($"{option}\n" + content, false);
            string newLine = Utils.GetNewLine(content);

            if (response.status)
            {
                string pattern = @"(?<!\r)\n";
                // 使用正则表达式替换换行符
                response.message = Regex.Replace(response.message, pattern, newLine);
                if (response.message.Contains("\n```"))
                {
                    response.message = Utils.RemoveMarkdown(response.message);
                }
            }

            string originCode = Utils.CommentAll(content, newLine);

            response.message += newLine + originCode;

            return response;
        }
    }
}