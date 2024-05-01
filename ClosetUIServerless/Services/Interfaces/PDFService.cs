using ClosetUIServerless.Models;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;

namespace ClosetUIServerless.Services.Interfaces;

public class PDFService : IPDFService
{
    public async Task<byte[]?> GenerateAndDownloadPdf(dynamic model)
    {
        ParamsModel? paramsModel = System.Text.Json.JsonSerializer.Deserialize<ParamsModel>(JsonConvert.SerializeObject(model));
        if (paramsModel == null)
        {
            return null;
        }

        PDFData pdfData = await PreparePDFData(paramsModel);
        return GeneratePdfDocument(pdfData);
    }

    private async Task<PDFData> PreparePDFData(ParamsModel paramsModel)
    {
        var pdfData = new PDFData
        {
            Title = "Board Layout",
            Boards = new List<BoardPDFInfo>()
        };

        int boardIndex = 1;
        double xPosition = 0, yPosition = 0;
        double currentRowMaxHeight = 0;
        int rowNumber = 1;

        BoardPDFInfo currentBoard = new BoardPDFInfo { BoardIndex = boardIndex, Parts = new List<PartPDFInfo>() };

        foreach (var group in paramsModel.FitHeights)
        {
            foreach (var partMeasure in group)
            {
                ClosetPart part = paramsModel.Parts.FirstOrDefault(p => p.ID == partMeasure.ID);
                if (part == null) continue;

                for (int qty = 0; qty < part.PartQty; qty++)
                {
                    if (xPosition + part.Wt > paramsModel.TotalWidth)
                    {
                        yPosition += currentRowMaxHeight;
                        xPosition = 0;
                        currentRowMaxHeight = 0;
                        rowNumber++;
                    }

                    if (yPosition + part.Ht > paramsModel.TotalHeight)
                    {
                        pdfData.Boards.Add(currentBoard);
                        boardIndex++;
                        currentBoard = new BoardPDFInfo { BoardIndex = boardIndex, Parts = new List<PartPDFInfo>() };
                        xPosition = 0;
                        yPosition = 0;
                        currentRowMaxHeight = 0;
                        rowNumber = 1;
                    }

                    currentBoard.Parts.Add(new PartPDFInfo
                    {
                        ID = part.ID,
                        Dimensions = $"{part.PartWidth}mm x {part.PartHeight}mm",
                        Position = $"X: {xPosition}mm, Y: {yPosition}mm",
                        RowNumber = rowNumber
                    });

                    // Reset position for next group
                    xPosition += part.Wt;
                    currentRowMaxHeight = Math.Max(currentRowMaxHeight, part.Ht);
                }
            }

            xPosition = 0;
            yPosition += currentRowMaxHeight;
            currentRowMaxHeight = 0;
        }

        if (currentBoard.Parts.Any())
        {
            pdfData.Boards.Add(currentBoard);
        }

        await Task.CompletedTask; // This line is technically redundant but makes the method async.
        return pdfData;
    }

    private byte[] GeneratePdfDocument(PDFData pdfData)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.Header().Text(pdfData.Title).FontSize(20).SemiBold();

                page.Content().Column(column =>
                {
                    foreach (var board in pdfData.Boards)
                    {
                        column.Item().Text($"Board {board.BoardIndex}").FontSize(16).Bold();
                        foreach (var part in board.Parts)
                        {
                            column.Item().Text($"Row {part.RowNumber}, Part {part.ID}: {part.Dimensions}, Position: {part.Position}");
                        }
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(18));
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }
}
