﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Data
{
    public interface IDbContext : IDisposable
    {

        /// <summary>
        /// Gets the transaction created with BeginTransaction.
        /// </summary>
        IDbTransaction? Transaction { get; }

        /// <summary>
        /// Begins a new transaction (if supported by the DbContext)
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Commits the current transaction (if supported by the DbContext)
        /// </summary>
        void Commit();

        /// <summary>
        /// Executes a query against the database
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        void ExecuteNonQuery(string sql, params IDbDataParameter[] dbParameters);

        /// <summary>
        /// Executes a query against the database using Dapper to substitute model values.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        void ExecuteNonQuery(string sql, object? param = null);

        /// <summary>
        /// Executes a query against the database
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        T ExecuteScalar<T>(string sql, object? param = null);

        /// <summary>
        /// Executes a query and maps the result to a strongly typed list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(string sql, object? param = null);

        /// <summary>
        /// Rolls back the current transaction (if supported by the DbContext)
        /// </summary>
        void Rollback();

    }

    public abstract class DbContext : IDbContext
    {

        /// <summary>
        /// Gets the transaction created with BeginTransaction.
        /// </summary>
        public IDbTransaction? Transaction { get; protected set; }

        /// <summary>
        /// Begins a new transaction (if supported by the DbContext)
        /// </summary>
        public abstract void BeginTransaction();

        /// <summary>
        /// Commits the current transaction (if supported by the DbContext)
        /// </summary>
        public abstract void Commit();

        public virtual void Dispose()
        {

        }

        /// <summary>
        /// Executes a query against the database
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public abstract void ExecuteNonQuery(string sql, params IDbDataParameter[] dbParameters);

        /// <summary>
        /// Executes a query against the database using Dapper to substitute model values.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        public abstract void ExecuteNonQuery(string sql, object? param = null);

        /// <summary>
        /// Executes a query against the database
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public abstract T ExecuteScalar<T>(string sql, object? param = null);

        /// <summary>
        /// Executes a query and maps the result to a strongly typed list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public abstract IEnumerable<T> Query<T>(string sql, object? param = null);

        /// <summary>
        /// Rolls back the current transaction (if supported by the DbContext)
        /// </summary>
        public abstract void Rollback();

    }

}
