using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebQuiz
{
    class Query
    {
        public string Questions = "SELECT * FROM quizQUESTION WHERE question_questionnaire=@ID order question_rang";
        public string Options = "SELECT * FROM INNER JOIN quizQUESTION on quizQUESTION.question_id=quizOption.option_question WHERE question_questionnaire=@ID order by question_rang,option_rang";
        public string Questionnaire = "SELECT * FROM quizQUESTIONNAIRE WHERE questionnaire_id=@ID";
    }
}
