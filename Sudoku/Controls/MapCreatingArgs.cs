using System;
using System.Drawing;

namespace Sudoku.Controls
{
    internal class MapCreatingArgs : EventArgs
    {
        public MapCreatingArgs(string name, Size size, 
            string description) 
        {
            Name = name;
            Size = size;
            Description = description;
        }

        public string Name { get; }

        public Size Size { get; }

        public string Description { get; }
    }
}