// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Numerics;
using System.Runtime.Intrinsics;

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
                for (; i <= x.Length - Vector512<T>.Count; i += Vector512<T>.Count)
                {
                    (Vector512.Create(x.Slice(i)) ^ Vector512.Create(y.Slice(i))).CopyTo(destination.Slice(i));
                }
            }

            if (Vector256.IsHardwareAccelerated && Vector256<T>.IsSupported)
            {
                for (; i <= x.Length - Vector256<T>.Count; i += Vector256<T>.Count)
                {
                    (Vector256.Create(x.Slice(i)) ^ Vector256.Create(y.Slice(i))).CopyTo(destination.Slice(i));
                }
            }

            if (Vector128.IsHardwareAccelerated && Vector128<T>.IsSupported)
            {
                for (; i <= x.Length - Vector128<T>.Count; i += Vector128<T>.Count)
                {
                    (Vector128.Create(x.Slice(i)) ^ Vector128.Create(y.Slice(i))).CopyTo(destination.Slice(i));
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

            int i = 0;
            if (Vector512.IsHardwareAccelerated && Vector512<T>.IsSupported && x.Length >= Vector512<T>.Count)
            {
                Vector512<T> value = Vector512.Create(y);
                for (; i <= x.Length - Vector512<T>.Count; i += Vector512<T>.Count)
                {
                    (Vector512.Create(x.Slice(i)) ^ value).CopyTo(destination.Slice(i));
                }
            }

            if (Vector256.IsHardwareAccelerated && Vector256<T>.IsSupported && x.Length - i >= Vector256<T>.Count)
            {
                Vector256<T> value = Vector256.Create(y);
                for (; i <= x.Length - Vector256<T>.Count; i += Vector256<T>.Count)
                {
                    (Vector256.Create(x.Slice(i)) ^ value).CopyTo(destination.Slice(i));
                }
            }

            if (Vector128.IsHardwareAccelerated && Vector128<T>.IsSupported && x.Length - i >= Vector128<T>.Count)
            {
                Vector128<T> value = Vector128.Create(y);
                for (; i <= x.Length - Vector128<T>.Count; i += Vector128<T>.Count)
                {
                    (Vector128.Create(x.Slice(i)) ^ value).CopyTo(destination.Slice(i));
                }
            }

            for (; i < x.Length; i++)
            {
                destination[i] = x[i] ^ y;
            }
        }

        private static void ValidateXorDestination<T>(ReadOnlySpan<T> input, Span<T> destination)
        {
            if (input.Length > destination.Length)
            {
                ThrowHelper.ThrowArgumentException_DestinationTooShort();
            }

            if (input.Overlaps(destination, out int offset) && offset != 0)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.InvalidOperation_SpanOverlappedOperation);
            }
        }
    }
}
