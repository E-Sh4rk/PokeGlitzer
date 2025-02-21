﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    public static class StringExtensions
    {
        public static string Capitalize(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => "",
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };
    }
    public static class RandomExtensions
    {
        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
    }
    static class Utils
    {
        public static T ToNumber<T>(string v)
        {
            try { checked { return (T)ToNumber(v, typeof(T)); } }
            catch { throw new FormatException(); }
        }
        public static object ToNumber(string v, Type type)
        {
            bool neg = false;
            v = v.Trim();
            if (v.StartsWith("-"))
            {
                v = v.Substring(1);
                v = v.Trim();
                neg = true;
            }
            int b = 10;
            if (v.StartsWith("0x") || v.StartsWith("0X"))
            {
                b = 16; v = v.Substring(2); v = v.Trim();
            }
            else if (v.StartsWith("0b") || v.StartsWith("0B"))
            {
                b = 2; v = v.Substring(2); v = v.Trim();
            }
            else if (v.StartsWith("0d") || v.StartsWith("0D"))
            {
                b = 10; v = v.Substring(2); v = v.Trim();
            }
            try
            {
                long res = Convert.ToInt64(v, b);
                if (neg) res = -res;
                return Convert.ChangeType(res, type);
            }
            catch { throw new FormatException(); }
        }
        public static string NumberToString(long nb, string format)
        {
            if (format == "X" || format == "x" || format == "B" || format == "b")
            {
                string prefix = nb >= 0 ? "" : "-";
                nb = Math.Abs(nb);
                return prefix + "0" + format + nb.ToString(format);
            }
            else
                return nb.ToString();
        }

        public static RangeObservableCollection<T> CollectionOfSize<T>(int size)
        {
            return new RangeObservableCollection<T>(new T[size]);
        }
        public static bool HasOnlyHexDigits(string str)
        {
            foreach (char c in str.ToUpperInvariant())
            {
                if (c >= 'A' && c <= 'F' || c >= '0' && c <= '9')
                    continue;
                return false;
            }
            return true;
        }

        public static bool IsNonTrivialReplacement<T>(NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Replace:
                    return !Enumerable.SequenceEqual(args.OldItems!.Cast<T>(), args.NewItems!.Cast<T>());
                default:
                    throw new NotImplementedException();
            }
        }

        public static void UpdateCollectionRange<T>(RangeObservableCollection<T> col, IEnumerable<T> newData, int start = 0)
        {
            /*for (int i = 0; i < newData.Length; i++)
                col[i + start] = newData[i];*/
            col.ReplaceRange(start, newData.Count(), newData);
        }
        public static T[] ExtractCollectionRange<T>(RangeObservableCollection<T> col, int start, int length)
        {
            int i = -1;
            T[] dataArr = new T[length];
            foreach (T b in col)
            {
                i++;
                if (i >= start)
                {
                    if (i >= start + length) break;
                    dataArr[i - start] = b;
                }
            }
            return dataArr;
        }

        public static T[] Slice<T>(this T[] src, int offset, int length) => src.AsSpan(offset, length).ToArray();

        // Only for blittable types
        public static T ByteToType<T>(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));
            return ByteToType<T>(bytes);
        }
        // Only for blittable types
        public static void TypeToByte<T>(BinaryWriter writer, T structure)
        {
            byte[] arr = TypeToByte(structure);
            writer.Write(arr);
        }
        // Only for blittable types
        public static T ByteToType<T>(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T structure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject())!;
            handle.Free();
            return structure;
        }
        // Only for blittable types
        public static byte[] TypeToByte<T>(T structure)
        {
            byte[] arr = new byte[Marshal.SizeOf(typeof(T))];
            GCHandle handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
            Marshal.StructureToPtr(structure!, handle.AddrOfPinnedObject(), false);
            handle.Free();
            return arr;
        }
        // Only for blittable types
        public static T MarshalCopy<T>(T structure)
        {
            return ByteToType<T>(TypeToByte(structure));
        }

        public static int OffsetOf<T>(string field)
        {
            return Marshal.OffsetOf<T>(field).ToInt32();
        }
    }
}
