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
    internal class Budget {
        public string Name;
        public string Description;
        public int TotalBudget; // Starting budget for a budget_period (generally a semester)
        public string ActiveOfficer; // Officer currently in position associated with budget

        // This is a sub-budget of the parent (generally master). If null, then is master
        public Budget? ParentBudget; 

        public int LedgerBalance { get; private set; } // Amount of money allocated for officer. When all transactions associated with this budget are settled, this will equal account balance
        public int AccountBalance { get; private set; } // Current money actually in account


        public bool SettleTransaction (Transaction trans) {
            if (trans.AssociatedBudget != this) {
                throw new SettleException($"Transaction couldn't be settled because {trans.ToString()} is not associated" +
                    $"with {this.ToString()}");
            }
            if (AccountBalance < trans.Amount) {
                return false;
            }
            trans.Settle();

            // Settle all ancestor budgets
            Budget nextBudget = this;
            while (nextBudget != null) {
                nextBudget.AccountBalance += trans.Amount;
                nextBudget = nextBudget.ParentBudget;
            }
            return true;

        }

        public void UseTransaction (Transaction trans) {
            LedgerBalance += trans.Amount;
            if (ParentBudget != null) {
                ParentBudget.UseTransaction(trans);
            }
        }

        public void UndoTransaction (Transaction trans) {
            if (trans.IsSettled == true) {
                throw new TransactionException("Transaction can't be undone because it's" +
                "already been settled"); 
            }
            LedgerBalance -= trans.Amount;
            if (ParentBudget != null) {
                ParentBudget.UndoTransaction(trans);
            }
            Logger.Log(this, Logger.LogLevel.Info, "Transaction undone.");
        }

        public override string ToString() {
            return $"Budget[{this.Name}](acc_bal:{AccountBalance})(leg_bal:{LedgerBalance})";
        }

        // New Budget Constructor: Used when creating a new budget (not stored in persistent)
        public Budget (int budget_amount, string budget_name, string budget_description, string active_officer, Budget? parentBudget)
        {
            this.TotalBudget = budget_amount;
            this.Name = budget_name;
            this.Description = budget_description;
            this.ActiveOfficer = active_officer;
            this.ParentBudget = parentBudget;
            this.AccountBalance = budget_amount;
            this.LedgerBalance = budget_amount;
            Logger.Log(this, Logger.LogLevel.Info, $"Created Sucessfully");
        }
    }
}
