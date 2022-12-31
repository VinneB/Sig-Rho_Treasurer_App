using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treasurer_App.Classes.Exceptions {
    internal class InitException : Exception {
        public InitException() {

        }

        public InitException(string message) : base(message) {

        }

        public InitException(string message, Exception inner)
            : base(message, inner) {
        }
    }
}