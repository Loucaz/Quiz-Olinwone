using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Utilitaires;

namespace Consol
{
    class Program
    {
        static void Main(string[] args)
        {

            Contact contact = new Contact() { Nom =  "Durand", Prenom = "Albert", Mail = "adurand@gmail.com" };

            string jsonSerializedObj = JsonConvert.SerializeObject("'questions': [{'question': 'efe is a...','answers': ['JavaScript library','Ruby Gem','PHP Framework','None of the above'],'correctAnswer': 1}]");
            File.WriteAllText(@"..\..\..\monfichierResultat.son", jsonSerializedObj);
        }
        public class Contact
        {
            public string Nom { get; set; }
            public string Prenom { get; set; }
            public string Mail { get; set; }
        }
    }
}
