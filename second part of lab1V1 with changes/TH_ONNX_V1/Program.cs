using BertAnalizator;

namespace Program
{
    class program
    {
        static Semaphore semaphore = new Semaphore(1, 1);
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                string path =  @args[0];
                //@"C:\Users\worka\source\repos\TH_ONNX_V1\text 2.txt";
                string text = File.ReadAllText(path);
                string modelWebSource = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
                Console.WriteLine(text);

                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                CancellationToken token = cancelTokenSource.Token;

                var createTask = await Berttokanalizator.Exist_Download_Model(modelWebSource);
                var Berttoker = createTask;
                
                string question; 
                semaphore.WaitOne();
                while ((question = Console.ReadLine()) != "")
                {
                    var answer = Get_Question(Berttoker, text, question, token);
                    semaphore.Release();
                }
            }
            async Task Get_Question(Berttokanalizator getSession, string text, string question, CancellationToken token)
            {
                try
                {
                    
                    var answer = await Task.Run(() => getSession.QA_text_Model(text, question, token)); 
                    semaphore.WaitOne();
                    Console.WriteLine(question + " | " + answer);
                    semaphore.Release();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(question + " | " + ex.Message);
                    semaphore.Dispose();
                }
            }
        }
    }
}