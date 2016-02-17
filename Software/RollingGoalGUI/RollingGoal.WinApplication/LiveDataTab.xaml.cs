﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Win32;

namespace RollingGoal.WinApplication
{
    public struct LineStructure
    {
        public ObservableDataSource<Point> RawData;
        public DataList Data;
        public LiveDataDisplay Label;

        public string Name => Data.Name;
        public string Unit => Data.Unit;

        public LineStructure(string name, string unit)
        {
            RawData = new ObservableDataSource<Point>();
            Data = new DataList(name, unit);
            Label = null;
        }
    }

    /// <summary>
    /// Interaction logic for LiveDataTab.xaml
    /// </summary>
    public partial class LiveDataTab
    {
        private ILiveDataSource _currentSource;
        private List<LineStructure?> _data;
        public bool HasBeenSaved { get; private set; } = true;

        public LiveDataTab()
        {
            InitializeComponent();
            ClearChart();
            SelectSource(new LiveDataEmulator(CsvDataSource.LoadFromFile("3TypesOfData17Rows.csv")));
        }

        public void SelectSource(ILiveDataSource source)
        {
            _data = new List<LineStructure?>();
            _currentSource = source;
            _currentSource.OnNextReadValue += ThreadMover;
        }

        private LineStructure CreateNewLine(DataEntry entry)
        {
            LineStructure lineStuct = new LineStructure(entry.Name, entry.Unit);
            string dataTitle = $"{entry.Name} ({entry.Unit})";

            if (entry.Name != "Time")
            {
                lineStuct.RawData.SetXYMapping(p => p);

                LiveDataChart.AddLineGraph(lineStuct.RawData, 2, dataTitle);
            }

            //Live values
            LiveDataDisplay lab = new LiveDataDisplay
            {
                TitleTextBlock = {Text = dataTitle},
                ValueTextBlock = {Text = entry.Value.ToString()}
            };


            LiveDataStackPanel.Children.Add(lab);

            lineStuct.Label = lab;

            return lineStuct;
        }

        private bool TryGetLineStructure(DataEntry entry, out LineStructure? value)
        {
            try
            {
                value = _data.FirstOrDefault(x => x != null && x.Value.Name == entry.Name);
                return value != null;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }


        /// <summary>
        /// The <see cref="IncommingData"/> method need to be run from the main/gui-thread, therefor we create a translator
        /// </summary>
        /// <param name="entries"></param>
        private void ThreadMover(IReadOnlyList<DataEntry> entries)
        {
            try
            {
                Dispatcher?.Invoke(() => IncommingData(entries));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Handles incomming data and creates new lines if no line for the data type is present
        /// </summary>
        /// <param name="entries"></param>
        private void IncommingData(IReadOnlyList<DataEntry> entries)
        {
            HasBeenSaved = false;

            double time = entries.First(x => x.Name == "Time").Value;

            foreach (DataEntry entry in entries)
            {
                LineStructure? lineStruct;

                //Try to get structure if exists, else create new
                if (!TryGetLineStructure(entry, out lineStruct))
                {
                    lineStruct = CreateNewLine(entry);
                    _data.Add(lineStruct);
                }

                //The above code ensures that linestruct is not null
                // ReSharper disable once PossibleInvalidOperationException
                lineStruct.Value.Data.AddData(entry.Value);

                if(entry.Name != "Time")
                    lineStruct.Value.RawData.AppendAsync(Dispatcher, new Point(time, entry.Value));

                lineStruct.Value.Label.ValueTextBlock.Text = entry.Value.ToString();
            }
        }

        /// <summary>
        /// When the user request a save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFileSaveDataset_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentData();
        }

        private bool SaveCurrentData()
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv"
            };


            if (dlg.ShowDialog() == true)
            {
                //Open file her
            }

            return false;
        }

        private void ClearChart()
        {
            _data = new List<LineStructure?>();
            LiveDataChart.Children.RemoveAll(typeof(LineGraph));
        }

        private void LiveDataClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (!HasBeenSaved)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to save changes?", "Unsaved changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        SaveCurrentData();
                        break;
                    case MessageBoxResult.No:
                        ClearChart();
                        break;
                }
            }
        }

        private void LiveDataStartStopButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
