using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace UnityAIPlugin.Editor
{
    public static class Utils
    {
        // 获取对象所在的路径
        public static string GetPathFromObject(Object script)
        {
            TextAsset textAsset = script as TextAsset;
            if (textAsset == null)
            {
                return null;
            }

            return AssetDatabase.GetAssetPath(textAsset);
        }

        // 统计文件中的代码行数
        public static int CountLines(string filePath)
        {
            int count = 0;

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("//"))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        // 统计字符串中的单词数
        public static int CountWords(string text)
        {
            string[] words = text.Split(' ');

            return words.Length;
        }

        // 删除Markdown格式
        public static string RemoveMarkdown(string content)
        {
            string pattern = @"^```.*$\s*";
            return Regex.Replace(content, pattern, "", RegexOptions.Multiline);
        }

        // 在每行代码前添加注释符号
        public static string CommentAll(string content, string newLine)
        {
            string[] lines = content.Split(newLine);
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = "// " + lines[i];
            }
            return string.Join(newLine, lines);
        }

        // 获取文件的换行符
        public static string GetNewLine(string text)
        {
            if (text.Contains("\r\n"))
            {
                return "\r\n"; // Windows 换行
            }
            else if (text.Contains("\n"))
            {
                return "\n"; // Unix 或 macOS 换行
            }
            else if (text.Contains("\r"))
            {
                return "\r"; // 旧版 Mac 换行
            }
            else
            {
                return null; // 未知换行
            }
        }

        // 将文件转换为UTF-8并添加注释后写入文件
        public static void GB2312ToUTF8(Object script)
        {
            string path = GetPathFromObject(script);
            if (path == null) return;

            string content = File.ReadAllText(path, System.Text.Encoding.GetEncoding("gb2312"));
            string newLine = GetNewLine(content);

            File.WriteAllText(path, content + newLine + CommentAll(content, GetNewLine(content)), System.Text.Encoding.UTF8);
        }
    }
}

// using System.IO;
// using System.Text.RegularExpressions;
// using UnityEditor;
// using UnityEngine;
// 
// namespace UnityAIPlugin.Editor
// {
//     public static class Utils
//     {
//         public static string GetPathFromObject(Object script)
//         {
//             // 将对象转换为TextAsset类型
//             TextAsset textAsset = script as TextAsset;
//             if (textAsset == null)
//             {
//                 //Debug.LogError("所选对象不是TextAsset");
//                 return null;
//             }
// 
//             // 获取对象所在的路径
//             return AssetDatabase.GetAssetPath(textAsset);
//         }
// 
//         public static int CountLines(string filePath)
//         {
//             int count = 0;
// 
//             using (StreamReader reader = new StreamReader(filePath))
//             {
//                 string line;
//                 while ((line = reader.ReadLine()) != null)
//                 {
//                     // 判断是否是有效代码行，如果是则增加计数
//                     if (!string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("//"))
//                     {
//                         count++;
//                     }
//                 }
//             }
// 
//             return count;
//         }
// 
//         public static int CountWords(string text)
//         {
//             // 将字符串按空格拆分为单词数组
//             string[] words = text.Split(' ');
// 
//             // 返回单词数量
//             return words.Length;
//         }
// 
//         public static string RemoveMarkdown(string content)
//         {
//             string pattern = @"^```.*$\s*";
//             // 使用正则表达式将Markdown格式移除
//             return Regex.Replace(content, pattern, "", RegexOptions.Multiline);
//         }
// 
//         public static string CommentAll(string content, string newLine)
//         {
//             string[] lines = content.Split(newLine);
//             for (int i = 0; i < lines.Length; i++)
//             {
//                 // 在每行代码前添加注释符号
//                 lines[i] = "// " + lines[i];
//             }
//             return string.Join(newLine, lines);
//         }
// 
//         public static string GetNewLine(string text)
//         {
//             if (text.Contains("\r\n"))
//             {
//                 return "\r\n"; // Windows 换行
//             }
//             else if (text.Contains("\n"))
//             {
//                 return "\n"; // Unix 或 macOS 换行
//             }
//             else if (text.Contains("\r"))
//             {
//                 return "\r"; // 旧版 Mac 换行
//             }
//             else
//             {
//                 return null; // 未知换行
//             }
//         }
// 
//         public static void GB2312ToUTF8(Object script)
//         {
//             string path = GetPathFromObject(script);
//             if (path == null) return;
// 
//             // 读取原始文件内容
//             string content = File.ReadAllText(path, System.Text.Encoding.GetEncoding("gb2312"));
//             string newLine = GetNewLine(content);
// 
//             // 将内容转换为UTF-8并添加注释后写入文件
//             File.WriteAllText(path, content + newLine + CommentAll(content, GetNewLine(content)), System.Text.Encoding.UTF8);
//         }
//     }
// }