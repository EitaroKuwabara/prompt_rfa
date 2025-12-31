// PromptRFA/Models/ShelfSpecs.cs
namespace PromptRFA.Models
{
    public class ShelfSpecs
    {
        public double Width { get; set; }
        public double Depth { get; set; }
        public double Height { get; set; }

        // 天板の厚み(mm)
        public double TopThickness { get; set; }

        // 側板の厚み (mm)
        public double SideThickness { get; set; }

        // 棚板の厚み (mm)
        public double ShelfThickness { get; set; }

        // 天板の素材
        public string? TopMaterialName { get; set; }

        // 側板の素材
        public string? SideMaterialName { get; set; }

        // 棚板の素材
        public string? ShelfMaterialName { get; set; }

        // 棚板の枚数
        public int? ShelfCount { get; set; }
    }
}
