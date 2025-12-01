using Microsoft.AspNetCore.Mvc;
using SampleWebApi.Model;

namespace SampleWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly BlobService _blobService;

        public FilesController(BlobService blobService)
        {
            _blobService = blobService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File not selected.");

            await _blobService.UploadFileAsync(file.OpenReadStream(), file.FileName);
            return Ok("File uploaded successfully.");
        }

        public async Task<IActionResult> FileDetails(IFormFile file)
        {
            return Ok();
        }
        public async Task<IActionResult> GetAllFiles(IFormFile file)
        {

            return null;
        }
        


        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            var stream = await _blobService.DownloadFileAsync(fileName);
            return File(stream, "application/octet-stream", fileName);
        }
    }

}
