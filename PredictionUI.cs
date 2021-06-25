using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMP304_AI_Assessment_2
{
    public partial class PredictionUI : MaterialForm
    {
        string imgFilePath;
        string modelFilePath;

        // automatically retreiving model file if it exists in the solution directory
        static string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
        string[] files = Directory.GetFiles(projectDirectory, "model.zip", SearchOption.AllDirectories);

        public PredictionUI()
        {
            InitializeComponent();

            // if model file is found it's set as the default path
            if (files.Length != 0)
            {
                modelFilePath = files[0];
                modelFilePathField.Text = modelFilePath;
            }
        }
        // dialog for image selection
        private void imgBrowseBtn_Click(object sender, EventArgs e)
        {
            var imgFileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                AddExtension = true,
                Multiselect = false,
                Filter = "Image Files|*.jpg;*.jpeg;*.png"
            };

            if (imgFileDialog.ShowDialog() == DialogResult.OK)
            {
                imgFilePath = imgFileDialog.FileName;
                imgFilePathField.Text = imgFilePath;
            }
        }
        // dialog for model file selection
        private void modelBrowseBtn_Click(object sender, EventArgs e)
        {
            var modelFileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                AddExtension = true,
                Multiselect = false,
                Filter = "ZIP Folder (.zip)|*.zip"
            };

            if (modelFileDialog.ShowDialog() == DialogResult.OK)
            {
                modelFilePath = modelFileDialog.FileName;
                modelFilePathField.Text = modelFilePath;
            }
        }

        private async void predictBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(modelFilePath))
            {
                MessageBox.Show("Please select a model", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(imgFilePath))
            {
                MessageBox.Show("Please select an image", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                resultsBox.Text = "Processing...";

                // loading image and extracting facial features
                FileInfo image = new FileInfo(imgFilePath);
                var featureExtract = new FeatureExtraction();
                List<FeatureExtraction.Features> features = await Task.Run(() => featureExtract.ExtractFeatures(image));

                // making a prediction
                var prediction = new Prediction();
                Dictionary<string, float> results = await Task.Run(() => prediction.MakePrediction(modelFilePath, features));

                resultsBox.Text = "";

                // outputting result
                foreach (var result in results)
                    resultsBox.AppendText(result.Key + " (" + result.Value.ToString("0.00") + "%)\n");
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
