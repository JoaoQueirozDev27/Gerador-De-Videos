using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Services.ContentSources.AiContentSource
{
    public record AiContentSourceRequest(string prompt, List<string> variables)
    {
        public string getPrompt() {
            string result = prompt;
            foreach (var variable in variables)
            {   
                var regex = new Regex(@"/\*.*?\*/");

                result = regex.Replace(result, variable, 1);
            }

            return result;
        }
    }
}
