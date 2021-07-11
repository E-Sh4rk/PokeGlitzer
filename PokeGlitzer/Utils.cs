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
        public static void UpdateCollectionRange(RangeObservableCollection<byte> col, IEnumerable<byte> newData, int start = 0)
        {
            /*for (int i = 0; i < newData.Length; i++)
                col[i + start] = newData[i];*/
            col.ReplaceRange(start, newData.Count(), newData);
        }
        public static byte[] ExtractCollectionRange(RangeObservableCollection<byte> col, int start, int length)
        {
            int i = -1;
            byte[] dataArr = new byte[length];
            foreach (byte b in col)
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
    }
}
