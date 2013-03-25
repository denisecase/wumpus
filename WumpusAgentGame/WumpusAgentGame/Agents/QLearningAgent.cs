using System;
using System.Text;

namespace WumpusAgentGame.Agents
{
    /// <summary>
    /// Q Learning Agent for Wumpus Game 
    /// </summary>
    class QLearningAgent : Agent
    {
        /// <summary>
        /// Q Learning Agent constructor.
        /// </summary>
        public QLearningAgent()
        {
            InitializeAgent();
        }

        /// <summary>
        /// Get next action from the QLearning Agent given the current location tile.
        /// </summary>
        /// <param name="currentTile">Player's current tile</param>
        /// <returns>Action</returns>
        public override Action GetAction(Tile currentTile)
        {
            if (currentTile == null) return null;

            Action newAction = null;
            Game.RewardState rewardState;
            double reward;

            GetNewPercepts(out rewardState, out reward);

            if (Game.Instance.IsSmartTrial)
            {
                newAction = GetSmartAction(currentTile);
            }
            else  // learning
            {
                UpdateLearningBasedOnLastAction(currentTile, reward);
                newAction = GetLearningAction(currentTile);
            }
            lastTile = currentTile;
            lastAction = newAction;
            lastRewardState = rewardState;
            return newAction;
        }

        #region Private variables

        /// <summary>
        /// Gamma is the Q-Learning discount factor.  
        /// </summary>
        /// 
        private const double GAMMA = 0.85; // 0 = only current, 1 = only future
        private const int NUM_ACTIONS = 11;  //Enum.GetValues(typeof(Action.Command)).Length;
        private static int boardSize;

        private const int NUM_VALUES = 2;  // Q value & times visited
        private static int QVALUE = 0;  // naming the second Q Table index values
        private static int VISITS = 1;  // naming the second Q Table index values

        private const int NUM_STATES = 2;  // only two states to care about so far
        private static int NOT_CARRYING_GOLD = 0;  // naming the third Q Table index values
        private static int CARRYING_GOLD = 1;  // naming the third Q Table index values
        private bool lastCarryingGold = false;

        private static TileData[,] map;
        private Action lastAction = null;
        private Game.RewardState lastRewardState;
        private Tile lastTile = null;

        struct TileData
        {
            public double[, ,] qTable;   // holds Q Table for each action from this tile
        }

        #endregion // Private variables

        #region Private methods

        /// <summary>
        /// Initialization procedures when first making the agent.
        /// </summary>
        private static void InitializeAgent()
        {
            // If first learning trial, zero the Qtables
            if (Game.Instance.GameCounter == 0)
            {
                boardSize = Game.Instance.BoardX;
                map = new TileData[boardSize, boardSize];
                ResetAllQTables();
            }
        }

        /// <summary>
        /// Set all Q table values to zero (only performed before the first trial).
        /// </summary>
        private static void ResetAllQTables()
        {
            int numActions = Enum.GetValues(typeof(Action.Command)).Length;
            if (Game.Instance.GameCounter == 0)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    for (int y = 0; y < boardSize; y++)
                    {
                        TileData cell = new TileData();
                        double[, ,] qTable = new double[numActions, 2, 2];

                        for (int i = 0; i < numActions; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                for (int k = 0; k < 2; k++)
                                {
                                    qTable[i, j, k] = 0.0;
                                }
                            }
                        }
                        cell.qTable = qTable;
                        map[x, y] = cell;
                    }
                }
            }
        }

        /// <summary>
        /// Get new precepts after the last action taken.
        /// </summary>
        /// <param name="newRewardState">Last reward state (e.g. Player Won)</param>
        /// <param name="reward">Double value of the immediate reward</param>
        private void GetNewPercepts(out Game.RewardState newRewardState, out double reward)
        {

            if (GameEndedAfterLastAction())
            {
                newRewardState = Game.Instance.EndGameRewardState;
                reward = Game.Instance.GetRewardPercept(Game.Instance.EndGameRewardState);
                Game.Instance.AddToPlayerLog("Got percepts. Reward is " + reward.ToString() + ". New state is " + Game.Instance.EndGameRewardState.ToString());
                DisplayAllQValues(Game.Instance.EndGameCarryingGold);
                lastCarryingGold = Game.Instance.EndGameCarryingGold;
            }
            else
            {
                newRewardState = Game.Instance.CurrentRewardState;
                reward = Game.Instance.GetRewardPercept(Game.Instance.CurrentRewardState);
                Game.Instance.AddToPlayerLog("Got percepts. Reward is " + reward.ToString() + ". New state is " + Game.Instance.CurrentRewardState.ToString());
                DisplayAllQValues(Game.Instance.PlayerCarryingGold);
                lastCarryingGold = Game.Instance.PlayerCarryingGold;
            }
        }
        /// <summary>
        /// Get a random action.
        /// </summary>
        /// <param name="currentTile">Current location tile</param>
        /// <returns>Action</returns>
        private Action GetLearningAction(Tile currentTile)
        {
            int iGold = Game.Instance.PlayerCarryingGold ? CARRYING_GOLD : NOT_CARRYING_GOLD;
            int x = currentTile._posX;
            int y = currentTile._posY;

            Action action = new Action();
            double value;
            // pick a random, non-punitive action
            do
            {
                if (IsEscapeAvailable(currentTile))
                {
                    action = ClimbOut();
                    Game.Instance.WinCounter++;
                }
                else if (IsGoldAvailable(currentTile))
                {
                    action = PickupGold();
                }
                else
                {
                    action = MoveRandomlyFrom();
                }
                int iAction = Action.GetIndex(action.CurrentCommand);
                value = map[x, y].qTable[iAction, QVALUE, iGold];
            } while (value < 0); // avoid actions that don't pay...
            return action;
        }

        /// <summary>
        /// Select an action based on Q-Learning.
        /// </summary>
        /// <param name="currentTile">Current location tile</param>
        /// <returns>Action</returns>
        private Action GetSmartAction(Tile currentTile)
        {
            Game.Instance.AddToPlayerLog(" The player " + GoldText(Game.Instance.PlayerCarryingGold) + " right now.");
            Game.Instance.AddToPlayerLog("Selecting Action given: " + GetQTableText(currentTile, Game.Instance.PlayerCarryingGold));
            Action smartAction = new Action();

            if (IsEscapeAvailable(currentTile))
            {
                smartAction = ClimbOut();
                Game.Instance.WinCounter++;
            }
            else if (IsGoldAvailable(currentTile))
            {
                smartAction = PickupGold();
            }
            else
            {
                smartAction = MoveFrom(currentTile);
            }
            return smartAction;
        }

        private Action MoveFrom(Tile currentTile)
        {
            int iGold = Game.Instance.PlayerCarryingGold ? CARRYING_GOLD : NOT_CARRYING_GOLD;

            double value;
            double bestQ = Double.MinValue;
            int iAction = -1;  // the best action to take from here
            string s = "PICKING FROM ";

            for (int i = 0; i < 9; i++)
            {
                s += map[currentTile._posX, currentTile._posY].qTable[i, QVALUE, iGold] + " ";
            }
            Game.Instance.AddToPlayerLog(s + "\n");

            Random Rand = new Random(DateTime.Now.Millisecond);

            // Game.Instance.AddToPlayerLog("Selecting Action given: " + GetQTableText(CurrentTile, carryingGold) + " " + GoldText(carryingGold));
            for (int i = 0; i < 4; i++)  // moving actions...  temp
            {
                double thisQValue = map[currentTile._posX, currentTile._posY].qTable[i, QVALUE, iGold];
                double thisApproxValue = thisQValue + Rand.Next(-9, 9); // introduce a little randomness...
                if (thisApproxValue > bestQ && !UndoingLastAction(i))
                {
                    iAction = i;
                    bestQ = thisQValue;
                }
            }
            value = map[currentTile._posX, currentTile._posY].qTable[iAction, QVALUE, iGold];
            Game.Instance.AddToPlayerLog("The bestQ value was " + value.ToString() + " for action " + Action.GetAction(iAction).ToString());
            Action action = new Action();
            action.CurrentCommand = Action.GetAction(iAction);
            return action;
        }

        private Action PickupGold()
        {
            Game.Instance.AddToPlayerLog("Picking up gold (we're standing on some and don't have any yet");
            Game.Instance.LastCarryingGold = false;
            Game.Instance.PlayerCarryingGold = true;
            Action action = new Action();
            action.CurrentCommand = Action.Command.PickUp;
            return action;
        }

        private Action ClimbOut()
        {
            Game.Instance.AddToPlayerLog("Climbing out (we're carrying gold and at the escape location");
            Game.Instance.LastCarryingGold = true;
            Game.Instance.PlayerCarryingGold = true;
            Action action = new Action();
            action.CurrentCommand = Action.Command.Climb;
            return action;
        }

        private Action MoveRandomlyFrom()
        {
            Random Rand = new Random(DateTime.Now.Millisecond);
            Action action = new Action();
            int num;
            num = Rand.Next(4);
            if (num == 0) action.CurrentCommand = Action.Command.Left;
            else if (num == 1) action.CurrentCommand = Action.Command.Right;
            else if (num == 2) action.CurrentCommand = Action.Command.Up;
            else if (num == 3) action.CurrentCommand = Action.Command.Down;
            return action;
        }

        private static bool IsGoldAvailable(Tile CurrentTile)
        {
            return !Game.Instance.PlayerCarryingGold && CurrentTile._gold;
        }

        private static bool IsEscapeAvailable(Tile CurrentTile)
        {
            return Game.Instance.PlayerCarryingGold && CurrentTile._posX == 0 && CurrentTile._posY == (Game.Instance.BoardY - 1);
        }

        private static bool GameEndedAfterLastAction()
        {
            return Game.Instance.CurrentRewardState == Game.RewardState.PlayerDied || Game.Instance.CurrentRewardState == Game.RewardState.PlayerWon;
        }

        private bool UndoingLastAction(int iAction)
        {
            if (lastAction == null) return false;
            Action.Command thisAction = Action.GetAction(iAction);
            if (thisAction == Action.Command.Left && lastAction.CurrentCommand == Action.Command.Right)
            {
                return true;
            }
            if (thisAction == Action.Command.Right && lastAction.CurrentCommand == Action.Command.Left)
            {
                return true;
            }
            if (thisAction == Action.Command.Up && lastAction.CurrentCommand == Action.Command.Down)
            {
                return true;
            }
            if (thisAction == Action.Command.Down && lastAction.CurrentCommand == Action.Command.Up)
            {
                return true;
            }
            return false;

        }

        /// <summary>
        /// Find the maximum possible Q value for any potential action.
        /// </summary>
        /// <param name="td">Tile data</param>
        /// <returns>Highest Q value</returns>
        private double MaxQ(TileData td, int iGold)
        {
            double maxQ = td.qTable[0, 0, iGold];
            for (int i = 1; i < NUM_ACTIONS; i++)
            {
                if (maxQ < td.qTable[i, QVALUE, iGold])
                {
                    maxQ = td.qTable[i, QVALUE, iGold];
                }
            }
            return maxQ;
        }

        /// <summary>
        /// Sets the new Qvalue according to the equation:
        /// Q(state,action) = PreviousQ(state,action) + alpha*
        ///    (Reward(state) + gamma*maxQ(state', action') - PreviousQ(state,action))
        /// where alpha = learning rate assumed to be 1/number of visits
        /// and gamma = discount rate where 0 = all current rewards and 1 = all future rewards
        /// and we want to build longer paths, so gamma is higher (around .8 or .85)
        /// 
        /// Q = (1-alpha)Qp + (alpha)*learnedvalue
        /// where
        /// learnedvalue = immediateReward + gamma*maxFutureValue
        /// 
        /// Russell & Norvig AIMA page 844 
        /// </summary>
        /// <param name="immediateReward">Immediate Reward</param>
        /// <param name="lastTile">Last Tile location</param>
        /// <param name="lastAction">Last Action taken</param>
        /// <param name="wasCarryingGold">Boolean indicating whether the player was carrying gold</param>
        /// <param name="curTile">Current Tile location</param>
        private void SetQValue(double immediateReward, Tile lastTile, Action lastAction, bool wasCarryingGold, Tile curTile)
        {
            if (lastTile == null || lastAction == null || curTile == null || lastAction.CurrentCommand == Action.Command.None) return;
            int iAction = Action.GetIndex(lastAction.CurrentCommand);
            int iGold = wasCarryingGold ? CARRYING_GOLD : NOT_CARRYING_GOLD;
            TileData lastData = map[lastTile._posX, lastTile._posY];
            int lastActionOccurances = (int)lastData.qTable[iAction, VISITS, iGold];

            lastData.qTable[iAction, VISITS, iGold] = ++lastActionOccurances; // increment occurrences

            if (immediateReward < 0)
            {
                lastData.qTable[iAction, QVALUE, iGold] = immediateReward;
            }
            else
            {
                TileData curData = map[curTile._posX, curTile._posY];
                double alpha = 1.0 / (double)lastActionOccurances;
                double oldActionQValue = lastData.qTable[iAction, QVALUE, iGold];
                double maxFutureValue = MaxQ(curData, iGold);  // whats the best I can do from here?
                double learnedValue = immediateReward + (GAMMA * maxFutureValue);
                lastData.qTable[iAction, QVALUE, iGold] = oldActionQValue + alpha * (learnedValue - oldActionQValue);
            }
        }

        /// <summary>
        /// Display all table results given carrying gold or not.
        /// </summary>
        /// <param name="gotGold">Is the player carrying the gold?</param>
        /// <returns>Text output</returns>
        public string DisplayAllQValues(bool gotGold)
        {
            Array values = Enum.GetValues(typeof(Action.Command));
            int numActions = values.Length;

            string s = GoldText(gotGold);
            int iGold = gotGold ? CARRYING_GOLD : NOT_CARRYING_GOLD;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\t----  NOT CARRYING GOLD ----------------\t\t\t  ------ CARRYING GOLD -------(L R U D P SL SR SU SD C N)");
            sb.AppendLine("\t(0,y)\t(1,y)\t(2,y)\t(3,y)\t(4,y)" + "\t\t" + "\t(0,y)\t(1,y)\t(2,y)\t(3,y)\t(4,y)");  // header row

            for (int y = 0; y < boardSize; y++)  // for each layer y
            {

                for (int i = 0; i < numActions; i++)  // for each row action
                {
                    sb.Append("(x," + y + ")\t");  // heading column
                    for (int x = 0; x < boardSize; x++) // for each column x
                    {
                        TileData cell = map[x, y];
                        sb.AppendFormat("{0:###0.00}\t", (Convert.ToInt32(cell.qTable[i, QVALUE, NOT_CARRYING_GOLD]*100)/100).ToString());
                    }
                    sb.AppendFormat("\t\t");
                    for (int x = 0; x < boardSize; x++) // for each column x
                    {
                        TileData cell = map[x, y];
                        sb.AppendFormat("{0:###0.00}\t", (Convert.ToInt32(cell.qTable[i, QVALUE, CARRYING_GOLD] * 100) / 100).ToString());
                    }
                    sb.AppendLine();  // end the row
                }
                sb.AppendLine();  //blank line between layers
            }
            Game.Instance.ClearPlayerLog();
            Game.Instance.AddToPlayerLog("Currently, the agent is " + s + "\n" + sb.ToString());

            return sb.ToString();
        }

        private void UpdateLearningBasedOnLastAction(Tile CurrentTile, double reward)
        {
            if (lastTile == null && Game.Instance.GameCounter > 0) // if it's due to a new game
            {
                if (Game.Instance.EndGameAction == null) return;
                int x = Game.Instance.EndGameX;
                int y = Game.Instance.EndGameY;
                lastTile = Game.Instance.GetTile(x, y);
                //Game.Instance.LastCarryingGold = Game.Instance.EndGameCarryingGold;
                //Game.Instance.CurrentRewardState = Game.Instance.EndGameRewardState;
                if (Game.Instance.EndGameRewardState == Game.RewardState.PlayerDied && CurrentTile.IsBase) // then we died & it missed our dying location
                {
                    Tile DeathTile = GetEndTileAfterDying(lastTile, Game.Instance.EndGameAction);
                    SetQValue(reward, lastTile, Game.Instance.EndGameAction, Game.Instance.EndGameCarryingGold, DeathTile);
                }
                else
                {
                    SetQValue(reward, lastTile, Game.Instance.EndGameAction, Game.Instance.EndGameCarryingGold, CurrentTile);
                }
                Game.Instance.AddToPlayerLog("Updating QValue for (" + lastTile._posX + ", " + lastTile._posY + ") in state " + GoldText(Game.Instance.EndGameCarryingGold) +
                          " based on action " + Game.Instance.EndGameAction.CurrentCommand.ToString() + " giving reward of " + reward.ToString());

            }
            else
            {
                if (Game.Instance.LastAction == null) return;
                SetQValue(reward, lastTile, Game.Instance.LastAction, Game.Instance.LastCarryingGold, CurrentTile);
                Game.Instance.AddToPlayerLog("Updating QValue for (" + lastTile._posX + ", " + lastTile._posY + ") in state " + GoldText(Game.Instance.LastCarryingGold) + " based on action " + Game.Instance.LastAction.CurrentCommand.ToString() + " giving reward of " + reward.ToString());
            }
        }



        /// <summary>
        /// Get a text description of the Q Table for a given tile.
        /// </summary>
        /// <param name="currentTile"></param>
        /// <returns></returns>
        private string GetQTableText(Tile currentTile, bool gotGold)
        {
            Array values = Enum.GetValues(typeof(Action.Command));
            int x = currentTile._posX;
            int y = currentTile._posY;
            string s = "Qtable values for (" + x + ", " + y + ") ";
            s += GoldText(gotGold);
            int iGold = gotGold ? CARRYING_GOLD : NOT_CARRYING_GOLD;
            for (int i = 0; i < NUM_ACTIONS; i++)
            {
                //if (map[x, y].qTable[i, QVALUE, iGold] != 0.0)  // only display if the Q value isn't a boring zero
                //{
                s += " " + values.GetValue(i).ToString();  // the action
                s += " (" + map[x, y].qTable[i, QVALUE, iGold] + "," + map[x, y].qTable[i, VISITS, iGold] + "), ";
                //}
            }

            return s;
        }

        public Tile GetEndTileAfterDying(Tile lastTile, Action lastAction)
        {
            Tile t;
            if (lastAction.CurrentCommand == Action.Command.Right && !lastTile._isRightWall)
            {
                t = new Tile(lastTile._posX + 1, lastTile._posY, true);
            }
            else if (lastAction.CurrentCommand == Action.Command.Left && !lastTile._isLeftWall)
            {
                t = new Tile(lastTile._posX - 1, lastTile._posY, true);
            }
            else if (lastAction.CurrentCommand == Action.Command.Up && !lastTile._isTopWall)
            {
                t = new Tile(lastTile._posX, lastTile._posY - 1, true);
            }
            else if (lastAction.CurrentCommand == Action.Command.Down && !lastTile._isBottomWall)
            {
                t = new Tile(lastTile._posX, lastTile._posY + 1, true);
            }
            else
            {
                t = new Tile(lastTile._posX, lastTile._posY, true);  // we didn't actually move 
            }
            return t;
        }

        private string GoldText(bool gotGold)
        {
            return gotGold ? "carrying gold" : "not carrying Gold";
        }

        #endregion // Private methods

    }
}
