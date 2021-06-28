# Overview
This project aimed to create a machine learning model that can identify seven facial expressions of emotion: neutral, joy, anger, sadness, disgust, fear, and surprise. The tool was created in C# paired with a Windows Forms user interface for easy navigation. The program can be used to extract facial features from images and create datasets, train and validate models on datasets of a user's choosing, and to make predictions with trained models on new images. 

When starting the application, the user is presented with a task selection form with three options – 'Create Dataset', 'Model Training' and 'Predict Emotions'.

![Main Screen](https://raw.githubusercontent.com/deyansp/AI-Emotion-Recognition/main/task-selection-UI.PNG)

When selecting 'Predict Emotions’ the user is presented with the following form, where they select the location of a trained ML model file to use for predictions and the image which they would like the model to interpret.

![Emotion Prediction Screen](https://raw.githubusercontent.com/deyansp/AI-Emotion-Recognition/main/emotion-prediction-UI.PNG) 

This project provided a good baseline for creating emotion classifiers in the future. The software can be quite versatile as it can easily be used to create different datasets and machine learning models.

# Performance evaluation
A confusion matrix was used to evaluate how well the ML model recognises emotions across all categories:

![Confusion Matrix](https://raw.githubusercontent.com/deyansp/AI-Emotion-Recognition/main/confusion-matrix.PNG) 

Overall, the confusion matrix is good with class predictions on the diagonal line. This model is best at recognizing feelings of anger, joy and sadness with joy having a precision of 80% and recall of 74%.
