using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunit.DependencyInjection.Test
{
    public interface IDependency
    {
        int Value { get; set; }
    }

    internal class DependencyClass : IDependency
    {
        public int Value { get; set; }
    }
}
