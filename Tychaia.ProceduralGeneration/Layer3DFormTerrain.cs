﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Tychaia.ProceduralGeneration
{
    /// <summary>
    /// Converts 2D information into a 3D landscape.
    /// </summary>
    [DataContract()]
    public class Layer3DFormTerrain : Layer3D
    {
        public Layer3DFormTerrain(Layer parent, Layer biomes)
            : base(new Layer[] { parent, biomes })
        {
        }

        public override int[] GenerateData(int x, int y, int z, int width, int height, int depth)
        {
            if (this.Parents.Length < 2 || this.Parents[0] == null || this.Parents[1] == null)
                return new int[width * height];

            int[] parent = this.Parents[0].GenerateData(x, y, width, height);
            int[] biomes = this.Parents[1].GenerateData(x, y, width, height);
            int[] data = new int[width * height * depth];

            // Fill data with air.
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    for (int k = 0; k < depth; k++)
                        data[i + j * width + k * width * height] = -1;

            // Loop over the terrain and fill in the areas.
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    if (parent[i + j * height] == 0)
                    {
                        // Ocean
                        if (0 >= z && 0 < z + depth)
                            data[i + j * width + 0 * width * height] = 0;
                    }
                    else
                    {
                        // Land
                        for (int k = z; k < z + depth; k++)
                            if (k < parent[i + j * height] + 1)
                                data[i + j * width + (k - z) * width * height] = biomes[i + j * height];
                    }
                }

            return data;
        }

        public override Dictionary<int, System.Drawing.Brush> GetLayerColors()
        {
            if (this.Parents.Length < 2 || this.Parents[0] == null || this.Parents[1] == null)
                return null;
            else
                return this.Parents[1].GetLayerColors();
        }

        public override string[] GetParentsRequired()
        {
            return new string[] { "Terrain", "Biomes" };
        }

        public override string ToString()
        {
            return "Form 3D Terrain";
        }

        public override int StandardDepth
        {
            get { return 16; }
        }

        public override bool[] GetParents3DRequired()
        {
            return new bool[] { false, false };
        }
    }
}