// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.InteropServices;

namespace System
{
    public static partial class MemoryExtensions
    {
        /// <summary>Computes the element-wise bitwise exclusive OR of two spans.</summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="x">The first input span.</param>
        /// <param name="y">The second input span.</param>
        /// <param name="destination">The span in which to store the results.</param>
        /// <exception cref="ArgumentException">The input spans have different lengths.</exception>
        /// <exception cref="ArgumentException">The destination is shorter than the input spans.</exception>
        /// <exception cref="ArgumentException">An input overlaps the destination without starting at the same location.</exception>
        /// <remarks>
        /// Computes <c>destination[i] = x[i] ^ y[i]</c> for each input element.
        /// The destination may start at the same location as either or both inputs.
        /// Elements of the destination beyond the input length are not modified.
        /// </remarks>
        public static void Xor<T>(this ReadOnlySpan<T> x, ReadOnlySpan<T> y, Span<T> destination)
            where T : IBitwiseOperators<T, T, T>
        {
            if (x.Length != y.Length)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_SpansMustHaveSameLength);
            }

            ValidateXorDestination(x, destination);
            ValidateXorDestination(y, destination);

            int i = 0;
            if (Vector512.IsHardwareAccelerated && Vector512<T>.IsSupported)
            {
                // Validate an eight-vector block once, then use constant offsets within it.
                for (; i <= x.Length - 8 * Vector512<T>.Count; i += 8 * Vector512<T>.Count)
                {
                    ReadOnlySpan<T> xBlock = x.Slice(i, 8 * Vector512<T>.Count);
                    ReadOnlySpan<T> yBlock = y.Slice(i, 8 * Vector512<T>.Count);
                    Span<T> dBlock = destination.Slice(i, 8 * Vector512<T>.Count);
                    Vector512<T> v0 = Vector512.Create(xBlock) ^ Vector512.Create(yBlock);
                    Vector512<T> v1 = Vector512.Create(xBlock.Slice(Vector512<T>.Count)) ^ Vector512.Create(yBlock.Slice(Vector512<T>.Count));
                    Vector512<T> v2 = Vector512.Create(xBlock.Slice(2 * Vector512<T>.Count)) ^ Vector512.Create(yBlock.Slice(2 * Vector512<T>.Count));
                    Vector512<T> v3 = Vector512.Create(xBlock.Slice(3 * Vector512<T>.Count)) ^ Vector512.Create(yBlock.Slice(3 * Vector512<T>.Count));
                    Vector512<T> v4 = Vector512.Create(xBlock.Slice(4 * Vector512<T>.Count)) ^ Vector512.Create(yBlock.Slice(4 * Vector512<T>.Count));
                    Vector512<T> v5 = Vector512.Create(xBlock.Slice(5 * Vector512<T>.Count)) ^ Vector512.Create(yBlock.Slice(5 * Vector512<T>.Count));
                    Vector512<T> v6 = Vector512.Create(xBlock.Slice(6 * Vector512<T>.Count)) ^ Vector512.Create(yBlock.Slice(6 * Vector512<T>.Count));
                    Vector512<T> v7 = Vector512.Create(xBlock.Slice(7 * Vector512<T>.Count)) ^ Vector512.Create(yBlock.Slice(7 * Vector512<T>.Count));
                    v0.CopyTo(dBlock);
                    v1.CopyTo(dBlock.Slice(Vector512<T>.Count));
                    v2.CopyTo(dBlock.Slice(2 * Vector512<T>.Count));
                    v3.CopyTo(dBlock.Slice(3 * Vector512<T>.Count));
                    v4.CopyTo(dBlock.Slice(4 * Vector512<T>.Count));
                    v5.CopyTo(dBlock.Slice(5 * Vector512<T>.Count));
                    v6.CopyTo(dBlock.Slice(6 * Vector512<T>.Count));
                    v7.CopyTo(dBlock.Slice(7 * Vector512<T>.Count));
                }

                for (; i <= x.Length - Vector512<T>.Count; i += Vector512<T>.Count)
                {
                    (Vector512.Create(x.Slice(i, Vector512<T>.Count)) ^ Vector512.Create(y.Slice(i, Vector512<T>.Count))).CopyTo(destination.Slice(i, Vector512<T>.Count));
                }
            }

            if (Vector256.IsHardwareAccelerated && Vector256<T>.IsSupported)
            {
                // Validate an eight-vector block once, then use constant offsets within it.
                for (; i <= x.Length - 8 * Vector256<T>.Count; i += 8 * Vector256<T>.Count)
                {
                    ReadOnlySpan<T> xBlock = x.Slice(i, 8 * Vector256<T>.Count);
                    ReadOnlySpan<T> yBlock = y.Slice(i, 8 * Vector256<T>.Count);
                    Span<T> dBlock = destination.Slice(i, 8 * Vector256<T>.Count);
                    Vector256<T> v0 = Vector256.Create(xBlock) ^ Vector256.Create(yBlock);
                    Vector256<T> v1 = Vector256.Create(xBlock.Slice(Vector256<T>.Count)) ^ Vector256.Create(yBlock.Slice(Vector256<T>.Count));
                    Vector256<T> v2 = Vector256.Create(xBlock.Slice(2 * Vector256<T>.Count)) ^ Vector256.Create(yBlock.Slice(2 * Vector256<T>.Count));
                    Vector256<T> v3 = Vector256.Create(xBlock.Slice(3 * Vector256<T>.Count)) ^ Vector256.Create(yBlock.Slice(3 * Vector256<T>.Count));
                    Vector256<T> v4 = Vector256.Create(xBlock.Slice(4 * Vector256<T>.Count)) ^ Vector256.Create(yBlock.Slice(4 * Vector256<T>.Count));
                    Vector256<T> v5 = Vector256.Create(xBlock.Slice(5 * Vector256<T>.Count)) ^ Vector256.Create(yBlock.Slice(5 * Vector256<T>.Count));
                    Vector256<T> v6 = Vector256.Create(xBlock.Slice(6 * Vector256<T>.Count)) ^ Vector256.Create(yBlock.Slice(6 * Vector256<T>.Count));
                    Vector256<T> v7 = Vector256.Create(xBlock.Slice(7 * Vector256<T>.Count)) ^ Vector256.Create(yBlock.Slice(7 * Vector256<T>.Count));
                    v0.CopyTo(dBlock);
                    v1.CopyTo(dBlock.Slice(Vector256<T>.Count));
                    v2.CopyTo(dBlock.Slice(2 * Vector256<T>.Count));
                    v3.CopyTo(dBlock.Slice(3 * Vector256<T>.Count));
                    v4.CopyTo(dBlock.Slice(4 * Vector256<T>.Count));
                    v5.CopyTo(dBlock.Slice(5 * Vector256<T>.Count));
                    v6.CopyTo(dBlock.Slice(6 * Vector256<T>.Count));
                    v7.CopyTo(dBlock.Slice(7 * Vector256<T>.Count));
                }

                for (; i <= x.Length - Vector256<T>.Count; i += Vector256<T>.Count)
                {
                    (Vector256.Create(x.Slice(i, Vector256<T>.Count)) ^ Vector256.Create(y.Slice(i, Vector256<T>.Count))).CopyTo(destination.Slice(i, Vector256<T>.Count));
                }
            }

            if (Vector128.IsHardwareAccelerated && Vector128<T>.IsSupported)
            {
                // Validate an eight-vector block once, then use constant offsets within it.
                for (; i <= x.Length - 8 * Vector128<T>.Count; i += 8 * Vector128<T>.Count)
                {
                    ReadOnlySpan<T> xBlock = x.Slice(i, 8 * Vector128<T>.Count);
                    ReadOnlySpan<T> yBlock = y.Slice(i, 8 * Vector128<T>.Count);
                    Span<T> dBlock = destination.Slice(i, 8 * Vector128<T>.Count);
                    Vector128<T> v0 = Vector128.Create(xBlock) ^ Vector128.Create(yBlock);
                    Vector128<T> v1 = Vector128.Create(xBlock.Slice(Vector128<T>.Count)) ^ Vector128.Create(yBlock.Slice(Vector128<T>.Count));
                    Vector128<T> v2 = Vector128.Create(xBlock.Slice(2 * Vector128<T>.Count)) ^ Vector128.Create(yBlock.Slice(2 * Vector128<T>.Count));
                    Vector128<T> v3 = Vector128.Create(xBlock.Slice(3 * Vector128<T>.Count)) ^ Vector128.Create(yBlock.Slice(3 * Vector128<T>.Count));
                    Vector128<T> v4 = Vector128.Create(xBlock.Slice(4 * Vector128<T>.Count)) ^ Vector128.Create(yBlock.Slice(4 * Vector128<T>.Count));
                    Vector128<T> v5 = Vector128.Create(xBlock.Slice(5 * Vector128<T>.Count)) ^ Vector128.Create(yBlock.Slice(5 * Vector128<T>.Count));
                    Vector128<T> v6 = Vector128.Create(xBlock.Slice(6 * Vector128<T>.Count)) ^ Vector128.Create(yBlock.Slice(6 * Vector128<T>.Count));
                    Vector128<T> v7 = Vector128.Create(xBlock.Slice(7 * Vector128<T>.Count)) ^ Vector128.Create(yBlock.Slice(7 * Vector128<T>.Count));
                    v0.CopyTo(dBlock);
                    v1.CopyTo(dBlock.Slice(Vector128<T>.Count));
                    v2.CopyTo(dBlock.Slice(2 * Vector128<T>.Count));
                    v3.CopyTo(dBlock.Slice(3 * Vector128<T>.Count));
                    v4.CopyTo(dBlock.Slice(4 * Vector128<T>.Count));
                    v5.CopyTo(dBlock.Slice(5 * Vector128<T>.Count));
                    v6.CopyTo(dBlock.Slice(6 * Vector128<T>.Count));
                    v7.CopyTo(dBlock.Slice(7 * Vector128<T>.Count));
                }

                for (; i <= x.Length - Vector128<T>.Count; i += Vector128<T>.Count)
                {
                    (Vector128.Create(x.Slice(i, Vector128<T>.Count)) ^ Vector128.Create(y.Slice(i, Vector128<T>.Count))).CopyTo(destination.Slice(i, Vector128<T>.Count));
                }
            }

            // Each element is processed once, including when an input is the destination.
            for (; i < x.Length; i++)
            {
                destination[i] = x[i] ^ y[i];
            }
        }

        /// <summary>Computes the element-wise bitwise exclusive OR of a span and a scalar.</summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="x">The input span.</param>
        /// <param name="y">The scalar value to XOR with each input element.</param>
        /// <param name="destination">The span in which to store the results.</param>
        /// <exception cref="ArgumentException">The destination is shorter than the input span.</exception>
        /// <exception cref="ArgumentException">The input overlaps the destination without starting at the same location.</exception>
        /// <remarks>
        /// Computes <c>destination[i] = x[i] ^ y</c> for each input element.
        /// The destination may start at the same location as the input.
        /// Elements of the destination beyond the input length are not modified.
        /// </remarks>
        public static void Xor<T>(this ReadOnlySpan<T> x, T y, Span<T> destination)
            where T : IBitwiseOperators<T, T, T>
        {
            ValidateXorDestination(x, destination);

            // Reserve the specialized path for a full unrolled block. Small inputs
            // avoid the extra call and alignment setup.
            if (Vector128.IsHardwareAccelerated && Vector128<T>.IsSupported &&
                x.Length >= 8 * (Vector512.IsHardwareAccelerated && Vector512<T>.IsSupported ? Vector512<T>.Count :
                    Vector256.IsHardwareAccelerated && Vector256<T>.IsSupported ? Vector256<T>.Count : Vector128<T>.Count) &&
                x == (ReadOnlySpan<T>)destination.Slice(0, x.Length))
            {
                XorInPlace(destination.Slice(0, x.Length), y);
                return;
            }

            int i = 0;
            if (Vector512.IsHardwareAccelerated && Vector512<T>.IsSupported && x.Length >= Vector512<T>.Count)
            {
                Vector512<T> value = Vector512.Create(y);
                // Validate an eight-vector block once, then use constant offsets within it.
                for (; i <= x.Length - 8 * Vector512<T>.Count; i += 8 * Vector512<T>.Count)
                {
                    ReadOnlySpan<T> xBlock = x.Slice(i, 8 * Vector512<T>.Count);
                    Span<T> dBlock = destination.Slice(i, 8 * Vector512<T>.Count);
                    Vector512<T> v0 = Vector512.Create(xBlock) ^ value;
                    Vector512<T> v1 = Vector512.Create(xBlock.Slice(Vector512<T>.Count)) ^ value;
                    Vector512<T> v2 = Vector512.Create(xBlock.Slice(2 * Vector512<T>.Count)) ^ value;
                    Vector512<T> v3 = Vector512.Create(xBlock.Slice(3 * Vector512<T>.Count)) ^ value;
                    Vector512<T> v4 = Vector512.Create(xBlock.Slice(4 * Vector512<T>.Count)) ^ value;
                    Vector512<T> v5 = Vector512.Create(xBlock.Slice(5 * Vector512<T>.Count)) ^ value;
                    Vector512<T> v6 = Vector512.Create(xBlock.Slice(6 * Vector512<T>.Count)) ^ value;
                    Vector512<T> v7 = Vector512.Create(xBlock.Slice(7 * Vector512<T>.Count)) ^ value;
                    v0.CopyTo(dBlock);
                    v1.CopyTo(dBlock.Slice(Vector512<T>.Count));
                    v2.CopyTo(dBlock.Slice(2 * Vector512<T>.Count));
                    v3.CopyTo(dBlock.Slice(3 * Vector512<T>.Count));
                    v4.CopyTo(dBlock.Slice(4 * Vector512<T>.Count));
                    v5.CopyTo(dBlock.Slice(5 * Vector512<T>.Count));
                    v6.CopyTo(dBlock.Slice(6 * Vector512<T>.Count));
                    v7.CopyTo(dBlock.Slice(7 * Vector512<T>.Count));
                }

                for (; i <= x.Length - Vector512<T>.Count; i += Vector512<T>.Count)
                {
                    (Vector512.Create(x.Slice(i, Vector512<T>.Count)) ^ value).CopyTo(destination.Slice(i, Vector512<T>.Count));
                }
            }

            if (Vector256.IsHardwareAccelerated && Vector256<T>.IsSupported && x.Length - i >= Vector256<T>.Count)
            {
                Vector256<T> value = Vector256.Create(y);
                // Validate an eight-vector block once, then use constant offsets within it.
                for (; i <= x.Length - 8 * Vector256<T>.Count; i += 8 * Vector256<T>.Count)
                {
                    ReadOnlySpan<T> xBlock = x.Slice(i, 8 * Vector256<T>.Count);
                    Span<T> dBlock = destination.Slice(i, 8 * Vector256<T>.Count);
                    Vector256<T> v0 = Vector256.Create(xBlock) ^ value;
                    Vector256<T> v1 = Vector256.Create(xBlock.Slice(Vector256<T>.Count)) ^ value;
                    Vector256<T> v2 = Vector256.Create(xBlock.Slice(2 * Vector256<T>.Count)) ^ value;
                    Vector256<T> v3 = Vector256.Create(xBlock.Slice(3 * Vector256<T>.Count)) ^ value;
                    Vector256<T> v4 = Vector256.Create(xBlock.Slice(4 * Vector256<T>.Count)) ^ value;
                    Vector256<T> v5 = Vector256.Create(xBlock.Slice(5 * Vector256<T>.Count)) ^ value;
                    Vector256<T> v6 = Vector256.Create(xBlock.Slice(6 * Vector256<T>.Count)) ^ value;
                    Vector256<T> v7 = Vector256.Create(xBlock.Slice(7 * Vector256<T>.Count)) ^ value;
                    v0.CopyTo(dBlock);
                    v1.CopyTo(dBlock.Slice(Vector256<T>.Count));
                    v2.CopyTo(dBlock.Slice(2 * Vector256<T>.Count));
                    v3.CopyTo(dBlock.Slice(3 * Vector256<T>.Count));
                    v4.CopyTo(dBlock.Slice(4 * Vector256<T>.Count));
                    v5.CopyTo(dBlock.Slice(5 * Vector256<T>.Count));
                    v6.CopyTo(dBlock.Slice(6 * Vector256<T>.Count));
                    v7.CopyTo(dBlock.Slice(7 * Vector256<T>.Count));
                }

                for (; i <= x.Length - Vector256<T>.Count; i += Vector256<T>.Count)
                {
                    (Vector256.Create(x.Slice(i, Vector256<T>.Count)) ^ value).CopyTo(destination.Slice(i, Vector256<T>.Count));
                }
            }

            if (Vector128.IsHardwareAccelerated && Vector128<T>.IsSupported && x.Length - i >= Vector128<T>.Count)
            {
                Vector128<T> value = Vector128.Create(y);
                // Validate an eight-vector block once, then use constant offsets within it.
                for (; i <= x.Length - 8 * Vector128<T>.Count; i += 8 * Vector128<T>.Count)
                {
                    ReadOnlySpan<T> xBlock = x.Slice(i, 8 * Vector128<T>.Count);
                    Span<T> dBlock = destination.Slice(i, 8 * Vector128<T>.Count);
                    Vector128<T> v0 = Vector128.Create(xBlock) ^ value;
                    Vector128<T> v1 = Vector128.Create(xBlock.Slice(Vector128<T>.Count)) ^ value;
                    Vector128<T> v2 = Vector128.Create(xBlock.Slice(2 * Vector128<T>.Count)) ^ value;
                    Vector128<T> v3 = Vector128.Create(xBlock.Slice(3 * Vector128<T>.Count)) ^ value;
                    Vector128<T> v4 = Vector128.Create(xBlock.Slice(4 * Vector128<T>.Count)) ^ value;
                    Vector128<T> v5 = Vector128.Create(xBlock.Slice(5 * Vector128<T>.Count)) ^ value;
                    Vector128<T> v6 = Vector128.Create(xBlock.Slice(6 * Vector128<T>.Count)) ^ value;
                    Vector128<T> v7 = Vector128.Create(xBlock.Slice(7 * Vector128<T>.Count)) ^ value;
                    v0.CopyTo(dBlock);
                    v1.CopyTo(dBlock.Slice(Vector128<T>.Count));
                    v2.CopyTo(dBlock.Slice(2 * Vector128<T>.Count));
                    v3.CopyTo(dBlock.Slice(3 * Vector128<T>.Count));
                    v4.CopyTo(dBlock.Slice(4 * Vector128<T>.Count));
                    v5.CopyTo(dBlock.Slice(5 * Vector128<T>.Count));
                    v6.CopyTo(dBlock.Slice(6 * Vector128<T>.Count));
                    v7.CopyTo(dBlock.Slice(7 * Vector128<T>.Count));
                }

                for (; i <= x.Length - Vector128<T>.Count; i += Vector128<T>.Count)
                {
                    (Vector128.Create(x.Slice(i, Vector128<T>.Count)) ^ value).CopyTo(destination.Slice(i, Vector128<T>.Count));
                }
            }

            for (; i < x.Length; i++)
            {
                destination[i] = x[i] ^ y;
            }
        }

        private static void XorInPlace<T>(Span<T> destination, T y)
            where T : IBitwiseOperators<T, T, T>
        {
            if (Vector512.IsHardwareAccelerated && Vector512<T>.IsSupported && destination.Length >= Vector512<T>.Count)
            {
                Vector512<T> value = Vector512.Create(y);
                Span<T> originalDestination = destination;
                int alignmentOffset = GetXorAlignmentOffset(destination, 64, Vector512<T>.Count);
                Vector512<T> beginning = default;
                if (alignmentOffset != 0)
                {
                    // Preload the overlapping beginning before any in-place writes.
                    beginning = Vector512.Create(destination) ^ value;
                    destination = destination.Slice(alignmentOffset);
                }
                while (destination.Length >= 8 * Vector512<T>.Count)
                {
                    Span<T> block = destination.Slice(0, 8 * Vector512<T>.Count);
                    ref T blockStart = ref MemoryMarshal.GetReference(block);
                    unsafe
                    {
                        // SAFETY: The checked slice contains eight complete vectors of a supported
                        // primitive type. Each reference offset is 0..7 vector lengths, so every
                        // load and store stays within the block. Managed references remain GC-tracked,
                        // and LoadUnsafe/StoreUnsafe permit unaligned addresses after relocation.
                        Vector512<T> v0 = Vector512.LoadUnsafe(ref blockStart) ^ value;
                        Vector512<T> v1 = Vector512.LoadUnsafe(ref Unsafe.Add(ref blockStart, Vector512<T>.Count)) ^ value;
                        Vector512<T> v2 = Vector512.LoadUnsafe(ref Unsafe.Add(ref blockStart, 2 * Vector512<T>.Count)) ^ value;
                        Vector512<T> v3 = Vector512.LoadUnsafe(ref Unsafe.Add(ref blockStart, 3 * Vector512<T>.Count)) ^ value;
                        Vector512<T> v4 = Vector512.LoadUnsafe(ref Unsafe.Add(ref blockStart, 4 * Vector512<T>.Count)) ^ value;
                        Vector512<T> v5 = Vector512.LoadUnsafe(ref Unsafe.Add(ref blockStart, 5 * Vector512<T>.Count)) ^ value;
                        Vector512<T> v6 = Vector512.LoadUnsafe(ref Unsafe.Add(ref blockStart, 6 * Vector512<T>.Count)) ^ value;
                        Vector512<T> v7 = Vector512.LoadUnsafe(ref Unsafe.Add(ref blockStart, 7 * Vector512<T>.Count)) ^ value;
                        v0.StoreUnsafe(ref blockStart);
                        v1.StoreUnsafe(ref Unsafe.Add(ref blockStart, Vector512<T>.Count));
                        v2.StoreUnsafe(ref Unsafe.Add(ref blockStart, 2 * Vector512<T>.Count));
                        v3.StoreUnsafe(ref Unsafe.Add(ref blockStart, 3 * Vector512<T>.Count));
                        v4.StoreUnsafe(ref Unsafe.Add(ref blockStart, 4 * Vector512<T>.Count));
                        v5.StoreUnsafe(ref Unsafe.Add(ref blockStart, 5 * Vector512<T>.Count));
                        v6.StoreUnsafe(ref Unsafe.Add(ref blockStart, 6 * Vector512<T>.Count));
                        v7.StoreUnsafe(ref Unsafe.Add(ref blockStart, 7 * Vector512<T>.Count));
                    }
                    destination = destination.Slice(8 * Vector512<T>.Count);
                }

                while (destination.Length >= Vector512<T>.Count)
                {
                    Span<T> block = destination.Slice(0, Vector512<T>.Count);
                    (Vector512.Create(block) ^ value).CopyTo(block);
                    destination = destination.Slice(Vector512<T>.Count);
                }

                if (alignmentOffset != 0)
                {
                    // The remaining tail starts beyond this saved vector. Overwrite the
                    // overlap with its precomputed result rather than XORing it twice.
                    beginning.CopyTo(originalDestination);
                }
            }

            if (Vector256.IsHardwareAccelerated && Vector256<T>.IsSupported && destination.Length >= Vector256<T>.Count)
            {
                Vector256<T> value = Vector256.Create(y);
                Span<T> originalDestination = destination;
                int alignmentOffset = GetXorAlignmentOffset(destination, 32, Vector256<T>.Count);
                Vector256<T> beginning = default;
                if (alignmentOffset != 0)
                {
                    // Preload the overlapping beginning before any in-place writes.
                    beginning = Vector256.Create(destination) ^ value;
                    destination = destination.Slice(alignmentOffset);
                }
                if ((!Vector512.IsHardwareAccelerated || !Vector512<T>.IsSupported))
                {
                    while (destination.Length >= 8 * Vector256<T>.Count)
                    {
                        Span<T> block = destination.Slice(0, 8 * Vector256<T>.Count);
                        ref T blockStart = ref MemoryMarshal.GetReference(block);
                        unsafe
                        {
                            // SAFETY: The checked slice contains eight complete vectors of a supported
                            // primitive type. Each reference offset is 0..7 vector lengths, so every
                            // load and store stays within the block. Managed references remain GC-tracked,
                            // and LoadUnsafe/StoreUnsafe permit unaligned addresses after relocation.
                            Vector256<T> v0 = Vector256.LoadUnsafe(ref blockStart) ^ value;
                            Vector256<T> v1 = Vector256.LoadUnsafe(ref Unsafe.Add(ref blockStart, Vector256<T>.Count)) ^ value;
                            Vector256<T> v2 = Vector256.LoadUnsafe(ref Unsafe.Add(ref blockStart, 2 * Vector256<T>.Count)) ^ value;
                            Vector256<T> v3 = Vector256.LoadUnsafe(ref Unsafe.Add(ref blockStart, 3 * Vector256<T>.Count)) ^ value;
                            Vector256<T> v4 = Vector256.LoadUnsafe(ref Unsafe.Add(ref blockStart, 4 * Vector256<T>.Count)) ^ value;
                            Vector256<T> v5 = Vector256.LoadUnsafe(ref Unsafe.Add(ref blockStart, 5 * Vector256<T>.Count)) ^ value;
                            Vector256<T> v6 = Vector256.LoadUnsafe(ref Unsafe.Add(ref blockStart, 6 * Vector256<T>.Count)) ^ value;
                            Vector256<T> v7 = Vector256.LoadUnsafe(ref Unsafe.Add(ref blockStart, 7 * Vector256<T>.Count)) ^ value;
                            v0.StoreUnsafe(ref blockStart);
                            v1.StoreUnsafe(ref Unsafe.Add(ref blockStart, Vector256<T>.Count));
                            v2.StoreUnsafe(ref Unsafe.Add(ref blockStart, 2 * Vector256<T>.Count));
                            v3.StoreUnsafe(ref Unsafe.Add(ref blockStart, 3 * Vector256<T>.Count));
                            v4.StoreUnsafe(ref Unsafe.Add(ref blockStart, 4 * Vector256<T>.Count));
                            v5.StoreUnsafe(ref Unsafe.Add(ref blockStart, 5 * Vector256<T>.Count));
                            v6.StoreUnsafe(ref Unsafe.Add(ref blockStart, 6 * Vector256<T>.Count));
                            v7.StoreUnsafe(ref Unsafe.Add(ref blockStart, 7 * Vector256<T>.Count));
                        }
                        destination = destination.Slice(8 * Vector256<T>.Count);
                    }
                }

                while (destination.Length >= Vector256<T>.Count)
                {
                    Span<T> block = destination.Slice(0, Vector256<T>.Count);
                    (Vector256.Create(block) ^ value).CopyTo(block);
                    destination = destination.Slice(Vector256<T>.Count);
                }

                if (alignmentOffset != 0)
                {
                    // The remaining tail starts beyond this saved vector. Overwrite the
                    // overlap with its precomputed result rather than XORing it twice.
                    beginning.CopyTo(originalDestination);
                }
            }

            if (Vector128.IsHardwareAccelerated && Vector128<T>.IsSupported && destination.Length >= Vector128<T>.Count)
            {
                Vector128<T> value = Vector128.Create(y);
                Span<T> originalDestination = destination;
                int alignmentOffset = GetXorAlignmentOffset(destination, 16, Vector128<T>.Count);
                Vector128<T> beginning = default;
                if (alignmentOffset != 0)
                {
                    // Preload the overlapping beginning before any in-place writes.
                    beginning = Vector128.Create(destination) ^ value;
                    destination = destination.Slice(alignmentOffset);
                }
                if ((!Vector512.IsHardwareAccelerated || !Vector512<T>.IsSupported) && (!Vector256.IsHardwareAccelerated || !Vector256<T>.IsSupported))
                {
                    while (destination.Length >= 8 * Vector128<T>.Count)
                    {
                        Span<T> block = destination.Slice(0, 8 * Vector128<T>.Count);
                        ref T blockStart = ref MemoryMarshal.GetReference(block);
                        unsafe
                        {
                            // SAFETY: The checked slice contains eight complete vectors of a supported
                            // primitive type. Each reference offset is 0..7 vector lengths, so every
                            // load and store stays within the block. Managed references remain GC-tracked,
                            // and LoadUnsafe/StoreUnsafe permit unaligned addresses after relocation.
                            Vector128<T> v0 = Vector128.LoadUnsafe(ref blockStart) ^ value;
                            Vector128<T> v1 = Vector128.LoadUnsafe(ref Unsafe.Add(ref blockStart, Vector128<T>.Count)) ^ value;
                            Vector128<T> v2 = Vector128.LoadUnsafe(ref Unsafe.Add(ref blockStart, 2 * Vector128<T>.Count)) ^ value;
                            Vector128<T> v3 = Vector128.LoadUnsafe(ref Unsafe.Add(ref blockStart, 3 * Vector128<T>.Count)) ^ value;
                            Vector128<T> v4 = Vector128.LoadUnsafe(ref Unsafe.Add(ref blockStart, 4 * Vector128<T>.Count)) ^ value;
                            Vector128<T> v5 = Vector128.LoadUnsafe(ref Unsafe.Add(ref blockStart, 5 * Vector128<T>.Count)) ^ value;
                            Vector128<T> v6 = Vector128.LoadUnsafe(ref Unsafe.Add(ref blockStart, 6 * Vector128<T>.Count)) ^ value;
                            Vector128<T> v7 = Vector128.LoadUnsafe(ref Unsafe.Add(ref blockStart, 7 * Vector128<T>.Count)) ^ value;
                            v0.StoreUnsafe(ref blockStart);
                            v1.StoreUnsafe(ref Unsafe.Add(ref blockStart, Vector128<T>.Count));
                            v2.StoreUnsafe(ref Unsafe.Add(ref blockStart, 2 * Vector128<T>.Count));
                            v3.StoreUnsafe(ref Unsafe.Add(ref blockStart, 3 * Vector128<T>.Count));
                            v4.StoreUnsafe(ref Unsafe.Add(ref blockStart, 4 * Vector128<T>.Count));
                            v5.StoreUnsafe(ref Unsafe.Add(ref blockStart, 5 * Vector128<T>.Count));
                            v6.StoreUnsafe(ref Unsafe.Add(ref blockStart, 6 * Vector128<T>.Count));
                            v7.StoreUnsafe(ref Unsafe.Add(ref blockStart, 7 * Vector128<T>.Count));
                        }
                        destination = destination.Slice(8 * Vector128<T>.Count);
                    }
                }

                while (destination.Length >= Vector128<T>.Count)
                {
                    Span<T> block = destination.Slice(0, Vector128<T>.Count);
                    (Vector128.Create(block) ^ value).CopyTo(block);
                    destination = destination.Slice(Vector128<T>.Count);
                }

                if (alignmentOffset != 0)
                {
                    // The remaining tail starts beyond this saved vector. Overwrite the
                    // overlap with its precomputed result rather than XORing it twice.
                    beginning.CopyTo(originalDestination);
                }
            }

            for (int i = 0; i < destination.Length; i++)
            {
                destination[i] = destination[i] ^ y;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetXorAlignmentOffset<T>(Span<T> destination, int vectorByteCount, int vectorElementCount)
        {
            if (destination.Length < 8 * vectorElementCount)
            {
                return 0;
            }

            unsafe
            {
                // SAFETY: The nonempty span supplies a live managed reference. The vector
                // width is a power of two. Only address bits are observed, never dereferenced.
                // Subsequent accesses are bounds-validated and tolerate unaligned addresses,
                // so correctness does not depend on the hint surviving a GC relocation.
                nuint misalignment = Unsafe.OpportunisticMisalignment(ref MemoryMarshal.GetReference(destination), (nuint)vectorByteCount);
                nuint elementSize = (nuint)(vectorByteCount / vectorElementCount);

                // A byte-misaligned span cannot be aligned by advancing whole elements.
                return misalignment % elementSize == 0
                    ? (int)(((nuint)vectorByteCount - misalignment) % (nuint)vectorByteCount / elementSize)
                    : 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ValidateXorDestination<T>(ReadOnlySpan<T> input, Span<T> destination)
        {
            if (input.Length > destination.Length)
            {
                ThrowHelper.ThrowArgumentException_DestinationTooShort();
            }

            // Span equality compares the starting reference and length, not element values.
            if (input != (ReadOnlySpan<T>)destination.Slice(0, input.Length) && input.Overlaps(destination))
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.InvalidOperation_SpanOverlappedOperation);
            }
        }
    }
}
