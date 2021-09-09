using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeGlitzer
{
    public class PokemonView : INotifyPropertyChanged
    {
        RangeObservableCollection<byte> data;
        RangeObservableCollection<byte> decodedData;
        public PokemonView(int size)
        {
            data = Utils.CollectionOfSize<byte>(size);
            decodedData = Utils.CollectionOfSize<byte>(size);
            interpreted = InterpretedData.Dummy;
            //data.CollectionChanged += (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data)));
            //decodedData.CollectionChanged += (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DecodedData)));
        }

        public RangeObservableCollection<byte> Data { get => data; }
        public RangeObservableCollection<byte> DecodedData { get => decodedData; }

        InterpretedData interpreted;
        public InterpretedData Interpreted {
            get => interpreted;
            set
            {
                bool changed = interpreted != value;
                interpreted = value;
                if (changed) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Interpreted)));
            }
        }
        TeamInterpretedData? teamInterpreted;
        public TeamInterpretedData? TeamInterpreted
        {
            get => teamInterpreted;
            set
            {
                bool changed = teamInterpreted != value;
                teamInterpreted = value;
                if (changed) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TeamInterpreted)));
            }
        }

        int subp1 = 1; int subp2 = 2; int subp3 = 3; int subp4 = 4;
        public int SubstructureAtPos0
        {
            get => subp1;
            set
            {
                subp1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubstructureAtPos0)));
            }
        }
        public int SubstructureAtPos1
        {
            get => subp2;
            set
            {
                subp2 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubstructureAtPos1)));
            }
        }
        public int SubstructureAtPos2
        {
            get => subp3;
            set
            {
                subp3 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubstructureAtPos2)));
            }
        }
        public int SubstructureAtPos3
        {
            get => subp4;
            set
            {
                subp4 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubstructureAtPos3)));
            }
        }

        bool checksumValid = true;
        public bool ChecksumValid
        {
            get => checksumValid;
            set
            {
                checksumValid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChecksumValid)));
            }
        }

        bool hasData = false;
        public bool HasData
        {
            get => hasData;
            set
            {
                hasData = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasData)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public record EVsIVs(byte hp, byte atk, byte def, byte speed, byte spe_atk, byte spe_def)
    {
        public static EVsIVs Dummy = new EVsIVs(0,0,0,0,0,0);
    }
    public record Condition(byte coolness, byte beauty, byte cuteness, byte smartness, byte toughness, byte feel)
    {
        public static Condition Dummy = new Condition(0, 0, 0, 0, 0, 0);
    }
    public record Moves(ushort m1, byte pp1, byte ppb1, ushort m2, byte pp2, byte ppb2, ushort m3, byte pp3, byte ppb3, ushort m4, byte pp4, byte ppb4)
    {
        public static Moves Dummy = new Moves(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }
    public record Battle(ushort item, byte ability, uint experience, byte friendship)
    {
        public static Battle Dummy = new Battle(0, 0, 0, 0);
    }
    public record Misc(byte pokerus_days, byte pokerus_strain, uint ribbons, bool obedient)
    {
        public static Misc Dummy = new Misc(0, 0, 0, false);
    }

    public enum Gender { Boy=0, Girl }
    public enum Language { Invalid=0, Japanese, English, French, Italian, German, Spanish }
    public enum Game { Invalid=0, Sapphire, Ruby, Emerald, FireRed, LeafGreen, ColosseumXD }
    public enum Ball { Invalid = 0, Master, Ultra, Great, Poke, Safari, Net, Dive, Nest, Repeat, Timer, Luxury, Premier }

    public record Identity(Language lang, string nickname, Gender otGender, string otName, byte metLocation, byte levelMet, Game gameOfOrigin, Ball ball)
    {
        public static Identity Dummy = new Identity(Language.Invalid, "", Gender.Boy, "", 0, 0, Game.Invalid, Ball.Invalid);
    }

    public record InterpretedData(uint PID, uint OTID, ushort species, bool hasSpecies, EggType egg, Identity identity, Battle battle, Moves moves, EVsIVs EVs, EVsIVs IVs, Condition condition, Misc misc)
    {
        public bool IsShiny
        {
            get
            {
                ushort key = (ushort)((PID >> 16) ^ (PID & 0xFFFF) ^ (OTID >> 16) ^ (OTID & 0xFFFF));
                return key < 8;
            }
        }
        public static InterpretedData Dummy = new InterpretedData(0, 0, 0, false, EggType.Invalid, Identity.Dummy, Battle.Dummy, Moves.Dummy, EVsIVs.Dummy, EVsIVs.Dummy, Condition.Dummy, Misc.Dummy);
    }
    public enum EggType
    {
        Invalid = 0,
        NotAnEgg = 1,
        Egg = 2,
        BadEgg = 3
    }

    public record TeamInterpretedData(PkmnStatus status, byte level, byte pokerusRemaining, ushort currentHP, ushort maxHP, ushort attack,
        ushort defense, ushort speed, ushort speAttack, ushort speDefense)
    {
        public static TeamInterpretedData Dummy = new TeamInterpretedData(new PkmnStatus(0,false,false,false,false,false),0,0,0,0,0,0,0,0,0);
    }
    public record PkmnStatus(byte sleep, bool poison, bool burn, bool freeze, bool paralysis, bool badPoison);

    public interface IEditorWindow
    {
        public DataLocation DataLocation { get; }
        public bool IsActive { get; }
        public void Show(Window parent);
        public void Close();
        public void Activate();
        public IBrush Background { get; set; }
        public event EventHandler Closed;
        public event EventHandler Activated;
        public event EventHandler Deactivated;
    }

}
