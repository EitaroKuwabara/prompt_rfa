// PromptRFA/Models/DeskSpecs.cs
namespace PromptRFA.Models
{
    public class DeskSpecs
    {
        // 基本寸法
        public double Width { get; set; }
        public double Depth { get; set; }
        public double Height { get; set; }

        // 机特有の寸法
        public double TopThickness { get; set; } // 天板の厚み (mm)
        public double LegWidth { get; set; } // 脚の太さ (mm) ※今回は正方形の脚とします

        // 素材 (null許容)
        public string? TopMaterialName { get; set; }
        public string? LegMaterialName { get; set; } // 脚・幕板用

        // オプション機能
        public bool HasDrawers { get; set; } // 引き出しをつけるかどうか
    }
}
