using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FaunaDB.Errors;
using FaunaDB.Types;

namespace FaunaDB.Client
{
    public class StreamingEventHandler : IObservable<Value>, IDisposable
    {
        private readonly List<IObserver<Value>> observers;
        private StreamReader streamReader;
        
        private static Field<string> CODE = Field.At("event", "code").To<string>();
        private static Field<string> DESCRIPTION = Field.At("event", "description").To<string>();
        private static Field<string> TYPE = Field.At("type").To<string>();
        
        public StreamingEventHandler(Stream dataSource)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            observers = new List<IObserver<Value>>();
            streamReader = new StreamReader(dataSource);
        }
        
        public IDisposable Subscribe(IObserver<Value> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }

            return new Unsubscriber<Value>(observers, observer);
        }
        
        public async void RequestData()
        {
            Action<IObserver<Value>> ev;
            try
            {
                var data = await streamReader.ReadLineAsync();
                var value = FaunaClient.FromJson(data);
                ev = observer => observer.OnNext(value);
                if (value.Get(TYPE) == "error")
                {
                    FaunaException ex = ConstructStreamingException(value);
                    ev = observer => observer.OnError(ex);
                }
            }
            catch (Exception ex)
            {
                FaunaException fex = ConstructStreamingException(ex);
                ev = observer => observer.OnError(fex);
            }

            foreach (var observer in observers.ToList())
            {
                ev(observer);
            }
        }

        public void Complete()
        {
            foreach (var observer in observers.ToList())
            {
                observer.OnCompleted();
            }
        }

        public void Dispose()
        {
            streamReader.Dispose();
        }

        private FaunaException ConstructStreamingException(Exception ex)
        {
            var queryError = new QueryError(null, "internal exception", ex.Message, null);
            var response = new QueryErrorResponse(500, new List<QueryError> {queryError});
            return new StreamingException(response);
        }

        private FaunaException ConstructStreamingException(ObjectV value)
        {
            var code = value.Get(CODE);
            var description = value.Get(DESCRIPTION);
            var queryError = new QueryError(null, code, description, null);
            var response = new QueryErrorResponse(500, new List<QueryError> {queryError});
            return new StreamingException(response);
        }
    }

    internal class Unsubscriber<Value> : IDisposable
    {
        private readonly List<IObserver<Value>> observers;
        private readonly IObserver<Value> observer;

        internal Unsubscriber(List<IObserver<Value>> observers, IObserver<Value> observer)
        {
            this.observers = observers;
            this.observer = observer;
        }
        
        public void Dispose()
        {
            if (observers.Contains(observer))
            {
                observers.Remove(observer);
            }
        }
    }
}
