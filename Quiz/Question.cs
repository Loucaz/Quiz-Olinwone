using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz
{
    class Question
    {
        public string question;
        public IList<string> answers = new List<string> { };
        public int correctAnswer;

    }

    class Conclusion
    {
        public string perfect;
        public string excellent;
        public string good;
        public string average;
        public string bad;
        public string poor;
        public string worst;
    }
}
