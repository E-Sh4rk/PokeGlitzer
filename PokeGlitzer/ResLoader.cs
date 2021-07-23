using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    static class ResLoader
    {
        public const int MAX_SPECIES = 386;
        public static void Initialize()
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            none = new Bitmap(assets.Open(new Uri("avares://PokeGlitzer/Resources/b_none.png")));
            try { special[0] = new Bitmap("Images/b_unknown.png"); } catch { special[0] = none; }
            try { special[1] = new Bitmap("Images/b_unknown_alt.png"); } catch { special[1] = none; }
            try { special[2] = new Bitmap("Images/b_egg.png"); } catch { special[2] = none; }
            try { special[3] = new Bitmap("Images/b_bad_egg.png"); } catch { special[3] = none; }
        }
        static void InitializeSpecies(int i)
        {
            try
            {
                species[i] = new Bitmap(String.Format("Images/species/b_{0}.png", i));
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
                shiny_species[i] = new Bitmap(String.Format("Images/shiny_species/b_{0}s.png", i));
            }
            catch
            {
                shiny_species[i] = Species(i);
            }
        }

        static Bitmap? none = null;
        static Bitmap?[] special = new Bitmap[4];
        static Bitmap?[] species = new Bitmap[MAX_SPECIES + 1];
        static Bitmap?[] shiny_species = new Bitmap[MAX_SPECIES + 1];
        public static Bitmap None { get => none!; }
        public static Bitmap Species(int i) { if (species[i] == null) InitializeSpecies(i); return species[i]!; }
        public static Bitmap ShinySpecies(int i) { if (shiny_species[i] == null) InitializeShinySpecies(i); return shiny_species[i]!; }
        public static Bitmap Egg { get => special[2]!; }
        public static Bitmap BadEgg { get => special[3]!; }
        public static Bitmap Unknown { get => special[1]!; }
        public static Bitmap Error { get => special[0]!; }
    }
}
