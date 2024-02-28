using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AI {

    // interface for the game state required for learning
    public interface IGameState {

        // the unique integer Id which encodes ALL relevant information
        // of the given game state
        uint Id { get; }

        // a list of actions which may be performed in the given state. The
        // algorithm relies on the fact, that this list must not contain any
        // invalid action, i.e. actions that cannot be performed in the current
        // state
        List<IAction> PossibleActions { get; }

        // Execute the action and return the reward (positive) or penalty (negative).
        // After the action is executed, the Id-Property of the game state has most 
        // likely changed (though not necessarily) 
        double ExecuteAction(IAction a);

    }
}
