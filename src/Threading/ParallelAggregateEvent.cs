using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Neuralia.Blockchains.Tools.Threading
{
    public abstract class ParallelAggregateEvent<T_DELEGATE> where T_DELEGATE : Delegate
    {
        /// <summary>
        /// True if the event has no handler; otherwise false.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsEventNull();

        /// <summary>
        /// Invoke all delegates asynchronously or synchronously.
        /// </summary>
        /// <param name="delegates"></param>
        /// <param name="executeSynchronously"></param>
        /// <param name="args"></param>
        protected void invokeDelegates(IEnumerable<Delegate> delegates, bool executeSynchronously, params object[] args)
        {
            IEnumerable<T_DELEGATE> castedDelegates = delegates.Cast<T_DELEGATE>();

            //We collect ourself the exceptions since Parallel never guaranteed to continue upon throwing in a thread (it's a gamble).
            //If you want to test: Try raising the delegates (with at least one throwing) in Parallel.ForEach with MaxDegreeOfParallelism = 1,
            //  you'll see that it stops at the first exception (just like when raising a normal event).
            List<Exception> exceptions = new List<Exception>();
            Action<T_DELEGATE> action = d =>
            {
#pragma warning disable CA1031 // We DO want to catch general exception types
                try
                {
                    d?.Method.Invoke(d.Target, args);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
#pragma warning restore CA1031 // We DO want to catch general exception types
            };

            //Parallel.ForeEach waits synchronously that all delegates complete, UNLESS a delegate is async. But, an async event handler should always handle errors itself.
            if (executeSynchronously)
            {
                var parallelOption = new ParallelOptions() { MaxDegreeOfParallelism = 1, TaskScheduler = TaskScheduler.Current };
                Parallel.ForEach(castedDelegates, parallelOption, action);
            }
            else
            {
                Parallel.ForEach(castedDelegates, action);
            }
            
            //If we have collected any exception: we aggregate them, flatten any aggregate exceptions together and throw.
            if (exceptions.Count > 0)
                throw new AggregateException(exceptions).Flatten();
        }
    }

    public class ParallelAggregateEventHandler : ParallelAggregateEvent<EventHandler>
    {
        public event EventHandler Event;
        public override bool IsEventNull() => this.Event == null;

        public void Invoke(object sender, EventArgs e, bool synchronous = false)
        {
            if (Event != null)
            {
                this.invokeDelegates(Event.GetInvocationList(), synchronous, sender, e);
            }
        }
    }

    public class ParallelAggregateEventHandler<TEventArgs> : ParallelAggregateEvent<EventHandler<TEventArgs>>
    {
        public event EventHandler<TEventArgs> Event;
        public override bool IsEventNull() => this.Event == null;

        public void Invoke(object sender, TEventArgs e, bool synchronous = false)
        {
            if (Event != null)
            {
                this.invokeDelegates(Event.GetInvocationList(), synchronous, sender, e);
            }
        }
    }

    public class ParallelAggregateEventAction : ParallelAggregateEvent<Action>
    {
        public event Action Event;
        public override bool IsEventNull() => this.Event == null;

        public void Invoke(bool synchronous = false)
        {
            if (Event != null)
            {
                this.invokeDelegates(Event.GetInvocationList(), synchronous);
            }
        }
    }

    public class ParallelAggregateEventAction<T1> : ParallelAggregateEvent<Action<T1>>
    {
        public event Action<T1> Event;
        public override bool IsEventNull() => this.Event == null;

        public void Invoke(T1 arg, bool synchronous = false)
        {
            if (Event != null)
            {
                this.invokeDelegates(Event.GetInvocationList(), synchronous, arg);
            }
        }
    }

    public class ParallelAggregateEventAction<T1, T2> : ParallelAggregateEvent<Action<T1, T2>>
    {
        public event Action<T1, T2> Event;
        public override bool IsEventNull() => this.Event == null;

        public void Invoke(T1 arg1, T2 arg2, bool synchronous = false)
        {
            if (Event != null)
            {
                this.invokeDelegates(Event.GetInvocationList(), synchronous, arg1, arg2);
            }
        }
    }

    public class ParallelAggregateEventAction<T1, T2, T3> : ParallelAggregateEvent<Action<T1, T2, T3>>
    {
        public event Action<T1, T2, T3> Event;
        public override bool IsEventNull() => this.Event == null;

        public void Invoke(T1 arg1, T2 arg2, T3 arg3, bool synchronous = false)
        {
            if (Event != null)
            {
                this.invokeDelegates(Event.GetInvocationList(), synchronous, arg1, arg2, arg3);
            }
        }
    }

    public class ParallelAggregateEventAction<T1, T2, T3, T4> : ParallelAggregateEvent<Action<T1, T2, T3, T4>>
    {
        public event Action<T1, T2, T3, T4> Event;
        public override bool IsEventNull() => this.Event == null;

        public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, bool synchronous = false)
        {
            if (Event != null)
            {
                this.invokeDelegates(Event.GetInvocationList(), synchronous, arg1, arg2, arg3, arg4);
            }
        }
    }
}
