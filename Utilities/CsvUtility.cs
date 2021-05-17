using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace Geotab.CustomerOnboardngStarterKit.Utilities
{
    /// <summary>
    /// Contains methods to assist in working with CSV files.
    /// </summary>
    public static class CsvUtility
    {
        /// <summary>
        /// Reads a CSV file, creating an object of the specified type for each row, and returns the objects in a <see cref="List"/>.
        /// </summary>
        /// <param name="stream">The stream to read data from the subject CSV file.</param>
        /// <param name="map"></param>
        /// <param name="startRowOffset"></param>
        /// <param name="startColumnOffset"></param>
        /// <param name="endRowOffset"></param>
        /// <param name="delimeter"></param>
        /// <param name="commentLinePrefix"></param>
        /// <typeparam name="T">The type of object to be used to represent rows in the CSV file.</typeparam>
        /// <returns>A <see cref="List"/> of objects of the specifed type.</returns>
        public static List<T> CsvToList<T>(this Stream stream, Dictionary<string, string> map = null, int startRowOffset = 0, int startColumnOffset = 0, int endRowOffset = 0, char delimeter = ',', char commentLinePrefix = '#') where T : new()
        {
            // DateTime Conversion
            var convertDateTime = new Func<double, DateTime>(csvDate =>
            {
                if (csvDate < 1)
                    throw new ArgumentException("CSV dates cannot be smaller than 0.");
                var dateOfReference = new DateTime(1900, 1, 1);
                if (csvDate > 60d)
                    csvDate -= 2;
                else
                    csvDate--;
                return dateOfReference.AddDays(csvDate);
            });
            using (var sr = new StreamReader(stream))
            {
                var data = sr.ReadToEnd();
                var lines = data.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Skip(startRowOffset);
                // Remove comment lines.
                lines = lines.Where(line => line.StartsWith(commentLinePrefix) == false);
                var props = typeof(T).GetProperties()
                .Select(prop =>
                {
                    var displayAttribute = (DisplayAttribute)prop.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
                    return new
                    {
                        prop.Name,
                        DisplayName = displayAttribute?.Name ?? prop.Name,
                        Order = displayAttribute == null || !displayAttribute.GetOrder().HasValue ? 999 : displayAttribute.Order,
                        PropertyInfo = prop,
                        prop.PropertyType,
                        HasDisplayName = displayAttribute != null
                    };
                })
                .Where(prop => !string.IsNullOrWhiteSpace(prop.DisplayName))
                .ToList();
                var retList = new List<T>();
                var columns = new List<CsvMap>();
                var startCol = startColumnOffset;
                var startRow = startRowOffset;
                var headerRow = lines.ElementAt(startRow).Split(delimeter);
                var endCol = headerRow.Length;
                var endRow = lines.Count();
                // Assume first row has column names.
                for (int col = startCol; col < endCol; col++)
                {
                    var cellValue = (lines.ElementAt(startRow).Split(delimeter)[col] ?? string.Empty).ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(cellValue))
                    {
                        columns.Add(new CsvMap()
                        {
                            Name = cellValue,
                            MappedTo = map == null || map.Count == 0 ?
                        cellValue :
                        map.ContainsKey(cellValue) ? map[cellValue] : string.Empty,
                            Index = col
                        });
                    }
                }
                // Now iterate over all the rows.
                for (int rowIndex = startRow + 1; rowIndex < endRow; rowIndex++)
                {
                    var item = new T();
                    columns.ForEach(column =>
                    {
                        var value = lines.ElementAt(rowIndex).Split(delimeter)[column.Index];
                        var valueStr = value == null ? string.Empty : value.ToString().Trim();
                        var prop = string.IsNullOrWhiteSpace(column.MappedTo) ?
                        null :
                        props.FirstOrDefault(p => p.Name.Trim().Contains(column.MappedTo));
                        // Handle mapping by DisplayName
                        if (prop == null && !string.IsNullOrWhiteSpace(column.MappedTo))
                        {
                            prop = props.FirstOrDefault(p => p.HasDisplayName && p.DisplayName.Trim().Contains(column.MappedTo));
                        }
                        // Excel stores all numbers as doubles, but we're relying on the object's property types.
                        if (prop != null)
                        {
                            var propertyType = prop.PropertyType;
                            object parsedValue = null;
                            if (propertyType == typeof(int?) || propertyType == typeof(int))
                            {
                                if (!int.TryParse(valueStr, out int val))
                                {
                                    val = default;
                                }
                                parsedValue = val;
                            }
                            else if (propertyType == typeof(short?) || propertyType == typeof(short))
                            {
                                if (!short.TryParse(valueStr, out short val))
                                    val = default;
                                parsedValue = val;
                            }
                            else if (propertyType == typeof(long?) || propertyType == typeof(long))
                            {
                                if (!long.TryParse(valueStr, out long val))
                                    val = default;
                                parsedValue = val;
                            }
                            else if (propertyType == typeof(decimal?) || propertyType == typeof(decimal))
                            {
                                if (!decimal.TryParse(valueStr, out decimal val))
                                    val = default;
                                parsedValue = val;
                            }
                            else if (propertyType == typeof(double?) || propertyType == typeof(double))
                            {
                                if (!double.TryParse(valueStr, out double val))
                                    val = default;
                                parsedValue = val;
                            }
                            else if (propertyType == typeof(DateTime?) || propertyType == typeof(DateTime))
                            {
                                try
                                {
                                    if (DateTime.TryParse(value, out DateTime output))
                                    {
                                        parsedValue = output;
                                    }
                                    else
                                    {
                                        parsedValue = convertDateTime(Double.Parse(value));
                                    }
                                }
                                catch
                                {
                                    if (propertyType == typeof(DateTime))
                                    {
                                        parsedValue = DateTime.MinValue;
                                    }
                                }
                            }
                            else if (propertyType.IsEnum)
                            {
                                try
                                {
                                    parsedValue = Enum.ToObject(propertyType, int.Parse(valueStr));
                                }
                                catch
                                {
                                    parsedValue = Enum.ToObject(propertyType, 0);
                                }
                            }
                            else if (propertyType == typeof(string))
                            {
                                parsedValue = valueStr;
                            }
                            else
                            {
                                try
                                {
                                    parsedValue = Convert.ChangeType(value, propertyType);
                                }
                                catch
                                {
                                    parsedValue = valueStr;
                                }
                            }
                            try
                            {
                                prop.PropertyInfo.SetValue(item, parsedValue);
                            }
                            catch (Exception)
                            {
                                // Indicate parsing error on row?
                            }
                        }
                    });
                    retList.Add(item);
                }
                return retList;
            }
        }
    }

    /// <summary>
    /// Used to map columns in a CSV file with fields in a class designed to represent CSV rows. 
    /// </summary>
    public class CsvMap
    {
        public string Name { get; set; }
        public string MappedTo { get; set; }
        public int Index { get; set; }
    }    
}
