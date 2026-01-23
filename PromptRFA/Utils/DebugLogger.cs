// PromptRFA/Utils/DebugLogger.cs
using Autodesk.Revit.UI;

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
            if (IsEnabled)
            {
                TaskDialog.Show(title, message);
            }
        }
    }
}
