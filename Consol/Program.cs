﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Utilitaires;
using Quiz;

namespace Consol
{
    class Program
    {
        static void Main(string[] args)
        {
            Questionnaire quiz = new Questionnaire("Server=LOUCAZ\\SQLEXPRESS;User Id=user;Password=1234;",1);
        }

    }
}