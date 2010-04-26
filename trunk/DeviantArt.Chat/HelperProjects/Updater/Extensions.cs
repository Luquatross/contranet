using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Updater
{
    /// <summary>
    /// Class to hold our extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Call invoke using a method invoker.
        /// </summary>
        public static void Invoke(this Form form, MethodInvoker method)
        {            
            form.Invoke(method);
        }

        /// <summary>
        /// Call invoke using a method invoker.
        /// </summary>
        public static void Invoke(this Form form, MethodInvoker method, params object[] args)
        {
            form.Invoke(method, args);
        }
    }
}
