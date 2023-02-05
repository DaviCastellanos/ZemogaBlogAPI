using System;
namespace ZemogaBlogAPI.Core.Entities
{
	public class CommentItem : BaseEntity
	{
		public string PostId { get; set; }
		public string PostAuthorId { get; set; }
		public bool IsReview { get; set; }
		public string Body { get; set; }
	}
}

