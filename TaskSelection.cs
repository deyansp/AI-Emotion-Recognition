using MaterialSkin.Controls;
using System;
using System.Linq;
using System.Windows.Forms;

namespace CMP304_AI_Assessment_2
{
    // opens the form window for the selected task after checking if it's not already open
    public partial class TaskSelection : MaterialForm
    {
        public TaskSelection()
        {
            InitializeComponent();
        }

        private void featureBtn_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<FeatureExtractionUI>().Count() >= 1)
                return;

            var form = new FeatureExtractionUI();
            form.Show();
        }

        private void trainBtn_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<MLTrainUI>().Count() >= 1)
                return;

            var form = new MLTrainUI();
            form.Show();
        }

        private void predictBtn_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<PredictionUI>().Count() >= 1)
                return;

            var form = new PredictionUI();
            form.Show();
        }
    }
}
