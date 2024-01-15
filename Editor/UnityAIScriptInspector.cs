using System.IO;
using UnityEngine;
using UnityEditor;
using UnityAIPlugin.API;

namespace UnityAIPlugin.Editor
{
    public class UnityAIScriptInspector : EditorWindow
    {
        private Object selectedObject;
        private string filePath;

        //Script Info
        private string scriptContent;
        private int linesCount;
        private int wordsCount;

        private string language;
        private string customPrompt;

        private string[] providers = { "openai" };
        private string[] models = {"gpt-3.5-turbo", "gpt-3.5-turbo-16k" };
        private int modelIndex;
        private int providerIndex;

        private Vector2 ScriptContentScrollPosition;

        private UnityAIEditorClient unityAIEditor = UnityAIEditorClient.Instance;
        private bool isProcessing;

        private bool IsProcessing
        {
            set 
            { 
                isProcessing = value;
                OnStatusChange();
            }
        }

        [MenuItem("Window/AI Script Inspector")]
        public static void ShowWindow()
        {
            GetWindow<UnityAIScriptInspector>("AI Script Inspector");
        }

        private void OnGUI()
        {
            if (selectedObject != null && Path.GetExtension(filePath) == ".cs")
            {
                ScriptInfoGUI();
                OptionButtonGUI();
                ScriptContentGUI();
            }
            else
            {
                NoScriptGUI();
            }
        }
        private void OnSelectionChange()
        {
            UpdateData();
            Repaint();
        }

        private void OnFocus()
        {
            UpdateData();
            Repaint();
        }

        private void OnStatusChange()
        {
            Repaint();
        }

        private void ScriptInfoGUI()
        {
            EditorGUILayout.Space();

            GUIStyle infoStyle = new GUIStyle(EditorStyles.label) { fontSize = 12 };

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Name:  {selectedObject.name}", infoStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("GB2312 to UTF-8", GUILayout.Width(120)))
            {
                Utils.GB2312ToUTF8(selectedObject);
                Repaint();
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"Path:  {filePath}", infoStyle);
            EditorGUILayout.LabelField($"Lines Count:  {linesCount}", infoStyle);
            EditorGUILayout.LabelField($"Words Count:  {wordsCount}", infoStyle);
        }

        private void OptionButtonGUI()
        {
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Provider", GUILayout.Width(50));
            providerIndex = EditorGUILayout.Popup(providerIndex, providers, GUILayout.Width(125));
            EditorGUILayout.LabelField("Model", GUILayout.Width(40));
            modelIndex = EditorGUILayout.Popup(modelIndex, models, GUILayout.Width(125));
            GUILayout.EndHorizontal();


            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Language", GUILayout.Width(60));
            language = EditorGUILayout.TextArea(language, EditorStyles.textArea, GUILayout.Width(100));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Comment", GUILayout.Width(80)))
            {
                ProcessScript(selectedObject, "为以下代码添加注释");
            }

            if (GUILayout.Button("Optimization", GUILayout.Width(100)))
            {
                ProcessScript(selectedObject, "对以下代码进行优化");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Complete the code", GUILayout.Width(140)))
            {
                ProcessScript(selectedObject, "补全以下代码");
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Custom Prompt (The prompt will be placed before the code.)");
            customPrompt = EditorGUILayout.TextArea(customPrompt, EditorStyles.textArea, GUILayout.ExpandHeight(false));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Process", GUILayout.Width(100)))
            {
                ProcessScript(selectedObject, customPrompt);
            }
            GUILayout.EndHorizontal();
        }

        private void ScriptContentGUI()
        {
            EditorGUILayout.Space();

            ScriptContentScrollPosition = EditorGUILayout.BeginScrollView(ScriptContentScrollPosition, GUILayout.ExpandHeight(true));
            GUIStyle style = new GUIStyle(GUI.skin.textArea);
            style.wordWrap = true;
            GUILayout.TextArea(scriptContent, EditorStyles.textField, GUILayout.ExpandHeight(false));
            //EditorGUILayout.TextArea(scriptContent, EditorStyles.textField, GUILayout.ExpandHeight(false));
            EditorGUILayout.EndScrollView();
        }

        private void NoScriptGUI()
        {
            GUILayout.BeginArea(new Rect(0, 0, position.width, position.height));
            GUILayout.FlexibleSpace(); // 在垂直方向上创建可伸缩空间

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // 在水平方向上创建可伸缩空间

            // 创建居中对齐的元素
            GUILayout.Label("No script selected.");

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
        }


        void UpdateData()
        {
            selectedObject = Selection.activeObject;
            filePath = Utils.GetPathFromObject(selectedObject);
            if (filePath != null)
            {
                linesCount = Utils.CountLines(filePath);
                scriptContent = File.ReadAllText(filePath);
                wordsCount = Utils.CountWords(scriptContent);
            }
        }

        private async void ProcessScript(Object script, string option)
        {
            if (isProcessing)
            {
                Debug.Log("处理进行中");
                return;
            }

            isProcessing = true;

            string scriptPath = Utils.GetPathFromObject(script);

            if (scriptPath == null) return;

            // 读取脚本文件的内容
            string scriptContent = File.ReadAllText(scriptPath);

            Debug.Log($"{scriptPath}  正在{option}，请稍等");
            Response response = await unityAIEditor.Process(scriptContent, models[modelIndex], option);

            if (response.status)
            {
                File.WriteAllText(scriptPath, response.message);
                Debug.Log(scriptPath + " 脚本操作完成");
            }
            else
            {
                IsProcessing = false;
                Debug.LogError(response.message);
            }

            IsProcessing = false;
            Repaint();
        }
    }
}