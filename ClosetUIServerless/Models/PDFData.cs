namespace ClosetUIServerless.Models;

public class PDFData
{
    public string Title { get; set; }
    public List<BoardPDFInfo> Boards { get; set; }
}

public class BoardPDFInfo
{
    public int BoardIndex { get; set; }
    public List<PartPDFInfo> Parts { get; set; }
}

public class PartPDFInfo
{
    public int ID { get; set; }
    public string Dimensions { get; set; } // $"{part.Wt}mm x {part.Ht}mm"
    public string Position { get; set; } // $"X: {xPosition}mm, Y: {yPosition}mm"
    public int RowNumber { get; set; }
}

