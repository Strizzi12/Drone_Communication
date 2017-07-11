using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;


namespace DroneControlCalculation
{
    /// <summary>
    /// This project is used to perform the control calculation for the drone.
    /// It reformats the incoming coordinates from the camera system and the unity app to a common format.
    /// Since the drone has 7 degrees of freedom, Multi-Threading is used.
    /// If one of those system send the coordinates faster, a buffer needs to be implemented. 
    /// This buffer should then calculates the mean of the coordinates between certain timestamps (This means that timestamps of incoming coordinates have to be saved.)
    /// To enable a higher performance the coordinates should be brought from float into integer.
    /// </summary>
    public partial class MainWindow : Window
    {
		/// <summary>
		/// Offers DB functionality
		/// </summary>
		private DBInterface dbInterface = null;
		private BackgroundWorker calculationWorker = null;

        public MainWindow()
        {
            InitializeComponent();
			initComponents();
			while(true)
			{
				dbInterface.QueryData();
			}
		}

		/// <summary>
		/// Inits should be done in here
		/// </summary>
		private void initComponents()
		{
			dbInterface = new DBInterface();
			calculationWorker = new BackgroundWorker();
			calculationWorker.DoWork += CalculusMaximus.startCalculation;

			calculationWorker.RunWorkerAsync(dbInterface);
		}



	}
}

