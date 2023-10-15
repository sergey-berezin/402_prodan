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
                string path = @args[0];
                //@"C:\Users\worka\source\repos\TH_ONNX_V1\text 2.txt";
                string text = File.ReadAllText(path);
                Console.WriteLine(text);

                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                CancellationToken token = cancelTokenSource.Token;

                var createTask = await Berttokanalizator.Exist_Download_Model();
                var Berttoker = createTask;
                
                string question; 
                while ((question = Console.ReadLine()) != "")
                {
                    await Get_Question(Berttoker, text, question, token).ConfigureAwait(false);
                }
            }
            async Task Get_Question(Berttokanalizator getSession, string text, string question, CancellationToken token)
            {
                try
                {
                    semaphore.WaitOne(); //захват семафора
                    var answer = await Task.Run(() => getSession.QA_text_Model(text, question, token)); 
                    Console.WriteLine(question + " | " + answer); 
                } 
                catch (Exception ex)
                {
                    Console.WriteLine(question + " | " + ex.Message);
                    semaphore.Dispose();
                }
                finally
                { 
                    semaphore.Release();
                    await Task.Yield(); //возможность выполнения для других потоков 
                    // позволяет обработать накопившиеся события input-output, возрат управлнеия
                    //возможно он и не нужен,т.к. текущий поток занимает семафор но в данном случае это возможно(???) не нужно,
                    //т.к. семафор не блокируется до тех пор, пока не будет вызван метод Release()
                }
            }

        }
    }
}
