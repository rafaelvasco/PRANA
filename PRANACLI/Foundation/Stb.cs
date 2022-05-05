﻿
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace PRANA.Foundation;

#pragma warning disable CS0162
#pragma warning disable CA2014


internal static unsafe class Stb
{
    internal class UnsafeArray1D<T> where T : struct
    {
        private readonly T[] _data;
        private readonly GCHandle _pinHandle;

        internal GCHandle PinHandle => _pinHandle;

        public T this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public UnsafeArray1D(int size)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            _data = new T[size];
            _pinHandle = GCHandle.Alloc(_data, GCHandleType.Pinned);
        }

        public UnsafeArray1D(T[] data, int sizeOf)
        {
            if (sizeOf <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sizeOf));
            }

            _data = data ?? throw new ArgumentNullException(nameof(data));
            _pinHandle = GCHandle.Alloc(_data, GCHandleType.Pinned);
        }

        ~UnsafeArray1D()
        {
            _pinHandle.Free();
        }

        private void* ToPointer()
        {
            return _pinHandle.AddrOfPinnedObject().ToPointer();
        }

        public static implicit operator void*(UnsafeArray1D<T> array)
        {
            return array.ToPointer();
        }

        public static void* operator +(UnsafeArray1D<T> array, int delta)
        {
            return array.ToPointer();
        }
    }

    internal class UnsafeArray2D<T> where T : struct
    {
        private readonly UnsafeArray1D<T>[] _data;
        private IntPtr[] _pinAddresses;
        private readonly GCHandle _pinAddressesHandle;

        public UnsafeArray1D<T> this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public UnsafeArray2D(int size1, int size2)
        {
            _data = new UnsafeArray1D<T>[size1];
            _pinAddresses = new IntPtr[size1];
            for (var i = 0; i < size1; ++i)
            {
                _data[i] = new UnsafeArray1D<T>(size2);
                _pinAddresses[i] = _data[i].PinHandle.AddrOfPinnedObject();
            }

            _pinAddressesHandle = GCHandle.Alloc(_pinAddresses, GCHandleType.Pinned);
        }

        ~UnsafeArray2D()
        {
            _pinAddressesHandle.Free();
        }

        private void* ToPointer() => _pinAddressesHandle.AddrOfPinnedObject().ToPointer();

        public static implicit operator void*(UnsafeArray2D<T> array)
        {
            return array.ToPointer();
        }
    }

	private static class MemoryStats
	{
		private static int _allocations;

		public static int Allocations
		{
			get
			{
				return _allocations;
			}
		}

		internal static void Allocated()
		{
			Interlocked.Increment(ref _allocations);
		}

		internal static void Freed()
		{
			Interlocked.Decrement(ref _allocations);
		}
	}

	private static class CRuntime
	{
        private const long DBL_EXP_MASK = 0x7ff0000000000000L;
        private const int DBL_MANT_BITS = 52;
        private const long DBL_SGN_MASK = -1 - 0x7fffffffffffffffL;
        private const long DBL_MANT_MASK = 0x000fffffffffffffL;
        private const long DBL_EXP_CLR_MASK = DBL_SGN_MASK | DBL_MANT_MASK;

		private static readonly string numbers = "0123456789";

		public static void* malloc(ulong size)
		{
			return malloc((long)size);
		}

        private static void* malloc(long size)
		{
			var ptr = Marshal.AllocHGlobal((int)size);

			MemoryStats.Allocated();

			return ptr.ToPointer();
		}

		public static void free(void* a)
		{
			if (a == null)
				return;

			var ptr = new IntPtr(a);
			Marshal.FreeHGlobal(ptr);
			MemoryStats.Freed();
		}

        private static void memcpy(void* a, void* b, long size)
		{
			var ap = (byte*)a;
			var bp = (byte*)b;
			for (long i = 0; i < size; ++i)
				*ap++ = *bp++;
		}

		public static void memcpy(void* a, void* b, ulong size)
		{
			memcpy(a, b, (long)size);
		}

        private static void memmove(void* a, void* b, long size)
		{
			void* temp = null;

			try
			{
				temp = malloc(size);
				memcpy(temp, b, size);
				memcpy(a, temp, size);
			}

			finally
			{
				if (temp != null)
					free(temp);
			}
		}

		public static void memmove(void* a, void* b, ulong size)
		{
			memmove(a, b, (long)size);
		}

        private static int memcmp(void* a, void* b, long size)
		{
			var result = 0;
			var ap = (byte*)a;
			var bp = (byte*)b;
			for (long i = 0; i < size; ++i)
			{
				if (*ap != *bp)
					result += 1;

				ap++;
				bp++;
			}

			return result;
		}

		public static int memcmp(void* a, void* b, ulong size)
		{
			return memcmp(a, b, (long)size);
		}

		public static int memcmp(byte* a, byte[] b, ulong size)
		{
			fixed (void* bptr = b)
			{
				return memcmp(a, bptr, (long)size);
			}
		}

        private static void memset(void* ptr, int value, long size)
		{
			var bptr = (byte*)ptr;
			var bval = (byte)value;
			for (long i = 0; i < size; ++i)
				*bptr++ = bval;
		}

		public static void memset(void* ptr, int value, ulong size)
		{
			memset(ptr, value, (long)size);
		}

		public static uint _lrotl(uint x, int y)
		{
			return (x << y) | (x >> (32 - y));
		}

        private static void* realloc(void* a, long newSize)
		{
			if (a == null)
				return malloc(newSize);

			var ptr = new IntPtr(a);
			var result = Marshal.ReAllocHGlobal(ptr, new IntPtr(newSize));

			return result.ToPointer();
		}

		public static void* realloc(void* a, ulong newSize)
		{
			return realloc(a, (long)newSize);
		}

		public static int abs(int v)
		{
			return Math.Abs(v);
		}

		public static double pow(double a, double b)
		{
			return Math.Pow(a, b);
		}

		public static void SetArray<T>(T[] data, T value)
		{
			for (var i = 0; i < data.Length; ++i)
				data[i] = value;
		}

		public static double ldexp(double number, int exponent)
		{
			return number * Math.Pow(2, exponent);
		}

		public static int strcmp(sbyte* src, string token)
		{
			var result = 0;

			for (var i = 0; i < token.Length; ++i)
			{
				if (src[i] != token[i])
				{
					++result;
				}
			}

			return result;
		}

		public static int strncmp(sbyte* src, string token, ulong size)
		{
			var result = 0;

			for (var i = 0; i < Math.Min(token.Length, (int)size); ++i)
			{
				if (src[i] != token[i])
				{
					++result;
				}
			}

			return result;
		}

		public static long strtol(sbyte* start, sbyte** end, int radix)
		{
			// First step - determine length
			var length = 0;
			sbyte* ptr = start;
			while (numbers.IndexOf((char)*ptr) != -1)
			{
				++ptr;
				++length;
			}

			long result = 0;

			// Now build up the number
			ptr = start;
			while (length > 0)
			{
				long num = numbers.IndexOf((char)*ptr);
				long pow = (long)Math.Pow(10, length - 1);
				result += num * pow;

				++ptr;
				--length;
			}

			if (end != null)
			{
				*end = ptr;
			}

			return result;
		}

        /// <summary>
        /// This code had been borrowed from here: https://github.com/MachineCognitis/C.math.NET
        /// </summary>
        /// <param name="number"></param>
        /// <param name="exponent"></param>
        /// <returns></returns>
        public static double frexp(double number, int* exponent)
        {
            var bits = BitConverter.DoubleToInt64Bits(number);
            var exp = (int) ((bits & DBL_EXP_MASK) >> DBL_MANT_BITS);
            *exponent = 0;

            if (exp == 0x7ff || number == 0D)
                number += number;
            else
            {
                // Not zero and finite.
                *exponent = exp - 1022;
                if (exp == 0)
                {
                    // Subnormal, scale number so that it is in [1, 2).
                    number *= BitConverter.Int64BitsToDouble(0x4350000000000000L); // 2^54
                    bits = BitConverter.DoubleToInt64Bits(number);
                    exp = (int) ((bits & DBL_EXP_MASK) >> DBL_MANT_BITS);
                    *exponent = exp - 1022 - 54;
                }

                // Set exponent to -1 so that number is in [0.5, 1).
                number = BitConverter.Int64BitsToDouble((bits & DBL_EXP_CLR_MASK) | 0x3fe0000000000000L);
            }

            return number;
        }
	}

	public enum ColorComponents
	{
		Default,
		Grey,
		GreyAlpha,
		RedGreenBlue,
		RedGreenBlueAlpha
	}

	/* STB IMAGE */

	public struct ImageInfo
	{
		public int Width;
		public int Height;
		public ColorComponents ColorComponents;
		public int BitsPerChannel;


		public static ImageInfo? FromStream(Stream stream)
		{
			int width, height, comp;
			var context = new StbImage.stbi__context(stream);

			var is16Bit = StbImage.stbi__is_16_main(context) == 1;
			StbImage.stbi__rewind(context);

			var infoResult = StbImage.stbi__info_main(context, &width, &height, &comp);
			StbImage.stbi__rewind(context);

			if (infoResult == 0) return null;

			return new ImageInfo
			{
				Width = width,
				Height = height,
				ColorComponents = (ColorComponents)comp,
				BitsPerChannel = is16Bit ? 16 : 8
			};
		}
	}

	public class ImageResult
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public ColorComponents SourceComp { get; set; }
		public ColorComponents Comp { get; set; }
		public byte[] Data { get; set; }

		internal static ImageResult FromResult(byte* result, int width, int height, ColorComponents comp,
			ColorComponents req_comp)
		{
			if (result == null)
				throw new InvalidOperationException(StbImage.stbi__g_failure_reason);

			var image = new ImageResult
			{
				Width = width,
				Height = height,
				SourceComp = comp,
				Comp = req_comp == ColorComponents.Default ? comp : req_comp
			};

			// Convert to array
			image.Data = new byte[width * height * (int)image.Comp];
			Marshal.Copy(new IntPtr(result), image.Data, 0, image.Data.Length);

			return image;
		}

		public static ImageResult FromStream(Stream stream,
			ColorComponents requiredComponents = ColorComponents.Default)
		{
			byte* result = null;

			try
			{
				int x, y, comp;

				var context = new StbImage.stbi__context(stream);

				result = StbImage.stbi__load_and_postprocess_8bit(context, &x, &y, &comp, (int)requiredComponents);

				return FromResult(result, x, y, (ColorComponents)comp, requiredComponents);
			}
			finally
			{
				if (result != null)
					CRuntime.free(result);
			}
		}

		public static ImageResult FromMemory(byte[] data, ColorComponents requiredComponents = ColorComponents.Default)
		{
			using (var stream = new MemoryStream(data))
			{
				return FromStream(stream, requiredComponents);
			}
		}

		public static IEnumerable<AnimatedFrameResult> AnimatedGifFramesFromStream(Stream stream,
			ColorComponents requiredComponents = ColorComponents.Default)
		{
			return new AnimatedGifEnumerable(stream, requiredComponents);
		}
	}

	public class ImageResultFloat
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public ColorComponents SourceComp { get; set; }
		public ColorComponents Comp { get; set; }
		public float[] Data { get; set; }

		internal static ImageResultFloat FromResult(float* result, int width, int height, ColorComponents comp,
			ColorComponents req_comp)
		{
			if (result == null)
				throw new InvalidOperationException(StbImage.stbi__g_failure_reason);

			var image = new ImageResultFloat
			{
				Width = width,
				Height = height,
				SourceComp = comp,
				Comp = req_comp == ColorComponents.Default ? comp : req_comp
			};

			// Convert to array
			image.Data = new float[width * height * (int)image.Comp];
			Marshal.Copy(new IntPtr(result), image.Data, 0, image.Data.Length);

			return image;
		}

		public static ImageResultFloat FromStream(Stream stream,
			ColorComponents requiredComponents = ColorComponents.Default)
		{
			float* result = null;

			try
			{
				int x, y, comp;

				var context = new StbImage.stbi__context(stream);

				result = StbImage.stbi__loadf_main(context, &x, &y, &comp, (int)requiredComponents);

				return FromResult(result, x, y, (ColorComponents)comp, requiredComponents);
			}
			finally
			{
				if (result != null)
					CRuntime.free(result);
			}
		}

		public static ImageResultFloat FromMemory(byte[] data,
			ColorComponents requiredComponents = ColorComponents.Default)
		{
			using (var stream = new MemoryStream(data))
			{
				return FromStream(stream, requiredComponents);
			}
		}
	}

	public class AnimatedFrameResult : ImageResult
	{
		public int DelayInMs { get; set; }
	}

	internal class AnimatedGifEnumerator : IEnumerator<AnimatedFrameResult>
	{
		private readonly StbImage.stbi__context _context;
		private StbImage.stbi__gif _gif;

		public AnimatedGifEnumerator(Stream input, ColorComponents colorComponents)
		{
			if (input == null) throw new ArgumentNullException(nameof(input));

			_context = new StbImage.stbi__context(input);

			if (StbImage.stbi__gif_test(_context) == 0) throw new Exception("Input stream is not GIF file.");

			_gif = new StbImage.stbi__gif();
			ColorComponents = colorComponents;
		}

		public ColorComponents ColorComponents { get; }

		public AnimatedFrameResult Current { get; private set; }

		object IEnumerator.Current => Current;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public bool MoveNext()
		{
			// Read next frame
			int ccomp;
			byte two_back;
			var result = StbImage.stbi__gif_load_next(_context, _gif, &ccomp, (int)ColorComponents, &two_back);
			if (result == null) return false;

			if (Current == null)
			{
				Current = new AnimatedFrameResult
				{
					Width = _gif.w,
					Height = _gif.h,
					SourceComp = (ColorComponents)ccomp,
					Comp = ColorComponents == ColorComponents.Default ? (ColorComponents)ccomp : ColorComponents
				};

				Current.Data = new byte[Current.Width * Current.Height * (int)Current.Comp];
			}

			Current.DelayInMs = _gif.delay;

			Marshal.Copy(new IntPtr(result), Current.Data, 0, Current.Data.Length);

			return true;
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		~AnimatedGifEnumerator()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_gif != null)
				{
					if (_gif._out_ != null)
					{
						CRuntime.free(_gif._out_);
						_gif._out_ = null;
					}

					if (_gif.history != null)
					{
						CRuntime.free(_gif.history);
						_gif.history = null;
					}

					if (_gif.background != null)
					{
						CRuntime.free(_gif.background);
						_gif.background = null;
					}

					_gif = null;
				}
			}
		}
	}

	public class AnimatedGifEnumerable : IEnumerable<AnimatedFrameResult>
	{
		private readonly Stream _input;

		public AnimatedGifEnumerable(Stream input, ColorComponents colorComponents)
		{
			_input = input;
			ColorComponents = colorComponents;
		}

		public ColorComponents ColorComponents { get; }

		public IEnumerator<AnimatedFrameResult> GetEnumerator()
		{
			return new AnimatedGifEnumerator(_input, ColorComponents);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public static partial class StbImage
	{
		public static string stbi__g_failure_reason;
		public static readonly char[] stbi__parse_png_file_invalid_chunk = new char[25];

		public class stbi__context
		{
			public byte[] _tempBuffer;
			public int img_n = 0;
			public int img_out_n = 0;
			public uint img_x = 0;
			public uint img_y = 0;

			public stbi__context(Stream stream)
			{
				if (stream == null)
					throw new ArgumentNullException("stream");

				Stream = stream;
			}

			public Stream Stream { get; }
		}

		private static int stbi__err(string str)
		{
			stbi__g_failure_reason = str;
			return 0;
		}

		public static byte stbi__get8(stbi__context s)
		{
			var b = s.Stream.ReadByte();
			if (b == -1) return 0;

			return (byte)b;
		}

		public static void stbi__skip(stbi__context s, int skip)
		{
			s.Stream.Seek(skip, SeekOrigin.Current);
		}

		public static void stbi__rewind(stbi__context s)
		{
			s.Stream.Seek(0, SeekOrigin.Begin);
		}

		public static int stbi__at_eof(stbi__context s)
		{
			return s.Stream.Position == s.Stream.Length ? 1 : 0;
		}

		public static int stbi__getn(stbi__context s, byte* buf, int size)
		{
			if (s._tempBuffer == null ||
				s._tempBuffer.Length < size)
				s._tempBuffer = new byte[size * 2];

			var result = s.Stream.Read(s._tempBuffer, 0, size);
			Marshal.Copy(s._tempBuffer, 0, new IntPtr(buf), result);

			return result;
		}
	}

	/* COMMON */

    partial class StbImage
	{
		public const int STBI__SCAN_header = 2;
		public const int STBI__SCAN_load = 0;
		public const int STBI__SCAN_type = 1;
		public const int STBI_default = 0;
		public const int STBI_grey = 1;
		public const int STBI_grey_alpha = 2;
		public const int STBI_ORDER_BGR = 1;
		public const int STBI_ORDER_RGB = 0;
		public const int STBI_rgb = 3;
		public const int STBI_rgb_alpha = 4;

		public static byte[] stbi__compute_huffman_codes_length_dezigzag =
			{16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15};

		public static int stbi__de_iphone_flag_global;
		public static int stbi__de_iphone_flag_local;
		public static int stbi__de_iphone_flag_set;
		public static float stbi__h2l_gamma_i = 1.0f / 2.2f;
		public static float stbi__h2l_scale_i = 1.0f;
		public static float stbi__l2h_gamma = 2.2f;
		public static float stbi__l2h_scale = 1.0f;
		public static byte[] stbi__process_frame_header_rgb = { 82, 71, 66 };
		public static byte[] stbi__process_marker_tag = { 65, 100, 111, 98, 101, 0 };
		public static int[] stbi__shiftsigned_mul_table = { 0, 0xff, 0x55, 0x49, 0x11, 0x21, 0x41, 0x81, 0x01 };
		public static int[] stbi__shiftsigned_shift_table = { 0, 0, 0, 1, 0, 2, 4, 6, 0 };
		public static int stbi__unpremultiply_on_load_global;
		public static int stbi__unpremultiply_on_load_local;
		public static int stbi__unpremultiply_on_load_set;
		public static int stbi__vertically_flip_on_load_global;
		public static int stbi__vertically_flip_on_load_local;
		public static int stbi__vertically_flip_on_load_set;

		public static int stbi__addsizes_valid(int a, int b)
		{
			if (b < 0)
				return 0;
			return a <= 2147483647 - b ? 1 : 0;
		}

		public static int stbi__bit_reverse(int v, int bits)
		{
			return stbi__bitreverse16(v) >> (16 - bits);
		}

		public static int stbi__bitcount(uint a)
		{
			a = (a & 0x55555555) + ((a >> 1) & 0x55555555);
			a = (a & 0x33333333) + ((a >> 2) & 0x33333333);
			a = (a + (a >> 4)) & 0x0f0f0f0f;
			a = a + (a >> 8);
			a = a + (a >> 16);
			return (int)(a & 0xff);
		}

		public static int stbi__bitreverse16(int n)
		{
			n = ((n & 0xAAAA) >> 1) | ((n & 0x5555) << 1);
			n = ((n & 0xCCCC) >> 2) | ((n & 0x3333) << 2);
			n = ((n & 0xF0F0) >> 4) | ((n & 0x0F0F) << 4);
			n = ((n & 0xFF00) >> 8) | ((n & 0x00FF) << 8);
			return n;
		}

		public static byte stbi__blinn_8x8(byte x, byte y)
		{
			var t = (uint)(x * y + 128);
			return (byte)((t + (t >> 8)) >> 8);
		}

		public static byte stbi__clamp(int x)
		{
			if ((uint)x > 255)
			{
				if (x < 0)
					return 0;
				if (x > 255)
					return 255;
			}

			return (byte)x;
		}

		public static byte stbi__compute_y(int r, int g, int b)
		{
			return (byte)((r * 77 + g * 150 + 29 * b) >> 8);
		}

		public static ushort stbi__compute_y_16(int r, int g, int b)
		{
			return (ushort)((r * 77 + g * 150 + 29 * b) >> 8);
		}

		public static byte* stbi__convert_16_to_8(ushort* orig, int w, int h, int channels)
		{
			var i = 0;
			var img_len = w * h * channels;
			byte* reduced;
			reduced = (byte*)stbi__malloc((ulong)img_len);
			if (reduced == null)
				return (byte*)(ulong)(stbi__err("outofmem") != 0 ? 0 : 0);
			for (i = 0; i < img_len; ++i) reduced[i] = (byte)((orig[i] >> 8) & 0xFF);

			CRuntime.free(orig);
			return reduced;
		}

		public static ushort* stbi__convert_8_to_16(byte* orig, int w, int h, int channels)
		{
			var i = 0;
			var img_len = w * h * channels;
			ushort* enlarged;
			enlarged = (ushort*)stbi__malloc((ulong)(img_len * 2));
			if (enlarged == null)
				return (ushort*)(byte*)(ulong)(stbi__err("outofmem") != 0 ? 0 : 0);
			for (i = 0; i < img_len; ++i) enlarged[i] = (ushort)((orig[i] << 8) + orig[i]);

			CRuntime.free(orig);
			return enlarged;
		}

		public static byte* stbi__convert_format(byte* data, int img_n, int req_comp, uint x, uint y)
		{
			var i = 0;
			var j = 0;
			byte* good;
			if (req_comp == img_n)
				return data;
			good = (byte*)stbi__malloc_mad3(req_comp, (int)x, (int)y, 0);
			if (good == null)
			{
				CRuntime.free(data);
				return (byte*)(ulong)(stbi__err("outofmem") != 0 ? 0 : 0);
			}

			for (j = 0; j < (int)y; ++j)
			{
				var src = data + j * x * img_n;
				var dest = good + j * x * req_comp;
				switch (img_n * 8 + req_comp)
				{
					case 1 * 8 + 2:
						for (i = (int)(x - 1); i >= 0; --i, src += 1, dest += 2)
						{
							dest[0] = src[0];
							dest[1] = 255;
						}

						break;
					case 1 * 8 + 3:
						for (i = (int)(x - 1); i >= 0; --i, src += 1, dest += 3) dest[0] = dest[1] = dest[2] = src[0];

						break;
					case 1 * 8 + 4:
						for (i = (int)(x - 1); i >= 0; --i, src += 1, dest += 4)
						{
							dest[0] = dest[1] = dest[2] = src[0];
							dest[3] = 255;
						}

						break;
					case 2 * 8 + 1:
						for (i = (int)(x - 1); i >= 0; --i, src += 2, dest += 1) dest[0] = src[0];

						break;
					case 2 * 8 + 3:
						for (i = (int)(x - 1); i >= 0; --i, src += 2, dest += 3) dest[0] = dest[1] = dest[2] = src[0];

						break;
					case 2 * 8 + 4:
						for (i = (int)(x - 1); i >= 0; --i, src += 2, dest += 4)
						{
							dest[0] = dest[1] = dest[2] = src[0];
							dest[3] = src[1];
						}

						break;
					case 3 * 8 + 4:
						for (i = (int)(x - 1); i >= 0; --i, src += 3, dest += 4)
						{
							dest[0] = src[0];
							dest[1] = src[1];
							dest[2] = src[2];
							dest[3] = 255;
						}

						break;
					case 3 * 8 + 1:
						for (i = (int)(x - 1); i >= 0; --i, src += 3, dest += 1)
							dest[0] = stbi__compute_y(src[0], src[1], src[2]);

						break;
					case 3 * 8 + 2:
						for (i = (int)(x - 1); i >= 0; --i, src += 3, dest += 2)
						{
							dest[0] = stbi__compute_y(src[0], src[1], src[2]);
							dest[1] = 255;
						}

						break;
					case 4 * 8 + 1:
						for (i = (int)(x - 1); i >= 0; --i, src += 4, dest += 1)
							dest[0] = stbi__compute_y(src[0], src[1], src[2]);

						break;
					case 4 * 8 + 2:
						for (i = (int)(x - 1); i >= 0; --i, src += 4, dest += 2)
						{
							dest[0] = stbi__compute_y(src[0], src[1], src[2]);
							dest[1] = src[3];
						}

						break;
					case 4 * 8 + 3:
						for (i = (int)(x - 1); i >= 0; --i, src += 4, dest += 3)
						{
							dest[0] = src[0];
							dest[1] = src[1];
							dest[2] = src[2];
						}

						break;
					default:
						;
						CRuntime.free(data);
						CRuntime.free(good);
						return (byte*)(ulong)(stbi__err("unsupported") != 0 ? 0 : 0);
				}
			}

			CRuntime.free(data);
			return good;
		}

		public static ushort* stbi__convert_format16(ushort* data, int img_n, int req_comp, uint x, uint y)
		{
			var i = 0;
			var j = 0;
			ushort* good;
			if (req_comp == img_n)
				return data;
			good = (ushort*)stbi__malloc((ulong)(req_comp * x * y * 2));
			if (good == null)
			{
				CRuntime.free(data);
				return (ushort*)(byte*)(ulong)(stbi__err("outofmem") != 0 ? 0 : 0);
			}

			for (j = 0; j < (int)y; ++j)
			{
				var src = data + j * x * img_n;
				var dest = good + j * x * req_comp;
				switch (img_n * 8 + req_comp)
				{
					case 1 * 8 + 2:
						for (i = (int)(x - 1); i >= 0; --i, src += 1, dest += 2)
						{
							dest[0] = src[0];
							dest[1] = 0xffff;
						}

						break;
					case 1 * 8 + 3:
						for (i = (int)(x - 1); i >= 0; --i, src += 1, dest += 3) dest[0] = dest[1] = dest[2] = src[0];

						break;
					case 1 * 8 + 4:
						for (i = (int)(x - 1); i >= 0; --i, src += 1, dest += 4)
						{
							dest[0] = dest[1] = dest[2] = src[0];
							dest[3] = 0xffff;
						}

						break;
					case 2 * 8 + 1:
						for (i = (int)(x - 1); i >= 0; --i, src += 2, dest += 1) dest[0] = src[0];

						break;
					case 2 * 8 + 3:
						for (i = (int)(x - 1); i >= 0; --i, src += 2, dest += 3) dest[0] = dest[1] = dest[2] = src[0];

						break;
					case 2 * 8 + 4:
						for (i = (int)(x - 1); i >= 0; --i, src += 2, dest += 4)
						{
							dest[0] = dest[1] = dest[2] = src[0];
							dest[3] = src[1];
						}

						break;
					case 3 * 8 + 4:
						for (i = (int)(x - 1); i >= 0; --i, src += 3, dest += 4)
						{
							dest[0] = src[0];
							dest[1] = src[1];
							dest[2] = src[2];
							dest[3] = 0xffff;
						}

						break;
					case 3 * 8 + 1:
						for (i = (int)(x - 1); i >= 0; --i, src += 3, dest += 1)
							dest[0] = stbi__compute_y_16(src[0], src[1], src[2]);

						break;
					case 3 * 8 + 2:
						for (i = (int)(x - 1); i >= 0; --i, src += 3, dest += 2)
						{
							dest[0] = stbi__compute_y_16(src[0], src[1], src[2]);
							dest[1] = 0xffff;
						}

						break;
					case 4 * 8 + 1:
						for (i = (int)(x - 1); i >= 0; --i, src += 4, dest += 1)
							dest[0] = stbi__compute_y_16(src[0], src[1], src[2]);

						break;
					case 4 * 8 + 2:
						for (i = (int)(x - 1); i >= 0; --i, src += 4, dest += 2)
						{
							dest[0] = stbi__compute_y_16(src[0], src[1], src[2]);
							dest[1] = src[3];
						}

						break;
					case 4 * 8 + 3:
						for (i = (int)(x - 1); i >= 0; --i, src += 4, dest += 3)
						{
							dest[0] = src[0];
							dest[1] = src[1];
							dest[2] = src[2];
						}

						break;
					default:
						;
						CRuntime.free(data);
						CRuntime.free(good);
						return (ushort*)(byte*)(ulong)(stbi__err("unsupported") != 0 ? 0 : 0);
				}
			}

			CRuntime.free(data);
			return good;
		}

		public static void stbi__float_postprocess(float* result, int* x, int* y, int* comp, int req_comp)
		{
			if ((stbi__vertically_flip_on_load_set != 0
				? stbi__vertically_flip_on_load_local
				: stbi__vertically_flip_on_load_global) != 0 && result != null)
			{
				var channels = req_comp != 0 ? req_comp : *comp;
				stbi__vertical_flip(result, *x, *y, channels * sizeof(float));
			}
		}

		public static int stbi__get16be(stbi__context s)
		{
			int z = stbi__get8(s);
			return (z << 8) + stbi__get8(s);
		}

		public static int stbi__get16le(stbi__context s)
		{
			int z = stbi__get8(s);
			return z + (stbi__get8(s) << 8);
		}

		public static uint stbi__get32be(stbi__context s)
		{
			var z = (uint)stbi__get16be(s);
			return (uint)((z << 16) + stbi__get16be(s));
		}

		public static uint stbi__get32le(stbi__context s)
		{
			var z = (uint)stbi__get16le(s);
			z += (uint)stbi__get16le(s) << 16;
			return z;
		}

		public static int stbi__high_bit(uint z)
		{
			var n = 0;
			if (z == 0)
				return -1;
			if (z >= 0x10000)
			{
				n += 16;
				z >>= 16;
			}

			if (z >= 0x00100)
			{
				n += 8;
				z >>= 8;
			}

			if (z >= 0x00010)
			{
				n += 4;
				z >>= 4;
			}

			if (z >= 0x00004)
			{
				n += 2;
				z >>= 2;
			}

			if (z >= 0x00002) n += 1;

			return n;
		}

		public static int stbi__info_main(stbi__context s, int* x, int* y, int* comp)
		{
			if (stbi__png_info(s, x, y, comp) != 0)
				return 1;
			if (stbi__gif_info(s, x, y, comp) != 0)
				return 1;
			return stbi__err("unknown image type");
		}

		public static int stbi__is_16_main(stbi__context s)
		{
			if (stbi__png_is16(s) != 0)
				return 1;
			return 0;
		}

		public static float* stbi__ldr_to_hdr(byte* data, int x, int y, int comp)
		{
			var i = 0;
			var k = 0;
			var n = 0;
			float* output;
			if (data == null)
				return null;
			output = (float*)stbi__malloc_mad4(x, y, comp, sizeof(float), 0);
			if (output == null)
			{
				CRuntime.free(data);
				return (float*)(ulong)(stbi__err("outofmem") != 0 ? 0 : 0);
			}

			if ((comp & 1) != 0)
				n = comp;
			else
				n = comp - 1;
			for (i = 0; i < x * y; ++i)
				for (k = 0; k < n; ++k)
					output[i * comp + k] =
						(float)(CRuntime.pow(data[i * comp + k] / 255.0f, stbi__l2h_gamma) * stbi__l2h_scale);

			if (n < comp)
				for (i = 0; i < x * y; ++i)
					output[i * comp + n] = data[i * comp + n] / 255.0f;

			CRuntime.free(data);
			return output;
		}

		public static ushort* stbi__load_and_postprocess_16bit(stbi__context s, int* x, int* y, int* comp, int req_comp)
		{
			var ri = new stbi__result_info();
			var result = stbi__load_main(s, x, y, comp, req_comp, &ri, 16);
			if (result == null)
				return null;
			if (ri.bits_per_channel != 16)
			{
				result = stbi__convert_8_to_16((byte*)result, *x, *y, req_comp == 0 ? *comp : req_comp);
				ri.bits_per_channel = 16;
			}

			if ((stbi__vertically_flip_on_load_set != 0
				? stbi__vertically_flip_on_load_local
				: stbi__vertically_flip_on_load_global) != 0)
			{
				var channels = req_comp != 0 ? req_comp : *comp;
				stbi__vertical_flip(result, *x, *y, channels * sizeof(ushort));
			}

			return (ushort*)result;
		}

		public static byte* stbi__load_and_postprocess_8bit(stbi__context s, int* x, int* y, int* comp, int req_comp)
		{
			var ri = new stbi__result_info();
			var result = stbi__load_main(s, x, y, comp, req_comp, &ri, 8);
			if (result == null)
				return null;
			if (ri.bits_per_channel != 8)
			{
				result = stbi__convert_16_to_8((ushort*)result, *x, *y, req_comp == 0 ? *comp : req_comp);
				ri.bits_per_channel = 8;
			}

			if ((stbi__vertically_flip_on_load_set != 0
				? stbi__vertically_flip_on_load_local
				: stbi__vertically_flip_on_load_global) != 0)
			{
				var channels = req_comp != 0 ? req_comp : *comp;
				stbi__vertical_flip(result, *x, *y, channels * sizeof(byte));
			}

			return (byte*)result;
		}

		public static void* stbi__load_main(stbi__context s, int* x, int* y, int* comp, int req_comp,
			stbi__result_info* ri, int bpc)
		{
			CRuntime.memset(ri, 0, (ulong)sizeof(stbi__result_info));
			ri->bits_per_channel = 8;
			ri->channel_order = STBI_ORDER_RGB;
			ri->num_channels = 0;
			if (stbi__png_test(s) != 0)
				return stbi__png_load(s, x, y, comp, req_comp, ri);
			return (byte*)(ulong)(stbi__err("unknown image type") != 0 ? 0 : 0);
		}

		public static float* stbi__loadf_main(stbi__context s, int* x, int* y, int* comp, int req_comp)
		{
			byte* data;

			data = stbi__load_and_postprocess_8bit(s, x, y, comp, req_comp);
			if (data != null)
				return stbi__ldr_to_hdr(data, *x, *y, req_comp != 0 ? req_comp : *comp);
			return (float*)(ulong)(stbi__err("unknown image type") != 0 ? 0 : 0);
		}

		public static int stbi__mad2sizes_valid(int a, int b, int add)
		{
			return stbi__mul2sizes_valid(a, b) != 0 && stbi__addsizes_valid(a * b, add) != 0 ? 1 : 0;
		}

		public static int stbi__mad3sizes_valid(int a, int b, int c, int add)
		{
			return stbi__mul2sizes_valid(a, b) != 0 && stbi__mul2sizes_valid(a * b, c) != 0 &&
				   stbi__addsizes_valid(a * b * c, add) != 0
				? 1
				: 0;
		}

		public static int stbi__mad4sizes_valid(int a, int b, int c, int d, int add)
		{
			return stbi__mul2sizes_valid(a, b) != 0 && stbi__mul2sizes_valid(a * b, c) != 0 &&
				   stbi__mul2sizes_valid(a * b * c, d) != 0 && stbi__addsizes_valid(a * b * c * d, add) != 0
				? 1
				: 0;
		}

		public static void* stbi__malloc(ulong size)
		{
			return CRuntime.malloc(size);
		}

		public static void* stbi__malloc_mad2(int a, int b, int add)
		{
			if (stbi__mad2sizes_valid(a, b, add) == 0)
				return null;
			return stbi__malloc((ulong)(a * b + add));
		}

		public static void* stbi__malloc_mad3(int a, int b, int c, int add)
		{
			if (stbi__mad3sizes_valid(a, b, c, add) == 0)
				return null;
			return stbi__malloc((ulong)(a * b * c + add));
		}

		public static void* stbi__malloc_mad4(int a, int b, int c, int d, int add)
		{
			if (stbi__mad4sizes_valid(a, b, c, d, add) == 0)
				return null;
			return stbi__malloc((ulong)(a * b * c * d + add));
		}

		public static int stbi__mul2sizes_valid(int a, int b)
		{
			if (a < 0 || b < 0)
				return 0;
			if (b == 0)
				return 1;
			return a <= 2147483647 / b ? 1 : 0;
		}

		public static int stbi__paeth(int a, int b, int c)
		{
			var p = a + b - c;
			var pa = CRuntime.abs(p - a);
			var pb = CRuntime.abs(p - b);
			var pc = CRuntime.abs(p - c);
			if (pa <= pb && pa <= pc)
				return a;
			if (pb <= pc)
				return b;
			return c;
		}

		public static int stbi__shiftsigned(uint v, int shift, int bits)
		{
			if (shift < 0)
				v <<= -shift;
			else
				v >>= shift;
			v >>= 8 - bits;
			return (int)(v * stbi__shiftsigned_mul_table[bits]) >> stbi__shiftsigned_shift_table[bits];
		}

		public static void stbi__unpremultiply_on_load_thread(int flag_true_if_should_unpremultiply)
		{
			stbi__unpremultiply_on_load_local = flag_true_if_should_unpremultiply;
			stbi__unpremultiply_on_load_set = 1;
		}

		public static void stbi__vertical_flip(void* image, int w, int h, int bytes_per_pixel)
		{
			var row = 0;
			var bytes_per_row = w * bytes_per_pixel;
			var temp = stackalloc byte[2048];
			var bytes = (byte*)image;
			for (row = 0; row < h >> 1; row++)
			{
				var row0 = bytes + row * bytes_per_row;
				var row1 = bytes + (h - row - 1) * bytes_per_row;
				var bytes_left = (ulong)bytes_per_row;
				while (bytes_left != 0)
				{
					var bytes_copy = bytes_left < 2048 * sizeof(byte) ? bytes_left : 2048 * sizeof(byte);
					CRuntime.memcpy(temp, row0, bytes_copy);
					CRuntime.memcpy(row0, row1, bytes_copy);
					CRuntime.memcpy(row1, temp, bytes_copy);
					row0 += bytes_copy;
					row1 += bytes_copy;
					bytes_left -= bytes_copy;
				}
			}
		}

		public static void stbi__vertical_flip_slices(void* image, int w, int h, int z, int bytes_per_pixel)
		{
			var slice = 0;
			var slice_size = w * h * bytes_per_pixel;
			var bytes = (byte*)image;
			for (slice = 0; slice < z; ++slice)
			{
				stbi__vertical_flip(bytes, w, h, bytes_per_pixel);
				bytes += slice_size;
			}
		}

		public static void stbi_convert_iphone_png_to_rgb(int flag_true_if_should_convert)
		{
			stbi__de_iphone_flag_global = flag_true_if_should_convert;
		}

		public static void stbi_convert_iphone_png_to_rgb_thread(int flag_true_if_should_convert)
		{
			stbi__de_iphone_flag_local = flag_true_if_should_convert;
			stbi__de_iphone_flag_set = 1;
		}

		public static void stbi_hdr_to_ldr_gamma(float gamma)
		{
			stbi__h2l_gamma_i = 1 / gamma;
		}

		public static void stbi_hdr_to_ldr_scale(float scale)
		{
			stbi__h2l_scale_i = 1 / scale;
		}

		public static void stbi_ldr_to_hdr_gamma(float gamma)
		{
			stbi__l2h_gamma = gamma;
		}

		public static void stbi_ldr_to_hdr_scale(float scale)
		{
			stbi__l2h_scale = scale;
		}

		public static void stbi_set_flip_vertically_on_load(int flag_true_if_should_flip)
		{
			stbi__vertically_flip_on_load_global = flag_true_if_should_flip;
		}

		public static void stbi_set_flip_vertically_on_load_thread(int flag_true_if_should_flip)
		{
			stbi__vertically_flip_on_load_local = flag_true_if_should_flip;
			stbi__vertically_flip_on_load_set = 1;
		}

		public static void stbi_set_unpremultiply_on_load(int flag_true_if_should_unpremultiply)
		{
			stbi__unpremultiply_on_load_global = flag_true_if_should_unpremultiply;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbi__result_info
		{
			public int bits_per_channel;
			public int num_channels;
			public int channel_order;
		}
	}

	/* PNG */

    partial class StbImage
	{
		public const int STBI__F_avg = 3;
		public const int STBI__F_avg_first = 5;
		public const int STBI__F_none = 0;
		public const int STBI__F_paeth = 4;
		public const int STBI__F_paeth_first = 6;
		public const int STBI__F_sub = 1;
		public const int STBI__F_up = 2;

		public static byte[] first_row_filter =
			{STBI__F_none, STBI__F_sub, STBI__F_none, STBI__F_avg_first, STBI__F_paeth_first};

		public static byte[] stbi__check_png_header_png_sig = { 137, 80, 78, 71, 13, 10, 26, 10 };
		public static byte[] stbi__depth_scale_table = { 0, 0xff, 0x55, 0, 0x11, 0, 0, 0, 0x01 };

		public static int stbi__check_png_header(stbi__context s)
		{
			var i = 0;
			for (i = 0; i < 8; ++i)
				if (stbi__get8(s) != stbi__check_png_header_png_sig[i])
					return stbi__err("bad png sig");

			return 1;
		}

		public static int stbi__compute_transparency(stbi__png z, byte* tc, int out_n)
		{
			var s = z.s;
			uint i = 0;
			var pixel_count = s.img_x * s.img_y;
			var p = z._out_;
			if (out_n == 2)
				for (i = 0; i < pixel_count; ++i)
				{
					p[1] = (byte)(p[0] == tc[0] ? 0 : 255);
					p += 2;
				}
			else
				for (i = 0; i < pixel_count; ++i)
				{
					if (p[0] == tc[0] && p[1] == tc[1] && p[2] == tc[2])
						p[3] = 0;
					p += 4;
				}

			return 1;
		}

		public static int stbi__compute_transparency16(stbi__png z, ushort* tc, int out_n)
		{
			var s = z.s;
			uint i = 0;
			var pixel_count = s.img_x * s.img_y;
			var p = (ushort*)z._out_;
			if (out_n == 2)
				for (i = 0; i < pixel_count; ++i)
				{
					p[1] = (ushort)(p[0] == tc[0] ? 0 : 65535);
					p += 2;
				}
			else
				for (i = 0; i < pixel_count; ++i)
				{
					if (p[0] == tc[0] && p[1] == tc[1] && p[2] == tc[2])
						p[3] = 0;
					p += 4;
				}

			return 1;
		}

		public static int stbi__create_png_image(stbi__png a, byte* image_data, uint image_data_len, int out_n,
			int depth, int color, int interlaced)
		{
			var bytes = depth == 16 ? 2 : 1;
			var out_bytes = out_n * bytes;
			byte* final;
			var p = 0;
			if (interlaced == 0)
				return stbi__create_png_image_raw(a, image_data, image_data_len, out_n, a.s.img_x, a.s.img_y, depth,
					color);
			final = (byte*)stbi__malloc_mad3((int)a.s.img_x, (int)a.s.img_y, out_bytes, 0);
			if (final == null)
				return stbi__err("outofmem");
			for (p = 0; p < 7; ++p)
			{
				var xorig = stackalloc int[] { 0, 4, 0, 2, 0, 1, 0 };
				var yorig = stackalloc int[] { 0, 0, 4, 0, 2, 0, 1 };
				var xspc = stackalloc int[] { 8, 8, 4, 4, 2, 2, 1 };
				var yspc = stackalloc int[] { 8, 8, 8, 4, 4, 2, 2 };
				var i = 0;
				var j = 0;
				var x = 0;
				var y = 0;
				x = (int)((a.s.img_x - xorig[p] + xspc[p] - 1) / xspc[p]);
				y = (int)((a.s.img_y - yorig[p] + yspc[p] - 1) / yspc[p]);
				if (x != 0 && y != 0)
				{
					var img_len = (uint)((((a.s.img_n * x * depth + 7) >> 3) + 1) * y);
					if (stbi__create_png_image_raw(a, image_data, image_data_len, out_n, (uint)x, (uint)y, depth,
						color) == 0)
					{
						CRuntime.free(final);
						return 0;
					}

					for (j = 0; j < y; ++j)
						for (i = 0; i < x; ++i)
						{
							var out_y = j * yspc[p] + yorig[p];
							var out_x = i * xspc[p] + xorig[p];
							CRuntime.memcpy(final + out_y * a.s.img_x * out_bytes + out_x * out_bytes,
								a._out_ + (j * x + i) * out_bytes, (ulong)out_bytes);
						}

					CRuntime.free(a._out_);
					image_data += img_len;
					image_data_len -= img_len;
				}
			}

			a._out_ = final;
			return 1;
		}

		public static int stbi__create_png_image_raw(stbi__png a, byte* raw, uint raw_len, int out_n, uint x, uint y,
			int depth, int color)
		{
			var bytes = depth == 16 ? 2 : 1;
			var s = a.s;
			uint i = 0;
			uint j = 0;
			var stride = (uint)(x * out_n * bytes);
			uint img_len = 0;
			uint img_width_bytes = 0;
			var k = 0;
			var img_n = s.img_n;
			var output_bytes = out_n * bytes;
			var filter_bytes = img_n * bytes;
			var width = (int)x;
			a._out_ = (byte*)stbi__malloc_mad3((int)x, (int)y, output_bytes, 0);
			if (a._out_ == null)
				return stbi__err("outofmem");
			if (stbi__mad3sizes_valid(img_n, (int)x, depth, 7) == 0)
				return stbi__err("too large");
			img_width_bytes = (uint)((img_n * x * depth + 7) >> 3);
			img_len = (img_width_bytes + 1) * y;
			if (raw_len < img_len)
				return stbi__err("not enough pixels");
			for (j = 0; j < y; ++j)
			{
				var cur = a._out_ + stride * j;
				byte* prior;
				int filter = *raw++;
				if (filter > 4)
					return stbi__err("invalid filter");
				if (depth < 8)
				{
					if (img_width_bytes > x)
						return stbi__err("invalid width");
					cur += x * out_n - img_width_bytes;
					filter_bytes = 1;
					width = (int)img_width_bytes;
				}

				prior = cur - stride;
				if (j == 0)
					filter = first_row_filter[filter];
				for (k = 0; k < filter_bytes; ++k)
					switch (filter)
					{
						case STBI__F_none:
							cur[k] = raw[k];
							break;
						case STBI__F_sub:
							cur[k] = raw[k];
							break;
						case STBI__F_up:
							cur[k] = (byte)((raw[k] + prior[k]) & 255);
							break;
						case STBI__F_avg:
							cur[k] = (byte)((raw[k] + (prior[k] >> 1)) & 255);
							break;
						case STBI__F_paeth:
							cur[k] = (byte)((raw[k] + stbi__paeth(0, prior[k], 0)) & 255);
							break;
						case STBI__F_avg_first:
							cur[k] = raw[k];
							break;
						case STBI__F_paeth_first:
							cur[k] = raw[k];
							break;
					}

				if (depth == 8)
				{
					if (img_n != out_n)
						cur[img_n] = 255;
					raw += img_n;
					cur += out_n;
					prior += out_n;
				}
				else if (depth == 16)
				{
					if (img_n != out_n)
					{
						cur[filter_bytes] = 255;
						cur[filter_bytes + 1] = 255;
					}

					raw += filter_bytes;
					cur += output_bytes;
					prior += output_bytes;
				}
				else
				{
					raw += 1;
					cur += 1;
					prior += 1;
				}

				if (depth < 8 || img_n == out_n)
				{
					var nk = (width - 1) * filter_bytes;
					switch (filter)
					{
						case STBI__F_none:
							CRuntime.memcpy(cur, raw, (ulong)nk);
							break;
						case STBI__F_sub:
							for (k = 0; k < nk; ++k) cur[k] = (byte)((raw[k] + cur[k - filter_bytes]) & 255);

							break;
						case STBI__F_up:
							for (k = 0; k < nk; ++k) cur[k] = (byte)((raw[k] + prior[k]) & 255);

							break;
						case STBI__F_avg:
							for (k = 0; k < nk; ++k)
								cur[k] = (byte)((raw[k] + ((prior[k] + cur[k - filter_bytes]) >> 1)) & 255);

							break;
						case STBI__F_paeth:
							for (k = 0; k < nk; ++k)
								cur[k] = (byte)((raw[k] + stbi__paeth(cur[k - filter_bytes], prior[k],
									prior[k - filter_bytes])) & 255);

							break;
						case STBI__F_avg_first:
							for (k = 0; k < nk; ++k) cur[k] = (byte)((raw[k] + (cur[k - filter_bytes] >> 1)) & 255);

							break;
						case STBI__F_paeth_first:
							for (k = 0; k < nk; ++k)
								cur[k] = (byte)((raw[k] + stbi__paeth(cur[k - filter_bytes], 0, 0)) & 255);

							break;
					}

					raw += nk;
				}
				else
				{
					switch (filter)
					{
						case STBI__F_none:
							for (i = x - 1;
								i >= 1;
								--i, cur[filter_bytes] = 255, raw += filter_bytes, cur += output_bytes, prior +=
									output_bytes)
								for (k = 0; k < filter_bytes; ++k)
									cur[k] = raw[k];

							break;
						case STBI__F_sub:
							for (i = x - 1;
								i >= 1;
								--i, cur[filter_bytes] = 255, raw += filter_bytes, cur += output_bytes, prior +=
									output_bytes)
								for (k = 0; k < filter_bytes; ++k)
									cur[k] = (byte)((raw[k] + cur[k - output_bytes]) & 255);

							break;
						case STBI__F_up:
							for (i = x - 1;
								i >= 1;
								--i, cur[filter_bytes] = 255, raw += filter_bytes, cur += output_bytes, prior +=
									output_bytes)
								for (k = 0; k < filter_bytes; ++k)
									cur[k] = (byte)((raw[k] + prior[k]) & 255);

							break;
						case STBI__F_avg:
							for (i = x - 1;
								i >= 1;
								--i, cur[filter_bytes] = 255, raw += filter_bytes, cur += output_bytes, prior +=
									output_bytes)
								for (k = 0; k < filter_bytes; ++k)
									cur[k] = (byte)((raw[k] + ((prior[k] + cur[k - output_bytes]) >> 1)) & 255);

							break;
						case STBI__F_paeth:
							for (i = x - 1;
								i >= 1;
								--i, cur[filter_bytes] = 255, raw += filter_bytes, cur += output_bytes, prior +=
									output_bytes)
								for (k = 0; k < filter_bytes; ++k)
									cur[k] = (byte)((raw[k] + stbi__paeth(cur[k - output_bytes], prior[k],
										prior[k - output_bytes])) & 255);

							break;
						case STBI__F_avg_first:
							for (i = x - 1;
								i >= 1;
								--i, cur[filter_bytes] = 255, raw += filter_bytes, cur += output_bytes, prior +=
									output_bytes)
								for (k = 0; k < filter_bytes; ++k)
									cur[k] = (byte)((raw[k] + (cur[k - output_bytes] >> 1)) & 255);

							break;
						case STBI__F_paeth_first:
							for (i = x - 1;
								i >= 1;
								--i, cur[filter_bytes] = 255, raw += filter_bytes, cur += output_bytes, prior +=
									output_bytes)
								for (k = 0; k < filter_bytes; ++k)
									cur[k] = (byte)((raw[k] + stbi__paeth(cur[k - output_bytes], 0, 0)) & 255);

							break;
					}

					if (depth == 16)
					{
						cur = a._out_ + stride * j;
						for (i = 0; i < x; ++i, cur += output_bytes) cur[filter_bytes + 1] = 255;
					}
				}
			}

			if (depth < 8)
			{
				for (j = 0; j < y; ++j)
				{
					var cur = a._out_ + stride * j;
					var _in_ = a._out_ + stride * j + x * out_n - img_width_bytes;
					var scale = (byte)(color == 0 ? stbi__depth_scale_table[depth] : 1);
					if (depth == 4)
					{
						for (k = (int)(x * img_n); k >= 2; k -= 2, ++_in_)
						{
							*cur++ = (byte)(scale * (*_in_ >> 4));
							*cur++ = (byte)(scale * (*_in_ & 0x0f));
						}

						if (k > 0)
							*cur++ = (byte)(scale * (*_in_ >> 4));
					}
					else if (depth == 2)
					{
						for (k = (int)(x * img_n); k >= 4; k -= 4, ++_in_)
						{
							*cur++ = (byte)(scale * (*_in_ >> 6));
							*cur++ = (byte)(scale * ((*_in_ >> 4) & 0x03));
							*cur++ = (byte)(scale * ((*_in_ >> 2) & 0x03));
							*cur++ = (byte)(scale * (*_in_ & 0x03));
						}

						if (k > 0)
							*cur++ = (byte)(scale * (*_in_ >> 6));
						if (k > 1)
							*cur++ = (byte)(scale * ((*_in_ >> 4) & 0x03));
						if (k > 2)
							*cur++ = (byte)(scale * ((*_in_ >> 2) & 0x03));
					}
					else if (depth == 1)
					{
						for (k = (int)(x * img_n); k >= 8; k -= 8, ++_in_)
						{
							*cur++ = (byte)(scale * (*_in_ >> 7));
							*cur++ = (byte)(scale * ((*_in_ >> 6) & 0x01));
							*cur++ = (byte)(scale * ((*_in_ >> 5) & 0x01));
							*cur++ = (byte)(scale * ((*_in_ >> 4) & 0x01));
							*cur++ = (byte)(scale * ((*_in_ >> 3) & 0x01));
							*cur++ = (byte)(scale * ((*_in_ >> 2) & 0x01));
							*cur++ = (byte)(scale * ((*_in_ >> 1) & 0x01));
							*cur++ = (byte)(scale * (*_in_ & 0x01));
						}

						if (k > 0)
							*cur++ = (byte)(scale * (*_in_ >> 7));
						if (k > 1)
							*cur++ = (byte)(scale * ((*_in_ >> 6) & 0x01));
						if (k > 2)
							*cur++ = (byte)(scale * ((*_in_ >> 5) & 0x01));
						if (k > 3)
							*cur++ = (byte)(scale * ((*_in_ >> 4) & 0x01));
						if (k > 4)
							*cur++ = (byte)(scale * ((*_in_ >> 3) & 0x01));
						if (k > 5)
							*cur++ = (byte)(scale * ((*_in_ >> 2) & 0x01));
						if (k > 6)
							*cur++ = (byte)(scale * ((*_in_ >> 1) & 0x01));
					}

					if (img_n != out_n)
					{
						var q = 0;
						cur = a._out_ + stride * j;
						if (img_n == 1)
							for (q = (int)(x - 1); q >= 0; --q)
							{
								cur[q * 2 + 1] = 255;
								cur[q * 2 + 0] = cur[q];
							}
						else
							for (q = (int)(x - 1); q >= 0; --q)
							{
								cur[q * 4 + 3] = 255;
								cur[q * 4 + 2] = cur[q * 3 + 2];
								cur[q * 4 + 1] = cur[q * 3 + 1];
								cur[q * 4 + 0] = cur[q * 3 + 0];
							}
					}
				}
			}
			else if (depth == 16)
			{
				var cur = a._out_;
				var cur16 = (ushort*)cur;
				for (i = 0; i < x * y * out_n; ++i, cur16++, cur += 2) *cur16 = (ushort)((cur[0] << 8) | cur[1]);
			}

			return 1;
		}

		public static void stbi__de_iphone(stbi__png z)
		{
			var s = z.s;
			uint i = 0;
			var pixel_count = s.img_x * s.img_y;
			var p = z._out_;
			if (s.img_out_n == 3)
			{
				for (i = 0; i < pixel_count; ++i)
				{
					var t = p[0];
					p[0] = p[2];
					p[2] = t;
					p += 3;
				}
			}
			else
			{
				if ((stbi__unpremultiply_on_load_set != 0
					? stbi__unpremultiply_on_load_local
					: stbi__unpremultiply_on_load_global) != 0)
					for (i = 0; i < pixel_count; ++i)
					{
						var a = p[3];
						var t = p[0];
						if (a != 0)
						{
							var half = (byte)(a / 2);
							p[0] = (byte)((p[2] * 255 + half) / a);
							p[1] = (byte)((p[1] * 255 + half) / a);
							p[2] = (byte)((t * 255 + half) / a);
						}
						else
						{
							p[0] = p[2];
							p[2] = t;
						}

						p += 4;
					}
				else
					for (i = 0; i < pixel_count; ++i)
					{
						var t = p[0];
						p[0] = p[2];
						p[2] = t;
						p += 4;
					}
			}
		}

		public static void* stbi__do_png(stbi__png p, int* x, int* y, int* n, int req_comp, stbi__result_info* ri)
		{
			void* result = null;
			if (req_comp < 0 || req_comp > 4)
				return (byte*)(ulong)(stbi__err("bad req_comp") != 0 ? 0 : 0);
			if (stbi__parse_png_file(p, STBI__SCAN_load, req_comp) != 0)
			{
				if (p.depth <= 8)
					ri->bits_per_channel = 8;
				else if (p.depth == 16)
					ri->bits_per_channel = 16;
				else
					return (byte*)(ulong)(stbi__err("bad bits_per_channel") != 0 ? 0 : 0);
				result = p._out_;
				p._out_ = null;
				if (req_comp != 0 && req_comp != p.s.img_out_n)
				{
					if (ri->bits_per_channel == 8)
						result = stbi__convert_format((byte*)result, p.s.img_out_n, req_comp, p.s.img_x, p.s.img_y);
					else
						result = stbi__convert_format16((ushort*)result, p.s.img_out_n, req_comp, p.s.img_x,
							p.s.img_y);
					p.s.img_out_n = req_comp;
					if (result == null)
						return result;
				}

				*x = (int)p.s.img_x;
				*y = (int)p.s.img_y;
				if (n != null)
					*n = p.s.img_n;
			}

			CRuntime.free(p._out_);
			p._out_ = null;
			CRuntime.free(p.expanded);
			p.expanded = null;
			CRuntime.free(p.idata);
			p.idata = null;
			return result;
		}

		public static int stbi__expand_png_palette(stbi__png a, byte* palette, int len, int pal_img_n)
		{
			uint i = 0;
			var pixel_count = a.s.img_x * a.s.img_y;
			byte* p;
			byte* temp_out;
			var orig = a._out_;
			p = (byte*)stbi__malloc_mad2((int)pixel_count, pal_img_n, 0);
			if (p == null)
				return stbi__err("outofmem");
			temp_out = p;
			if (pal_img_n == 3)
				for (i = 0; i < pixel_count; ++i)
				{
					var n = orig[i] * 4;
					p[0] = palette[n];
					p[1] = palette[n + 1];
					p[2] = palette[n + 2];
					p += 3;
				}
			else
				for (i = 0; i < pixel_count; ++i)
				{
					var n = orig[i] * 4;
					p[0] = palette[n];
					p[1] = palette[n + 1];
					p[2] = palette[n + 2];
					p[3] = palette[n + 3];
					p += 4;
				}

			CRuntime.free(a._out_);
			a._out_ = temp_out;
			return 1;
		}

		public static stbi__pngchunk stbi__get_chunk_header(stbi__context s)
		{
			var c = new stbi__pngchunk();
			c.length = stbi__get32be(s);
			c.type = stbi__get32be(s);
			return c;
		}

		public static int stbi__parse_png_file(stbi__png z, int scan, int req_comp)
		{
			var palette = stackalloc byte[1024];
			byte pal_img_n = 0;
			byte has_trans = 0;
			var tc = stackalloc byte[] { 0, 0, 0 };
			var tc16 = stackalloc ushort[3];
			uint ioff = 0;
			uint idata_limit = 0;
			uint i = 0;
			uint pal_len = 0;
			var first = 1;
			var k = 0;
			var interlace = 0;
			var color = 0;
			var is_iphone = 0;
			var s = z.s;
			z.expanded = null;
			z.idata = null;
			z._out_ = null;
			if (stbi__check_png_header(s) == 0)
				return 0;
			if (scan == STBI__SCAN_type)
				return 1;
			for (; ; )
			{
				var c = stbi__get_chunk_header(s);
				switch (c.type)
				{
					case ((uint)67 << 24) + ((uint)103 << 16) + ((uint)66 << 8) + 73:
						is_iphone = 1;
						stbi__skip(s, (int)c.length);
						break;
					case ((uint)73 << 24) + ((uint)72 << 16) + ((uint)68 << 8) + 82:
						{
							var comp = 0;
							var filter = 0;
							if (first == 0)
								return stbi__err("multiple IHDR");
							first = 0;
							if (c.length != 13)
								return stbi__err("bad IHDR len");
							s.img_x = stbi__get32be(s);
							s.img_y = stbi__get32be(s);
							if (s.img_y > 1 << 24)
								return stbi__err("too large");
							if (s.img_x > 1 << 24)
								return stbi__err("too large");
							z.depth = stbi__get8(s);
							if (z.depth != 1 && z.depth != 2 && z.depth != 4 && z.depth != 8 && z.depth != 16)
								return stbi__err("1/2/4/8/16-bit only");
							color = stbi__get8(s);
							if (color > 6)
								return stbi__err("bad ctype");
							if (color == 3 && z.depth == 16)
								return stbi__err("bad ctype");
							if (color == 3)
								pal_img_n = 3;
							else if ((color & 1) != 0)
								return stbi__err("bad ctype");
							comp = stbi__get8(s);
							if (comp != 0)
								return stbi__err("bad comp method");
							filter = stbi__get8(s);
							if (filter != 0)
								return stbi__err("bad filter method");
							interlace = stbi__get8(s);
							if (interlace > 1)
								return stbi__err("bad interlace method");
							if (s.img_x == 0 || s.img_y == 0)
								return stbi__err("0-pixel image");
							if (pal_img_n == 0)
							{
								s.img_n = ((color & 2) != 0 ? 3 : 1) + ((color & 4) != 0 ? 1 : 0);
								if ((1 << 30) / s.img_x / s.img_n < s.img_y)
									return stbi__err("too large");
								if (scan == STBI__SCAN_header)
									return 1;
							}
							else
							{
								s.img_n = 1;
								if ((1 << 30) / s.img_x / 4 < s.img_y)
									return stbi__err("too large");
							}

							break;
						}

					case ((uint)80 << 24) + ((uint)76 << 16) + ((uint)84 << 8) + 69:
						{
							if (first != 0)
								return stbi__err("first not IHDR");
							if (c.length > 256 * 3)
								return stbi__err("invalid PLTE");
							pal_len = c.length / 3;
							if (pal_len * 3 != c.length)
								return stbi__err("invalid PLTE");
							for (i = 0; i < pal_len; ++i)
							{
								palette[i * 4 + 0] = stbi__get8(s);
								palette[i * 4 + 1] = stbi__get8(s);
								palette[i * 4 + 2] = stbi__get8(s);
								palette[i * 4 + 3] = 255;
							}

							break;
						}

					case ((uint)116 << 24) + ((uint)82 << 16) + ((uint)78 << 8) + 83:
						{
							if (first != 0)
								return stbi__err("first not IHDR");
							if (z.idata != null)
								return stbi__err("tRNS after IDAT");
							if (pal_img_n != 0)
							{
								if (scan == STBI__SCAN_header)
								{
									s.img_n = 4;
									return 1;
								}

								if (pal_len == 0)
									return stbi__err("tRNS before PLTE");
								if (c.length > pal_len)
									return stbi__err("bad tRNS len");
								pal_img_n = 4;
								for (i = 0; i < c.length; ++i) palette[i * 4 + 3] = stbi__get8(s);
							}
							else
							{
								if ((s.img_n & 1) == 0)
									return stbi__err("tRNS with alpha");
								if (c.length != (uint)s.img_n * 2)
									return stbi__err("bad tRNS len");
								has_trans = 1;
								if (z.depth == 16)
									for (k = 0; k < s.img_n; ++k)
										tc16[k] = (ushort)stbi__get16be(s);
								else
									for (k = 0; k < s.img_n; ++k)
										tc[k] = (byte)((byte)(stbi__get16be(s) & 255) * stbi__depth_scale_table[z.depth]);
							}

							break;
						}

					case ((uint)73 << 24) + ((uint)68 << 16) + ((uint)65 << 8) + 84:
						{
							if (first != 0)
								return stbi__err("first not IHDR");
							if (pal_img_n != 0 && pal_len == 0)
								return stbi__err("no PLTE");
							if (scan == STBI__SCAN_header)
							{
								s.img_n = pal_img_n;
								return 1;
							}

							if ((int)(ioff + c.length) < (int)ioff)
								return 0;
							if (ioff + c.length > idata_limit)
							{
								var idata_limit_old = idata_limit;
								byte* p;
								if (idata_limit == 0)
									idata_limit = c.length > 4096 ? c.length : 4096;
								while (ioff + c.length > idata_limit) idata_limit *= 2;

								p = (byte*)CRuntime.realloc(z.idata, (ulong)idata_limit);
								if (p == null)
									return stbi__err("outofmem");
								z.idata = p;
							}

							if (stbi__getn(s, z.idata + ioff, (int)c.length) == 0)
								return stbi__err("outofdata");
							ioff += c.length;
							break;
						}

					case ((uint)73 << 24) + ((uint)69 << 16) + ((uint)78 << 8) + 68:
						{
							uint raw_len = 0;
							uint bpl = 0;
							if (first != 0)
								return stbi__err("first not IHDR");
							if (scan != STBI__SCAN_load)
								return 1;
							if (z.idata == null)
								return stbi__err("no IDAT");
							bpl = (uint)((s.img_x * z.depth + 7) / 8);
							raw_len = (uint)(bpl * s.img_y * s.img_n + s.img_y);
							z.expanded = (byte*)stbi_zlib_decode_malloc_guesssize_headerflag((sbyte*)z.idata, (int)ioff,
								(int)raw_len, (int*)&raw_len, is_iphone == 0 ? 1 : 0);
							if (z.expanded == null)
								return 0;
							CRuntime.free(z.idata);
							z.idata = null;
							if (req_comp == s.img_n + 1 && req_comp != 3 && pal_img_n == 0 || has_trans != 0)
								s.img_out_n = s.img_n + 1;
							else
								s.img_out_n = s.img_n;
							if (stbi__create_png_image(z, z.expanded, raw_len, s.img_out_n, z.depth, color, interlace) == 0)
								return 0;
							if (has_trans != 0)
							{
								if (z.depth == 16)
								{
									if (stbi__compute_transparency16(z, tc16, s.img_out_n) == 0)
										return 0;
								}
								else
								{
									if (stbi__compute_transparency(z, tc, s.img_out_n) == 0)
										return 0;
								}
							}

							if (is_iphone != 0 &&
								(stbi__de_iphone_flag_set != 0
									? stbi__de_iphone_flag_local
									: stbi__de_iphone_flag_global) != 0 && s.img_out_n > 2)
								stbi__de_iphone(z);
							if (pal_img_n != 0)
							{
								s.img_n = pal_img_n;
								s.img_out_n = pal_img_n;
								if (req_comp >= 3)
									s.img_out_n = req_comp;
								if (stbi__expand_png_palette(z, palette, (int)pal_len, s.img_out_n) == 0)
									return 0;
							}
							else if (has_trans != 0)
							{
								++s.img_n;
							}

							CRuntime.free(z.expanded);
							z.expanded = null;
							stbi__get32be(s);
							return 1;
						}

					default:
						if (first != 0)
							return stbi__err("first not IHDR");
						if ((c.type & (1 << 29)) == 0)
						{
							stbi__parse_png_file_invalid_chunk[0] = (char)((c.type >> 24) & 255);
							stbi__parse_png_file_invalid_chunk[1] = (char)((c.type >> 16) & 255);
							stbi__parse_png_file_invalid_chunk[2] = (char)((c.type >> 8) & 255);
							stbi__parse_png_file_invalid_chunk[3] = (char)((c.type >> 0) & 255);
							return stbi__err(new string(stbi__parse_png_file_invalid_chunk));
						}

						stbi__skip(s, (int)c.length);
						break;
				}

				stbi__get32be(s);
			}
		}

		public static int stbi__png_info(stbi__context s, int* x, int* y, int* comp)
		{
			var p = new stbi__png();
			p.s = s;
			return stbi__png_info_raw(p, x, y, comp);
		}

		public static int stbi__png_info_raw(stbi__png p, int* x, int* y, int* comp)
		{
			if (stbi__parse_png_file(p, STBI__SCAN_header, 0) == 0)
			{
				stbi__rewind(p.s);
				return 0;
			}

			if (x != null)
				*x = (int)p.s.img_x;
			if (y != null)
				*y = (int)p.s.img_y;
			if (comp != null)
				*comp = p.s.img_n;
			return 1;
		}

		public static int stbi__png_is16(stbi__context s)
		{
			var p = new stbi__png();
			p.s = s;
			if (stbi__png_info_raw(p, null, null, null) == 0)
				return 0;
			if (p.depth != 16)
			{
				stbi__rewind(p.s);
				return 0;
			}

			return 1;
		}

		public static void* stbi__png_load(stbi__context s, int* x, int* y, int* comp, int req_comp,
			stbi__result_info* ri)
		{
			var p = new stbi__png();
			p.s = s;
			return stbi__do_png(p, x, y, comp, req_comp, ri);
		}

		public static int stbi__png_test(stbi__context s)
		{
			var r = 0;
			r = stbi__check_png_header(s);
			stbi__rewind(s);
			return r;
		}

		public class stbi__png
		{
			public byte* _out_;
			public int depth;
			public byte* expanded;
			public byte* idata;
			public stbi__context s;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbi__pngchunk
		{
			public uint length;
			public uint type;
		}
	}

	/* GIF */

    partial class StbImage
	{
		public static int stbi__gif_header(stbi__context s, stbi__gif g, int* comp, int is_info)
		{
			byte version = 0;
			if (stbi__get8(s) != 71 || stbi__get8(s) != 73 || stbi__get8(s) != 70 || stbi__get8(s) != 56)
				return stbi__err("not GIF");
			version = stbi__get8(s);
			if (version != 55 && version != 57)
				return stbi__err("not GIF");
			if (stbi__get8(s) != 97)
				return stbi__err("not GIF");
			stbi__g_failure_reason = "";
			g.w = stbi__get16le(s);
			g.h = stbi__get16le(s);
			g.flags = stbi__get8(s);
			g.bgindex = stbi__get8(s);
			g.ratio = stbi__get8(s);
			g.transparent = -1;
			if (g.w > 1 << 24)
				return stbi__err("too large");
			if (g.h > 1 << 24)
				return stbi__err("too large");
			if (comp != null)
				*comp = 4;
			if (is_info != 0)
				return 1;
			if ((g.flags & 0x80) != 0)
				stbi__gif_parse_colortable(s, g.pal, 2 << (g.flags & 7), -1);
			return 1;
		}

		public static int stbi__gif_info(stbi__context s, int* x, int* y, int* comp)
		{
			return stbi__gif_info_raw(s, x, y, comp);
		}

		public static int stbi__gif_info_raw(stbi__context s, int* x, int* y, int* comp)
		{
			var g = new stbi__gif();
			if (g == null)
				return stbi__err("outofmem");
			if (stbi__gif_header(s, g, comp, 1) == 0)
			{
				stbi__rewind(s);
				return 0;
			}

			if (x != null)
				*x = g.w;
			if (y != null)
				*y = g.h;
			return 1;
		}

		public static void* stbi__gif_load(stbi__context s, int* x, int* y, int* comp, int req_comp,
			stbi__result_info* ri)
		{
			byte* u = null;
			var g = new stbi__gif();
			u = stbi__gif_load_next(s, g, comp, req_comp, null);

			if (u != null)
			{
				*x = g.w;
				*y = g.h;
				if (req_comp != 0 && req_comp != 4)
					u = stbi__convert_format(u, 4, req_comp, (uint)g.w, (uint)g.h);
			}
			else if (g._out_ != null)
			{
				CRuntime.free(g._out_);
			}

			CRuntime.free(g.history);
			CRuntime.free(g.background);
			return u;
		}

		public static byte* stbi__gif_load_next(stbi__context s, stbi__gif g, int* comp, int req_comp, byte* two_back)
		{
			var dispose = 0;
			var first_frame = 0;
			var pi = 0;
			var pcount = 0;
			first_frame = 0;
			if (g._out_ == null)
			{
				if (stbi__gif_header(s, g, comp, 0) == 0)
					return null;
				if (stbi__mad3sizes_valid(4, g.w, g.h, 0) == 0)
					return (byte*)(ulong)(stbi__err("too large") != 0 ? 0 : 0);
				pcount = g.w * g.h;
				g._out_ = (byte*)stbi__malloc((ulong)(4 * pcount));
				g.background = (byte*)stbi__malloc((ulong)(4 * pcount));
				g.history = (byte*)stbi__malloc((ulong)pcount);
				if (g._out_ == null || g.background == null || g.history == null)
					return (byte*)(ulong)(stbi__err("outofmem") != 0 ? 0 : 0);
				CRuntime.memset(g._out_, 0x00, (ulong)(4 * pcount));
				CRuntime.memset(g.background, 0x00, (ulong)(4 * pcount));
				CRuntime.memset(g.history, 0x00, (ulong)pcount);
				first_frame = 1;
			}
			else
			{
				dispose = (g.eflags & 0x1C) >> 2;
				pcount = g.w * g.h;
				if (dispose == 3 && two_back == null) dispose = 2;

				if (dispose == 3)
				{
					for (pi = 0; pi < pcount; ++pi)
						if (g.history[pi] != 0)
							CRuntime.memcpy(&g._out_[pi * 4], &two_back[pi * 4], (ulong)4);
				}
				else if (dispose == 2)
				{
					for (pi = 0; pi < pcount; ++pi)
						if (g.history[pi] != 0)
							CRuntime.memcpy(&g._out_[pi * 4], &g.background[pi * 4], (ulong)4);
				}

				CRuntime.memcpy(g.background, g._out_, (ulong)(4 * g.w * g.h));
			}

			CRuntime.memset(g.history, 0x00, (ulong)(g.w * g.h));
			for (; ; )
			{
				int tag = stbi__get8(s);
				switch (tag)
				{
					case 0x2C:
						{
							var x = 0;
							var y = 0;
							var w = 0;
							var h = 0;
							byte* o;
							x = stbi__get16le(s);
							y = stbi__get16le(s);
							w = stbi__get16le(s);
							h = stbi__get16le(s);
							if (x + w > g.w || y + h > g.h)
								return (byte*)(ulong)(stbi__err("bad Image Descriptor") != 0 ? 0 : 0);
							g.line_size = g.w * 4;
							g.start_x = x * 4;
							g.start_y = y * g.line_size;
							g.max_x = g.start_x + w * 4;
							g.max_y = g.start_y + h * g.line_size;
							g.cur_x = g.start_x;
							g.cur_y = g.start_y;
							if (w == 0)
								g.cur_y = g.max_y;
							g.lflags = stbi__get8(s);
							if ((g.lflags & 0x40) != 0)
							{
								g.step = 8 * g.line_size;
								g.parse = 3;
							}
							else
							{
								g.step = g.line_size;
								g.parse = 0;
							}

							if ((g.lflags & 0x80) != 0)
							{
								stbi__gif_parse_colortable(s, g.lpal, 2 << (g.lflags & 7),
									(g.eflags & 0x01) != 0 ? g.transparent : -1);
								g.color_table = (byte*)g.lpal;
							}
							else if ((g.flags & 0x80) != 0)
							{
								g.color_table = (byte*)g.pal;
							}
							else
							{
								return (byte*)(ulong)(stbi__err("missing color table") != 0 ? 0 : 0);
							}

							o = stbi__process_gif_raster(s, g);
							if (o == null)
								return null;
							pcount = g.w * g.h;
							if (first_frame != 0 && g.bgindex > 0)
								for (pi = 0; pi < pcount; ++pi)
									if (g.history[pi] == 0)
									{
										g.pal[g.bgindex][3] = 255;
										CRuntime.memcpy(&g._out_[pi * 4], &g.pal[g.bgindex], (ulong)4);
									}

							return o;
						}

					case 0x21:
						{
							var len = 0;
							int ext = stbi__get8(s);
							if (ext == 0xF9)
							{
								len = stbi__get8(s);
								if (len == 4)
								{
									g.eflags = stbi__get8(s);
									g.delay = 10 * stbi__get16le(s);
									if (g.transparent >= 0) g.pal[g.transparent][3] = 255;

									if ((g.eflags & 0x01) != 0)
									{
										g.transparent = stbi__get8(s);
										if (g.transparent >= 0) g.pal[g.transparent][3] = 0;
									}
									else
									{
										stbi__skip(s, 1);
										g.transparent = -1;
									}
								}
								else
								{
									stbi__skip(s, len);
									break;
								}
							}

							while ((len = stbi__get8(s)) != 0) stbi__skip(s, len);

							break;
						}

					case 0x3B:
						return null;
					default:
						return (byte*)(ulong)(stbi__err("unknown code") != 0 ? 0 : 0);
				}
			}
		}

		public static void stbi__gif_parse_colortable(stbi__context s, byte** pal, int num_entries, int transp)
		{
			var i = 0;
			for (i = 0; i < num_entries; ++i)
			{
				pal[i][2] = stbi__get8(s);
				pal[i][1] = stbi__get8(s);
				pal[i][0] = stbi__get8(s);
				pal[i][3] = (byte)(transp == i ? 0 : 255);
			}
		}

		public static int stbi__gif_test(stbi__context s)
		{
			var r = stbi__gif_test_raw(s);
			stbi__rewind(s);
			return r;
		}

		public static int stbi__gif_test_raw(stbi__context s)
		{
			var sz = 0;
			if (stbi__get8(s) != 71 || stbi__get8(s) != 73 || stbi__get8(s) != 70 || stbi__get8(s) != 56)
				return 0;
			sz = stbi__get8(s);
			if (sz != 57 && sz != 55)
				return 0;
			if (stbi__get8(s) != 97)
				return 0;
			return 1;
		}

		public static void* stbi__load_gif_main(stbi__context s, int** delays, int* x, int* y, int* z, int* comp,
			int req_comp)
		{
			if (stbi__gif_test(s) != 0)
			{
				var layers = 0;
				byte* u = null;
				byte* _out_ = null;
				byte* two_back = null;
				var g = new stbi__gif();
				var stride = 0;
				var out_size = 0;
				var delays_size = 0;
				if (delays != null) *delays = null;

				do
				{
					u = stbi__gif_load_next(s, g, comp, req_comp, two_back);
					if (u != null)
					{
						*x = g.w;
						*y = g.h;
						++layers;
						stride = g.w * g.h * 4;
						if (_out_ != null)
						{
							void* tmp = (byte*)CRuntime.realloc(_out_, (ulong)(layers * stride));
							if (tmp == null) return stbi__load_gif_main_outofmem(g, _out_, delays);

							_out_ = (byte*)tmp;
							out_size = layers * stride;

							if (delays != null)
							{
								var new_delays = (int*)CRuntime.realloc(*delays, (ulong)(sizeof(int) * layers));
								if (new_delays == null)
									return stbi__load_gif_main_outofmem(g, _out_, delays);
								*delays = new_delays;
								delays_size = layers * sizeof(int);
							}
						}
						else
						{
							_out_ = (byte*)stbi__malloc((ulong)(layers * stride));
							if (_out_ == null)
								return stbi__load_gif_main_outofmem(g, _out_, delays);
							out_size = layers * stride;
							if (delays != null)
							{
								*delays = (int*)stbi__malloc((ulong)(layers * sizeof(int)));
								if (*delays == null)
									return stbi__load_gif_main_outofmem(g, _out_, delays);
								delays_size = layers * sizeof(int);
							}
						}

						CRuntime.memcpy(_out_ + (layers - 1) * stride, u, (ulong)stride);
						if (layers >= 2) two_back = _out_ - 2 * stride;

						if (delays != null) (*delays)[layers - 1U] = g.delay;
					}
				} while (u != null);

				CRuntime.free(g._out_);
				CRuntime.free(g.history);
				CRuntime.free(g.background);
				if (req_comp != 0 && req_comp != 4)
					_out_ = stbi__convert_format(_out_, 4, req_comp, (uint)(layers * g.w), (uint)g.h);
				*z = layers;
				return _out_;
			}

			return (byte*)(ulong)(stbi__err("not GIF") != 0 ? 0 : 0);
		}

		public static void* stbi__load_gif_main_outofmem(stbi__gif g, byte* _out_, int** delays)
		{
			CRuntime.free(g._out_);
			CRuntime.free(g.history);
			CRuntime.free(g.background);
			if (_out_ != null)
				CRuntime.free(_out_);
			if (delays != null && *delays != null)
				CRuntime.free(*delays);
			return (byte*)(ulong)(stbi__err("outofmem") != 0 ? 0 : 0);
		}

		public static void stbi__out_gif_code(stbi__gif g, ushort code)
		{
			byte* p;
			byte* c;
			var idx = 0;
			if (g.codes[code].prefix >= 0)
				stbi__out_gif_code(g, (ushort)g.codes[code].prefix);
			if (g.cur_y >= g.max_y)
				return;
			idx = g.cur_x + g.cur_y;
			p = &g._out_[idx];
			g.history[idx / 4] = 1;
			c = &g.color_table[g.codes[code].suffix * 4];
			if (c[3] > 128)
			{
				p[0] = c[2];
				p[1] = c[1];
				p[2] = c[0];
				p[3] = c[3];
			}

			g.cur_x += 4;
			if (g.cur_x >= g.max_x)
			{
				g.cur_x = g.start_x;
				g.cur_y += g.step;
				while (g.cur_y >= g.max_y && g.parse > 0)
				{
					g.step = (1 << g.parse) * g.line_size;
					g.cur_y = g.start_y + (g.step >> 1);
					--g.parse;
				}
			}
		}

		public static byte* stbi__process_gif_raster(stbi__context s, stbi__gif g)
		{
			byte lzw_cs = 0;
			var len = 0;
			var init_code = 0;
			uint first = 0;
			var codesize = 0;
			var codemask = 0;
			var avail = 0;
			var oldcode = 0;
			var bits = 0;
			var valid_bits = 0;
			var clear = 0;
			stbi__gif_lzw* p;
			lzw_cs = stbi__get8(s);
			if (lzw_cs > 12)
				return null;
			clear = 1 << lzw_cs;
			first = 1;
			codesize = lzw_cs + 1;
			codemask = (1 << codesize) - 1;
			bits = 0;
			valid_bits = 0;
			for (init_code = 0; init_code < clear; init_code++)
			{
				g.codes[init_code].prefix = -1;
				g.codes[init_code].first = (byte)init_code;
				g.codes[init_code].suffix = (byte)init_code;
			}

			avail = clear + 2;
			oldcode = -1;
			len = 0;
			for (; ; )
				if (valid_bits < codesize)
				{
					if (len == 0)
					{
						len = stbi__get8(s);
						if (len == 0)
							return g._out_;
					}

					--len;
					bits |= stbi__get8(s) << valid_bits;
					valid_bits += 8;
				}
				else
				{
					var code = bits & codemask;
					bits >>= codesize;
					valid_bits -= codesize;
					if (code == clear)
					{
						codesize = lzw_cs + 1;
						codemask = (1 << codesize) - 1;
						avail = clear + 2;
						oldcode = -1;
						first = 0;
					}
					else if (code == clear + 1)
					{
						stbi__skip(s, len);
						while ((len = stbi__get8(s)) > 0) stbi__skip(s, len);

						return g._out_;
					}
					else if (code <= avail)
					{
						if (first != 0) return (byte*)(ulong)(stbi__err("no clear code") != 0 ? 0 : 0);

						if (oldcode >= 0)
						{
							p = &g.codes[avail++];
							if (avail > 8192) return (byte*)(ulong)(stbi__err("too many codes") != 0 ? 0 : 0);

							p->prefix = (short)oldcode;
							p->first = g.codes[oldcode].first;
							p->suffix = code == avail ? p->first : g.codes[code].first;
						}
						else if (code == avail)
						{
							return (byte*)(ulong)(stbi__err("illegal code in raster") != 0 ? 0 : 0);
						}

						stbi__out_gif_code(g, (ushort)code);
						if ((avail & codemask) == 0 && avail <= 0x0FFF)
						{
							codesize++;
							codemask = (1 << codesize) - 1;
						}

						oldcode = code;
					}
					else
					{
						return (byte*)(ulong)(stbi__err("illegal code in raster") != 0 ? 0 : 0);
					}
				}
		}

		public class stbi__gif
		{
			public byte* _out_;
			public byte* background;
			public int bgindex;
			public stbi__gif_lzw* codes;
			public UnsafeArray1D<stbi__gif_lzw> codesArray = new UnsafeArray1D<stbi__gif_lzw>(8192);
			public byte* color_table;
			public int cur_x;
			public int cur_y;
			public int delay;
			public int eflags;
			public int flags;
			public int h;
			public byte* history;
			public int lflags;
			public int line_size;
			public byte** lpal;
			public UnsafeArray2D<byte> lpalArray = new UnsafeArray2D<byte>(256, 4);
			public int max_x;
			public int max_y;
			public byte** pal;
			public UnsafeArray2D<byte> palArray = new UnsafeArray2D<byte>(256, 4);
			public int parse;
			public int ratio;
			public int start_x;
			public int start_y;
			public int step;
			public int transparent;
			public int w;

			public stbi__gif()
			{
				pal = (byte**)palArray;
				lpal = (byte**)lpalArray;
				codes = (stbi__gif_lzw*)codesArray;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbi__gif_lzw
		{
			public short prefix;
			public byte first;
			public byte suffix;
		}
	}

	/* ZLIB */

    partial class StbImage
	{
		public static byte[] stbi__zdefault_distance =
			{5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5};

		public static byte[] stbi__zdefault_length =
		{
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
			9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
			9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
			9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
			9, 9, 9, 9, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 8, 8, 8, 8, 8, 8, 8, 8
		};

		public static int[] stbi__zdist_base =
		{
			1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097,
			6145, 8193, 12289, 16385, 24577, 0, 0
		};

		public static int[] stbi__zdist_extra =
			{0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13, 0, 0};

		public static int[] stbi__zlength_base =
		{
			3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31, 35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195,
			227, 258, 0, 0
		};

		public static int[] stbi__zlength_extra =
			{0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0, 0, 0};

		public static int stbi__compute_huffman_codes(stbi__zbuf* a)
		{
			var z_codelength = new stbi__zhuffman();
			var lencodes = stackalloc byte[455];
			var codelength_sizes = stackalloc byte[19];
			var i = 0;
			var n = 0;
			var hlit = (int)(stbi__zreceive(a, 5) + 257);
			var hdist = (int)(stbi__zreceive(a, 5) + 1);
			var hclen = (int)(stbi__zreceive(a, 4) + 4);
			var ntot = hlit + hdist;
			CRuntime.memset(codelength_sizes, 0, (ulong)(19 * sizeof(byte)));
			for (i = 0; i < hclen; ++i)
			{
				var s = (int)stbi__zreceive(a, 3);
				codelength_sizes[stbi__compute_huffman_codes_length_dezigzag[i]] = (byte)s;
			}

			if (stbi__zbuild_huffman(&z_codelength, codelength_sizes, 19) == 0)
				return 0;
			n = 0;
			while (n < ntot)
			{
				var c = stbi__zhuffman_decode(a, &z_codelength);
				if (c < 0 || c >= 19)
					return stbi__err("bad codelengths");
				if (c < 16)
				{
					lencodes[n++] = (byte)c;
				}
				else
				{
					byte fill = 0;
					if (c == 16)
					{
						c = (int)(stbi__zreceive(a, 2) + 3);
						if (n == 0)
							return stbi__err("bad codelengths");
						fill = lencodes[n - 1];
					}
					else if (c == 17)
					{
						c = (int)(stbi__zreceive(a, 3) + 3);
					}
					else if (c == 18)
					{
						c = (int)(stbi__zreceive(a, 7) + 11);
					}
					else
					{
						return stbi__err("bad codelengths");
					}

					if (ntot - n < c)
						return stbi__err("bad codelengths");
					CRuntime.memset(lencodes + n, fill, (ulong)c);
					n += c;
				}
			}

			if (n != ntot)
				return stbi__err("bad codelengths");
			if (stbi__zbuild_huffman(&a->z_length, lencodes, hlit) == 0)
				return 0;
			if (stbi__zbuild_huffman(&a->z_distance, lencodes + hlit, hdist) == 0)
				return 0;
			return 1;
		}

		public static int stbi__do_zlib(stbi__zbuf* a, sbyte* obuf, int olen, int exp, int parse_header)
		{
			a->zout_start = obuf;
			a->zout = obuf;
			a->zout_end = obuf + olen;
			a->z_expandable = exp;
			return stbi__parse_zlib(a, parse_header);
		}

		public static void stbi__fill_bits(stbi__zbuf* z)
		{
			do
			{
				if (z->code_buffer >= 1U << z->num_bits)
				{
					z->zbuffer = z->zbuffer_end;
					return;
				}

				z->code_buffer |= (uint)stbi__zget8(z) << z->num_bits;
				z->num_bits += 8;
			} while (z->num_bits <= 24);
		}

		public static int stbi__parse_huffman_block(stbi__zbuf* a)
		{
			var zout = a->zout;
			for (; ; )
			{
				var z = stbi__zhuffman_decode(a, &a->z_length);
				if (z < 256)
				{
					if (z < 0)
						return stbi__err("bad huffman code");
					if (zout >= a->zout_end)
					{
						if (stbi__zexpand(a, zout, 1) == 0)
							return 0;
						zout = a->zout;
					}

					*zout++ = (sbyte)z;
				}
				else
				{
					byte* p;
					var len = 0;
					var dist = 0;
					if (z == 256)
					{
						a->zout = zout;
						return 1;
					}

					z -= 257;
					len = stbi__zlength_base[z];
					if (stbi__zlength_extra[z] != 0)
						len += (int)stbi__zreceive(a, stbi__zlength_extra[z]);
					z = stbi__zhuffman_decode(a, &a->z_distance);
					if (z < 0)
						return stbi__err("bad huffman code");
					dist = stbi__zdist_base[z];
					if (stbi__zdist_extra[z] != 0)
						dist += (int)stbi__zreceive(a, stbi__zdist_extra[z]);
					if (zout - a->zout_start < dist)
						return stbi__err("bad dist");
					if (zout + len > a->zout_end)
					{
						if (stbi__zexpand(a, zout, len) == 0)
							return 0;
						zout = a->zout;
					}

					p = (byte*)(zout - dist);
					if (dist == 1)
					{
						var v = *p;
						if (len != 0)
							do
							{
								*zout++ = (sbyte)v;
							} while (--len != 0);
					}
					else
					{
						if (len != 0)
							do
							{
								*zout++ = (sbyte)*p++;
							} while (--len != 0);
					}
				}
			}
		}

		public static int stbi__parse_uncompressed_block(stbi__zbuf* a)
		{
			var header = stackalloc byte[4];
			var len = 0;
			var nlen = 0;
			var k = 0;
			if ((a->num_bits & 7) != 0)
				stbi__zreceive(a, a->num_bits & 7);
			k = 0;
			while (a->num_bits > 0)
			{
				header[k++] = (byte)(a->code_buffer & 255);
				a->code_buffer >>= 8;
				a->num_bits -= 8;
			}

			if (a->num_bits < 0)
				return stbi__err("zlib corrupt");
			while (k < 4) header[k++] = stbi__zget8(a);

			len = header[1] * 256 + header[0];
			nlen = header[3] * 256 + header[2];
			if (nlen != (len ^ 0xffff))
				return stbi__err("zlib corrupt");
			if (a->zbuffer + len > a->zbuffer_end)
				return stbi__err("read past buffer");
			if (a->zout + len > a->zout_end)
				if (stbi__zexpand(a, a->zout, len) == 0)
					return 0;
			CRuntime.memcpy(a->zout, a->zbuffer, (ulong)len);
			a->zbuffer += len;
			a->zout += len;
			return 1;
		}

		public static int stbi__parse_zlib(stbi__zbuf* a, int parse_header)
		{
			var final = 0;
			var type = 0;
			if (parse_header != 0)
				if (stbi__parse_zlib_header(a) == 0)
					return 0;
			a->num_bits = 0;
			a->code_buffer = 0;
			do
			{
				final = (int)stbi__zreceive(a, 1);
				type = (int)stbi__zreceive(a, 2);
				if (type == 0)
				{
					if (stbi__parse_uncompressed_block(a) == 0)
						return 0;
				}
				else if (type == 3)
				{
					return 0;
				}
				else
				{
					if (type == 1)
					{
						fixed (byte* b = stbi__zdefault_length)
						{
							if (stbi__zbuild_huffman(&a->z_length, b, 288) == 0)
								return 0;
						}

						fixed (byte* b = stbi__zdefault_distance)
						{
							if (stbi__zbuild_huffman(&a->z_distance, b, 32) == 0)
								return 0;
						}
					}
					else
					{
						if (stbi__compute_huffman_codes(a) == 0)
							return 0;
					}

					if (stbi__parse_huffman_block(a) == 0)
						return 0;
				}
			} while (final == 0);

			return 1;
		}

		public static int stbi__parse_zlib_header(stbi__zbuf* a)
		{
			int cmf = stbi__zget8(a);
			var cm = cmf & 15;
			int flg = stbi__zget8(a);
			if (stbi__zeof(a) != 0)
				return stbi__err("bad zlib header");
			if ((cmf * 256 + flg) % 31 != 0)
				return stbi__err("bad zlib header");
			if ((flg & 32) != 0)
				return stbi__err("no preset dict");
			if (cm != 8)
				return stbi__err("bad compression");
			return 1;
		}

		public static int stbi__zbuild_huffman(stbi__zhuffman* z, byte* sizelist, int num)
		{
			var i = 0;
			var k = 0;
			var code = 0;
			var next_code = stackalloc int[16];
			var sizes = stackalloc int[17];
			CRuntime.memset(sizes, 0, (ulong)(17 * sizeof(int)));
			CRuntime.memset(z->fast, 0, (ulong)(512 * sizeof(ushort)));
			for (i = 0; i < num; ++i) ++sizes[sizelist[i]];

			sizes[0] = 0;
			for (i = 1; i < 16; ++i)
				if (sizes[i] > 1 << i)
					return stbi__err("bad sizes");

			code = 0;
			for (i = 1; i < 16; ++i)
			{
				next_code[i] = code;
				z->firstcode[i] = (ushort)code;
				z->firstsymbol[i] = (ushort)k;
				code = code + sizes[i];
				if (sizes[i] != 0)
					if (code - 1 >= 1 << i)
						return stbi__err("bad codelengths");
				z->maxcode[i] = code << (16 - i);
				code <<= 1;
				k += sizes[i];
			}

			z->maxcode[16] = 0x10000;
			for (i = 0; i < num; ++i)
			{
				int s = sizelist[i];
				if (s != 0)
				{
					var c = next_code[s] - z->firstcode[s] + z->firstsymbol[s];
					var fastv = (ushort)((s << 9) | i);
					z->size[c] = (byte)s;
					z->value[c] = (ushort)i;
					if (s <= 9)
					{
						var j = stbi__bit_reverse(next_code[s], s);
						while (j < 1 << 9)
						{
							z->fast[j] = fastv;
							j += 1 << s;
						}
					}

					++next_code[s];
				}
			}

			return 1;
		}

		public static int stbi__zeof(stbi__zbuf* z)
		{
			return z->zbuffer >= z->zbuffer_end ? 1 : 0;
		}

		public static int stbi__zexpand(stbi__zbuf* z, sbyte* zout, int n)
		{
			sbyte* q;
			uint cur = 0;
			uint limit = 0;
			uint old_limit = 0;
			z->zout = zout;
			if (z->z_expandable == 0)
				return stbi__err("output buffer limit");
			cur = (uint)(z->zout - z->zout_start);
			limit = old_limit = (uint)(z->zout_end - z->zout_start);
			if (0xffffffff - cur < (uint)n)
				return stbi__err("outofmem");
			while (cur + n > limit)
			{
				if (limit > 0xffffffff / 2)
					return stbi__err("outofmem");
				limit *= 2;
			}

			q = (sbyte*)CRuntime.realloc(z->zout_start, (ulong)limit);
			if (q == null)
				return stbi__err("outofmem");
			z->zout_start = q;
			z->zout = q + cur;
			z->zout_end = q + limit;
			return 1;
		}

		public static byte stbi__zget8(stbi__zbuf* z)
		{
			return (byte)(stbi__zeof(z) != 0 ? 0 : *z->zbuffer++);
		}

		public static int stbi__zhuffman_decode(stbi__zbuf* a, stbi__zhuffman* z)
		{
			var b = 0;
			var s = 0;
			if (a->num_bits < 16)
			{
				if (stbi__zeof(a) != 0) return -1;

				stbi__fill_bits(a);
			}

			b = z->fast[a->code_buffer & ((1 << 9) - 1)];
			if (b != 0)
			{
				s = b >> 9;
				a->code_buffer >>= s;
				a->num_bits -= s;
				return b & 511;
			}

			return stbi__zhuffman_decode_slowpath(a, z);
		}

		public static int stbi__zhuffman_decode_slowpath(stbi__zbuf* a, stbi__zhuffman* z)
		{
			var b = 0;
			var s = 0;
			var k = 0;
			k = stbi__bit_reverse((int)a->code_buffer, 16);
			for (s = 9 + 1; ; ++s)
				if (k < z->maxcode[s])
					break;

			if (s >= 16)
				return -1;
			b = (k >> (16 - s)) - z->firstcode[s] + z->firstsymbol[s];
			if (b >= 288)
				return -1;
			if (z->size[b] != s)
				return -1;
			a->code_buffer >>= s;
			a->num_bits -= s;
			return z->value[b];
		}

		public static uint stbi__zreceive(stbi__zbuf* z, int n)
		{
			uint k = 0;
			if (z->num_bits < n)
				stbi__fill_bits(z);
			k = (uint)(z->code_buffer & ((1 << n) - 1));
			z->code_buffer >>= n;
			z->num_bits -= n;
			return k;
		}

		public static int stbi_zlib_decode_buffer(sbyte* obuffer, int olen, sbyte* ibuffer, int ilen)
		{
			var a = new stbi__zbuf();
			a.zbuffer = (byte*)ibuffer;
			a.zbuffer_end = (byte*)ibuffer + ilen;
			if (stbi__do_zlib(&a, obuffer, olen, 0, 1) != 0)
				return (int)(a.zout - a.zout_start);
			return -1;
		}

		public static sbyte* stbi_zlib_decode_malloc(sbyte* buffer, int len, int* outlen)
		{
			return stbi_zlib_decode_malloc_guesssize(buffer, len, 16384, outlen);
		}

		public static sbyte* stbi_zlib_decode_malloc_guesssize(sbyte* buffer, int len, int initial_size, int* outlen)
		{
			var a = new stbi__zbuf();
			var p = (sbyte*)stbi__malloc((ulong)initial_size);
			if (p == null)
				return null;
			a.zbuffer = (byte*)buffer;
			a.zbuffer_end = (byte*)buffer + len;
			if (stbi__do_zlib(&a, p, initial_size, 1, 1) != 0)
			{
				if (outlen != null)
					*outlen = (int)(a.zout - a.zout_start);
				return a.zout_start;
			}

			CRuntime.free(a.zout_start);
			return null;
		}

		public static sbyte* stbi_zlib_decode_malloc_guesssize_headerflag(sbyte* buffer, int len, int initial_size,
			int* outlen, int parse_header)
		{
			var a = new stbi__zbuf();
			var p = (sbyte*)stbi__malloc((ulong)initial_size);
			if (p == null)
				return null;
			a.zbuffer = (byte*)buffer;
			a.zbuffer_end = (byte*)buffer + len;
			if (stbi__do_zlib(&a, p, initial_size, 1, parse_header) != 0)
			{
				if (outlen != null)
					*outlen = (int)(a.zout - a.zout_start);
				return a.zout_start;
			}

			CRuntime.free(a.zout_start);
			return null;
		}

		public static int stbi_zlib_decode_noheader_buffer(sbyte* obuffer, int olen, sbyte* ibuffer, int ilen)
		{
			var a = new stbi__zbuf();
			a.zbuffer = (byte*)ibuffer;
			a.zbuffer_end = (byte*)ibuffer + ilen;
			if (stbi__do_zlib(&a, obuffer, olen, 0, 0) != 0)
				return (int)(a.zout - a.zout_start);
			return -1;
		}

		public static sbyte* stbi_zlib_decode_noheader_malloc(sbyte* buffer, int len, int* outlen)
		{
			var a = new stbi__zbuf();
			var p = (sbyte*)stbi__malloc(16384);
			if (p == null)
				return null;
			a.zbuffer = (byte*)buffer;
			a.zbuffer_end = (byte*)buffer + len;
			if (stbi__do_zlib(&a, p, 16384, 1, 0) != 0)
			{
				if (outlen != null)
					*outlen = (int)(a.zout - a.zout_start);
				return a.zout_start;
			}

			CRuntime.free(a.zout_start);
			return null;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbi__zbuf
		{
			public byte* zbuffer;
			public byte* zbuffer_end;
			public int num_bits;
			public uint code_buffer;
			public sbyte* zout;
			public sbyte* zout_start;
			public sbyte* zout_end;
			public int z_expandable;
			public stbi__zhuffman z_length;
			public stbi__zhuffman z_distance;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbi__zhuffman
		{
			public fixed ushort fast[512];
			public fixed ushort firstcode[16];
			public fixed int maxcode[17];
			public fixed ushort firstsymbol[16];
			public fixed byte size[288];
			public fixed ushort value[288];
		}
	}


	/* STB IMAGE WRITE */

	static partial class StbImageWrite
	{
		public static void stbiw__writefv(stbi__write_context s, string fmt, params object[] v)
		{
			var vindex = 0;
			for (var i = 0; i < fmt.Length; ++i)
			{
				var c = fmt[i];
				switch (c)
				{
					case ' ':
						break;
					case '1':
					{
						var x = (byte) ((int) v[vindex++] & 0xff);
						s.func(s.context, &x, 1);
						break;
					}
					case '2':
					{
						var x = (int) v[vindex++];
						var b = stackalloc byte[2];
						b[0] = (byte) (x & 0xff);
						b[1] = (byte) ((x >> 8) & 0xff);
						s.func(s.context, b, 2);
						break;
					}
					case '4':
					{
						var x = Convert.ToUInt32(v[vindex++]);
						var b = stackalloc byte[4];
						b[0] = (byte) (x & 0xff);
						b[1] = (byte) ((x >> 8) & 0xff);
						b[2] = (byte) ((x >> 16) & 0xff);
						b[3] = (byte) ((x >> 24) & 0xff);
						s.func(s.context, b, 4);
						break;
					}
				}
			}
		}

		public static void stbiw__writef(stbi__write_context s, string fmt, params object[] v)
		{
			stbiw__writefv(s, fmt, v);
		}

		public static int stbiw__outfile(stbi__write_context s, int rgb_dir, int vdir, int x, int y, int comp,
			int expand_mono, void* data, int alpha, int pad, string fmt, params object[] v)
		{
			if ((y < 0) || (x < 0))
			{
				return 0;
			}

			stbiw__writefv(s, fmt, v);
			stbiw__write_pixels(s, rgb_dir, vdir, x, y, comp, data, alpha, pad, expand_mono);
			return 1;
		}

		public static int stbi_write_hdr_core(stbi__write_context s, int x, int y, int comp, float* data)
		{
			if ((y <= 0) || (x <= 0) || (data == null))
			{
				return 0;
			}

			var scratch = (byte*) (CRuntime.malloc((ulong) (x*4)));

			int i;
			var header = "#?RADIANCE\n# Written by stb_image_write.h\nFORMAT=32-bit_rle_rgbe\n";
			var bytes = Encoding.UTF8.GetBytes(header);
			fixed (byte* ptr = bytes)
			{
				s.func(s.context, ((sbyte*) ptr), bytes.Length);
			}

			var str = $"EXPOSURE=          1.0000000000000\n\n-Y {y} +X {x}\n";
			bytes = Encoding.UTF8.GetBytes(str);
			fixed (byte* ptr = bytes)
			{
				s.func(s.context, ((sbyte*) ptr), bytes.Length);
			}
			for (i = 0; i < y; i++)
			{
				stbiw__write_hdr_scanline(s, x, comp, scratch, data + comp*i*x);
			}
			CRuntime.free(scratch);
			return 1;
		}
	}

    private partial class StbImageWrite
	{
		public delegate void delegate0(void* arg0, void* arg1, int arg2);

		public static int stbi__flip_vertically_on_write;
		public static int stbi_write_force_png_filter = -1;

		public static float[] stbi_write_jpg_core_aasf =
		{
			1.0f * 2.828427125f, 1.387039845f * 2.828427125f, 1.306562965f * 2.828427125f, 1.175875602f * 2.828427125f,
			1.0f * 2.828427125f, 0.785694958f * 2.828427125f, 0.541196100f * 2.828427125f, 0.275899379f * 2.828427125f
		};

		public static ushort[] stbi_write_jpg_core_fillBits = { 0x7F, 7 };

		public static byte[] stbi_write_jpg_core_head0 =
			{0xFF, 0xD8, 0xFF, 0xE0, 0, 0x10, 74, 70, 73, 70, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0, 0xFF, 0xDB, 0, 0x84, 0};

		public static byte[] stbi_write_jpg_core_head2 = { 0xFF, 0xDA, 0, 0xC, 3, 1, 0, 2, 0x11, 3, 0x11, 0, 0x3F, 0 };

		public static byte[] stbi_write_jpg_core_std_ac_chrominance_nrcodes =
			{0, 0, 2, 1, 2, 4, 4, 3, 4, 7, 5, 4, 4, 0, 1, 2, 0x77};

		public static byte[] stbi_write_jpg_core_std_ac_chrominance_values =
		{
			0x00, 0x01, 0x02, 0x03, 0x11, 0x04, 0x05, 0x21, 0x31, 0x06, 0x12, 0x41, 0x51, 0x07, 0x61, 0x71, 0x13, 0x22,
			0x32, 0x81, 0x08, 0x14, 0x42, 0x91, 0xa1, 0xb1, 0xc1, 0x09, 0x23, 0x33, 0x52, 0xf0, 0x15, 0x62, 0x72, 0xd1,
			0x0a, 0x16, 0x24, 0x34, 0xe1, 0x25, 0xf1, 0x17, 0x18, 0x19, 0x1a, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x35, 0x36,
			0x37, 0x38, 0x39, 0x3a, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58,
			0x59, 0x5a, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a,
			0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8a, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9a,
			0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6, 0xb7, 0xb8, 0xb9, 0xba,
			0xc2, 0xc3, 0xc4, 0xc5, 0xc6, 0xc7, 0xc8, 0xc9, 0xca, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda,
			0xe2, 0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9, 0xea, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8, 0xf9, 0xfa
		};

		public static byte[] stbi_write_jpg_core_std_ac_luminance_nrcodes =
			{0, 0, 2, 1, 3, 3, 2, 4, 3, 5, 5, 4, 4, 0, 0, 1, 0x7d};

		public static byte[] stbi_write_jpg_core_std_ac_luminance_values =
		{
			0x01, 0x02, 0x03, 0x00, 0x04, 0x11, 0x05, 0x12, 0x21, 0x31, 0x41, 0x06, 0x13, 0x51, 0x61, 0x07, 0x22, 0x71,
			0x14, 0x32, 0x81, 0x91, 0xa1, 0x08, 0x23, 0x42, 0xb1, 0xc1, 0x15, 0x52, 0xd1, 0xf0, 0x24, 0x33, 0x62, 0x72,
			0x82, 0x09, 0x0a, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x34, 0x35, 0x36, 0x37,
			0x38, 0x39, 0x3a, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59,
			0x5a, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0x83,
			0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8a, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9a, 0xa2, 0xa3,
			0xa4, 0xa5, 0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6, 0xb7, 0xb8, 0xb9, 0xba, 0xc2, 0xc3,
			0xc4, 0xc5, 0xc6, 0xc7, 0xc8, 0xc9, 0xca, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda, 0xe1, 0xe2,
			0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9, 0xea, 0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8, 0xf9, 0xfa
		};

		public static byte[] stbi_write_jpg_core_std_dc_chrominance_nrcodes =
			{0, 0, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0};

		public static byte[] stbi_write_jpg_core_std_dc_chrominance_values = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

		public static byte[] stbi_write_jpg_core_std_dc_luminance_nrcodes =
			{0, 0, 1, 5, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0};

		public static byte[] stbi_write_jpg_core_std_dc_luminance_values = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

		public static ushort[,] stbi_write_jpg_core_UVAC_HT =
		{
			{0, 2}, {1, 2}, {4, 3}, {10, 4}, {24, 5}, {25, 5}, {56, 6}, {120, 7}, {500, 9}, {1014, 10}, {4084, 12},
			{0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {11, 4}, {57, 6}, {246, 8}, {501, 9}, {2038, 11},
			{4085, 12}, {65416, 16}, {65417, 16}, {65418, 16}, {65419, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0},
			{0, 0}, {26, 5}, {247, 8}, {1015, 10}, {4086, 12}, {32706, 15}, {65420, 16}, {65421, 16}, {65422, 16},
			{65423, 16}, {65424, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {27, 5}, {248, 8}, {1016, 10},
			{4087, 12}, {65425, 16}, {65426, 16}, {65427, 16}, {65428, 16}, {65429, 16}, {65430, 16}, {0, 0}, {0, 0},
			{0, 0}, {0, 0}, {0, 0}, {0, 0}, {58, 6}, {502, 9}, {65431, 16}, {65432, 16}, {65433, 16}, {65434, 16},
			{65435, 16}, {65436, 16}, {65437, 16}, {65438, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {59, 6},
			{1017, 10}, {65439, 16}, {65440, 16}, {65441, 16}, {65442, 16}, {65443, 16}, {65444, 16}, {65445, 16},
			{65446, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {121, 7}, {2039, 11}, {65447, 16}, {65448, 16},
			{65449, 16}, {65450, 16}, {65451, 16}, {65452, 16}, {65453, 16}, {65454, 16}, {0, 0}, {0, 0}, {0, 0},
			{0, 0}, {0, 0}, {0, 0}, {122, 7}, {2040, 11}, {65455, 16}, {65456, 16}, {65457, 16}, {65458, 16},
			{65459, 16}, {65460, 16}, {65461, 16}, {65462, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0},
			{249, 8}, {65463, 16}, {65464, 16}, {65465, 16}, {65466, 16}, {65467, 16}, {65468, 16}, {65469, 16},
			{65470, 16}, {65471, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {503, 9}, {65472, 16},
			{65473, 16}, {65474, 16}, {65475, 16}, {65476, 16}, {65477, 16}, {65478, 16}, {65479, 16}, {65480, 16},
			{0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {504, 9}, {65481, 16}, {65482, 16}, {65483, 16},
			{65484, 16}, {65485, 16}, {65486, 16}, {65487, 16}, {65488, 16}, {65489, 16}, {0, 0}, {0, 0}, {0, 0},
			{0, 0}, {0, 0}, {0, 0}, {505, 9}, {65490, 16}, {65491, 16}, {65492, 16}, {65493, 16}, {65494, 16},
			{65495, 16}, {65496, 16}, {65497, 16}, {65498, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0},
			{506, 9}, {65499, 16}, {65500, 16}, {65501, 16}, {65502, 16}, {65503, 16}, {65504, 16}, {65505, 16},
			{65506, 16}, {65507, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {2041, 11}, {65508, 16},
			{65509, 16}, {65510, 16}, {65511, 16}, {65512, 16}, {65513, 16}, {65514, 16}, {65515, 16}, {65516, 16},
			{0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {16352, 14}, {65517, 16}, {65518, 16}, {65519, 16},
			{65520, 16}, {65521, 16}, {65522, 16}, {65523, 16}, {65524, 16}, {65525, 16}, {0, 0}, {0, 0}, {0, 0},
			{0, 0}, {0, 0}, {1018, 10}, {32707, 15}, {65526, 16}, {65527, 16}, {65528, 16}, {65529, 16}, {65530, 16},
			{65531, 16}, {65532, 16}, {65533, 16}, {65534, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}
		};

		public static ushort[,] stbi_write_jpg_core_UVDC_HT =
		{
			{0, 2}, {1, 2}, {2, 2}, {6, 3}, {14, 4}, {30, 5}, {62, 6}, {126, 7}, {254, 8}, {510, 9}, {1022, 10},
			{2046, 11}
		};

		public static int[] stbi_write_jpg_core_UVQT =
		{
			17, 18, 24, 47, 99, 99, 99, 99, 18, 21, 26, 66, 99, 99, 99, 99, 24, 26, 56, 99, 99, 99, 99, 99, 47, 66, 99,
			99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99,
			99, 99, 99, 99, 99, 99, 99, 99, 99, 99
		};

		public static ushort[,] stbi_write_jpg_core_YAC_HT =
		{
			{10, 4}, {0, 2}, {1, 2}, {4, 3}, {11, 4}, {26, 5}, {120, 7}, {248, 8}, {1014, 10}, {65410, 16}, {65411, 16},
			{0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {12, 4}, {27, 5}, {121, 7}, {502, 9}, {2038, 11},
			{65412, 16}, {65413, 16}, {65414, 16}, {65415, 16}, {65416, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0},
			{0, 0}, {28, 5}, {249, 8}, {1015, 10}, {4084, 12}, {65417, 16}, {65418, 16}, {65419, 16}, {65420, 16},
			{65421, 16}, {65422, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {58, 6}, {503, 9}, {4085, 12},
			{65423, 16}, {65424, 16}, {65425, 16}, {65426, 16}, {65427, 16}, {65428, 16}, {65429, 16}, {0, 0}, {0, 0},
			{0, 0}, {0, 0}, {0, 0}, {0, 0}, {59, 6}, {1016, 10}, {65430, 16}, {65431, 16}, {65432, 16}, {65433, 16},
			{65434, 16}, {65435, 16}, {65436, 16}, {65437, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0},
			{122, 7}, {2039, 11}, {65438, 16}, {65439, 16}, {65440, 16}, {65441, 16}, {65442, 16}, {65443, 16},
			{65444, 16}, {65445, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {123, 7}, {4086, 12}, {65446, 16},
			{65447, 16}, {65448, 16}, {65449, 16}, {65450, 16}, {65451, 16}, {65452, 16}, {65453, 16}, {0, 0}, {0, 0},
			{0, 0}, {0, 0}, {0, 0}, {0, 0}, {250, 8}, {4087, 12}, {65454, 16}, {65455, 16}, {65456, 16}, {65457, 16},
			{65458, 16}, {65459, 16}, {65460, 16}, {65461, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0},
			{504, 9}, {32704, 15}, {65462, 16}, {65463, 16}, {65464, 16}, {65465, 16}, {65466, 16}, {65467, 16},
			{65468, 16}, {65469, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {505, 9}, {65470, 16},
			{65471, 16}, {65472, 16}, {65473, 16}, {65474, 16}, {65475, 16}, {65476, 16}, {65477, 16}, {65478, 16},
			{0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {506, 9}, {65479, 16}, {65480, 16}, {65481, 16},
			{65482, 16}, {65483, 16}, {65484, 16}, {65485, 16}, {65486, 16}, {65487, 16}, {0, 0}, {0, 0}, {0, 0},
			{0, 0}, {0, 0}, {0, 0}, {1017, 10}, {65488, 16}, {65489, 16}, {65490, 16}, {65491, 16}, {65492, 16},
			{65493, 16}, {65494, 16}, {65495, 16}, {65496, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0},
			{1018, 10}, {65497, 16}, {65498, 16}, {65499, 16}, {65500, 16}, {65501, 16}, {65502, 16}, {65503, 16},
			{65504, 16}, {65505, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {2040, 11}, {65506, 16},
			{65507, 16}, {65508, 16}, {65509, 16}, {65510, 16}, {65511, 16}, {65512, 16}, {65513, 16}, {65514, 16},
			{0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {65515, 16}, {65516, 16}, {65517, 16}, {65518, 16},
			{65519, 16}, {65520, 16}, {65521, 16}, {65522, 16}, {65523, 16}, {65524, 16}, {0, 0}, {0, 0}, {0, 0},
			{0, 0}, {0, 0}, {2041, 11}, {65525, 16}, {65526, 16}, {65527, 16}, {65528, 16}, {65529, 16}, {65530, 16},
			{65531, 16}, {65532, 16}, {65533, 16}, {65534, 16}, {0, 0}, {0, 0}, {0, 0}, {0, 0}, {0, 0}
		};

		public static ushort[,] stbi_write_jpg_core_YDC_HT =
			{{0, 2}, {2, 3}, {3, 3}, {4, 3}, {5, 3}, {6, 3}, {14, 4}, {30, 5}, {62, 6}, {126, 7}, {254, 8}, {510, 9}};

		public static int[] stbi_write_jpg_core_YQT =
		{
			16, 11, 10, 16, 24, 40, 51, 61, 12, 12, 14, 19, 26, 58, 60, 55, 14, 13, 16, 24, 40, 57, 69, 56, 14, 17, 22,
			29, 51, 87, 80, 62, 18, 22, 37, 56, 68, 109, 103, 77, 24, 35, 55, 64, 81, 104, 113, 92, 49, 64, 78, 87, 103,
			121, 120, 101, 72, 92, 95, 98, 112, 100, 103, 99
		};

		public static int stbi_write_png_compression_level = 8;
		public static int stbi_write_tga_with_rle = 1;

		public static ushort[] stbi_zlib_compress_distc =
		{
			1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097,
			6145, 8193, 12289, 16385, 24577, 32768
		};

		public static byte[] stbi_zlib_compress_disteb =
			{0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13};

		public static ushort[] stbi_zlib_compress_lengthc =
		{
			3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31, 35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195,
			227, 258, 259
		};

		public static byte[] stbi_zlib_compress_lengtheb =
			{0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0};

		public static uint[] stbiw__crc32_crc_table =
		{
			0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA, 0x076DC419, 0x706AF48F, 0xE963A535, 0x9E6495A3, 0x0eDB8832,
			0x79DCB8A4, 0xE0D5E91E, 0x97D2D988, 0x09B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91, 0x1DB71064, 0x6AB020F2,
			0xF3B97148, 0x84BE41DE, 0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7, 0x136C9856, 0x646BA8C0, 0xFD62F97A,
			0x8A65C9EC, 0x14015C4F, 0x63066CD9, 0xFA0F3D63, 0x8D080DF5, 0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172,
			0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B, 0x35B5A8FA, 0x42B2986C, 0xDBBBC9D6, 0xACBCF940, 0x32D86CE3,
			0x45DF5C75, 0xDCD60DCF, 0xABD13D59, 0x26D930AC, 0x51DE003A, 0xC8D75180, 0xBFD06116, 0x21B4F4B5, 0x56B3C423,
			0xCFBA9599, 0xB8BDA50F, 0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924, 0x2F6F7C87, 0x58684C11, 0xC1611DAB,
			0xB6662D3D, 0x76DC4190, 0x01DB7106, 0x98D220BC, 0xEFD5102A, 0x71B18589, 0x06B6B51F, 0x9FBFE4A5, 0xE8B8D433,
			0x7807C9A2, 0x0F00F934, 0x9609A88E, 0xE10E9818, 0x7F6A0DBB, 0x086D3D2D, 0x91646C97, 0xE6635C01, 0x6B6B51F4,
			0x1C6C6162, 0x856530D8, 0xF262004E, 0x6C0695ED, 0x1B01A57B, 0x8208F4C1, 0xF50FC457, 0x65B0D9C6, 0x12B7E950,
			0x8BBEB8EA, 0xFCB9887C, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65, 0x4DB26158, 0x3AB551CE, 0xA3BC0074,
			0xD4BB30E2, 0x4ADFA541, 0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB, 0x4369E96A, 0x346ED9FC, 0xAD678846, 0xDA60B8D0,
			0x44042D73, 0x33031DE5, 0xAA0A4C5F, 0xDD0D7CC9, 0x5005713C, 0x270241AA, 0xBE0B1010, 0xC90C2086, 0x5768B525,
			0x206F85B3, 0xB966D409, 0xCE61E49F, 0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17, 0x2EB40D81,
			0xB7BD5C3B, 0xC0BA6CAD, 0xEDB88320, 0x9ABFB3B6, 0x03B6E20C, 0x74B1D29A, 0xEAD54739, 0x9DD277AF, 0x04DB2615,
			0x73DC1683, 0xE3630B12, 0x94643B84, 0x0D6D6A3E, 0x7A6A5AA8, 0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1,
			0xF00F9344, 0x8708A3D2, 0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB, 0x196C3671, 0x6E6B06E7, 0xFED41B76,
			0x89D32BE0, 0x10DA7A5A, 0x67DD4ACC, 0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5, 0xD6D6A3E8, 0xA1D1937E,
			0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1, 0xA6BC5767, 0x3FB506DD, 0x48B2364B, 0xD80D2BDA, 0xAF0A1B4C, 0x36034AF6,
			0x41047A60, 0xDF60EFC3, 0xA867DF55, 0x316E8EEF, 0x4669BE79, 0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236,
			0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F, 0xC5BA3BBE, 0xB2BD0B28, 0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7,
			0xB5D0CF31, 0x2CD99E8B, 0x5BDEAE1D, 0x9B64C2B0, 0xEC63F226, 0x756AA39C, 0x026D930A, 0x9C0906A9, 0xEB0E363F,
			0x72076785, 0x05005713, 0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38, 0x92D28E9B, 0xE5D5BE0D, 0x7CDCEFB7,
			0x0BDBDF21, 0x86D3D2D4, 0xF1D4E242, 0x68DDB3F8, 0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777,
			0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C, 0x8F659EFF, 0xF862AE69, 0x616BFFD3, 0x166CCF45, 0xA00AE278,
			0xD70DD2EE, 0x4E048354, 0x3903B3C2, 0xA7672661, 0xD06016F7, 0x4969474D, 0x3E6E77DB, 0xAED16A4A, 0xD9D65ADC,
			0x40DF0B66, 0x37D83BF0, 0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9, 0xBDBDF21C, 0xCABAC28A, 0x53B39330,
			0x24B4A3A6, 0xBAD03605, 0xCDD70693, 0x54DE5729, 0x23D967BF, 0xB3667A2E, 0xC4614AB8, 0x5D681B02, 0x2A6F2B94,
			0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D
		};

		public static int[] stbiw__encode_png_line_firstmap = { 0, 1, 0, 5, 6 };
		public static int[] stbiw__encode_png_line_mapping = { 0, 1, 2, 3, 4 };

		public static byte[] stbiw__jpg_ZigZag =
		{
			0, 1, 5, 6, 14, 15, 27, 28, 2, 4, 7, 13, 16, 26, 29, 42, 3, 8, 12, 17, 25, 30, 41, 43, 9, 11, 18, 24, 31,
			40, 44, 53, 10, 19, 23, 32, 39, 45, 52, 54, 20, 22, 33, 38, 46, 51, 55, 60, 21, 34, 37, 47, 50, 56, 59, 61,
			35, 36, 48, 49, 57, 58, 62, 63
		};

		public static void stbi__start_write_callbacks(stbi__write_context s, delegate0 c, void* context)
		{
			s.func = c;
			s.context = context;
		}

		public static void stbi_flip_vertically_on_write(int flag)
		{
			stbi__flip_vertically_on_write = flag;
		}

        private static int stbi_write_bmp_core(stbi__write_context s, int x, int y, int comp, void* data)
		{
			if (comp != 4)
			{
				var pad = (-x * 3) & 3;
				return stbiw__outfile(s, -1, -1, x, y, comp, 1, data, 0, pad, "11 4 22 44 44 22 444444", 66, 77,
					14 + 40 + (x * 3 + pad) * y, 0, 0, 14 + 40, 40, x, y, 1, 24, 0, 0, 0, 0, 0, 0);
			}

			return stbiw__outfile(s, -1, -1, x, y, comp, 1, data, 1, 0,
				"11 4 22 44 44 22 444444 4444 4 444 444 444 444", 66, 77, 14 + 108 + x * y * 4, 0, 0, 14 + 108, 108, x,
				y, 1, 32, 3, 0, 0, 0, 0, 0, 0xff0000, 0xff00, 0xff, 0xff000000u, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		}

		public static int stbi_write_bmp_to_func(delegate0 func, void* context, int x, int y, int comp, void* data)
		{
			var s = new stbi__write_context();
			stbi__start_write_callbacks(s, func, context);
			return stbi_write_bmp_core(s, x, y, comp, data);
		}

		public static int stbi_write_hdr_to_func(delegate0 func, void* context, int x, int y, int comp, float* data)
		{
			var s = new stbi__write_context();
			stbi__start_write_callbacks(s, func, context);
			return stbi_write_hdr_core(s, x, y, comp, data);
		}

		public static int stbi_write_jpg_core(stbi__write_context s, int width, int height, int comp, void* data,
			int quality)
		{
			var row = 0;
			var col = 0;
			var i = 0;
			var k = 0;
			var subsample = 0;
			var fdtbl_Y = stackalloc float[64];
			var fdtbl_UV = stackalloc float[64];
			var YTable = stackalloc byte[64];
			var UVTable = stackalloc byte[64];
			if (data == null || width == 0 || height == 0 || comp > 4 || comp < 1) return 0;

			quality = quality != 0 ? quality : 90;
			subsample = quality <= 90 ? 1 : 0;
			quality = quality < 1 ? 1 : quality > 100 ? 100 : quality;
			quality = quality < 50 ? 5000 / quality : 200 - quality * 2;
			for (i = 0; i < 64; ++i)
			{
				var uvti = 0;
				var yti = (stbi_write_jpg_core_YQT[i] * quality + 50) / 100;
				YTable[stbiw__jpg_ZigZag[i]] = (byte)(yti < 1 ? 1 : yti > 255 ? 255 : yti);
				uvti = (stbi_write_jpg_core_UVQT[i] * quality + 50) / 100;
				UVTable[stbiw__jpg_ZigZag[i]] = (byte)(uvti < 1 ? 1 : uvti > 255 ? 255 : uvti);
			}

			for (row = 0, k = 0; row < 8; ++row)
				for (col = 0; col < 8; ++col, ++k)
				{
					fdtbl_Y[k] = 1 / (YTable[stbiw__jpg_ZigZag[k]] * stbi_write_jpg_core_aasf[row] *
									  stbi_write_jpg_core_aasf[col]);
					fdtbl_UV[k] = 1 / (UVTable[stbiw__jpg_ZigZag[k]] * stbi_write_jpg_core_aasf[row] *
									   stbi_write_jpg_core_aasf[col]);
				}

			{
				var head1 = stackalloc byte[]
				{
					0xFF,
					0xC0,
					0,
					0x11,
					8,
					(byte)(height >> 8),
					(byte)(height & 0xff),
					(byte)(width >> 8),
					(byte)(width & 0xff),
					3,
					1,
					(byte)(subsample != 0 ? 0x22 : 0x11),
					0,
					2,
					0x11,
					1,
					3,
					0x11,
					1,
					0xFF,
					0xC4,
					0x01,
					0xA2,
					0
				};
				fixed (byte* ptr = stbi_write_jpg_core_head0)
				{
					s.func(s.context, ptr, 25 * sizeof(byte));
				}

				s.func(s.context, YTable, 64 * sizeof(byte));
				stbiw__putc(s, 1);
				s.func(s.context, UVTable, 64 * sizeof(byte));
				s.func(s.context, head1, 24 * sizeof(byte));
				fixed (byte* ptr = &stbi_write_jpg_core_std_dc_luminance_nrcodes[1])
				{
					s.func(s.context, ptr, 17 * sizeof(byte) - 1);
				}

				fixed (byte* ptr = stbi_write_jpg_core_std_dc_luminance_values)
				{
					s.func(s.context, ptr, 12 * sizeof(byte));
				}

				stbiw__putc(s, 0x10);

				fixed (byte* ptr = &stbi_write_jpg_core_std_ac_luminance_nrcodes[1])
				{
					s.func(s.context, ptr, 17 * sizeof(byte) - 1);
				}

				fixed (byte* ptr = stbi_write_jpg_core_std_ac_luminance_values)
				{
					s.func(s.context, ptr, 162 * sizeof(byte));
				}

				stbiw__putc(s, 1);
				fixed (byte* ptr = &stbi_write_jpg_core_std_dc_chrominance_nrcodes[1])
				{
					s.func(s.context, ptr, 17 * sizeof(byte) - 1);
				}

				fixed (byte* ptr = stbi_write_jpg_core_std_dc_chrominance_values)
				{
					s.func(s.context, ptr, 12 * sizeof(byte));
				}

				stbiw__putc(s, 0x11);

				fixed (byte* ptr = &stbi_write_jpg_core_std_ac_chrominance_nrcodes[1])
				{
					s.func(s.context, ptr, 17 * sizeof(byte) - 1);
				}

				fixed (byte* ptr = stbi_write_jpg_core_std_ac_chrominance_values)
				{
					s.func(s.context, ptr, 162 * sizeof(byte));
				}

				fixed (byte* ptr = stbi_write_jpg_core_head2)
				{
					s.func(s.context, ptr, 14 * sizeof(byte));
				}
			}

			{
				var DCY = 0;
				var DCU = 0;
				var DCV = 0;
				var bitBuf = 0;
				var bitCnt = 0;
				var ofsG = comp > 2 ? 1 : 0;
				var ofsB = comp > 2 ? 2 : 0;
				var dataR = (byte*)data;
				var dataG = dataR + ofsG;
				var dataB = dataR + ofsB;
				var x = 0;
				var y = 0;
				var pos = 0;
				if (subsample != 0)
				{
					var Y = stackalloc float[256];
					var U = stackalloc float[256];
					var V = stackalloc float[256];
					var subU = stackalloc float[64];
					var subV = stackalloc float[64];

					for (y = 0; y < height; y += 16)
						for (x = 0; x < width; x += 16)
						{
							for (row = y, pos = 0; row < y + 16; ++row)
							{
								var clamped_row = row < height ? row : height - 1;
								var base_p =
									(stbi__flip_vertically_on_write != 0 ? height - 1 - clamped_row : clamped_row) * width *
									comp;
								for (col = x; col < x + 16; ++col, ++pos)
								{
									var p = base_p + (col < width ? col : width - 1) * comp;
									float r = dataR[p];
									float g = dataG[p];
									float b = dataB[p];
									Y[pos] = +0.29900f * r + 0.58700f * g + 0.11400f * b - 128;
									U[pos] = -0.16874f * r - 0.33126f * g + 0.50000f * b;
									V[pos] = +0.50000f * r - 0.41869f * g - 0.08131f * b;
								}
							}

							DCY = stbiw__jpg_processDU(s, &bitBuf, &bitCnt, Y + 0, 16, fdtbl_Y, DCY,
								stbi_write_jpg_core_YDC_HT, stbi_write_jpg_core_YAC_HT);
							DCY = stbiw__jpg_processDU(s, &bitBuf, &bitCnt, Y + 8, 16, fdtbl_Y, DCY,
								stbi_write_jpg_core_YDC_HT, stbi_write_jpg_core_YAC_HT);
							DCY = stbiw__jpg_processDU(s, &bitBuf, &bitCnt, Y + 128, 16, fdtbl_Y, DCY,
								stbi_write_jpg_core_YDC_HT, stbi_write_jpg_core_YAC_HT);
							DCY = stbiw__jpg_processDU(s, &bitBuf, &bitCnt, Y + 136, 16, fdtbl_Y, DCY,
								stbi_write_jpg_core_YDC_HT, stbi_write_jpg_core_YAC_HT);
							{
								var yy = 0;
								var xx = 0;
								for (yy = 0, pos = 0; yy < 8; ++yy)
									for (xx = 0; xx < 8; ++xx, ++pos)
									{
										var j = yy * 32 + xx * 2;
										subU[pos] = (U[j + 0] + U[j + 1] + U[j + 16] + U[j + 17]) * 0.25f;
										subV[pos] = (V[j + 0] + V[j + 1] + V[j + 16] + V[j + 17]) * 0.25f;
									}

								DCU = stbiw__jpg_processDU(s, &bitBuf, &bitCnt, subU, 8, fdtbl_UV, DCU,
									stbi_write_jpg_core_UVDC_HT, stbi_write_jpg_core_UVAC_HT);
								DCV = stbiw__jpg_processDU(s, &bitBuf, &bitCnt, subV, 8, fdtbl_UV, DCV,
									stbi_write_jpg_core_UVDC_HT, stbi_write_jpg_core_UVAC_HT);
							}
						}
				}
				else
				{
					var Y = stackalloc float[64];
					var U = stackalloc float[64];
					var V = stackalloc float[64];

					for (y = 0; y < height; y += 8)
						for (x = 0; x < width; x += 8)
						{
							for (row = y, pos = 0; row < y + 8; ++row)
							{
								var clamped_row = row < height ? row : height - 1;
								var base_p =
									(stbi__flip_vertically_on_write != 0 ? height - 1 - clamped_row : clamped_row) * width *
									comp;
								for (col = x; col < x + 8; ++col, ++pos)
								{
									var p = base_p + (col < width ? col : width - 1) * comp;
									float r = dataR[p];
									float g = dataG[p];
									float b = dataB[p];
									Y[pos] = +0.29900f * r + 0.58700f * g + 0.11400f * b - 128;
									U[pos] = -0.16874f * r - 0.33126f * g + 0.50000f * b;
									V[pos] = +0.50000f * r - 0.41869f * g - 0.08131f * b;
								}
							}

							DCY = stbiw__jpg_processDU(s, &bitBuf, &bitCnt, Y, 8, fdtbl_Y, DCY, stbi_write_jpg_core_YDC_HT,
								stbi_write_jpg_core_YAC_HT);
							DCU = stbiw__jpg_processDU(s, &bitBuf, &bitCnt, U, 8, fdtbl_UV, DCU,
								stbi_write_jpg_core_UVDC_HT, stbi_write_jpg_core_UVAC_HT);
							DCV = stbiw__jpg_processDU(s, &bitBuf, &bitCnt, V, 8, fdtbl_UV, DCV,
								stbi_write_jpg_core_UVDC_HT, stbi_write_jpg_core_UVAC_HT);
						}
				}

				stbiw__jpg_writeBits(s, &bitBuf, &bitCnt,
					stbi_write_jpg_core_fillBits[0],
					stbi_write_jpg_core_fillBits[1]);
			}

			stbiw__putc(s, 0xFF);
			stbiw__putc(s, 0xD9);
			return 1;
		}

		public static int stbi_write_jpg_to_func(delegate0 func, void* context, int x, int y, int comp, void* data,
			int quality)
		{
			var s = new stbi__write_context();
			stbi__start_write_callbacks(s, func, context);
			return stbi_write_jpg_core(s, x, y, comp, data, quality);
		}

		public static int stbi_write_png_to_func(delegate0 func, void* context, int x, int y, int comp, void* data,
			int stride_bytes)
		{
			var len = 0;
			var png = stbi_write_png_to_mem((byte*)data, stride_bytes, x, y, comp, &len);
			if (png == null)
				return 0;
			func(context, png, len);
			CRuntime.free(png);
			return 1;
		}

		public static byte* stbi_write_png_to_mem(byte* pixels, int stride_bytes, int x, int y, int n, int* out_len)
		{
			var force_filter = stbi_write_force_png_filter;
			var ctype = stackalloc int[] { -1, 0, 4, 2, 6 };
			var sig = stackalloc byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
			byte* _out_;
			byte* o;
			byte* filt;
			byte* zlib;
			sbyte* line_buffer;
			var j = 0;
			var zlen = 0;
			if (stride_bytes == 0)
				stride_bytes = x * n;
			if (force_filter >= 5) force_filter = -1;

			filt = (byte*)CRuntime.malloc((ulong)((x * n + 1) * y));
			if (filt == null)
				return null;
			line_buffer = (sbyte*)CRuntime.malloc((ulong)(x * n));
			if (line_buffer == null)
			{
				CRuntime.free(filt);
				return null;
			}

			for (j = 0; j < y; ++j)
			{
				var filter_type = 0;
				if (force_filter > -1)
				{
					filter_type = force_filter;
					stbiw__encode_png_line(pixels, stride_bytes, x, y, j, n, force_filter, line_buffer);
				}
				else
				{
					var best_filter = 0;
					var best_filter_val = 0x7fffffff;
					var est = 0;
					var i = 0;
					for (filter_type = 0; filter_type < 5; filter_type++)
					{
						stbiw__encode_png_line(pixels, stride_bytes, x, y, j, n, filter_type, line_buffer);
						est = 0;
						for (i = 0; i < x * n; ++i) est += CRuntime.abs(line_buffer[i]);

						if (est < best_filter_val)
						{
							best_filter_val = est;
							best_filter = filter_type;
						}
					}

					if (filter_type != best_filter)
					{
						stbiw__encode_png_line(pixels, stride_bytes, x, y, j, n, best_filter, line_buffer);
						filter_type = best_filter;
					}
				}

				filt[j * (x * n + 1)] = (byte)filter_type;
				CRuntime.memmove(filt + j * (x * n + 1) + 1, line_buffer, (ulong)(x * n));
			}

			CRuntime.free(line_buffer);
			zlib = stbi_zlib_compress(filt, y * (x * n + 1), &zlen, stbi_write_png_compression_level);
			CRuntime.free(filt);
			if (zlib == null)
				return null;
			_out_ = (byte*)CRuntime.malloc((ulong)(8 + 12 + 13 + 12 + zlen + 12));
			if (_out_ == null)
				return null;
			*out_len = 8 + 12 + 13 + 12 + zlen + 12;
			o = _out_;
			CRuntime.memmove(o, sig, (ulong)8);
			o += 8;
			o[0] = (13 >> 24) & 0xff;
			o[1] = (13 >> 16) & 0xff;
			o[2] = (13 >> 8) & 0xff;
			o[3] = 13 & 0xff;
			o += 4;
			o[0] = (byte)("IHDR"[0] & 0xff);
			o[1] = (byte)("IHDR"[1] & 0xff);
			o[2] = (byte)("IHDR"[2] & 0xff);
			o[3] = (byte)("IHDR"[3] & 0xff);
			o += 4;
			o[0] = (byte)((x >> 24) & 0xff);
			o[1] = (byte)((x >> 16) & 0xff);
			o[2] = (byte)((x >> 8) & 0xff);
			o[3] = (byte)(x & 0xff);
			o += 4;
			o[0] = (byte)((y >> 24) & 0xff);
			o[1] = (byte)((y >> 16) & 0xff);
			o[2] = (byte)((y >> 8) & 0xff);
			o[3] = (byte)(y & 0xff);
			o += 4;
			*o++ = 8;
			*o++ = (byte)(ctype[n] & 0xff);
			*o++ = 0;
			*o++ = 0;
			*o++ = 0;
			stbiw__wpcrc(&o, 13);
			o[0] = (byte)((zlen >> 24) & 0xff);
			o[1] = (byte)((zlen >> 16) & 0xff);
			o[2] = (byte)((zlen >> 8) & 0xff);
			o[3] = (byte)(zlen & 0xff);
			o += 4;
			o[0] = (byte)("IDAT"[0] & 0xff);
			o[1] = (byte)("IDAT"[1] & 0xff);
			o[2] = (byte)("IDAT"[2] & 0xff);
			o[3] = (byte)("IDAT"[3] & 0xff);
			o += 4;
			CRuntime.memmove(o, zlib, (ulong)zlen);
			o += zlen;
			CRuntime.free(zlib);
			stbiw__wpcrc(&o, zlen);
			o[0] = (0 >> 24) & 0xff;
			o[1] = (0 >> 16) & 0xff;
			o[2] = (0 >> 8) & 0xff;
			o[3] = 0 & 0xff;
			o += 4;
			o[0] = (byte)("IEND"[0] & 0xff);
			o[1] = (byte)("IEND"[1] & 0xff);
			o[2] = (byte)("IEND"[2] & 0xff);
			o[3] = (byte)("IEND"[3] & 0xff);
			o += 4;
			stbiw__wpcrc(&o, 0);
			return _out_;
		}

		public static int stbi_write_tga_core(stbi__write_context s, int x, int y, int comp, void* data)
		{
			var has_alpha = comp == 2 || comp == 4 ? 1 : 0;
			var colorbytes = has_alpha != 0 ? comp - 1 : comp;
			var format = colorbytes < 2 ? 3 : 2;
			if (y < 0 || x < 0)
				return 0;
			if (stbi_write_tga_with_rle == 0)
				return stbiw__outfile(s, -1, -1, x, y, comp, 0, data, has_alpha, 0, "111 221 2222 11", 0, 0, format, 0,
					0, 0, 0, 0, x, y, (colorbytes + has_alpha) * 8, has_alpha * 8);

			var i = 0;
			var j = 0;
			var k = 0;
			var jend = 0;
			var jdir = 0;
			stbiw__writef(s, "111 221 2222 11", 0, 0, format + 8, 0, 0, 0, 0, 0, x, y, (colorbytes + has_alpha) * 8,
				has_alpha * 8);
			if (stbi__flip_vertically_on_write != 0)
			{
				j = 0;
				jend = y;
				jdir = 1;
			}
			else
			{
				j = y - 1;
				jend = -1;
				jdir = -1;
			}

			for (; j != jend; j += jdir)
			{
				var row = (byte*)data + j * x * comp;
				var len = 0;
				for (i = 0; i < x; i += len)
				{
					var begin = row + i * comp;
					var diff = 1;
					len = 1;
					if (i < x - 1)
					{
						++len;
						diff = CRuntime.memcmp(begin, row + (i + 1) * comp, (ulong)comp);
						if (diff != 0)
						{
							var prev = begin;
							for (k = i + 2; k < x && len < 128; ++k)
								if (CRuntime.memcmp(prev, row + k * comp, (ulong)comp) != 0)
								{
									prev += comp;
									++len;
								}
								else
								{
									--len;
									break;
								}
						}
						else
						{
							for (k = i + 2; k < x && len < 128; ++k)
								if (CRuntime.memcmp(begin, row + k * comp, (ulong)comp) == 0)
									++len;
								else
									break;
						}
					}

					if (diff != 0)
					{
						var header = (byte)((len - 1) & 0xff);
						stbiw__write1(s, header);
						for (k = 0; k < len; ++k) stbiw__write_pixel(s, -1, comp, has_alpha, 0, begin + k * comp);
					}
					else
					{
						var header = (byte)((len - 129) & 0xff);
						stbiw__write1(s, header);
						stbiw__write_pixel(s, -1, comp, has_alpha, 0, begin);
					}
				}
			}

			stbiw__write_flush(s);

			return 1;
		}

		public static int stbi_write_tga_to_func(delegate0 func, void* context, int x, int y, int comp, void* data)
		{
			var s = new stbi__write_context();
			stbi__start_write_callbacks(s, func, context);
			return stbi_write_tga_core(s, x, y, comp, data);
		}

		public static byte* stbi_zlib_compress(byte* data, int data_len, int* out_len, int quality)
		{
			uint bitbuf = 0;
			var i = 0;
			var j = 0;
			var bitcount = 0;
			byte* _out_ = null;
			var hash_table = (byte***)CRuntime.malloc((ulong)(16384 * sizeof(byte**)));
			if (hash_table == null)
				return null;
			if (quality < 5)
				quality = 5;
			if (_out_ == null || ((int*)_out_ - 2)[1] + 1 >= ((int*)_out_ - 2)[0])
				stbiw__sbgrowf((void**)&_out_, 1, sizeof(byte));

			_out_[((int*)_out_ - 2)[1]++] = 0x78;
			if (_out_ == null || ((int*)_out_ - 2)[1] + 1 >= ((int*)_out_ - 2)[0])
				stbiw__sbgrowf((void**)&_out_, 1, sizeof(byte));

			_out_[((int*)_out_ - 2)[1]++] = 0x5e;
			{
				bitbuf |= (uint)(1 << bitcount);
				bitcount += 1;
				_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
			}

			{
				bitbuf |= (uint)(1 << bitcount);
				bitcount += 2;
				_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
			}

			for (i = 0; i < 16384; ++i) hash_table[i] = null;

			i = 0;
			while (i < data_len - 3)
			{
				var h = (int)(stbiw__zhash(data + i) & (16384 - 1));
				var best = 3;
				byte* bestloc = null;
				var hlist = hash_table[h];
				var n = hlist != null ? ((int*)hlist - 2)[1] : 0;
				for (j = 0; j < n; ++j)
					if (hlist[j] - data > i - 32768)
					{
						var d = (int)stbiw__zlib_countm(hlist[j], data + i, data_len - i);
						if (d >= best)
						{
							best = d;
							bestloc = hlist[j];
						}
					}

				if (hash_table[h] != null && ((int*)hash_table[h] - 2)[1] == 2 * quality)
				{
					CRuntime.memmove(hash_table[h], hash_table[h] + quality, (ulong)(sizeof(byte*) * quality));
					((int*)hash_table[h] - 2)[1] = quality;
				}

				if (hash_table[h] == null || ((int*)hash_table[h] - 2)[1] + 1 >= ((int*)hash_table[h] - 2)[0])
					stbiw__sbgrowf((void**)&hash_table[h], 1, sizeof(byte*));

				hash_table[h][((int*)hash_table[h] - 2)[1]++] = data + i;
				if (bestloc != null)
				{
					h = (int)(stbiw__zhash(data + i + 1) & (16384 - 1));
					hlist = hash_table[h];
					n = hlist != null ? ((int*)hlist - 2)[1] : 0;
					for (j = 0; j < n; ++j)
						if (hlist[j] - data > i - 32767)
						{
							var e = (int)stbiw__zlib_countm(hlist[j], data + i + 1, data_len - i - 1);
							if (e > best)
							{
								bestloc = null;
								break;
							}
						}
				}

				if (bestloc != null)
				{
					var d = (int)(data + i - bestloc);
					for (j = 0; best > stbi_zlib_compress_lengthc[j + 1] - 1; ++j)
					{
					}

					if (j + 257 <= 143)
					{
						bitbuf |= (uint)(stbiw__zlib_bitrev(0x30 + j + 257, 8) << bitcount);
						bitcount += 8;
						_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
					}
					else if (j + 257 <= 255)
					{
						bitbuf |= (uint)(stbiw__zlib_bitrev(0x190 + j + 257 - 144, 9) << bitcount);
						bitcount += 9;
						_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
					}
					else if (j + 257 <= 279)
					{
						bitbuf |= (uint)(stbiw__zlib_bitrev(0 + j + 257 - 256, 7) << bitcount);
						bitcount += 7;
						_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
					}
					else
					{
						bitbuf |= (uint)(stbiw__zlib_bitrev(0xc0 + j + 257 - 280, 8) << bitcount);
						bitcount += 8;
						_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
					}

					if (stbi_zlib_compress_lengtheb[j] != 0)
					{
						bitbuf |= (uint)((best - stbi_zlib_compress_lengthc[j]) << bitcount);
						bitcount += stbi_zlib_compress_lengtheb[j];
						_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
					}

					for (j = 0; d > stbi_zlib_compress_distc[j + 1] - 1; ++j)
					{
					}

					{
						bitbuf |= (uint)(stbiw__zlib_bitrev(j, 5) << bitcount);
						bitcount += 5;
						_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
					}

					if (stbi_zlib_compress_disteb[j] != 0)
					{
						bitbuf |= (uint)((d - stbi_zlib_compress_distc[j]) << bitcount);
						bitcount += stbi_zlib_compress_disteb[j];
						_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
					}

					i += best;
				}
				else
				{
					if (data[i] <= 143)
					{
						bitbuf |= (uint)(stbiw__zlib_bitrev(0x30 + data[i], 8) << bitcount);
						bitcount += 8;
						_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
					}
					else
					{
						bitbuf |= (uint)(stbiw__zlib_bitrev(0x190 + data[i] - 144, 9) << bitcount);
						bitcount += 9;
						_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
					}

					++i;
				}
			}

			for (; i < data_len; ++i)
				if (data[i] <= 143)
				{
					bitbuf |= (uint)(stbiw__zlib_bitrev(0x30 + data[i], 8) << bitcount);
					bitcount += 8;
					_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
				}
				else
				{
					bitbuf |= (uint)(stbiw__zlib_bitrev(0x190 + data[i] - 144, 9) << bitcount);
					bitcount += 9;
					_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
				}

			if (256 <= 143)
			{
				bitbuf |= (uint)(stbiw__zlib_bitrev(0x30 + 256, 8) << bitcount);
				bitcount += 8;
				_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
			}

			if (256 <= 255)
			{
				bitbuf |= (uint)(stbiw__zlib_bitrev(0x190 + 256 - 144, 9) << bitcount);
				bitcount += 9;
				_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
			}

			if (256 <= 279)
			{
				bitbuf |= (uint)(stbiw__zlib_bitrev(0 + 256 - 256, 7) << bitcount);
				bitcount += 7;
				_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
			}
			else
			{
				bitbuf |= (uint)(stbiw__zlib_bitrev(0xc0 + 256 - 280, 8) << bitcount);
				bitcount += 8;
				_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
			}

			while (bitcount != 0)
			{
				bitbuf |= (uint)(0 << bitcount);
				bitcount += 1;
				_out_ = stbiw__zlib_flushf(_out_, &bitbuf, &bitcount);
			}

			for (i = 0; i < 16384; ++i)
				if (hash_table[i] != null)
					CRuntime.free((int*)hash_table[i] - 2);

			CRuntime.free(hash_table);
			if (((int*)_out_ - 2)[1] > data_len + 2 + (data_len + 32766) / 32767 * 5)
			{
				((int*)_out_ - 2)[1] = 2;
				for (j = 0; j < data_len;)
				{
					var blocklen = data_len - j;
					if (blocklen > 32767)
						blocklen = 32767;
					if (_out_ == null || ((int*)_out_ - 2)[1] + 1 >= ((int*)_out_ - 2)[0])
						stbiw__sbgrowf((void**)&_out_, 1, sizeof(byte));

					_out_[((int*)_out_ - 2)[1]++] = (byte)(data_len - j == blocklen ? 1 : 0);
					if (_out_ == null || ((int*)_out_ - 2)[1] + 1 >= ((int*)_out_ - 2)[0])
						stbiw__sbgrowf((void**)&_out_, 1, sizeof(byte));

					_out_[((int*)_out_ - 2)[1]++] = (byte)(blocklen & 0xff);
					if (_out_ == null || ((int*)_out_ - 2)[1] + 1 >= ((int*)_out_ - 2)[0])
						stbiw__sbgrowf((void**)&_out_, 1, sizeof(byte));

					_out_[((int*)_out_ - 2)[1]++] = (byte)((blocklen >> 8) & 0xff);
					if (_out_ == null || ((int*)_out_ - 2)[1] + 1 >= ((int*)_out_ - 2)[0])
						stbiw__sbgrowf((void**)&_out_, 1, sizeof(byte));

					_out_[((int*)_out_ - 2)[1]++] = (byte)(~blocklen & 0xff);
					if (_out_ == null || ((int*)_out_ - 2)[1] + 1 >= ((int*)_out_ - 2)[0])
						stbiw__sbgrowf((void**)&_out_, 1, sizeof(byte));

					_out_[((int*)_out_ - 2)[1]++] = (byte)((~blocklen >> 8) & 0xff);
					CRuntime.memcpy(_out_ + ((int*)_out_ - 2)[1], data + j, (ulong)blocklen);
					((int*)_out_ - 2)[1] += blocklen;
					j += blocklen;
				}
			}

			{
				uint s1 = 1;
				uint s2 = 0;
				var blocklen = data_len % 5552;
				j = 0;
				while (j < data_len)
				{
					for (i = 0; i < blocklen; ++i)
					{
						s1 += data[j + i];
						s2 += s1;
					}

					s1 %= 65521;
					s2 %= 65521;
					j += blocklen;
					blocklen = 5552;
				}

				if (_out_ == null || ((int*)_out_ - 2)[1] + 1 >= ((int*)_out_ - 2)[0])
					stbiw__sbgrowf((void**)&_out_, 1, sizeof(byte));

				_out_[((int*)_out_ - 2)[1]++] = (byte)((s2 >> 8) & 0xff);
				if (_out_ == null || ((int*)_out_ - 2)[1] + 1 >= ((int*)_out_ - 2)[0])
					stbiw__sbgrowf((void**)&_out_, 1, sizeof(byte));

				_out_[((int*)_out_ - 2)[1]++] = (byte)(s2 & 0xff);
				if (_out_ == null || ((int*)_out_ - 2)[1] + 1 >= ((int*)_out_ - 2)[0])
					stbiw__sbgrowf((void**)&_out_, 1, sizeof(byte));

				_out_[((int*)_out_ - 2)[1]++] = (byte)((s1 >> 8) & 0xff);
				if (_out_ == null || ((int*)_out_ - 2)[1] + 1 >= ((int*)_out_ - 2)[0])
					stbiw__sbgrowf((void**)&_out_, 1, sizeof(byte));

				_out_[((int*)_out_ - 2)[1]++] = (byte)(s1 & 0xff);
			}

			*out_len = ((int*)_out_ - 2)[1];
			CRuntime.memmove((int*)_out_ - 2, _out_, (ulong)*out_len);
			return (byte*)((int*)_out_ - 2);
		}

		public static uint stbiw__crc32(byte* buffer, int len)
		{
			var crc = ~0u;
			var i = 0;
			for (i = 0; i < len; ++i) crc = (crc >> 8) ^ stbiw__crc32_crc_table[buffer[i] ^ (crc & 0xff)];

			return ~crc;
		}

		public static void stbiw__encode_png_line(byte* pixels, int stride_bytes, int width, int height, int y, int n,
			int filter_type, sbyte* line_buffer)
		{
			var mymap = y != 0 ? stbiw__encode_png_line_mapping : stbiw__encode_png_line_firstmap;
			var i = 0;
			var type = mymap[filter_type];
			var z = pixels + stride_bytes * (stbi__flip_vertically_on_write != 0 ? height - 1 - y : y);
			var signed_stride = stbi__flip_vertically_on_write != 0 ? -stride_bytes : stride_bytes;
			if (type == 0)
			{
				CRuntime.memcpy(line_buffer, z, (ulong)(width * n));
				return;
			}

			for (i = 0; i < n; ++i)
				switch (type)
				{
					case 1:
						line_buffer[i] = (sbyte)z[i];
						break;
					case 2:
						line_buffer[i] = (sbyte)(z[i] - z[i - signed_stride]);
						break;
					case 3:
						line_buffer[i] = (sbyte)(z[i] - (z[i - signed_stride] >> 1));
						break;
					case 4:
						line_buffer[i] = (sbyte)(z[i] - stbiw__paeth(0, z[i - signed_stride], 0));
						break;
					case 5:
						line_buffer[i] = (sbyte)z[i];
						break;
					case 6:
						line_buffer[i] = (sbyte)z[i];
						break;
				}

			switch (type)
			{
				case 1:
					for (i = n; i < width * n; ++i) line_buffer[i] = (sbyte)(z[i] - z[i - n]);

					break;
				case 2:
					for (i = n; i < width * n; ++i) line_buffer[i] = (sbyte)(z[i] - z[i - signed_stride]);

					break;
				case 3:
					for (i = n; i < width * n; ++i)
						line_buffer[i] = (sbyte)(z[i] - ((z[i - n] + z[i - signed_stride]) >> 1));

					break;
				case 4:
					for (i = n; i < width * n; ++i)
						line_buffer[i] =
							(sbyte)(z[i] - stbiw__paeth(z[i - n], z[i - signed_stride], z[i - signed_stride - n]));

					break;
				case 5:
					for (i = n; i < width * n; ++i) line_buffer[i] = (sbyte)(z[i] - (z[i - n] >> 1));

					break;
				case 6:
					for (i = n; i < width * n; ++i) line_buffer[i] = (sbyte)(z[i] - stbiw__paeth(z[i - n], 0, 0));

					break;
			}
		}

		public static void stbiw__jpg_calcBits(int val, ushort* bits)
		{
			var tmp1 = val < 0 ? -val : val;
			val = val < 0 ? val - 1 : val;
			bits[1] = 1;
			while ((tmp1 >>= 1) != 0) ++bits[1];

			bits[0] = (ushort)(val & ((1 << bits[1]) - 1));
		}

		public static void stbiw__jpg_DCT(float* d0p, float* d1p, float* d2p, float* d3p, float* d4p, float* d5p,
			float* d6p, float* d7p)
		{
			var d0 = *d0p;
			var d1 = *d1p;
			var d2 = *d2p;
			var d3 = *d3p;
			var d4 = *d4p;
			var d5 = *d5p;
			var d6 = *d6p;
			var d7 = *d7p;
			float z1 = 0;
			float z2 = 0;
			float z3 = 0;
			float z4 = 0;
			float z5 = 0;
			float z11 = 0;
			float z13 = 0;
			var tmp0 = d0 + d7;
			var tmp7 = d0 - d7;
			var tmp1 = d1 + d6;
			var tmp6 = d1 - d6;
			var tmp2 = d2 + d5;
			var tmp5 = d2 - d5;
			var tmp3 = d3 + d4;
			var tmp4 = d3 - d4;
			var tmp10 = tmp0 + tmp3;
			var tmp13 = tmp0 - tmp3;
			var tmp11 = tmp1 + tmp2;
			var tmp12 = tmp1 - tmp2;
			d0 = tmp10 + tmp11;
			d4 = tmp10 - tmp11;
			z1 = (tmp12 + tmp13) * 0.707106781f;
			d2 = tmp13 + z1;
			d6 = tmp13 - z1;
			tmp10 = tmp4 + tmp5;
			tmp11 = tmp5 + tmp6;
			tmp12 = tmp6 + tmp7;
			z5 = (tmp10 - tmp12) * 0.382683433f;
			z2 = tmp10 * 0.541196100f + z5;
			z4 = tmp12 * 1.306562965f + z5;
			z3 = tmp11 * 0.707106781f;
			z11 = tmp7 + z3;
			z13 = tmp7 - z3;
			*d5p = z13 + z2;
			*d3p = z13 - z2;
			*d1p = z11 + z4;
			*d7p = z11 - z4;
			*d0p = d0;
			*d2p = d2;
			*d4p = d4;
			*d6p = d6;
		}

		public static int stbiw__jpg_processDU(stbi__write_context s, int* bitBuf, int* bitCnt, float* CDU,
			int du_stride, float* fdtbl, int DC, ushort[,] HTDC, ushort[,] HTAC)
		{
			var EOB = stackalloc ushort[] { HTAC[0x00, 0], HTAC[0x00, 1] };
			var M16zeroes = stackalloc ushort[] { HTAC[0xF0, 0], HTAC[0xF0, 1] };
			var dataOff = 0;
			var i = 0;
			var j = 0;
			var n = 0;
			var diff = 0;
			var end0pos = 0;
			var x = 0;
			var y = 0;
			var DU = stackalloc int[64];
			for (dataOff = 0, n = du_stride * 8; dataOff < n; dataOff += du_stride)
				stbiw__jpg_DCT(&CDU[dataOff], &CDU[dataOff + 1], &CDU[dataOff + 2], &CDU[dataOff + 3],
					&CDU[dataOff + 4], &CDU[dataOff + 5], &CDU[dataOff + 6], &CDU[dataOff + 7]);

			for (dataOff = 0; dataOff < 8; ++dataOff)
				stbiw__jpg_DCT(&CDU[dataOff], &CDU[dataOff + du_stride], &CDU[dataOff + du_stride * 2],
					&CDU[dataOff + du_stride * 3], &CDU[dataOff + du_stride * 4], &CDU[dataOff + du_stride * 5],
					&CDU[dataOff + du_stride * 6], &CDU[dataOff + du_stride * 7]);

			for (y = 0, j = 0; y < 8; ++y)
				for (x = 0; x < 8; ++x, ++j)
				{
					float v = 0;
					i = y * du_stride + x;
					v = CDU[i] * fdtbl[j];
					DU[stbiw__jpg_ZigZag[j]] = (int)(v < 0 ? v - 0.5f : v + 0.5f);
				}

			diff = DU[0] - DC;
			if (diff == 0)
			{
				stbiw__jpg_writeBits(s, bitBuf, bitCnt, HTDC[0, 0], HTDC[0, 1]);
			}
			else
			{
				var bits = stackalloc ushort[2];
				stbiw__jpg_calcBits(diff, bits);
				stbiw__jpg_writeBits(s, bitBuf, bitCnt,
					HTDC[bits[1], 0], HTDC[bits[1], 1]);
				stbiw__jpg_writeBits(s, bitBuf, bitCnt, bits[0], bits[1]);
			}

			end0pos = 63;
			for (; end0pos > 0 && DU[end0pos] == 0; --end0pos)
			{
			}

			if (end0pos == 0)
			{
				stbiw__jpg_writeBits(s, bitBuf, bitCnt, EOB[0], EOB[1]);
				return DU[0];
			}

			for (i = 1; i <= end0pos; ++i)
			{
				var startpos = i;
				var nrzeroes = 0;
				var bits = stackalloc ushort[2];
				for (; DU[i] == 0 && i <= end0pos; ++i)
				{
				}

				nrzeroes = i - startpos;
				if (nrzeroes >= 16)
				{
					var lng = nrzeroes >> 4;
					var nrmarker = 0;
					for (nrmarker = 1; nrmarker <= lng; ++nrmarker)
						stbiw__jpg_writeBits(s, bitBuf, bitCnt, M16zeroes[0], M16zeroes[1]);

					nrzeroes &= 15;
				}

				stbiw__jpg_calcBits(DU[i], bits);
				stbiw__jpg_writeBits(s, bitBuf, bitCnt,
					HTAC[(nrzeroes << 4) + bits[1], 0],
					HTAC[(nrzeroes << 4) + bits[1], 1]);
				stbiw__jpg_writeBits(s, bitBuf, bitCnt, bits[0], bits[1]);
			}

			if (end0pos != 63) stbiw__jpg_writeBits(s, bitBuf, bitCnt, EOB[0], EOB[1]);

			return DU[0];
		}

		public static void stbiw__jpg_writeBits(stbi__write_context s, int* bitBufP, int* bitCntP, ushort bs0,
			ushort bs1)
		{
			var bitBuf = *bitBufP;
			var bitCnt = *bitCntP;
			bitCnt += bs1;
			bitBuf |= bs0 << (24 - bitCnt);
			while (bitCnt >= 8)
			{
				var c = (byte)((bitBuf >> 16) & 255);
				stbiw__putc(s, c);
				if (c == 255) stbiw__putc(s, 0);

				bitBuf <<= 8;
				bitCnt -= 8;
			}

			*bitBufP = bitBuf;
			*bitCntP = bitCnt;
		}

		public static void stbiw__linear_to_rgbe(byte* rgbe, float* linear)
		{
			var exponent = 0;
			var maxcomp = linear[0] > (linear[1] > linear[2] ? linear[1] : linear[2]) ? linear[0] :
				linear[1] > linear[2] ? linear[1] : linear[2];
			if (maxcomp < 1e-32f)
			{
				rgbe[0] = rgbe[1] = rgbe[2] = rgbe[3] = 0;
			}
			else
			{
				var normalize = (float)CRuntime.frexp(maxcomp, &exponent) * 256.0f / maxcomp;
				rgbe[0] = (byte)(linear[0] * normalize);
				rgbe[1] = (byte)(linear[1] * normalize);
				rgbe[2] = (byte)(linear[2] * normalize);
				rgbe[3] = (byte)(exponent + 128);
			}
		}

		public static byte stbiw__paeth(int a, int b, int c)
		{
			var p = a + b - c;
			var pa = CRuntime.abs(p - a);
			var pb = CRuntime.abs(p - b);
			var pc = CRuntime.abs(p - c);
			if (pa <= pb && pa <= pc)
				return (byte)(a & 0xff);
			if (pb <= pc)
				return (byte)(b & 0xff);
			return (byte)(c & 0xff);
		}

		public static void stbiw__putc(stbi__write_context s, byte c)
		{
			s.func(s.context, &c, 1);
		}

		public static void* stbiw__sbgrowf(void** arr, int increment, int itemsize)
		{
			var m = *arr != null ? 2 * ((int*)*arr - 2)[0] + increment : increment + 1;
			var p = CRuntime.realloc(*arr != null ? (int*)*arr - 2 : null, (ulong)(itemsize * m + sizeof(int) * 2));
			if (p != null)
			{
				if (*arr == null)
					((int*)p)[1] = 0;
				*arr = (int*)p + 2;
				((int*)*arr - 2)[0] = m;
			}

			return *arr;
		}

		public static void stbiw__wpcrc(byte** data, int len)
		{
			var crc = stbiw__crc32(*data - len - 4, len + 4);
			(*data)[0] = (byte)((crc >> 24) & 0xff);
			(*data)[1] = (byte)((crc >> 16) & 0xff);
			(*data)[2] = (byte)((crc >> 8) & 0xff);
			(*data)[3] = (byte)(crc & 0xff);
			(*data) += 4;
		}

		public static void stbiw__write_dump_data(stbi__write_context s, int length, byte* data)
		{
			var lengthbyte = (byte)(length & 0xff);
			s.func(s.context, &lengthbyte, 1);
			s.func(s.context, data, length);
		}

		public static void stbiw__write_flush(stbi__write_context s)
		{
			if (s.buf_used != 0)
			{
				s.func(s.context, s.buffer, s.buf_used);
				s.buf_used = 0;
			}
		}

		public static void stbiw__write_hdr_scanline(stbi__write_context s, int width, int ncomp, byte* scratch,
			float* scanline)
		{
			var scanlineheader = stackalloc byte[] { 2, 2, 0, 0 };
			var rgbe = stackalloc byte[4];
			var linear = stackalloc float[3];
			var x = 0;
			scanlineheader[2] = (byte)((width & 0xff00) >> 8);
			scanlineheader[3] = (byte)(width & 0x00ff);
			if (width < 8 || width >= 32768)
			{
				for (x = 0; x < width; x++)
				{
					switch (ncomp)
					{
						case 4:
						case 3:
							linear[2] = scanline[x * ncomp + 2];
							linear[1] = scanline[x * ncomp + 1];
							linear[0] = scanline[x * ncomp + 0];
							break;
						default:
							linear[0] = linear[1] = linear[2] = scanline[x * ncomp + 0];
							break;
					}

					stbiw__linear_to_rgbe(rgbe, linear);
					s.func(s.context, rgbe, 4);
				}
			}
			else
			{
				var c = 0;
				var r = 0;
				for (x = 0; x < width; x++)
				{
					switch (ncomp)
					{
						case 4:
						case 3:
							linear[2] = scanline[x * ncomp + 2];
							linear[1] = scanline[x * ncomp + 1];
							linear[0] = scanline[x * ncomp + 0];
							break;
						default:
							linear[0] = linear[1] = linear[2] = scanline[x * ncomp + 0];
							break;
					}

					stbiw__linear_to_rgbe(rgbe, linear);
					scratch[x + width * 0] = rgbe[0];
					scratch[x + width * 1] = rgbe[1];
					scratch[x + width * 2] = rgbe[2];
					scratch[x + width * 3] = rgbe[3];
				}

				s.func(s.context, scanlineheader, 4);
				for (c = 0; c < 4; c++)
				{
					var comp = &scratch[width * c];
					x = 0;
					while (x < width)
					{
						r = x;
						while (r + 2 < width)
						{
							if (comp[r] == comp[r + 1] && comp[r] == comp[r + 2])
								break;
							++r;
						}

						if (r + 2 >= width)
							r = width;
						while (x < r)
						{
							var len = r - x;
							if (len > 128)
								len = 128;
							stbiw__write_dump_data(s, len, &comp[x]);
							x += len;
						}

						if (r + 2 < width)
						{
							while (r < width && comp[r] == comp[x]) ++r;

							while (x < r)
							{
								var len = r - x;
								if (len > 127)
									len = 127;
								stbiw__write_run_data(s, len, comp[x]);
								x += len;
							}
						}
					}
				}
			}
		}

		public static void stbiw__write_pixel(stbi__write_context s, int rgb_dir, int comp, int write_alpha,
			int expand_mono, byte* d)
		{
			var bg = stackalloc byte[] { 255, 0, 255 };
			var px = stackalloc byte[3];
			var k = 0;
			if (write_alpha < 0)
				stbiw__write1(s, d[comp - 1]);
			switch (comp)
			{
				case 2:
				case 1:
					if (expand_mono != 0)
						stbiw__write3(s, d[0], d[0], d[0]);
					else
						stbiw__write1(s, d[0]);
					break;
				case 4:
				case 3:
					if (comp == 4 && write_alpha == 0)
					{
						for (k = 0; k < 3; ++k) px[k] = (byte)(bg[k] + (d[k] - bg[k]) * d[3] / 255);

						stbiw__write3(s, px[1 - rgb_dir], px[1], px[1 + rgb_dir]);
						break;
					}

					stbiw__write3(s, d[1 - rgb_dir], d[1], d[1 + rgb_dir]);
					break;
			}

			if (write_alpha > 0)
				stbiw__write1(s, d[comp - 1]);
		}

		public static void stbiw__write_pixels(stbi__write_context s, int rgb_dir, int vdir, int x, int y, int comp,
			void* data, int write_alpha, int scanline_pad, int expand_mono)
		{
			uint zero = 0;
			var i = 0;
			var j = 0;
			var j_end = 0;
			if (y <= 0)
				return;
			if (stbi__flip_vertically_on_write != 0)
				vdir *= -1;
			if (vdir < 0)
			{
				j_end = -1;
				j = y - 1;
			}
			else
			{
				j_end = y;
				j = 0;
			}

			for (; j != j_end; j += vdir)
			{
				for (i = 0; i < x; ++i)
				{
					var d = (byte*)data + (j * x + i) * comp;
					stbiw__write_pixel(s, rgb_dir, comp, write_alpha, expand_mono, d);
				}

				stbiw__write_flush(s);
				s.func(s.context, &zero, scanline_pad);
			}
		}

		public static void stbiw__write_run_data(stbi__write_context s, int length, byte databyte)
		{
			var lengthbyte = (byte)((length + 128) & 0xff);
			s.func(s.context, &lengthbyte, 1);
			s.func(s.context, &databyte, 1);
		}

		public static void stbiw__write1(stbi__write_context s, byte a)
		{
			if ((ulong)s.buf_used + 1 > 64 * sizeof(byte))
				stbiw__write_flush(s);
			s.buffer[s.buf_used++] = a;
		}

		public static void stbiw__write3(stbi__write_context s, byte a, byte b, byte c)
		{
			var n = 0;
			if ((ulong)s.buf_used + 3 > 64 * sizeof(byte))
				stbiw__write_flush(s);
			n = s.buf_used;
			s.buf_used = n + 3;
			s.buffer[n + 0] = a;
			s.buffer[n + 1] = b;
			s.buffer[n + 2] = c;
		}

		public static uint stbiw__zhash(byte* data)
		{
			var hash = (uint)(data[0] + (data[1] << 8) + (data[2] << 16));
			hash ^= hash << 3;
			hash += hash >> 5;
			hash ^= hash << 4;
			hash += hash >> 17;
			hash ^= hash << 25;
			hash += hash >> 6;
			return hash;
		}

		public static int stbiw__zlib_bitrev(int code, int codebits)
		{
			var res = 0;
			while (codebits-- != 0)
			{
				res = (res << 1) | (code & 1);
				code >>= 1;
			}

			return res;
		}

		public static uint stbiw__zlib_countm(byte* a, byte* b, int limit)
		{
			var i = 0;
			for (i = 0; i < limit && i < 258; ++i)
				if (a[i] != b[i])
					break;

			return (uint)i;
		}

		public static byte* stbiw__zlib_flushf(byte* data, uint* bitbuffer, int* bitcount)
		{
			while (*bitcount >= 8)
			{
				if (data == null || ((int*)data - 2)[1] + 1 >= ((int*)data - 2)[0])
					stbiw__sbgrowf((void**)&data, 1, sizeof(byte));

				data[((int*)data - 2)[1]++] = (byte)(*bitbuffer & 0xff);
				*bitbuffer >>= 8;
				*bitcount -= 8;
			}

			return data;
		}

		public class stbi__write_context
		{
			public int buf_used;
			public byte* buffer;
			public UnsafeArray1D<byte> bufferArray = new UnsafeArray1D<byte>(64);
			public void* context;
			public delegate0 func;

			public stbi__write_context()
			{
				buffer = (byte*)bufferArray;
			}
		}
	}

	public class ImageWriter
	{
		private Stream _stream;
		private byte[] _buffer = new byte[1024];

		private void WriteCallback(void* context, void* data, int size)
		{
			if (data == null || size <= 0)
			{
				return;
			}

			if (_buffer.Length < size)
			{
				_buffer = new byte[size * 2];
			}

			var bptr = (byte*)data;

			Marshal.Copy(new IntPtr(bptr), _buffer, 0, size);

			_stream.Write(_buffer, 0, size);
		}

		private static void CheckParams(byte[] data, int width, int height, ColorComponents components)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}

			if (width <= 0)
			{
				throw new ArgumentOutOfRangeException("width");
			}

			if (height <= 0)
			{
				throw new ArgumentOutOfRangeException("height");
			}

			int requiredDataSize = width * height * (int)components;
			if (data.Length < requiredDataSize)
			{
				throw new ArgumentException(
                    $"Not enough data. 'data' variable should contain at least {requiredDataSize} bytes.");
			}
		}

		public void WriteBmp(void* data, int width, int height, ColorComponents components, Stream dest)
		{
			try
			{
				_stream = dest;
				StbImageWrite.stbi_write_bmp_to_func(WriteCallback, null, width, height, (int)components, data);
			}
			finally
			{
				_stream = null;
			}
		}

		public void WritePng(void* data, int width, int height, ColorComponents components, Stream dest)
		{
			try
			{
				_stream = dest;

				StbImageWrite.stbi_write_png_to_func(WriteCallback, null, width, height, (int)components, data,
				   width * (int)components);
			}
			finally
			{
				_stream = null;
			}
		}

		public void WritePng(byte[] data, int width, int height, ColorComponents components, Stream dest)
		{
			CheckParams(data, width, height, components);

			fixed (byte* b = &data[0])
			{
				WritePng(b, width, height, components, dest);
			}
		}
	}
}

#pragma warning restore CS0162
#pragma warning restore CA2014