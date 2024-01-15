using System.Collections.Generic;

namespace UnityAIPlugin.API
{
    public struct OpenAIDataStruct
    {
        public List<Choice> choices;
    }
    public struct Choice
    {
        public Message delta;
        public Message message;
    }
    public struct Message
    {
        public string role;
        public string content;

        public Message(string _role, string _content)
        {
            this.role = _role;
            this.content = _content;
        }
    }
    public struct Config
    {
        public string apiKey;
        public string apiURL;
    }

    public struct Response
    {
        public bool status;
        public string message;

        public Response(bool _status, string _message)
        {
            this.status = _status;
            this.message = _message;
        }
    }
}