using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Tesseract;

namespace ReadImage
{
    public class Reader
    {
        static string spechars = "\":7";
        public static string[,] chartable = new string[4, 4];
        public static string[,] bonustable = new string[4, 4];
        public static int[,] bonusColorTable = new int[4, 4];

        public static void Read()
        {
            string solutionDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            var areaVals = File.ReadAllText(solutionDirectory + "/Config/Area.txt").Split(",");

            
            int startx = 767;
            int starty = 512;
            int diff = 111;

            startx = int.Parse(areaVals[0]) - (int)((double)diff / 5.55);
            starty = int.Parse(areaVals[1]) - (int)((double)diff / 7.9285);
            diff = int.Parse(areaVals[2]);

            Bitmap[,] letterBitmap = new Bitmap[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    letterBitmap[i, j] = new Bitmap(36, 45);
                }
            }

            Bitmap[,] bonusBitmap = new Bitmap[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    bonusBitmap[i, j] = new Bitmap(25, 18);
                }
            }


            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Graphics letterG = Graphics.FromImage(letterBitmap[i, j]);
                    Graphics bonusG = Graphics.FromImage(bonusBitmap[i, j]);

                    letterG.CopyFromScreen((int)(startx + (i * 111)), (int)(starty + (j * diff)), 0, 0, new Size(36, 45));
                    bonusG.CopyFromScreen((int)(734 + (i * diff)), (int)(485 + (j * diff)), 0, 0, new Size(25, 18));
                }
            }


            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (bonusBitmap[i,j].GetPixel(1, 1).R > 225 && bonusBitmap[i, j].GetPixel(1, 1).G < 225)
                    {
                        bonusColorTable[i, j] = 4;
                    }
                    if (bonusBitmap[i, j].GetPixel(1, 1).R > 200 && bonusBitmap[i, j].GetPixel(1, 1).G < 100)
                    {
                        bonusColorTable[i, j] = 3;
                    }
                    if (bonusBitmap[i, j].GetPixel(1, 1).R > 157 && bonusBitmap[i, j].GetPixel(1, 1).G < 200 && bonusBitmap[i, j].GetPixel(1, 1).B > 200)
                    {
                        bonusColorTable[i, j] = 2;
                    }
                    if (bonusBitmap[i, j].GetPixel(1, 1).R < 150 && bonusBitmap[i, j].GetPixel(1, 1).G > 150)
                    {
                        bonusColorTable[i, j] = 1;
                    }
                    if (bonusBitmap[i, j].GetPixel(1, 1).R < 50)
                    {
                        bonusColorTable[i, j] = 0;
                    }


                    Bitmap testbonusimg = new Bitmap(1, 1);
                    testbonusimg.SetPixel(0,0, bonusBitmap[i, j].GetPixel(1, 1));
                    testbonusimg.Save("test"+i+j+".png");

                    //Removes background and makes font black from letterBitmap
                    for (int y = 0; (y <= (letterBitmap[i, j].Height - 1)); y++)
                    {
                        for (int x = 0; (x <= (letterBitmap[i, j].Width - 1)); x++)
                        {

                            //Excel new black and white
                            if (letterBitmap[i, j].GetPixel(x, y).R == 26 && letterBitmap[i, j].GetPixel(x, y).G == 18 && letterBitmap[i, j].GetPixel(x, y).B == 43)
                            {
                                Color inv = Color.FromArgb(0, 0, 0);
                                letterBitmap[i, j].SetPixel(x, y, inv);
                            }
                            else
                            {
                                Color inv = Color.FromArgb(255, 255, 255);
                                letterBitmap[i, j].SetPixel(x, y, inv);
                            }

                        }
                    }

                    var letterStream = new MemoryStream();

                    letterBitmap[i, j].Save(letterStream, System.Drawing.Imaging.ImageFormat.Png);

                    var oneChar = letterStream.ToArray();

                    Pix w = Pix.LoadFromMemory(oneChar);

                    TesseractEngine letterEngine = new TesseractEngine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName + "/ReadImage/", "hun", EngineMode.Default);

                    Page singlechar = letterEngine.Process(w, PageSegMode.SingleChar);

                    string charText = singlechar.GetText();
                    bool wrongchar = false;
                    int k = 0;

                    letterBitmap[i, j].Save("char" + i.ToString() + j.ToString()+".png");
                    bonusBitmap[i, j].Save("bonus" + i.ToString() + j.ToString() + ".png");


                    while (k < charText.Length && !wrongchar)
                    { 
                        if (spechars.Contains(charText[k]))
                        {
                            wrongchar = true;
                        }
                        k++;
                    }

                    //Handling letter differences from webpage style changes.

                    if (wrongchar)
                    {
                        chartable[i, j] = "T";
                    }

                    else if (charText.Contains("2"))
                    {
                        chartable[i, j] = "Z";
                    }
                    else if (charText.Contains("l"))
                    {
                        int blackcount = 0;
                        for (int bitmapx = 0; bitmapx < letterBitmap[i, j].Width - 1; bitmapx++)
                        {
                            for (int bitmapy = 0; bitmapy < letterBitmap[i, j].Height - 1; bitmapy++)
                            {
                                if (letterBitmap[i, j].GetPixel(bitmapx, bitmapy).R == 0)
                                {
                                    blackcount++;
                                }
                            }
                        }
                        chartable[i, j] = "Í";

                        if (blackcount == 153)
                        {
                            chartable[i, j] = "Í";
                        }
                        if(blackcount == 132)
                        {
                            chartable[i, j] = "I";
                        }
                    }
                    else if (charText.ToLower().Contains("ő") || charText.ToLower().Contains("ö"))
                    {
                        int blackcount = 0;
                        for (int bitmapx = 0; bitmapx < letterBitmap[i,j].Width-1; bitmapx++)
                        {
                            for (int bitmapy = 0; bitmapy < letterBitmap[i,j].Height-1; bitmapy++)
                            {
                                if(letterBitmap[i,j].GetPixel(bitmapx,bitmapy).R == 0)
                                {
                                    blackcount++;
                                }
                            }
                        }

                        if (blackcount >= 398)
                        {
                            chartable[i, j] = "Ő";
                        }
                        else
                        {
                            chartable[i, j] = "Ö";
                        }

                    }
                    else if (charText.Contains("U") || charText.Contains("8"))
                    {
                        int blackcount = 0;
                        for (int bitmapx = 0; bitmapx < letterBitmap[i, j].Width - 1; bitmapx++)
                        {
                            for (int bitmapy = 0; bitmapy < letterBitmap[i, j].Height - 1; bitmapy++)
                            {
                                if (letterBitmap[i, j].GetPixel(bitmapx, bitmapy).R == 0)
                                {
                                    blackcount++;
                                }
                            }
                        }
                        if(blackcount > 324)
                        {
                            chartable[i, j] = "Ü";
                        }
                        else
                        {
                            chartable[i, j] = "U";
                        }
                    }
                    else
                    {
                        chartable[i, j] = singlechar.GetText()[0].ToString();
                    }
                }
            }
        }
    }
}
