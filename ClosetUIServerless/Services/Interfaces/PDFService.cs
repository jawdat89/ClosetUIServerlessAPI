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
            return default;
        }

        var pdfData = await PreparePDFData(paramsModel);

        var document = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(TextStyle.Default.FontSize(12));
                page.Header()
                    .PaddingBottom(20)
                    .Background(Colors.Grey.Lighten1)
                    .AlignCenter()
                    .AlignMiddle()
                    .Text($"{pdfData.Title}")
                    .FontSize(20)
                    .SemiBold();

                page.Content().Padding(10).Column(column =>
                {
                    int partIndex = 0;

                    foreach (var board in pdfData.Boards)
                    {
                        if (board.Parts.Count > 0)
                        {
                            column.Item().Element(element =>
                            {
                                element.Row(row =>
                                {
                                    row.RelativeItem().Text($"Board {board.BoardIndex}").Bold().FontSize(16);
                                });
                            });

                            foreach (var part in board.Parts)
                            {
                                // Include the row number in the part's text
                                column.Item().Padding(2).Text($"Row {part.RowNumber}, Part {part.ID}: {part.Dimensions}, Position: {part.Position}");
                            }

                            partIndex++;

                            if (partIndex == board.Parts.Count + 1)
                            {
                                column.Item().PageBreak();
                                partIndex = 0;
                            }
                            else
                            {
                                column.Item().Padding(10);
                            }
                        }
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
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

    private static async Task<PDFData> PreparePDFData(ParamsModel paramsModel)
    {
        var pdfData = new PDFData
        {
            Title = "Board Layout",
            Boards = []
        };

        var boardWidth = paramsModel.TotalWidth;
        var boardHeight = paramsModel.TotalHeight;
        int boardIndex = 1;
        double xPosition = 0, yPosition = 0;
        double currentRowMaxHeight = 0;

        var fitWidthsCopy = new List<List<PartMeasu>>(paramsModel.FitWidths);
        int rowNumber = 1; // Initialize row number for the first row.

        while (fitWidthsCopy.Count > 0)
        {
            var row = fitWidthsCopy.First();
            BoardPDFInfo boardPDFInfo = new()
            {
                BoardIndex = boardIndex,
                Parts = []
            };

            foreach (var partMeasure in row)
            {
                var part = paramsModel.Parts.FirstOrDefault(p => p.ID == partMeasure.ID);
                if (part == null) continue;

                // Check if a new row or board is needed due to size constraints.
                if (xPosition + part.Wt > boardWidth)
                {
                    // Start a new row within the current board.
                    yPosition += currentRowMaxHeight;
                    xPosition = 0;
                    currentRowMaxHeight = 0;
                    rowNumber++; // Increment the row number within the current board.
                }

                if (yPosition + part.Ht > boardHeight)
                {
                    // Save the current board and start a new one.
                    pdfData.Boards.Add(boardPDFInfo);
                    boardIndex++;
                    boardPDFInfo = new BoardPDFInfo
                    {
                        BoardIndex = boardIndex,
                        Parts = []
                    };
                    xPosition = 0;
                    yPosition = 0;
                    currentRowMaxHeight = 0;
                    rowNumber = 1; // Reset row number for the new board.
                }

                var partPDFInfo = new PartPDFInfo
                {
                    ID = part.ID,
                    Dimensions = $"{part.Wt}mm x {part.Ht}mm",
                    Position = $"X: {xPosition}mm, Y: {yPosition}mm",
                    RowNumber = rowNumber
                };

                boardPDFInfo.Parts.Add(partPDFInfo);
                xPosition += part.Wt;
                currentRowMaxHeight = Math.Max(currentRowMaxHeight, part.Ht);
            }

            // After processing a row, prepare for the next one.
            fitWidthsCopy.RemoveAt(0);
            if (xPosition > 0) // Only adjust if the last row was partially filled.
            {
                xPosition = 0;
                yPosition += currentRowMaxHeight;
                currentRowMaxHeight = 0;
                rowNumber++;
            }

            if (boardPDFInfo.Parts.Count > 0 && !pdfData.Boards.Contains(boardPDFInfo))
            {
                pdfData.Boards.Add(boardPDFInfo);
            }
        }

        await Task.CompletedTask;
        return pdfData;
    }
}
