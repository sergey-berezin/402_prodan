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

//for nuget begin
//public class GetSession
//{
//    private InferenceSession session;
//    private static string modelUrl;
//    private static string modelPath;
//    CancellationToken cancelToken;

//    private GetSession(InferenceSession inferenceSession)
//    {
//        this.session = inferenceSession;
//    }
//    public static async Task<GetSession> Exist_Download_Model(string modelWebSource, InferenceSession session)
//    {
//        try
//        {
//            String path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
//            var modelPath = Path.Combine(path, "bert-large-uncased-whole-word-masking-finetuned-squad.onnx");
//            string downaloadedModelPath = modelPath;

//            if (!File.Exists(modelPath))
//            {
//                downaloadedModelPath = await Download_Model(modelPath, modelWebSource);
//                //session = new InferenceSession(modelPath);
//            }
//            return new GetSession(new InferenceSession(downaloadedModelPath));
//            // return session;
//        }
//        catch
//        {
//            throw;
//        }
//    }
//    public static async Task<string> Download_Model(string modelPath, string modelWebSource)
//    {
//        try
//        {
//            var httpClient = new HttpClient();
//            bool Downloaded = false;
//            while (!Downloaded)
//            {
//                try
//                {
//                    using var stream = await httpClient.GetStreamAsync(modelWebSource);
//                    using var fileStream = new FileStream(modelPath, FileMode.CreateNew);
//                    await stream.CopyToAsync(fileStream);
//                    Downloaded = true;
//                }
//                catch (Exception)
//                {
//                    //await Task.Delay(3000);
//                }
//            }
//            if (!File.Exists(modelPath))
//            {
//                throw new Exception("cannot download model!");
//            }
//            return modelPath;
//        }
//        catch (Exception)
//        {
//            throw;
//        }
//    }

//    //public async Task<string> QA_text_Model(string text, string question, CancellationToken token)
//    //{
//реакцию на исключения лучше не рассматривать, т.к. не понятно как с ними дальше работать
//    //}
//for nuget end
// // ОТДЕЛЬНЫМ КЛАССОМ

//    static async Task Main(string[] args)
//    {

//    }
//}




//АНАЛИЗАТОР, БЕЗ ГЛАВНОГО И ОТДЕЛЬНАЯ ЧАСТЬ ДЛЯ ЗАПУСКА(два пространства имен)
//АНАЛИЗАТОР УЖЕ НЕЗАВИСИМ ОТ ФАЙЛОВОЙ СИСТЕМЫ

//    internal class Program
//    {
//        private static readonly HttpClient httpClient = new HttpClient();
//        private static readonly string modelUrl =
//               "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
//        private static readonly string modelPath = 
//            @"C:\Users\worka\source\repos\TH_ONNX_V1\bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
//        private static async Task DownloadModelAsync()
//        {
//            using (var response = await httpClient.GetAsync(modelUrl, HttpCompletionOption.ResponseHeadersRead))
//            {
//                response.EnsureSuccessStatusCode();
//                using (var fileStream = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
//                {
//                    await response.Content.CopyToAsync(fileStream);
//                }
//                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
//                CancellationToken token = cancelTokenSource.Token;
//            }
//        }

//        static async Task Main(string[] args)
//        {
//            if (File.Exists(modelPath))
//            {
//                Console.WriteLine("Model exists");
//            }
//            else
//            {
//                Console.WriteLine("Downloading model...");
//                await DownloadModelAsync();
//            }
//            string path = @"C:\Users\worka\source\repos\TH_ONNX_V1\text 2.txt";
//            string text = File.ReadAllText(path);


//            while (true)
//            {
//                string context_CTX = text;
//                string question_QTX = Console.ReadLine();

//                if (string.IsNullOrEmpty(question_QTX))
//                    break;

//                var sentence = "{\"question\": \"@QTX\", \"context\": \"@CTX\"}".Replace("@QTX", question_QTX).Replace("CTX", context_CTX);

//                var tokenizer = new BertUncasedLargeTokenizer();
//                var tokens = tokenizer.Tokenize(sentence);
//                //Console.WriteLine(String.Join(", ", tokens));
//                var encoded = tokenizer.Encode(tokens.Count(), sentence);

//                var bertInput = new BertInput()
//                {
//                    InputIds = encoded.Select(t => t.InputIds).ToArray(),
//                    AttentionMask = encoded.Select(t => t.AttentionMask).ToArray(),
//                    TypeIds = encoded.Select(t => t.TokenTypeIds).ToArray(),
//                };
//                var modelPath = @"C:\Users\worka\source\repos\TH_ONNX_V1\bert-large-uncased-whole-word-masking-finetuned-squad.onnx";

//                var input_ids = ConvertToTensor(bertInput.InputIds, bertInput.InputIds.Length);
//                var attention_mask = ConvertToTensor(bertInput.AttentionMask, bertInput.InputIds.Length);
//                var token_type_ids = ConvertToTensor(bertInput.TypeIds, bertInput.InputIds.Length);

//                var input = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input_ids", input_ids),
//                                         NamedOnnxValue.CreateFromTensor("input_mask", attention_mask),
//                                         NamedOnnxValue.CreateFromTensor("segment_ids", token_type_ids) };

//                var session = new InferenceSession(modelPath);

//                var output = session.Run(input);

//                List<float> startLogits = (output.ToList().First().Value as IEnumerable<float>).ToList();
//                List<float> endLogits = (output.ToList().Last().Value as IEnumerable<float>).ToList();

//                var startIndex = startLogits.ToList().IndexOf(startLogits.Max());
//                var endIndex = endLogits.ToList().IndexOf(endLogits.Max());

//                var predictedTokens = tokens
//                            .Skip(startIndex)
//                            .Take(endIndex + 1 - startIndex)
//                            .Select(o => tokenizer.IdToToken((int)o.VocabularyIndex))
//                            .ToList();
//                Task.Delay(3000).Wait();
//                Console.WriteLine("Ans:");
//                Console.WriteLine(String.Join(" ", predictedTokens));

//                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
//                CancellationToken token = cancelTokenSource.Token;
//            }

//            //var question = "who is the main character? \n";

//            ////var sentence = "{\"question\": \"who is the main character?\", \"context\": \"@CTX\"}".Replace("@CTX", hobbit);
//            //var sentence = "{\"question\": \"@QTX\", \"context\": \"@CTX\"}".Replace("@QTX", question).Replace("CTX", text);
//            //Console.WriteLine(sentence)

//        }
//        public static Tensor<long> ConvertToTensor(long[] inputArray, int inputDimension)
//        {
//            // Create a tensor with the shape the model is expecting. Here we are sending in 1 batch with the inputDimension as the amount of tokens.
//            Tensor<long> input = new DenseTensor<long>(new[] { 1, inputDimension });

//            // Loop through the inputArray (InputIds, AttentionMask and TypeIds)
//            for (var i = 0; i < inputArray.Length; i++)
//            {
//                // Add each to the input Tenor result.
//                // Set index and array value of each input Tensor.
//                input[0, i] = inputArray[i];
//            }
//            return input;
//        }
//    }
//    public class BertInput
//    {
//        public long[] InputIds { get; set; }
//        public long[] AttentionMask { get; set; }
//        public long[] TypeIds { get; set; }
//    }
