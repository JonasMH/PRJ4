﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;


namespace RollingGoal
{
    /// <summary>
    /// 
    /// </summary>
    public class CsvDataSource : IDataSource
    {
        private readonly List<DataList> _data = new List<DataList>(); 

        /// <summary>
        /// No public ctor, must be created by <see cref="LoadFromFile"/>
        /// </summary>
        private CsvDataSource()
        {
            
        }

        /// <summary>
        /// Name of the file
        /// </summary>
        public string Name { get; private set; } = "Unknown";

        /// <summary>
        /// Description written in the file
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Path of the file opened
        /// </summary>
        public string FilePath { get; private set; } = "Unknown";

        /// <summary>
        /// Get datalist by name
        /// </summary>
        /// <param name="name">Name of the data (Ex "Time", "Torque")</param>
        /// <returns></returns>
        public DataList GetDataList(string name)
        {
            foreach (var dataList in _data)
            {
                if (dataList.Name == name)
                    return dataList;
            }

            throw new ArgumentException("Name not found");
        }

        /// <summary>
        /// Returns all data known
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<DataList> GetAllData()
        {
            return _data.AsReadOnly();
        }

        /// <summary>
        /// Save datasource to file
        /// </summary>
        /// <param name="path"></param>
        public void SaveToFile(string path)
        {
            
        }

        public static CsvDataSource LoadFromStream(StreamReader reader)
        {

            //Make sure stream is not empty
            if (reader.EndOfStream)
                throw new EndOfStreamException("File is empty");

            //Names/types
            var line = reader.ReadLine();
            string[] names = line.Split(';');

            //We need at least to data lists to make a graph 
            if (names.Length < 3)
                throw new Exception("Header is to short");

            //Make sure header name matches (basic check to make not all files are loaded)
            if (names[0].ToLower() != "shell eco marathon")
                throw new Exception("Invalid header: " + names[0].ToLower());

            //Units
            line = reader.ReadLine();
            string[] units = line.Split(';');

            //Make sure our unit length matches our amount of names
            if (units.Length != names.Length)
                throw new Exception("Unit lenght does not match names");


            CsvDataSource data = new CsvDataSource();

            //Create a new list for each of the units
            for (int i = 1; i < names.Length; i++)
            {
                data._data.Add(new DataList(names[i], units[i]));
            }

            //We allready read two lines
            int curLine = 2;

            //Read all data
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                string[] readData = line.Split(';');

                //No data cell is allowed to be null, so our data must be at least as long as our amount of datatypes
                if (readData.Length < names.Length)
                    throw new Exception("Error at line " + curLine);

                //Parse values
                for (int i = 1; i < readData.Length; i++)
                {
                    data._data[i - 1].AddData(double.Parse(readData[i], CultureInfo.InvariantCulture));
                }

                //Progress one line
                curLine++;
            }

            //Setup meta info
            data.Description = units[0];

            return data;
        }

        /// <exception cref="FileLoadException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <param name="path">Path of the csv file to load</param>
        public static CsvDataSource LoadFromFile(string path)
        {
            CsvDataSource data;

            if (!File.Exists(path))
                throw new FileNotFoundException("File not found", path);

            using (FileStream fileStream = File.OpenRead(path))
            {
                StreamReader reader = new StreamReader(fileStream);

                data = LoadFromStream(reader);
                data.Name = Path.GetFileName(path);
                data.FilePath = path;
            }

            return data;
        }
    }
}
