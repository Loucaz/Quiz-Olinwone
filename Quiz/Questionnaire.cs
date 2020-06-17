using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using Utilitaires;

namespace Quiz
{
    public class Questionnaire
    {
        public Questionnaire(string dsn, int id)
        {
            Query query = new Query();
            IList<Question> json = new List<Question> { };
            DataServices data = new DataServices(dsn, query.question);
            data.AddParameter("@ID", SqlDbType.Int, 255, ParameterDirection.Input, id, "questionnaire_id");
            data.GetStructures();
            DataTable question = data.UtilityDataTable;
            data.Dispose();
            foreach (DataRow rowQuestion in question.Rows)
            {
                Question result = new Question();
                result.question = rowQuestion["question_valeur"].ToString();

                data = new DataServices(dsn, query.option);
                data.AddParameter("@ID", SqlDbType.Int, 255, ParameterDirection.Input, rowQuestion["question_id"], "question_id");
                data.GetStructures();
                DataTable option = data.UtilityDataTable;
                data.Dispose();
                foreach (DataRow rowO in option.Rows)
                {
                    result.answers.Add(rowO["option_valeur"].ToString());
                    if((bool)rowO["option_correct"])
                    {
                        result.correctAnswer = (int) rowO["option_rang"];
                    }
                }
                json.Add(result);
            }
            data = new DataServices(dsn, query.conclusion);
            data.AddParameter("@ID", SqlDbType.Int, 255, ParameterDirection.Input, id, "questionnaire_id");
            data.GetStructures();
            DataTable conclu = data.UtilityDataTable;
            data.Dispose();
            Conclusion conclusion = new Conclusion
            {
                perfect = conclu.Rows[0]["questionnaire_conclusion1"].ToString(),
                excellent = conclu.Rows[0]["questionnaire_conclusion2"].ToString(),
                good = conclu.Rows[0]["questionnaire_conclusion3"].ToString(),
                average = conclu.Rows[0]["questionnaire_conclusion4"].ToString(),
                bad = conclu.Rows[0]["questionnaire_conclusion5"].ToString(),
                poor = conclu.Rows[0]["questionnaire_conclusion6"].ToString(),
                worst = conclu.Rows[0]["questionnaire_conclusion7"].ToString()
            };

            string jsonSerializedObj = "var init = { 'questions':"+JsonConvert.SerializeObject(json)+ ",'resultComments' :"+ JsonConvert.SerializeObject(conclusion) + "};";
            File.WriteAllText(@"..\..\..\init.js", jsonSerializedObj);
        }
    }
}
