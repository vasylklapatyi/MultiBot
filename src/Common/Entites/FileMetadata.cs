﻿using Common.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entites
{
    public class FileMetadata : AuditableEntity
    {
        public string Name { get; set; }
    }
}
