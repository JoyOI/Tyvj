﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Text.RegularExpressions;
using Tyvj.DataModels;
using Tyvj.ViewModels;

namespace Tyvj.Controllers
{
    public class UserController : BaseController
    {
        // GET: User
        public ActionResult Index(int id)
        {
            var user = DbContext.Users.Find(id);
            var ac_list = (from s in DbContext.Statuses
                           where s.UserID == id
                           && s.Problem.Hide == false
                           && s.ResultAsInt == (int)JudgeResult.Accepted
                           orderby s.ProblemID ascending
                           select s.ProblemID).Distinct().ToList();
            ac_list.Sort((a, b) => { return a - b; });
            ViewBag.AcceptedList = ac_list;
            return View(user);
        }

        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated == true)
            {
                return Redirect("/");
            }
            if (User.Identity.IsAuthenticated)
                return Redirect("/");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(vLogin model)
        {
            if (User.Identity.IsAuthenticated == true)
            {
                return Redirect("/");
            }
            var user = (from u in DbContext.Users
                        where u.Username == model.Username
                        select u).SingleOrDefault();
            if (user == null)
            {
                ViewBag.Info = "不存在这个用户！";
                return View();
            }

            //更新md5密码为sha1
            if (user.Password.Length == 32)
            {
                if (Helpers.Security.MD5(model.Password).ToUpper() == user.Password.ToUpper())
                {
                    user.Password = Helpers.Security.SHA1(model.Password);
                    DbContext.SaveChanges();
                }
            }

            //更新明文密码为sha1
            if (user.Password.Length < 16)
            {
                if (user.Password == model.Password)
                {
                    user.Password = Helpers.Security.SHA1(model.Password);
                    DbContext.SaveChanges();
                }
            }

            if (user.Password != Helpers.Security.SHA1(model.Password))
            {
                ViewBag.Info = "密码错误！";
                return View();
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.Remember);
                user.LastLoginTime = DateTime.Now;
                DbContext.SaveChanges();
                if (Request.UrlReferrer == null)
                    return Redirect("/");
                else
                    return Redirect(Request.UrlReferrer.ToString());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            var user = (from u in DbContext.Users
                        where u.Username == User.Identity.Name
                        select u).Single();
            DbContext.SaveChanges();
            FormsAuthentication.SignOut();
            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated == true)
            {
                return Redirect("/");
            }
            return View();
        }

        [Authorize]
        public ActionResult Settings(int id)
        {
            var user = DbContext.Users.Find(id);
            return View(user);
        }

        public ActionResult Contests(int id)
        {
            var user = DbContext.Users.Find(id);
            var contests = (from c in DbContext.Contests
                            where c.UserID == id
                            orderby c.ID descending
                            select c).ToList();
            ViewBag.Contests = contests;
            return View(user);
        }

        [HttpPost]
        public ActionResult Register(string Email)
        {
            if ((from u in DbContext.Users where u.Email == Email select u).Count() > 0)
                return Message("这个电子邮箱已经被注册过了，请返回更换电子邮箱后重新尝试注册！");
            if (!Helpers.Regexes.Email.IsMatch(Email))
                return Message("您输入的电子邮箱不合法，请返回更换电子邮箱后重新尝试注册！");

            EmailVerification email_verification = (from ev in DbContext.EmailVerifications
                                                    where ev.Email == Email
                                                    select ev).SingleOrDefault();
            if (email_verification == null)
            {
                email_verification = new EmailVerification
                {
                    Email = Email,
                    Time = DateTime.Now.AddHours(2),
                    Token = Helpers.String.RandomString(16)
                };
                DbContext.EmailVerifications.Add(email_verification);
            }
            else
            {
                email_verification.Time = DateTime.Now.AddHours(2);
                email_verification.Token = Helpers.String.RandomString(16);
            }
            DbContext.SaveChanges();

            var Sitename = "Tyvj";
            var SiteAddress = Request.Url.ToString().Replace(Request.RawUrl.ToString(), "");
            string strEmail = "<!DOCTYPE HTML><html><head><meta charset=\"UTF-8\"/><title>[Title]</title><style>p{margin:5px 0px}a{color:#1D76C7;text-decoration:none}.body{margin:0px;color:#333333;font-size:14px;font-family:Tahoma,'Segoe UI',Verdana,微软雅黑,'Microsoft YaHei',宋体;padding:30px;background-color:#F2F2F2}.container{box-shadow:rgba(0,0,0,0.3)0px 0px 15px;border-top-left-radius:5px;border-top-right-radius:5px;border-bottom-left-radius:5px;border-bottom-right-radius:5px;background-color:#FFF}.header{color:#FFF;padding:10px;line-height:200%;font-size:15px;border-top-left-radius:5px;border-top-right-radius:5px;border-bottom-left-radius:0px;border-bottom-right-radius:0px;border-bottom-width:3px;border-bottom-style:solid;border-bottom-color:#85CAEB;background-color:#3AA9DE}.problem-body{padding:30px}.link{padding:5px 10px;border-left-width:10px;border-left-style:solid;border-left-color:#E2EFFA;margin:20px 20px 20px 0px}.footer{color:#444;padding:10px;font-size:12px;border-top-width:1px;border-top-style:solid;border-top-color:#DDD;border-top-left-radius:0px;border-top-right-radius:0px;border-bottom-left-radius:5px;border-bottom-right-radius:5px;background-color:#F4F4F4}</style></head><body><div class=\"body\"><div class=\"container\"><div class=\"header\">新用户激活 - "+Sitename+"</div><div class=\"body\"><p><strong>您好，欢迎您注册"+Sitename+"帐号，请根据下面的提示信息继续完成注册操作。</strong></p><p>请点击下面的链接完成帐号工作，激活成功后将会自动登录"+Sitename+"系统：</p><blockquote class=\"link\"><p><a href=\""+SiteAddress+"/Verify/Register/" + email_verification.ID + "/" + email_verification.Token + "\" target=\"_blank\">"+SiteAddress+"/Verify/Register/" + email_verification.ID + "/" + email_verification.Token + "</a></p></blockquote><p>如果这次操作不是您本人的行为，请忽略本条邮件。</p></div><div class=\"footer\"><p>这封邮件由<a href=\""+SiteAddress+"\"target=\"_blank\">"+Sitename+"</a>自动发送，请勿直接回复。</p></div></div></div></body></html>";

            Helpers.SMTP.Send(email_verification.Email, Sitename+" 用户注册邮箱验证", strEmail);
            return Message( "我们已经向" + email_verification.Email + "中发送了一封包含验证链接的电子邮件，请根据电子邮件中的提示进行下一步的注册。");
        }

        public ActionResult RegisterDetail()
        {
            if (Session["Email"] == null)
                return Message("非法访问");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterDetail(vRegister model)
        {
            if (Session["Email"] == null)
                return Message("非法访问。");
            if (!Regex.IsMatch(model.Username, @"^\w+$") || Helpers.String.StringLen(model.Username) < 4 || Helpers.String.StringLen(model.Username) > 16)
                return Message("用户名不合法，用户名长度必须为4~16个字符，同时只允许使用英文字母、数字和下划线" );
            var user = (from u in DbContext.Users
                        where u.Username == model.Username
                        select u).SingleOrDefault();
            var email = Session["Email"].ToString();
            if (user != null) return Message("用户名已经存在，请返回更换用户名再试！" );
            DbContext.Users.Add(new User
            {
                Username = model.Username,
                Password = Helpers.Security.SHA1(model.Password),
                Email = email,
                LastLoginTime = DateTime.Now,
                RegisterTime = DateTime.Now,
                Role = UserRole.Member,
                Motto = "",
                CommonLanguage = Language.C,
                Gravatar = email,
                QQ = model.QQ,
                School = model.School
            });
            DbContext.SaveChanges();
            var email_verification = (from ev in DbContext.EmailVerifications
                                      where ev.Email == email
                                      select ev).Single();
            DbContext.EmailVerifications.Remove(email_verification);
            DbContext.SaveChanges();
            return Message("注册成功，您可以通过右上方登录按钮进行登录操作");
        }
    }
}