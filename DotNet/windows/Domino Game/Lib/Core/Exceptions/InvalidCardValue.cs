namespace Domino_Game.Lib.Core.Exceptions
{
    public class InvalidCardValue : Exception
    {
        /// <summary>
        /// Exception for when the card value is invalid
        /// </summary>
        /// <param name="message"></param>
        public InvalidCardValue(string message) : base(message)
        {
        }



    }
}
