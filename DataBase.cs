using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ContentGraber
{
    public class posts
    {
        public int postsId { get; set; }
        public int postID { get; set; }
        public string website { get; set; }
    }

    public class DataBase
    {
        public static void addPosts(List<posts> posts)
        {
            using (var db = new MobileContext())
            {
                foreach (posts answer in posts)
                    db.answers.Add(answer);

                db.SaveChanges();
            }
        }

        public static void addPost(posts post)
        {
            using (var db = new MobileContext())
            {
                db.answers.Add(post);

                db.SaveChanges();
            }
        }

        public static posts[] GetPostsByIdAndWebsite(int id, string website)
        {
            using (var db = new MobileContext())
            {
                return db.answers.Where(item => item.postID == id && item.website == website).ToArray();
            }
        }
    }

    public class MobileContext : DbContext
    {
        public DbSet<posts> answers { get; set; }

        public MobileContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=DBrule34.xxx.db");
        }
    }
}