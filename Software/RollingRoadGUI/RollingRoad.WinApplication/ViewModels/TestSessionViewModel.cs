﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using RollingRoad.Data;
using RollingRoad.Loggers;

namespace RollingRoad.WinApplication.ViewModels
{
    public class TestSessionViewModel : BindableBase
    {
        public enum TestSessionStatus
        {
            Stopped,
            Running
        }

        public DelegateCommand StartStopCommand { get; }
        public ICollection<string> TestSessionList { get; }

        public TestSessionStatus Status
        {
            get { return _status; }
            private set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public ILogger Logger { get; set; }
        public TorqueControlViewModel Control
        {
            get { return _control; }
            set { _control = value; }
        }

        public double CurrentTorque
        {
            get { return _currentTorque; }
            private set
            {
                if (Math.Abs(value - _currentTorque) < double.Epsilon)
                    return;

                _currentTorque = value;
                _control.Torque = value;
                OnPropertyChanged(nameof(CurrentTorque));
            }
        }


        public int SelectedTestSession { get; set; }
        public double DistanceOffset { get; set; }
        public double LastestDistance
        {
            set
            {
                if (Status != TestSessionStatus.Running)
                    return;

                DataList distanceDataList = TorqueDataset.First(x => x.Type.Name == "Distance");

                double closest = distanceDataList.OrderBy(x => Math.Abs(x - value + DistanceOffset)).First();
                int index = distanceDataList.IndexOf(closest);
                CurrentTorque = TorqueDataset.First(x => x.Type.Name == "Torque").ElementAt(index);
            }
        }
        
        private TorqueControlViewModel _control;
        private double _currentTorque;
        private TestSessionStatus _status;
        private Dataset TorqueDataset { get; set; }

        public TestSessionViewModel(TorqueControlViewModel control = null)
        {
            string folder = "TestSessions";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            TestSessionList = Directory.GetFiles(folder).Select(x => x.Substring(folder.Length + 1)).ToList();
            SelectedTestSession = 0;

            StartStopCommand = new DelegateCommand(StartStop);
            Control = control;
        }

        public void StartStop()
        {
            if (Status == TestSessionStatus.Running)
            {
                Status = TestSessionStatus.Stopped;
            }
            else
            {
                TorqueDataset =
                    CsvDataFile.LoadFromFile("TestSessions/" + TestSessionList.ElementAt(SelectedTestSession),
                        "eco shell marathon torque");

                //Must contain torque and distance info
                if (TorqueDataset.FirstOrDefault(x => x.Type.Name == "Torque") == null ||
                    TorqueDataset.FirstOrDefault(x => x.Type.Name == "Distance") == null)
                {
                    Logger?.WriteLine("Torque track does not contain torque and distance");
                    return;
                }

                Status = TestSessionStatus.Running;
                LastestDistance = DistanceOffset;
            }
        }
    }
}
