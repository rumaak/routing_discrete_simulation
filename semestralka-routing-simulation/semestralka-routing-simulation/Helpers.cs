// Discrete simulation of routing
// Jan Ruman, 1st year of study
// Summer term, 2019 / 2020
// NPRG031

using System;
using System.Collections.Generic;
using System.Text;

namespace semestralka_routing_simulation
{
    /// <summary>
    /// Contains universaly useful functions.
    /// </summary>
    static class Helpers
    {
        /// <summary>
        /// Return a random ulong with uniform distribution.
        /// </summary>
        public static ulong GetNextUniform(ulong maxValue, Random rnd)
        {
            byte[] buffer = new byte[sizeof(ulong)];
            rnd.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0) % (maxValue + 1);
        }

        /// <summary>
        /// Return a random ulong with Gaussian distribution (but discrete).
        /// </summary>
        /// <remarks>
        /// <para>There is no choice for user to alter distributions parameters - mean and variance
        /// are hardcoded to <c>maxTime / 2</c> and <c>maxTime / 4</c> respectively.
        /// These parameters correspond to probability density being about 8 times greater
        /// in point <c>maxTime / 2</c> compared to points at the ends of distribution, while
        /// still leaving only minimum of probability left for points out of bounds (ie smaller
        /// than 0 and bigger than <c>maxTime</c>. </para>
        /// <para>Also, I wanted to use Poisson, but factorial doesn't make good friends with
        /// big numbers.</para>
        /// </remarks>
        public static ulong GetNextGaussian(ulong maxTime, Random rnd)
        {
            // When generated number gets out of bounds, regenerate
            double randNormal = -1;
            while (randNormal < 0 || randNormal > maxTime)
            {
                double mean = ((double)maxTime) / 2;
                double std = ((double)maxTime) / 4;

                double u1 = 1.0 - rnd.NextDouble();
                double u2 = 1.0 - rnd.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                randNormal = mean + std * randStdNormal;
            }

            return (ulong)randNormal;
        }
    }
}
