using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qaud.Test
{
    public class FooModel
    {
        private long _autoID = 0;
        private long _autoCommentID = 0;
        [Key]
        public long ID { get; set; }

        public DateTime CreateDate { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public IList<Comment> Comments { get; set; }

        public FooModel()
        {
            Comments = new List<Comment>();
            CreateDate = DateTime.Now;
        }

        public class Comment
        {
            public long ID { get; set; }
            public string Author { get; set; }
            public string Message { get; set; }
        }

        static Random rnd;

        public void AutoPopulate()
        {
            rnd = rnd ?? (rnd = new Random());
            int titleLength = rnd.Next(250);
            int contentLength = rnd.Next(10000);
            ID = _autoID++;
            CreateDate = new DateTime(DateTime.Now.Ticks - (long)rnd.Next(1000000000));
            Title = GenerateString(titleLength);
            Content = GenerateString(contentLength);
            Comments = new List<Comment>
            {
                new Comment
                {
                    ID = _autoCommentID++,
                    Author = GenerateString(titleLength),
                    Message = GenerateString(titleLength*rnd.Next(00))
                },
                new Comment
                {
                    ID = _autoCommentID++,
                    Author = GenerateString(titleLength),
                    Message = GenerateString(titleLength*rnd.Next(00))
                },
                new Comment
                {
                    ID = _autoCommentID++,
                    Author = GenerateString(titleLength),
                    Message = GenerateString(titleLength*rnd.Next(00))
                },
                new Comment
                {
                    ID = _autoCommentID++,
                    Author = GenerateString(titleLength),
                    Message = GenerateString(titleLength*rnd.Next(00))
                },
                new Comment
                {
                    ID = _autoCommentID++,
                    Author = GenerateString(titleLength),
                    Message = GenerateString(titleLength*rnd.Next(00))
                }
            };
        }

        private string GenerateString(int len)
        {
            var sb = new StringBuilder(len);
            const string abc =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789                    ";
            for (var n = 0; n < len; n++)
            {
                sb.Append(abc[rnd.Next(abc.Length)]);
            }
            return sb.ToString();
        }
    }
}
