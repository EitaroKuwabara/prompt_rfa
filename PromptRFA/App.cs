// App.cs
using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using DesignAutomationFramework;

namespace PromptRFA
{
    // ★変更点: IExternalApplication (UI用) ではなく IExternalDBApplication (DB用) にする
    public class App : IExternalDBApplication
    {
        public ExternalDBApplicationResult OnStartup(
            ControlledApplication application
        )
        {
            // Design Automationの準備完了イベントに登録
            DesignAutomationBridge.DesignAutomationReadyEvent +=
                HandleDesignAutomationReadyEvent!;
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnShutdown(
            ControlledApplication application
        )
        {
            // イベント登録解除
            DesignAutomationBridge.DesignAutomationReadyEvent -=
                HandleDesignAutomationReadyEvent!;
            return ExternalDBApplicationResult.Succeeded;
        }

        // ★ここがクラウドでの「メイン関数」になります
        public void HandleDesignAutomationReadyEvent(
            object sender,
            DesignAutomationReadyEventArgs e
        )
        {
            try
            {
                // ここで実際の処理を行う (FamilyProcessorを呼び出す)
                // e.DesignAutomationData.RevitApp で Application オブジェクトが取れます
                new FamilyProcessor().Run(
                    e.DesignAutomationData.RevitApp
                );

                // 成功を通知
                e.Succeeded = true;
            }
            catch (Exception ex)
            {
                // クラウド上のログに出力される
                Console.WriteLine(
                    "Exception in HandleDesignAutomationReadyEvent: "
                        + ex.Message
                );
                e.Succeeded = false;
            }
        }
    }
}
