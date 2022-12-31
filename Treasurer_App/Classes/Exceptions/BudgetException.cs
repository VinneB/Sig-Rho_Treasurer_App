using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treasurer_App.Classes.Exceptions {
    internal class BudgetException : Exception {
        public BudgetException() { }
        public BudgetException(string message) : base(message) { }
        public BudgetException(string message, Exception inner) : base(message, inner) { }
    }
}
