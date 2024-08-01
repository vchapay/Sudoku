using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.MapLogic
{
    internal class GroupInterface
    {
        private int _id;
        private Map.GroupType _type;
        private int _sum;

        public GroupInterface(int id, Map.GroupType type, int sum) 
        {
            _id = id;
            _type = type;
            _sum = sum;
        }

        public int ID { get { return _id; } }

        public Map.GroupType Type { get { return _type; } }

        public int Sum { get { return _sum; } }

        public bool IsSelected { get; set; }

        public static bool operator ==(GroupInterface left, GroupInterface right)
        {
            if (ReferenceEquals(null, left) && !ReferenceEquals(null, right))
                return false;

            if (!ReferenceEquals(null, left) && ReferenceEquals(null, right))
                return false;

            if (ReferenceEquals(null, left) && ReferenceEquals(null, right))
                return true;

            return left.Equals(right);
        }

        public static bool operator !=(GroupInterface left, GroupInterface right)
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

            if (obj is GroupInterface comp)
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
