using MaterialSkin.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMP304_AI_Assessment_2
{
    public partial class FeatureExtractionUI : MaterialForm
    {
        string folderPath;
        public FeatureExtractionUI()
        {
            InitializeComponent();
        }
        
        // dialog for selecting images directory
        private void browseBtn_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    folderPath = folderDialog.SelectedPath;
                    filePathTextField.Text = folderPath;
                }
            }
        }

        private async void extractBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                MessageBox.Show("Please select a folder", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                progressText.Visible = true;
                extractBtn.Enabled = false;
                browseBtn.Enabled = false;

                // updates the UI with progress percentage
                var prog = new Progress<int>(percent =>
                {
                    progressText.Text = "Processing..." + percent + "%";
                });

                // extracting features and saving to csv
                var extraction = new FeatureExtraction();
                await Task.Run(() => extraction.CreateDataset(folderPath, prog));
                MessageBox.Show("Feature vector sucessfully created.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                progressText.Visible = false;
                extractBtn.Enabled = true;
                browseBtn.Enabled = true;
            }
            catch (FileNotFoundException error)
            {
                MessageBox.Show(error.Message , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (InvalidDataException error)
            {
                MessageBox.Show(error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
