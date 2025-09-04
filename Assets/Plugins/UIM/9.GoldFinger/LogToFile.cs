using System;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// log存储功能与其他游戏内数据存储并不使用同种方法
/// </summary>
public static class LogToFile 
{

    // 保存数据到文件
    public static void SaveLogToFile(string message)
    {
        // 获取项目的根目录
        string projectPath = UIM_SaveLoad.GetLocalPath("Log"); 

        // 定义文件存储路径，可以自定义文件夹和文件名
        string filePath = Path.Combine(projectPath, "LogData.txt");

        try
        {
            // 检查文件是否存在，如果不存在则创建一个新的文件
            if (!Directory.Exists(projectPath))
            {
                Directory.CreateDirectory(projectPath);



                // 如果文件不存在，创建一个新的文件
                File.WriteAllText(filePath, "Log Start\n" ); // 添加一个文件头
            }

            // 使用 StreamWriter 将文本写入文件，指定编码为 UTF-8
            using (StreamWriter writer = new StreamWriter(filePath, append: true, encoding: Encoding.UTF8))
            {
                writer.WriteLine(message); // 将纯文本写入文件
            }

            //// 追加内容到文件
            //File.AppendAllText(filePath, message + "\n");
            //Debug.Log("Log saved to: " + filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to save log: " + ex.Message);
        }
    }
}
