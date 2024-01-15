using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace UnityAIPlugin.Editor
{
    public static class Utils
    {
        public static string GetPathFromObject(Object script)
        {
            // ????????????????Asset
            TextAsset textAsset = script as TextAsset;
            if (textAsset == null)
            {
                //Debug.LogError("???????????????????Asset");
                return null;
            }

            // ????????·??
            return AssetDatabase.GetAssetPath(textAsset);
        }

        public static int CountLines(string filePath)
        {
            int count = 0;

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // ?ж???????Ч??????У????????к?????
                    if (!string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("//"))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public static int CountWords(string text)
        {
            // ??ÿ????????????????
            string[] words = text.Split(' ');

            // ??????????????????????
            return words.Length;
        }

        public static string RemoveMarkdown(string content)
        {
            string pattern = @"^```.*$\s*";
            return Regex.Replace(content, pattern, "", RegexOptions.Multiline);
        }

        public static string CommentAll(string content, string newLine)
        {
            string[] lines = content.Split(newLine);
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = "// " + lines[i];
            }
            return string.Join(newLine, lines);
        }

        public static string GetNewLine(string text)
        {
            if (text.Contains("\r\n"))
            {
                return "\r\n"; // Windows ???з?
            }
            else if (text.Contains("\n"))
            {
                return "\n"; // Unix ?? macOS ???з?
            }
            else if (text.Contains("\r"))
            {
                return "\r"; // ??? Mac ???з?
            }
            else
            {
                return null; // δ??????з?
            }
        }

        public static void GB2312ToUTF8(Object script)
        {
            string path = GetPathFromObject(script);
            if (path == null) return;

            // 读取原始文件内容
            string content = File.ReadAllText(path, System.Text.Encoding.GetEncoding("gb2312"));
            string newLine = GetNewLine(content);

            // ????????UTF-8????д???????
            File.WriteAllText(path, content + newLine + CommentAll(content, GetNewLine(content)), System.Text.Encoding.UTF8);
        }
    }
}
