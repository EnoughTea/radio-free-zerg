using System;
using System.Collections.Generic;

namespace RadioFreeZerg.States
{
    /// <summary> Basic state. </summary>
    public abstract class State<TStateId, TStateData, TStateEvent> where TStateId: struct {
        /// <summary> State ID. Serves as a key in the dictionary of states. </summary>
        public TStateId Id { get; }

        /// <summary> Initializes a new instance of the <see cref="State{TStateId, TStateData}"/> class. </summary>
        /// <param name="stateId">State's ID.</param>
        protected State(TStateId stateId) {
            if (EqualityComparer<TStateId>.Default.Equals(default, stateId)) {
                throw new ArgumentException("State ID should not be a default value", nameof(stateId));
            }

            Id = stateId;
        }

        /// <summary> Called when the state is entered. </summary>
        /// <param name="previousStateId">ID of the previous state.</param>
        /// <param name="data">State data.</param>
        public virtual void StateEnter(TStateId previousStateId, TStateData data) { }
    
        /// <summary> Called when the state it exited. </summary>
        /// <param name="data">State data.</param>
        public virtual void StateExit(TStateData data) { }
    
        /// <summary> Resets state. </summary>
        /// <param name="data">State data.</param>
        public virtual void Reset(TStateData data) { }

        /// <summary> Handles event with the given name and returns ID of the next state.
        /// default(TStateId) means no transition. </summary>
        /// <param name="data">State data.</param>
        /// <param name="stateEvent">Event to handle.</param>
        /// <returns>ID of the next state, if event results in a state change; default(TStateId) otherwise.</returns>
        public virtual TStateId HandleEvent(TStateEvent stateEvent, TStateData data) {
            return default;
        }
    }
}