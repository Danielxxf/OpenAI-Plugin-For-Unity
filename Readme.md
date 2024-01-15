# OpenAI Plugin for Unity
本插件包装了 OpenAI API，可为 Unity 提供调用 OpenAI api 能力，并且提供了 AI Script Inspector，可以基于 ChatGPT 一键注释、优化、补全脚本文件。
## Usage
### 设置 API Key 和 API URL
在系统盘（例如 C 盘）C:\Users\username\\.openai 文件夹下创建并编辑 auth.json 文件；  
填入你自己的OpenAI的API Key和API URL。
```json
{
    "apiKey": "",
    "apiURL": ""
}
```
### 调用API
```csharp
using UnityAIPlugin.API;
...
OpenAIClient client = new OpenAIClient();
client.SendMessageAsync("你好！", (result) =>
{
    Debug.Log(result);
});
```
### AI Script Inspector
#### 显示 AI Script Inspector
顶部选项 Window - AI Script Inspector  
![image](https://github.com/Danielxxf/OpenAI-Plugin-For-Unity/assets/48150158/d1e58b3e-2aae-484c-a0c5-4ca197fc8693)
#### AI Script Inspector 面板
Project窗口中选择任一 .cs 后缀的 C# 脚本；  
AI Script Inspector 如下所示，三个按钮“Comment”、“Optimization”、“Complete the code”分别为注释、优化、补全代码。  
![image](https://github.com/Danielxxf/OpenAI-Plugin-For-Unity/assets/48150158/950f1df0-4b44-4a33-88e9-c6da2d00c293)
