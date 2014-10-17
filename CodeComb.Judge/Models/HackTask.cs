﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tyvj.DataModels;

namespace CodeComb.Judge.Models
{
    public class HackTask
    {
        public HackTask() { }
        public HackTask(Hack hack) 
        {
            HackID = hack.ID;
            StatusID = hack.StatusID;
            Code = hack.Status.Code;
            CodeLanguage = hack.Status.Language;
            SpecialJudgeCode = hack.Status.Problem.SpecialJudge;
            SpecialJudgeCodeLanguage = hack.Status.Problem.SpecialJudgeLanguage;
            RangeValidatorCode = hack.Status.Problem.RangeValidator;
            RangeValidatorCodeLanguage = hack.Status.Problem.RangeValidatorLanguage;
            StandardCode = hack.Status.Problem.StandardProgram;
            StandardCodeLanguage = hack.Status.Problem.StandardProgramLanguage;
            DataMakerCode = hack.DataMakerCode;
            DataMakerCodeLanguage = hack.DataMakerLanguage;
            InputData = hack.InputData;
            TimeLimit = hack.Status.Problem.TimeLimit;
            MemoryLimit = hack.Status.Problem.MemoryLimit;
        }
        public string Token { get; set; }
        public int StatusID { get; set; }
        public int HackID { get; set; }
        public string Code { get; set; }
        public Language CodeLanguage { get; set; }
        public string SpecialJudgeCode { get; set; }
        public Language SpecialJudgeCodeLanguage { get; set; }
        public string RangeValidatorCode { get; set; }
        public Language RangeValidatorCodeLanguage { get; set; }
        public string StandardCode { get; set; }
        public Language StandardCodeLanguage { get; set; }
        public string DataMakerCode { get; set; }
        public Language DataMakerCodeLanguage { get; set; }
        public string InputData { get; set; }
        public int TimeLimit { get; set; }
        public int MemoryLimit { get; set; }
    }
}
