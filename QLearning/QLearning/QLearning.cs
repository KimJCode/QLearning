using System;
using AI.Util;

namespace AI.QLearning
{
    public class QLearningAI
    {
        #region --- Constructor ---
        public QLearningAI() {
            LearnedTable = new QTable();
        }
        #endregion 

        #region  --- Public Properties ---
        public IGameState GameState { get; set; } = null;
        public double ExplorationRate { get; set; } = 0.8f;
        public double DiscountRate { get; set; } = 0.75f;
        public double LearningRate { get; set; } = 0.3f;
        #endregion

        #region --- Public Member Functions ---
        public void Learn(int numIterations) {

            if (GameState == null) throw new InvalidOperationException("GameState has not been initialized!");

            for(int i = 0; i < numIterations; i++) {
                Step();
            }
        }
        #endregion

        #region --- Private Helper Functions ----
        protected void Step() {

            uint CurrentState = GameState.Id;
            IAction a = SelectAction(GameState);
            double Reward = GameState.ExecuteAction(a);
            uint FollowState = GameState.Id;

            double OldQuality = LearnedTable.ValueFor(CurrentState, a);
            double FollowStateQuality = LearnedTable.ValueFor(FollowState, GameState.PossibleActions);
            double NewQuality = 
                (1.0 - LearningRate) * OldQuality +
                (LearningRate) * (Reward + DiscountRate * FollowStateQuality);

            if (NewQuality != OldQuality) {
                LearnedTable.SetValue(CurrentState, a, NewQuality);
            }
        }

        private IAction SelectAction(IGameState state) {

            if (Rand.NextDouble() < ExplorationRate) {
                return RandomList<IAction>.RandomEntry(state.PossibleActions);
            } else { 
                return LearnedTable.BestAction(state.Id, state.PossibleActions);
            }
        }

        #endregion

        #region --- Private Member Variables ---

        private QTable LearnedTable;
        private Random Rand = new Random();
        
        #endregion
    }
}
