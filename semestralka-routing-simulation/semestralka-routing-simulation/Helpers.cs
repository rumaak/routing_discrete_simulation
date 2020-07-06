using System;
using System.Collections.Generic;
using System.Text;

namespace semestralka_routing_simulation
{
    static class Helpers
    {
        public static ulong getNextUniform(ulong maxValue, Random rnd)
        {
            byte[] buffer = new byte[sizeof(ulong)];
            rnd.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0) % (maxValue + 1);
        }

        // Normal distribution with mean = maxTime / 2 and std = maxTime / 4
        public static ulong getNextGaussian(ulong maxTime, Random rnd)
        {
            // When generated numbers gets out of bounds, regenerate
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
