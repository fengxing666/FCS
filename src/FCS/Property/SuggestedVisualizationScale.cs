namespace FCS.Property
{
    /// <summary>
    /// 推荐的可视化比例 PnD
    /// </summary>
    public struct SuggestedVisualizationScale
    {
        public SuggestedVisualizationScaleType Type { get; set; }
        public double F1 { get; set; }
        public double F2 { get; set; }

        public SuggestedVisualizationScale(SuggestedVisualizationScaleType type, double f1, double f2)
        {
            this.Type = type;
            this.F1 = f1;
            this.F2 = f2;
        }

        public SuggestedVisualizationScale(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                this.Type = SuggestedVisualizationScaleType.Unknown;
                this.F1 = this.F2 = 0d;
                return;
            }
            var spirts = str.Split(',');
            if (spirts.Length != 3)
            {
                this.Type = SuggestedVisualizationScaleType.Unknown;
                this.F1 = this.F2 = 0d;
                return;
            }
            switch (spirts[0].ToUpper())
            {
                case "LINEAR":
                    this.Type = SuggestedVisualizationScaleType.Linear;
                    break;
                case "LOGARITHMIC":
                    this.Type = SuggestedVisualizationScaleType.Logarithmic;
                    break;
                default:
                    this.Type = SuggestedVisualizationScaleType.Unknown;
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
                case SuggestedVisualizationScaleType.Linear:
                    typestring = "LINEAR";
                    break;
                case SuggestedVisualizationScaleType.Logarithmic:
                    typestring = "LOGARITHMIC";
                    break;
                default:
                    return string.Empty;
            }
            return string.Concat(typestring, ",", F1, ",", F2);
        }

    }

    public enum SuggestedVisualizationScaleType
    {
        Unknown = 0,
        Linear = 1,
        Logarithmic = 2
    }
}
