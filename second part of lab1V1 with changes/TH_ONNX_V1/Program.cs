using BertAnalizator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Program
{
    class program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                string path = //@"C:\Users\worka\source\repos\TH_ONNX_V1\text 2.txt";
                    @args[0];
                string text = File.ReadAllText(path);
                string modelWebSource = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
                Console.WriteLine(text);

                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                CancellationToken token = cancelTokenSource.Token;

                var createTask = GetSession.Exist_Download_Model(modelWebSource);

                var Berttoker = await createTask;

                string question;
                while ((question = Console.ReadLine()) != "")
                {
                    var answer = Get_Question(Berttoker, text, question, token);
                }
            }
            static async Task Get_Question(GetSession getSession, string text, string question, CancellationToken token)
            {
                try
                {
                    var answer = await Task.Run(() => getSession.QA_text_Model(text, question, token));
                    Console.WriteLine(question + " : " + answer);
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(question + " : " + ex.Message);
                }
            }
        }
    }
}
