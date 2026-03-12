using System;
using GonDraz.StateMachine;

namespace GlobalState
{
    public partial class GlobalStateMachine : BaseGlobalStateMachine<GlobalStateMachine>
    {
        protected override Type InitialState()
        {
            return typeof(PreLoaderState);
        }
    }
}