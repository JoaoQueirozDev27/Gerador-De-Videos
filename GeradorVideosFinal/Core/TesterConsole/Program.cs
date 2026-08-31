using Application.interfaces;
using Application.UseCases.Audio.CalcSrtDuraton;
using Application.UseCases.Audio.TextToSpeech;
using Application.UseCases.IA.GetSummarizedSiteContentCommand;
using Application.UseCases.Imagem.GenerateImage;
using Services;
using Services.Factories;
using Spectre.Console;
using Domain.Entities;
using Services.ContentSources.AiContentSource;
using System.Reflection;

namespace Presentation
{
    class Program
    {
        public static void Main(string[] args)
        {
            AiContentSourceResponse aiContentSourceResponse = new();

            aiContentSourceResponse.Id = 1;
            aiContentSourceResponse.Title = "Teste";
            aiContentSourceResponse.Result = "Resultado";


            PropertyInfo? propriedade = typeof(AiContentSourceResponse).GetProperty("Result");
            PropertyInfo[]? propriedades = typeof(AiContentSourceResponse).GetProperties();

            foreach (PropertyInfo item in propriedades)
            {
                AnsiConsole.MarkupLine(item.Name.ToString());
            }
            /*
            if (propriedade != null)
            {
                var valor = propriedade.GetValue(aiContentSourceResponse);
                Console.WriteLine($"Valor da propriedade 'Result': {valor}");
            }
            else
            {
                Console.WriteLine("Propriedade 'Result' não encontrada.");
            }
            */
        }
    }
}

