using System.Text.Json.Serialization;

namespace ClosetUIServerless.Models;

public class ClosetPart
{
    public int ID { get; set; }
    public int PlateID { get; set; }
    public int X { get; set; }  // in mm   X on big wood plate
    public int Y { get; set; }  // in mm   Y on big wood plate
    public int Wt { get; set; } // in mm   part width  + blade thikness
    public int Ht { get; set; } // in mm   part height  + blade thikness
    public int Hypotenuse { get; set; }
    public string PartName { get; set; }
    public int PartWidth { get; set; }
    public int PartHeight { get; set; }
    public int PartQty { get; set; }
}
