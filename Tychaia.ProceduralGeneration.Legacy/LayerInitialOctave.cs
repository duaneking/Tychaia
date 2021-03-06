using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using Protogame.Noise;

namespace Tychaia.ProceduralGeneration
{
    /// <summary>
    /// Generates a layer from octave noise.
    /// </summary>
    [DataContract]
    [FlowDesignerCategory(FlowCategory.General)]
    [FlowDesignerName("Initial Octave")]
    public class LayerInitialOctave : Layer2D
    {
        [DataMember]
        [DefaultValue(100)]
        [Description("The scale of the octave noise map.")]
        public double Scale
        {
            get;
            set;
        }

        [DataMember]
        [Description("The seed modifier value to apply to this octave map.")]
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

        [DataMember]
        [DefaultValue(8)]
        [Description("The number of octave steps.")]
        public int Octaves
        {
            get;
            set;
        }

        public LayerInitialOctave()
            : base()
        {
            // Set defaults.
            this.Scale = 100;
            this.Modifier = new Random().Next();
            this.MinValue = 0;
            this.MaxValue = 100;
            this.Octaves = 8;
        }

        protected override int[] GenerateDataImpl(long x, long y, long width, long height)
        {
            int[] data = new int[width * height];
            OctaveNoise octave = new OctaveNoise(this.GetPerlinRNG(), this.Octaves);

            for (int a = 0; a < width; a++)
                for (int b = 0; b < height; b++)
                {
                    double noise = octave.Noise((x + a) / this.Scale, (y + b) / this.Scale, 0) / 2.0 + 0.5;
                    data[a + b * width] = (int)((noise * (this.MaxValue - this.MinValue)) + this.MinValue);
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

        public override string[] GetParentsRequired()
        {
            return new string[] { };
        }

        public override string ToString()
        {
            return "Initial Octave";
        }
    }
}
