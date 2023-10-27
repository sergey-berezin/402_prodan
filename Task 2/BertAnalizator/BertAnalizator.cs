using BERTTokenizers;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace BertAnalizator
{
    public class Berttokanalizator
    {
        private InferenceSession session;
        static Semaphore sessionSemaphore = new Semaphore(1, 1);
        static bool isDownloaded = false;
        public Berttokanalizator()
        { 
            this.session = session; 
        }
        public async Task Exist_Download_Model()
        {
            try
            {
                string web_source = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var model_path = Path.Combine(path, "bert-large-uncased-whole-word-masking-finetuned-squad.onnx");

                string downaloadedModelPath = model_path;

                if (!File.Exists(model_path))
                {
                    downaloadedModelPath = await Download_Model(model_path, web_source);
                }
                else
                {
                    isDownloaded = true;
                }
                this.session = new InferenceSession(downaloadedModelPath);
            }
            catch
            {
                throw;
            }
        }
        public static async Task<string> Download_Model(string model_path, string web_source_model)
        {
            try
            {
                var httpClient = new HttpClient();
                while (!isDownloaded)
                { 
                    var stream = await httpClient.GetStreamAsync(web_source_model);
                    var fileStream = new FileStream(model_path, FileMode.CreateNew);
                    await stream.CopyToAsync(fileStream);
                    isDownloaded = true;
                }
                if (!File.Exists(model_path))
                {
                    throw new Exception("cannot download model!");
                }
                return model_path;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<string> QA_Text_Model(string context_CTX, string context_QTX, CancellationToken token)
        {
            try
            {
                var FactoryTask = await Task.Factory.StartNew<string>(_ =>
                {
                    try
                    {
                        if (!isDownloaded)
                            throw new Exception("Model is not downloaded!");
                        if (token.IsCancellationRequested)
                            token.ThrowIfCancellationRequested();
                        var sentence = $"{{\"context_QTX\": {context_QTX}, \"context\": \"@CTX\"}}".Replace("@CTX", context_CTX);
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
                        sessionSemaphore.WaitOne();
                        var output = session.Run(input);
                        sessionSemaphore.Release();
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
                        if (token.IsCancellationRequested)
                            token.ThrowIfCancellationRequested();
                        return string.Join(" ", predictedTokens);//пробелы меж слов в ответе 
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }, token, TaskCreationOptions.LongRunning);
                return FactoryTask;
            }
            catch (Exception)
            {
                sessionSemaphore.Release();
                throw;
            }
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
    }
    public class BertInput
    {
        public long[]? InputIds { get; set; }
        public long[]? AttentionMask { get; set; }
        public long[]? TypeIds { get; set; }
    }
}

