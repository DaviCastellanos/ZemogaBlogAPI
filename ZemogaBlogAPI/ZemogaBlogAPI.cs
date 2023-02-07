using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using ZemogaBlogAPI.Infrastructure.CosmosDbData.Repository;
using ZemogaBlogAPI.Infrastructure.Auth;
using ZemogaBlogAPI.Core.Interfaces;
using ZemogaBlogAPI.Core.Enums;
using Newtonsoft.Json;
using ZemogaBlogAPI.Core.Entities;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Linq;

namespace ZemogaBlogAPI
{
    public class ZemogaBlogAPI
    {
        private readonly ILogger<ZemogaBlogAPI> log;
        private readonly PostRepository postRepo;
        private readonly CommentRepository commentRepo;

        public ZemogaBlogAPI(ILogger<ZemogaBlogAPI> log, IPostRepository postRepo, ICommentRepository commentRepo)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.postRepo = (PostRepository)postRepo;
            this.commentRepo = (CommentRepository)commentRepo;

            if (postRepo == null || this.log == null)
            {
                log.LogError("Null dependencies");
                throw new ArgumentNullException();
            }
        }

        [FunctionName("CreateNewComment")]
        public async Task<IActionResult> CreateNewComment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/CreateNewComment")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Started create new comment");

            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                CommentItem item = JsonConvert.DeserializeObject<CommentItem>(requestBody);


                if (item != null)
                {
                    var createdItem = await commentRepo.CreateCommentAsync(item);
                    return new CreatedResult("comments container", createdItem);
                }
                else
                    return new BadRequestResult();
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                log.LogError(e.Message);
                return new BadRequestResult();
            }
        }

        [FunctionName("GetCommentsByPostId")]
        public async Task<IActionResult> GetCommentsByPostId(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/GetCommentsByPostId")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Started GetCommentsByPostId");

            string id = req.Query["postId"];
            bool includeReviews = false;

            Boolean.TryParse(req.Query["includeReviews"], out includeReviews);

            if (!String.IsNullOrEmpty(id))
            {
                var response = await commentRepo.GetCommentsByPostIdAsync(id, includeReviews);

                if (!response.Any())
                    return new NotFoundObjectResult(id);

                return new OkObjectResult(response);
            }
            else
                return new BadRequestResult();
            
        }


        [FunctionName("GetPostsByStatus")]
        public async Task<IActionResult> GetPostsByStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/GetPostsByStatus")] HttpRequest req,
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/GetPostById")] HttpRequest req,
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

        [Authorize("Task.Write")]
        [FunctionName("GetPostsByAuthor")]
        public async Task<IActionResult> GetPostsByAuthor(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/GetPostsByAuthor")] HttpRequest req,
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

        [Authorize("Task.Write")]
        [FunctionName("CreateNewPost")]
        public async Task<IActionResult> CreateNewPost(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/CreateNewPost")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Started create new post");

            try
            { 
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                PostItem item = JsonConvert.DeserializeObject<PostItem>(requestBody);

                if (item != null)
                {
                    return new CreatedResult("posts container", await postRepo.CreatePostAsync(item));
                }
                else
                    return new BadRequestResult();
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                log.LogError(e.Message);
                return new BadRequestResult();
            }
}

        [Authorize("Task.Edit")]
        [FunctionName("PublishPost")]
        public async Task<IActionResult> PublishPost(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/PublishPost")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Started Publish Post");

            string id = req.Query["postId"];

            if (!String.IsNullOrEmpty(id))
            {
                PostItem post = await postRepo.GetPostByIdAsync(id);

                if (post == null)
                    return new NotFoundResult();

                post.Status = Status.Published;
                post.DatePublished = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));


                PostItem newItem = await postRepo.UpdatePostAsync(post);

                return new OkObjectResult(newItem);
            }
            else
                return new BadRequestResult();
        }

        [Authorize("Task.Write")]
        [FunctionName("UpdatePost")]
        public async Task<IActionResult> UpdatePost(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/UpdatePost")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Started update post");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            PostItem item = JsonConvert.DeserializeObject<PostItem>(requestBody);

            if (item != null)
            {
                PostItem post = await postRepo.GetPostByIdAsync(item.PostId);

                if (post == null)
                    return new NotFoundResult();

                post.Status = item.Status;
                post.Title = item.Title;
                post.Header = item.Header;
                post.Body = item.Body;
                post.Version = item.Version++;
                    
                PostItem newItem = await postRepo.UpdatePostAsync(post);

                return new OkObjectResult(newItem);
            }
            else
                return new BadRequestResult();
        }
    }
}

