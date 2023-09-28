using BERTTokenizers;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace ONNX_V1
{
    internal class Program
    {
        static public Queue<string> progressBar = new Queue<string>();
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly string modelUrl =
               "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        private static readonly string modelPath = @"C:\Users\worka\source\repos\TH_ONNX_V1\bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        private static async Task DownloadModelAsync()
        {
            using (var response = await httpClient.GetAsync(modelUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                using (var fileStream = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }
        }

        static async Task Main(string[] args)
        {
            if (File.Exists(modelPath))
            {
                Console.WriteLine("Model exists");
            }
            else
            {
                Console.WriteLine("Downloading model...");
                await DownloadModelAsync();
            }
            string path = @"C:\Users\worka\source\repos\TH_ONNX_V1\text 2.txt";
            string text = File.ReadAllText(path);


            while (true) 
            {
                string context_CTX = text;
                string question_QTX = Console.ReadLine();
               
                if (string.IsNullOrEmpty(question_QTX))
                    break;
                
                var sentence = "{\"question\": \"@QTX\", \"context\": \"@CTX\"}".Replace("@QTX", question_QTX).Replace("CTX",context_CTX);

                var tokenizer = new BertUncasedLargeTokenizer();
                var tokens = tokenizer.Tokenize(sentence);
                //Console.WriteLine(String.Join(", ", tokens));
                var encoded = tokenizer.Encode(tokens.Count(), sentence);

                var bertInput = new BertInput()
                {
                    InputIds = encoded.Select(t => t.InputIds).ToArray(),
                    AttentionMask = encoded.Select(t => t.AttentionMask).ToArray(),
                    TypeIds = encoded.Select(t => t.TokenTypeIds).ToArray(),
                };
                var modelPath = @"C:\Users\worka\source\repos\TH_ONNX_V1\bert-large-uncased-whole-word-masking-finetuned-squad.onnx";

                var input_ids = ConvertToTensor(bertInput.InputIds, bertInput.InputIds.Length);
                var attention_mask = ConvertToTensor(bertInput.AttentionMask, bertInput.InputIds.Length);
                var token_type_ids = ConvertToTensor(bertInput.TypeIds, bertInput.InputIds.Length);

                var input = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input_ids", input_ids),
                                         NamedOnnxValue.CreateFromTensor("input_mask", attention_mask),
                                         NamedOnnxValue.CreateFromTensor("segment_ids", token_type_ids) };

                var session = new InferenceSession(modelPath);

                var output = session.Run(input);

                List<float> startLogits = (output.ToList().First().Value as IEnumerable<float>).ToList();
                List<float> endLogits = (output.ToList().Last().Value as IEnumerable<float>).ToList();

                var startIndex = startLogits.ToList().IndexOf(startLogits.Max());
                var endIndex = endLogits.ToList().IndexOf(endLogits.Max());

                var predictedTokens = tokens
                            .Skip(startIndex)
                            .Take(endIndex + 1 - startIndex)
                            .Select(o => tokenizer.IdToToken((int)o.VocabularyIndex))
                            .ToList();
                Task.Delay(3000).Wait();
                Console.WriteLine("Ans:");
                Console.WriteLine(String.Join(" ", predictedTokens));

                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                CancellationToken token = cancelTokenSource.Token;
            }

            //var question = "who is the main character? \n";

            ////var sentence = "{\"question\": \"who is the main character?\", \"context\": \"@CTX\"}".Replace("@CTX", hobbit);
            //var sentence = "{\"question\": \"@QTX\", \"context\": \"@CTX\"}".Replace("@QTX", question).Replace("CTX", text);
            //Console.WriteLine(sentence)

        }
        public static Tensor<long> ConvertToTensor(long[] inputArray, int inputDimension)
        {
            // Create a tensor with the shape the model is expecting. Here we are sending in 1 batch with the inputDimension as the amount of tokens.
            Tensor<long> input = new DenseTensor<long>(new[] { 1, inputDimension });

            // Loop through the inputArray (InputIds, AttentionMask and TypeIds)
            for (var i = 0; i < inputArray.Length; i++)
            {
                // Add each to the input Tenor result.
                // Set index and array value of each input Tensor.
                input[0, i] = inputArray[i];
            }
            return input;
        }
    }
    public class BertInput
    {
        public long[] InputIds { get; set; }
        public long[] AttentionMask { get; set; }
        public long[] TypeIds { get; set; }
    }

}