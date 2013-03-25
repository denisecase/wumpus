using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WumpusAgentGame.Search
{
    class DFS
    {
        List<TileNode> openList; //Open List - Nodes that are unknown?
        List<TileNode> closedList; //Closed List - Nodes that are known?

        int pathValue;

        bool wumpusKilled = false;

        static double RiskFactor = 0.50;

        static double RiskFog = 0.50;
        static double RiskFogWumpus = 0.66;

        int LowerBound = 0;
        int UpperBound = -2;

        public TileNode Current; //Current Location

        //Player Log
        List<string> PlayerLog;

        public DFS(List<string> log)
        {
            openList = new List<TileNode>();
            closedList = new List<TileNode>();
            Current = new TileNode(0, 0, 0, 0);
            openList.Add(Current);

            //Player logs
            PlayerLog = log;
        }

        public Stack<Action> GetRoute(bool gold)
        {
            Stack<Action> Route = new Stack<Action>();
            Action Act = new Action();
            TileNode Goal;
            TileNode pathNode;
            
            Route.Push(new Action());

            Goal = GetGoal();

            if (gold)
            {
                Act.CurrentCommand = Action.Command.PickUp;
                Route.Push(Act);
                Goal = GetExit();
                PlayerLog.Add("Agent Goal: " + Goal.xLoc + ", " + Goal.yLoc);

                pathValue = -1;
                ClearPathValues();
                FindLowestPath(Goal, 0);
                ClearNodePointer(Goal);

                pathNode = Current;
                while (pathNode.BackX >= 0)
                {
                    if (pathNode != WumpusAdj(pathNode))
                    {
                        TileNode node = WumpusAdj(pathNode);
                        node.WumpusRisk = 0;
                        ClearWumpus();
                        Action act = GetAction(node, pathNode);
                        if (act.CurrentCommand == Action.Command.Down) act.CurrentCommand = Action.Command.ShootDown;
                        else if (act.CurrentCommand == Action.Command.Up) act.CurrentCommand = Action.Command.ShootUp;
                        else if (act.CurrentCommand == Action.Command.Left) act.CurrentCommand = Action.Command.ShootLeft;
                        else if (act.CurrentCommand == Action.Command.Right) act.CurrentCommand = Action.Command.ShootRight;
                        wumpusKilled = true;
                        Route.Push(act);
                    }
                    Route.Push(GetAction(GetNode(pathNode.BackX,pathNode.BackY),pathNode));
                    pathNode = GetNode(pathNode.BackX, pathNode.BackY);
                }
                Route.Push(new Action(Action.Command.Climb));
            }
            else
            {
                PlayerLog.Add("Agent Goal: " + Goal.xLoc + ", " + Goal.yLoc);

                pathValue = -1;
                ClearPathValues();
                FindLowestPath(Goal, 0);
                ClearNodePointer(Goal);

                pathNode = Current;
                while (pathNode.BackX != -2)
                {
                    if (pathNode != WumpusAdj(pathNode))
                    {
                        TileNode node = WumpusAdj(pathNode);
                        node.WumpusRisk = 0;
                        ClearWumpus();
                        Action act = GetAction(node, pathNode);
                        if (act.CurrentCommand == Action.Command.Down) act.CurrentCommand = Action.Command.ShootDown;
                        else if (act.CurrentCommand == Action.Command.Up) act.CurrentCommand = Action.Command.ShootUp;
                        else if (act.CurrentCommand == Action.Command.Left) act.CurrentCommand = Action.Command.ShootLeft;
                        else if (act.CurrentCommand == Action.Command.Right) act.CurrentCommand = Action.Command.ShootRight;
                        wumpusKilled = true;
                        Route.Push(act);
                    }
                    Route.Push(GetAction(GetNode(pathNode.BackX, pathNode.BackY), pathNode));
                    pathNode = GetNode(pathNode.BackX, pathNode.BackY);
                }
            }
            Stack<Action> tempStack = new Stack<Action>();
            tempStack.Push(new Action());
            while (Route.Count() > 1)
            {
                tempStack.Push(Route.Pop());
            }
            return tempStack;
        }

        //Checks if the Wumpus is Adjacent
        public TileNode WumpusAdj(TileNode TN)
        {
            TileNode node;
            for (int i = 0; i < 4; i++)
            {
                node = GetNodeAdj(TN, i);
                if(node != null && node.WumpusRisk == 1)
                {
                    return node;
                }
            }
            node = TN;
            return node;
        }

        //Gets a node if available
        public TileNode GetNode(int x, int y)
        {
            TileNode TN = null;
            foreach (TileNode node in openList)
            {
                if (node.xLoc == x && node.yLoc == y)
                {
                    return node;
                }
            }
            foreach (TileNode node in closedList)
            {
                if (node.xLoc == x && node.yLoc == y)
                {
                    return node;
                }
            }
            return TN;
        }

        //Clears all the Nodes backpointers
        public void ClearNodePointer(TileNode TN)
        {
            TN.SetBackPointer(null);
        }

        //Sets a Nodes backpointer
        public void SetNodePointer(TileNode TN, TileNode point)
        {
            TN.SetBackPointer(point);
        }

        public Action GetAction(TileNode dest, TileNode cur)
        {
            Action move = new Action();
            if (dest.xLoc + 1 == cur.xLoc) move.CurrentCommand = Action.Command.Left;
            else if (dest.xLoc - 1 == cur.xLoc) move.CurrentCommand = Action.Command.Right;
            else if (dest.yLoc + 1 == cur.yLoc) move.CurrentCommand = Action.Command.Down;
            else if (dest.yLoc - 1 == cur.yLoc) move.CurrentCommand = Action.Command.Up;
            return move;
        }

        public TileNode GetGoal()
        {
            TileNode TN = null;
            
            for (int i = 0; i < 4; i++)
            {
                TN = GetNodeAdj(Current, i);
                if (TN != null && openList.Contains(TN) && TN.Risk <= 0.35 && TN.WumpusRisk <= 0.35) return TN;
            }
            TN = new TileNode(-1, -1, -1, -1);
            TN.pathValue = 1000;
            TN.Risk = 1;
            foreach (TileNode node in openList)
            {
                if ((node.Risk < RiskFactor) && (node.Risk <= TN.Risk) && (node.WumpusRisk < 0.5))
                {
                    TN = node;
                }
            }
            if (TN.pathValue == 1000)
            {
                foreach (TileNode node in openList)
                {
                    if (node.Risk < RiskFogWumpus && node.WumpusRisk != 1 && (node.pathValue < TN.pathValue))
                    {
                        TN = node;
                    }
                }
            }
            if (TN.pathValue == 1000)
            {
                foreach (TileNode node in openList)
                {
                    if (node.WumpusRisk != 1)
                    {
                        return node;
                    }
                }
            }
            return TN;
        }

        //Returns node 0,0
        public TileNode GetExit()
        {
            TileNode exit = new TileNode(0, 0, 0, 0);
            foreach (TileNode node in closedList)
            {
                if (node.xLoc == 0 && node.yLoc == 0)
                {
                    return node;
                }
            }
            return exit;
        }

        //returns true if goal is next to current
        public bool isAdjacent(TileNode cur, TileNode goal)
        {
            bool adj = false;
            if (cur.xLoc + 1 == goal.xLoc && cur.yLoc == goal.yLoc) adj = true;
            else if (cur.xLoc == goal.xLoc && cur.yLoc - 1 == goal.yLoc) adj = true;
            else if (cur.xLoc - 1 == goal.xLoc && cur.yLoc == goal.yLoc) adj = true;
            else if (cur.xLoc == goal.xLoc && cur.yLoc + 1 == goal.yLoc) adj = true;
            return adj;
        }

        //Returns true if TN == Current
        public bool isCurrent(TileNode TN)
        {
            if (TN.xLoc == Current.xLoc && TN.yLoc == Current.yLoc)
            {
                return true;
            }
            return false;
        }


        //Adds Nodes to Openlist and closes current node
        public bool AddNodes(int x, int y, bool hazard, bool stench)
        {
            
            TileNode TN = GetNode(x,y);
            Current = TN;
            openList.RemoveAll(isCurrent);
            closedList.RemoveAll(isCurrent);
            Current.Risk = 0;
            Current.WumpusRisk = 0;
            closedList.Add(TN);


            bool[] obstacle = new bool[4];


            obstacle[0] = AddLowerRaise(TN.xLoc + 1, TN.yLoc, hazard, stench);
            obstacle[1] = AddLowerRaise(TN.xLoc - 1, TN.yLoc, hazard, stench);
            obstacle[2] = AddLowerRaise(TN.xLoc, TN.yLoc + 1, hazard, stench);
            obstacle[3] = AddLowerRaise(TN.xLoc, TN.yLoc - 1, hazard, stench);

            for (int i = 0; i < 4; i++)
            {
                if (obstacle[i])
                {
                    for (int j = 0; j < 4; j++)
                    {
                        TileNode node = GetNodeAdj(Current, j);
                        if (node != null && node.WumpusRisk == 1) ClearWumpus();
                        LowerNode(node, 0.1);
                    }
                }
            }
            if (stench && !wumpusKilled)
            {
                int NotWumpus = 0;
                TileNode mightWumpus = new TileNode(-1,-1,-1,-1);
                for (int i = 0; i < 4; i++)
                {
                    TileNode node = GetNodeAdj(Current, i);
                    if (node != null && node.WumpusRisk == 0) NotWumpus++;
                    else mightWumpus = node;
                }

                if (NotWumpus == 3)
                {
                    RaiseWumpus(mightWumpus.xLoc, mightWumpus.yLoc);
                }
            }

            return true;
        }

        //Returns true if obstacle found
        public bool AddLowerRaise(int x, int y, bool hazard, bool stench)
        {
            double val = 0;
            double Wval = 0;
            if (hazard && stench)
            {
                val = RiskFogWumpus;
            }
            else if (hazard)
            {
                val = RiskFog;
            }

            if (stench && wumpusKilled == false)
            {
                Wval = 0.50;
            }

            if (x >= LowerBound && y >= LowerBound && (UpperBound == -2 || x<UpperBound && y<UpperBound ))
            {
                if (!isInList(x, y))
                {
                    openList.Add(new TileNode(x, y, val, Wval));
                    return false;
                }
                else if (!(hazard || stench))
                {
                    LowerNode(x, y, val);
                    return false;
                }
                else if (hazard)
                {
                    if (stench) RaiseWumpus(x, y);
                    return RaiseNode(x, y, val, stench);
                }
                else if (stench && wumpusKilled == false)
                {
                    if (!hazard) LowerNode(x, y);
                    return RaiseWumpus(x, y);
                }
            }
            return false;
        }

        public bool RaiseWumpus(int x, int y)
        {
            TileNode CurNode = GetNode(x, y);
            if (openList.Contains(CurNode) && CurNode.WumpusRisk != 0)
            {
                PlayerLog.Add("WUMPUS is at ("+x+", "+y+")");
                openList.Remove(CurNode);
                CurNode.WumpusRisk = 1;
                CurNode.Risk = 0;
                closedList.Add(CurNode);
                return true;
            }
            return false;
        }

        //Clears the WumpusRisk of all Nodes that arnt Wumpus
        public bool ClearWumpus()
        {
            foreach (TileNode node in openList)
            {
                if (node.WumpusRisk < 1)
                {
                    node.WumpusRisk = 0;
                }
            }
            return true;
        }

        //
        public bool isInList(int x, int y)
        {
            if (openList.Contains(GetNode(x,y))) return true;
            else if (closedList.Contains(GetNode(x,y))) return true;
            return false;
        }

        public bool RaiseNode(int x, int y, double val, bool wumpus)
        {
            TileNode CurNode = GetNode(x, y);

            if (!wumpus) CurNode.WumpusRisk = 0;
            if (openList.Contains(CurNode) && CurNode.Risk > 0)
            {
                if (CurNode.CurrentState == TileNode.State.RAISED)
                {
                    PlayerLog.Add("Pit at: (" + x + ", " + y +")");

                    CurNode.Risk = 1.0;
                    CurNode.Raise();
                    return true;
                }
                else if (CurNode.CurrentState == TileNode.State.NEW || CurNode.CurrentState == TileNode.State.OPEN || CurNode.CurrentState == TileNode.State.LOWER)
                {
                    CurNode.Risk = CurNode.Risk + val;
                    if (AgainstWall(Current) && AgainstWall(CurNode))
                    {
                        int j = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            TileNode node = GetNodeAdj(CurNode, i);
                            if (node != null && node.CurrentState == TileNode.State.OPEN) j++;
                        }
                        if (j == 1)
                        {
                            PlayerLog.Add("Pit at: (" + x + ", " + y + ")");

                            CurNode.Risk = 1;
                            CurNode.Raise();
                            return true;
                        }
                    }
                    if (CurNode.Risk >= .9)
                    {
                        PlayerLog.Add("Pit at: (" + x + ", " + y + ")");

                        CurNode.Risk = 1;
                        CurNode.Raise();
                        return true;
                    } //Should never hit this
                    CurNode.Raise();
                    openList.Add(CurNode);
                    return false;
                }
            }
            return false;
        }

        public bool LowerNode(int x, int y, double val)
        {
            TileNode CurNode = GetNode(x,y);

            if (openList.Contains(CurNode) && val == 0)
            {
                CurNode.Risk = 0;
                CurNode.WumpusRisk = 0;
                CurNode.Lower();
                if (CurNode.Risk < 0) CurNode.Risk = 0;
                return true;
            }
            return false;
        }

        public bool LowerNode(int x, int y)
        {
            TileNode CurNode = GetNode(x, y);

            if (openList.Contains(CurNode))
            {
                CurNode.Risk = 0;
                CurNode.Lower();
                if (CurNode.Risk < 0) CurNode.Risk = 0;
                return true;
            }
            return false;
        }

        public bool LowerNode(TileNode TN, double val)
        {
            TileNode CurNode = TN;

            if (openList.Contains(CurNode) && !(CurNode.Risk >= 1))
            {
                PlayerLog.Add("Risk Lowered: (" + CurNode.xLoc + ", " + CurNode.yLoc + ")");
                CurNode.Risk = CurNode.Risk - val;
                CurNode.Lower();
                if (CurNode.Risk < 0) CurNode.Risk = 0;
                return true;
            }
            return false;
        }

        public bool AgainstWall(TileNode TN)
        {
                if (TN.xLoc - 1 < LowerBound || TN.yLoc - 1 < LowerBound)
                {
                    return true;
                }
                else if (UpperBound != -2 && (TN.xLoc + 1 < LowerBound || TN.yLoc + 1 < LowerBound))
                {
                    return true;
                }
            return false;
        }

        public bool FindLowestPath(TileNode cur, int val)
        {
            TileNode node;
            cur.pathValue = val;
            if (val < pathValue || pathValue == -1)
            {
                for (int i = 0; i < 4; i++)
                {
                    node = GetNodeAdj(cur, i);
                    if (node != null && ((isAdjacent(node, Current) && !(node.Risk >= 0.35) && !(node.WumpusRisk >= 0.50) || (node.SameNode(Current)))))
                    {
                        if (node.SameNode(Current))
                        {
                            Current.SetBackPointer(cur);
                            Current.pathValue = val;
                            pathValue = val;
                        }
                        else
                        {
                            node.pathValue = val + 1;
                            node.SetBackPointer(cur);
                            Current.SetBackPointer(node);
                            Current.pathValue = val;
                            pathValue = val;
                        }
                        return true;
                    }
                    if (node != null && !(node.pathValue == val - 1 || node.pathValue == val + 1) && !(node.pathValue < cur.pathValue - 1 && node.pathValue != -2) && !(node.Risk >= 0.35) && !(node.WumpusRisk >= 0.50))
                    {
                        node.pathValue = val + 1;
                        node.SetBackPointer(cur);
                        FindLowestPath(node, val + 1);
                    }
                    else if (node != null && node.pathValue < val - 1 && !(node.pathValue == -2))
                    {
                        return true;
                    }
                }
            }
            return true;
        }

        public void ClearPathValues()
        {
            foreach (TileNode node in closedList)
            {
                node.pathValue = -2;
            }
            foreach (TileNode node in openList)
            {
                node.pathValue = -2;
            }
        }

        public TileNode GetNodeAdj(TileNode TN,int dir)
        {
            int x = 0;
            int y = 0;

            if (dir == 0) x = 1;
            else if (dir == 1) x = -1;
            else if (dir == 2) y = 1;
            else if (dir == 3) y = -1;
            TileNode UP = null;
            foreach (TileNode node in closedList)
            {
                if (node.xLoc == TN.xLoc+x && node.yLoc == TN.yLoc+y)
                {
                    return node;
                }
            }
            foreach (TileNode node in openList)
            {
                if (node.xLoc == TN.xLoc + x && node.yLoc == TN.yLoc + y)
                {
                    return node;
                }
            }
            return UP;
        }

        public void CloseNode(int x, int y)
        {
            TileNode CurNode = GetNode(x, y);

            if (CurNode != null)
            {
                PlayerLog.Add("BUMP!");

                openList.Remove(CurNode);
                closedList.Remove(CurNode);
                CurNode.Close();
                closedList.Add(CurNode);
                for (int i = 0; i < 4; i++)
                {
                    TileNode node = GetNodeAdj(CurNode, i);
                    if (node != null && openList.Contains(node))
                    {
                        openList.Remove(node);
                        CurNode.Close();
                        closedList.Add(CurNode);
                    }
                }
                if (CurNode.xLoc > CurNode.yLoc)
                {
                    UpperBound = CurNode.xLoc;
                }
                else
                {
                    UpperBound = CurNode.yLoc;
                }
            }
        }
    }
}
