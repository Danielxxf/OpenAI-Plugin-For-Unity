using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Text;
using System.Linq;

namespace UnityAIPlugin.API
{
    public class OpenAIApi
    {
        private string apiKey;
        private string apiURL;

        public string ApiKey { set { apiKey = value; } }

        public string ApiURL { private get; set; }

        private static OpenAIApi instance;

        public static OpenAIApi Instance { 
            get { 
                if(instance == null)
                    instance = new OpenAIApi();
                return instance;
            } 
        }

        private OpenAIApi()
        {
            LoadConfig();
        }

        // Load API key and URL from config.json
        public void LoadConfig()
        {
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string authPath = $"{userPath}/.openai/auth.json";

            if (File.Exists(authPath))
            {
                string configJson = File.ReadAllText(authPath);
                Config configData = JsonConvert.DeserializeObject<Config>(configJson);

                // Set the API key and URL
                apiKey = configData.apiKey;
                apiURL = configData.apiURL;
            }
            else
            {
                Debug.LogError($"auth.json not found in the {authPath}!");
            }
        }

        public async void SendMessageAsync(List<Message> messages, string model, float temp, UnityAction<string> onSuccess = null)
        {
            using (UnityWebRequest request = Utils.WrapOpenAIRequest(messages, model, temp, apiKey, apiURL, false))
            {
                var asyncOperation = request.SendWebRequest();

                while (!asyncOperation.isDone) await Task.Yield();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    // Failed
                    Debug.LogError("HTTP request error: " + request.error);
                }
                else
                {
                    // Success
                    string responseData = request.downloadHandler.text;
                    string reply = JsonConvert.DeserializeObject<OpenAIDataStruct>(responseData).choices[0].message.content;

                    messages.Add(new Message("assistant", reply));

                    onSuccess?.Invoke(reply);
                }
            }
        }

        // SendMessage Task version
        public async Task<Response> SendMessageTask(List<Message> messages, string model, float temp)
        {
            using (UnityWebRequest request = Utils.WrapOpenAIRequest(messages, model, temp, apiKey, apiURL, false))
            {
                var asyncOperation = request.SendWebRequest();
                while (!asyncOperation.isDone) await Task.Yield();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    // Failed
                    Debug.LogError("HTTP request error: " + request.error);
                    return new Response(false, "Error: " + request.error);
                }
                else
                {
                    string responseData = request.downloadHandler.text;
                    Debug.Log(responseData);
                    // Success
                    string reply = JsonConvert.DeserializeObject<OpenAIDataStruct>(responseData).choices[0].message.content;

                    messages.Add(new Message("assistant", reply));
                    return new Response(true, reply);
                }
            }
        }

        // SendMessage Coroutine version
        public IEnumerator SendMessageCoroutine(List<Message> messages, string model, float temp, UnityAction<string> onSuccess = null)
        {
            using (UnityWebRequest request = Utils.WrapOpenAIRequest(messages, model, temp, apiKey, apiURL, false))
            {
                Debug.Log(apiURL);
                var asyncOperation = request.SendWebRequest();
                while (!asyncOperation.isDone) yield return null;

                Debug.Log(request.result);

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("HTTP request error: " + request.error);
                }
                else
                {
                    string responseData = request.downloadHandler.text;
                    Debug.Log(responseData);
                    string reply = JsonConvert.DeserializeObject<OpenAIDataStruct>(responseData).choices[0].message.content;

                    messages.Add(new Message("assistant", reply));

                    onSuccess?.Invoke(reply);
                }
            }
        }

        // SendMessage Coroutine Streaming version
        public IEnumerator SendMessageStreamCoroutine(
            List<Message> messages, string model, float temp, 
            UnityAction<string> onResponse = null, UnityAction<string> onResponseDone = null, UnityAction onComplete = null, UnityAction<string> onError = null)
        {
            using (UnityWebRequest request = Utils.WrapOpenAIRequest(messages, model, temp, apiKey, apiURL, true))
            {
                var asyncOperation = request.SendWebRequest();
                string reply = null;
                bool isDone = false;

                // Ϊʲô���ܽ� asyncOperation.isDone ֱ����Ϊѭ����������
                // һ��ʼ�ο����� github ĳ��Դ���ʵ�֣��� asyncOperation.isDone ֱ����Ϊ while ѭ��������
                // ������ʵ�ʵ����У���һ������ĩβ�����ݻᶪʧ��
                // Ҳ��˷���һ�����⣬��UnityWebRequest�������������ں�̨�߳���ִ�еģ����������߳�
                // ��ѭ�����ڽ���һ�ζ����ݵĴ���ʱ��UnityWebRequest���ܵ����µ����ݣ����һ�����ݣ�����ʱ asyncOperation.isDone Ϊ true��ѭ�������ж�ΪFalse�����ٳ�����һ��ѭ�����������һ�εõ������ݣ����������ĩβ���ݶ�ʧ
                // asyncOperation.isDone == true ʱ���������ݴ�����ɣ���ʱ��ʵҪ�������һ�ζ����ݣ��������ݣ��Ĳ���
                // ��̳�ٷ���Ա���https://forum.unity.com/threads/is-unitywebrequest-threaded-to-background.514360/
                while (!isDone)
                {
                    yield return new WaitForSeconds(0.1f);// ����̫Ƶ����������

                    if (request.result == UnityWebRequest.Result.ConnectionError ||
                        request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        reply = request.error;
                        onError?.Invoke(request.error);
                        Debug.Log("HTTP request error: " + request.error);
                        onResponse?.Invoke(reply);
                        break;
                    }

                    isDone = asyncOperation.isDone;
                    if (request.downloadHandler.text != "")
                    {
                        reply = "";
                        string[] lines = request.downloadHandler.text.Split("\n");
                        foreach (string line in lines)
                        {
                            if (line.StartsWith("data: "))
                            {
                                if (line.Substring(6) == "[DONE]")
                                {
                                    Debug.Log("��⵽[DONE]���������");
                                    break;
                                }
                                string content = JsonConvert.DeserializeObject<OpenAIDataStruct>(line.Substring(6)).choices[0].delta.content;
                                reply += content;
                            }
                        }
                    }

                    onResponse?.Invoke(reply);
                }
                onResponseDone?.Invoke(reply);
                onComplete?.Invoke();
            }
        }
    }
}
