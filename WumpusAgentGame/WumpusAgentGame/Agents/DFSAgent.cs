using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WumpusAgentGame.Search;

namespace WumpusAgentGame.Agents
{
    class DFSAgent : Agent
    {
        DFS DSearch;
        Stack<Action> Plan;

        int agentX = 0;
        int agentY = 0;

        Tile LastTile;
        Tile OlderTile;
        int lastX = 0;
        int lastY = 0;

        bool hasFound = false;

        public DFSAgent(List<string> log)
        {
            //Setup the DStar Search
            Plan = new Stack<Action>();
            Plan.Push(new Action());
            DSearch = new DFS(log);
        }



        //This method will only be called when the player is stopped
        public override Action GetAction(Tile CurrentTile)
        {
            Action CurAction = new Action();
            //if (info.AgentStep == true || !(info.StepOn))
            //{
                DSearch.AddNodes(agentX, agentY, CurrentTile._breeze, CurrentTile._stench);
                if (CurrentTile._stench)
                {
                }
                if ((Plan.Peek().Equals(new Action()) || CurrentTile._gold && !hasFound) && (!isSameTile(CurrentTile, LastTile) && !hasFound))
                {
                    if (CurrentTile._gold)
                    {
                        hasFound = true;
                    }
                    
                    Plan = DSearch.GetRoute(CurrentTile._gold);
                }
                else if (isSameTile(CurrentTile, LastTile))
                {
                    DSearch.CloseNode(agentX, agentY);
                    CurAction = StopPlan();
                    LastTile = OlderTile;
                }

                //info.AgentStep = false;
            //}

            //If Action, return it
            if (!Plan.Peek().Equals(new Action()))
            {
                CurAction = Plan.Pop();
                if (CurAction.CurrentCommand != Action.Command.PickUp && !CurAction.IsFiringAction())
                {
                    OlderTile = LastTile;
                    LastTile = CurrentTile;
                    UpdateLocation(CurAction);
                }
            }


            return CurAction;
        }

        public bool isSameTile(Tile A, Tile B)
        {
            if (A != null && B != null && A._posX == B._posX && A._posY == B._posY)
            {
                return true;
            }
            return false;
        }

        public Action StopPlan()
        {
            Action CurAction = Plan.Pop();
            CurAction = AbortPlan();
            return CurAction;
        }

        public void CloseNode(Action act)
        {
            UpdateLocation(act);
            DSearch.CloseNode(agentX, agentY);
            this.agentX = lastX;
            this.agentY = lastY;
        }

        public Action AbortPlan()
        {
            Action CurAction = new Action();
            Plan = new Stack<Action>();
            Plan.Push(new Action());
            this.agentX = lastX;
            this.agentY = lastY;
            return CurAction;
        }

        public void UpdateLocation(Action Act)
        {
            if (Act.CurrentCommand == Action.Command.Right)
            {
                lastX = agentX;
                lastY = agentY;
                agentX++;
            }
            else if (Act.CurrentCommand == Action.Command.Left)
            {
                lastX = agentX;
                lastY = agentY;
                agentX--;
            }
            else if(Act.CurrentCommand == Action.Command.Up)
            {
                lastX = agentX;
                lastY = agentY;
                agentY++;
            }
            else if (Act.CurrentCommand == Action.Command.Down)
            {
                lastX = agentX;
                lastY = agentY;
                agentY--;
            }
        }
    }
}
