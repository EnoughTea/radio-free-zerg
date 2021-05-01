using System;
using System.Threading.Tasks;
using NLog;
using RadioFreeZerg.CuteRadio;
using RadioFreeZerg.States;

namespace RadioFreeZerg
{
    internal class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args) {
            var appStateMachine = new AppStateMachine(new AppStateData());
            appStateMachine.Add(new InitialState());
            appStateMachine.Add(new QuittingState());
            appStateMachine.Add(new StationSearchState());
            appStateMachine.Transition(AppStateId.Initial);

            Console.CancelKeyPress += (_, _) => appStateMachine.Transition(AppStateId.Quitting);
            AppDomain.CurrentDomain.ProcessExit += (_, _) => appStateMachine.Transition(AppStateId.Quitting);
            while (appStateMachine.Current?.Id != AppStateId.Quitting) {
                var userInput = Console.ReadLine();
                appStateMachine.Event(userInput);
            }
        }
    }
}