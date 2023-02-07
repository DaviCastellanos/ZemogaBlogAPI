using System;
using ZemogaBlogAPI.Core.Enums;

namespace ZemogaBlogAPI.Core.Entities
{
	public class PostItem: BaseEntity
    {
        public string AuthorName { get; set; }
        public string AuthorId { get; set; }
        public DateTime DatePublished { get; set; }
        public Status Status { get; set; }
        public string Title { get; set; }
        public string Header { get; set; }
        public string Body { get; set; }
        public int Version { get; set; }
        public string PostId { get { return base.id; } set { } }
    }
}

