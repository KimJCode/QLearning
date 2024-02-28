using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    public class Counter
    {
        public int WinCounterPlayer1 { get; set; } = 0;
        public int WinCounterPlayer2 { get; set; } = 0;
        public int LossCounterPlayer1 { get; set; } = 0;
        public int LossCounterPlayer2 { get; set; } = 0;
        public int DrawCounter { get; set; } = 0;
        public int GamesPlayedCounter { get; set; } = 0;
        public Counter() { }
        public void Reset()
        {
            WinCounterPlayer1 = 0;
            WinCounterPlayer2 = 0;
            LossCounterPlayer1 = 0;
            LossCounterPlayer2 = 0;
            DrawCounter = 0;
            GamesPlayedCounter = 0;
        }
    }
}
