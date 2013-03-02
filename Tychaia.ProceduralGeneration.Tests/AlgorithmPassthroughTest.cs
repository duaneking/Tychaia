//
// This source code is licensed in accordance with the licensing outlined
// on the main Tychaia website (www.tychaia.com).  Changes to the
// license on the website apply retroactively.
//
using System;
using NUnit.Framework;

namespace Tychaia.ProceduralGeneration.Tests
{
    [TestFixture()]
    public class AlgorithmPassthroughTest
    {
        [Test()]
        public void TestRuntimePassthroughNoBorder()
        {
            this.TestRuntimePassthrough(0, 0);
        }
        
        [Test()]
        public void TestRuntimePassthroughOnePositiveBorder()
        {
            this.TestRuntimePassthrough(1, 1);
        }
        
        [Test()]
        public void TestRuntimePassthroughOneCombinationBorder()
        {
            this.TestRuntimePassthrough(0, 1);
            this.TestRuntimePassthrough(1, 0);
            this.TestRuntimePassthrough(1, 1);
        }
        
        [Test()]
        public void TestRuntimePassthroughSmallPositiveBorder()
        {
            this.TestRuntimePassthrough(2, 2);
        }
        
        [Test()]
        public void TestRuntimePassthroughMediumPositiveBorder()
        {
            this.TestRuntimePassthrough(5, 5);
        }
        
        [Test()]
        public void TestRuntimePassthroughLargePositiveBorder()
        {
            this.TestRuntimePassthrough(20, 20);
        }
        
        [Test()]
        public void TestRuntimePassthroughUnevenPositiveBorder()
        {
            this.TestRuntimePassthrough(4, 7);
        }

        private void TestRuntimePassthrough(int xBorder, int yBorder)
        {
            int computations1, computations2;
            int width = 16, height = 16, depth = 16;
            var gradient = new RuntimeLayer(new AlgorithmGradientInitial());
            var passthrough = new RuntimeLayer(new AlgorithmPassthrough { XBorder = xBorder, YBorder = yBorder });
            passthrough.SetInput(0, gradient);
            var i1 = gradient.GenerateData(0, 0, 0, width, height, depth, out computations1);
            var i2 = passthrough.GenerateData(0, 0, 0, width, height, depth, out computations2);

            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    for (var z = 0; z < depth; z++)
                        Assert.AreEqual(i1[x + y * width + z * width * height], i2[x + y * width + z * width * height], "Value differs in passthrough (" + xBorder + ", " + yBorder + ").");
        }
    }
}
