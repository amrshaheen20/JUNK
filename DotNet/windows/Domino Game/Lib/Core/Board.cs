using System.Diagnostics;

namespace Domino_Game.Lib.Core
{
    public class CardNode
    {
        public Card BaseCard { get; set; }
        public CardNode BaseNode { get; set; }
        public CardNode TopNode { get; set; }
        public CardNode BottomNode { get; set; }
        public CardNode RightNode { get; set; }
        public CardNode LeftNode { get; set; }

        public CardNode() { }
        public CardNode(Card card) { BaseCard = card; }

        public void SetRight(Card node)
        {
            node.direction = eDirection.Right;
            var temp = GetRightCard();
            temp.RightNode = new CardNode(node);
            temp.RightNode.LeftNode = temp;
            temp.RightNode.BaseNode = BaseNode;

        }

        public void SetLeft(Card node)
        {
            node.direction = eDirection.Left;
            var temp = GetLeftCard();
            temp.LeftNode = new CardNode(node);
            temp.LeftNode.RightNode = temp;
            temp.LeftNode.BaseNode = BaseNode;
        }

        public void SetTop(Card node)
        {
            node.direction = eDirection.Top;
            var temp = GetTopCard();
            temp.TopNode = new CardNode(node);
            temp.TopNode.BottomNode = temp;
            temp.TopNode.BaseNode = BaseNode;
        }

        public void SetBottom(Card node)
        {
            node.direction = eDirection.Bottom;
            var temp = GetBottomCard();
            temp.BottomNode = new CardNode(node);
            temp.BottomNode.TopNode = temp;
            temp.BottomNode.BaseNode = BaseNode;
        }

        public int Count
        {
            get
            {
                int count = 0;
                if (BaseCard != null)
                    count++;

                var temp = this.LeftNode;
                while (temp != null)
                {
                    temp = temp.RightNode;
                    count++;
                }

                var temp2 = this.TopNode;
                while (temp2 != null)
                {
                    temp2 = temp2.BottomNode;
                    count++;
                }

                return count;
            }
        }

        public CardNode GetRightCard(bool LastToLastOne = true) => GetNode(this, LastToLastOne, n => n.RightNode);
        public CardNode GetLeftCard(bool LastToLastOne = true) => GetNode(this, LastToLastOne, n => n.LeftNode);
        public CardNode GetTopCard(bool LastToLastOne = true) => GetNode(this, LastToLastOne, n => n.TopNode);
        public CardNode GetBottomCard(bool LastToLastOne = true) => GetNode(this, LastToLastOne, n => n.BottomNode);

        private CardNode GetNode(CardNode startNode, bool LastToLastOne, Func<CardNode, CardNode> nextNode)
        {
            var temp = startNode;

            if (LastToLastOne)
            {
                var lastnode = temp;
                while (temp != null)
                {
                    lastnode = temp;
                    temp = nextNode(temp);
                }
                return lastnode;
            }
            return temp;
        }


        public int GetCardCount(int value)
        {
            int count = 0;
            void Counter(Card card)
            {
                if (card.IsDouble) count += (card.Head == value && card.Tail == value) ? 1 : 0;
                else if (card.Head == value || card.Tail == value) count++;
            }

            var temp = GetLeftCard();
            while (temp != null && temp.BaseCard != null) { Counter(temp.BaseCard); temp = temp.RightNode; }


            temp = GetTopCard();
            while (temp != null && temp.BaseCard != null)
            {
                if (temp != BaseNode)//calculated this node Before :D
                {
                    Counter(temp.BaseCard);
                }

                temp = temp.BottomNode;
            }
            return count;
        }

    }



    //left->Head || right-> Tail 
    //Top->Head || Bottom->Tail
    public class Board : CardNode
    {
        public GameState gamestate { get; set; }
        public void Add(Card item)
        {
            if (BaseNode == null)
            {
                BaseCard = item;
                BaseNode = this;
                PrintList();
                return;
            }


            if (CanAddToLeft(item))
            {
                AddToLeft(item);
            }
            else if (CanAddToRight(item))
            {
                AddToRight(item);
            }
            else if (CanAddToTop(item))
            {
                AddToTop(item);
            }
            else if (CanAddToBottom(item))
            {
                AddToBottom(item);
            }
            else
            {
                throw new Exception("Invalid Move");
            }

            PrintList();
        }


        private bool CanAddToLeft(Card card)
        {
            return GetLeftCard().BaseCard.Head == card.Tail || GetLeftCard().BaseCard.Head == card.Head;
        }

        private bool CanAddToRight(Card card)
        {
            return GetRightCard().BaseCard.Tail == card.Head || GetRightCard().BaseCard.Tail == card.Tail;
        }

        private bool CanAddToTop(Card card)
        {
            return GetTopCard().BaseCard.Head == card.Head || GetTopCard().BaseCard.Head == card.Tail;
        }

        private bool CanAddToBottom(Card card)
        {
            return GetBottomCard().BaseCard.Tail == card.Head || GetBottomCard().BaseCard.Tail == card.Tail;
        }


        public void AddToLeft(Card card)
        {
            if (Count == 0)
            {
                Add(card);
                return;
            }

            if (CanAddToLeft(card))
            {
                if (GetLeftCard().BaseCard.Head == card.Head)
                {
                    card.Flip();
                }
                SetLeft(card);
            }
            else
            {
                Add(card);
            }
            PrintList();
        }



        public void AddToRight(Card card)
        {
            if (Count == 0)
            {
                Add(card);
                return;
            }
            if (CanAddToRight(card))
            {

                if (GetRightCard().BaseCard.Tail == card.Tail)
                {
                    card.Flip();
                }
                SetRight(card);
            }
            else
            {
                Add(card);
            }
            PrintList();
        }

        public void AddToTop(Card card)
        {
            if (Count == 0)
            {
                Add(card);
                return;
            }
            if (CanAddToTop(card))
            {
                if (GetTopCard().BaseCard.Head == card.Head)
                {
                    card.Flip();
                }
                SetTop(card);
            }
            else
            {
                Add(card);
            }
            PrintList();
        }

        public void AddToBottom(Card card)
        {
            if (Count == 0)
            {
                Add(card);
                return;
            }
            if (CanAddToBottom(card))
            {
                if (GetBottomCard().BaseCard.Tail == card.Tail)
                {
                    card.Flip();
                }
                SetBottom(card);
            }
            else
            {
                Add(card);
            }
            PrintList();
        }


        private bool IsFirstRound() => gamestate.IsFirstRound && Count == 0;
        public bool CardCanSet(Card item) => CardCanSet(item, eDirection.auto);
        public bool CardCanSetInRight(Card item) => CardCanSet(item, eDirection.Right);
        public bool CardCanSetInLeft(Card item) => CardCanSet(item, eDirection.Left);
        public bool CardCanSetInTop(Card item) => CardCanSet(item, eDirection.Top);
        public bool CardCanSetInBottom(Card item) => CardCanSet(item, eDirection.Bottom);
        public bool CardCanSet(Card item, eDirection direction)
        {
#warning "AllFives no put"

            if (IsFirstRound())
            {
                if (item.IsDouble && item.Head == 6)
                {
                    return true;
                }
                else
                {
                    var IsPlayerHas6_6 = item.Player.Hand.Any(x => x.IsDouble && x.Head == 6);
                    if (IsPlayerHas6_6)
                    {
                        return false;
                    }
                }
                return item.IsDouble;
            }

            if (Count == 0)
                return true;

            if (direction == eDirection.auto)
            {
                return CardCanSet(item, eDirection.Right) ||
                       CardCanSet(item, eDirection.Left)  ||
                       CardCanSet(item, eDirection.Top)   ||
                       CardCanSet(item, eDirection.Bottom);
            }
            else if (direction == eDirection.Right)
            {
                var Right = GetRightCard();
                return (Right.BaseCard.Tail == item.Head) || (Right.BaseCard.Tail == item.Tail);
            }
            else if (direction == eDirection.Left)
            {
                var Left = GetLeftCard();
                return (Left.BaseCard.Head == item.Tail) || (Left.BaseCard.Head == item.Head);
            }
            else if (direction == eDirection.Top && gamestate.gameType == eGameType.AllFives)
            {
                var Top = GetTopCard();
                return (Top.BaseCard.Head == item.Head) || (Top.BaseCard.Head == item.Tail);
            }
            else if (direction == eDirection.Bottom && gamestate.gameType == eGameType.AllFives)
            {
                var Bottom = GetBottomCard();
                return (Bottom.BaseCard.Tail == item.Head) || (Bottom.BaseCard.Tail == item.Tail);
            }

            return false;
        }

        public bool PlayerCanPlay(Player player)
        {
            foreach (var card in player.Hand)
            {
                if (CardCanSet(card))
                    return true;
            }
            return false;
        }

        void PrintList()
        {
            Debug.Write("List:");
            var temp = GetLeftCard();
            while (temp != null)
            {
                Debug.Write(temp.BaseCard.Name + " ");
                temp = temp.RightNode;
            }

            Debug.WriteLine("");

            Debug.Write("List2:");
            temp = GetTopCard();
            while (temp != null)
            {
                Debug.Write(temp.BaseCard.Name + " ");
                temp = temp.BottomNode;
            }

            Debug.WriteLine("");
        }

        public bool GameOver()
        {

            if (Count == 0)
                return false;

            foreach (var player in gamestate.Players)
            {
                if (player.IsWinner)//if hand is empty
                {
                    return true;
                }
            }


            if (gamestate.gameType == eGameType.AllFives &&
                (GetLeftCard().BaseCard.Head +
                GetRightCard().BaseCard.Tail +
                GetTopCard().BaseCard.Head +
                GetBottomCard().BaseCard.Tail) % 5 == 0)
            {
                return true;
            }
            else if (gamestate.gameType == eGameType.Regular && GetCardCount(GetLeftCard().BaseCard.Head) >= 7 && GetCardCount(GetRightCard().BaseCard.Tail) >= 7)
            {
                return true;
            }


            return false;
        }

        public Card GetRightNode(Card card)
        {
            var temp = GetLeftCard();
            while (temp != null)
            {

                if (temp != null)
                {
                    if (temp.BaseCard == card)
                    {
                        if (temp.RightNode != null)
                            return temp.RightNode.BaseCard;
                    }
                }

                temp = temp.RightNode;
            }

            return null;
        }

        public Card GetLeftNode(Card card)
        {
            var temp = GetRightCard();
            while (temp != null)
            {

                if (temp != null)
                {
                    if (temp.BaseCard == card)
                    {
                        if (temp.LeftNode != null)
                            return temp.LeftNode.BaseCard;
                    }
                }
                temp = temp.LeftNode;
            }

            return null;
        }

        public Card GetTopNode(Card card)
        {
            var temp = GetBottomCard();
            while (temp != null)
            {

                if (temp != null)
                {
                    if (temp.BaseCard == card)
                    {
                        if (temp.TopNode != null)
                            return temp.TopNode.BaseCard;
                    }
                }
                temp = temp.TopNode;
            }

            return null;
        }

        public Card GetBottomNode(Card card)
        {
            var temp = GetTopCard();
            while (temp != null)
            {

                if (temp != null)
                {
                    if (temp.BaseCard == card)
                    {
                        if (temp.BottomNode != null)
                            return temp.BottomNode.BaseCard;
                    }
                }
                temp = temp.BottomNode;
            }

            return null;
        }

        public void Clear()
        {
            BaseNode = null;
            BaseCard = null;
            TopNode = null;
            BottomNode = null;
            RightNode = null;
            LeftNode = null;
        }
    }
}
