﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tyvj.DataModels;

namespace CodeComb.Judge.Models
{
    public class JudgeTask
    {
        public JudgeTask() { }
        public JudgeTask(Tyvj.DataModels.JudgeTask jt) 
        {
            ID = jt.ID;
            Code = jt.Status.Code;
            CodeLanguage = jt.Status.Language;
            DataID = jt.TestCaseID;
            SpecialJudgeCode = jt.Status.Problem.SpecialJudge;
            SpecialJudgeCodeLanguage = jt.Status.Problem.SpecialJudgeLanguage;
            TimeLimit = jt.Status.Problem.TimeLimit;
            MemoryLimit = jt.Status.Problem.MemoryLimit;
        }
        public string Token { get; set; }
        public int ID { get; set; }
        public string Code { get; set; }
        public Language CodeLanguage { get; set; }
        public int DataID { get; set; }
        public string SpecialJudgeCode { get; set; }
        public Language SpecialJudgeCodeLanguage { get; set; }
        public int TimeLimit { get; set; }
        public int MemoryLimit { get; set; }
    }
}
