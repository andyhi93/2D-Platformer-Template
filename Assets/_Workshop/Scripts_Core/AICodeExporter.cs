using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text; // 處理編碼必備

public class AICodeExporter : EditorWindow
{
    [MenuItem("Tools/一鍵導出程式碼給 AI")]
    public static void ExportScriptsForAI()
    {
        string scriptFolder = Application.dataPath;

        if (!Directory.Exists(scriptFolder))
        {
            Debug.LogError($"找不到資料夾：{scriptFolder}。請確保你的核心腳本都放在 Assets/Scripts 底下！");
            return;
        }

        string[] files = Directory.GetFiles(scriptFolder, "*.cs", SearchOption.AllDirectories);
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("以下是目前專案的底層框架程式碼，採用事件驅動與組件化設計。請以此為基礎進行閱讀或擴充，請勿隨意修改底層邏輯：\n");

        int validFileCount = 0;

        foreach (string file in files)
        {
            if (file.Replace("\\", "/").Contains("/Editor/")) continue;

            string fileName = Path.GetFileName(file);

            // 👇 關鍵修正：強制以 UTF-8 格式讀取檔案，防止中文註解變亂碼
            string fileContent = File.ReadAllText(file, Encoding.UTF8);

            sb.AppendLine($"// ==========================================");
            sb.AppendLine($"// 檔案名稱：{fileName}");
            sb.AppendLine($"// ==========================================");
            sb.AppendLine(fileContent);
            sb.AppendLine("\n");

            validFileCount++;
        }

        string finalResult = sb.ToString();

        GUIUtility.systemCopyBuffer = finalResult;

        string exportPath = Path.Combine(Application.dataPath, "../ProjectCode_For_AI.txt");

        // 👇 關鍵修正：強制以 UTF-8 格式寫入 txt 檔
        File.WriteAllText(exportPath, finalResult, Encoding.UTF8);

        EditorUtility.DisplayDialog(
            "導出成功！",
            $"已成功打包 {validFileCount} 個腳本。\n\n程式碼已經【自動複製到剪貼簿】了！\n(中文亂碼問題已修復 ✨)\n\n(備份檔案儲存於: {exportPath})",
            "太神啦"
        );
    }
}