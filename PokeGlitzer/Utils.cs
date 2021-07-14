using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    static class Utils
    {
        public static RangeObservableCollection<T> ByteCollectionOfSize<T>(int size)
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

        public static int OffsetOf<T>(string field)
        {
            return Marshal.OffsetOf<T>(field).ToInt32();
        }
    }
}
