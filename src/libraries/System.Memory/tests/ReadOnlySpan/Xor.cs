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
        [InlineData(511)]
        [InlineData(512)]
        [InlineData(513)]
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

        [Theory]
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
        [InlineData(511)]
        [InlineData(512)]
        [InlineData(513)]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1025)]
        public static void Xor_ScalarInPlace_AllByteOffsets(int length)
        {
            const int Mask = unchecked((int)0xA1B2C3D4);
            Random random = new(42);
            for (int offset = 0; offset < 64; offset++)
            {
                byte[] actual = new byte[length * sizeof(int) + 128];
                random.NextBytes(actual);
                byte[] expected = (byte[])actual.Clone();
                Span<int> expectedValues = MemoryMarshal.Cast<byte, int>(expected.AsSpan(offset, length * sizeof(int)));
                for (int i = 0; i < expectedValues.Length; i++)
                {
                    expectedValues[i] ^= Mask;
                }

                // Includes byte-misaligned int spans which cannot be aligned by advancing
                // whole elements, as well as every possible vector-alignment prefix.
                Span<int> values = MemoryMarshal.Cast<byte, int>(actual.AsSpan(offset, length * sizeof(int)));
                MemoryExtensions.Xor<int>(values, Mask, values);
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
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
        [InlineData(511)]
        [InlineData(512)]
        [InlineData(513)]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1025)]
        public static void Xor_IndependentByteOffsets_PreservesInputsAndGuards(int length)
        {
            const int Mask = unchecked((int)0xA1B2C3D4);
            Random random = new(42);
            for (int xOffset = 0; xOffset < 64; xOffset++)
            {
                int yOffset = (17 * xOffset + 7) % 64;
                int destinationOffset = (29 * xOffset + 11) % 64;
                byte[] x = new byte[length * sizeof(int) + 128];
                byte[] y = new byte[x.Length];
                byte[] destination = new byte[x.Length];
                random.NextBytes(x);
                random.NextBytes(y);
                random.NextBytes(destination);
                byte[] originalX = (byte[])x.Clone();
                byte[] originalY = (byte[])y.Clone();
                ReadOnlySpan<int> xValues = MemoryMarshal.Cast<byte, int>(x.AsSpan(xOffset, length * sizeof(int)));
                ReadOnlySpan<int> yValues = MemoryMarshal.Cast<byte, int>(y.AsSpan(yOffset, length * sizeof(int)));

                // Vary each span's byte alignment independently and include unused destination
                // elements. Compare whole buffers to detect writes to inputs or surrounding guards.
                byte[] expected = (byte[])destination.Clone();
                Span<int> expectedValues = MemoryMarshal.Cast<byte, int>(expected.AsSpan(destinationOffset));
                for (int i = 0; i < length; i++)
                    expectedValues[i] = xValues[i] ^ yValues[i];
                MemoryExtensions.Xor(xValues, yValues, MemoryMarshal.Cast<byte, int>(destination.AsSpan(destinationOffset)));
                Assert.Equal(expected, destination);

                for (int i = 0; i < length; i++)
                    expectedValues[i] = xValues[i] ^ Mask;
                MemoryExtensions.Xor(xValues, Mask, MemoryMarshal.Cast<byte, int>(destination.AsSpan(destinationOffset)));
                Assert.Equal(expected, destination);
                Assert.Equal(originalX, x);
                Assert.Equal(originalY, y);

                // The destination can also start at either input, including byte-misaligned spans.
                foreach (bool destinationIsX in new[] { true, false })
                {
                    byte[] actual = (byte[])(destinationIsX ? x : y).Clone();
                    int offset = destinationIsX ? xOffset : yOffset;
                    expected = (byte[])actual.Clone();
                    expectedValues = MemoryMarshal.Cast<byte, int>(expected.AsSpan(offset));
                    for (int i = 0; i < length; i++)
                        expectedValues[i] = xValues[i] ^ yValues[i];
                    ReadOnlySpan<int> aliasedInput = MemoryMarshal.Cast<byte, int>(actual.AsSpan(offset, length * sizeof(int)));
                    MemoryExtensions.Xor(destinationIsX ? aliasedInput : xValues, destinationIsX ? yValues : aliasedInput,
                        MemoryMarshal.Cast<byte, int>(actual.AsSpan(offset)));
                    Assert.Equal(expected, actual);
                }
                Assert.Equal(originalX, x);
                Assert.Equal(originalY, y);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void Xor_ByteAndHalf_TailsOffsetsAndAliases(bool half)
        {
            if (half)
                TestXorRawBits<Half>();
            else
                TestXorRawBits<byte>();
        }

        private static void TestXorRawBits<T>() where T : unmanaged, IBitwiseOperators<T, T, T>
        {
            // Exercise every short tail, each SIMD/unroll boundary, independently unaligned
            // inputs, and all permitted destination aliases. Compare bits, including Half NaNs.
            int elementSize = Marshal.SizeOf<T>();
            Random random = new(42);
            for (int test = 0; test < 156; test++)
            {
                int length = test <= 146 ? test : test switch
                {
                    147 => 255, 148 => 256, 149 => 257,
                    150 => 511, 151 => 512, 152 => 513,
                    153 => 1023, 154 => 1024, _ => 1025,
                };
                for (int offset = 0; offset < 64; offset++)
                {
                    int yOffset = (17 * offset + 7) % 64;
                    int dOffset = (29 * offset + 11) % 64;
                    byte[] x = new byte[length * elementSize + 128];
                    byte[] y = new byte[x.Length];
                    byte[] output = new byte[x.Length];
                    random.NextBytes(x);
                    random.NextBytes(y);
                    random.NextBytes(output);
                    byte[] originalX = (byte[])x.Clone();
                    byte[] originalY = (byte[])y.Clone();
                    ReadOnlySpan<T> xValues = MemoryMarshal.Cast<byte, T>(x.AsSpan(offset, length * elementSize));
                    ReadOnlySpan<T> yValues = MemoryMarshal.Cast<byte, T>(y.AsSpan(yOffset, length * elementSize));
                    T mask = MemoryMarshal.Read<T>(y);

                    for (int mode = 0; mode < 6; mode++)
                    {
                        bool scalar = mode >= 4;
                        bool aliasX = mode is 1 or 3 or 5;
                        bool aliasY = mode is 2 or 3;
                        int start = aliasX ? offset : aliasY ? yOffset : dOffset;
                        byte[] actual = (byte[])(aliasX ? x : aliasY ? y : output).Clone();
                        byte[] expected = (byte[])actual.Clone();
                        Span<T> expectedValues = MemoryMarshal.Cast<byte, T>(expected.AsSpan(start));
                        for (int i = 0; i < length; i++)
                            expectedValues[i] = xValues[i] ^ (scalar ? mask : mode == 3 ? xValues[i] : yValues[i]);
                        Span<T> destination = MemoryMarshal.Cast<byte, T>(actual.AsSpan(start));
                        ReadOnlySpan<T> first = aliasX ? destination.Slice(0, length) : xValues;
                        if (scalar)
                            MemoryExtensions.Xor(first, mask, destination);
                        else
                            MemoryExtensions.Xor(first, aliasY ? destination.Slice(0, length) : yValues, destination);
                        Assert.Equal(expected, actual);
                        Assert.Equal(originalX, x);
                        Assert.Equal(originalY, y);
                    }
                }
            }
        }

        [Fact]
        public static void Xor_Half_AllBitPatterns()
        {
            Half[] x = new Half[65536];
            Half[] y = new Half[x.Length];
            Half[] actual = new Half[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = BitConverter.UInt16BitsToHalf((ushort)i);
                y[i] = BitConverter.UInt16BitsToHalf((ushort)(i * 17 + 7));
            }
            MemoryExtensions.Xor<Half>(x, y, actual);
            for (int i = 0; i < x.Length; i++)
                Assert.Equal((ushort)(i ^ (ushort)(i * 17 + 7)), BitConverter.HalfToUInt16Bits(actual[i]));
            MemoryExtensions.Xor<Half>(x, BitConverter.UInt16BitsToHalf(0xFE01), actual);
            for (int i = 0; i < x.Length; i++)
                Assert.Equal((ushort)(i ^ 0xFE01), BitConverter.HalfToUInt16Bits(actual[i]));
        }

        [Fact]
        public static void Xor_Half_ValidationBeforeWrites()
        {
            Half[] buffer = new Half[130];
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = BitConverter.UInt16BitsToHalf((ushort)(i * 73));
            byte[] original = MemoryMarshal.AsBytes(buffer.AsSpan()).ToArray();
            Half[] other = new Half[128];
            Half mask = (Half)42;
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<Half>(buffer.AsSpan(0, 128), new Half[127], buffer));
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<Half>(other, other, buffer.AsSpan(0, 127)));
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<Half>(other, mask, buffer.AsSpan(0, 127)));
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<Half>(buffer.AsSpan(0, 128), other, buffer.AsSpan(1)));
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<Half>(other, buffer.AsSpan(1, 128), buffer));
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<Half>(buffer.AsSpan(0, 128), mask, buffer.AsSpan(1)));
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<Half>(buffer.AsSpan(1, 128), mask, buffer));
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<Half>(new Half[2], buffer.AsSpan(128), buffer));
            Assert.Throws<ArgumentException>(() => MemoryExtensions.Xor<Half>(buffer.AsSpan(128), mask, buffer));
            Assert.Equal(original, MemoryMarshal.AsBytes(buffer.AsSpan()).ToArray());
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
