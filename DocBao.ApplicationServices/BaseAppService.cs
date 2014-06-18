using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocBao.ApplicationServices
{
    public class BaseAppService
    {
        public AppResult<T> AppResult<T>(T target)
        {
            return new AppResult<T>(target);
        }

        public AppResult<T> AppResult<T>(ErrorCode error)
        {
            return new AppResult<T>(error);
        }
    }
}
