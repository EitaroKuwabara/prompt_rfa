// PromptRFA/App.cs
using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;

namespace PromptRFA
{
    public class App : IExternalDBApplication
    {
        public ExternalDBApplicationResult OnStartup(ControlledApplication app)
        {
            // ロガー初期化
            Logger.Initialize();
            Logger.Write("OnStartup: アドインがロードされました。");
            
            // イベント登録
            app.ApplicationInitialized += OnApplicationInitialized;
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnShutdown(ControlledApplication app)
        {
            Logger.Write("OnShutdown: アドインが終了しました。");
            app.ApplicationInitialized -= OnApplicationInitialized;
            return ExternalDBApplicationResult.Succeeded;
        }

        private void OnApplicationInitialized(object sender, ApplicationInitializedEventArgs e)
        {
            Application app = sender as Application;
            if (app == null) return;

            Logger.Write("Event: Revit初期化完了。DeskProcessorを実行します。");

            try
            {
                // 新しいクラスに処理を丸投げ
                var processor = new DeskProcessor();
                processor.Run(app);
            }
            catch (Exception ex)
            {
                Logger.Write($"Critical Error: {ex.Message}");
            }
        }
    }
}