using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.interfaces
{
    public interface IAiService
    {
        public Task<string> SendPrompt(string prompt);
        public Task<string> TransformText(string prompt,string content) =>  SendPrompt(prompt + content);
    
    }

}

