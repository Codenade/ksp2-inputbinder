using System;
using System.Collections.Generic;
using System.Text;

namespace Codenade.Inputbinder.ui
{
    internal interface IMenuPanel
    {
        /// <summary>
        /// Attempt to navigate back and close the panel
        /// </summary>
        /// <returns>True if the panel was closed false if the panel is still open</returns>
        abstract bool Back();
    }
}
