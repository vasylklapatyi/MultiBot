﻿using Persistence.Sql.BaseTypes;
using Persistence.Sql.Entites;

namespace Persistence.Sql.Repositories
{
    public class SetRepository : Repository<Set>
    {
        public SetRepository(SqlServerDbContext context) : base(context)
        {

        }

    }
}
