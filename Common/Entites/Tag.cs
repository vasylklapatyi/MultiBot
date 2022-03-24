﻿using Common.BaseTypes;
using System;
using System.Collections.Generic;

namespace Common.Entites
{
    public class Tag : AuditableEntity
    {
        public Guid? UserId { get; set; }
        public string Name { get; set; }

        public List<Note> Notes { get; set; }
    }
}