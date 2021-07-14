using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            data = new RangeObservableCollection<byte>(new byte[size]);
            decodedData = new RangeObservableCollection<byte>(new byte[size]);
            //data.CollectionChanged += (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data)));
            //decodedData.CollectionChanged += (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DecodedData)));
        }

        public RangeObservableCollection<byte> Data { get => data; }
        public RangeObservableCollection<byte> DecodedData { get => decodedData; }

        InterpretedData? interpreted;
        public InterpretedData? Interpreted {
            get => interpreted;
            set
            {
                interpreted = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Interpreted)));
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

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public record InterpretedData(uint PID, uint OTID, ushort species, EggType egg);
    public enum EggType
    {
        Invalid = 0,
        NotAnEgg = 1,
        Egg = 2,
        BadEgg = 3
    }

}
