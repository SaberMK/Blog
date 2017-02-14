using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Blog.Models;
namespace Blog.Controllers
{
    public class BlogController : Controller
    {
        // GET: Blog
        public ActionResult Index()
        {
            return View();
        }


        #region Create(создание блога)
        [HttpGet]
        public ActionResult Create()
        {
            if (Session["id"] == null)
                return RedirectToAction("Index", "User");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)] //Сам проверю
        public ActionResult Create(BlogViewModel bvm)
        {
            var db = new BlogModelDataContext();
            if (ModelState.IsValid)
            {

                int id = int.Parse(Session["id"].ToString());
                Articles article = new Articles();
                article.Title = bvm.Title;
                article.Content = GetHtmlString(bvm.Content);
                //article.Content = bvm.Content;
                article.AuthorId = id;
                article.DateTime = DateTime.Now;
                db.Articles.InsertOnSubmit(article);
                db.SubmitChanges();
                if (bvm.Tags != null)
                {
                    string[] separatedTags = bvm.Tags.Split(new char[] { ' ', '#', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    List<int> tagIds = new List<int>();
                    foreach (var separatedtag in separatedTags)
                    {
                        if (db.Tags.Count(x => x.Tag == separatedtag) == 0)
                        {
                            Tags t = new Tags() { Tag = separatedtag };
                            db.Tags.InsertOnSubmit(t);
                            db.SubmitChanges(); //TODO потом повыносить в отдельный лист такие добавленные тэги
                            tagIds.Add(t.TagId);
                        }
                        else
                        {
                            tagIds.Add(db.Tags.SingleOrDefault(x => x.Tag == separatedtag).TagId);
                        }
                    }
                    db.SubmitChanges();
                    int articleId = db.Articles.Single(x => x == article).ArticleId;

                    foreach (var tagId in tagIds)
                    {
                        db.TagsAndArticles.InsertOnSubmit(new TagsAndArticles() { ArticleId = articleId, TagId = tagId });
                    }
                    db.SubmitChanges();
                }
                return RedirectToAction("SuccessBlogCreate", "Blog");
            }
            return View();
        }
        public static string GetHtmlString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }
            var result = value.Replace("\n", "<br/>");
            return result;
        }
        #endregion
         
        [HttpGet]
        public ActionResult AddComment()
        {
            if (Session["id"] == null || Session["lastArticleId"]==null)
                return RedirectToAction("Index", "User");
            return View();
        }

        [HttpPost]
        public ActionResult AddComment(CommentaryViewModel cvm)
        {
            var db = new BlogModelDataContext();
            if (ModelState.IsValid)
            {
                Commentaries commentary = new Commentaries();
                commentary.ArticleId = int.Parse(Session["lastArticleId"].ToString());
                commentary.AuthorId = int.Parse(Session["id"].ToString());
                commentary.Content = cvm.Context;
                commentary.DateTime = DateTime.Now;
                db.Commentaries.InsertOnSubmit(commentary);
                db.SubmitChanges();
                return RedirectToAction("ViewArticle", "Blog",new { id = commentary.ArticleId });
            }
            return View();
        }




        [HttpGet]
        public ActionResult Feed(int? id)
        {
            int uId = id.HasValue ? id.Value <= 0 ? 1 : id.Value : 1;
            var db = new BlogModelDataContext();
            var blogs = db.Articles.OrderByDescending(x => x.DateTime).Skip((uId - 1 ) * 10).Take(10).ToList();
            for(int i=0;i<blogs.Count();++i)
            {
                blogs[i].Content = blogs[i].Content.Replace("\r<br/>", "<br>");
            }
            ViewBag.id = uId;
            return View(blogs);
        }
        
        [HttpGet]
        public ActionResult Best(int? id)
        {
            int uId = id.HasValue ? id.Value <= 0 ? 1 : id.Value : 1;
            var db = new BlogModelDataContext();

            var articles = db.Articles;
            List<KeyValuePair<Articles, int>> rating = new List<KeyValuePair<Articles, int>>();

            foreach (var a in articles)
            {
                rating.Add(new KeyValuePair<Articles, int>(a, a.Ratings.Sum(y => y.Amount)));
            }
            rating = rating.OrderByDescending(x=>x.Value).ToList().Skip((uId - 1) * 10).Take(10).ToList();
            ViewBag.id = uId;
            return View(rating);
        }

        public ActionResult ViewArticle(int? id,int? param)
        {
            if (id.HasValue == false)
                RedirectToAction("WrongId", "Blog");
            else
            {
                if (param.HasValue == false)
                    param = 1;
                var db = new BlogModelDataContext();
                var article = db.Articles.Single(x => x.ArticleId == id.Value);
                ViewBag.AuthorName = db.Users.Single(x => x.UserId == article.AuthorId).Login;

                //Подгружаем теги

                List<Tags> tags = new List<Tags>();
                foreach(var t in article.TagsAndArticles)
                {
                    tags.Add(db.Tags.Single(x => x.TagId == t.TagId));
                }
                ViewBag.Tags = tags;

                //Теперь для лайков/дизлайков

                //Количество лайков и дизлайков на статье
                int count;
                var z = db.Ratings.Where(x => x.ArticleId == id.Value);
                if (db.Ratings.Where(x => x.ArticleId == id.Value).Count() != 0)
                    count = db.Ratings.Where(x => x.ArticleId == id.Value).Sum(x => x.Amount);
                else
                    count = 0;
                ViewBag.Rating = count;

                //Отмечал ли пользователь эту статью?
                if (Session["id"] != null)
                {
                    int userId = int.Parse(Session["id"].ToString());
                    bool isMarked = db.Ratings.Count(x => x.ArticleId == id.Value && x.AuthorId == userId) == 1;
                    ViewBag.IsMarked = isMarked;

                    //Оценка пользователя(если есть)
                    if (isMarked)
                    {
                        //Понравилась или нет?
                        bool isLike = db.Ratings.Single(x => x.ArticleId == id.Value && x.AuthorId == userId).Amount == 1;
                        ViewBag.IsLike = isLike;
                    }
                    //Если нормально всё отобразилось, то запоминаем статью
                    Session["lastArticleId"] = id;
                    ViewBag.Id = id;
                    ViewBag.Param = param;
                    //А теперь все комменты
                    List<Commentaries> comments= db.Commentaries.Where(x => x.ArticleId == id).OrderByDescending(x=>x.DateTime).Skip((param.Value - 1) * 10).Take(10).ToList();
                    ViewBag.Comments = comments;
                    Dictionary<int, string> authors = new Dictionary<int, string>();
                    foreach(var comm in comments)
                    {
                        if(!authors.Keys.Contains(comm.AuthorId))
                        {
                            authors.Add(comm.AuthorId, db.Users.Single(x => x.UserId == comm.AuthorId).Login);
                        }
                    }
                    ViewBag.Authors = authors;
                    return View(article);
                }
                else
                {
                    RedirectToAction("Login", "Account", null);
                }
                //Всё остальное через вьюшку сделано

            }
            return View();
        }


        [HttpGet]
        public ActionResult Tag(int? id, int? param)
        {
            if (param.HasValue == false)
            {
                param = 1;
            }
            if (id.HasValue == false)
            {
                return View();
            }

            ViewBag.TagId = id;
            //Когда разобрались с возможными простейшими ошибками в пути, переходим к поиску тега:
            var db = new BlogModelDataContext();
            Tags currentTag = db.Tags.SingleOrDefault(x => x.TagId == id);
            ViewBag.Tag = currentTag.Tag;

            //Теперь список статей по тегу:
            List<int> articleIds = new List<int>();
            IEnumerable<TagsAndArticles> taa = db.TagsAndArticles.Where(x => x.TagId == currentTag.TagId);
            List<Articles> articles = new List<Articles>();
            foreach(var t in taa)
            {
                articles.Add(db.Articles.SingleOrDefault(x => x.ArticleId == t.ArticleId));
            }

            return View(articles);
        }

        [HttpGet]
        public ActionResult Rate(int? id, int? param)
        {
            if (id.HasValue && param.HasValue && Session["id"] != null)
            {
                //TODO: Проверка на ид(чтобы была статья с таким ид
                var db = new BlogModelDataContext();
                var rating = db.Ratings.SingleOrDefault(x => x.ArticleId == id.Value && x.AuthorId == int.Parse(Session["id"].ToString()));

                //ПЕРЕДЕЛАТЬ ЭТО КОСТЫЛЬ
                //Если нет рейтинга
                if (rating == null)
                {
                    db.Ratings.InsertOnSubmit(new Ratings() { Amount = param.Value == 1 ? 1 : -1, ArticleId = id.Value, AuthorId = int.Parse(Session["id"].ToString()) });
                }
                else if(rating.Amount == (param.Value == 1 ? 1 : -1))
                {
                    db.Ratings.DeleteOnSubmit(rating);
                }
                db.SubmitChanges();

                ViewBag.IsSuccess = true;
                ViewBag.ArticleId = id.Value;
                ViewBag.Message = @"Вы успешно оценили статью!";
                return View();
            }
            else
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = @"Произошла ошибка.";
                return View();
            }
        }

        public ActionResult SuccessBlogCreate()
        {
            return View();
        }

        public ActionResult WrongId()
        {
            return View();
        }
    }
}