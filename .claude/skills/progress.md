using Microsoft.EntityFrameworkCore;
using PolovniTelefoni_DatabeseContext.Context;
using RepoServiceDAL.Interfaces.Common;
using RepositoryDALImplementation.DALImplementation.Common.Filters;
using RepositoryModel.Models.Abstract;
using RepositoryModel.RequestModels.Filter;
using RepositoryModel.RequestModels.Paging;
using RepositoryModel.RequestModels.Sort;
using RepositoryModel.ResponseModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryDALImplementation.DALImplementation.Common
{
    public class RepoEntityObjectBaseDAL<Tdb> : IRepoBaseDAL<long, Tdb> where Tdb : ARepoBaseEntity
    {
        public PADbContext db { get; set; }

        public RepoEntityObjectBaseDAL(PADbContext inDb)
        {
            if (inDb == null) throw new ArgumentNullException("inDb", "In database is null");
            db = inDb;
        }
        #region Create
        public RepositoryResponseBase<Tdb> Create(Tdb inObject)
        {
            RepositoryResponseBase<Tdb> toRet = new RepositoryResponseBase<Tdb>();

            try
            {
                db.Set<Tdb>().Add(inObject);
                var save = db.SaveChanges() > 0;
                if (save)
                {
                    toRet.Success = true;
                    toRet.Value = inObject;
                }
                else
                {
                    toRet.Success = false;
                    toRet.Message = $"Error creating new entity in database. SaveChanges faild. {typeof(Tdb)}";
                    toRet.Value = inObject;
                }

            }
            catch (Exception ex)
            {
                toRet.Success = false;
                toRet.Message = ex.Message;
                toRet.MessageDescription = "Exception error: " + ex;
                toRet.Value = inObject;
                toRet.TranslateMessageId = "COMMON.ERROR_EX.CREATE";
            }


            return toRet;
        }
        #endregion

        #region Delete
        public RepositoryResponseBase<Tdb> Delete(Tdb inObject)
        {
            return Delete(inObject?.Id ?? 0);
        }

        public RepositoryResponseBase<Tdb> Delete(long inId)
        {
            RepositoryResponseBase<Tdb> toRet = new RepositoryResponseBase<Tdb>();

            try
            {
                var dbObj = db.Set<Tdb>().FirstOrDefault(o => o.Id == inId);
                if (dbObj != null)
                {
                    db.Set<Tdb>().Remove(dbObj);
                    var save = db.SaveChanges() > 0;
                    if (save)
                    {
                        toRet.Success = true;
                        toRet.Value = dbObj;
                    }
                    else
                    {
                        toRet.Success = false;
                        toRet.Message = $"Error deleting entity in database. SaveChanges faild. {typeof(Tdb)}";
                        toRet.Value = dbObj;
                    }

                }
                else
                {
                    toRet.Success = false;
                    toRet.Message = $"Object for delete not found in database. {typeof(Tdb)}";
                    toRet.Value = dbObj;

                }
            }
            catch (Exception ex)
            {

                toRet.Success = false;
                toRet.Message = ex.Message;
                toRet.MessageDescription = "Exception error: " + ex;
                toRet.Value = null; ;
                toRet.TranslateMessageId = "COMMON.ERROR_EX.DELETE";
            }

            return toRet;
        }
        #endregion

        #region Get
        public RepositoryResponseBase<Tdb> Get(long inId)
        {
            RepositoryResponseBase<Tdb> toRet = new RepositoryResponseBase<Tdb>();

            try
            {
                var dbObj = db.Set<Tdb>().FirstOrDefault(o => o.Id == inId);
                if (dbObj != null)
                {

                    //Add custom include f

                    toRet.Success = true;
                    toRet.Value = dbObj;
                }
                else
                {
                    toRet.Success = false;
                    toRet.Message = $"Object , not found in database.  {typeof(Tdb)}";
                    toRet.Value = null;
                }
            }
            catch (Exception ex)
            {
                toRet.Success = false;
                toRet.Message = ex.Message;
                toRet.MessageDescription = "Exception error: " + ex;
                toRet.Value = null; ;
                toRet.TranslateMessageId = "COMMON.ERROR_EX.GET";
            }

            return toRet;
        }

        public RepositoryResponseBase<IEnumerable<Tdb>> GetListEager(RepositoryPaging inRepoPaging, params Expression<Func<Tdb, object>>[] properties)
        {
            RepositoryResponseBase<IEnumerable<Tdb>> toRet = new RepositoryResponseBase<IEnumerable<Tdb>>();
            List<Tdb> currentPageList = null;


            try
            {
                if (inRepoPaging == null) inRepoPaging = new RepositoryPaging();

                #region Apply default filter
                if (inRepoPaging.RepositoryFilters == null || inRepoPaging.RepositoryFilters.Count == 0 || !inRepoPaging.RepositoryFilters.Any(o => o.ApplySort))
                {
                    if (inRepoPaging.RepositoryFilters == null)
                    {
                        inRepoPaging.RepositoryFilters = new List<RepositoryFilter>();
                    }
                    inRepoPaging.RepositoryFilters.Add(
                        new RepositoryFilter()
                        {

                            PropName = "Id",
                            ApplySort = true,
                            SortType = ESortTypes.Descending
                        }
                    );
                }
                if (!inRepoPaging.RepositoryFilters.Any(o => o.PropName == "SoftDeleted"))
                {
                    inRepoPaging.RepositoryFilters.Add(new RepositoryFilter()
                    {
                        PropName = "SoftDeleted",
                        ApplyFilter = true,
                        FilterType = EFilterTypes.Equal,
                        FilterValue = "false"
                    });
                }
                #endregion

                //var x = db.Set<Tdb>().AsQueryable();

                //RepoCommonCustomIncludeHelper.TestMethod(x, typeof(Tdb));
                IQueryable<Tdb> entityQueryable = db.Set<Tdb>().AsQueryable();
                //IQueryable<Tdb> query = db.Set<Tdb>().AsQueryable();

                if (properties != null && properties.Length > 0)
                {
                    entityQueryable = properties
                        .Aggregate(entityQueryable, (current, prop) => current.Include(prop));

                }



                //IQueryable<RepoAuthor> t = db.Set<RepoAuthor>().Include(o => o.Books).AsQueryable();

                foreach (var itemF in inRepoPaging.RepositoryFilters)
                {
                    if (itemF != null)
                    {

                        entityQueryable = GenericPropsFilters.ApplyGlobalGeneralPropertyFilter(ref entityQueryable, itemF);

                    }
                }

                inRepoPaging.totalCount = entityQueryable.Count();
                if (inRepoPaging.countPerPage == 0)
                {
                    inRepoPaging.countPerPage = 100;
                }
                inRepoPaging.totalPages = (int)Math.Ceiling((decimal)inRepoPaging.totalCount / inRepoPaging.countPerPage);

                int tmpZeroBased = (inRepoPaging.page - 1);
                if (tmpZeroBased < 0)
                    tmpZeroBased = 0;

                currentPageList = entityQueryable.Skip(tmpZeroBased * inRepoPaging.countPerPage).Take(inRepoPaging.countPerPage).ToList();
                if (currentPageList == null)
                {
                    throw new Exception("Error pagging object in database.");
                }

                toRet.Success = true;
                toRet.PaggingObject = inRepoPaging;
                toRet.Value = currentPageList;

                //List<ARepoBaseEntity> tmpList = currentPageList.Cast<ARepoBaseEntity>().ToList();

            }
            catch (Exception ex)
            {
                toRet.Success = false;
                toRet.Message = ex.Message;
                toRet.MessageDescription = "Exception error: " + ex;
                toRet.Value = null; ;
                toRet.TranslateMessageId = "COMMON.ERROR_EX.GET";
            }

            return toRet;
        }
        public RepositoryResponseBase<IEnumerable<Tdb>> GetList(RepositoryPaging inRepoPaging)
        {
            RepositoryResponseBase<IEnumerable<Tdb>> toRet = new RepositoryResponseBase<IEnumerable<Tdb>>();
            List<Tdb> currentPageList = null;


            try
            {
                if (inRepoPaging == null) inRepoPaging = new RepositoryPaging();

                #region Apply default filter
                if (inRepoPaging.RepositoryFilters == null || inRepoPaging.RepositoryFilters.Count == 0 || !inRepoPaging.RepositoryFilters.Any(o => o.ApplySort))
                {
                    if (inRepoPaging.RepositoryFilters == null)
                    {
                        inRepoPaging.RepositoryFilters = new List<RepositoryFilter>();
                    }
                    inRepoPaging.RepositoryFilters.Add(
                        new RepositoryFilter()
                        {

                            PropName = "Id",
                            ApplySort = true,
                            SortType = ESortTypes.Descending
                        }
                    );
                }
                if (!inRepoPaging.RepositoryFilters.Any(o => o.PropName == "SoftDeleted"))
                {
                    inRepoPaging.RepositoryFilters.Add(new RepositoryFilter()
                    {
                        PropName = "SoftDeleted",
                        ApplyFilter = true,
                        FilterType = EFilterTypes.Equal,
                        FilterValue = "false"
                    });
                }
                #endregion

                //var x = db.Set<Tdb>().AsQueryable();

                //RepoCommonCustomIncludeHelper.TestMethod(x, typeof(Tdb));
                IQueryable<Tdb> entityQueryable = db.Set<Tdb>().AsQueryable();

                var x = entityQueryable.GetType();

                var this1 = this.GetType();
                var tp = typeof(Tdb);



                

                foreach (var itemF in inRepoPaging.RepositoryFilters)
                {
                    if (itemF != null)
                    {

                        entityQueryable = GenericPropsFilters.ApplyGlobalGeneralPropertyFilter(ref entityQueryable, itemF);

                    }
                }

                inRepoPaging.totalCount = entityQueryable.Count();
                if (inRepoPaging.countPerPage == 0)
                {
                    inRepoPaging.countPerPage = 100;
                }
                inRepoPaging.totalPages = (int)Math.Ceiling((decimal)inRepoPaging.totalCount / inRepoPaging.countPerPage);

                int tmpZeroBased = (inRepoPaging.page - 1);
                if (tmpZeroBased < 0)
                    tmpZeroBased = 0;

                currentPageList = entityQueryable.Skip(tmpZeroBased * inRepoPaging.countPerPage).Take(inRepoPaging.countPerPage).ToList();
                if (currentPageList == null)
                {
                    throw new Exception("Error pagging object in database.");
                }

                toRet.Success = true;
                toRet.PaggingObject = inRepoPaging;
                toRet.Value = currentPageList;

                List<ARepoBaseEntity> tmpList = currentPageList.Cast<ARepoBaseEntity>().ToList();

            }
            catch (Exception ex)
            {
                toRet.Success = false;
                toRet.Message = ex.Message;
                toRet.MessageDescription = "Exception error: " + ex;
                toRet.Value = null; ;
                toRet.TranslateMessageId = "COMMON.ERROR_EX.GET";
            }

            return toRet;
        }
        #endregion

        #region Update
        public RepositoryResponseBase<Tdb> Update(Tdb inObject)
        {
            RepositoryResponseBase<Tdb> toRet = new RepositoryResponseBase<Tdb>();

            try
            {
                var dbObj = db.Set<Tdb>().FirstOrDefault(o => o.Id == inObject.Id);
                if (dbObj != null)
                {

                    //Add custom include f
                    db.Entry(dbObj).CurrentValues.SetValues(inObject);
                    var save = db.SaveChanges() > 0;
                    if (save)
                    {
                        toRet.Success = true;
                        toRet.Value = dbObj;
                    }
                    else
                    {
                        toRet.Success = false;
                        toRet.Message = $"Error Update entity in database. SaveChanges faild. {typeof(Tdb)}";
                        toRet.Value = dbObj;
                        toRet.Success = true;
                        toRet.Value = dbObj;
                    }
                }
                else
                {
                    toRet.Success = false;
                    toRet.Message = $"Object , not found in database.  {typeof(Tdb)}";
                    toRet.Value = null;
                }
            }
            catch (Exception ex)
            {
                toRet.Success = false;
                toRet.Message = ex.Message;
                toRet.MessageDescription = "Exception error: " + ex;
                toRet.Value = null; ;
                toRet.TranslateMessageId = "COMMON.ERROR_EX.UPDATE";
            }

            return toRet;
        }
        #endregion

        #region SoftDelete
        public RepositoryResponseBase<Tdb> SoftDelete(Tdb inObject)
        {
            throw new NotImplementedException();
        }

        public RepositoryResponseBase<Tdb> SoftDelete(long inId)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ObjectExist
        public bool ObjectExist(long inId)
        {
            throw new NotImplementedException();
        }



        #endregion
    }
}