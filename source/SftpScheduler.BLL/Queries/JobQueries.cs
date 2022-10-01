﻿using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Queries
{
    public class JobQueries
    {
        public virtual IEnumerable<JobEntity> GetAll(IDbContext dbContext)
        {
            string sql = @"SELECT * FROM Job";
            return dbContext.Query<JobEntity>(sql).ToArray();
        }

        public virtual JobEntity GetById(IDbContext dbContext, int id)
        {
            string sql = @"SELECT * FROM Job WHERE Id = @Id";
            return dbContext.Query<JobEntity>(sql, new { Id = id }).Single();
        }


    }
}
