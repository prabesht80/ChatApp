using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer
{
    internal class Key
    {
        private readonly int n;
        private readonly int e;
        private int d;

        internal Key(int primeOne, int primeTwo, int primeThree, List<int> primes)
        {
            n = primeOne * primeTwo * primeThree;
            var phi = (primeOne - 1) * (primeTwo - 1) * (primeThree - 1);
            var end = primes.Count - 1;
            var start = end / 4;
            var random = new Random();
            do
            {
                do
                {
                    e = primes[random.Next(start, end)];
                } while (e == primeOne || e == primeTwo || e == primeThree);
            } while (!IsFoundD(phi));
            Console.WriteLine("Public Key: (e, n) = (" + e + ", " + n + ")");
        }

        private bool IsFoundD(int phi)
        {
            for (var i = phi - 1; i > 1; i--)
            {
                var mul = BigInteger.Multiply(e, i);
                var result = BigInteger.Remainder(mul, phi);
                if (result.Equals(1))
                {
                    d = i;
                    Console.WriteLine("Private Key: (d, n) = (" + d + ", " + n + ")");
                    return true;
                }
            }
            return false;
        }

        internal int[] Encrypt(string message)
        {
            var charArray = message.ToCharArray();
            var array = new int[charArray.Length];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = (int)BigInteger.ModPow(charArray[i], e, n);
            }
            return array;
        }

        internal string Decrypt(int[] cyphertext)
        {
            var array = new char[cyphertext.Length];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = (char)BigInteger.ModPow(cyphertext[i], d, n);
            }
            return new string(array);
        }


    }

    internal class Primes
    {
        private const int MaxValue = 250;
        //private readonly bool[] isPrime = new bool[MaxValue + 1];
        private readonly List<int> primes = new List<int>();

        internal Primes()
        {
            for (var i = 2; i <= MaxValue; i++)
            {
                if (IsPrime(i, MaxValue))
                {
                    primes.Add(i);
                }
            }
        }


        //miller rabin test 
        static bool IsPrime(long n, int iteration)
        {
            if (n == 0 || n == 1)
                return false;
            if (n == 2)
                return true;
            if (n % 2 == 0)
                return false;

            long s = n - 1;
            while (s % 2 == 0)
                s /= 2;

            Random rand = new Random();
            for (int i = 0; i < iteration; i++)
            {
                long r = Math.Abs(rand.Next((int)s, (int)(n - 1)));
                long x = ModularExponentiation(r, s, n);


                while (s != n - 1 && x != 1 && x != n - 1)
                {
                    x = (x * x) % n;
                    s *= 2;
                }

                if (x != n - 1 && s % 2 == 0)
                    return false;
            }
            return true;
        }

        static long ModularExponentiation(long baseValue, long exponent, long modulus)
        {
            long result = 1;
            while (exponent > 0)
            {
                if (exponent % 2 == 1)
                    result = (result * baseValue) % modulus;
                baseValue = (baseValue * baseValue) % modulus;
                exponent /= 2;
            }
            return result;
        }


        internal Key GetKey()
        {
            var end = primes.Count - 1;
            var start = end / 4;
            var random = new Random();
            var primeOne = primes[random.Next(start, end)];
            var primeTwo = primes[random.Next(start, end)];
            var primeThree = primes[random.Next(start, end)];

            while ((primeTwo == primeOne) || (primeThree == primeOne))
            {
                primeOne = primes[random.Next(start, end)];
            }
            while ((primeTwo == primeOne) || (primeThree == primeTwo))
            {
                primeTwo = primes[random.Next(start, end)];

            }
            return new Key(primeOne, primeTwo, primeThree, primes);
        }
    }
}
