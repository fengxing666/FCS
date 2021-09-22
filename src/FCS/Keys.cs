namespace FCS
{
    public class Keys
    {
        public const byte DelimiterByte = 0x0A;//默认文本段分隔符,换行符
        #region must
        public const string BeginDataKey = "$BEGINDATA";
        public const string ByteOrdKey = "$BYTEORD";
        public const string CYTKey = "$CYT";
        public const string DataTypeKey = "$DATATYPE";
        public const string EndDataKey = "$ENDDATA";
        public const string NextDataKey = "$NEXTDATA";
        public const string PARKey = "$PAR";
        public const string PnBKey = "$P{0}B";
        public const string PnEKey = "$P{0}E";
        public const string PnNKey = "$P{0}N";
        public const string PnRKey = "$P{0}R";
        public const string TOTKey = "$TOT";
        #endregion
        #region choose
        public const string ABRTKey = "$ABRT";
        public const string BeginAnalysisKey = "$BEGINANALYSIS";
        public const string BeginDateTimeKey = "$BEGINDATETIME";
        public const string BeginSTextKey = "$BEGINSTEXT";
        public const string CarrierTypeKey = "$CARRIERTYPE";
        public const string CellsKey = "$CELLS";
        public const string COMKey = "$COM";
        public const string CYTSNKey = "$CYTSN";
        public const string EndAnalysisKey = "$ENDANALYSIS";
        public const string EndDateTimeKey = "$ENDDATETIME";
        public const string EndSTextKey = "$ENDSTEXT";
        public const string EXPKey = "$EXP";
        public const string FILKey = "$FIL";
        public const string FlowRateKey = "$FLOWRATE";
        public const string INSTKey = "$INST";
        public const string LastModifiedKey = "$LAST_MODIFIED";
        public const string LastModifierKey = "$LAST_MODIFIER";
        public const string LostKey = "$LOST";
        public const string ModeKey = "$MODE";
        public const string OPKey = "$OP";
        public const string OriginalityKey = "$ORIGINALITY";
        public const string PnAnalyteKey = "$P{0}ANALYTE";
        public const string PnCalibrationKey = "$P{0}CALIBRATION";
        public const string PnDKey = "$P{0}D";
        public const string PnDataTypeKey = "$P{0}DATATYPE";
        public const string PnDETKey = "$P{0}DET";
        public const string PnFKey = "$P{0}F";
        public const string PnFeatureKey = "$P{0}FEATURE";
        public const string PnGKey = "$P{0}G";
        public const string PnLKey = "$P{0}L";
        public const string PnOKey = "$P{0}O";
        public const string PnSKey = "$P{0}S";
        public const string PnTKey = "$P{0}T";
        public const string PnTagKey = "$P{0}TAG";
        public const string PnTypeKey = "$PnTYPE";
        public const string PnVKey = "$P{0}V";
        public const string PROJKey = "$PROJ";
        public const string SMNOKey = "$SMNO";
        public const string SpilLoverKey = "$SPILLOVER";
        public const string SRCKey = "$SRC";
        public const string SYSKey = "$SYS";
        public const string TimeStepKey = "$TIMESTEP";
        public const string TRKey = "$TR";
        public const string UnstainedCentersKey = "$UNSTAINEDCENTERS";
        public const string UnstainedInfoKey = "$UNSTAINEDINFO";
        public const string VOLKey = "$VOL";
        #endregion
        /// <summary>
        /// 早期fcs文件的补偿关键字
        /// </summary>
        public const string COMPKey = "$COMP";

        public const string UnicodeKey = "$UNICODE";//编码类型
    }
}
