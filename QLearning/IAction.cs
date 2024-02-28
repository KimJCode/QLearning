using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AI {

    // Interface for an action. We leave this interface pretty generic. 
    // The only information we require is a name for an action
    public interface IAction {
        string Name { get; }
    }
}
