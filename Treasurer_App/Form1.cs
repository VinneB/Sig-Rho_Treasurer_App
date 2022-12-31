using Treasurer_App.Classes;
using Treasurer_App.Classes.StaticClasses;

namespace Treasurer_App
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e) {
            PersistentDataManager pdm = PersistentDataManager.Instance;
            string master_token = pdm.CreateBudget(100, "master_test", "This is a description", "Me", null);
            _ = pdm.CreateBudget(50, "sub_test", "sub budget", "Me", pdm.MasterBudget);
            foreach (Budget b in pdm.Budgets) {
                if (b.Name.Equals("sub_test")) {
                    pdm.CreateTransaction(-20, "trans 1", "trans 1 descrp", b);
                    pdm.CreateTransaction(10, "deposit", "I deposit mny thx", b);
                }
            }
            System.Diagnostics.Debug.WriteLine(pdm.Budget("sub_test"));
            System.Diagnostics.Debug.WriteLine(pdm.MasterBudget);
            foreach (Transaction t in pdm.Transactions) {
                System.Diagnostics.Debug.WriteLine(t.ToString());
            }
            foreach (Transaction t in pdm.Transactions) {
                if (t.AssociatedBudget == pdm.Budget("sub_test")) {
                    pdm.Budget("sub_test").SettleTransaction(t);
                    pdm.RemoveTransaction(t.Id);
                }
            }
            System.Diagnostics.Debug.WriteLine(pdm.Budget("sub_test"));
            System.Diagnostics.Debug.WriteLine(pdm.MasterBudget);

        }
        
    }
}