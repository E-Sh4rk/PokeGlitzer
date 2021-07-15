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
            special = new Bitmap[3];
            try { special[0] = new Bitmap("Images/b_unknown.png"); } catch { special[0] = none; }
            try { special[1] = new Bitmap("Images/b_unknown_alt.png"); } catch { special[1] = none; }
            try { special[2] = new Bitmap("Images/b_egg.png"); } catch { special[2] = none; }

            species = new Bitmap[MAX_SPECIES+1];
            for (int i = 0; i <= MAX_SPECIES; i++)
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

            shiny_species = new Bitmap[MAX_SPECIES + 1];
            for (int i = 0; i <= MAX_SPECIES; i++)
            {
                try
                {
                    shiny_species[i] = new Bitmap(String.Format("Images/shiny_species/b_{0}s.png", i));
                }
                catch
                {
                    shiny_species[i] = species[i];
                }
            }
        }

        static Bitmap? none = null;
        static Bitmap[]? special = null;
        static Bitmap[]? species = null;
        static Bitmap[]? shiny_species = null;
        public static Bitmap None
        {
            get { if (none == null) Initialize(); return none!; }
        }
        public static Bitmap[] Species
        {
            get { if (species == null) Initialize(); return species!; }
        }
        public static Bitmap[] ShinySpecies
        {
            get { if (shiny_species == null) Initialize(); return shiny_species!; }
        }
        public static Bitmap Egg
        {
            get { if (special == null) Initialize(); return special![2]; }
        }
        public static Bitmap Unknown
        {
            get { if (special == null) Initialize(); return special![1]; }
        }
        public static Bitmap Error
        {
            get { if (special == null) Initialize(); return special![0]; }
        }
    }
}
