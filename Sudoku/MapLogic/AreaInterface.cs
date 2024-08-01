using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.MapLogic
{
    internal class AreaInterface
    {
        private int _id;
        private Map.AreaType _type;
        private int _sum;

        public AreaInterface(int id, Map.AreaType type, int sum) 
        {
            _id = id;
            _type = type;
            _sum = sum;
        }

        public int ID { get { return _id; } }

        public Map.AreaType Type { get { return _type; } }

        public int Sum { get { return _sum; } }

        public static bool operator ==(AreaInterface left, AreaInterface right)
        {
            if (ReferenceEquals(null, left) && !ReferenceEquals(null, right))
                return false;

            if (!ReferenceEquals(null, left) && ReferenceEquals(null, right))
                return false;

            if (ReferenceEquals(null, left) && ReferenceEquals(null, right))
                return true;

            return left.Equals(right);
        }

        public static bool operator !=(AreaInterface left, AreaInterface right)
        {
            if (ReferenceEquals(null, left) && !ReferenceEquals(null, right))
                return true;

            if (!ReferenceEquals(null, left) && ReferenceEquals(null, right))
                return true;

            if (ReferenceEquals(null, left) && ReferenceEquals(null, right))
                return false;

            return left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is AreaInterface comp)
            {
                bool id = this.ID == comp.ID;
                bool type = this.Type == comp.Type;
                return id && type;
            }

            else return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
