﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tyvj.DataModels
{
    public class CommonEnums
    {
        public static string[] LanguageDisplay = { "C", "C++", "C++11", "Java", "Pascal", "Python 2.7", "Python 3.3", "Ruby", "C#", "VB.Net", "F#" };
        public static string[] JudgeResultDisplay = { "Accepted", "Presentation Error", "Wrong Answer", "Output Limit Exceeded", "Time Limit Exceeded", "Memory Limit Exceeded", "Runtime Error", "Compile Error", "System Error", "Hacked", "Running", "Pending", "Hidden" };
        public static string[] HackResultDisplay = { "Success", "Failure", "Bad Data", "Data-maker Error", "Systeｍ Error", "Running", "Pending" };
    }
    public enum Language { C, Cxx, Cxx11, Java, Pascal, Python27, Python33, Ruby, CSharp, VB, FSharp };
    public enum JudgeResult { Accepted, PresentationError, WrongAnswer, OutputLimitExceeded, TimeLimitExceeded, MemoryLimitExceeded, RuntimeError, CompileError, SystemError, Hacked, Running, Pending, Hidden };
    public enum HackResult { Success, Failure, BadData, DatamakerError, SystemError, Running, Pending };
}