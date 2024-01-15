using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;
using System.Collections;

namespace UnityAIPlugin.API
{
    public class OpenAIClient
    {
        public string model;
        public string prompt;
        public List<Message> context;

        private float temp;

        public OpenAIClient(string _model = "gpt-3.5-turbo", string _prompt = "Your are a helpful assistant.", float _temp = 0.9f)
        {
            context = new List<Message>();
            this.prompt = _prompt;
            this.temp = _temp;
            this.model = _model;
        }

        public void SendMessageAsync(string message, UnityAction<string> onSuccess = null, bool isRecord = true)
        {
            List<Message> messages = WrapMessage(message, isRecord);

            UnityAction<string> callback = onSuccess + UpdateContext;

            OpenAIApi.Instance.SendMessageAsync(messages, model, temp, callback);
        }

        // SendMessage Task version
        public async Task<Response> SendMessageTask(string message, bool isRecord = true)
        {
            List<Message> messages = WrapMessage(message, isRecord);

            return await OpenAIApi.Instance.SendMessageTask(messages, model, temp);
        }

        // SendMessage Coroutine version
        public IEnumerator SendMessageCoroutine(string message, UnityAction<string> onSuccess = null, bool isRecord = true)
        {
            List<Message> messages = WrapMessage(message, isRecord);

            UnityAction<string> callback = onSuccess + UpdateContext;

            return OpenAIApi.Instance.SendMessageCoroutine(messages, model, temp, callback);
        }

        public IEnumerator SendMessageStreamCoroutine(string message, UnityAction<string> onResponse = null, UnityAction onComplete = null, bool isRecord = true)
        {
            List<Message> messages = WrapMessage(message, isRecord);

            UnityAction<string> onResponseDone = UpdateContext;

            return OpenAIApi.Instance.SendMessageStreamCoroutine(messages, model, temp, onResponse, onResponseDone, onComplete);
        }

        public void ClearHistory()
        {
            context.Clear();
        }

        void UpdateContext(string message)
        {
            context.Add(new Message("assistant", message));
        }

        List<Message> WrapMessage(string message, bool isRecord)
        {
            Message messageUser = new Message("user", message);
            List<Message> messages = new List<Message>() { new Message("system", prompt) };

            context.Add(messageUser);
            if (isRecord) messages.AddRange(context);
            else messages.Add(messageUser);

            return messages;
        }
    }
}
