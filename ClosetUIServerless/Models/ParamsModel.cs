namespace ClosetUIServerless.Models;

public class ParamsModel
{
        public int WoodType { get; set; }  // 1 = Interior or 2 = Outsider
        public int Direction { get; set; } // 1 = Verticat or 2 = Horizontal
        public int BladeThickness { get; set; }   //   in mm
        public int TotalWidth { get; set; }  // X in mm
        public int TotalHeight { get; set; } // Y in mm
        public int Hypotenuse { get; set; }
        public List<ClosetPart> Parts { get; set; }
        public List<PartMeasu> AllWidths { get; set; }
        public List<PartMeasu> AllHeights { get; set; }
        public List<PartMeasu> AllHypotenuse { get; set; }
        public List<List<PartMeasu>> FitWidths { get; set; }
        public List<List<PartMeasu>> FitHeights { get; set; }
        public List<List<PartMeasu>> FitHypotenuse { get; set; }
}
