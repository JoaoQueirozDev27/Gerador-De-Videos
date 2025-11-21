using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Audio.CalcSrtDuraton
{
    public record CalcSrtDurationCommand(string text,string modelPath, string path);
}
