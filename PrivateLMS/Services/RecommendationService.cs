using PrivateLMS.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.AspNetCore.Http;

namespace PrivateLMS.Services
{
    public class RecommendationService
    {
        private InferenceSession _session;

        public RecommendationService(IHostEnvironment env)
        {
            // Load the ONNX model
            var modelPath = Path.Combine(env.ContentRootPath, "Models", "anfis_model.onnx");
            _session = new InferenceSession(modelPath);
        }

        public Task<float> GetRecommendationScoreAsync(float categoryMatch, float authorMatch, float languageMatch)
        {
            try
            {
                var inputData = new List<float> { categoryMatch, authorMatch, languageMatch };
                var tensor = new DenseTensor<float>(inputData.ToArray(), new[] { 1, 3 });

                var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input", tensor)
        };

                using var results = _session.Run(inputs);
                var output = results.First().AsTensor<float>();
                return Task.FromResult(output.First());
            }
            catch (Exception ex)
            {
                // Log error and return a default score
                Console.WriteLine($"ONNX inference failed: {ex.Message}");
                return Task.FromResult(0.0f);
            }
        }

    }
}