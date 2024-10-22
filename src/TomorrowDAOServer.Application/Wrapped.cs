using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using AElf.Indexing.Elasticsearch.Exceptions;
using Nest;
using Volo.Abp.Domain.Entities;

namespace TomorrowDAOServer;

public class Wrapped<TEntity, TKey> : INESTRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
{
    private readonly INESTRepository<TEntity, TKey> _inner;

    public static Wrapped<TEntity, TKey> New(INESTRepository<TEntity, TKey> inner)
    {
        return new Wrapped<TEntity, TKey>(inner);
    }

    public Wrapped(INESTRepository<TEntity, TKey> inner)
    {
        _inner = inner;
    }

    public Task<TEntity> GetAsync(TKey id, string index = null)
    {
        return _inner.GetAsync(id, index);
    }

    public Task<TEntity> GetAsync(Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterFunc = null,
        Func<SourceFilterDescriptor<TEntity>, ISourceFilter> includeFieldFunc = null,
        Expression<Func<TEntity, object>> sortExp = null,
        SortOrder sortType = SortOrder.Ascending, string index = null)
    {
        return _inner.GetAsync(filterFunc, includeFieldFunc, sortExp, sortType, index);
    }

    public async Task<Tuple<long, List<TEntity>>> GetListAsync(
        Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterFunc = null,
        Func<SourceFilterDescriptor<TEntity>, ISourceFilter> includeFieldFunc = null,
        Expression<Func<TEntity, object>> sortExp = null,
        SortOrder sortType = SortOrder.Ascending, int limit = 1000, int skip = 0, string index = null)
    {
        try
        {
            return await _inner.GetListAsync(filterFunc, includeFieldFunc, sortExp, sortType, limit, skip, index);
        }
        catch (ElasticSearchException ex)
        {
            if (ex.Message.Contains("no such index"))
            {
                return new Tuple<long, List<TEntity>>(0, new List<TEntity>());
            }
            else
            {
                throw;
            }
        }
    }

    public Task<Tuple<long, List<TEntity>>> GetSortListAsync(
        Func<QueryContainerDescriptor<TEntity>, QueryContainer> filterFunc = null,
        Func<SourceFilterDescriptor<TEntity>, ISourceFilter> includeFieldFunc = null,
        Func<SortDescriptor<TEntity>, IPromise<IList<ISort>>> sortFunc = null, int limit = 1000,
        int skip = 0, string index = null)
    {
        return _inner.GetSortListAsync(filterFunc, includeFieldFunc, sortFunc, limit, skip, index);
    }

    public Task<ISearchResponse<TEntity>> SearchAsync(SearchDescriptor<TEntity> query, int skip, int size,
        string index = null, string[] includeFields = null,
        string preTags = "<strong style=\"color: red;\">", string postTags = "</strong>", bool disableHigh = false,
        params string[] highField)
    {
        return _inner.SearchAsync(query, skip, size, index, includeFields, preTags, postTags, disableHigh,
            highField);
    }

    public Task<CountResponse> CountAsync(Func<QueryContainerDescriptor<TEntity>, QueryContainer> query,
        string indexPrefix = null)
    {
        return _inner.CountAsync(query, indexPrefix);
    }

    public Task<Tuple<long, List<TEntity>>> GetListByLucenceAsync(string filter,
        Func<SortDescriptor<TEntity>, IPromise<IList<ISort>>> sortFunc = null, int limit = 1000, int skip = 0,
        string index = null)
    {
        return _inner.GetListByLucenceAsync(filter, sortFunc, limit, skip, index);
    }

    public Task AddOrUpdateAsync(TEntity model, string index = null)
    {
        return _inner.AddOrUpdateAsync(model, index);
    }

    public Task AddAsync(TEntity model, string index = null)
    {
        return _inner.AddAsync(model, index);
    }

    public Task UpdateAsync(TEntity model, string index = null)
    {
        return _inner.UpdateAsync(model, index);
    }

    public Task DeleteAsync(TKey id, string index = null)
    {
        return _inner.DeleteAsync(id, index);
    }

    public Task DeleteAsync(TEntity model, string index = null)
    {
        return _inner.DeleteAsync(model, index);
    }

    public Task BulkAddOrUpdateAsync(List<TEntity> list, string index = null)
    {
        return _inner.BulkAddOrUpdateAsync(list, index);
    }

    public Task BulkDeleteAsync(List<TEntity> list, string index = null)
    {
        return _inner.BulkDeleteAsync(list, index);
    }
}