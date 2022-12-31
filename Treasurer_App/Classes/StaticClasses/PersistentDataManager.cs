using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Reflection;
using Treasurer_App.Classes.Exceptions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace Treasurer_App.Classes.StaticClasses
{
    [Serializable]
    internal class PersistentDataManager {
        // Static Functionality

        // Init Fields
        private static PersistentDataManager? s_instance = null;
        private static string? s_per_path = null;

        // Accesor property to singleton instance
        public static PersistentDataManager? Instance {
            get {
                if (s_instance == null) {
                    s_instance = new PersistentDataManager();
                }
                return s_instance;
            }
            private set { }
        }

        public static void Load() {
            if (s_per_path == null) { throw new InitException("Save failed because s_per_path is null"); }
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(s_per_path, FileMode.Create, FileAccess.Write)) {
                IFormatter writer = new BinaryFormatter();
                #pragma warning disable SYSLIB0011
                s_instance = (PersistentDataManager)writer.Deserialize(stream);
            }
            Logger.Log(PersistentDataManager.StaticToString, Logger.LogLevel.Info, $"Persistent Data loaded from {s_per_path}");
        }

        public static void Save() {
            if (s_per_path == null) { throw new InitException("Save faield because s_per_path is null"); }
            if (s_instance == null) { throw new InitException("Save failed because s_instance is null"); }
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(s_per_path, FileMode.Create, FileAccess.Write)) {
                IFormatter writer = new BinaryFormatter();
                #pragma warning disable SYSLIB0011
                writer.Serialize(stream, s_instance);
            }
            Logger.Log(PersistentDataManager.StaticToString, Logger.LogLevel.Info, $"Persistent Data saved to {s_per_path}");
        }
        
        public static string StaticToString => "Persistent Data Manager";

        public static void Init() {

            // Set path either relative or absolute
            #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            string persistent_dir = bool.Parse(ConfigurationManager.AppSettings["relative_persistent_path"]) ?
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\"
                    + ConfigurationManager.AppSettings["persistent_path"] : ConfigurationManager.AppSettings["persistent path"];
            #pragma warning restore CS8600

            // Create Persistent directory if needed
            if (!Directory.Exists(persistent_dir)) {  Directory.CreateDirectory(persistent_dir); }

            // Settings
            s_per_path = persistent_dir + @"\" + ConfigurationManager.AppSettings["persistent_file_name"];

            if (File.Exists(s_per_path)) {
                // Load Persistent Data
                Load();
            } else {
                s_instance= new PersistentDataManager();
                Logger.Log(PersistentDataManager.StaticToString, Logger.LogLevel.Info, $"No saved persistent data found" +
                    $". Instance created");
            }
            Logger.Log(PersistentDataManager.StaticToString, Logger.LogLevel.Info, $"Initialized successfully");
        }

        protected PersistentDataManager() {
            _budgets = new Dictionary<string, Budget>();
            _transactions = new Dictionary<int, Transaction>();

            _nextTransactionId = 0;
            TransactionCount = 0;
            BudgetCount = 0;
    }

        // Instance Functionality (serialized)
        private Dictionary<string, Budget> _budgets;
        private Dictionary<int, Transaction> _transactions;

        private int _nextTransactionId;
        public int TransactionCount { get; private set; }
        public int BudgetCount { get; private set; }

        // Getter
        public Dictionary<string, Budget>.ValueCollection Budgets => _budgets.Values;
        public Budget Budget (string key) { return _budgets[key]; }
        public Dictionary<int, Transaction>.ValueCollection Transactions => _transactions.Values;
        public Transaction Transaction (int key) { return _transactions[key]; }

        public Budget MasterBudget { get; private set; }

        // Add and Remove
        public int CreateTransaction (int amount, string name, string description, Budget associated_budget) {
            Transaction b_tans = new Transaction(amount, name, description, associated_budget, _nextTransactionId);
            _transactions.Add(_nextTransactionId, b_tans);
            TransactionCount++;
            associated_budget.UseTransaction(b_tans);
            Logger.Log(this, Logger.LogLevel.Info, $"{b_tans.ToString()} added to persistent");
            return _nextTransactionId++;
        }

        public bool RemoveTransaction (int key) {
            Transaction trans = Transaction(key);
            trans.AssociatedBudget.UndoTransaction(trans);
            bool ret = _transactions.Remove(key);
            TransactionCount--;
            Logger.Log(this, Logger.LogLevel.Info, $"{trans.ToString()} deleted from persistent");
            return ret;
        }

        public string CreateBudget (int budget_amount, string budget_name, string budget_description, string active_officer, Budget parentBudget) {
            Budget budget = new Budget(budget_amount, budget_name, budget_description, active_officer, parentBudget);
            if (DoesBudgetNameExist(budget.Name)) { throw new BudgetException("A budget with that name already exists"); }
            if (parentBudget == null && MasterBudget != null) { throw new BudgetException($"{MasterBudget} is already master");  }
            if (parentBudget == null) {
                MasterBudget = budget;
            }
            _budgets.Add(budget.Name, budget);
            BudgetCount++;
            Logger.Log(this, Logger.LogLevel.Info, $"{budget.ToString()} added to persistent");
            return budget.Name;
        }

        public bool RemoveBudget (string key) {
            Budget r_budget = Budget(key);
            // Can't remove master budget. Throw error
            if (r_budget.ParentBudget == null) { throw new BudgetException("Cannot remove master budget"); }
            // Associate all transactions from budget to parent budget
            // Remove all child budgets
            foreach (Budget b in Budgets) {
                if (b.ParentBudget == Budget(key)) {
                    RemoveBudget(b.Name);
                }
            }
            foreach (Transaction t in Transactions) {
                if (t.AssociatedBudget == r_budget) {
                    t.AssociatedBudget = r_budget.ParentBudget;
                    Logger.Log(this, Logger.LogLevel.Info, $"{t} is now associated with {r_budget.ParentBudget}");
                }
            }
            bool ret = _budgets.Remove(key);
            BudgetCount--;
            Logger.Log(this, Logger.LogLevel.Info, $"{_budgets[key].ToString()} deleted from persistent");
            return ret;
        }

        public override string ToString() => "Persistent Data Manager";

        public bool DoesBudgetNameExist(string name) {
            foreach (Budget b in Budgets) {
                if (name == b.Name) {
                    return true;
                }
            }
                return false;
        }

        public bool DoesTransactionNameExist(string name) {
            foreach (Transaction b in Transactions) {
                if (name == b.Name) {
                    return true;
                }
            }
            return false;
        }
    }
}
