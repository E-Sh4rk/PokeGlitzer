using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    public struct PokedexEntry
    {
        const int CATEGORY_MAX_LENGTH = 12;
        const int DESCR_MAX_LENGTH = 100; /* Arbitrary limit. */
        byte[] Category { get; set; }
        int CategoryLength { get; set; }
        ushort Height { get; set; }
        ushort Weight { get; set; }
        byte[] Descr { get; set; }
        int DescrLength { get; set; }
        ushort Scale { get; set; }
        ushort Offset { get; set; }
        ushort TrainerScale { get; set; }
        ushort TrainerOffset { get; set; }
    }
    public struct SpeciesInfo
    {
        byte BaseHP { get; set; }
        byte BaseAttack { get; set; }
        byte BaseDefense { get; set; }
        byte BaseSpeed { get; set; }
        byte BaseSpAttack { get; set; }
        byte BaseSpDefense { get; set; }
        byte FirstType { get; set; }
        byte SecondType { get; set; }
        byte CatchRate { get; set; }
        byte ExpYield { get; set; }
        ushort EvYield { get; set; }
        ushort ItemCommon { get; set; }
        ushort ItemRare { get; set; }
        byte GenderRatio { get; set; }
        byte EggCycles { get; set; }
        byte BaseFriendship { get; set; }
        byte ExpCurve { get; set; }
        byte FirstEggGroup { get; set; }
        byte SecondEggGroup { get; set; }
        byte FirstAbility { get; set; }
        byte SecondAbility { get; set; }
        byte SafariFleeRate { get; set; }
        byte BodyColorAndNoFlip { get; set; }
    }
    public struct Evolution
    {
        ushort Method { get; set; }
        ushort Param { get; set; }
        ushort TargetSpecies { get; set; }
    }
    public struct PkmnSpecies
    {
        const int POKEMON_NAME_LENGTH = 10;
        const int SPECIES_NAME_MAX_LENGTH = 100; /* Arbitrary limit. */
        const int EVOS_PER_MON = 5;
        PokedexEntry Pokedex { get; set; }
        SpeciesInfo Info { get; set; }
        byte[] SpeciesName { get; set; }
        int SpeciesNameLength { get; set; }
        byte AnimationID { get; set; }
        byte AnimationDelay { get; set; }
        ushort DexNumber { get; set; }
        ushort CryID { get; set; }

        Evolution[] Evolution { get; set; }
        uint LearnsetPointer { get; set; }

        uint PalettePointer { get; set; }
        ushort PaletteTag { get; set; }
        uint ShinyPalettePointer { get; set; }
        ushort ShinyPaletteTag { get; set; }
    }
    internal class Species
    {
    }
}
