//
// This source code is licensed in accordance with the licensing outlined
// on the main Tychaia website (www.tychaia.com).  Changes to the
// license on the website apply retroactively.
//
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Tychaia.ProceduralGeneration.Compiler;
using ICSharpCode.NRefactory.CSharp;
using Protogame.Noise;

namespace Tychaia.ProceduralGeneration
{
    public class RuntimeLayer : IRuntimeContext, IGenerator
    {
        /// <summary>
        /// The current algorithm for this layer.
        /// </summary>
        [DataMember]
        private IAlgorithm
            m_Algorithm;

        /// <summary>
        /// The input layers.
        /// </summary>
        [DataMember]
        private RuntimeLayer[]
            m_Inputs;

        /// <summary>
        /// The current algorithm that this runtime layer is using.
        /// </summary>
        public IAlgorithm Algorithm
        {
            get { return this.m_Algorithm; }
        }

        /// <summary>
        /// Creates a new runtime layer that holds the specified algorithm.
        /// </summary>
        public RuntimeLayer(IAlgorithm algorithm)
        {
            if (algorithm == null)
                throw new ArgumentNullException("algorithm");
            this.m_Algorithm = algorithm;
            var inputs = new List<RuntimeLayer>();
            for (var i = 0; i < algorithm.InputTypes.Length; i++)
                inputs.Add(null);
            this.m_Inputs = inputs.ToArray();

            this.Seed = 100;
            this.Modifier = 0;
        }

        /// <summary>
        /// Determines whether or not the specified input algorithm can be used as an
        /// input for the current algorithm in the specified index slot.
        /// </summary>
        public bool CanBeInput(int index, RuntimeLayer input)
        {
            if (input == null)
                return true;
            if (index < 0 || index >= this.m_Algorithm.InputTypes.Length)
                return false;
            return (input.m_Algorithm.OutputType == this.m_Algorithm.InputTypes[index]);
        }

        /// <summary>
        /// Sets the specified algorithm as the input at the specified index.
        /// </summary>
        public void SetInput(int index, RuntimeLayer input)
        {
            if (!this.CanBeInput(index, input))
                throw new InvalidOperationException("Specified algorithm can not be set as input at this index.");
            this.m_Inputs[index] = input;
        }

        /// <summary>
        /// Returns the current list of inputs for this runtime layer.
        /// </summary>
        public RuntimeLayer[] GetInputs()
        {
            if (this.m_Inputs == null)
            {
                var inputs = new List<RuntimeLayer>();
                for (var i = 0; i < this.m_Algorithm.InputTypes.Length; i++)
                    inputs.Add(null);
                this.m_Inputs = inputs.ToArray();
            }
            return this.m_Inputs;
        }

        /// <summary>
        /// The modifier used by algorithms as an additional input to the
        /// random function calls.
        /// </summary>
        [DataMember]
        [Description("The seed modifier value to apply.")]
        public long Modifier
        {
            get;
            set;
        }

        // Just finding offsets, then use them to determine max width, start X location, etc.
        public static void FindMaximumOffsets(
            RuntimeLayer layer, 
            out int OffsetX,
            out int OffsetY,
            out int OffsetZ,
            out int HalfX, 
            out int HalfY, 
            out int HalfZ)
        {
            if (layer == null)
                throw new ArgumentNullException("layer");

            OffsetX = 0;
            OffsetY = 0;
            OffsetZ = 0;
            HalfX = 0;
            HalfY = 0;
            HalfZ = 0;

            if (layer.m_Inputs.Length != 0)
            {
                int inputs = 0;
                int[] TempOffsetX = new int[layer.m_Inputs.Length];
                int[] TempOffsetY = new int[layer.m_Inputs.Length];
                int[] TempOffsetZ = new int[layer.m_Inputs.Length];
                int[] TempHalfX = new int[layer.m_Inputs.Length];
                int[] TempHalfY = new int[layer.m_Inputs.Length];
                int[] TempHalfZ = new int[layer.m_Inputs.Length];

                foreach (var input in layer.m_Inputs)
                {
                    if (input == null)
                        continue;

                    // can't just divide offsets after half by half
                    // 

//                    TempOffsetX[inputs] += (layer.m_Algorithm.InputWidthAtHalfSize[inputs] ? Math.Abs(layer.m_Algorithm.RequiredXBorder[inputs]) * 2 : Math.Abs(layer.m_Algorithm.RequiredXBorder[inputs]));
//                    TempOffsetY[inputs] += (layer.m_Algorithm.InputHeightAtHalfSize[inputs] ? Math.Abs(layer.m_Algorithm.RequiredYBorder[inputs]) * 2 : Math.Abs(layer.m_Algorithm.RequiredYBorder[inputs]));
//                    TempOffsetZ[inputs] += (layer.m_Algorithm.InputDepthAtHalfSize[inputs] ? Math.Abs(layer.m_Algorithm.RequiredZBorder[inputs]) * 2 : Math.Abs(layer.m_Algorithm.RequiredZBorder[inputs]));
                    
                    TempHalfX[inputs] += (layer.m_Algorithm.InputWidthAtHalfSize[inputs] ? 1 : 0);
                    TempHalfY[inputs] += (layer.m_Algorithm.InputHeightAtHalfSize[inputs] ? 1 : 0);
                    TempHalfZ[inputs] += (layer.m_Algorithm.InputDepthAtHalfSize[inputs] ? 1 : 0);
                    TempOffsetX[inputs] += Math.Abs(layer.m_Algorithm.RequiredXBorder[inputs]);
                    TempOffsetY[inputs] += Math.Abs(layer.m_Algorithm.RequiredYBorder[inputs]);
                    TempOffsetZ[inputs] += Math.Abs(layer.m_Algorithm.RequiredZBorder[inputs]);

                    FindMaximumOffsets(input, out OffsetX, out OffsetY, out OffsetZ, out HalfX, out HalfY, out HalfZ);

                    TempOffsetX[inputs] += OffsetX;
                    TempOffsetY[inputs] += OffsetY;
                    TempOffsetZ[inputs] += OffsetZ;
                    TempHalfX[inputs] += HalfX;
                    TempHalfY[inputs] += HalfY;
                    TempHalfZ[inputs] += HalfZ;
                    inputs++;
                }

                for (int count = 0; count < inputs; count++)
                {
                    if (OffsetX < TempOffsetX[count])
                        OffsetX = TempOffsetX[count];
                    if (OffsetY < TempOffsetY[count])
                        OffsetY = TempOffsetY[count];
                    if (OffsetZ < TempOffsetZ[count])
                        OffsetZ = TempOffsetZ[count];
                    if (HalfX < TempHalfX[count])
                        HalfX = TempHalfX[count];
                    if (HalfY < TempHalfY[count])
                        HalfY = TempHalfY[count];
                    if (HalfZ < TempHalfZ[count])
                        HalfZ = TempHalfZ[count]; 
                }
            }
        }

        /// <summary>
        /// Performs the algorithm runtime call using reflection.  This is rather slow,
        /// so we should use a static compiler to prepare world configurations for
        /// release mode (in-game and MMAW).
        /// </summary>
        /// <param name="X">The absolute X value.</param>
        /// <param name="Y">The absolute Y value.</param>
        /// <param name="Z">The absolute Z value.</param>
        /// <param name="arrayWidth">The array width.</param>
        /// <param name="arrayHeight">The array height.</param>
        /// <param name="arrayDepth">The array depth.</param>
        /// <param name="MaxOffsetX">The X offset maximum from all layers.</param>
        /// <param name="MaxOffsetY">The Y offset maximum from all layers.</param>
        /// <param name="MaxOffsetZ">The Z offset maximum from all layers.</param>
        /// <param name="childOffsetX">The X offset from all previous layers.</param>
        /// <param name="childOffsetY">The Y offset from all previous layers.</param>
        /// <param name="childOffsetZ">The Z offset from all previous layers.</param>
        /// <param name="halfInputWidth">If the layer only provides half the output of width.</param>
        /// <param name="halfInputHeight">If the layer only provides half the output of height.</param>
        /// <param name="halfInputDepth">If the layer only provides half the output of depth.</param>
        private dynamic PerformAlgorithmRuntimeCall(long absoluteX, long absoluteY, long absoluteZ,
                                                    int width, int height, int depth,
                                                    int arrayWidth, int arrayHeight, int arrayDepth,
                                                    int MaxOffsetX, int MaxOffsetY, int MaxOffsetZ,
                                                    int childOffsetX, int childOffsetY, int childOffsetZ,
                                                    ref int computations)
        {
            // Check the generate width, height and depth. This actually doesn't work with this system anyway
            /*
            if (arrayWidth != (int)(xTo - xFrom) ||
                arrayHeight != (int)(yTo - yFrom) ||
                arrayDepth != (int)(zTo - zFrom))
                throw new InvalidOperationException("Size generation is out of sync!"); 
            */

            // Get the method for processing cells.
            dynamic algorithm = this.m_Algorithm;
            var processCell = this.m_Algorithm.GetType().GetMethod("ProcessCell");

            dynamic outputArray = Activator.CreateInstance(
                this.m_Algorithm.OutputType.MakeArrayType(),
                (int)(arrayWidth * arrayHeight * arrayDepth));

            // Depending on the argument count, invoke the method appropriately.
            switch (processCell.GetParameters().Length)
            {
                case 14: // 0 inputs
                    {

                    algorithm.Initialize(this);

                        // context, output, x, y, z, i, j, k, width, height, depth, ox, oy, oz                    
                    for (int k = -childOffsetZ; k < (depth - childOffsetZ > 0 ? depth - childOffsetZ : 1); k++)
                        for (int i = -childOffsetX; i < (width - childOffsetX > 0 ? width - childOffsetX : 1); i++)
                            for (int j = -childOffsetY; j < (height - childOffsetY > 0 ? height - childOffsetY : 1); j++)
                                {
                                    algorithm.ProcessCell(this, outputArray, absoluteX + i, absoluteY + j, absoluteZ + k, i, j, k, arrayWidth, arrayHeight, arrayDepth, MaxOffsetX, MaxOffsetY, MaxOffsetZ);
                                    computations += 1;
                                }
                        break;
                    }
                case 18: // 1 input
                    {
                        // context, input, output, x, y, z, i, j, k, width, height, depth, ox, oy, oz
                        if (this.m_Inputs[0] != null)
                        {
                            dynamic inputArray = this.m_Inputs[0].PerformAlgorithmRuntimeCall(
                                (this.m_Algorithm.InputWidthAtHalfSize[0] ? ((absoluteX) < 0 ? (absoluteX - 1) / 2 : (absoluteX) / 2) : absoluteX),
                                (this.m_Algorithm.InputHeightAtHalfSize[0] ? ((absoluteY) < 0 ? (absoluteY - 1) / 2 : (absoluteY) / 2) : absoluteY),
                                (this.m_Algorithm.InputDepthAtHalfSize[0] ? ((absoluteZ) < 0 ? (absoluteZ - 1) / 2 : (absoluteZ) / 2) : absoluteZ),
                                (this.m_Algorithm.InputWidthAtHalfSize[0] ? (width / 2) + this.m_Algorithm.RequiredXBorder[0] * 2 : width + this.m_Algorithm.RequiredXBorder[0] * 2), 
                                (this.m_Algorithm.InputHeightAtHalfSize[0] ? (height / 2) + this.m_Algorithm.RequiredYBorder[0] * 2 : height + this.m_Algorithm.RequiredYBorder[0] * 2), 
                                (this.m_Algorithm.InputDepthAtHalfSize[0] ? (depth / 2) + this.m_Algorithm.RequiredZBorder[0] * 2 : depth + this.m_Algorithm.RequiredZBorder[0] * 2), 
                                arrayWidth, 
                                arrayHeight, 
                                arrayDepth,
                                MaxOffsetX,
                                MaxOffsetY,
                                MaxOffsetZ,
                                (this.m_Algorithm.InputWidthAtHalfSize[0] ? (childOffsetX / 2) + this.m_Algorithm.RequiredXBorder[0] : childOffsetX + this.m_Algorithm.RequiredXBorder[0]),
                                (this.m_Algorithm.InputHeightAtHalfSize[0] ? (childOffsetY / 2) + this.m_Algorithm.RequiredYBorder[0] : childOffsetY + this.m_Algorithm.RequiredYBorder[0]),
                                (this.m_Algorithm.InputDepthAtHalfSize[0] ? (childOffsetZ / 2) + this.m_Algorithm.RequiredZBorder[0] : childOffsetZ + this.m_Algorithm.RequiredZBorder[0]),
                                ref computations);

                            int[] ocx = new int[] {0};
                            int[] ocy = new int[] {0};
                            int[] ocz = new int[] {0};
                        bool xmod = false;
                        bool ymod = false;
                        bool zmod = false;

                        for (int input = 0; input < this.m_Inputs.Length; input++)
                        {
                            if (this.m_Algorithm.InputWidthAtHalfSize[input])
                                xmod = absoluteX % 2 != 0;
                            if (this.m_Algorithm.InputHeightAtHalfSize[input])
                                ymod = absoluteY % 2 != 0;
                            if (this.m_Algorithm.InputDepthAtHalfSize[input])
                                zmod = absoluteZ % 2 != 0;
                        }

                        algorithm.Initialize(this);

                        for (int k = -childOffsetZ; k < (depth - childOffsetZ > 0 ? depth - childOffsetZ : 1); k++)
                            for (int i = -childOffsetX; i < (width - childOffsetX > 0 ? width - childOffsetX : 1); i++)
                                for (int j = -childOffsetY; j < (height - childOffsetY > 0 ? height - childOffsetY : 1); j++)
                                    {
                                        for (int input = 0; input < this.m_Inputs.Length; input++)
                                        {
                                            if (this.m_Algorithm.InputWidthAtHalfSize[input])
                                                ocx[input] = xmod ? (int)(i % 2) : 0;
                                            if (this.m_Algorithm.InputHeightAtHalfSize[input])
                                                ocy[input] = ymod ? (int)(j % 2) : 0;
                                            if (this.m_Algorithm.InputDepthAtHalfSize[input])
                                                ocz[input] = zmod ? (int)(k % 2) : 0;
                                        }
                                
                                        algorithm.ProcessCell(
                                    this,
                                    inputArray,
                                    outputArray,
                                    absoluteX + i,
                                    absoluteY + j,
                                    absoluteZ + k,
                                    i,
                                    j,
                                    k,
                                    arrayWidth,
                                    arrayHeight,
                                    arrayDepth,
                                    MaxOffsetX,
                                    MaxOffsetY,
                                    MaxOffsetZ,
                                    ocx, ocy, ocz);
                                        computations += 1;
                                    }
                        }
                        break;
                    }
                case 19: // 2 inputs
                {
                    // context, inputA, inputB, output, x, y, z, i, j, k, width, height, depth, ox, oy, oz
                    if (this.m_Inputs[0] != null && this.m_Inputs[1] != null)
                    {
                        dynamic inputAArray = this.m_Inputs[0].PerformAlgorithmRuntimeCall(
                            (this.m_Algorithm.InputWidthAtHalfSize[0] ? ((absoluteX) < 0 ? (absoluteX - 1) / 2 : (absoluteX) / 2) : absoluteX),
                            (this.m_Algorithm.InputHeightAtHalfSize[0] ? ((absoluteY) < 0 ? (absoluteY - 1) / 2 : (absoluteY) / 2) : absoluteY),
                            (this.m_Algorithm.InputDepthAtHalfSize[0] ? ((absoluteZ) < 0 ? (absoluteZ - 1) / 2 : (absoluteZ) / 2) : absoluteZ),
                            (this.m_Algorithm.InputWidthAtHalfSize[0] ? (width / 2) + this.m_Algorithm.RequiredXBorder[0] * 2 : width + this.m_Algorithm.RequiredXBorder[0] * 2), 
                            (this.m_Algorithm.InputHeightAtHalfSize[0] ? (height / 2) + this.m_Algorithm.RequiredYBorder[0] * 2 : height + this.m_Algorithm.RequiredYBorder[0] * 2), 
                            (this.m_Algorithm.InputDepthAtHalfSize[0] ? (depth / 2) + this.m_Algorithm.RequiredZBorder[0] * 2 : depth + this.m_Algorithm.RequiredZBorder[0] * 2), 
                            arrayWidth, 
                            arrayHeight, 
                            arrayDepth,
                            MaxOffsetX,
                            MaxOffsetY,
                            MaxOffsetZ,
                            (this.m_Algorithm.InputWidthAtHalfSize[0] ? (childOffsetX / 2) + this.m_Algorithm.RequiredXBorder[0] : childOffsetX + this.m_Algorithm.RequiredXBorder[0]),
                            (this.m_Algorithm.InputHeightAtHalfSize[0] ? (childOffsetY / 2) + this.m_Algorithm.RequiredYBorder[0] : childOffsetY + this.m_Algorithm.RequiredYBorder[0]),
                            (this.m_Algorithm.InputDepthAtHalfSize[0] ? (childOffsetZ / 2) + this.m_Algorithm.RequiredZBorder[0] : childOffsetZ + this.m_Algorithm.RequiredZBorder[0]),
                            ref computations);

                        dynamic inputBArray = this.m_Inputs[1].PerformAlgorithmRuntimeCall(
                            (this.m_Algorithm.InputWidthAtHalfSize[1] ? ((absoluteX) < 0 ? (absoluteX - 1) / 2 : (absoluteX) / 2) : absoluteX),
                            (this.m_Algorithm.InputHeightAtHalfSize[1] ? ((absoluteY) < 0 ? (absoluteY - 1) / 2 : (absoluteY) / 2) : absoluteY),
                            (this.m_Algorithm.InputDepthAtHalfSize[1] ? ((absoluteZ) < 0 ? (absoluteZ - 1) / 2 : (absoluteZ) / 2) : absoluteZ),
                            (this.m_Algorithm.InputWidthAtHalfSize[1] ? (width / 2) + this.m_Algorithm.RequiredXBorder[1] * 2 : width + this.m_Algorithm.RequiredXBorder[1] * 2), 
                            (this.m_Algorithm.InputHeightAtHalfSize[1] ? (height / 2) + this.m_Algorithm.RequiredYBorder[1] * 2 : height + this.m_Algorithm.RequiredYBorder[1] * 2), 
                            (this.m_Algorithm.InputDepthAtHalfSize[1] ? (depth / 2) + this.m_Algorithm.RequiredZBorder[1] * 2 : depth + this.m_Algorithm.RequiredZBorder[1] * 2), 
                            arrayWidth, 
                            arrayHeight, 
                            arrayDepth,
                            MaxOffsetX,
                            MaxOffsetY,
                            MaxOffsetZ,
                            (this.m_Algorithm.InputWidthAtHalfSize[1] ? (childOffsetX / 2) + this.m_Algorithm.RequiredXBorder[1] : childOffsetX + this.m_Algorithm.RequiredXBorder[1]),
                            (this.m_Algorithm.InputHeightAtHalfSize[1] ? (childOffsetY / 2) + this.m_Algorithm.RequiredYBorder[1] : childOffsetY + this.m_Algorithm.RequiredYBorder[1]),
                            (this.m_Algorithm.InputDepthAtHalfSize[1] ? (childOffsetZ / 2) + this.m_Algorithm.RequiredZBorder[1] : childOffsetZ + this.m_Algorithm.RequiredZBorder[1]),
                            ref computations);
                        
                        int[] ocx = new int[] {0};
                        int[] ocy = new int[] {0};
                        int[] ocz = new int[] {0};
                        bool xmod = false;
                        bool ymod = false;
                        bool zmod = false;
                        
                        for (int input = 0; input < this.m_Inputs.Length; input++)
                        {
                            if (this.m_Algorithm.InputWidthAtHalfSize[input])
                                xmod = absoluteX % 2 != 0;
                            if (this.m_Algorithm.InputHeightAtHalfSize[input])
                                ymod = absoluteY % 2 != 0;
                            if (this.m_Algorithm.InputDepthAtHalfSize[input])
                                zmod = absoluteZ % 2 != 0;
                        }
                        
                        algorithm.Initialize(this);
                        
                        for (int k = -childOffsetZ; k < (depth - childOffsetZ > 0 ? depth - childOffsetZ : 1); k++)
                            for (int i = -childOffsetX; i < (width - childOffsetX > 0 ? width - childOffsetX : 1); i++)
                                for (int j = -childOffsetY; j < (height - childOffsetY > 0 ? height - childOffsetY : 1); j++)
                            {
                                for (int input = 0; input < this.m_Inputs.Length; input++)
                                {
                                    if (this.m_Algorithm.InputWidthAtHalfSize[input])
                                        ocx[input] = xmod ? (int)(i % 2) : 0;
                                    if (this.m_Algorithm.InputHeightAtHalfSize[input])
                                        ocy[input] = ymod ? (int)(j % 2) : 0;
                                    if (this.m_Algorithm.InputDepthAtHalfSize[input])
                                        ocz[input] = zmod ? (int)(k % 2) : 0;
                                }
                                
                                algorithm.ProcessCell(
                                    this,
                                    inputAArray,
                                    inputBArray,
                                    outputArray,
                                    absoluteX + i,
                                    absoluteY + j,
                                    absoluteZ + k,
                                    i,
                                    j,
                                    k,
                                    arrayWidth,
                                    arrayHeight,
                                    arrayDepth,
                                    MaxOffsetX,
                                    MaxOffsetY,
                                    MaxOffsetZ,
                                    ocx, ocy, ocz);
                                computations += 1;
                            }
                    }
                    break;
                }
                case 20: // 3 inputs
                {
                    // context, inputA, inputB, inputC, output, x, y, z, i, j, k, width, height, depth, ox, oy, oz
                    if (this.m_Inputs[0] != null && this.m_Inputs[1] != null && this.m_Inputs[2] != null)
                    {
                        dynamic inputAArray = this.m_Inputs[0].PerformAlgorithmRuntimeCall(
                            (this.m_Algorithm.InputWidthAtHalfSize[0] ? ((absoluteX) < 0 ? (absoluteX - 1) / 2 : (absoluteX) / 2) : absoluteX),
                            (this.m_Algorithm.InputHeightAtHalfSize[0] ? ((absoluteY) < 0 ? (absoluteY - 1) / 2 : (absoluteY) / 2) : absoluteY),
                            (this.m_Algorithm.InputDepthAtHalfSize[0] ? ((absoluteZ) < 0 ? (absoluteZ - 1) / 2 : (absoluteZ) / 2) : absoluteZ),
                            (this.m_Algorithm.InputWidthAtHalfSize[0] ? (width / 2) + this.m_Algorithm.RequiredXBorder[0] * 2 : width + this.m_Algorithm.RequiredXBorder[0] * 2), 
                            (this.m_Algorithm.InputHeightAtHalfSize[0] ? (height / 2) + this.m_Algorithm.RequiredYBorder[0] * 2 : height + this.m_Algorithm.RequiredYBorder[0] * 2), 
                            (this.m_Algorithm.InputDepthAtHalfSize[0] ? (depth / 2) + this.m_Algorithm.RequiredZBorder[0] * 2 : depth + this.m_Algorithm.RequiredZBorder[0] * 2), 
                            arrayWidth, 
                            arrayHeight, 
                            arrayDepth,
                            MaxOffsetX,
                            MaxOffsetY,
                            MaxOffsetZ,
                            (this.m_Algorithm.InputWidthAtHalfSize[0] ? (childOffsetX / 2) + this.m_Algorithm.RequiredXBorder[0] : childOffsetX + this.m_Algorithm.RequiredXBorder[0]),
                            (this.m_Algorithm.InputHeightAtHalfSize[0] ? (childOffsetY / 2) + this.m_Algorithm.RequiredYBorder[0] : childOffsetY + this.m_Algorithm.RequiredYBorder[0]),
                            (this.m_Algorithm.InputDepthAtHalfSize[0] ? (childOffsetZ / 2) + this.m_Algorithm.RequiredZBorder[0] : childOffsetZ + this.m_Algorithm.RequiredZBorder[0]),
                            ref computations);
                        
                        dynamic inputBArray = this.m_Inputs[1].PerformAlgorithmRuntimeCall(
                            (this.m_Algorithm.InputWidthAtHalfSize[1] ? ((absoluteX) < 0 ? (absoluteX - 1) / 2 : (absoluteX) / 2) : absoluteX),
                            (this.m_Algorithm.InputHeightAtHalfSize[1] ? ((absoluteY) < 0 ? (absoluteY - 1) / 2 : (absoluteY) / 2) : absoluteY),
                            (this.m_Algorithm.InputDepthAtHalfSize[1] ? ((absoluteZ) < 0 ? (absoluteZ - 1) / 2 : (absoluteZ) / 2) : absoluteZ),
                            (this.m_Algorithm.InputWidthAtHalfSize[1] ? (width / 2) + this.m_Algorithm.RequiredXBorder[1] * 2 : width + this.m_Algorithm.RequiredXBorder[1] * 2), 
                            (this.m_Algorithm.InputHeightAtHalfSize[1] ? (height / 2) + this.m_Algorithm.RequiredYBorder[1] * 2 : height + this.m_Algorithm.RequiredYBorder[1] * 2), 
                            (this.m_Algorithm.InputDepthAtHalfSize[1] ? (depth / 2) + this.m_Algorithm.RequiredZBorder[1] * 2 : depth + this.m_Algorithm.RequiredZBorder[1] * 2), 
                            arrayWidth, 
                            arrayHeight, 
                            arrayDepth,
                            MaxOffsetX,
                            MaxOffsetY,
                            MaxOffsetZ,
                            (this.m_Algorithm.InputWidthAtHalfSize[1] ? (childOffsetX / 2) + this.m_Algorithm.RequiredXBorder[1] : childOffsetX + this.m_Algorithm.RequiredXBorder[1]),
                            (this.m_Algorithm.InputHeightAtHalfSize[1] ? (childOffsetY / 2) + this.m_Algorithm.RequiredYBorder[1] : childOffsetY + this.m_Algorithm.RequiredYBorder[1]),
                            (this.m_Algorithm.InputDepthAtHalfSize[1] ? (childOffsetZ / 2) + this.m_Algorithm.RequiredZBorder[1] : childOffsetZ + this.m_Algorithm.RequiredZBorder[1]),
                            ref computations);

                        dynamic inputCArray = this.m_Inputs[2].PerformAlgorithmRuntimeCall(
                            (this.m_Algorithm.InputWidthAtHalfSize[2] ? ((absoluteX) < 0 ? (absoluteX - 1) / 2 : (absoluteX) / 2) : absoluteX),
                            (this.m_Algorithm.InputHeightAtHalfSize[2] ? ((absoluteY) < 0 ? (absoluteY - 1) / 2 : (absoluteY) / 2) : absoluteY),
                            (this.m_Algorithm.InputDepthAtHalfSize[2] ? ((absoluteZ) < 0 ? (absoluteZ - 1) / 2 : (absoluteZ) / 2) : absoluteZ),
                            (this.m_Algorithm.InputWidthAtHalfSize[2] ? (width / 2) + this.m_Algorithm.RequiredXBorder[2] * 2 : width + this.m_Algorithm.RequiredXBorder[2] * 2), 
                            (this.m_Algorithm.InputHeightAtHalfSize[2] ? (height / 2) + this.m_Algorithm.RequiredYBorder[2] * 2 : height + this.m_Algorithm.RequiredYBorder[2] * 2), 
                            (this.m_Algorithm.InputDepthAtHalfSize[2] ? (depth / 2) + this.m_Algorithm.RequiredZBorder[2] * 2 : depth + this.m_Algorithm.RequiredZBorder[2] * 2), 
                            arrayWidth, 
                            arrayHeight, 
                            arrayDepth,
                            MaxOffsetX,
                            MaxOffsetY,
                            MaxOffsetZ,
                            (this.m_Algorithm.InputWidthAtHalfSize[2] ? (childOffsetX / 2) + this.m_Algorithm.RequiredXBorder[2] : childOffsetX + this.m_Algorithm.RequiredXBorder[2]),
                            (this.m_Algorithm.InputHeightAtHalfSize[2] ? (childOffsetY / 2) + this.m_Algorithm.RequiredYBorder[2] : childOffsetY + this.m_Algorithm.RequiredYBorder[2]),
                            (this.m_Algorithm.InputDepthAtHalfSize[2] ? (childOffsetZ / 2) + this.m_Algorithm.RequiredZBorder[2] : childOffsetZ + this.m_Algorithm.RequiredZBorder[2]),
                            ref computations);

                        int[] ocx = new int[] {0};
                        int[] ocy = new int[] {0};
                        int[] ocz = new int[] {0};
                        bool xmod = false;
                        bool ymod = false;
                        bool zmod = false;
                        
                        for (int input = 0; input < this.m_Inputs.Length; input++)
                        {
                            if (this.m_Algorithm.InputWidthAtHalfSize[input])
                                xmod = absoluteX % 2 != 0;
                            if (this.m_Algorithm.InputHeightAtHalfSize[input])
                                ymod = absoluteY % 2 != 0;
                            if (this.m_Algorithm.InputDepthAtHalfSize[input])
                                zmod = absoluteZ % 2 != 0;
                        }
                        
                        algorithm.Initialize(this);
                        
                        for (int k = -childOffsetZ; k < (depth - childOffsetZ > 0 ? depth - childOffsetZ : 1); k++)
                            for (int i = -childOffsetX; i < (width - childOffsetX > 0 ? width - childOffsetX : 1); i++)
                                for (int j = -childOffsetY; j < (height - childOffsetY > 0 ? height - childOffsetY : 1); j++)
                            {
                                for (int input = 0; input < this.m_Inputs.Length; input++)
                                {
                                    if (this.m_Algorithm.InputWidthAtHalfSize[input])
                                        ocx[input] = xmod ? (int)(i % 2) : 0;
                                    if (this.m_Algorithm.InputHeightAtHalfSize[input])
                                        ocy[input] = ymod ? (int)(j % 2) : 0;
                                    if (this.m_Algorithm.InputDepthAtHalfSize[input])
                                        ocz[input] = zmod ? (int)(k % 2) : 0;
                                }
                                
                                algorithm.ProcessCell(
                                    this,
                                    inputAArray,
                                    inputBArray,
                                    inputCArray,
                                    outputArray,
                                    absoluteX + i,
                                    absoluteY + j,
                                    absoluteZ + k,
                                    i,
                                    j,
                                    k,
                                    arrayWidth,
                                    arrayHeight,
                                    arrayDepth,
                                    MaxOffsetX,
                                    MaxOffsetY,
                                    MaxOffsetZ,
                                    ocx, ocy, ocz);
                                computations += 1;
                            }
                    }
                    break;
                }
                default:
                    // FIXME!
                    throw new NotImplementedException();
            }

            return outputArray;
        }

        /// <summary>
        /// Generates data using the current algorithm.
        /// </summary>
        public dynamic GenerateData(long x, long y, long z, int width, int height, int depth, out int computations)
        {
            // Initialize the computation count.
            computations = 0;

            // Just replicate this into the CompiledLayer system
            int MaxOffsetX = 0;
            int MaxOffsetY = 0; 
            int MaxOffsetZ = 0;
            int MaxHalfX = 0;
            int MaxHalfY = 0;
            int MaxHalfZ = 0;

            FindMaximumOffsets(this, out MaxOffsetX, out MaxOffsetY, out MaxOffsetZ, out MaxHalfX, out MaxHalfY, out MaxHalfZ);
            /*
            // Work out the maximum bounds of the array.
            var ranged = new RangedLayer(this);
            Expression ix, iy, iz, iwidth, iheight, idepth, iouterx, ioutery, iouterz;
            RangedLayer.FindMaximumBounds(ranged, out ix, out iy, out iz, out iwidth, out iheight, out idepth, out iouterx, out ioutery, out iouterz);

            // Perform the algorithm calculations.
            int resultOffsetX = (int)RangedLayer.EvaluateExpression(ix, new Dictionary<string, object> { { "x", x } }).Value;
            int resultOffsetY = (int)RangedLayer.EvaluateExpression(iy, new Dictionary<string, object> { { "y", y } }).Value;
            int resultOffsetZ = (int)RangedLayer.EvaluateExpression(iz, new Dictionary<string, object> { { "z", z } }).Value;
            int resultWidth = (int)RangedLayer.EvaluateExpression(iwidth, new Dictionary<string, object> { { "x", x }, { "width", width } }).Value;
            int resultHeight = (int)RangedLayer.EvaluateExpression(iheight, new Dictionary<string, object> { { "y", y }, { "height", height } }).Value;
            int resultDepth = (int)RangedLayer.EvaluateExpression(idepth, new Dictionary<string, object> { { "z", z }, { "depth", depth } }).Value;
            resultWidth -= resultOffsetX;  // Sometimes the width doesn't encapsulate the whole region we need
            resultHeight -= resultOffsetY; // since the upper layers require the lower layers to be filled in
            resultDepth -= resultOffsetZ;  // the offset areas.
            */

            /*
            int resultWidth = width;
            int resultHeight = height;
            int resultDepth = depth;
            int OffsetX = 0;
            int OffsetY = 0;
            int OffsetZ = 0;
*/
            // change generate width to total width generated by ranged layer
            // change offset x to total offset x generated my ranged layer
            // pass this offset x + offsetx to be offest from all childs
            // var relativeX = i + generateOffsetX - pregenOffsetX;
            // var relativeX = i + TOTALOFFSETX - this.RequiredXBorder + OffsetX

            // Need MaxOffsetX/Y/Z (derive total width/l/d from that)
            // In runtime do X + MaxOffsetX - ChildOffsetX
            // Actually added MaxOffsetX onto the I value unnessecarily
            // This causes the X value to shift with every change
            // Need to remove MaxOffsetX so I've just made X = X - MaxOffsetX

            // Actually for compiled you can just have ParentOffsetX
            // Increment that every layer
            // Reset it at the start of each for loop 
            // then add it up over each layer
            // For ( reset if () add if () add ))
            // Then it will be I + ParentOffsetX

            // Going to have to make the halfinputwidth
            // Can make this calculated by finding the total then subtracting its parents
            // for the compiled layer.

            // ix, etc = input of input layer
            // ix = x - offset (relativeX) = xFrom
            // iwidth = width * offsetX * 2.
            // iouterx = xTo

            int arrayWidth = width + MaxOffsetX * 2;
            int arrayHeight = height + MaxOffsetY * 2;
            int arrayDepth = depth + MaxOffsetZ * 2;

            // TODO: Optimization List
            // 1) Make it check Is2DOnly == true, make Z = 0, Depth = 1;
            // 2) Fix array size to be closer to the actual generation size
            //    Aka account for halving
            //    Can completely redo system, work out array width then max offset off of that 
            //    Otherwise halves get wierd with different width
            //    Because then if the offset is bigger than the half it gets funky
            // TODO: Addition List
            // 1) Make a system for layers that can generate just 1 value
            //    Not sure if this is needed?

            dynamic resultArray = this.PerformAlgorithmRuntimeCall(
                x,
                y,
                z,
                width,
                height,
                depth,
                arrayWidth,
                arrayHeight,
                arrayDepth,
                MaxOffsetX, 
                MaxOffsetY, 
                MaxOffsetZ,
                0, 0, 0,
                ref computations);

            // Copy the result into a properly sized array.
            dynamic correctArray = Activator.CreateInstance(
                this.m_Algorithm.OutputType.MakeArrayType(),
                (int)(width * height * depth));

            for (var k = 0; k < depth; k++)
                for (var i = 0; i < width; i++)
                    for (var j = 0; j < height; j++)
                        correctArray[i + j * width + k * width * height] =
                            resultArray[(i + MaxOffsetX) +
                            (j + MaxOffsetY) * arrayWidth +
                            (k + MaxOffsetZ) * arrayWidth * arrayHeight];

            // Return the result.
            return correctArray;
        }
        
        #region Randomness
        
        private long m_Seed;
        
        /// <summary>
        /// The world seed.
        /// </summary>
        public long Seed
        {
            get
            {
                return this.m_Seed;
            }
            set
            {
                this.m_Seed = value;
            }
        }
        
        /// <summary>
        /// Returns a random positive integer between the specified 0 and
        /// the exclusive end value.
        /// </summary>
        public int GetRandomRange(long x, long y, long z, int end, long modifier = 0)
        {
            return AlgorithmUtility.GetRandomRange(this.Seed, x, y, z, end, modifier);
        }
        
        /// <summary>
        /// Returns a random positive integer between the specified inclusive start
        /// value and the exclusive end value.
        /// </summary>
        public int GetRandomRange(long x, long y, long z, int start, int end, long modifier)
        {
            return AlgorithmUtility.GetRandomRange(this.Seed, x, y, z, start, end, modifier);
        }
        
        /// <summary>
        /// Returns a random integer over the range of valid integers based
        /// on the provided X and Y position, and the specified modifier.
        /// </summary>
        public int GetRandomInt(long x, long y, long z, long modifier = 0)
        {
            return AlgorithmUtility.GetRandomInt(this.Seed, x, y, z, modifier);
        }
        
        /// <summary>
        /// Returns a random long integer over the range of valid long integers based
        /// on the provided X and Y position, and the specified modifier.
        /// </summary>
        public long GetRandomLong(long x, long y, long z, long modifier = 0)
        {
            return AlgorithmUtility.GetRandomLong(this.Seed, x, y, z, modifier);
        }
        
        /// <summary>
        /// Returns a random double between the range of 0.0 and 1.0 based on
        /// the provided X and Y position, and the specified modifier.
        /// </summary>
        public double GetRandomDouble(long x, long y, long z, long modifier = 0)
        {
            return AlgorithmUtility.GetRandomDouble(this.Seed, x, y, z, modifier);
        }

        #endregion

        #region Other
        
        /// <summary>
        /// Smoothes the specified data according to smoothing logic.  Apparently
        /// inlining this functionality causes the algorithms to run slower, so we
        /// leave this function on it's own.
        /// </summary>
        public int Smooth(bool isFuzzy, long x, long y, int northValue, int southValue, int westValue, int eastValue, int southEastValue, int currentValue, long i, long j, long ox, long oy, long rw, int[] parent)
        {
            // Parent-based Smoothing
            int selected = 0;
            
            if (x % 2 == 0)
            {
                if (y % 2 == 0)
                {
                    return currentValue;
                }
                else
                {
                    selected = this.GetRandomRange(x, y, 0, 2);
                    switch (selected)
                    {
                        case 0:
                            return currentValue;
                        case 1:
                            return southValue;
                    }
                }
            }
            else
            {
                if (y % 2 == 0)
                {
                    selected = this.GetRandomRange(x, y, 0, 2);
                    switch (selected)
                    {
                        case 0:
                            return currentValue;
                        case 1:
                            return eastValue;
                    }
                }
                else
                {
                    if (!isFuzzy)
                    {
                        selected = this.GetRandomRange(x, y, 0, 3);
                        switch (selected)
                        {
                            case 0:
                                return currentValue;
                            case 1:
                                return southValue;
                            case 2:
                                return eastValue;
                        }
                    }
                    else
                    {
                        selected = this.GetRandomRange(x, y, 0, 4);
                        switch (selected)
                        {
                            case 0:
                                return currentValue;
                            case 1:
                                return southValue;
                            case 2:
                                return eastValue;
                            case 3:
                                return southEastValue;
                        }
                    }
                }
            }
            
            // Select one of the four options if we couldn't otherwise
            // determine a value.
            selected = this.GetRandomRange(x, y, 0, 4);
            
            switch (selected)
            {
                case 0:
                    return northValue;
                case 1:
                    return southValue;
                case 2:
                    return eastValue;
                case 3:
                    return westValue;
            }
            
            throw new InvalidOperationException();
        }
        
        #endregion

    }
}

