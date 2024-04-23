namespace Domino_Game.Lib.Core.Exceptions
{
    public class PlayersOutOfRangeException : Exception
    {
        /// <summary>
        /// Exception for when the number of players is out of range
        /// </summary>
        /// <param name="message"></param>
        public PlayersOutOfRangeException(string message) : base(message)
        {
        }
    }
}
