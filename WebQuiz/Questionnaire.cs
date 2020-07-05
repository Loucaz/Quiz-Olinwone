using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using Utilitaires;

namespace WebQuiz
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:Questionnaire runat=server></{0}:Questionnaire>")]
    public class Questionnaire : Literal
    {

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string QuestionnairePath;

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string QuestionnaireUrl;

        DataTable MonQuestionnaire = new DataTable();
        DataTable MesQuestions = new DataTable();
        DataTable MesReponses = new DataTable();
        DataServices DataOlinwone = new DataServices();
        Query Requetes = new Query();
        bool CheckChargement;
        Snippets adnTools = new Snippets();
        int id;
        string dsn;

        public Questionnaire()
        {
            id = 1;
            dsn = "Server=LOUCAZ\\SQLEXPRESS;User Id=user;Password=1234;";
            QuestionnairePath = "@C:\\Users\\Loucaz\\source\\repos\\Quiz\\WebApplication3\\jquizzy\\init.js";
            QuestionnaireUrl = "@https://localhost:44393/jquizzy/jquizzy.htm";
            //dsn = Context.Application["dsn"].ToString();
            //ChargeProprietes((DataTable)Context.Application["composants"]);

            //Chargement de données
            DataOlinwone = new DataServices(dsn);
            //Charge le questionnaire
            MonQuestionnaire = ChargeData(Requetes.Questionnaire, id);
            //Charge les questions
            MesQuestions = ChargeData(Requetes.Questions, id);
            //Charge les options
            MesReponses = ChargeData(Requetes.Options, id);
            //Fermer la connexion
            DataOlinwone.CloseConnection();

            if (CheckChargement)
            {
                //Traitement des données
                IList<Question> json = new List<Question> { };

                foreach (DataRow Item in MesQuestions.Rows)
                {
                    Question result = new Question();
                    result.question = Item["question_valeur"].ToString();
                    DataRow[] answers = MesReponses.Select($"option_question=" + Item["question_id"]);

                    foreach (DataRow answer in answers)
                    {
                        result.answers.Add(answer["option_valeur"].ToString());
                        if ((bool)answer["option_correct"])
                        {
                            result.correctAnswer = (int)answer["option_rang"];
                        }
                    }
                    //Structure les questions
                    json.Add(result);
                }

                //Structure les conclusions
                DataRow MesConclusions = MonQuestionnaire.Rows[0];

                Conclusion conclusion = new Conclusion
                {
                    perfect = MesConclusions["questionnaire_conclusion1"].ToString(),
                    excellent = MesConclusions["questionnaire_conclusion2"].ToString(),
                    good = MesConclusions["questionnaire_conclusion3"].ToString(),
                    average = MesConclusions["questionnaire_conclusion4"].ToString(),
                    bad = MesConclusions["questionnaire_conclusion5"].ToString(),
                    poor = MesConclusions["questionnaire_conclusion6"].ToString(),
                    worst = MesConclusions["questionnaire_conclusion7"].ToString()
                };

                //Génération du fichier
                string jsonSerializedObj = "var init = { 'questions':" + JsonConvert.SerializeObject(json) + ",'resultComments' :" + JsonConvert.SerializeObject(conclusion) + "};";
                //generation automatiquement le nom
                File.WriteAllText(QuestionnairePath, jsonSerializedObj);
            }
            Text = $"<a href=\""+QuestionnaireUrl+ "\">Quiz</a>";

        }

        private DataTable ChargeData(string req, int id)
        {
            DataOlinwone.BuildCommand(req);
            DataOlinwone.AddParameter("@ID", SqlDbType.Int, 0, ParameterDirection.Input, id, "questionnaire_id");
            DataOlinwone.GetStructures();
            DataTable resultat = DataOlinwone.UtilityDataTable;
            CheckChargement = DataOlinwone.UtilityDataExiste;

            DataOlinwone.CloseCommand();
            return resultat;
        }

        private void ChargeProprietes(DataTable _composants)
        {
            /*
            string _thisId;
            string _thisName;
            string _pageId = ((DataRow)Context.Session["page"])["page_id"].ToString();

           try
                {
                //Cherche un composant avec ce code
                QuestionnairePath = adnTools.ChargeAttribut("QuestionnairePath", _composants, ID);
                _thisName = ID;
                 }
            catch
                {
                //Récupère le composant associé à la page
                _thisId = ((DataTable)Context.Application["affectations"]).Select("composant_type='Questionnaire' AND relation_page=" + _pageId)[0]["relation_composant"].ToString();
                _thisName = ((DataTable)Context.Application["composants"]).Select("composantId=" + _thisId)[0]["composant_nom"].ToString();
                 }
                 */

            QuestionnairePath = "..";
            QuestionnaireUrl = "..";
        }


    }
}
