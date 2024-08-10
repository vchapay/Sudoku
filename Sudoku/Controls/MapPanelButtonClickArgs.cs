using Sudoku.MapLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.Controls
{
    internal class MapPanelButtonClickArgs
    {
        private readonly Map _map;

        public MapPanelButtonClickArgs(Map map)
        {
            _map = map;
        }

        /// <summary>
        /// Карта, для которой была нажата кнопка
        /// </summary>
        public Map Map => _map;
    }
}
