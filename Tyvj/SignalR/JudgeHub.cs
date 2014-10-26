﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Microsoft.AspNet.SignalR;
using CodeComb.Judge.Models;
using Tyvj.DataModels;
using Tyvj.ViewModels;

namespace Tyvj.SignalR
{
    public class JudgeHub : Hub
    {
        public static List<Client> Online = new List<Client>();
        public DB DbContext = new DB();
        public static Microsoft.AspNet.SignalR.IHubContext context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<JudgeHub>();
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var index = Online.FindIndex(x=>x.Token == Context.ConnectionId);
            if (index >= 0)
            {
                Groups.Remove(Context.ConnectionId, Online[index].Username);
                Online.RemoveAt(index);
            }
            //if (Online.Count == 0)
            //{ 
            //    var ids = (from u in DbContext.Users
            //                   where u.RoleAsInt >= (int)UserRole.Master
            //                   select u.ID).ToList();
            //    foreach (var id in ids)
            //        SignalR.MobileHub.PushTo(id, "评测机全部离线，请修复。");
            //}
            return base.OnDisconnected(stopCalled);
        }
        public UploadTask GetTestCase(int ID)
        {
            if (Online.FindIndex(x => x.Token == Context.ConnectionId) < 0)
                return null;
            var upload = (from tc in DbContext.TestCases
                          where tc.ID == ID
                          let hasoutput = tc.Output.Length == 0 ? false : true
                          select new UploadTask
                          {
                              ID = ID,
                              Hash = tc.Hash,
                              HasOutput = hasoutput,
                              Input = tc.Input,
                              Output = tc.Output
                          }).Single();
            return upload;
        }
        public string GetTestCaseHash(int ID)
        {
            if (Online.FindIndex(x => x.Token == Context.ConnectionId) < 0)
                return null;
            return DbContext.TestCases.Find(ID).Hash;
        }
        public void Auth(string Username, string Password, int MaxThreads)
        {
            var pwd = Helpers.Security.SHA1(Password);
            var user = (from u in DbContext.Users
                        where u.Username == Username
                        && u.Password == pwd
                        && u.RoleAsInt == (int)Tyvj.DataModels.UserRole.Root
                        select u).SingleOrDefault();
            if (user != null)
            {
                var index = Online.FindIndex(x => x.Username == user.Username);
                Client client;
                if (index >= 0)
                {
                    Groups.Remove(Online[index].Token, Online[index].Username);
                    Online[index].Token = Context.ConnectionId;
                    Groups.Add(Context.ConnectionId, user.Username);
                    client = Online[index];
                }
                else
                {
                    client = new Client { Token = Context.ConnectionId, Username = user.Username, MaxThreads = MaxThreads, CurrentThreads = 0 };
                    Online.Add(client);
                }
                Online = Online.Distinct().ToList();
                System.Threading.Tasks.Task.Factory.StartNew(() => 
                {
                    SignalR.UserHub.context.Clients.Group("System Judge").onJudgerStatusChanged(new vJudger(user, client));
                });
                Groups.Add(Context.ConnectionId, user.Username);
                Clients.Group(user.Username).onMessage(String.Format("{0}，欢迎您。您已经成功注册成为Code Comb评测机！", user.Username));
                var time = DateTime.Now.AddMinutes(-5);
                var err_statuses = (from s in DbContext.Statuses
                           where s.ResultAsInt == (int)JudgeResult.Pending
                           || (s.ResultAsInt == (int)JudgeResult.Running && s.Time< time)
                           select s).ToList();
                foreach (var status in err_statuses)
                {
                    foreach (var jt in status.JudgeTasks.ToList())
                    {
                        var judgetask = new CodeComb.Judge.Models.JudgeTask(jt);
                        ThreadBusy(user.Username);
                        System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            SignalR.JudgeHub.context.Clients.Group(user.Username).Judge(judgetask);
                        });
                    }
                    status.Result = JudgeResult.Running;
                    DbContext.SaveChanges();
                    SignalR.UserHub.context.Clients.All.onStatusCreated(new vStatus(status));//推送新状态
                }
            }
        }
        public void JudgeFeedBack(JudgeFeedback jfb)
        {
            DB db = new DB();
            if (Online.FindIndex(x => x.Token == Context.ConnectionId) < 0)
                return;
            var jt = db.JudgeTasks.Find(jfb.ID);
            jt.Hint = jfb.Hint;
            jt.MemoryUsage = jfb.MemoryUsage;
            jt.TimeUsage = jfb.TimeUsage;
            jt.Result = jfb.Result;
            db.SaveChanges();
            ThreadFree();
            if (jt.Status.JudgeTasks.Where(x => x.ResultAsInt == (int)JudgeResult.Running || x.ResultAsInt == (int)JudgeResult.Pending).Count() == 0)
            {
                jt.Status.ResultAsInt = jt.Status.JudgeTasks.Max(x => x.ResultAsInt);
                db.SaveChanges();
                if (jt.Status.Result == JudgeResult.Accepted)
                {
                    var aclist = Helpers.AcList.GetList(jt.Status.User.AcceptedList);
                    aclist.Add(jt.Status.ProblemID);
                    jt.Status.User.AcceptedList = Helpers.AcList.ToString(aclist);
                    jt.Status.User.AcceptedCount = jt.Status.User.AcceptedList.Count();
                    jt.Status.Problem.AcceptedCount++;
                    db.SaveChanges();
                }
                var contest = jt.Status.Contest;
                if (contest!=null && DateTime.Now >= contest.Begin && DateTime.Now < contest.End)
                {
                    SignalR.UserHub.context.Clients.All.onStandingsChanged(contest.ID, new vStanding(jt.Status.User, contest));
                }
            }
            SignalR.UserHub.context.Clients.All.onStatusChanged(new vStatus(jt.Status));//推送新状态
        }
        public void HackFeedBack(HackFeedback hfb)
        {
            if (Online.FindIndex(x => x.Token == Context.ConnectionId) < 0)
                return;
            var hack = DbContext.Hacks.Find(hfb.HackID);
            hack.Result = hfb.Result;
            hack.Hint = hfb.Hint;
            DbContext.SaveChanges();
            if (hack.Result == HackResult.Success)
            {
                var hash = Helpers.Security.SHA1(hack.InputData);
                var tc = (from t in DbContext.TestCases where t.Hash == hash select t).FirstOrDefault();
                if (tc == null)
                { 
                    tc = new TestCase
                    {
                        Input = hack.InputData,
                        Hash = Helpers.Security.SHA1(hack.InputData),
                        Type = TestCaseType.Custom,
                        ProblemID = hack.Status.ProblemID,
                        Output = hfb.Output
                    };
                    DbContext.TestCases.Add(tc);
                }
                DbContext.JudgeTasks.Add(new Tyvj.DataModels.JudgeTask
                {
                    StatusID = hack.StatusID,
                    Result = hfb.JudgeResult,
                    TimeUsage = hfb.TimeUsage,
                    MemoryUsage = hfb.MemoryUsage,
                    Hint = hfb.Hint,
                    TestCaseID = tc.ID
                });
                hack.Status.Result = JudgeResult.Hacked;
                DbContext.SaveChanges();
                SignalR.UserHub.context.Clients.Group(hack.Defender.Username).onHacked(new vHackResult(hack, true));
                SignalR.UserHub.context.Clients.Group(hack.Hacker.Username).onHackFinished(new vHackResult(hack));

                SignalR.UserHub.context.Clients.All.onStatusChanged(new vStatus(hack.Status));
                if (DateTime.Now >= hack.Status.Contest.Begin)
                {
                    SignalR.UserHub.context.Clients.All.onStandingsChanged(hack.Status.Contest.ID, new vStanding(hack.Defender, hack.Status.Contest));
                    SignalR.UserHub.context.Clients.All.onStandingsChanged(hack.Status.Contest.ID, new vStanding(hack.Hacker, hack.Status.Contest));
                }
            }
            else
            {
                SignalR.UserHub.context.Clients.Group(hack.Hacker.Username).onHackFinished(new vHackResult(hack));
                if (DateTime.Now >= hack.Status.Contest.Begin && DateTime.Now < hack.Status.Contest.End)
                {
                    SignalR.UserHub.context.Clients.All.onStandingsChanged(hack.Status.Contest.ID, new vStanding(hack.Hacker, hack.Status.Contest));
                }
            }
        }
        public static void ThreadBusy(string JudgerName)
        {
            var index = Online.FindIndex(x => x.Username == JudgerName);
            if (index >= 0)
            {
                Online[index].CurrentThreads++;
                DB DbContext = new DB();
                var username = Online[index].Username;
                var user = (from u in DbContext.Users where u.Username == username select u).Single();
                SignalR.UserHub.context.Clients.Group("System Judge").onJudgerStatusChanged(new vJudger(user, Online[index]));
            }
        }
        public static string GetNode()
        {
            if (Online.Count == 0) return null;
            Online = Online.OrderByDescending(x=>x.FreeThreads).ThenBy(x=>x.MaxThreads).ToList();
            if (Online.Where(x => x.Ratio < 100).Count() == 0)
                Online = Online.OrderBy(x => x.Ratio).ToList();
            return Online[0].Username;
        }
        private void ThreadFree()
        {
            var index = Online.FindIndex(x => x.Token == Context.ConnectionId);
            if (index >= 0)
            {
                Online[index].CurrentThreads--;
                if (Online[index].CurrentThreads < 0)
                    Online[index].CurrentThreads = 0;
                var username = Online[index].Username;
                var user = (from u in DbContext.Users where u.Username == username select u).Single();
                SignalR.UserHub.context.Clients.Group("System Judge").onJudgerStatusChanged(new vJudger(user, Online[index]));
            }
        }
    }
}