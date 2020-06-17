using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz
{
    class Query
    {
        public string question = "SELECT question_id,question_valeur FROM quizQUESTION WHERE questionnaire_id=@ID";
        public string option = "SELECT option_valeur,option_correct,option_rang FROM quizOption WHERE question_id=@ID";
        public string conclusion = "SELECT questionnaire_conclusion1,questionnaire_conclusion2,questionnaire_conclusion3,questionnaire_conclusion4,questionnaire_conclusion5,questionnaire_conclusion6,questionnaire_conclusion7 FROM quizQUESTIONNAIRE WHERE questionnaire_id=@ID";
    }
}
