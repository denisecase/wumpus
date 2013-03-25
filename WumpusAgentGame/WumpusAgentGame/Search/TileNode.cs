using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WumpusAgentGame.Search
{
    class TileNode : IComparable<TileNode>
    {
        public enum State
        {
            NEW,OPEN,CLOSED,RAISED,LOWER
        }

        public State CurrentState;

        public double Risk;
        public double WumpusRisk;
        public double pathValue;

        public int xLoc;
        public int yLoc;

        public int BackX;
        public int BackY;

        public TileNode(int x, int y, double val, double Wval)
        {
            xLoc = x;
            yLoc = y;
            Risk = val;
            WumpusRisk = Wval;
            CurrentState = State.NEW;
        }
        public TileNode(int x, int y, double val, double Wval, TileNode TN)
        {
            xLoc = x;
            yLoc = y;
            Risk = val;
            WumpusRisk = Wval;
            CurrentState = State.NEW;
            BackX = TN.xLoc;
            BackY = TN.yLoc;
        }

        public void SetBackPointer(TileNode TN)
        {
            if (TN != null)
            {
                BackX = TN.xLoc;
                BackY = TN.yLoc;
            }
            else
            {
                BackX = -2;
                BackY = -2;
            }
        }

        int IComparable<TileNode>.CompareTo(TileNode other)
        {
            if (other.Risk > this.Risk)
                return -1;
            else if (other.Risk == this.Risk)
                return 0;
            else
                return 1;
        }

        public bool SameNode(TileNode other)
        {
            if (other.xLoc == this.xLoc && other.yLoc == this.yLoc)
            {
                return true;
            }
            return false;
        }

        public bool Raise()
        {
            CurrentState = State.RAISED;
            return true;
        }

        public bool Lower()
        {
            CurrentState = State.LOWER;
            return true;
        }

        public bool Open()
        {
            CurrentState = State.OPEN;
            return true;
        }

        public bool Close()
        {
            CurrentState = State.CLOSED;
            return true;
        }

    }
}
