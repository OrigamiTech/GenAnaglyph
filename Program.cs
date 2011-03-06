using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace GenAnaglyph
{
    class Program
    {
        enum Channel
        {
            B = 0,
            G = 1,
            R = 2,
            A = 3,
        }
        static void Main(string[] args)
        {
            foreach (string path in args)
                if (File.Exists(path))
                    using (Image img = Image.FromFile(path))
                        AnaglyphBlend(new Bitmap(img)).Save(path + ".anaglyph.png");
        }
        static Bitmap AnaglyphBlend(Bitmap input)
        {
            try
            {
                Bitmap output = new Bitmap(input.Width / 2, input.Height, PixelFormat.Format32bppArgb);
                var bitsInput = input.LockBits(new Rectangle(new Point(0, 0), input.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var bitsOutput = output.LockBits(new Rectangle(new Point(0, 0), output.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                unsafe      // Remember to build with the 'allow unsafe code' flag enabled.
                {
                    for (int y = 0; y < input.Height; y++)
                    {
                        byte* ptrInput = (byte*)bitsInput.Scan0 + y * bitsInput.Stride;
                        byte* ptrOutput = (byte*)bitsOutput.Scan0 + y * bitsOutput.Stride;
                        for (int x = 0; x < output.Width; x++)
                        {
                            ptrOutput[4 * x + (int)Channel.B] = Screen(ptrInput[4 * x + (int)Channel.B], 0);
                            ptrOutput[4 * x + (int)Channel.G] = Screen(ptrInput[4 * x + (int)Channel.G], 0);
                            ptrOutput[4 * x + (int)Channel.R] = Screen(0, ptrInput[4 * (x + output.Width) + (int)Channel.R]);
                            ptrOutput[4 * x + (int)Channel.A] = Screen(ptrInput[4 * x + (int)Channel.A], ptrInput[4 * (x + output.Width) + (int)Channel.A]);
                        }
                    }
                }
                input.UnlockBits(bitsInput);
                output.UnlockBits(bitsOutput);
                return output;
            }
            catch { return input; }
        }
        static byte Screen(byte top, byte bottom)
        { return (byte)(0xFF - (((0xFF - top) * (0xFF - bottom)) / 0xFF)); }
    }
}