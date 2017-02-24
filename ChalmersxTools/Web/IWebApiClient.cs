using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChalmersxTools.Web
{
    public interface IWebApiClient
    {
        dynamic GetJson(string url);
    }
}
