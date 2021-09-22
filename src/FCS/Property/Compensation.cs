using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FCS.Property
{
    /// <summary>
    /// 补偿值
    /// </summary>
    public struct Compensation
    {
        /// <summary>
        /// 参数集合
        /// </summary>
        public string[] MeasurementNames { get; set; }
        /// <summary>
        /// 补偿值集合
        /// </summary>
        public float[][] Coefficients { get; set; }

        public Compensation(string[] measurementNames, float[][] coefficients)
        {
            this.MeasurementNames = measurementNames;
            Coefficients = coefficients;
        }

        public Compensation(string str)
        {
            var temps = str.Split(',');
            if (temps.Length < 7 || !int.TryParse(temps[0], out int count))
            {
                MeasurementNames = null;
                Coefficients = null;
                return;
            }
            List<float> fs = new List<float>(count * count);
            for (int x = count + 1; x < temps.Length; x++)
            {
                if (!float.TryParse(temps[x], out float f))
                {
                    MeasurementNames = null;
                    Coefficients = null;
                    return;
                }
                fs.Add(f);
            }
            MeasurementNames = new string[count];
            for (int i = 0; i < count; i++) MeasurementNames[i] = temps[i + 1];
            Coefficients = new float[count][];
            for (int i = 0; i < count; i++)
            {
                Coefficients[i] = new float[count];
                for (int j = 0; j < count; j++) Coefficients[i][j] = fs[i * count + j];
            }
        }

        public override string ToString()
        {
            if (MeasurementNames == null || Coefficients == null || MeasurementNames.Length != Coefficients.Length || Coefficients.Contains(null) || Coefficients.Select(p => p.Length).Distinct().ToArray().Length != 1) return null;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(MeasurementNames.Length);
            foreach (var name in MeasurementNames)
            {
                stringBuilder.Append(",");
                stringBuilder.Append(name);
            }
            foreach (var item in Coefficients)
            {
                foreach (var temp in item)
                {
                    stringBuilder.Append(",");
                    stringBuilder.Append(temp.ToString());
                }
            }
            return stringBuilder.ToString();
        }
    }
}
