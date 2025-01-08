The Patient Monitor project is an academic software application designed to simulate and manage physiological parameters, allowing users to monitor and analyze patient data in real-time. The project serves as a practical introduction to object-oriented programming concepts, software design patterns, signal processing, and UI development.

The application focuses on simulating physiological signals such as ECG, EEG, EMG, and Respiration, providing a platform for students to learn about real-time data visualization, patient management, and signal analysis through a combination of backend logic and user interface.

The project is composed of the following main components:

* Physiological Parameters: The core of the application includes classes such as ECG, EEG, EMG, and Respiration, which simulate physiological signals. Each parameter class calculates samples based on specific algorithms, such as cosine waves or linear waveforms, and includes alarm thresholds for monitoring critical values.

* Patient Management: The application supports the creation and management of patient records. Patients can be categorized as ambulatory or stationary, with additional attributes such as age, date of study, clinic, and room number for stationary patients.

* Signal Processing: The application implements a Fast Fourier Transform (FFT) to analyze the frequency components of physiological signals. Users can perform FFT analysis at any time, visualizing the frequency spectrum of the active parameter.

* User Interface: The UI, built with XAML and C#, provides a dynamic and interactive interface for users. It includes functionalities such as adding patients, selecting parameters, viewing signal charts, and navigating MRI images. Alarms are visually and programmatically displayed when parameters exceed predefined thresholds.

* Database: Patient data is managed using a database that supports adding, sorting, saving, and loading patient records. This ensures that user interactions are persistent and can be reused across sessions.

* Image Management: The application includes a module to manage and display MRI images associated with patients. Users can load image sets, navigate between images, and set the maximum number of images to display.

* Testing and Comparisons: The project emphasizes testing by incorporating classes such as PatientComparer, which provides functionality to compare and sort patients based on attributes like age, name, or clinic.
