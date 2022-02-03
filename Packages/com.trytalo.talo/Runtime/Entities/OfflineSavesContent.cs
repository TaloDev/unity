using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TaloGameServices
{
    [System.Serializable]
    public class OfflineSavesContent
    {
        public GameSave[] saves;

        public OfflineSavesContent(GameSave[] saves)
        {
            this.saves = saves;
        }
    }
}
