using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    static class ResLoader
    {
        const int MAX_SPECIES = 386;
        const int MAX_ITEMS = 264;
        const int MAX_MAILS = 11;
        static string? imgdir;
        public static void Initialize()
        {
            imgdir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            imgdir = Path.Combine(imgdir, "Images");
            none = new Bitmap(AssetLoader.Open(new Uri("avares://PokeGlitzer/Assets/b_none.png")));
            none_item = new Bitmap(AssetLoader.Open(new Uri("avares://PokeGlitzer/Assets/bitem_none.png")));
            try { special[0] = new Bitmap(Path.Combine(imgdir, "b_unknown.png")); } catch { special[0] = none; }
            try { special[1] = new Bitmap(Path.Combine(imgdir, "b_unknown_alt.png")); } catch { special[1] = none; }
            try { special[2] = new Bitmap(Path.Combine(imgdir, "b_egg.png")); } catch { special[2] = none; }
            try { special[3] = new Bitmap(Path.Combine(imgdir, "b_bad_egg.png")); } catch { special[3] = none; }
            try { special[4] = new Bitmap(Path.Combine(imgdir, "bitem_tm.png")); } catch { special[4] = none_item; }
            try { special[5] = new Bitmap(Path.Combine(imgdir, "bitem_unk.png")); } catch { special[5] = none_item; }
            try { special[6] = new Bitmap(Path.Combine(imgdir, "bitem_tr.png")); } catch { special[6] = none_item; }
        }
        static void InitializeSpecies(int i)
        {
            try
            {
                species[i] = new Bitmap(Path.Combine(imgdir!, string.Format("species/b_{0}.png", i)));
            }
            catch
            {
                species[i] = special[1];
            }
        }
        static void InitializeShinySpecies(int i)
        {
            try
            {
                shiny_species[i] = new Bitmap(Path.Combine(imgdir!, string.Format("shiny_species/b_{0}s.png", i)));
            }
            catch
            {
                shiny_species[i] = Species(i);
            }
        }
        static void InitializeItem(int i)
        {
            try
            {
                items[i] = new Bitmap(Path.Combine(imgdir!, string.Format("items/bitem_{0}.png", i)));
            }
            catch
            {
                items[i] = special[5];
            }
        }
        static void InitializeMail(int i)
        {
            try
            {
                mails[i] = new Bitmap(Path.Combine(imgdir!, string.Format("mails/mail_{0}.png", i)));
            }
            catch
            {
                mails[i] = special[5];
            }
        }

        static Bitmap? none = null;
        static Bitmap? none_item = null;
        static Bitmap?[] special = new Bitmap[7];
        static Bitmap?[] species = new Bitmap[MAX_SPECIES + 1];
        static Bitmap?[] shiny_species = new Bitmap[MAX_SPECIES + 1];
        static Bitmap?[] items = new Bitmap[MAX_ITEMS + 1];
        static Bitmap?[] mails = new Bitmap[MAX_MAILS + 1];
        public static Bitmap None { get => none!; }
        public static Bitmap NoneItem { get => none_item!; }
        public static Bitmap Species(int i) {
            if (i > MAX_SPECIES)
                return Unknown;
            if (species[i] == null) InitializeSpecies(i); return species[i]!;
        }
        public static Bitmap ShinySpecies(int i) {
            if (i > MAX_SPECIES)
                return Unknown;
            if (shiny_species[i] == null) InitializeShinySpecies(i); return shiny_species[i]!;
        }
        public static Bitmap Items(int i)
        {
            if (i > MAX_ITEMS)
                return UnknownItem;
            if (items[i] == null) InitializeItem(i); return items[i]!;
        }
        public static Bitmap Mails(int i)
        {
            if (i > MAX_MAILS)
                return UnknownItem;
            if (mails[i] == null) InitializeMail(i); return mails[i]!;
        }
        public static Bitmap Egg { get => special[2]!; }
        public static Bitmap BadEgg { get => special[3]!; }
        public static Bitmap Unknown { get => special[1]!; }
        public static Bitmap Error { get => special[0]!; }
        public static Bitmap TM { get => special[4]!; }
        public static Bitmap HM { get => special[6]!; }
        public static Bitmap UnknownItem { get => special[5]!; }
    }
}
