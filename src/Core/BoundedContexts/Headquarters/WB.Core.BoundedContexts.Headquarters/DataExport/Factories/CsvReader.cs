﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    public class CsvReader : ICsvReader
    {
        public IEnumerable<T> ReadAll<T>(Stream csvFileStream, string delimiter, bool hasHeaderRow = true)
        {
            csvFileStream.Seek(0, SeekOrigin.Begin);

            using var reader = new CsvHelper.CsvReader(new StreamReader(csvFileStream),
                GetConfiguration(delimiter, hasHeaderRow, true));
            reader.Read();
            reader.ReadHeader();

            while (reader.Read())
                yield return reader.GetRecord<T>();
        }

        public IEnumerable<string[]> ReadRowsWithHeader(Stream csvFileStream, string delimiter)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter,
                Mode = CsvMode.NoEscape,
            };
            using var csvReader = new CsvHelper.CsvReader(new StreamReader(csvFileStream), configuration);
            csvReader.Read();
            csvReader.ReadHeader();

            if (csvReader.HeaderRecord != null && csvReader.HeaderRecord.Length > 0)
            {
                yield return csvReader.HeaderRecord;
            }

            while (csvReader.Read())
                yield return csvReader.HeaderRecord.Select((x, index) =>
                    index < csvReader.Parser.Record.Length ? csvReader.GetField(x) : null).ToArray();
        }

        public IEnumerable<dynamic> GetRecords(Stream csvFileStream, string delimiter)
        {
            csvFileStream.Seek(0, SeekOrigin.Begin);

            using var reader = new CsvHelper.CsvReader(new StreamReader(csvFileStream), GetConfiguration(delimiter, true), true);

            foreach (var record in reader.GetRecords<dynamic>())
                yield return record;
        }

        public string[] ReadHeader(Stream csvFileStream, string delimiter)
        {
            csvFileStream.Seek(0, SeekOrigin.Begin);

            using (var parser = new CsvParser(new StreamReader(csvFileStream),
                GetConfiguration(delimiter, true), true))
            {
                return parser.Read()
                    ? parser.Record
                    : new string[] { };
            }
        }

        private static CsvConfiguration GetConfiguration(string delimiter, bool hasHeaderRow, bool ignoreCameCase = false)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                Delimiter = delimiter,
                HasHeaderRecord = hasHeaderRow,
                Mode = CsvMode.NoEscape,
            };

            if (ignoreCameCase)
                configuration.PrepareHeaderForMatch = (s) => s.Header.ToLower();

            return configuration;
        }
    }
}
