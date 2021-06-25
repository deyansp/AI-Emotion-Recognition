using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMP304_AI_Assessment_2
{
    internal class Prediction
    {
        DataViewSchema modelSchema;
        ITransformer trainedModel;

        internal Dictionary<string, float> MakePrediction(string modelPath, List<FeatureExtraction.Features> featureData)
        {
            MLContext mlContext = new MLContext();
            trainedModel = mlContext.Model.Load(modelPath, out modelSchema);

            PredictionEngine<FaceData, EmotionPrediction> predictionEngine = mlContext.Model.CreatePredictionEngine<FaceData, EmotionPrediction>(trainedModel);
            
            // stores each label with its probability score
            Dictionary<string, float> scoreEntries = new Dictionary<string, float>();
            
            foreach (var face in featureData)
            {
                var prediction = predictionEngine.Predict(new FaceData()
                {
                    leftEyebrow = (float)face.leftEyebrow,
                    rightEyebrow = (float)face.rightEyebrow,
                    leftLip = (float)face.leftLip,
                    rightLip = (float)face.rightLip,
                    lipHeight = (float)face.lipHeight,
                    lipWidth = (float)face.lipWidth
                });
                
                scoreEntries = GetScoresWithLabelsSorted(predictionEngine.OutputSchema, "Score", prediction.Scores);
            }

            return scoreEntries;
        }

        // retireves the prediction probability score for each label
        private static Dictionary<string, float> GetScoresWithLabelsSorted(DataViewSchema schema, string name, float[] scores)
        {
            Dictionary<string, float> result = new Dictionary<string, float>();

            var column = schema.GetColumnOrNull(name);

            var slotNames = new VBuffer<ReadOnlyMemory<char>>();
            column.Value.GetSlotNames(ref slotNames);
            var names = new string[slotNames.Length];
            var num = 0;
            foreach (var denseValue in slotNames.DenseValues())
            {
                result.Add(denseValue.ToString(), scores[num++]);
            }

            return result.OrderByDescending(c => c.Value).ToDictionary(i => i.Key, i => i.Value * 100);
        }

    }
}
