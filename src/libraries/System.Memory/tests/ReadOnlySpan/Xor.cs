// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Numerics;
using System.Runtime.InteropServices;
using Xunit;

namespace System.SpanTests
{
    public static partial class ReadOnlySpanTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(15)]
        [InlineData(16)]
        [InlineData(17)]
        [InlineData(31)]
        [InlineData(32)]
        [InlineData(33)]
        [InlineData(63)]
        [InlineData(64)]
        [InlineData(65)]
        [InlineData(127)]
        [InlineData(128)]
        [InlineData(129)]
        [InlineData(255)]
        [InlineData(256)]
        [InlineData(257)]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1025)]
        [InlineData(4097)]
        public static void Xor_NumericTypes_MatchesElementwiseOperator(int length)
        {
            TestXorNumeric<byte>(length);
            TestXorNumeric<sbyte>(length);
            TestXorNumeric<short>(length);
            TestXorNumeric<ushort>(length);
            TestXorNumeric<int>(length);
            TestXorNumeric<uint>(length);
            TestXorNumeric<long>(length);
            TestXorNumeric<ulong>(length);
            TestXorNumeric<nint>(length);
            TestXorNumeric<nuint>(length);
            TestXorNumeric<Int128>(length);
            TestXorNumeric<UInt128>(length);
            TestXorNumeric<BigInteger>(length);
            TestXorNumeric<Half>(length);
            TestXorNumeric<float>(length);
            TestXorNumeric<double>(length);
        }

        private static void TestXorNumeric<T>(int length) where T : INumberBase<T>, IBitwiseOperators<T, T, T>
        {
            T[] x = new T[length + 8];
            T[] y = new T[length + 8];
            Random random = new(42);
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = T.CreateTruncating(random.NextInt64(long.MinValue, long.MaxValue));
                y[i] = T.CreateTruncating(random.NextInt64(long.MinValue, long.MaxValue));
            }

            foreach (int offset in new[] { 0, 1, 3 })
            {
                T[] originalX = (T[])x.Clone();
                T[] originalY = (T[])y.Clone();
                T[] expected = (T[])x.Clone();
                T[] actual = (T[])x.Clone();
                for (int i = offset; i < offset + length; i++)
                {
                    expected[i] = x[i] ^ y[i];
                }

                // Leave extra space in the destination; it and both inputs must remain unchanged.
                ReadOnlySpan<T> source = x.AsSpan(offset, length);
                source.Xor(y.AsSpan(offset, length), actual.AsSpan(offset));
                Assert.Equal(expected, actual);
                Assert.Equal(originalX, x);
                Assert.Equal(originalY, y);

                actual = (T[])x.Clone();
                MemoryExtensions.Xor<T>(actual.AsSpan(offset, length), y.AsSpan(offset, length), actual.AsSpan(offset));
                Assert.Equal(expected, actual);

                actual = (T[])y.Clone();
                for (int i = 0; i < actual.Length; i++)
                {
                    if (i < offset || i >= offset + length)
                        expected[i] = y[i];
                }
                MemoryExtensions.Xor<T>(x.AsSpan(offset, length), actual.AsSpan(offset, length), actual.AsSpan(offset));
                Assert.Equal(expected, actual);

                actual = (T[])x.Clone();
                expected = (T[])x.Clone();
                for (int i = offset; i < offset + length; i++)
                    expected[i] = x[i] ^ x[i];
                MemoryExtensions.Xor<T>(actual.AsSpan(offset, length), actual.AsSpan(offset, length), actual.AsSpan(offset));
                Assert.Equal(expected, actual);

                T scalar = y[offset];
                expected = (T[])x.Clone();
                for (int i = offset; i < offset + length; i++)
                    expected[i] = x[i] ^ scalar;
                actual = (T[])x.Clone();
                source.Xor(scalar, actual.AsSpan(offset));
                Assert.Equal(expected, actual);
                Assert.Equal(originalX, x);

                actual = (T[])x.Clone();
                MemoryExtensions.Xor<T>(actual.AsSpan(offset, length), scalar, actual.AsSpan(offset));
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public static void Xor_Empty()
        {
            MemoryExtensions.Xor<int>(default, ReadOnlySpan<int>.Empty, default);
            MemoryExtensions.Xor<int>(default, 42, default);
        }

        [Fact]
        public static void Xor_InvalidLengths_DoNotWrite()
        {
            int[] destination = { 91, 92, 93 };
            int[] original = (int[])destination.Clone();
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<int>(new int[2], new int[3], destination));
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<int>(new int[3], new int[2], destination));
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<int>(ReadOnlySpan<int>.Empty, new int[1], destination));
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<int>(new int[4], new int[4], destination));
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<int>(new int[4], 42, destination));
            Assert.Equal(original, destination);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        public static void Xor_PartialOverlap_DoNotWrite(int inputOffset, int destinationOffset)
        {
            int[] buffer = new int[129];
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = i;
            int[] original = (int[])buffer.Clone();
            int[] other = new int[128];

            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<int>(buffer.AsSpan(inputOffset, 128), other, buffer.AsSpan(destinationOffset, 128)));
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<int>(other, buffer.AsSpan(inputOffset, 128), buffer.AsSpan(destinationOffset, 128)));
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<int>(buffer.AsSpan(inputOffset, 128), 42, buffer.AsSpan(destinationOffset, 128)));
            Assert.Equal(original, buffer);
        }

        [Fact]
        public static void Xor_OverlapWithUnusedDestinationTail_Throws()
        {
            int[] buffer = new int[12];
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<int>(buffer.AsSpan(8, 4), 42, buffer));
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<int>(new int[4], buffer.AsSpan(8, 4), buffer));
        }

        [Fact]
        public static void Xor_InputsMayOverlap()
        {
            int[] buffer = new int[130];
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = i;
            int[] actual = new int[129];
            MemoryExtensions.Xor<int>(buffer.AsSpan(0, 129), buffer.AsSpan(1, 129), actual);
            for (int i = 0; i < actual.Length; i++)
                Assert.Equal(i ^ (i + 1), actual[i]);
        }

        [Fact]
        public static void Xor_UnalignedByteSlices()
        {
            byte[] source = new byte[1029];
            byte[] destination = new byte[1031];
            new Random(42).NextBytes(source);
            destination.AsSpan().Fill(0xCC);
            byte[] original = (byte[])source.Clone();
            ReadOnlySpan<int> input = MemoryMarshal.Cast<byte, int>(source.AsSpan(1, 1024));
            Span<int> output = MemoryMarshal.Cast<byte, int>(destination.AsSpan(3, 1024));
            MemoryExtensions.Xor(input, 0x12345678, output);
            for (int i = 0; i < input.Length; i++)
                Assert.Equal(input[i] ^ 0x12345678, output[i]);
            Assert.Equal(original, source);
            Assert.Equal(-1, destination.AsSpan(0, 3).IndexOfAnyExcept((byte)0xCC));
            Assert.Equal(-1, destination.AsSpan(1027).IndexOfAnyExcept((byte)0xCC));
        }

        [Fact]
        public static void Xor_FloatingPoint_PreservesBits()
        {
            int[] bits = { 0, int.MinValue, 0x7F800000, unchecked((int)0xFF800000), 0x7FC01234, 0x7F801234, 1, -1 };
            float[] source = new float[129];
            float[] mask = new float[source.Length];
            float[] destination = new float[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                source[i] = BitConverter.Int32BitsToSingle(bits[i % bits.Length]);
                mask[i] = BitConverter.Int32BitsToSingle(bits[(i + 3) % bits.Length]);
            }
            MemoryExtensions.Xor<float>(source, mask, destination);
            for (int i = 0; i < source.Length; i++)
                Assert.Equal(BitConverter.SingleToInt32Bits(source[i]) ^ BitConverter.SingleToInt32Bits(mask[i]), BitConverter.SingleToInt32Bits(destination[i]));
            MemoryExtensions.Xor<float>(source, mask[0], destination);
            for (int i = 0; i < source.Length; i++)
                Assert.Equal(BitConverter.SingleToInt32Bits(source[i]) ^ BitConverter.SingleToInt32Bits(mask[0]), BitConverter.SingleToInt32Bits(destination[i]));
        }

        [Fact]
        public static void Xor_CustomReferenceType_UsesOperator()
        {
            XorValue[] source = new XorValue[129];
            XorValue[] other = new XorValue[source.Length];
            XorValue[] destination = new XorValue[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                source[i] = new(i);
                other[i] = new(i + 1);
            }
            MemoryExtensions.Xor<XorValue>(source, other, destination);
            for (int i = 0; i < source.Length; i++)
                Assert.Equal(i + i + 2, destination[i].Value);
            MemoryExtensions.Xor<XorValue>(source, new XorValue(42), source);
            for (int i = 0; i < source.Length; i++)
                Assert.Equal(i + 43, source[i].Value);
        }

        private sealed class XorValue(int value) : IBitwiseOperators<XorValue, XorValue, XorValue>
        {
            public int Value { get; } = value;
            public static XorValue operator ^(XorValue x, XorValue y) => new(x.Value + y.Value + 1);
            public static XorValue operator &(XorValue x, XorValue y) => throw new NotSupportedException();
            public static XorValue operator |(XorValue x, XorValue y) => throw new NotSupportedException();
            public static XorValue operator ~(XorValue x) => throw new NotSupportedException();
        }
    }
}
