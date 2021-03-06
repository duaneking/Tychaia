using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Tychaia.ProceduralGeneration;
using System.Threading.Tasks;
using System.Threading;

namespace Tychaia.ProceduralGeneration.Flow
{
    public static class AlgorithmTraceImageGeneration
    {
        #region Cell Render Ordering

        private static int[][] CellRenderOrder = new int[4][]
            {
                null,
                null,
                null,
                null
            };
        private const int RenderToNE = 0;
        private const int RenderToNW = 1;
        private const int RenderToSE = 2;
        private const int RenderToSW = 3;

        private static int[] CalculateCellRenderOrder(int targetDir, int width, int height)
        {
            /*               North
             *        0  1  2  3  4  5  6
             *        1  2  3  4  5  6  7
             *        2  3  4  5  6  7  8
             *  East  3  4  5  6  7  8  9  West
             *        4  5  6  7  8  9  10
             *        5  6  7  8  9  10 11
             *        6  7  8  9  10 11 12
             *               South
             *
             * Start value is always 0.
             * Last value is (MaxX + MaxY).
             * This is the AtkValue.
             *
             * We attack from the left side of the render first
             * with (X: 0, Y: AtkValue) until Y would be less than
             * half of AtkValue.
             *
             * We then attack from the right side of the render
             * with (X: AtkValue, Y: 0) until X would be less than
             * half of AtkValue - 1.
             *
             * If we are attacking from the left, but Y is now
             * greater than MaxY, then we are over half-way and are
             * now starting at the bottom of the grid.
             *
             * In this case, we start with (X: AtkValue - MaxY, Y: MaxY)
             * and continue until we reach the same conditions that
             * apply normally.  The same method applies to the right hand
             * side where we start with (X: MaxX, Y: AtkValue - MaxX).
             *
             */

            if (targetDir != RenderToNE)
                throw new InvalidOperationException();

            int[] result = new int[width * height];
            int count = 0;
            int start = 0;
            int maxx = width - 1;
            int maxy = height - 1;
            int last = maxx + maxy;
            int x, y;

            for (int atk = start; atk <= last; atk++)
            {
                // Attack from the left.
                if (atk < maxy)
                {
                    x = 0;
                    y = atk;
                }
                else
                {
                    x = atk - maxy;
                    y = maxy;
                }
                while (y > atk / 2)
                    result[count++] = y-- * width + x++;

                // Attack from the right.
                if (atk < maxx)
                {
                    x = atk;
                    y = 0;
                }
                else
                {
                    x = maxx;
                    y = atk - maxx;
                }
                while (y <= atk / 2)
                    result[count++] = y++ * width + x--;
            }

            return result;
        }

        private static int[] GetCellRenderOrder(int cameraDirection, int width, int height)
        {
            if (CellRenderOrder[cameraDirection] == null)
                CellRenderOrder[cameraDirection] = CalculateCellRenderOrder(cameraDirection, width, height);
            return CellRenderOrder[cameraDirection];
        }

        #endregion

        private const int TraceScale = 3;
        private const int TraceRenderWidth = 64 * TraceScale;
        private const int TraceRenderHeight = 64 * TraceScale;
        private const int TraceRenderDepth = 64 * TraceScale;

        public static Bitmap RenderTraceResult(
            RuntimeLayer layer,
            dynamic data,
            int width,
            int height,
            int depth)
        {
            int owidth = width;
            int oheight = height;
            width = 128 * TraceScale;
            height = 128 * TraceScale;
            depth = 128 * TraceScale;

            Bitmap b = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            StorageLayer parent;
            if (layer.GetInputs().Length == 0)
                parent = null;
            else
                parent = StorageAccess.FromRuntime(layer.GetInputs()[0]);

            int[] render = GetCellRenderOrder(RenderToNE, TraceRenderWidth, TraceRenderHeight);
            int ztop = layer.Algorithm.Is2DOnly ? 1 : 128;
            int zbottom = 0;
            for (int z = zbottom; z < ztop; z++)
            {
                int rcx = width / 2 - 1;
                int rcy = height / 2 - (height / 2 - 1);
                int rw = 2;
                int rh = 1;
                for (int i = 0; i < render.Length; i++)
                {
                    // Calculate the X / Y of the tile in the grid.
                    int x = render[i] % TraceRenderWidth;
                    int y = render[i] / TraceRenderWidth;

                    // Calculate the render position on screen.
                    int rx = rcx + (int)((x - y) / 2.0 * rw);// (int)(x / ((RenderWidth + 1) / 2.0) * rw);
                    int ry = rcy + (x + y) * rh - (rh / 2 * (TraceRenderWidth + TraceRenderHeight)) - (z - zbottom) * 1;

                    while (true)
                    {
                        try
                        {
                            Color lc = layer.Algorithm.GetColorForValue(
                                parent,
                                data[x + y * owidth + z * owidth * oheight]);
                            SolidBrush sb = new SolidBrush(Color.FromArgb(lc.A, lc.R, lc.G, lc.B));
                            g.FillRectangle(
                                sb,
                                new Rectangle(rx, ry, rw, rh)
                            );
                            break;
                        }
                        catch (InvalidOperationException)
                        {
                            // Graphics can be in use elsewhere, but we don't care; just try again.
                        }
                    }
                }
            }

            return b;
        }
    }
}
