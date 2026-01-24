// PromptRFA/Utils/DebugLogger.cs
using System.Diagnostics;
# if DEBUG
using Autodesk.Revit.UI;
# endif

namespace PromptRFA.Utils
{
    public static class DebugLogger
    {
        // ★ここがマスタースイッチです
        // trueなら表示、falseなら無視します
        public static bool IsEnabled { get; set; } = false;

        /// <summary>
        /// スイッチがONの時だけダイアログを出す
        /// </summary>
        public static void Show(
            string title,
            string message
        )
        {
# if DEBUG
            if (IsEnabled)
            {
                TaskDialog.Show(title, message);
            }
            // 念のため出力ウィンドウにも出す
            Debug.WriteLine($"[{title}] {message}");
# else
            // 本番環境の場合は何もしない
            Console.WriteLine(
                $"[PromptRFA Log] {title}: {message}"
            );
# endif
        }
    }
}
