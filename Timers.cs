using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnumerableTimer
{
    public class TimerQueryable<T> : IQueryable<T>
    {
        readonly IQueryable<T> _queryable;
        readonly IQueryProvider _provider;
        readonly Stopwatch _watch;

        public TimerQueryable(IQueryable<T> queryable, Stopwatch watch)
        {
            _watch = watch;
            _queryable = queryable;
            _provider = queryable.Provider;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new TimerEnumerator<T>(this._provider.CreateQuery<T>(Expression).GetEnumerator(), _watch);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public Type ElementType
        {
            get { return _queryable.ElementType; }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get { return _queryable.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return _queryable.Provider; }
        }
    }

    public class TimerEnumerator<T> : IEnumerator<T>
    {
        IEnumerator<T> _enumerator;
        Stopwatch _watch;

        public TimerEnumerator(IEnumerator<T> enumerator, Stopwatch watch)
        {
            _enumerator = enumerator;
            _watch = watch;
        }

        public T Current
        {
            get { return _enumerator.Current; }
        }

        public void Dispose()
        {
            _watch.Stop();
            _enumerator.Dispose();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return this.Current; }
        }

        public bool MoveNext()
        {
            if (!_watch.IsRunning)
            {
                _watch.Reset();
                _watch.Start();
            }
            return this._enumerator.MoveNext();
        }

        public void Reset()
        {
            this._enumerator.Reset();
        }
    }

    public static class Extensions
    {
        public static IQueryable<T> WithTimer<T>(this IQueryable<T> source, Stopwatch watch)
        {
#if TRACE
            return new TimerQueryable<T>(source, watch);
#else
            return source;
#endif
        }

        public static IEnumerable<T> WithTimer<T>(this IEnumerable<T> source, Stopwatch watch)
        {
#if TRACE
            watch.Reset();
            watch.Start();
            foreach (var item in source)
                yield return item;
            watch.Stop();
#else
            return source;
#endif
        }
    }
}
