using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenMark
{
    public class GenProcessing
    {
        public static void CalcFinalScore(List<GenInfo> gens, GenInfo baseGen)
        {
            var score = new int[] { 0, 0, 0 };
            if (gens.Count == 0) return;
            foreach (var gen in gens)
            {
                if (gen.Read_Sum >= 200)
                {
                    gen.score = 1;
                    score[0]++;
                }
                else if (gen.Read_Sum >= 10)
                {
                    gen.score = 2;
                    score[1]++;
                }
                else
                {
                    gen.score = 3;
                    score[2]++;

                }
                int backgroundSum = gen.Bgr_Sum.Value + gen.AllBgr_Sum.Value;
                gen.BackgroundSum = backgroundSum;
                gen.B = backgroundSum > 0.2 * gen.Read_Sum;

            }


            // a. Single 1 score
            if (score[0] == 1)
            {
                var gen = gens.First(g => g.score == 1);
                gen.setFinal(gen.B ? "B" : "", "a.Only only one have best counts ");
            }
            //For 2 or more 1 score sets:   
            else if (score[0] > 1)
            {
                var gensScore1 = gens.Where(g => g.score == 1).ToList();
                var gens250L = gensScore1.Where(g => g.AmpLength >= 45 && g.AmpLength <= 250).ToList();
                // When two or three primer sets get a 1 score (with all amplicon lengths 45-250):
                if (gens250L.Count == score[0])
                {
                    var gens1500R = gens250L.Where(g => g.Read_Sum < 15000).ToList();
                    var gens1500R_B = gens1500R.Where(g => !g.B).ToList();
                    if (gens1500R_B.Count > 0)
                        gens1500R = gens1500R_B;
                    if (gens1500R.Count > 0)
                    {
                        var maxGem = GetMaxByReadSum(gens1500R);
                        if (maxGem != null) maxGem.setFinal(maxGem.B ? "B" : "", "b.for those with all primer length 45-180, pick one with highest reads ");
                    }
                    else
                    {
                        gens1500R = gens250L.Where(g => g.Read_Sum >= 15000).ToList();
                        gens1500R_B = gens1500R.Where(g => !g.B).ToList();
                        if (gens1500R_B.Count > 0)
                            gens1500R = gens1500R_B;
                        var maxGem = GetMinByReadSum(gens1500R);
                        if (maxGem != null) maxGem.setFinal(maxGem.B ? "AB+B" : "AB", "b.for those with all primer length 45-180, pick one with highest reads ");
                    }
                }
                //  for those with one or two primer length 45-180
                else
                {
                    // c. When two or three primer sets get a 1 score (with one or two amplicon lengths > 250):
                    var gens250G = gensScore1.Where(g => g.AmpLength > 250).ToList();
                    if (gens250G.Count > 0)
                    {
                        var gens1500R = gensScore1.Where(g => g.Read_Sum >= 15000).ToList();
                        //if all sets with 1 score > 15000, take lowest, add AB (abundant) to comment column
                        if (gens1500R.Count == gensScore1.Count)
                        {
                            var maxGem = GetMinByReadSum(gens1500R);
                            maxGem.setFinal("AB", "c. When two or three primer sets get a 1 score (with one or two amplicon lengths > 250):");
                        }
                        else if (gens1500R.Count != gensScore1.Count)
                        {
                            if (gens250L.Count > 0)
                            {
                                var max250GGem = GetMaxByReadSum(gens250G);
                                var max250LGem = GetMaxByReadSum(gens250L);
                                if (max250GGem.Read_Sum > max250LGem.Read_Sum * 3)
                                {
                                    max250GGem.setFinal("", "c. only take set with amplicon length > 250 if it is > 3X highest read of best set with amplicon length 45-250");
                                }
                                else
                                {
                                    max250LGem.setFinal("", "c. otherwise take best from 45-250 amplicon length set(s)");
                                }
                            }
                        }
                        else
                        {
                            var max250GGem = GetMaxByReadSum(gens250G);
                            max250GGem.setFinal("", "if all amplicon lengths > 250, take one with highest reads");
                        }

                        //-for all best picks, calculate total background as sum of col G & H background values,
                        //if total background is > 20% of reads for best pick, exclude and take next best choice and enter B into comment column
                        var finalGen1 = getFinal(gens);
                        if (finalGen1 != null)
                        {
                            if (finalGen1.BackgroundSum > 0.2 * finalGen1.Read_Sum)
                            {
                                finalGen1.final = false;
                                finalGen1.finalString = null;
                                finalGen1.comment = null;
                            }
                        }

                    }
                }
            }

            var finalGen = getFinal(gens);
            //# Next
            if (finalGen == null)
            {
                // For sets with only scores of 2s and 3s:
                if (score[0] == 0)
                {
                    var gens2Score = gens.Where(g => g.score == 2).ToList();
                    // #-for all 2s, pick one with highest reads, mark as 2
                    if (score[1] > 0 && score[2] == 0)
                    {
                        var maxGem = GetMaxByReadSum(gens2Score);
                        maxGem.setFinal("2", "for all 2s, pick one with highest reads, mark as 2");
                    }
                    else if (score[1] > 0 && score[2] > 0)
                    {
                        var maxGem = GetMaxByReadSum(gens2Score);
                        maxGem.setFinal("2", "for mix of 2s and 3s, pick the 2 with highest reads, mark as 2");
                    }
                    else
                    {
                        gens2Score = gens.Where(g => g.score == 3).ToList();
                        var maxGem = GetMaxByReadSum(gens2Score);
                        if (maxGem != null)
                            maxGem.setFinal("3", "for all 3s, pick one with highest reads, mark as 3");
                    }
                    //   #if the sum of background (columns G& H) is 20% or more of the MAX number, 
                    // # enter a "B" in the comment column.     
                    finalGen = getFinal(gens);
                    if (finalGen != null)
                    {
                        if (finalGen.B) finalGen.comment = "B";
                    }
                }
            }
            finalGen = getFinal(gens);
            if (finalGen != null)
            {
                if (finalGen.Read_Sum > 15000)
                    finalGen.comment = "AB";
                if (baseGen != null)
                {
                    if (baseGen.Read_Sum * 2 > finalGen.Read_Sum)
                    {
                        finalGen.comment = "L" + (finalGen.comment ?? "");
                    }
                }
            }
            if (gens.Count == 1)
            {
                gens[0].comment = (gens[0].comment ?? "") + "4";
            }

        }
        public static GenInfo GetMaxByReadSum(List<GenInfo> gens)
        {
            GenInfo ret = null;
            foreach (var gen in gens)
            {
                if (ret == null || ret.Read_Sum < gen.Read_Sum)
                {
                    ret = gen;
                }
            }
            return ret;
        }
        public static GenInfo GetMinByReadSum(List<GenInfo> gens)
        {
            GenInfo ret = null;
            foreach (var gen in gens)
            {
                if (ret == null || ret.Read_Sum > gen.Read_Sum)
                {
                    ret = gen;
                }
            }
            return ret;
        }
        public static GenInfo getFinal(List<GenInfo> gens)
        {
            GenInfo ret = null;
            foreach (var gen in gens)
            {
                if (gen.final)
                {
                    ret = gen;
                }
            }
            return ret;
        }
    }
}
