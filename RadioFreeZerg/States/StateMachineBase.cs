using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NLog;

namespace RadioFreeZerg.States
{
    /// <summary> Finite state machine. </summary>
    public abstract class StateMachineBase<TStateId, TStateData, TStateEvent> where TStateId : struct
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentDictionary<TStateId, State<TStateId, TStateData, TStateEvent>> idsToStates = new();
        private readonly object locker = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="StateMachineBase{TStateId,TStateData, TStateEvent}" /> class.
        /// </summary>
        protected StateMachineBase(TStateData initialStateData) => Data = initialStateData;

        /// <summary> Gets the state count. </summary>
        public int Count => idsToStates.Count;

        /// <summary> Gets the reference to the current state. </summary>
        /// <returns>Current state or null.</returns>
        public State<TStateId, TStateData, TStateEvent>? Current { get; private set; }

        /// <summary> Gets ID of the current state. </summary>
        /// <returns>ID of the current state.</returns>
        public TStateId CurrentId => Current?.Id ?? default;

        /// <summary> Gets the reference to the state data. </summary>
        /// <returns> Current state data.</returns>
        public TStateData Data { get; set; }

        /// <summary> Checks if state with given ID already exists. </summary>
        /// <param name="stateId">ID in question.</param>
        /// <returns>true if state exists; false otherwise.</returns>
        public bool Exists(TStateId stateId) => idsToStates.ContainsKey(stateId);

        /// <summary> Get state with specified state ID. </summary>
        /// <param name="stateId">State identifier.</param>
        /// <typeparam name="T">Type of the state to get.</typeparam>
        /// <returns>Found state or null if state was not found or its type is incorrect.</returns>
        public T? GetOrNull<T>(TStateId stateId) where T : State<TStateId, TStateData, TStateEvent> =>
            (T?) GetOrNull(stateId);

        /// <summary> Get state with specified state ID. </summary>
        /// <param name="stateId">State identifier.</param>
        /// <returns>Found state or null if state was not found.</returns>
        public State<TStateId, TStateData, TStateEvent>? GetOrNull(TStateId stateId) {
            idsToStates.TryGetValue(stateId, out var foundState);
            return foundState;
        }

        /// <summary> Explicit transition method — cause a transition outside of the event handling. </summary>
        /// <param name="stateId">State to transit into.</param>
        public void Transition(TStateId stateId) {
            log.Trace($"Transitioning from {Current?.Id.ToString() ?? "<null>"} to {stateId}.");
            ExitCurrentState();
            // If a state ID was provided, find and enter the next state with this ID.
            if (!IsDefaultId(ref stateId))
                lock (locker) {
                    if (idsToStates.TryGetValue(stateId, out var nextState)) {
                        var prevStateId = CurrentId;
                        Current = nextState;
                        log.Trace($"Entering {Current.Id}.");
                        Current.StateEnter(prevStateId, Data);
                        log.Trace($"Entered {Current.Id}.");
                    } else {
                        throw new InvalidOperationException($"Can't get the next state with ID: {stateId}");
                    }
                }
        }


        public void Add(IEnumerable<State<TStateId, TStateData, TStateEvent>> states) {
            foreach (var state in states) {
                Add(state);
            }
        }

        /// <summary> Adds a state. </summary>
        /// <param name="newState">State to add.</param>
        public void Add(State<TStateId, TStateData, TStateEvent> newState) {
            if (Exists(newState.Id))
                throw new ArgumentException("State with the same ID already exists", nameof(newState));

            if (idsToStates.TryAdd(newState.Id, newState)) newState.Reset(Data);
        }

        /// <summary> Resets the machine and all its states. </summary>
        /// <param name="initialState">Resets machine to initial state.</param>
        public void Reset(TStateId initialState = default) {
            log.Trace($"Resetting FSM to {initialState}.");
            ExitCurrentState();

            lock (locker) {
                foreach (var kvp in idsToStates) {
                    log.Trace($"Resetting {kvp.Value.Id}.");
                    kvp.Value.Reset(Data);
                }
            }

            if (!IsDefaultId(ref initialState))
                Transition(initialState);
        }

        /// <summary> Calls event on current state. </summary>
        public void Event(TStateEvent stateEvent) {
            lock (locker) {
                if (Current is not null) {
                    var nextStateId = Current.HandleEvent(stateEvent, Data);
                    // if state was changed, do a transition
                    if (!IsDefaultId(ref nextStateId)) {
                        var currentStateId = Current.Id;
                        if (!IdEquals(ref nextStateId, ref currentStateId))
                            Transition(nextStateId);
                    }
                }
            }
        }

        private void ExitCurrentState() {
            lock (locker) {
                if (Current is not null) {
                    log.Trace($"Exiting {Current.Id}.");
                    Current.StateExit(Data);
                    log.Trace($"Exited {Current.Id}.");
                }
            }
        }

        private static bool IsDefaultId(ref TStateId stateId) =>
            EqualityComparer<TStateId>.Default.Equals(stateId, default);

        private static bool IdEquals(ref TStateId a, ref TStateId b) =>
            EqualityComparer<TStateId>.Default.Equals(a, b);
    }
}