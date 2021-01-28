namespace FCS.Property
{
    /// <summary>
    /// 推荐的可视化比例 PnD
    /// </summary>
    public struct RecommendsVisualizationScale
    {
        public RecommendsVisualizationScaleType Type { get; set; }
        public double F1 { get; set; }
        public double F2 { get; set; }

        public RecommendsVisualizationScale(RecommendsVisualizationScaleType type, double f1, double f2)
        {
            this.Type = type;
            this.F1 = f1;
            this.F2 = f2;
        }

        public RecommendsVisualizationScale(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                this.Type = RecommendsVisualizationScaleType.Unknown;
                this.F1 = this.F2 = 0d;
                return;
            }
            var spirts = str.Split(',');
            if (spirts.Length != 3)
            {
                this.Type = RecommendsVisualizationScaleType.Unknown;
                this.F1 = this.F2 = 0d;
                return;
            }
            switch (spirts[0].ToUpper())
            {
                case "LINEAR":
                    this.Type = RecommendsVisualizationScaleType.Linear;
                    break;
                case "LOGARITHMIC":
                    this.Type = RecommendsVisualizationScaleType.Logarithmic;
                    break;
                default:
                    this.Type = RecommendsVisualizationScaleType.Unknown;
                    break;
            }
            if (double.TryParse(spirts[1], out double f1)) this.F1 = f1;
            else this.F1 = 0d;
            if (double.TryParse(spirts[2], out double f2)) this.F2 = f2;
            else this.F2 = 0d;
        }

        public override string ToString()
        {
            string typestring;
            switch (Type)
            {
                case RecommendsVisualizationScaleType.Linear:
                    typestring = "LINEAR";
                    break;
                case RecommendsVisualizationScaleType.Logarithmic:
                    typestring = "LOGARITHMIC";
                    break;
                default:
                    return string.Empty;
            }
            return string.Concat(typestring, ",", F1, ",", F2);
        }

    }

    public enum RecommendsVisualizationScaleType
    {
        Unknown = 0,
        Linear = 1,
        Logarithmic = 2
    }
}
