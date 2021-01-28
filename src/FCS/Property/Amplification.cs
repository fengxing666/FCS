namespace FCS.Property
{
    /// <summary>
    /// 放大类型参数 PnE
    /// v=10^（PowerNumber * xc /（PnR））* ZeroValue
    /// </summary>
    public struct Amplification
    {
        /// <summary>
        /// 10的次方数
        /// </summary>
        public double PowerNumber { get; set; }
        /// <summary>
        /// 0对应的转换值
        /// </summary>
        public double ZeroValue { get; set; }

        public Amplification(double powerNumber, double zeroValue)
        {
            this.PowerNumber = powerNumber;
            this.ZeroValue = zeroValue;
        }

        public Amplification(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                this.PowerNumber = this.ZeroValue = 0d;
                return;
            }
            var spirts = str.Split(',');
            if (spirts.Length != 2)
            {
                this.PowerNumber = this.ZeroValue = 0d;
                return;
            }
            if (double.TryParse(spirts[0], out double powerNumber)) this.PowerNumber = powerNumber;
            else this.PowerNumber = 0d;
            if (double.TryParse(spirts[1], out double zeroValue)) this.ZeroValue = zeroValue;
            else this.ZeroValue = 0d;
        }

        public override string ToString()
        {
            return string.Concat(PowerNumber, ",", ZeroValue);
        }

    }
}
