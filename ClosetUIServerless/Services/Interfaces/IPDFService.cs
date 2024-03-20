using ClosetUIServerless.Models;

namespace ClosetUIServerless.Services;

public interface IPDFService
{
    Task<byte[]?> GenerateAndDownloadPdf(dynamic model);
}
