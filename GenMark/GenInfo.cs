namespace GenMark
{
        public class GenInfo
        {
            public string GeneSymbol { get; set; }
            public int GeneSymbolStep { get; set; }
            public int AmpLength { get; set; }
            public int Read_Sum { get; set; }
            public int? Bgr_Sum { get; set; }
            public int? AllBgr_Sum { get; set; }
            public bool final = false;
            public string comment;
            public string finalString;
            public int score = 0;
            public bool B = false;
            public int? BackgroundSum { get; set; }
            public void setFinal(string comment, string finalString)
            {
                this.final = true;
                this.comment = comment;
                this.finalString = finalString;
            }
        }
    
}
