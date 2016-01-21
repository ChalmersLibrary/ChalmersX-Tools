﻿using ChalmersxTools.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Tools
{
    public abstract class SimpleDataStorageToolBase : ToolBase
    {
        public override ViewIdentifierAndModel HandleRequest(HttpRequestBase request)
        {
            var res = "";
            if (request.Form["action"] == "create")
            {
                res = Create(request);
            }

            if (request.Form["action"] == "edit")
            {
                res = Edit(request);
            }

            return GetViewIdentifierAndModel(res);
        }

        protected abstract string Create(HttpRequestBase request);
        protected abstract string Edit(HttpRequestBase request);
        protected abstract ViewIdentifierAndModel GetViewIdentifierAndModel(string message);
    }
}