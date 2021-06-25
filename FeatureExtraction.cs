using DlibDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using Dlib = DlibDotNet.Dlib;

namespace CMP304_AI_Assessment_2
{
    internal class FeatureExtraction
    {
        // header for csv file
        readonly string header = "label,leftEyebrow,rightEyebrow,leftLip,rightLip,lipHeight,lipWidth\n";
        StreamWriter file;
        FrontalFaceDetector fd = Dlib.GetFrontalFaceDetector();
        ShapePredictor sp = ShapePredictor.Deserialize("shape_predictor_68_face_landmarks.dat");

        internal struct Features
        {
            internal double leftEyebrow;
            internal double rightEyebrow;
            internal double leftLip;
            internal double rightLip;
            internal double lipWidth;
            internal double lipHeight;
        }

        double constructFeature(ref FullObjectDetection shape, int stationary, int landmark1, int landmark2, int landmark3, int landmark4)
        {
            // substracting by 1 because indexes start at 0
            Point stationaryPoint = shape.GetPart((uint)stationary - 1);

            Point distance1 = shape.GetPart((uint)landmark1 - 1) - stationaryPoint; // distance used for normalization
            Point distance2 = shape.GetPart((uint)landmark2 - 1) - stationaryPoint;
            Point distance3 = shape.GetPart((uint)landmark3 - 1) - stationaryPoint;
            Point distance4 = shape.GetPart((uint)landmark4 - 1) - stationaryPoint;

            // normalizing the distance
            double normalized1 = distance1.Length / distance1.Length;
            double normalized2 = distance2.Length / distance1.Length;
            double normalized3 = distance3.Length / distance1.Length;
            double normalized4 = distance4.Length / distance1.Length;

            // if it is the lips we only use 3 landmarks
            if (stationary != 34)
            {
                return normalized1 + normalized2 + normalized3 + normalized4;
            }
            else
            {
                return normalized2 + normalized3 + normalized4;
            }
        }

        double constructLipWidthOrHeight(ref FullObjectDetection shape, bool isWidth)
        {
            Point normalizationDist = shape.GetPart((uint)34 - 1) - shape.GetPart((uint)52 - 1);
            Point distance;
            if (isWidth)
            {
                distance = shape.GetPart((uint)49 - 1) - shape.GetPart((uint)55 - 1);
            }
            else // lip height
            {
                distance = shape.GetPart((uint)52 - 1) - shape.GetPart((uint)58 - 1);
            }

            return distance.Length / normalizationDist.Length;
        }

        string getLabel(string filename)
        {
            if (filename.Contains("neutral"))
                return "neutral";
            else if (filename.Contains("joy"))
                return "joy";
            else if (filename.Contains("surprise"))
                return "surprise";
            else if (filename.Contains("anger"))
                return "anger";
            else if (filename.Contains("fear"))
                return "fear";
            else if (filename.Contains("disgust"))
                return "disgust";
            else if (filename.Contains("sadness"))
                return "sadness";
            else
                throw new InvalidDataException("Unable to determine label from image file name. " +
                    "Please make sure the emotion label is in the file name and try again.");
        }

        internal List<Features> ExtractFeatures(FileInfo imgFileInfo)
        {
            var listOfFeatures = new List<Features>();
            // load input image
            using (var img = Dlib.LoadImage<RgbPixel>(imgFileInfo.FullName))
            {

                // find all faces in the image
                var faces = fd.Operator(img);

                // for each face draw over the facial landmarks
                foreach (var face in faces)
                {
                    // find the landmark points for this face
                    FullObjectDetection shape = sp.Detect(img, face);

                    Features features = new Features
                    {
                        leftEyebrow = constructFeature(ref shape, 40, 22, 21, 20, 19),
                        rightEyebrow = constructFeature(ref shape, 43, 23, 24, 25, 26),
                        leftLip = constructFeature(ref shape, 34, 52, 51, 50, 49),
                        rightLip = constructFeature(ref shape, 34, 52, 53, 54, 55),
                        lipWidth = constructLipWidthOrHeight(ref shape, true),
                        lipHeight = constructLipWidthOrHeight(ref shape, false)
                    };

                    listOfFeatures.Add(features);
                }
            }
            return listOfFeatures;
        }

        void SaveFeaturesToFile(FileInfo imgFileInfo, List<Features> listOffeatures)
        {
            foreach (var face in listOffeatures)
            {
                // getting emotion label from file name
                string label = getLabel(imgFileInfo.Name);

                var str = label + "," + face.leftEyebrow + "," + face.rightEyebrow + "," + face.leftLip +
                    "," + face.rightLip + "," + face.lipHeight + "," + face.lipWidth;

                file.WriteLine(str);
                file.Flush();
            }
        }

        internal void CreateDataset(string folder, IProgress<int> progress)
        {
            DirectoryInfo imgDir = new DirectoryInfo(folder);

            // storing all image file paths in a list
            List<FileInfo> imageFilesInfo = new List<FileInfo>(imgDir.EnumerateFiles("*.jpg", SearchOption.AllDirectories));
            imageFilesInfo.AddRange(imgDir.EnumerateFiles("*.png", SearchOption.AllDirectories));

            int totalImages = imageFilesInfo.Count;

            if (totalImages == 0)
                throw new FileNotFoundException("Folder does not contain any .jpg or .png files.");
            
            // creating csv file for feature vectors
            File.WriteAllText(@"feature_vectors.csv", header);
            file = new StreamWriter(@"feature_vectors.csv", true);

            int processedImages = 0;

            foreach (var image in imageFilesInfo)
            {
                SaveFeaturesToFile(image, ExtractFeatures(image));
                processedImages++;

                // reporting progress back to the UI
                if (progress != null)
                    progress.Report((processedImages * 100) / totalImages);
            }
            file.Close();
            file.Dispose();
        }
    }
}