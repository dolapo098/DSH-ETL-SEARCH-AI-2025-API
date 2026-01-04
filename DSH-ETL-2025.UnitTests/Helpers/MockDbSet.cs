using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Collections;
using System.Linq.Expressions;

namespace DSH_ETL_2025.UnitTests.Helpers;

internal class AsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal AsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new AsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new AsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression)!;
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
    {
        return new AsyncEnumerable<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        Type resultType = typeof(TResult).GetGenericArguments()[0];
        System.Reflection.MethodInfo? executeMethod = typeof(IQueryProvider)
            .GetMethod(
                nameof(IQueryProvider.Execute),
                1,
                new[] { typeof(Expression) })
            ?.MakeGenericMethod(resultType);

        object? result = executeMethod?.Invoke(_inner, new object[] { expression });

        System.Reflection.MethodInfo? fromResultMethod = typeof(Task).GetMethod(nameof(Task.FromResult))?.MakeGenericMethod(resultType);
        object? task = fromResultMethod?.Invoke(null, new[] { result });

        return (TResult)task!;
    }
}

internal class AsyncEnumerable<T> : IAsyncEnumerable<T>, IQueryable<T>
{
    private readonly IQueryable<T> _queryable;

    public AsyncEnumerable(Expression expression)
    {
        _queryable = new EnumerableQuery<T>(expression);
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new AsyncEnumerator<T>(_queryable.GetEnumerator());
    }

    public IEnumerator<T> GetEnumerator() => _queryable.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public Type ElementType => _queryable.ElementType;

    public Expression Expression => _queryable.Expression;

    public IQueryProvider Provider => new AsyncQueryProvider<T>(_queryable.Provider);
}

internal class AsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public AsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();

        return ValueTask.CompletedTask;
    }
}

public class MockDbSet<TEntity> : Mock<DbSet<TEntity>> where TEntity : class
{
    private readonly List<TEntity> _data;
    private readonly IQueryable<TEntity> _queryable;

    public MockDbSet(List<TEntity>? dataSource = null)
    {
        _data = dataSource ?? new List<TEntity>();
        _queryable = _data.AsQueryable();

        AsyncQueryProvider<TEntity> asyncQueryProvider = new AsyncQueryProvider<TEntity>(_queryable.Provider);
        AsyncEnumerable<TEntity> asyncQueryable = new AsyncEnumerable<TEntity>(_queryable.Expression);

        this.As<IQueryable<TEntity>>().Setup(e => e.Provider).Returns(asyncQueryProvider);
        this.As<IQueryable<TEntity>>().Setup(e => e.Expression).Returns(_queryable.Expression);
        this.As<IQueryable<TEntity>>().Setup(e => e.ElementType).Returns(_queryable.ElementType);
        this.As<IQueryable<TEntity>>().Setup(e => e.GetEnumerator()).Returns(() => _queryable.GetEnumerator());
        this.As<IEnumerable>().Setup(e => e.GetEnumerator()).Returns(() => _queryable.GetEnumerator());
        this.As<IAsyncEnumerable<TEntity>>().Setup(e => e.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => asyncQueryable.GetAsyncEnumerator());

        this.Setup(_ => _.Add(It.IsAny<TEntity>())).Callback<TEntity>(entity =>
        {
            _data.Add(entity);
        });

        this.Setup(_ => _.AddAsync(It.IsAny<TEntity>(), It.IsAny<CancellationToken>()))
            .Callback<TEntity, CancellationToken>((entity, token) =>
            {
                _data.Add(entity);
            });

        this.Setup(_ => _.Remove(It.IsAny<TEntity>())).Callback<TEntity>(entity =>
        {
            _data.Remove(entity);
        });

        this.Setup(_ => _.Attach(It.IsAny<TEntity>())).Callback<TEntity>(entity =>
        {
            if (!_data.Contains(entity))
            {
                _data.Add(entity);
            }
        });

        this.Setup(_ => _.Update(It.IsAny<TEntity>())).Callback<TEntity>(entity =>
        {
            int index = _data.FindIndex(e => GetKeyValue(e)?.Equals(GetKeyValue(entity)) == true);

            if (index >= 0)
            {
                _data[index] = entity;
            }
            else
            {
                _data.Add(entity);
            }
        });

        this.Setup(_ => _.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync((object[] keyValues) =>
            {
                System.Reflection.PropertyInfo? idProperty = typeof(TEntity).GetProperty($"{typeof(TEntity).Name}ID");

                if (idProperty != null && keyValues.Length > 0)
                {
                    return _data.FirstOrDefault(e => idProperty.GetValue(e)?.Equals(keyValues[0]) == true);
                }

                return null;
            });

        this.Setup(_ => _.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] keyValues, CancellationToken token) =>
            {
                System.Reflection.PropertyInfo? idProperty = typeof(TEntity).GetProperty($"{typeof(TEntity).Name}ID");

                if (idProperty != null && keyValues.Length > 0)
                {
                    return _data.FirstOrDefault(e => idProperty.GetValue(e)?.Equals(keyValues[0]) == true);
                }

                return null;
            });
    }

    public List<TEntity> Data => _data;

    private object? GetKeyValue(TEntity entity)
    {
        System.Reflection.PropertyInfo? idProperty = typeof(TEntity).GetProperty($"{typeof(TEntity).Name}ID");

        return idProperty?.GetValue(entity);
    }
}

