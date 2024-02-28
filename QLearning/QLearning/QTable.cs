using System;
using System.Collections.Generic;
using AI.Util;

namespace AI.QLearning {
    public class QTable {
        public QTable() {
            // Lazy initialization: At startup, the dictionary is empty, which means
            // that whenever we find a state or an action not contained in the dictionary
            // we set it's value to neutral (0.0) in the access functions below
            ValueTable = new Dictionary<uint, Dictionary<IAction, double>>();
        }

        #region --- Access functions ---
        public double ValueFor(uint StateId, IAction a) {
            // Lazy Initialization

            if (!ValueTable.ContainsKey(StateId)) {
                // We have not visited this state before
                ValueTable[StateId] = new Dictionary<IAction, double>();
            }

            if (!ValueTable[StateId].ContainsKey(a)) {
                // We have never performed this action in the state before
                ValueTable[StateId][a] = 0.0;
            }

            return ValueTable[StateId][a];
        }

        public double ValueFor(uint StateId, List<IAction> AvailableActions) {
            // Lazy Initialization

            if (!ValueTable.ContainsKey(StateId)) {
                // We have not visited this state before
                ValueTable[StateId] = new Dictionary<IAction, double>();
                // set all available actions to neutral (0.0) in the table
                foreach (var a in AvailableActions) {
                    ValueTable[StateId][a] = 0.0;
                }
                return 0.0;
            }

            // the quality of a state is defined as the quality of 
            // the best action available in this state
            double MaxValue = Double.MinValue;
            foreach (var a in AvailableActions) {

                // check again, if the action is contained in the list.
                // Note that QLearning does not require the available
                // actions to be consistens for a given state, i.e. the
                // list of available actions in a given state may change
                // over time
                if (!ValueTable[StateId].ContainsKey(a)) {
                    ValueTable[StateId][a] = 0.0f;
                }

                double value = ValueTable[StateId][a];
                if (MaxValue < value) {
                    MaxValue = value;
                }

            }
            return MaxValue;
        }

        public IAction BestAction(uint StateId, List<IAction> AvailableActions) {
            
            // Lazy initialization

            if (!ValueTable.ContainsKey(StateId)) {
                // we have not visited this state before
                ValueTable[StateId] = new Dictionary<IAction, double>();
                // set all available actions to neutral (0.0) in the table
                foreach(var a in AvailableActions) {
                    ValueTable[StateId][a] = 0.0;
                }
                return RandomList<IAction>.RandomEntry(AvailableActions);
            }

            // It is very important here to not only find one of the 
            // best action! If we have several actions which are have
            // the same quality, we need to chose one randomly! If we
            // don't do this, we will end up in only very few states.

            List<IAction> bestActions = new List<IAction>();
            
            double maxValue = double.MinValue;
            foreach (var a in AvailableActions) {
                if (!ValueTable[StateId].ContainsKey(a)) {
                    ValueTable[StateId][a] = 0.0f;
                }

                double value = ValueTable[StateId][a];
                if (maxValue < value) {
                    maxValue = value;
                    bestActions.Clear();
                    bestActions.Add(a);
                }
                if (maxValue == value) {
                    bestActions.Add(a);
                }
            }

            // this is a generic function, which choses a random action from
            // the given list (see Util.cs for the definition)
            return RandomList<IAction>.RandomEntry(bestActions);

        }
        #endregion

        #region --- Setter functions ----
        public void SetValue(uint StateId, IAction a, double newValue) {


            if (!ValueTable.ContainsKey(StateId)) {
                // We have not visited this state before
                ValueTable[StateId] = new Dictionary<IAction, double>();
            }

            // store the value in the table
            ValueTable[StateId][a] = newValue;

        }
        #endregion

        #region --- Private Members ---
        // this is the big lookup table, which stores a quality value (double)
        // for each action (IAction) in each state (IGameState.Id)
        private Dictionary<uint, Dictionary<IAction, double>> ValueTable;
        #endregion
    }
}
