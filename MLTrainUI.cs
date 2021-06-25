using MaterialSkin.Controls;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMP304_AI_Assessment_2
{
    public partial class MLTrainUI : MaterialForm
    {
        string filePath;
        
        // automatically retreiving dataset file if it exists in the solution directory
        static string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
        string[] files = Directory.GetFiles(projectDirectory, "feature_vectors.csv", SearchOption.AllDirectories);
        // indicates if training is complete
        bool isComplete = false;

        public MLTrainUI()
        {
            InitializeComponent();

            // if dataset file is found it's set as the default path
            if (files.Length != 0)
            {
                filePath = files[0];
                filePathTextField.Text = filePath;
            }
        }

        // dialog for selecting dataset csv
        private void browseBtn_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                AddExtension = true,
                Multiselect = false,
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
                filePathTextField.Text = filePath;
            }
            progressText.Visible = false;
        }

        private async void trainBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                MessageBox.Show("Please select a file first.", "No file selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (System.IO.Path.GetExtension(filePath) != ".csv")
            {
                MessageBox.Show("Please select a csv file.", "Invalid File Format", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                progressText.Text = "";
                progressText.Visible = true;
                isComplete = false;
                trainBtn.Enabled = false;
                browseBtn.Enabled = false;

                // displaying progress text in the UI
                Task update = Task.Run(() => updateWaiting(ref isComplete));

                // training the model
                string result = await Task.Run(() => MLTrain.TrainAndEvaluate(filePath));

                isComplete = true;
                update.Wait();
                trainBtn.Enabled = true;
                browseBtn.Enabled = true;
                MessageBox.Show("Model evaluation results:" + result, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception err)
            {
                progressText.Visible = false;
                isComplete = true;
                trainBtn.Enabled = true;
                browseBtn.Enabled = true;

                MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // updates the progress text in the UI while training
        private void updateWaiting(ref bool isComplete)
        {
            int count = 0;
            StringBuilder waitingText = new StringBuilder();

            waitingText.Append("Training and evaluating");
            int baseLen = waitingText.Length;

            while (isComplete == false)
            {
                Thread.Sleep(800); // time between adding dots
                if (count >= 3) // number of dots
                {
                    waitingText.Remove(baseLen, count);
                    count = 0;
                }
                waitingText.Append(".");
                count++;

                BeginInvoke(new Action(() => { updateText(waitingText.ToString()); }));
            }
            BeginInvoke(new Action(() => { updateText("Training Complete"); }));
        }

        private void updateText(string txt)
        {
            progressText.Text = txt;
        }
    }
}
