using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using ZemogaBlogAPI.Infrastructure.CosmosDbData.Repository;
using ZemogaBlogAPI.Core.Interfaces;
using ZemogaBlogAPI.Core.Enums;
using Newtonsoft.Json;
using ZemogaBlogAPI.Core.Entities;
using System.Net.NetworkInformation;

namespace ZemogaBlogAPI
{
    public class ZemogaBlogAPI
    {
        private readonly ILogger<ZemogaBlogAPI> log;
        private readonly PostRepository postRepo;

        public ZemogaBlogAPI(ILogger<ZemogaBlogAPI> log, IPostRepository repo)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.postRepo = (PostRepository)repo;

            if (postRepo == null || this.log == null)
            {
                log.LogError("Null dependencies");
                throw new ArgumentNullException();
            }
        }

        [FunctionName("GetPostsByStatus")]
        public async Task<IActionResult> GetPostsByStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetPostsByStatus")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Started GetPostsByStatus");

            Status status = 0;

            if (Enum.TryParse(req.Query["status"], out status))
            {
                var response = await postRepo.GetPostsByStatusAsync(status);

                return new OkObjectResult(response);
            }
            else
                return new BadRequestResult();
        }

        [FunctionName("GetPostById")]
        public async Task<IActionResult> GetPostById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetPostById")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Started GetPostById");

            string id = req.Query["postId"];

            if (!String.IsNullOrEmpty(id))
            {
                var response = await postRepo.GetPostByIdAsync(id);

                return new OkObjectResult(response);
            }
            else
                return new BadRequestResult();
        }

        [FunctionName("GetPostsByAuthor")]
        public async Task<IActionResult> GetPostsByAuthor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetPostsByAuthor")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Started GetPostsByAuthor");

            string id = req.Query["authorId"];

            if (!String.IsNullOrEmpty(id))
            {
                var response = await postRepo.GetPostsByAuthor(id);

                return new OkObjectResult(response);
            }
            else
                return new BadRequestResult();
        }

        [FunctionName("CreateNewPost")]
        public async Task<IActionResult> CreateNewPost(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CreateNewPost")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Started create new post");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            PostItem item = JsonConvert.DeserializeObject<PostItem>(requestBody);

            if (item != null)
            {
                await postRepo.CreatePostAsync(item);

                return new CreatedResult("posts container", item);
            }
            else
                return new BadRequestResult();
        }

        [FunctionName("UpdatePostStatus")]
        public async Task<IActionResult> UpdatePostStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "UpdatePostStatus")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Started update post status");

            string id = req.Query["postId"];
            Status status = 0;

            if (!String.IsNullOrEmpty(id) && Enum.TryParse(req.Query["status"], out status))
            {
                PostItem post = await postRepo.GetPostByIdAsync(id);

                if (post == null)
                    return new NotFoundResult();

                post.Status = status;

                PostItem newItem = await postRepo.UpdatePostAsync(post);

                return new OkObjectResult(newItem);
            }
            else
                return new BadRequestResult();
        }
    }
}

