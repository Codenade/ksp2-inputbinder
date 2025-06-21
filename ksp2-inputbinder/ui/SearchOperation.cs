using System;
using System.Collections.Generic;
using System.Text;

namespace Codenade.Inputbinder.ui
{
    internal class SearchOperation
    {
        internal event Action Done;
        internal string Query { get; set; }

        internal SearchOperation(string query)
        {
            Query = query;
        }

        ~SearchOperation()
        {
            Done.Invoke();
        }
    }
}
