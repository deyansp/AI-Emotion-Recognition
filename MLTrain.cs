using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace CMP304_AI_Assessment_2
{
    public class FaceData
    {
        [LoadColumn(1)]
        public float leftEyebrow { get; set; }

        [LoadColumn(2)]
        public float rightEyebrow { get; set; }

        [LoadColumn(3)]
        public float leftLip { get; set; }

        [LoadColumn(4)]
        public float rightLip { get; set; }

        [LoadColumn(5)]
        public float lipHeight { get; set; }

        [LoadColumn(6)]
        public float lipWidth { get; set; }

        [LoadColumn(0)]
        public string label { get; set; }
    }

    public class EmotionPrediction
    {
        [ColumnName("PredictedLabel")]
        public string emotionPrediction { get; set; }

        [ColumnName("Score")]
        public float[] Scores { get; set; }
    }

    class MLTrain
    {
        internal static string TrainAndEvaluate(string path)
        {
            var mlContext = new MLContext();

            IDataView dataView = mlContext.Data.
                LoadFromTextFile<FaceData>(path, separatorChar: ',');
            
            // shuffling the data to add variance
            dataView = mlContext.Data.ShuffleRows(dataView);

            // split into training and test datasets
            DataOperationsCatalog.TrainTestData dataSplit = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            IDataView trainData = dataSplit.TrainSet;
            IDataView testData = dataSplit.TestSet;

            // define model's pipeline
            var featureVectorName = "Features";
            var labelColumnName = "Labels";

            var pipeline = mlContext.Transforms.Conversion.MapValueToKey(
                inputColumnName: "label",
                outputColumnName: labelColumnName)
                .Append(mlContext.Transforms.Concatenate(featureVectorName,
                "leftEyebrow",
                "rightEyebrow",
                "leftLip",
                "rightLip",
                "lipHeight",
                "lipWidth"
                ))
                .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                .AppendCacheCheckpoint(mlContext)
                .Append(mlContext.MulticlassClassification.Trainers.
                SdcaMaximumEntropy(labelColumnName, featureVectorName)).
                Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
            
            // fitting
            var model = pipeline.Fit(trainData);
            
            // saving model to file
            using (var fileStream = new FileStream("model.zip", FileMode.Create,
                FileAccess.Write, FileShare.Write)) { mlContext.Model.Save(model, dataView.Schema,
                    fileStream);}

            // evaluating trained model
            var testMetrics = mlContext.MulticlassClassification.Evaluate(model.Transform(testData), labelColumnName, "Score", "PredictedLabel", 0);
    
            string result = "\n MicroAccuracy: " + testMetrics.MicroAccuracy.ToString() +
            "\n"+ " MacroAccuracy: " + testMetrics.MacroAccuracy.ToString() +
            "\n" + " LogLoss: " + testMetrics.LogLoss.ToString() +
            "\n" + " LogLossReduction: " + testMetrics.LogLossReduction.ToString()
            + "\n";
            
            // saving metrics and confusion matrix to a txt file
            File.WriteAllText("modelMetrics " + DateTime.Now.ToString("dd-MM-yy HH-mm-ss") + ".txt", result + testMetrics.ConfusionMatrix.GetFormattedConfusionTable());

            return result;
        }
    }
}
