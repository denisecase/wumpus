using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace WumpusAgentGame
{
    public partial class GameControl : Form
    {
        public GameControl()
        {
            InitializeComponent();
            GameOutput = new List<string>();
        }

        private List<string> GameOutput;

        public void setSeed(int sd)
        {
            lblSeed.Text = Convert.ToString(sd);
        }

        public void SetOutputList(List<string> ls)
        {
            GameOutput = ls;
        }

        public void UpdateList()
        {
            listGameOutput.Items.Clear();
            for (int i = GameOutput.Count-1; i >= 0; i--)
            {
                listGameOutput.Items.Add(GameOutput.ElementAt(i));
            }
        }
        public void UpdateScore(int i)
        {
            lblScore.Text = Convert.ToString(i);
        }

        public void SetStatus(string s)
        {
            txtStatus.Text = s;
        }

        internal void SetLearningTrialDisplay(int gameCounter, int numTrials, bool isVisible, int winCounter)
        {
            lblLearningTrial.Visible = isVisible;

            if (isVisible)
            {
                if (gameCounter == numTrials)  // done learning... playing smart round
                {
                    lblLearningTrial.Text = "Done with " + gameCounter + " learning trials; playing smart round.";
                }
                else  // still learning
                {
                    lblLearningTrial.Text = "Learning Trial: " + (gameCounter + 1) + "/" + numTrials + " (" + winCounter + " win" + (winCounter==1? ")":"s)");
                }

            }
            else
            {
                lblLearningTrial.Text = String.Empty;
            }

        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            var sb = new StringBuilder();

            foreach (var item in listGameOutput.Items)
            {
                sb.AppendLine(item.ToString());
            }
            MessageBox.Show(sb.ToString(), "Hit CTRL-C to Copy");
        }

        private void btnStopStart_Click(object sender, EventArgs e)
        {
            
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
         
        }

       
    }
}
