using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasurer_App.Classes.Exceptions;
using Treasurer_App.Classes.StaticClasses;

namespace Treasurer_App.Classes
{

    [Serializable]
    internal class Transaction {
        public string Name { get; }

        public string Description { get; }

        // This is the budget who's balace is modified according to this Transaction. The associated budget should
        // only be changed when removing it's associated budget
        public Budget AssociatedBudget { get; set; }

        public DateTime DateCreated { get; }

        public bool IsSettled { get; private set; }

        public int Amount { get; }

        public int Id { get; }

        // New Transaction Constructor: Used when creating a new Transaction (not stored in persistent)
        public Transaction(int amount, string name, string description, Budget associated_budget, int id) {
            this.Amount = amount;
            this.Name = name;
            this.Id = id;
            this.Description = description;
            this.AssociatedBudget = associated_budget;
            this.DateCreated = DateTime.Now;
            IsSettled = false;
            Logger.Log(this, Logger.LogLevel.Info, $"Created Successfully");
        }

        public override string ToString() {
            return $"Transaction[{this.Name}](amt:{Amount})(settled:{IsSettled.ToString()})(bgt:{AssociatedBudget.Name})";
        }

        public void Settle () { 
            if (IsSettled) { throw new SettleException("Transaction already settled");  }
            IsSettled = true;
            Logger.Log(this, Logger.LogLevel.Info, "Transaction Settled");
        }
    }
}
