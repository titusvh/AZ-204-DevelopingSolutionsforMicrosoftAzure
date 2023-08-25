using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Flurl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("/")]
    public class ImagesController : ControllerBase
    {
        private HttpClient _httpClient;
        private Options _options;
        private readonly ILogger _logger;

        public ImagesController(HttpClient httpClient, Options options, ILoggerFactory logfac)
        {
            _httpClient = httpClient;
            _options = options;
            _logger = logfac.CreateLogger<ImagesController>();
            _logger.LogInformation("Hallo hallo, we zijn er weer!");
        }

        private async Task<BlobContainerClient> GetCloudBlobContainer(string containerName)
        {
            
            BlobServiceClient serviceClient = new BlobServiceClient(_options.StorageConnectionString);
            BlobContainerClient containerClient = serviceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();
            return containerClient;
        }

        [Route("/")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            BlobContainerClient containerClient = await GetCloudBlobContainer(_options.FullImageContainerName);
            List<string> results = new List<string>();
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                results.Add(
                    Flurl.Url.Combine(
                        containerClient.Uri.AbsoluteUri,
                        blobItem.Name
                    )
                );
            }
            Console.Out.WriteLine("Got Images");
            _logger.LogInformation("Hallo hallo, we goit the list!");
            return Ok(results);
        }

        private async Task<bool> FileNameIsAlreadyInUse(string fileName)
        {
            var existing = await GetExistingImages().ConfigureAwait(false);
            return existing.Contains(fileName);
        }

        private async Task<List<string>> GetExistingImages()
        {
            BlobContainerClient containerClient = await GetCloudBlobContainer(_options.FullImageContainerName);
            List<string> results = new List<string>();
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                results.Add(blobItem.Name);
            }
            Console.Out.WriteLine("Got Images");
            return results;
        }

        [Route("/")]
        [HttpPost]
        public async Task<ActionResult> Post()
        {
            if (Request.Body == null)
            {
                _logger.LogError("Body is null");
                return BadRequest("Body with upload file is NULL");
            }
            if (Request.ContentLength == 0)
            {
                _logger.LogError("Body is empty");
                return BadRequest("Body with upload file is EMPTY");
            }
            Stream image = Request.Body;
            var preferredFileName = Request.Headers["PreferredFileName"];

            BlobContainerClient containerClient = await GetCloudBlobContainer(_options.FullImageContainerName);
            string blobName = 
                await FileNameIsAlreadyInUse(preferredFileName).ConfigureAwait(false)
                ? Guid.NewGuid().ToString().ToLower().Replace("-", String.Empty)
                : preferredFileName;

            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(image);
            return Created(blobClient.Uri, null);
        }
    }
}