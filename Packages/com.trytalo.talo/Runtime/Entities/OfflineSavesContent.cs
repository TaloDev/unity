using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TaloGameServices
{
    [System.Serializable]
    public class OfflineSavesContent
    {
        public GameSave[] saves;

        public OfflineSavesContent(List<GameSave> saves)
        {
            this.saves = saves.Where((save) => save.id < 0).ToArray();
        }
    }
}
