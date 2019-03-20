using System.Collections.Generic;

namespace XFramework.Dal.Data
{
    public class DalHints
    {
        private List<DalHint> hintList;

        public static DalHints CreateIfAbsent(DalHints hints)
        {
            if (hints == null)
            {
                return new DalHints();
            }

            return hints;
        }

        public static DalHints Create()
        {
            return new DalHints();
        }

        public void Add(DalHint hint)
        {
            if (hintList == null)
            {
                hintList = new List<DalHint>();
            }

            hintList.Add(hint);
        }

        public static bool IsSet(DalHints hints, DalHint hint)
        {
            if (hints == null) return false;

            if (hints.HintList == null || hints.HintList.Count == 0)
            {
                return false;
            }

            return hints.HintList.Contains(hint);
        }
        
        public List<DalHint> HintList => hintList;
        
    }

    public enum DalHint
    {
        SetIdentity = 1
    }
}
