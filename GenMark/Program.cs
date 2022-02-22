using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace GenMark
{
    partial class Program
    {
        static void Main(string[] args)
        {
            string inputFile = EexeclInfo.INPUT_FILE;
            if (args.Length > 0)
            {
                inputFile = args[0];
            }
            Console.WriteLine($"Input file {inputFile}");
            Console.WriteLine("");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(inputFile)))
            {
                string geneSymbol = "";
                int emptyLineCount = 0;
                var sheet = package.Workbook.Worksheets[0];
                List<GenInfo> gens = new List<GenInfo>();
                GenInfo baseGen = null;
                int geneStartLine = 0, geneSymbolStep = 0, countGen = 0;

                sheet.Cells[1, EexeclInfo.FinalScoreINDEX].Value = "Score";
                sheet.Cells[1, EexeclInfo.FinalScoreMarkINDEX].Value = "Result";
                sheet.Cells[1, EexeclInfo.FinalScoreCommentINDEX].Value = "Comment";
                sheet.Cells[1, EexeclInfo.FinalScoreTextINDEX].Value = "Text";


            for (int r = 2; r < 100000; r++)
                {
                    string gen = sheet.Cells[r, EexeclInfo.GenINDEX].GetValue<String>();
                    if (String.IsNullOrEmpty(gen)) emptyLineCount++;
                    else emptyLineCount = 0;
                    if (emptyLineCount > 5) break;
                    if (gen != geneSymbol)
                    {
                        GenProcessing.CalcFinalScore(gens, baseGen);
                        ShowResult(gens, sheet, geneStartLine - 1);
                        if(countGen%100==0 && !String.IsNullOrWhiteSpace(gen))
                            Console.WriteLine($"\rGen={gen} count={countGen}");

                        gens = new List<GenInfo>();
                        baseGen = null;
                        geneSymbol = gen;
                        geneSymbolStep = 0;
                        geneStartLine = r;
                        countGen += 1;
                    }
                    geneSymbolStep++;
                    var genInfo = new GenInfo()
                    {
                        GeneSymbolStep = geneSymbolStep,
                        GeneSymbol = geneSymbol,
                        AmpLength = sheet.Cells[r, EexeclInfo.AmpLengthINDEX].GetValue<int>(),
                        Read_Sum = sheet.Cells[r, EexeclInfo.Read_SumINDEX].GetValue<int>(),
                        Bgr_Sum = sheet.Cells[r, EexeclInfo.Bgr_SumINDEX].GetValue<int?>(),
                        AllBgr_Sum = sheet.Cells[r, EexeclInfo.AllBgr_SumINDEX].GetValue<int?>(),


                    };
                    if (genInfo.Bgr_Sum == null)
                    {
                        baseGen = genInfo;
                    }
                    else
                    {
                        gens.Add(genInfo);
                    }
                }
                GenProcessing.CalcFinalScore(gens, baseGen);
                ShowResult(gens, sheet, geneStartLine - 1);
                string resultFile = "result" + DateTime.Now.ToString("yyyyMMdd_Hmm") + ".xlsx";
                package.SaveAs(resultFile);
            }
        }

        private static void ShowResult(List<GenInfo> gens, ExcelWorksheet sheet, int r)
        {
            foreach (var gen in gens)
            {
                if (gen.final)
                {
                    sheet.Cells[gen.GeneSymbolStep + r, EexeclInfo.FinalScoreMarkINDEX].Value = "MAX";
                    if (!String.IsNullOrEmpty(gen.comment))
                        sheet.Cells[gen.GeneSymbolStep + r, EexeclInfo.FinalScoreCommentINDEX].Value = gen.comment;
                    if (!String.IsNullOrEmpty(gen.finalString))
                        sheet.Cells[gen.GeneSymbolStep + r, EexeclInfo.FinalScoreTextINDEX].Value = gen.finalString;
                    sheet.Cells[gen.GeneSymbolStep + r, EexeclInfo.FinalScoreINDEX].Value = gen.score;
                }
                else
                {
                    // if (gen.B)  sheet.Cells[gen.GeneSymbolStep + r, EexeclInfo.FinalScoreCommentINDEX].Value = "B";

                }

            }

        }


    }
}
