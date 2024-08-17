using Sudoku.MapLogic;

namespace Sudoku.Controls
{
    internal class MapActionClickArgs
    {
        private readonly Map _map;

        public MapActionClickArgs(Map map)
        {
            _map = map;
        }

        /// <summary>
        /// Карта, для которой было совершено действие
        /// </summary>
        public Map Map => _map;
    }
}
