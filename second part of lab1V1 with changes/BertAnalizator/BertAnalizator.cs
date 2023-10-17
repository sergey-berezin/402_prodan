using BERTTokenizers;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace BertAnalizator
{
    public class Berttokanalizator
    {
        private InferenceSession session;
        static Semaphore semaphore = new Semaphore(1,1);
      //CancellationToken cancelToken;

        private Berttokanalizator(InferenceSession inferenceSession)
        {
            this.session = inferenceSession;
        }
        public static async Task<Berttokanalizator> Exist_Download_Model()
        {
                //спрятал скачивание модели
                string web_source = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
                String path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var modelPath = Path.Combine(path, "bert-large-uncased-whole-word-masking-finetuned-squad.onnx");
                string downaloadedModelPath = modelPath;

                if (!File.Exists(modelPath))
                {
                    downaloadedModelPath = await Download_Model(modelPath, web_source);
                }
                return new Berttokanalizator(new InferenceSession(downaloadedModelPath));
        }
        public static async Task<string> Download_Model(string model_path, string web_source_model)
        {
            var httpClient = new HttpClient();
            bool Downloaded = false;
            while (!Downloaded)
            {
             
                using var stream = await httpClient.GetStreamAsync(web_source_model);
                using var fileStream = new FileStream(model_path, FileMode.CreateNew);
                await stream.CopyToAsync(fileStream);
                Downloaded = true;
            }
            if (!File.Exists(model_path))
            {
                throw new Exception("cannot download model!");
            }
            return model_path;
        }
        public async Task<string> QA_text_Model(string context_CTX, string question_QTX, CancellationToken token)
        {
            var FactoryTask = await Task.Factory.StartNew<string>(_ =>
            {
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();

                // var sentence = "{\"question\": \"@QTX\", " +
                // "\"context\": \"@CTX\"}".Replace("@QTX", question_QTX).Replace("CTX", context_CTX);
                var sentence = $"{{\"question\": {question_QTX}, \"context\": \"@CTX\"}}".Replace("@CTX", context_CTX);
                var tokenizer = new BertUncasedLargeTokenizer();
                var tokens = tokenizer.Tokenize(sentence);
                var encoded = tokenizer.Encode(tokens.Count(), sentence);
               
                var bertInput = new BertInput()
                {
                    InputIds = encoded.Select(t => t.InputIds).ToArray(),
                    AttentionMask = encoded.Select(t => t.AttentionMask).ToArray(),
                    TypeIds = encoded.Select(t => t.TokenTypeIds).ToArray(),
                };

                var input_ids = ConvertToTensor(bertInput.InputIds, bertInput.InputIds.Length);
                var attention_mask = ConvertToTensor(bertInput.AttentionMask, bertInput.InputIds.Length);
                var token_type_ids = ConvertToTensor(bertInput.TypeIds, bertInput.InputIds.Length);

                var input = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input_ids", input_ids),
                                                        NamedOnnxValue.CreateFromTensor("input_mask", attention_mask),
                                                        NamedOnnxValue.CreateFromTensor("segment_ids", token_type_ids) };

                //var session = new InferenceSession(modelPath);
                //using var output = session.Run(input);

                //защита от конкурентного выполнения
                semaphore.WaitOne();
                var output = session.Run(input);
                semaphore.Release();

                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();

                List<float> startLogits = ((IEnumerable<float>)output.ToList().First().Value).ToList();
                List<float> endLogits = ((IEnumerable<float>)output.ToList().Last().Value).ToList();

                var startIndex = startLogits.ToList().IndexOf(startLogits.Max());
                var endIndex = endLogits.ToList().IndexOf(endLogits.Max());

                var predictedTokens = tokens
                            .Skip(startIndex)
                            .Take(endIndex + 1 - startIndex)
                            .Select(o => tokenizer.IdToToken((int)o.VocabularyIndex))
                            .ToList();

                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();

                return String.Join(" ", predictedTokens);
                

            }, token, TaskCreationOptions.LongRunning);
            return FactoryTask;
        }
        public static Tensor<long> ConvertToTensor(long[] inputArray, int inputDimension)
        {
            Tensor<long> input = new DenseTensor<long>(new[] { 1, inputDimension });

            for (var i = 0; i < inputArray.Length; i++)
            {
                input[0, i] = inputArray[i];
            }
            return input;
        }
        public class BertInput
        {
            public long[]? InputIds { get; set; }
            public long[]? AttentionMask { get; set; }
            public long[]? TypeIds { get; set; }
        }
    }
}
