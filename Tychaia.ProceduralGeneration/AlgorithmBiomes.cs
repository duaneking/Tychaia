//
// This source code is licensed in accordance with the licensing outlined
// on the main Tychaia website (www.tychaia.com).  Changes to the
// license on the website apply retroactively.
//
using System;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Drawing;
using Tychaia.ProceduralGeneration.Biomes;
using System.Linq;

namespace Tychaia.ProceduralGeneration
{
    [DataContract]
    [FlowDesignerMajorCategory(FlowMajorCategory.Specific)]
    [FlowDesignerCategory(FlowCategory.Biomes)]
    [FlowDesignerName("Biomes")]
    public class AlgorithmBiomes : Algorithm<int, int, int, Biome>
    {
        public override string[] InputNames
        {
            get { return new string[] { "Rainfall", "Temperature", "Terrain" }; }
        }

        public override bool Is2DOnly
        {
            get { return true; }
        }

        public override void ProcessCell(IRuntimeContext context, int[] inputA, int[] inputB, int[] inputC, Biome[] output, long x, long y, long z, int i, int j, int k, int width, int height, int depth, int ox, int oy, int oz)
        {
            if (inputC[(i + ox) + (j + oy) * width + (k + oz) * width * height] != 0)
                output[(i + ox) + (j + oy) * width + (k + oz) * width * height] = Biomes.BiomeEngine.GetBiomeForCell(inputA[(i + ox) + (j + oy) * width + (k + oz) * width * height], inputB[(i + ox) + (j + oy) * width + (k + oz) * width * height], inputC[(i + ox) + (j + oy) * width + (k + oz) * width * height]);
            else if (inputC[(i + ox) + (j + oy) * width + (k + oz) * width * height] == 0)
                output[(i + ox) + (j + oy) * width + (k + oz) * width * height] = Biomes.BiomeEngine.Biomes.First(v => v is WaterBiome);
        }

        public override Color GetColorForValue(StorageLayer parent, dynamic value)
        {
            if (value == null)
                return Color.Transparent;
            else
                return value.BrushColor;
        }
    }
}

