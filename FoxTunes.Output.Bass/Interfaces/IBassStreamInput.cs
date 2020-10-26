using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamInput : IBassStreamComponent
    {
        IEnumerable<int> Queue { get; }

        bool PreserveBuffer { get; }

        bool CheckFormat(BassOutputStream stream);

        bool Contains(BassOutputStream stream);

        int Position(BassOutputStream stream);

        bool Add(BassOutputStream stream);

        bool Remove(BassOutputStream stream, Action<BassOutputStream> callBack);

        void Reset();
    }
}
