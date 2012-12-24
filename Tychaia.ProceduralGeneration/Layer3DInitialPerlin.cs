﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using Protogame.Noise;

namespace Tychaia.ProceduralGeneration
{
    /// <summary>
    /// Generates a 3D layer from perlin noise.
    /// </summary>
    [DataContract]
    public class Layer3DInitialPerlin : Layer3D
    {
        [DataMember]
        [DefaultValue(100)]
        [Description("The scale of the perlin noise map.")]
        public double Scale
        {
            get;
            set;
        }

        [DataMember]
        [Description("The seed modifier value to apply to this perlin map.")]
        public long Modifier
        {
            get;
            set;
        }

        [DataMember]
        [DefaultValue(0)]
        [Description("The minimum integer value in the resulting layer.")]
        public int MinValue
        {
            get;
            set;
        }

        [DataMember]
        [DefaultValue(100)]
        [Description("The maximum integer value in the resulting layer.")]
        public int MaxValue
        {
            get;
            set;
        }

        public Layer3DInitialPerlin()
            : base()
        {
            // Set defaults.
            this.Scale = 100;
            this.Modifier = new Random().Next();
            this.MinValue = 0;
            this.MaxValue = 100;
        }

        protected override int[] GenerateDataImpl(long x, long y, long z, long width, long height, long depth)
        {
            int[] data = new int[width * height * depth];
            PerlinNoise perlin = new PerlinNoise(this.GetPerlinRNG());

            for (int a = 0; a < width; a++)
                for (int b = 0; b < height; b++)
                    for (int c = 0; c < depth; c++)
                    {
                        double noise = perlin.Noise((x + a) / this.Scale, (y + b) / this.Scale, (z + c) / this.Scale) / 2.0 + 0.5;
                        data[a + b * width + c * width * height] = (int)((noise * (this.MaxValue - this.MinValue)) + this.MinValue);
                    }

            return data;
        }

        private int GetPerlinRNG()
        {
            long seed = this.Seed;
            seed += this.Modifier;
            seed *= this.Modifier;
            seed += this.Modifier;
            seed *= this.Modifier;
            seed += this.Modifier;
            seed *= this.Modifier;
            return (int)seed;
        }

        public override Dictionary<int, LayerColor> GetLayerColors()
        {
            return LayerColors.GetGradientBrushes(this.MinValue, this.MaxValue);
        }

        public override bool[] GetParents3DRequired()
        {
            return new bool[] { };
        }

        public override string[] GetParentsRequired()
        {
            return new string[] { };
        }

        public override string ToString()
        {
            return "Initial Perlin 3D";
        }
    }
}